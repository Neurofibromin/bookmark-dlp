using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bookmark_dlp;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Microsoft.Data.Sqlite;

internal class Methods
{
    public static Folderclass[] Sqlintake(string filePath)
    {
        ///docs: https://kb.mozillazine.org/Places.sqlite
        ///https://stackoverflow.com/questions/11769524/how-can-i-restore-firefox-bookmark-files-from-sqlite-files
        int[] parentid = new int[File.ReadLines(filePath).Count()]; //parentid[i] = the id of the parent folder of the bookmark with the id i
        List<Bookmark> bookmarks = new List<Bookmark>();
        using (var connection = new SqliteConnection("Data Source=" + filePath))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT moz_places.url, moz_bookmarks.title, moz_bookmarks.id, moz_bookmarks.parent, moz_bookmarks.type, dateAdded, lastModified
                FROM moz_bookmarks left join moz_places on moz_bookmarks.fk = moz_places.id
                WHERE moz_bookmarks.title<>''
                ";

            using (var reader = command.ExecuteReader()) //the order of the variables in SELECT is the same order in which they are returned, coloumn by coloumn
            {
                while (reader.Read())
                {
                    Bookmark thisone = new Bookmark();
                    string type = "";
                    if (!reader.IsDBNull(0)) //only try to convert the coloumn value to string with getstring if it is not null - otherwise error
                    {
                        thisone.url = reader.GetString(0);
                    }
                    thisone.name = reader.GetString(1);
                    thisone.id = Convert.ToInt16(reader.GetString(2));
                    if (!reader.IsDBNull(3))
                    {
                        parentid[thisone.id] = Convert.ToInt32(reader.GetString(3));
                    }
                    if (!reader.IsDBNull(4))
                    {
                        type = reader.GetString(4);
                    }
                    if (!reader.IsDBNull(5))
                    {
                        thisone.date_added = Convert.ToInt64(reader.GetString(5));
                    }
                    if (!reader.IsDBNull(6))
                    {
                        thisone.date_modified = Convert.ToInt64(reader.GetString(6));
                    }
                    if (type.Contains("2"))
                    {
                        thisone.type = "folder";
                        //Console.WriteLine("url:" + thisone.url + " name:" + thisone.name + " pid:" + parentid[thisone.id] + " id:" + thisone.id + " type:" + thisone.type + "|" + thisone.date_added + "|" + thisone.date_modified);
                    }
                    else if (type.Contains("1"))
                    {
                        thisone.type = "url";
                    }
                    else
                    {
                        Console.WriteLine("Error: this bookmark is not of type folder or url - undefined");
                    }
                    bookmarks.Add(thisone);
                }
            }
        }
        //sqlite3 places.sqlite "select '<a href=''' || url || '''>' || moz_bookmarks.title || '</a><br/>' as ahref from moz_bookmarks left join moz_places on fk=moz_places.id where url<>'' and moz_bookmarks.title<>''" > t1.html

        //trying to place the data from the Bookmark object into a Folderclass[] object
        //in the sql only parent ids are given, not children, so the process has to be reversed compared to the json
        foreach (Bookmark bookmark in bookmarks.ToList<Bookmark>()) //must use tolist<> to avoid "Collection was modified; enumeration operation may not execute" when removing item from bookmarks
        {
            if (bookmark.type == "url") //urls have no children, it is safe to add them to their parent folders (even if they are not at the deepest depth
            {
                bookmarks.Single(a => a.id == parentid[bookmark.id]).children.Add(bookmark); //bookmark added to their parent's .children list
                bookmarks.Remove(bookmark); //bookmark is removed from the sql_list (as it is already in its parent's list
            }
            //only folders remain in the sql_list
        }
        Console.WriteLine("DEV Finished foreach");
        Folderclass[] folders = new Folderclass[bookmarks.Count]; //declare and initialise the folders[]
        for (int q = 0; q < bookmarks.Count; q++)
        {
            folders[q] = new Folderclass();
        }
        //now AutoImport.Globals.sql_Bookmarks contains all the Bookmark objects for every folder.
        //These should now be united into one Bookmarkroot by adding them as each other's children from deepest depth upwards.
        //but instead they are just converted into folderclasses - this is also fine
        int folderid = 1;
        foreach (Bookmark bookmark in bookmarks)
        {
            int i = bookmark.id;
            //i refers to the id (from the sql) of the folder that is being examined. folderid will be its new id, so every folderid refers to folders and there is no gap between them:
            //examples:
            //folder toolbars id: 2 folderid: 1
            //folder a id: 7 folderid: 2
            //folder b id: 15 folderid: 3
            //folder c id: 43175 folderid: 4
            //the difference is large because id was also given in the sql db for url bookmarks, while now only folder bookmarks are examined, so large gaps are expected
            Console.WriteLine("1");
            Console.WriteLine("parentid: " + parentid[i]);
            int index = Array.FindIndex(folders, a => a.startline == parentid[i]);
            Console.WriteLine("Index: " + index);
            Console.WriteLine("Name: " + folders.SingleOrDefault(a => a.startline == parentid[i]).name);
            if (folders.SingleOrDefault(a => a.startline == parentid[i]).depth == 0) //if a folder's parent does not have a depth value, the folder probably does not have a parent - as such its depth should be 0
            { //i - id of the examined folder, parentid[i] - id of the examined folder's parent : we are looking for the folder[] that has this parentid[i] as its startingline (refers to the original id), so "folders.SingleOrDefault(a => a.startline == parentid[i])" refers to the parent of the examined folder
                folders[folderid].depth = 1;
            }
            else
            {
                folders[folderid].depth = folders.SingleOrDefault(a => a.startline == parentid[i]).depth + 1; //the given folders depth is the depth of their parent folder + 1
            }
            Console.WriteLine("2");
            //maybe should discard the above condition? it parent does not exist its 0 anyway, so the value would be set 0+1=1

            //old lookup using for loop instead of foreach: folders[i].name = bookmarks.SingleOrDefault(a => a.id == i).name;
            folders[folderid].name = bookmark.name;
            folders[folderid].startline = bookmark.id;
            folders[folderid].numberoflinks = bookmark.children.Count();
            Console.WriteLine("Name: {0} ID: {1} Numberoflinks: {2} Depth: {3}", folders[folderid].name, folders[folderid].startline, folders[folderid].numberoflinks, folders[folderid].depth);
            foreach (Bookmark urlbookmark in bookmark.children)
            {
                //adding the url of each child to the url list of their parent
                folders[folderid].urls.Add(urlbookmark.url);
            }
        }
        return folders;
    }

    public static void Checkformissing(string rootdir, Folderclass[] folders, int numberoffolders)
    {
        ///Check whether the youtube links that were on the list are now dowloaded into the appripriate directories. Good for finding rotten links.
        ///Only checks the filenames for the yt-id (11 characters): if yt-dlp config is set to not include such id in the filename it will not work.
        ///yt-dlp logs could also be parsed for the same info, although if a video was downloaded in the past and no longer available on the net, it would still be flagged (?) - depends on the archive.txt usage setting
        StreamWriter failedlinks = new StreamWriter(rootdir + "allfailed.txt");
        int allmissinglinksnumber = 0;
        for (int r = 0; r < numberoffolders; r++)
        {
            if (Directory.Exists(folders[r].folderpath))
            {
                Directory.SetCurrentDirectory(folders[r].folderpath);
                StreamWriter failedinthisfolder = new StreamWriter("failedhere.txt");
                int checkspassed = 0;
                int thisdirmissinglinknumber = 0;
                if (File.Exists(folders[r].name + ".txt"))
                {
                    StreamReader reader = new StreamReader(folders[r].name + ".txt");
                    string[] linkcheckerlist = new string[folders[r].numberoflinks];
                    for (int n = 0; n < folders[r].numberoflinks; n++)
                    {
                        linkcheckerlist[n] = reader.ReadLine();
                    }
                    foreach (string link in linkcheckerlist)
                    {
                        string youtubeid = link.Substring(32, 11);
                        bool contains = Directory.EnumerateFiles(Directory.GetCurrentDirectory()).Any(f => f.Contains(youtubeid));
                        if (!contains)
                        {
                            Console.WriteLine(youtubeid + " id in folder: " + folders[r].name + " not found. Probably download unsuccessful.");
                            failedlinks.WriteLine(youtubeid);
                            failedinthisfolder.WriteLine(youtubeid);
                            allmissinglinksnumber++;
                            thisdirmissinglinknumber++;
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
                                failedlinks.WriteLine(youtubeid);
                                failedinthisfolder.WriteLine(youtubeid);
                                allmissinglinksnumber++;
                                thisdirmissinglinknumber++;
                            }
                        }
                    }
                    checkspassed++;
                }
                if (checkspassed == 0) { Console.WriteLine("No checks for directory content passed."); }
                Console.WriteLine("Number of missing links in directory {0}: {1}", folders[r].name, thisdirmissinglinknumber);
                failedinthisfolder.Flush();
                failedinthisfolder.Close();
                folders[r].numberofmissinglinks = thisdirmissinglinknumber;
            }
        }
        failedlinks.Flush();
        failedlinks.Close();
        Console.WriteLine("Total number of missing links: " + allmissinglinksnumber);
    }

    public static void Dumptoconsole(Folderclass[] folders, int numberoffolders = 0, int totalyoutubelinknumber = 0)
    {
        if (numberoffolders == 0)
        {
            numberoffolders = folders.Count();
        }
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
        Console.WriteLine(" youtube links." + "0");
        for (int m = 1; m < numberoffolders + 1; m++) //writing the depth, the starting line, the ending line, name, and number of links of all the folders
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
            Console.Write(" youtube links." + m);
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
            Console.WriteLine(totalyoutubelinknumber + " youtube links were found, written into " + numberoffolders + " folders.");
        }
        else
        {
            Console.WriteLine("Alltogether " + numberoffolders + " folders were written.");
        }
        Console.WriteLine("Waiting for enter to confirm findings");
        Console.ReadKey();
    }

    public static string Yt_dlp_pathfinder(string rootdir)
    {
        string ytdlp_path = ""; //checks is yt-dlp binary is present in root or if it is on path, returns ytdlp_path so it can be written into the script files
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (File.Exists(Path.Combine(rootdir, "yt-dlp.exe")))
            {
                Console.WriteLine(Path.Combine(rootdir, "yt-dlp.exe") + " found");
                ytdlp_path = Path.Combine(rootdir, "yt-dlp.exe");
            }
            else
            {
                Console.WriteLine(Path.Combine(rootdir, "yt-dlp.exe") + " not found, searching PATH.");
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
                        throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing.");
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
            string[] filenames = {"yt-dlp", "yt-dlp_linux", "yt-dlp_linux_aarch64", "yt-dlp_linux_armv7l" };
            foreach (string filename in filenames)
            {
                if (File.Exists(Path.Combine(rootdir, filename)))
                {
                    Console.WriteLine(Path.Combine(rootdir, filename) + " found");
                    ytdlp_path = Path.Combine(rootdir, filename);
                }
            }
            if (ytdlp_path == "")
            {
                Console.WriteLine(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.");
                /*Process process;
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
                process.Close();*/
                Console.WriteLine("Is it on the path? Y/N");
                if(Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
                else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }
            }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string[] filenames = { "yt-dlp", "yt-dlp_macos", "yt-dlp_macos_legacy" };
            foreach (string filename in filenames)
            {
                if (File.Exists(Path.Combine(rootdir, filename)))
                {
                    Console.WriteLine(Path.Combine(rootdir, filename) + " found");
                    ytdlp_path = Path.Combine(rootdir, filename);
                }
            }
            if (ytdlp_path == "")
            {
                Console.WriteLine(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.");
                Console.WriteLine("Is it on the path? Y/N");
                if (Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
                else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }
            }
        }
        return ytdlp_path;
    }

    public static void Scriptwriter(Folderclass[] folders, int numberoffolders, string ytdlp_path)
    {
        string extensionforscript = ""; //writing scripts
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            extensionforscript = ".bat";
            for (int q = 0; q < numberoffolders + 1; q++) //writing bat files for every folder
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
                //Console.WriteLine(q + "/" + numberoffolders + " folder bat file writing finished.");
            }
            Console.WriteLine(numberoffolders + " folder bat file writing finished.");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            extensionforscript = ".sh";
            for (int q = 0; q < numberoffolders + 1; q++) //writing sh files for every folder
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
                //Console.WriteLine(q + "/" + numberoffolders + " folder sh file writing finished");
            }
            Console.WriteLine(numberoffolders + " folder sh file writing finished");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            extensionforscript = ".sh";
            for (int q = 0; q < numberoffolders + 1; q++) //writing sh files for every folder
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
            }
            Console.WriteLine(numberoffolders + " folder sh file writing finished");
        }
        Console.WriteLine("Waiting for enter to confirm");
        Console.Read();
    }

    public static void Deleteemptyfolders(Folderclass[] folders, string rootdir, int numberoffolders, int deepestdepth)
    {
        //Deleting empty folders
        Directory.SetCurrentDirectory(rootdir);
        for (int q = deepestdepth; q > 0; q--) //deleting empty folders from the deepest layer upwards
        {
            for (int j = 0; j < numberoffolders; j++)
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
        Console.WriteLine("Empty folders deleted");
        Console.WriteLine("Waiting for enter to confirm");
        Console.Read();
    }

    public static void Runningthescripts(Folderclass[] folders, int numberoffolders)
    {
        Console.WriteLine("Running the scripts");
        Console.Read();
        string extensionforscript = ""; //writing scripts
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { extensionforscript = ".bat"; }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { extensionforscript = ".sh"; }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { extensionforscript = ".sh"; }

        //running the script files one after another, in the order of folders[].startline
        for (int j = 0; j < numberoffolders; j++)
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
                        Console.WriteLine("FILE " + downloadserialnumber + " / " + folders[j].numberoflinks + "---------------------------" + "Folder: " + folders[j].name + "(" + j + "out of " + numberoffolders + ")");
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
            Console.Write((j + 1) + "/" + numberoffolders);
            Console.ResetColor();
            Console.Write(" folders are finished\n");
        }
    }

    public static bool Wantcomplex()
    {
        Console.WriteLine("Do you want to write and download not video links? (eg. playlists and channels. by default: no)");
        Console.WriteLine("Depending on the yt-dlp conf settings this can result in very large downloads, a single bookmark can lead to hundreds of videos being downloaded.");
        Console.WriteLine("Y/N");
        bool wantcomplex = false;
        if (Console.ReadKey().Equals('y'))
        {
            wantcomplex = true;
            Console.WriteLine("You chose to write all youtube links.");
        }
        return wantcomplex;
    }
}