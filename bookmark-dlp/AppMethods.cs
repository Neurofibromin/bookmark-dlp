using bookmark_dlp;
using Microsoft.Data.Sqlite;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NfLogger;

internal class AppMethods
{
    /// <summary>
    /// Check whether the youtube links that were on the list are now dowloaded into the appripriate directories. Good for finding rotten links.
    /// Only checks the filenames for the yt-id (11 characters): if yt-dlp config is set to not include such id in the filename it will not work.
    /// yt-dlp logs could also be parsed for the same info, although if a video was downloaded in the past and no longer available on the net, it would still be flagged (?) - depends on the archive.txt usage setting
    /// </summary>
    /// <param name="rootdir">Filesystem directory path to find videos in</param>
    /// <param name="folders"></param>
    public static void Checkformissing(string rootdir, List<Folderclass> folders)
    {
        //TODO: handle channels
        List<Bookmark> notfoundbookmarks = new List<Bookmark>();
        foreach (Folderclass folder in folders)
        {
            if (Directory.Exists(folder.folderpath))
            {
                Directory.SetCurrentDirectory(folder.folderpath);
                int checkspassed = 0;
                if (File.Exists(folder.name + ".txt"))
                {
                    string[] linkcheckerlist = File.ReadAllLines(folder.name + ".txt");
                    foreach (string link in linkcheckerlist)
                    {
                        string youtubeid = link.Substring(32, 11);
                        bool contains = Directory.EnumerateFiles(Directory.GetCurrentDirectory()).Any(f => f.Contains(youtubeid));
                        if (!contains)
                        {
                            Logger.LogVerbose($"{youtubeid} in folder: {folder.name} not found.", Logger.Verbosity.Warning);
                            notfoundbookmarks.Add(new Bookmark() { url = link });
                        }
                    }
                    checkspassed++;
                }
                if (File.Exists("archive.txt")) //checks the archive.txt written by yt-dlp if the config is used there
                {
                    string[] archivecheckerlist = File.ReadAllLines("archive.txt");
                    foreach (string link in archivecheckerlist)
                    {
                        if (link.StartsWith("youtube")) //only check for youtube videos downloaded by yt-dlp in the given folder
                        {
                            string youtubeid = link.Substring(8, 11); //start at coloumn 8 because archive.txt does not store links, rather only the platform name and the video id
                            bool contains = Directory.EnumerateFiles(Directory.GetCurrentDirectory()).Any(f => f.Contains(youtubeid));
                            if (!contains)
                            {
                                Logger.LogVerbose($"{youtubeid} in folder: {folder.name} not found, despite it being present in archive.txt.", Logger.Verbosity.Warning);
                                notfoundbookmarks.Add(new Bookmark() { url = link });
                            }
                        }
                    }
                    checkspassed++;
                }
                if (checkspassed == 0) { Logger.LogVerbose($"No checks for directory content passed for {folder.name}.", Logger.Verbosity.Warning); }
                folder.numberofmissinglinks = notfoundbookmarks.Distinct().Count();
                Logger.LogVerbose($"Number of missing links in directory {folder.name}: {folder.numberofmissinglinks}");
            }
        }
    }

    /// <summary>
    /// Finds yt-dlp binary. Checks multiple places. More details in project Readme.
    /// </summary>
    /// <param name="rootdir">The rootdir where bookmark-dlp is called from.</param>
    /// <returns>String containing the yt-dlp binary filepath.</returns>
    /// <exception cref="Exception">yt-dlp is not installed! Cannot proceed.</exception>
    public static string Yt_dlp_pathfinder(string rootdir)
    {
        string ytdlp_path = ""; //checks is yt-dlp binary is present in root or if it is on path, returns ytdlp_path so it can be written into the script files
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (rootdir != null && File.Exists(Path.Combine(rootdir, "yt-dlp.exe")))
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
                if (rootdir != null && File.Exists(Path.Combine(rootdir, filename)))
                {
                    // Console.WriteLine(Path.Combine(rootdir, filename) + " found");
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
                //Console.WriteLine("Is it on the path? Y/N");
                //if(Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
                //else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }
            }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string[] filenames = { "yt-dlp", "yt-dlp_macos", "yt-dlp_macos_legacy" };
            foreach (string filename in filenames)
            {
                if (rootdir != null && File.Exists(Path.Combine(rootdir, filename)))
                {
                    // Console.WriteLine(Path.Combine(rootdir, filename) + " found");
                    ytdlp_path = Path.Combine(rootdir, filename);
                }
            }
            if (ytdlp_path == "")
            {
                // TODO check OSX path for yt-dlp
                /*Console.WriteLine(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.");
                Console.WriteLine("Is it on the path? Y/N");
                if (Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
                else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }*/
                return null;
            }
        }
        return ytdlp_path;
    }

    /// <summary>
    /// Creates the scripts in every filesystem folder where they are necessary. Operating system aware.
    /// </summary>
    /// <param name="folders">The bookmark folder structure, where every bookmark folder already has the folderpath field filled with the correct filesystem folder path.</param>
    /// <param name="ytdlp_path">Path to the yt-dlp binary which will be called by the scripts.</param>
    public static void Scriptwriter(List<Folderclass> folders, string ytdlp_path)
    {
        string extensionforscript = ""; //writing scripts
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            extensionforscript = ".bat";
            for (int q = 0; q < folders.Count; q++) //writing bat files for every folder
            {
                if (folders[q].numberoflinks > 0)
                {
                    StreamWriter writer1 = new StreamWriter(Path.Combine(folders[q].folderpath, folders[q].name + extensionforscript), append: true);
                    writer1.WriteLine("chcp 65001"); //uft8 charset in commandline - it will not work without this if there are special characters in access path
                    writer1.WriteLine("\"" + ytdlp_path + "\" -a \"" + Path.Combine(folders[q].folderpath, folders[q].name + ".txt") + "\"");
                    //writer1.WriteLine("pause");
                    writer1.Flush();
                    writer1.Close();
                }
                Logger.LogVerbose(q + "/" + folders.Count + " folder bat file writing finished.", Logger.Verbosity.Trace);
            }
            Logger.LogVerbose(folders.Count + " folder bat file writing finished.", Logger.Verbosity.Info);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            extensionforscript = ".sh";
            for (int q = 0; q < folders.Count; q++) //writing sh files for every folder
            {
                if (folders[q].numberoflinks > 0)
                {
                    StreamWriter writer1 = new StreamWriter(Path.Combine(folders[q].folderpath, folders[q].name + extensionforscript), append: true);
                    writer1.WriteLine("#! /bin/bash");
                    writer1.WriteLine("\"" + ytdlp_path + "\" -a \"" + Path.Combine(folders[q].folderpath, folders[q].name + ".txt") + "\"");
                    //writer1.WriteLine("pause");
                    writer1.Flush();
                    writer1.Close();
                }
                Logger.LogVerbose(q + "/" + folders.Count + " folder sh file writing finished.", Logger.Verbosity.Trace);
            }
            Logger.LogVerbose(folders.Count + " folder sh file writing finished", Logger.Verbosity.Info);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            extensionforscript = ".sh";
            for (int q = 0; q < folders.Count; q++) //writing sh files for every folder
            {
                if (folders[q].numberoflinks > 0)
                {
                    StreamWriter writer1 = new StreamWriter(Path.Combine(folders[q].folderpath, folders[q].name + extensionforscript), append: true);
                    writer1.WriteLine("#!/usr/bin/env bash");
                    writer1.WriteLine("\"" + ytdlp_path + "\" -a \"" + Path.Combine(folders[q].folderpath, folders[q].name + ".txt") + "\"");
                    //writer1.WriteLine("pause");
                    writer1.Flush();
                    writer1.Close();
                }
                Logger.LogVerbose(q + "/" + folders.Count + " folder sh file writing finished.", Logger.Verbosity.Trace);
            }
            Logger.LogVerbose(folders.Count + " folder sh file writing finished", Logger.Verbosity.Info);
        }
        if (Logger.verbosity >= Logger.Verbosity.Info)
        {
            Logger.LogVerbose("Waiting for enter to confirm");
            Console.Read();
        }
    }

    /// <summary>
    /// Delete filesystem folders that are associated with bookmark folders if 
    /// 1) filesystems folder has no files AND
    /// 2) filesystem folder has no folders
    /// </summary>
    /// <param name="folders"></param>
    public static void Deleteemptyfolders(List<Folderclass> folders)
    {
        int deepestdepth = 0; //Finding the deepest folder depth
        for (int q = 0; q < folders.Count; q++)
        {
            if (deepestdepth < folders[q].depth)
            {
                deepestdepth = folders[q].depth;
            }
        }
        for (int q = deepestdepth; q > 0; q--) //deleting empty folders from the deepest layer upwards
        {
            for (int j = 0; j < folders.Count; j++)
            {
                if (folders[j].depth == q) //only check folders with the given depth
                {
                    bool thisfolderisempty = true;
                    string path = folders[j].folderpath;
                    if (Directory.Exists(path))
                    {
                        if (Directory.GetDirectories(@path).Length != 0) //check if the given directory has any children directories
                        {
                            thisfolderisempty = false;
                        }
                        if (Directory.GetFiles(folders[j].folderpath).Length != 0) //check if the given directory has any files
                        {
                            thisfolderisempty = false;
                        }
                        if (thisfolderisempty == true)
                        {
                            Directory.Delete(folders[j].folderpath);
                        }
                    }
                }
            }
        }
        Logger.LogVerbose("Empty folders deleted", Logger.Verbosity.Info);
        if (Logger.verbosity >= Logger.Verbosity.Info)
        {
            Logger.LogVerbose("Waiting for enter to confirm");
            Console.Read();
        }
    }

    /// <summary>
    /// Executes the batch or bash scripts in every folder. The scripts had to be written before (by Scriptwriter()) and the Folderclass folderpaths must be correct.
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

        //running the script files one after another, in the order of folders[].startline
        for (int j = 0; j < folders.Count; j++)
        {
            if (folders[j].numberoflinks > 0)
            {
                int downloadserialnumber = 1;
                string targetDir = string.Format(@folders[j].folderpath);
                Process process;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string command = "\"" + Path.Combine(targetDir, folders[j].name + extensionforscript) + "\"";
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
                    string command = "\"" + Path.Combine(targetDir, folders[j].name + extensionforscript) + "\"";
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
                    string command = "\"" + Path.Combine(targetDir, folders[j].name + extensionforscript) + "\"";
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
                    File.AppendAllText(Path.Combine(folders[j].folderpath, "log" + DateTime.Now.ToString("yyyy’-‘MM’-‘dd’-’HH’h’mm’m’ss") + ".txt"), e.Data + Environment.NewLine);
                    if (e.Data != null && e.Data.Contains("[youtube] Extracting URL:"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FILE " + downloadserialnumber + " / " + folders[j].numberoflinks + "---------------------------" + "Folder: " + folders[j].name + "(" + j + "out of " + folders.Count + ")");
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
                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + extensionforscript));
            }
            Console.Write("{0} Folder was downloaded. ", folders[j].name);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write((j + 1) + "/" + folders.Count);
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
            string configpath_osx = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "bookmark-dlp/bookmark-dlp.conf");
            if (File.Exists(configpath_osx)) { return configpath_osx; }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string configpath_windows = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bookmark-dlp\\bookmark-dlp.conf");
            if (File.Exists(configpath_windows)) { return configpath_windows; }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string configpath_linux = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmark-dlp/bookmark-dlp.conf");
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

    public enum ProgramUI { GUI, CLI }
    public static ProgramUI programUI;
    
}
