using bookmark_dlp;
using Microsoft.Data.Sqlite;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal class AppMethods
{
    public static void Checkformissing(string rootdir, Folderclass[] folders, int numberoffolders)
    {
        ///Check whether the youtube links that were on the list are now dowloaded into the appripriate directories. Good for finding rotten links.
        ///Only checks the filenames for the yt-id (11 characters): if yt-dlp config is set to not include such id in the filename it will not work.
        ///yt-dlp logs could also be parsed for the same info, although if a video was downloaded in the past and no longer available on the net, it would still be flagged (?) - depends on the archive.txt usage setting
        List<Bookmark> notfoundbookmarks = new List<Bookmark>();
        //temptative: numberoffolders = folders.Length
        for (int r = 0; r < numberoffolders; r++)
        {
            if (Directory.Exists(folders[r].folderpath))
            {
                Directory.SetCurrentDirectory(folders[r].folderpath);
                int parentid = folders[r].id;
                int checkspassed = 0;
                if (File.Exists(folders[r].name + ".txt"))
                {
                    string[] linkcheckerlist = File.ReadAllLines(folders[r].name + ".txt");
                    foreach (string link in linkcheckerlist)
                    {
                        string youtubeid = link.Substring(32, 11);
                        bool contains = Directory.EnumerateFiles(Directory.GetCurrentDirectory()).Any(f => f.Contains(youtubeid));
                        if (!contains)
                        {
                            Console.WriteLine(youtubeid + " id in folder: " + folders[r].name + " not found. Probably download unsuccessful.");
                            notfoundbookmarks.Add(new Bookmark(){url = link});
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
                                Console.WriteLine(youtubeid + " id in folder: " + folders[r].name + " not found, despite it being present in archive.txt.");
                            }
                        }
                    }
                    checkspassed++;
                }
                if (checkspassed == 0) { Console.WriteLine("No checks for directory content passed."); }
                Console.WriteLine("Number of missing links in directory");
                folders[r].numberofmissinglinks = 1;
            }
        }
        Console.WriteLine("Total number of missing links: ");
    }

    public static void Dumptoconsole(List<Folderclass> folders, int totalyoutubelinknumber = 0)
    {
        Methods.LogVerbose("Dumptoconsole not given totalyoutubelinknumber", Methods.Verbosity.warning);
        Console.WriteLine("\n\n");
        Console.WriteLine("The following folders were found");
        int depthsymbolcounter = 0;
        Console.Write(string.Concat(Enumerable.Repeat("-", depthsymbolcounter)) + folders[0].depth + " is the depth of " + folders[0].startline + "/" + folders[0].endingline + " "); //writing root folder first
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(folders[0].name);
        Console.ResetColor();
        Console.Write(" root folder, which contains ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(folders[0].numberoflinks);
        Console.ResetColor();
        Console.WriteLine(" youtube links.");
        for (int m = 1; m < folders.Count; m++) //writing the depth, the starting line, the ending line, name, and number of links of all the folders
        {
            if (folders[m].depth > folders[m - 1].depth) //greater depth than before
            {
                depthsymbolcounter = depthsymbolcounter + (folders[m].depth - folders[m - 1].depth);
            }
            if (folders[m].depth < folders[m - 1].depth) //lesser depth than before
            {
                depthsymbolcounter = depthsymbolcounter - (folders[m - 1].depth - folders[m].depth);
            }
            if (folders[m].depth == folders[m - 1].depth) //same depth as before
            { 
                //depthsymbolcounter does not change
            }
            Console.Write(string.Concat(Enumerable.Repeat("-", depthsymbolcounter)) + folders[m].depth + " is the depth of " + folders[m].startline + "/" + folders[m].endingline + " ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(folders[m].name);
            Console.ResetColor();
            Console.Write(" folder, which contains ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(folders[m].numberoflinks);
            Console.ResetColor();
            Console.Write(" youtube links. m:" + m + " id: " + folders[m].id + " parent: " + folders[m].parent);
            if (folders[m].numberofmissinglinks != 0)
            {
                Console.Write(" (missing: ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(folders[m].numberofmissinglinks);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine();
            }
        }
        if (totalyoutubelinknumber != 0)
        {
            Console.WriteLine(totalyoutubelinknumber + " youtube links were found, written into " + folders.Count + " folders.");
        }
        else
        {
            Console.WriteLine("Alltogether " + folders.Count + " folders were written.");
        }
        Console.WriteLine("Waiting for enter to confirm findings");
        Console.ReadKey();
    }

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
                        // throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing.");
                        return null;
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
                if (rootdir != null && File.Exists(Path.Combine(rootdir, filename)))
                {
                    // Console.WriteLine(Path.Combine(rootdir, filename) + " found");
                    ytdlp_path = Path.Combine(rootdir, filename);
                    break;
                }
            }
            if (ytdlp_path == "")
            {
                throw new NotImplementedException();
                
                Console.WriteLine(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.");
                Process process;
                string command = "if (which yt-dlp); then echo \"true\"; else echo \"false\"; fi";
                process = new Process
                {
                    StartInfo = new ProcessStartInfo("bash", command)
                    {
                        WorkingDirectory = rootdir,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine("Result: " + result);
                if (result.Contains("true"))
                {
                    ytdlp_path = "yt-dlp"; //yt-dlp is on the path
                }
                else
                {
                    throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing.");
                }
                Console.WriteLine("ExitCode: {0}", process.ExitCode);
                process.Close();
                //Console.WriteLine("Is it on the path? Y/N");
                //if(Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
                //else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }
                return null;
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
                // TODO
                /*Console.WriteLine(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.");
                Console.WriteLine("Is it on the path? Y/N");
                if (Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
                else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }*/
                return null;
            }
        }
        return ytdlp_path;
    }

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
                Methods.LogVerbose(q + "/" + folders.Count + " folder bat file writing finished.", Methods.Verbosity.trace);
            }
            Methods.LogVerbose(folders.Count + " folder bat file writing finished.", Methods.Verbosity.info);
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
                Methods.LogVerbose(q + "/" + folders.Count + " folder sh file writing finished.", Methods.Verbosity.trace);
            }
            Methods.LogVerbose(folders.Count + " folder sh file writing finished", Methods.Verbosity.info);
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
                Methods.LogVerbose(q + "/" + folders.Count + " folder sh file writing finished.", Methods.Verbosity.trace);
            }
            Methods.LogVerbose(folders.Count + " folder sh file writing finished", Methods.Verbosity.info);
        }
        if (Methods.verbosity >= Methods.Verbosity.info)
        {
            Methods.LogVerbose("Waiting for enter to confirm");
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
        Methods.LogVerbose("Empty folders deleted", Methods.Verbosity.info);
        if (Methods.verbosity >= Methods.Verbosity.info)
        {
            Methods.LogVerbose("Waiting for enter to confirm");
            Console.Read();
        }
    }

    public static void Runningthescripts(List<Folderclass> folders)
    {
        if (Methods.verbosity >= Methods.Verbosity.info)
        {
            Methods.LogVerbose("Running the scripts, press enter to confirm.", Methods.Verbosity.info);
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
                    Console.WriteLine($"cmd.exe /c {command}");
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
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) //maybe it does not work, not tested?
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
                    Console.WriteLine("Error");
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

    public static (bool, bool, bool) Wantcomplex()
    {
        Methods.LogVerbose("Do you want to write and download not video links? (eg. playlists and channels. by default: no)");
        Methods.LogVerbose("Depending on the yt-dlp conf settings this can result in very large downloads, a single bookmark can lead to hundreds of videos being downloaded.");
        Methods.LogVerbose("Y/N");
        bool downloadPlaylists = false;
        bool downloadShorts = false;
        bool downloadChannels = false;
        if (Methods.verbosity >= Methods.Verbosity.info)
        {
            if (Console.ReadKey().ToString().ToLower().Equals("y"))
            {
                Methods.LogVerbose("Playlists? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { downloadPlaylists = true; }
                Methods.LogVerbose("Shorts? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { downloadShorts = true; }
                Methods.LogVerbose("Channels? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { downloadChannels = true; }
            }
        }
        return (downloadPlaylists, downloadShorts, downloadChannels);
    }

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
    




    public static int ValidateCommandLineOptions(CommandLineOptions options)
    {
        if (options.Interactive)
        {
            return 0; //if interactive the options don't matter
        }
        if (options.HtmlFileLocation != null)
        {
            if (!File.Exists(options.HtmlFileLocation)) { return 1; }
            if (Path.GetExtension(options.HtmlFileLocation) != ".html") { return 1; }
        }
        if (options.Yt_dlp_binary_path != null)
        {
            if (!File.Exists(options.Yt_dlp_binary_path)) { return 1; }
        }
        if (options.Outputfolder != null)
        {
            if (!Directory.Exists(options.HtmlFileLocation)) 
            {
                try
                {
                    Directory.CreateDirectory(options.Outputfolder);
                }
                catch (Exception)
                {
                    Methods.LogVerbose("Could not create directory: " + options.Outputfolder, Methods.Verbosity.error);
                    return 1;
                }
            }
            return 0;
        }
        return 0;
    }
}
