using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Nfbookmark;
using System.Web;
using Serilog;

namespace bookmark_dlp;

/// <summary>
///     Used in the import process
/// </summary>
/// <example>
///     <code>
/// var importedFolders = BookmarkImporterFactory.SmartImport(somefile);
/// var mappedFolders = FolderManager.CreateFolderStructure(importedFolders, rootdir);
/// var resolvedFolders = AutoImport.LinksFromUrls(mappedFolders);
/// AppMethods.CountWantedVideos(resolvedFolders);
/// AppMethods.CheckCurrentFilesystemState(resolvedFolders);
/// //optionally: FolderManager.DeleteEmptyFolders(mappedFolders);
/// </code>
/// </example>
public static class AutoImport
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(AutoImport));

    /// <summary>
    ///     Creates ResolvedFolder objects with links populated from the MappedFolder urls.
    ///     If not a youtube url, no link is generated.
    ///     If link parsing throws an exception, no link is generated for that URL,
    ///     and a log message is written, but no exception will be thrown from this method.
    ///     <br />
    ///     Uses internet to query video ids for playlists and channels.<br />
    ///     Requires (on input objects):
    ///     <list type="bullet">
    ///         <item> Urls </item>
    ///     </list>
    ///     Returns:
    ///     <list type="bullet">
    ///         <item> A new list of ResolvedFolder instances with the 'Links' property populated. </item>
    ///     </list>
    /// </summary>
    /// <param name="folders">The source list of mapped folders to process.</param>
    public static List<ResolvedFolder> LinksFromUrls(List<MappedFolder> folders)
    {
        ResolvedFolder Projection(MappedFolder folder)
        {
            var parsedLinks = folder.Urls
                .Select(url =>
                {
                    try
                    {
                        return LinkFromUrl(url);
                    }
                    catch (InvalidLinkException e)
                    {
                        Log.Error(e, "Link parsing failed for URL '{Url}' in folder '{FolderName}'", url, folder.Name);
                        return (YTLink?)null;
                    }
                })
                .OfType<YTLink>()
                .ToList();

            return new ResolvedFolder(folder, parsedLinks);
        }

        return folders
            .AsParallel()
            .Select(Projection)
            .ToList();
    }


    /// <summary>
    ///     Writes the links found in the bookmark folder into the filesystem folder in a $foldername.txt file. Id sequential
    ///     writing is NOT GUARANTEED.<br />
    ///     Requires:
    ///     <list type="bullet">
    ///         <item> FolderPath </item>
    ///         <item> Links </item>
    ///     </list>
    /// </summary>
    /// <param name="folders">the resolved folders containing the links</param>
    /// <param name="debugdirectory">the filesystem directory (only used for writing debug)</param>
    /// <param name="downloadPlaylists">options</param>
    /// <param name="downloadShorts">options</param>
    /// <param name="downloadChannels">options</param>
    public static void WriteLinksToTextFiles(List<ResolvedFolder> folders,
        bool downloadPlaylists = false, bool downloadChannels = false, bool downloadShorts = false,
        string debugdirectory = "")
    {
        StreamWriter? temp = null;
        if (!string.IsNullOrEmpty(debugdirectory))
        {
            //writing into temp.txt all the youtube links that are not for videos (but for channels, playlists, etc.)
            temp = new StreamWriter(Path.Combine(debugdirectory, "temp.txt"), true);
        }

        foreach (ResolvedFolder folder in folders)
        {
            StreamWriter writer = new StreamWriter(Path.Combine(folder.FolderPath, folder.Name + ".txt"), false);
            //writing all the youtube links that are not for videos (but for channels, playlists, etc.) in the given folder
            StreamWriter complexnotsimple =
                new StreamWriter(Path.Combine(folder.FolderPath, folder.Name + ".complex.txt"), true);
            foreach (YTLink link in folder.Links)
            {
                switch (link.linktype)
                {
                    case Linktype.Video:
                        writer.WriteLine(link.url);
                        break;
                    case Linktype.Short:
                        if (downloadShorts)
                            writer.WriteLine(link.url);
                        break;
                    case Linktype.Search:
                        break;
                    case Linktype.Playlist:
                        complexnotsimple.WriteLine(link.url);
                        temp?.WriteLine(link.url);
                        if (downloadPlaylists)
                            writer.WriteLine(link.url);
                        break;
                    case Linktype.Channel_c:
                    case Linktype.Channel_user:
                    case Linktype.Channel_channel:
                    case Linktype.Channel_at:
                        complexnotsimple.WriteLine(link.url);
                        temp?.WriteLine(link.url);
                        if (downloadChannels)
                            writer.WriteLine(link.url);
                        break;
                }
            }

            writer.Flush();
            writer.Close();
            complexnotsimple.Flush();
            complexnotsimple.Close();
            if (new FileInfo(Path.Combine(folder.FolderPath, folder.Name + ".complex.txt")).Length == 0)
                File.Delete(Path.Combine(folder.FolderPath, folder.Name + ".complex.txt"));
            //if the txt remained empty it is deleted
            if (new FileInfo(Path.Combine(folder.FolderPath, folder.Name + ".txt")).Length == 0)
            {
                File.Delete(Path.Combine(folder.FolderPath, folder.Name + ".txt"));
                Log.Verbose("Deleted txt of {FolderName}", folder.Name);
            }
        }
        if (temp != null)
        {
            temp.Flush();
            temp.Close();
        }
    }
    
    /// <summary>
    ///     Parses url to YTLink object and fills:<br />
    ///     <list type="bullet">
    ///         <item> url </item>
    ///         <item> linktype </item>
    ///         <item> yt_id </item>
    ///         <item> MemberIds </item>
    ///     </list>
    /// </summary>
    /// <param name="url">
    ///     Url to parse. Usually FQDN, like
    ///     https://www.youtube.com/watch?v=12345678912
    /// </param>
    /// <returns>YTLink with parameters filled or null if url is not a valid youtube link</returns>
    public static YTLink? LinkFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Log.Information("Empty URL!");
            return null;
        }
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            Log.Information("Not a valid URI: {Url}", url);
            return null;
        }
        
        if (!uri.Host.EndsWith("youtube.com"))
        {
            Log.Information("Not a youtube url: {Url}", url);
            return null;
        }

        var link = new YTLink { url = url };
        string[] segments = uri.Segments.Select(s => s.Trim('/')).Where(s => !string.IsNullOrEmpty(s)).ToArray();

        if (segments.Length > 0)
        {
            var path = segments[0];
            switch (path)
            {
                case "watch":
                    var queryParams = HttpUtility.ParseQueryString(uri.Query);
                    string? videoId = queryParams["v"];
                    if (!string.IsNullOrEmpty(videoId) && videoId.Length == 11)
                    {
                        link.linktype = Linktype.Video;
                        link.yt_id = videoId;
                    }
                    else
                    {
                        Log.Information("Invalid YouTube video URL: {Url}", url);
                        return null;
                    }
                    break;

                case "playlist":
                     var playlistParams = HttpUtility.ParseQueryString(uri.Query);
                     string? playlistId = playlistParams["list"];
                     if (!string.IsNullOrEmpty(playlistId))
                     {
                         link.linktype = Linktype.Playlist;
                         link.yt_id = playlistId;
                     }
                     else
                     {
                         Log.Information("Invalid YouTube playlist URL: {Url}", url);
                         return null;
                     }
                     break;
                    
                case "shorts":
                    if (segments.Length > 1 && !string.IsNullOrEmpty(segments[1]))
                    {
                        link.linktype = Linktype.Short;
                        link.yt_id = segments[1];
                    }
                    else
                    {
                        Log.Information("Invalid YouTube shorts URL: {Url}", url);
                        return null;
                    }
                    break;
                
                case "channel":
                    if (segments.Length > 1 && !string.IsNullOrEmpty(segments[1]))
                    {
                        link.linktype = Linktype.Channel_channel;
                        link.yt_id = segments[1];
                    }
                    else
                    {
                        Log.Information("Invalid YouTube channel URL: {Url}", url);
                        return null;
                    }
                    break;

                case "c":
                    if (segments.Length > 1 && !string.IsNullOrEmpty(segments[1]))
                    {
                        link.linktype = Linktype.Channel_c;
                        link.yt_id = segments[1];
                    }
                    else
                    {
                        Log.Information("Invalid YouTube c channel URL: {Url}", url);
                        return null;
                    }
                    break;
                
                case "user":
                    if (segments.Length > 1 && !string.IsNullOrEmpty(segments[1]))
                    {
                        link.linktype = Linktype.Channel_user;
                        link.yt_id = segments[1];
                    }
                    else
                    {
                        Log.Information("Invalid YouTube user channel URL: {Url}", url);
                        return null;
                    }
                    break;
                
                case "results":
                    link.linktype = Linktype.Search;
                    break;
                
                default:
                    // Handle channel URLs with '@' handles, which don't have a specific path like "channel"
                    if (path.StartsWith("@"))
                    {
                        link.linktype = Linktype.Channel_at;
                        string handle = path.Substring(1); // Remove '@'
                        if (!string.IsNullOrEmpty(handle))
                        {
                            link.yt_id = handle; 
                        }
                        else
                        {
                            Log.Information("Invalid YouTube @handle: {Url}", url);
                            return null;
                        }
                    }
                    else
                    {
                        Log.Information("Invalid YouTube URL: {Url}", url);
                        return null;
                    }
                    break;
            }
        }
        else
        {
            Log.Information("This is a base youtube.com link with no path. Not a video/channel/etc: {Url}", url);
            return null;
        }

        var enrichedlink = EnrichLinkWithMemberIds(link);

        Log.Verbose("Url {Url} was parsed to ytlink {Link}", url, enrichedlink);
        return enrichedlink;
    }
    
    private static YTLink EnrichLinkWithMemberIds(YTLink link)
    {
        switch (link.linktype)
        {
            case Linktype.Video:
            case Linktype.Short:
                if (string.IsNullOrEmpty(link.yt_id))
                    Log.Warning("{Link} does not have yt_id filled!", link);
                break;
            case Linktype.Search:
                break;
            case Linktype.Playlist:
                link.MemberIds = YtdlpInterfacing.GetVideoIdsFromPlaylistUrl(link.url);
                break;
            case Linktype.Channel_c:
            case Linktype.Channel_user:
            case Linktype.Channel_channel:
            case Linktype.Channel_at:
                link.MemberIds = YtdlpInterfacing.GetVideoIdsFromChannelUrl(link.url);
                break;
        }
        return link;
    }

    #region Scripts

    /// <summary>
    ///     Creates the scripts in every filesystem folder where they are necessary. Operating system aware. <br />
    ///     Requires:
    ///     <list type="bullet">
    ///         <item> Urls </item>
    ///         <item> Name </item>
    ///         <item> FolderPath </item>
    ///     </list>
    /// </summary>
    /// <param name="folders">The resolved folders with filesystem paths and links.</param>
    /// <param name="ytdlp_path">Path to the yt-dlp binary which will be called by the scripts.</param>
    /// <exception cref="DirectoryNotFoundException">If folder.FolderPath does not exist for any one folder.</exception>
    public static void Scriptwriter(List<ResolvedFolder> folders, string ytdlp_path)
    {
        if (!File.Exists(ytdlp_path))
        {
            Log.Warning("Writing scripts with faulty yt-dlp binary path! The binary does not exist: {YtDlpPath}.", ytdlp_path);
        }

        bool foldersok = true;
        foreach (ResolvedFolder folder in folders)
        {
            if (!Directory.Exists(folder.FolderPath))
            {
                Log.Error("Directory does not exist for the folder {FolderName}: {FolderPath}. Cannot write scripts!", folder.Name, folder.FolderPath);
                foldersok = false;
            }
        }

        if (!foldersok)
            throw new DirectoryNotFoundException(
                "The directories for scriptwriting could not be found for one or more folders.");

        string scriptExtension;
        Func<string, string, string> scriptContentFactory;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            scriptExtension = ".bat";
            //uft8 charset in commandline - it will not work without this if there are special characters in access path
            scriptContentFactory = (path, txtFile) => $"chcp 65001\r\n\"{path}\" -a \"{txtFile}\"";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            scriptExtension = ".sh";
            scriptContentFactory = (path, txtFile) => $"#! /bin/bash\n\"{path}\" -a \"{txtFile}\"";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            scriptExtension = ".sh";
            scriptContentFactory = (path, txtFile) => $"#!/usr/bin/env bash\n\"{path}\" -a \"{txtFile}\"";
        }
        else { throw new PlatformNotSupportedException();}

        foreach (var folder in folders.Where(f => f.Urls.Count > 0))
        {
            string scriptPath = Path.Combine(folder.FolderPath, folder.Name + scriptExtension);
            string txtFilePath = Path.Combine(folder.FolderPath, folder.Name + ".txt");
            string scriptContent = scriptContentFactory(ytdlp_path, txtFilePath);
    
            File.WriteAllText(scriptPath, scriptContent);
            Log.Verbose("{FolderId}/{FolderCount} folder script writing finished.", folder.Id, folders.Count);
        }
        Log.Information("{FolderCount} folder script file writing finished.", folders.Count);
    }

    /// <summary>
    ///     Executes the batch or bash scripts in every folder. The scripts had to be written before (by Scriptwriter()).
    ///     <br />
    ///     Requires:
    ///     <list type="bullet">
    ///         <item> FolderPath </item>
    ///         <item> Urls </item>
    ///     </list>
    /// </summary>
    /// <param name="folders">Resolved folders with filesystem paths.</param>
    public static void Runningthescripts(List<ResolvedFolder> folders)
    {
        if (true) //TODO: check for interactivity in CLI mode here
        {
            Log.Information("Running the scripts, press enter to confirm.");
            Console.Read();
        }

        string extensionforscript = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) extensionforscript = ".bat";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) extensionforscript = ".sh";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) extensionforscript = ".sh";

        foreach (ResolvedFolder folder in folders)
        {
            if (folder.Urls.Count > 0)
            {
                int downloadserialnumber = 1;
                string targetDir = string.Format(folder.FolderPath);
                Process process;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string command = "\"" + Path.Combine(targetDir, folder.Name + extensionforscript) + "\"";
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo("cmd.exe", $"/c {command}")
                        {
                            WorkingDirectory = targetDir,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        }
                    };
                    Log.Debug("Executing: cmd.exe /c {Command}", command);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string command = "\"" + Path.Combine(targetDir, folder.Name + extensionforscript) + "\"";
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo("bash", command)
                        {
                            WorkingDirectory = targetDir,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        }
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string command = "\"" + Path.Combine(targetDir, folder.Name + extensionforscript) + "\"";
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo("bash", command)
                        {
                            WorkingDirectory = targetDir,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        }
                    };
                }
                else
                {
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo()
                    };
                    Log.Error("Platform not supported for script execution.");
                    Environment.Exit(1);
                }

                process.OutputDataReceived += (sender, e) =>
                {
                    Console.WriteLine("output :: " + e.Data);
                    File.AppendAllText(
                        Path.Combine(folder.FolderPath,
                            "log" + DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'h'mm'm'ss") + ".txt"),
                        e.Data + Environment.NewLine);
                    if (e.Data != null && e.Data.Contains("[youtube] Extracting URL:"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FILE " + downloadserialnumber + " / " + folder.Urls.Count +
                                          "---------------------------" + "Folder: " + folder.Name + "(" +
                                          folders.IndexOf(folder) + "out of " + folders.Count + ")");
                        downloadserialnumber++;
                        Console.ResetColor();
                    }
                };
                process.ErrorDataReceived += (sender, e) => Console.WriteLine("error :: " + e.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                Console.WriteLine("ExitCode: {0}", process.ExitCode);
                process.Close();
                File.Delete(Path.Combine(folder.FolderPath, folder.Name + extensionforscript));
            }

            Console.Write("{0} Folder was downloaded. ", folder.Name);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(folders.IndexOf(folder) + 1 + "/" + folders.Count);
            Console.ResetColor();
            Console.Write(" folders are finished\n");
        }
    }

    #endregion Scripts
}