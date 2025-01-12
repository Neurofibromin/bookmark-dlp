using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using bookmark_dlp;
using Microsoft.Data.Sqlite;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Channels;
using System.Collections.ObjectModel;
using System.Net;
using Nfbookmark;
using NfLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using bookmark_dlp;
using NfLogger;

namespace bookmark_dlp
{
    /// <summary>
    /// Used in the import process
    /// </summary>
    /// <example>
    /// <code>
    /// var folders = Import.SmartImport(somefile);
    /// AutoImport.LinksFromUrls(folders);
    /// AppMethods.CountWantedVideos(folders)
    /// AppMethods.CheckCurrentFilesystemState(folders)
    /// Functions.FoldernameValidation(folders);
    /// Functions.Createfolderstructure(folders, rootdir);
    /// AppMethods.Deleteemptyfolders(folders);
    /// </code>
    /// </example>
    public static class AutoImport
    {
        /// <summary>
        /// Fills links from urls. If not youtube url no link is generated.
        /// If link parsing throws exception no link is generated,
        /// and log message is written, but no exception will be thrown. Wrapper around LinkFromUrl. <br/>
        /// Requires:
        /// <list type="bullet">
        /// <item> urls </item>
        /// </list>
        /// Fills:
        /// <list type="bullet">
        /// <item> links </item>
        /// </list>
        /// </summary>
        /// <param name="folders">The folders that are filled</param>
        public static void LinksFromUrls(List<Folderclass> folders)
        {
            foreach (Folderclass folder in folders)
            {
                foreach (string url in folder.urls)
                {
                    YTLink? link;
                    try
                    {
                        link = AutoImport.LinkFromUrl(url);
                    }
                    catch (InvalidLinkException e)
                    {
                        Logger.LogVerbose(e.Message, Logger.Verbosity.Error);
                        continue;
                    }
                    if (link is not null)
                    {
                        folder.links.Add(link.Value);
                        //already logged in LinkFromUrl(): Logger.LogVerbose($"Link from {url} successfully converted to {link.Value}.", Logger.Verbosity.Trace);
                    }
                    else // link is null, it was not a youtube link
                    {
                        continue;
                    }
                }
            }
        }


        /// <summary>
        /// Writes the links found in the bookmark folder into the filesystem folder in a $foldername.txt file. Id sequencial writing is NOT GUARANTEED.
        /// </summary>
        /// <param name="folders">the bookmark folders containing the links</param>
        /// <param name="rootdir">the filesystem directory (not really used)</param>
        /// <param name="downloadPlaylists">options</param>
        /// <param name="downloadShorts">options</param>
        /// <param name="downloadChannels">options</param>
        /// <returns></returns>
        public static int WritelinkstotxtFromFolderclasses(ref List<Folderclass> folders, string rootdir,
            bool downloadPlaylists = false, bool downloadShorts = false, bool downloadChannels = false)
        {
            //TODO: Deprecate this, use YTLinks instead
            StreamWriter
                temp = new StreamWriter(Path.Combine(rootdir, "temp.txt"),
                    append: true); //writing into temp.txt all the youtube links that are not for videos (but for channels, playlists, etc.)
            int totalyoutubelinknumber = 0;
            for (int j = 0; j < folders.Count; j++)
            {
                StreamWriter writer = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"),
                    append: false);
                StreamWriter complexnotsimple =
                    new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"),
                        append: true); //writing all the youtube links that are not for videos (but for channels, playlists, etc.) in the given folder
                int linknumbercounter = 0; //counts links in one folder
                foreach (string url in folders[j].urls)
                {
                    string linkthatisbeingexamined = url;
                    if (linkthatisbeingexamined.Contains("www.youtube.com")) //only write lines that are youtube links
                    {
                        bool iscomplicated = false;
                        bool isShort = false;
                        bool isChannel = false;
                        bool isPlaylist = false;
                        if (linkthatisbeingexamined.Substring(24, 8) ==
                            "playlist") //filtering the links with the consecutive ifs to find if they are for videos or else (channels, playlists, etc.)
                        {
                            //playlist
                            complexnotsimple.WriteLine(linkthatisbeingexamined);
                            temp.WriteLine(linkthatisbeingexamined);
                            isPlaylist = true;
                        }

                        if (linkthatisbeingexamined.Substring(24, 4) == "user")
                        {
                            //channel
                            complexnotsimple.WriteLine(linkthatisbeingexamined);
                            temp.WriteLine(linkthatisbeingexamined);
                            isChannel = true;
                        }

                        if (linkthatisbeingexamined.Substring(24, 7) == "channel")
                        {
                            //channel
                            complexnotsimple.WriteLine(linkthatisbeingexamined);
                            temp.WriteLine(linkthatisbeingexamined);
                            isChannel = true;
                        }

                        if (linkthatisbeingexamined.Substring(24, 7) ==
                            "results") //youtube search result was bookmarked
                        {
                            //not saving search results
                            temp.WriteLine(linkthatisbeingexamined);
                            iscomplicated = true;
                        }

                        if (linkthatisbeingexamined.Substring(24, 1) == "@")
                        {
                            //channel
                            complexnotsimple.WriteLine(linkthatisbeingexamined);
                            temp.WriteLine(linkthatisbeingexamined);
                            isChannel = true;
                        }

                        if (linkthatisbeingexamined.Substring(24, 2) == "c/")
                        {
                            //channel
                            complexnotsimple.WriteLine(linkthatisbeingexamined);
                            temp.WriteLine(linkthatisbeingexamined);
                            isChannel = true;
                        }

                        if (linkthatisbeingexamined.Substring(24, 6) == "shorts")
                        {
                            //shorts
                            complexnotsimple.WriteLine(linkthatisbeingexamined);
                            temp.WriteLine(linkthatisbeingexamined);
                            isShort = true;
                        }

                        if (!(isShort || isChannel || isPlaylist)) //its normal
                        {
                            writer.WriteLine(linkthatisbeingexamined);
                            linknumbercounter++;
                        }

                        if (isShort && downloadShorts)
                        {
                            writer.WriteLine(linkthatisbeingexamined);
                            linknumbercounter++;
                        }

                        if (isPlaylist && downloadPlaylists)
                        {
                            writer.WriteLine(linkthatisbeingexamined);
                            linknumbercounter++;
                        }

                        if (isChannel && downloadChannels)
                        {
                            writer.WriteLine(linkthatisbeingexamined);
                            linknumbercounter++;
                        }
                        /*if (iscomplicated == false)
                           {
                               writer.WriteLine(linkthatisbeingexamined);
                               if (!wantcomplex)
                               {
                                   totalyoutubelinknumber++;
                                   linknumbercounter++;
                               }
                           }
                           if (wantcomplex)
                           {
                               totalyoutubelinknumber++;
                               linknumbercounter++;
                           }*/
                    }
                }

                writer.Flush();
                writer.Close();
                complexnotsimple.Flush();
                complexnotsimple.Close();
                totalyoutubelinknumber +=
                    linknumbercounter; //increase total link number by number of links found in this folder
                if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt")).Length ==
                    0) //if the txt reamined empty it is deleted
                {
                    File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"));
                }

                if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + ".txt")).Length ==
                    0) //if the txt remained empty it is deleted
                {
                    File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"));
                    Logger.LogVerbose($"Deleted txt of {folders[j].name}", Logger.Verbosity.Trace);
                }
                /*if (!wantcomplex)
                {
                    File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"));
                }*/
            }

            temp.Flush();
            temp.Close();
            Logger.LogVerbose("Total number of youtube links found: " + totalyoutubelinknumber, Logger.Verbosity.Info);
            return totalyoutubelinknumber;
        }

        
        /// <summary>
        /// Parses url to YTLink object and fills:<br/>
        /// <list type="bullet">
        /// <item> url </item>
        /// <item> linktype </item>
        /// <item> channel_id </item>
        /// <item> playlist_id </item>
        /// <item> yt_id </item>
        /// <item> member_ids </item>
        /// </list>
        /// </summary>
        /// <param name="_url">Url to parse, must contain youtube.com. Usually FQDN, like https://www.youtube.com/watch?v=12345678912</param>
        /// <returns>YTLink with parameters filled or null if url is not a youtube link</returns>
        /// <exception cref="InvalidLinkException">If link parsing encounters unexpected characters</exception>
        public static YTLink? LinkFromUrl(string _url)
        {
            // TODO: test this
            if (!_url.Contains("www.youtube.com")) //only work with youtube links
                return null;
            YTLink link = new YTLink();
            link.url = _url;
            //filtering the links with the consecutive ifs to find if they are for videos or else (channels, playlists, etc.)
            if (_url.Substring(24, 8) == "playlist") 
            {
                //playlist
                int start = _url.IndexOf("playlist?list=", StringComparison.Ordinal) + "playlist?list=".Length;
                string temp = _url.Substring(start);
                int end = temp.IndexOf('/');
                if (end == -1)
                  end = _url.Length;
                link.linktype = Linktype.Playlist;
                link.playlist_id = _url.Substring(start, end);
            }
            else if (_url.Substring(24, 4) == "user")
            {
                //channel
                string pattern = @"youtube\.com/user/([a-zA-Z0-9_-]+)";
                Match match = Regex.Match(_url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    link.channel_id = match.Groups[1].Value;
                }
                else
                {
                    throw new InvalidLinkException($"Invalid URL: {_url}, regex pattern: {pattern}");
                }
                link.linktype = Linktype.Channel_user;
            }
            else if (_url.Substring(24, 7) == "channel")
            {
                //channel
                string pattern = @"youtube\.com/channel/([a-zA-Z0-9_-]+)";
                Match match = Regex.Match(_url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    link.channel_id = match.Groups[1].Value;
                }
                else
                {
                    throw new InvalidLinkException($"Invalid URL: {_url}, regex pattern: {pattern}");
                }
                link.linktype = Linktype.Channel_channel;
            }
            else if (_url.Substring(24, 7) == "results") //youtube search result was bookmarked
            {
                //not saving search results
                link.linktype = Linktype.Search;
            }
            else if (_url.Substring(24, 1) == "@")
            {
                //channel
                string pattern = @"youtube\.com/@([a-zA-Z0-9_-]+)";
                Match match = Regex.Match(_url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    link.channel_id = match.Groups[1].Value;
                }
                else
                {
                    throw new InvalidLinkException($"Invalid URL: {_url}, regex pattern: {pattern}");
                }
                link.linktype = Linktype.Channel_at;
            }
            else if (_url.Substring(24, 2) == "c/")
            {
                //channel
                string pattern = @"youtube\.com/c/([a-zA-Z0-9_-]+)";
                Match match = Regex.Match(_url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    link.channel_id = match.Groups[1].Value;
                }
                else
                {
                    throw new InvalidLinkException($"Invalid URL: {_url}, regex pattern: {pattern}");
                }
                link.linktype = Linktype.Channel_c;
            }
            else if (_url.Substring(24, 6) == "shorts")
            {
                //shorts
                string pattern = @"youtube\.com/shorts/([a-zA-Z0-9_-]+)";
                Match match = Regex.Match(_url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    link.yt_id = match.Groups[1].Value;
                }
                else
                {
                    throw new InvalidLinkException($"Invalid URL: {_url}, regex pattern: {pattern}");
                }
                link.linktype = Linktype.Short;
            }
            else
            {
                string regexed, manparsed;
                string pattern = @"youtube\.com/watch\?v=([a-zA-Z0-9_-]+)";
                Match match = Regex.Match(_url, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    regexed = match.Groups[1].Value;
                }
                else
                {
                    throw new InvalidLinkException($"Invalid URL: {_url}, regex pattern: {pattern}");
                }
                int start = _url.IndexOf("watch?v=", StringComparison.Ordinal) + "watch?v=".Length;
                string temp = _url.Substring(start);
                int end = temp.IndexOf('&');
                if (end == -1)
                    end = _url.Length;
                manparsed = _url.Substring(start, end);
                if (manparsed.Length == 11 && regexed.Length == 11 &&
                    String.Equals(manparsed, regexed, StringComparison.CurrentCultureIgnoreCase))
                {
                    link.linktype = Linktype.Video;
                    link.yt_id = manparsed;    
                }
                else
                {
                    Logger.LogVerbose($"Invalid URL: {_url}, REGEX conflict. Regex pattern: {pattern}, manparsed: {manparsed}, regexed: {regexed}.", Logger.Verbosity.Error);
                    link.linktype = Linktype.Video;
                    link.yt_id = manparsed;
                }
            }
            switch (link.linktype)
            {
                case Linktype.Video:
                case Linktype.Short:
                    if (string.IsNullOrEmpty(link.yt_id))
                    {
                        Logger.LogVerbose($"{link} does not have yt_id filled!", Logger.Verbosity.Warning);
                    }
                    break;
                case Linktype.Search:
                    break;
                case Linktype.Playlist:
                    link.member_ids = YtdlpInterfacing.GetVideoIdsFromPlaylistUrl(link.url);
                    break;
                case Linktype.Channel_c:
                case Linktype.Channel_user:
                case Linktype.Channel_channel:
                case Linktype.Channel_at:
                    link.member_ids = YtdlpInterfacing.GetVideoIdsFromChannelUrl(link.url);
                    break;
            }
            Logger.LogVerbose($"Url {_url} was parsed to ytlink {link}", Logger.Verbosity.Trace);
            return link;
        }

        /// <summary>
        /// Creates the scripts in every filesystem folder where they are necessary. Operating system aware. <br/>
        /// Requires:
        /// <list type="bullet">
        /// <item> urls </item>
        /// <item> name </item>
        /// <item> folderpath </item>
        /// <item> id </item>
        /// </list>
        /// </summary>
        /// <param name="folders">The bookmark folder structure, where every bookmark folder already has the folderpath field filled with the correct filesystem folder path.</param>
        /// <param name="ytdlp_path">Path to the yt-dlp binary which will be called by the scripts.</param>
        /// <exception cref="DirectoryNotFoundException">If folder.folderpath does not exist for any one folder.</exception>
        public static void Scriptwriter(List<Folderclass> folders, string ytdlp_path)
        {
            if (!File.Exists(ytdlp_path))
            {
                Logger.LogVerbose(
                    $"Writing scripts with faulty yt-dlp binary path! The binary does not exist: {ytdlp_path}.",
                    Logger.Verbosity.Warning);
            }

            bool foldersok = true;
            foreach (Folderclass folder in folders)
            {
                if (!Directory.Exists(folder.folderpath))
                {
                    Logger.LogVerbose(
                        $"Directory does not exist for the folder {folder.name}: {folder.folderpath}. Cannot write scripts!",
                        Logger.Verbosity.Error);
                    foldersok = false;
                }
            }
            if (!foldersok)
            {
                throw new DirectoryNotFoundException("The directories for scriptwriting could not be found for one or more folders.");
            }

            string extensionforscript = ""; //writing scripts
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                extensionforscript = ".bat";
                foreach (Folderclass folder in folders) //writing bat files for every folder
                {
                    if (folder.urls.Count > 0)
                    {
                        StreamWriter writer1 = new StreamWriter(Path.Combine(folder.folderpath, folder.name + extensionforscript), append: true);
                        writer1.WriteLine("chcp 65001"); //uft8 charset in commandline - it will not work without this if there are special characters in access path
                        writer1.WriteLine("\"" + ytdlp_path + "\" -a \"" + Path.Combine(folder.folderpath, folder.name + ".txt") + "\"");
                        //writer1.WriteLine("pause");
                        writer1.Flush();
                        writer1.Close();
                    }
                    Logger.LogVerbose(folder.id + "/" + folders.Count + " folder bat file writing finished.", Logger.Verbosity.Trace);
                }
                Logger.LogVerbose(folders.Count + " folder bat file writing finished.", Logger.Verbosity.Info);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                extensionforscript = ".sh";
                foreach (Folderclass folder in folders) //writing sh files for every folder
                {
                    if (folder.urls.Count > 0)
                    {
                        StreamWriter writer1 = new StreamWriter(Path.Combine(folder.folderpath, folder.name + extensionforscript), append: true);
                        writer1.WriteLine("#! /bin/bash");
                        writer1.WriteLine("\"" + ytdlp_path + "\" -a \"" + Path.Combine(folder.folderpath, folder.name + ".txt") + "\"");
                        //writer1.WriteLine("pause");
                        writer1.Flush();
                        writer1.Close();
                    }
                    Logger.LogVerbose(folder.id + "/" + folders.Count + " folder sh file writing finished.", Logger.Verbosity.Trace);
                }
                Logger.LogVerbose(folders.Count + " folder sh file writing finished", Logger.Verbosity.Info);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                extensionforscript = ".sh";
                foreach (Folderclass folder in folders) //writing sh files for every folder
                {
                    if (folder.urls.Count > 0)
                    {
                        StreamWriter writer1 = new StreamWriter(Path.Combine(folder.folderpath, folder.name + extensionforscript), append: true);
                        writer1.WriteLine("#!/usr/bin/env bash");
                        writer1.WriteLine("\"" + ytdlp_path + "\" -a \"" + Path.Combine(folder.folderpath, folder.name + ".txt") + "\"");
                        //writer1.WriteLine("pause");
                        writer1.Flush();
                        writer1.Close();
                    }
                    Logger.LogVerbose(folder.id + "/" + folders.Count + " folder sh file writing finished.", Logger.Verbosity.Trace);
                }
                Logger.LogVerbose(folders.Count + " folder sh file writing finished", Logger.Verbosity.Info);
            }
        }

        /// <summary>
        /// Executes the batch or bash scripts in every folder. The scripts had to be written before (by Scriptwriter()). <br/>
        /// Requires:
        /// <list type="bullet">
        /// <item> folderpath </item>
        /// <item> urls </item>
        /// </list>
        /// </summary>
        /// <param name="folders">List containing all the bookmark folders, their folderpath has the filesystem path to the folder representing them.</param>
        public static void Runningthescripts(List<Folderclass> folders)
        {
            if (Logger.verbosity >= Logger.Verbosity.Info)
            {
                Logger.LogVerbose("Running the scripts, press enter to confirm.", Logger.Verbosity.Info);
                Console.Read();
            }
            string extensionforscript = ""; //writing scripts
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { extensionforscript = ".bat"; }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { extensionforscript = ".sh"; }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { extensionforscript = ".sh"; }

            foreach (Folderclass folder in folders)
            {
                if (folder.urls.Count > 0)
                {
                    int downloadserialnumber = 1;
                    string targetDir = string.Format(@folder.folderpath);
                    Process process;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        string command = "\"" + Path.Combine(targetDir, folder.name + extensionforscript) + "\"";
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
                        Logger.LogVerbose($"cmd.exe /c {command}", Logger.Verbosity.Debug);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        string command = "\"" + Path.Combine(targetDir, folder.name + extensionforscript) + "\"";
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
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) //TODO: maybe it does not work, not tested?
                    {
                        string command = "\"" + Path.Combine(targetDir, folder.name + extensionforscript) + "\"";
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
                            {
                            }
                        };
                        Logger.LogVerbose("Error", Logger.Verbosity.Error);
                        System.Environment.Exit(1);
                    }
                    //processInfo.FileName = path.extension;
                    //processInfo.WindowStyle = ProcessWindowStyle.Normal;
                    //process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => Console.WriteLine("output :: " + e.Data);
                    process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        Console.WriteLine("output :: " + e.Data);
                        File.AppendAllText(Path.Combine(folder.folderpath, "log" + DateTime.Now.ToString("yyyy’-‘MM’-‘dd’-’HH’h’mm’m’ss") + ".txt"), e.Data + Environment.NewLine);
                        if (e.Data != null && e.Data.Contains("[youtube] Extracting URL:"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("FILE " + downloadserialnumber + " / " + folder.urls.Count + "---------------------------" + "Folder: " + folder.name + "(" + folders.IndexOf(folder) + "out of " + folders.Count + ")");
                            downloadserialnumber++;
                            Console.ResetColor();
                        }
                    });
                    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => Console.WriteLine("error :: " + e.Data);
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    Console.WriteLine("ExitCode: {0}", process.ExitCode);
                    process.Close();
                    File.Delete(Path.Combine(folder.folderpath, folder.name + extensionforscript));
                }
                Console.Write("{0} Folder was downloaded. ", folder.name);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write((folders.IndexOf(folder) + 1) + "/" + folders.Count);
                Console.ResetColor();
                Console.Write(" folders are finished\n");
            }
        }
    }
}