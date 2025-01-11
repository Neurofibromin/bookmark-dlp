using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using bookmark_dlp.Models;
using NfLogger;

namespace bookmark_dlp;

public static class AppMethods
{
    /// <summary>
    /// Finds yt-dlp binary. Checks multiple places. More details in project Readme.
    /// </summary>
    /// <param name="rootdir">The rootdir where bookmark-dlp is called from.</param>
    /// <returns>String containing the yt-dlp binary filepath.</returns>
    /// <exception cref="Exception">yt-dlp is not installed! Cannot proceed.</exception>
    public static string Yt_dlp_pathfinder(string rootdir = "")
    {
        string ytdlp_path = ""; //checks is yt-dlp binary is present in root or if it is on path, returns ytdlp_path so it can be written into the script files
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!String.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, "yt-dlp.exe")))
            {
                //Console.WriteLine(Path.Combine(rootdir, "yt-dlp.exe") + " found");
                ytdlp_path = Path.Combine(rootdir, "yt-dlp.exe");
            }
            else
            {
                //Console.WriteLine(Path.Combine(rootdir, "yt-dlp.exe") + " not found, searching PATH.");
                //TODO: Windows path check for yt-dlp
                // maybe already works? test.
                try
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = "yt-dlp.exe";
                    proc.Start();
                    proc.WaitForExit();
                    ytdlp_path = "yt-dlp.exe";
                }
                // check into Win32Exceptions and their error codes!
                catch (Win32Exception winEx)
                {
                    if (winEx.NativeErrorCode == 2 || winEx.NativeErrorCode == 3)
                    {
                        // 2 => "The system cannot find the FILE specified."
                        // 3 => "The system cannot find the PATH specified."
                        Logger.LogVerbose("yt-dlp not installed!", Logger.Verbosity.Critical);
                        throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing.");
                        //return null;
                    }
                    else
                    {
                        // unknown Win32Exception, re-throw to show the raw error msg
                        throw;
                    }
                }
                ytdlp_path = "yt-dlp.exe"; //no exception was thrown, so yt-dlp must be on the path
            }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] filenames = { "yt-dlp", "yt-dlp_linux", "yt-dlp_linux_aarch64", "yt-dlp_linux_armv7l" };
            foreach (string filename in filenames)
            {
                // check for yt-dlp executable in working rootdir
                if (!String.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, filename)))
                {
                    Logger.LogVerbose("yt-dlp binary found at: " + Path.Combine(rootdir, filename), Logger.Verbosity.Debug);
                    ytdlp_path = Path.Combine(rootdir, filename);
                    break;
                }
            }
            if (ytdlp_path == "")
            {
                // check for yt-dlp binary on path
                Logger.LogVerbose(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.", Logger.Verbosity.Info);
                string command = $"-c \"if (which yt-dlp); then echo \"true\"; else echo \"false\"; fi\"";
                command = $"-c \"which yt-dlp\"";
                Process process = new Process();
                // Console.WriteLine("command: " + command);
                var processStartInfo = new ProcessStartInfo()
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "bash",
                    WorkingDirectory = rootdir,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = command
                };
                process.StartInfo = processStartInfo;
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                String error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                // Console.WriteLine("Result: " + result.Trim());
                // Console.WriteLine("Error: " + error);
                if (result.Contains("which"))
                {
                    Logger.LogVerbose("yt-dlp not installed!", Logger.Verbosity.Critical);
                    throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing.");
                }
                else //yt-dlp is on the path
                {
                    ytdlp_path = result.Trim();
                }
                // Console.WriteLine("ExitCode: {0}", process.ExitCode);
                process.Close();
            }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string[] filenames = { "yt-dlp", "yt-dlp_macos", "yt-dlp_macos_legacy" };
            foreach (string filename in filenames)
            {
                if (!String.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, filename)))
                {
                    // Console.WriteLine(Path.Combine(rootdir, filename) + " found");
                    ytdlp_path = Path.Combine(rootdir, filename);
                    break;
                }
            }
            if (ytdlp_path == "")
            {
                throw new NotImplementedException();
                // TODO check OSX path for yt-dlp
                /*Console.WriteLine(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.");
            Console.WriteLine("Is it on the path? Y/N");
            if (Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
            else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }*/
            }
        }
        return ytdlp_path;
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
    /// Delete filesystem folders that are associated with bookmark folders if 
    /// 1) filesystems folder has no files AND
    /// 2) filesystem folder has no folders <br/>
    /// Requires:
    /// <list type="bullet">
    /// <item> name </item>
    /// <item> depth </item>
    /// <item> parent </item>
    /// <item> folderpath </item>
    /// </list>
    /// </summary>
    /// <param name="folders"></param>
    public static void Deleteemptyfolders(List<Folderclass> folders)
    {
        int a = 0;
        var deepestdepth = folders.Select(f => f.depth).Prepend(0).Max(); //Finding the deepest folder depth
        for (int q = deepestdepth; q > 0; q--) //deleting empty folders from the deepest layer upwards
        {
            foreach (var t in folders)
            {
                if (t.depth == q) //only check folders with the given depth
                {
                    bool thisfolderisempty = true;
                    string path = t.folderpath;
                    if (Directory.Exists(path))
                    {
                        if (Directory.GetDirectories(@path).Length != 0) //check if the given directory has any children directories
                        {
                            thisfolderisempty = false;
                        }
                        if (Directory.GetFiles(t.folderpath).Length != 0) //check if the given directory has any files
                        {
                            thisfolderisempty = false;
                        }
                        if (thisfolderisempty == true)
                        {
                            Directory.Delete(t.folderpath);
                            a++;
                        }
                    }
                }
            }
        }
        Logger.LogVerbose($"{a} empty folders deleted", Logger.Verbosity.Info);
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

    /// <summary>
    /// Asks user if they want playlists, shorts and channels downloaded.
    /// </summary>
    /// <returns>Want (downloadPlaylists, downloadShorts, downloadChannels) in this order.</returns>
    public static (bool, bool, bool) Wantcomplex()
    {
        Logger.LogVerbose("Do you want to write and download not video links? (eg. playlists and channels. by default: no)");
        Logger.LogVerbose("Depending on the yt-dlp conf settings this can result in very large downloads, a single bookmark can lead to hundreds of videos being downloaded.");
        Logger.LogVerbose("Y/N");
        bool downloadPlaylists = false;
        bool downloadShorts = false;
        bool downloadChannels = false;
        if (Logger.verbosity >= Logger.Verbosity.Info)
        {
            if (Console.ReadKey().ToString().ToLower().Equals("y"))
            {
                Logger.LogVerbose("Playlists? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { downloadPlaylists = true; }
                Logger.LogVerbose("Shorts? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { downloadShorts = true; }
                Logger.LogVerbose("Channels? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { downloadChannels = true; }
            }
        }
        return (downloadPlaylists, downloadShorts, downloadChannels);
    }

    /// <summary>
    /// Finds config file for bookmark-dlp. Searches multiple locations, more details in project Readme. 
    /// </summary>
    /// <returns>Found config path or NULL, if config not found.</returns>
    public static string? ConfigFileLocation()
    {
        string configpath_local = Path.Combine(Directory.GetCurrentDirectory(), "bookmark-dlp.conf");
        if (File.Exists(configpath_local))
        {
            return configpath_local;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string configpath_osx = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "bookmark-dlp", "bookmark-dlp.conf");
            if (File.Exists(configpath_osx)) { return configpath_osx; }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string configpath_windows = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bookmark-dlp", "bookmark-dlp.conf");
            if (File.Exists(configpath_windows)) { return configpath_windows; }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string configpath_linux = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmark-dlp", "bookmark-dlp.conf");
            if (File.Exists(configpath_linux)) { return configpath_linux; }
        }

        return null; //no config paths were found
    }

    public static bool IsConfigPresent()
    {
        if (ConfigFileLocation() == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    
    /// <summary>
    /// Generating Hierarchical Observable FolderCollection from folders, used when displaying the list of folders in the TreeDataGrid.
    /// </summary>
    /// <param name="folders"></param>
    /// <returns></returns>
    public static ObservableCollection<HierarchicalFolderclass> GenerateHierarchicalFolderclassesFromList(List<Folderclass> folders)
    {
        ObservableCollection<HierarchicalFolderclass> hierarchicalFolderclasses = new ObservableCollection<HierarchicalFolderclass>();
        
        foreach (Folderclass folder in folders.Where(a => a.depth == 0))
        {
            hierarchicalFolderclasses.Add(new HierarchicalFolderclass(folder) { IsExpanded = true });
            Logger.LogVerbose("added root: " + folder.name, Logger.Verbosity.Trace);
        }
        foreach (Folderclass folder in folders.Where(a => a.depth != 0).OrderBy(a => a.depth))
        {
            Logger.LogVerbose("examining " + folder.name + " " + folder.id + " parent:" + folder.parent, Logger.Verbosity.Trace);
            bool foundparent = false;
            foreach (HierarchicalFolderclass parent in hierarchicalFolderclasses)
            {
                if (parent.Id == folder.parent) { 
                    Logger.LogVerbose("Found parent: " + parent.Name, Logger.Verbosity.Trace);
                    parent._children.Add(new HierarchicalFolderclass(folder));
                    parent.HasChildren = true;
                    foundparent = true;
                    break;
                }
            }
            if (!foundparent)
            {
                hierarchicalFolderclasses.Add(new HierarchicalFolderclass(folder));
                Logger.LogVerbose("The following folder has no parent despite depth != 0: " + folder.name, Logger.Verbosity.Error);
            }
            // HierarchcalFolderCollection.Single(parent => parent.Id == folder.parent).Children.Add(onefolder);
            Logger.LogVerbose("added: " + folder.name, Logger.Verbosity.Trace);
        }
        return hierarchicalFolderclasses;
    }

    public enum ProgramUI { GUI, CLI }
    public static ProgramUI programUI;

    /// <summary>
    /// Count how many videos are wanted directly or indirectly. <br/>
    /// Requires:
    /// <list type="bullet">
    /// <item> links </item>
    /// </list>
    /// Fills:
    /// <list type="bullet"> 
    /// <item> numberOfVideosDirectlyWanted </item>
    /// <item> numberOfVideosIndirectlyWanted </item>
    /// </list>    
    /// </summary>
    /// <param name="folder"></param>
    public static void CountWantedVideos(ref Folderclass folder)
    {
        foreach (YTLink link in folder.links)
        {
            if (link.linktype == Linktype.Video || link.linktype == Linktype.Short)
            {
                folder.numberOfVideosDirectlyWanted++;
            }
            else if (link.linktype == Linktype.Channel_channel ||
                     link.linktype == Linktype.Channel_at ||
                     link.linktype == Linktype.Channel_user ||
                     link.linktype == Linktype.Channel_c ||
                     link.linktype == Linktype.Playlist)
            {
                folder.numberOfVideosIndirectlyWanted += link.member_ids.Count;
            }
        }
    }
    
    
    /// <summary>
    /// Checks if wanted videos are found on the filesystem (are/were downloaded) and fills Folderclass fields for object accordingly.
    /// Only checks the filenames for the yt-id (11 characters): if yt-dlp config is set to not include such id in the filename it will not work.<br/>
    /// Requires:
    /// <list type="bullet">
    /// <item> folderpath </item>
    /// <item> links </item>
    /// </list>
    /// Fills:
    /// <list type="bullet"> 
    /// <item> numberOfDirectlyWantedVideosFound </item>
    /// <item> numberOfIndirectlyWantedVideosFound </item>
    /// <item> numberOfOtherVideosFound </item>
    /// <item> LinksWithNoMissingVideos </item>
    /// <item> LinksWithMissingVideos </item>
    /// </list>  
    /// </summary>
    /// <param name="folders">The list of folders that is being checked</param>
    public static void CheckCurrentFilesystemState(ref List<Folderclass> folders)
    {
        foreach (Folderclass folder in folders)
        {
            folder.numberOfDirectlyWantedVideosFound = 0;
            folder.numberOfIndirectlyWantedVideosFound = 0;
            folder.numberOfOtherVideosFound = 0;
            if (Directory.Exists(folder.folderpath))
            {
                var files = Directory.GetFiles(folder.folderpath);
                HashSet<string>? ytIdsFoundInArchive = null;
                if (File.Exists(Path.Combine(folder.folderpath, "archive.txt"))) //checks the archive.txt written by yt-dlp if the config is used there
                {
                    string[] archivecheckerlist = File.ReadAllLines(Path.Combine(folder.folderpath, "archive.txt"));
                    string[] ytIdsFoundInArchiveTxt = archivecheckerlist.Where(f => f.StartsWith("youtube")).Select(t => t.Substring(8, 11)).ToArray();
                    ytIdsFoundInArchive = new HashSet<string>(ytIdsFoundInArchiveTxt);
                    /*foreach (string link in archivecheckerlist)
                    {
                        if (link.StartsWith("youtube")) //only check for youtube videos downloaded by yt-dlp in the given folder
                        {
                            string youtubeid = link.Substring(8, 11); //start at coloumn 8 because archive.txt does not store links, rather only the platform name and the video id
                        }
                    }*/
                }
                foreach (YTLink link in folder.links)
                {
                    List<string>? idsToCheck = null;
                    bool found = true;
                    if (link.linktype == Linktype.Short ||
                        link.linktype == Linktype.Video)
                    {
                        if (ytIdsFoundInArchive != null)
                        {
                            if (ytIdsFoundInArchive.Contains(link.yt_id)) // in archive.txt
                            {
                                folder.LinksWithNoMissingVideos.Add(link);
                                Logger.LogVerbose($"In folder {folder.folderpath} video {link.url} found in archive.txt", Logger.Verbosity.Trace);
                                continue;
                            }
                        }
                        if (files.Any(s => s.Contains(link.yt_id)))
                        {
                            folder.LinksWithNoMissingVideos.Add(link);
                            Logger.LogVerbose($"In folder {folder.folderpath} video {link.url} found in files list", Logger.Verbosity.Trace);
                        }
                        else
                        {
                            folder.LinksWithMissingVideos.Add(link);
                            Logger.LogVerbose($"In folder {folder.folderpath} video {link.url} not found", Logger.Verbosity.Trace);
                        }
                        continue;
                    }
                    else if (link.linktype == Linktype.Channel_channel ||
                             link.linktype == Linktype.Channel_at ||
                             link.linktype == Linktype.Channel_user ||
                             link.linktype == Linktype.Channel_c ||
                             link.linktype == Linktype.Playlist)
                    {
                        idsToCheck = link.member_ids;
                    }
                    else //link.linktype == Linktype.Search
                    {
                        continue;
                    }
                    if (idsToCheck == null)
                    {
                        Logger.LogVerbose($"Could not ascertain which videos are wanted by link {link}. May be a network error", Logger.Verbosity.Error);
                        found = false;
                        folder.LinksWithMissingVideos.Add(link);
                        continue;
                    }
                    foreach (string id in idsToCheck)
                    {
                        if (ytIdsFoundInArchive != null)
                        {
                            if (ytIdsFoundInArchive.Contains(id)) // in archive.txt
                            {
                                link.member_ids_found.Add(id);
                                Logger.LogVerbose($"In folder {folder.folderpath} member id {id} for link {link.url} found in archive.txt", Logger.Verbosity.Trace);
                                continue;
                            }
                        }
                        //NOTE: this may be a slow operation
                        if (files.Any(s => s.Contains(id)))
                        {
                            link.member_ids_found.Add(id);
                            Logger.LogVerbose($"In folder {folder.folderpath} member id {id} for link {link.url} found in files list", Logger.Verbosity.Trace);
                        }
                        else
                        {
                            link.member_ids_not_found.Add(id);
                            Logger.LogVerbose($"In folder {folder.folderpath} member id {id} for link {link.url} not found", Logger.Verbosity.Trace);
                            found = false;
                        }
                    }
                    if (found)
                    {
                        folder.LinksWithNoMissingVideos.Add(link);
                        Logger.LogVerbose($"All members were found for {link.url} in folder {folder.folderpath}", Logger.Verbosity.Trace);
                    }
                    else
                    {
                        folder.LinksWithMissingVideos.Add(link);
                        Logger.LogVerbose($"Not all members were found for {link.url} in folder {folder.folderpath}", Logger.Verbosity.Trace);
                    }
                }
            }
            else
            {
                continue;
            }
            // folder.numberOfIndirectlyWantedVideosNotFound = folder.links.Select(f => f.member_ids_not_found).ToList().Select(f => f.Count()).Sum();
            // folder.numberOfDirectlyWantedVideosNotFound = folder.LinksWithMissingVideos.Count(f => f.linktype is Linktype.Video or Linktype.Short);
            // folder.numberOfDirectlyWantedVideosNotFound = folder.numberOfVideosDirectlyWanted - folder.numberOfDirectlyWantedVideosFound;
            folder.numberOfIndirectlyWantedVideosFound = folder.links.Select(f => f.member_ids_found.Count).Sum();
            folder.numberOfDirectlyWantedVideosFound = folder.LinksWithNoMissingVideos.Count(f => f.linktype is Linktype.Video or Linktype.Short);
        }
    }
}