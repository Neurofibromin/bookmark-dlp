using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MintPlayer.PlatformBrowser;
using System.ComponentModel;

namespace bookmark_extract_youtube_links
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //aim: reformat google chrome bookmars.html from google takeouts and browser bookmark exports
            //download all the youtube videos listed with yt-dlp
            //maintain folder structure (download all videos into the folder they were bookmarked in

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var InstalledBrowsers = new List<string>();
                InstalledBrowsers = AutoImport.getinstalledbrowsers();
                Console.WriteLine(InstalledBrowsers);
            }

            //read .html
            
            string rootdir = Directory.GetCurrentDirectory(); //current directory
            if (File.Exists(Path.Combine(rootdir, "Bookmarks.html"))){ }
            else
            {
                Console.WriteLine("No Bookmarks.html found in root directory, proceeding with search in installed browser default locations");
                List<Folderclass> foldersfrombrowsers = AutoImport.getAppdataFolders();
                foreach (Folderclass folderss in foldersfrombrowsers)
                {
                    Console.WriteLine(folderss.name + " name linknumber: " + folderss.numberoflinks);
                }
            }

            StreamReader reader = new StreamReader("Bookmarks.html"); //read the file containing all the bookmarks - a single file using chrome export
            var lineCount = File.ReadLines("Bookmarks.html").Count(); //how many lines are there in the file - max number of bookmarks
                                                                      //read whole file into inputarray[] array line by line
            string oneline;
            oneline = reader.ReadLine();
            string[] inputarray = new string[lineCount + 100];
            int i = 1;
            while (i <= lineCount)
            {
                inputarray[i] = oneline;
                i++;
                oneline = reader.ReadLine();
            }
            Console.WriteLine(i - 1 + "/" + lineCount + " lines were read.");
            Console.WriteLine("The intake has finished!");



            //Creating the folders[] object array and initialize all its elements, notice that the max number of folders equals the number of lines
            Folderclass[] folders = new Folderclass[lineCount];
            for (int q = 0; q < lineCount; q++)
            {
                folders[q] = new Folderclass();
            }

            //Finding all the lines starting with dt h3 (these lines start every folder) and adding the number of these lines (j) to the object array folders[].startline
            //the folders[].startline gives us the number of the first line of the given folder in the inputarray[] array (in is like the endingline in the next loop, just for the start)
            //This also gives us the number of folders (numberoffolders)
            string[] line = new string[1000];
            int numberoffolders = 0;
            for (int j = 1; j < lineCount; j++)
            {
                line = inputarray[j].Trim().Split(' ');
                if (line[0].Trim() == "<DT><H3")
                {
                    numberoffolders++;
                    folders[numberoffolders].startline = j;

                }
            }
            Console.WriteLine(numberoffolders + " folders were found in the bookmarks");

            //Finding the end of the folders (</DL><p>) and adding the line number to the object array (folders[].endingline)
            //Counting the lines from the start while the folders from the back, so even in folders embedded into folders the endingline will be correct
            for (int j = 1; j < lineCount; j++)
            {
                oneline = inputarray[j].Trim();
                if (oneline == "</DL><p>") //if we find a line that ends a folder
                {
                    for (int m = numberoffolders; m > 0; m--)
                    {
                        if (folders[m].startline < j && folders[m].endingline == 0) //finding the last folder that has a starting line earlier than this endingline, and has not yet been closed
                        {
                            folders[m].endingline = j;
                            break;
                            //break is necessary, because in embedded folders not only the correct folder's startline would be found correct, but all the not-yet closed folders that are already open: all their parent folders
                            //the break prevents parent folders getting the same endingline as their children
                        }
                    }
                }
            }

            //Finding the folder names and adding them to the object array (folders[].name)
            int whereisthechar;
            for (int m = 1; m < numberoffolders + 1; m++)
            {
                line = inputarray[folders[m].startline].Trim().Split('>');
                whereisthechar = line[line.Length - 2].IndexOf("<");
                folders[m].name = line[line.Length - 2].Substring(0, whereisthechar);
                //Console.WriteLine(line[line.Length-2].Substring(0,whereisthechar));
                //Console.WriteLine(folders[m].startline + " " + folders[m].name);
                //Console.WriteLine(folders[m] + " line " + whereisthechar + " " + line[line.Length - 2]);
            }

            //Finding the folder depths (how embedded they are) and adding them to the object array (folders[].depth)
            for (int m = 1; m < numberoffolders + 1; m++)
            {
                line = inputarray[folders[m].startline].Split('<');
                folders[m].depth = line[0].Length / 8;
            }
            Console.WriteLine("\n\n");

            //creating the folder structure and storing the access paths to the folders[].folderpath object array
            System.IO.Directory.CreateDirectory("Bookmarks");
            Directory.SetCurrentDirectory("Bookmarks");
            for (int m = 1; m < numberoffolders + 1; m++)
            {
                if (m > 1)
                {

                    if (folders[m].depth > folders[m - 1].depth) //more depth than previous folder
                    {
                        Directory.SetCurrentDirectory(folders[m - 1].name);
                        System.IO.Directory.CreateDirectory(folders[m].name);
                        Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                        folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                        Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                    }

                    if (folders[m].depth < folders[m - 1].depth) //less depth than the previous folder
                    {
                        for (int q = 0; q < (folders[m - 1].depth - folders[m].depth); q++) //the depth may have decreased by more than 1
                        {
                            Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), ".."));
                        }
                        System.IO.Directory.CreateDirectory(folders[m].name);
                        Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                        folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                        Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                    }

                    if (folders[m].depth == folders[m - 1].depth) //the same depth as the previous folder
                    {
                        System.IO.Directory.CreateDirectory(folders[m].name);
                        Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                        folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                        Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                    }

                }
                else //it is the first folder
                {
                    System.IO.Directory.CreateDirectory(folders[m].name);
                    folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                }
            }

            int deepestdepth = 0; //Finding the deepest folder depth
            for (int q = 1; q < numberoffolders + 1; q++)
            {
                if (deepestdepth < folders[q].depth)
                {
                    deepestdepth = folders[q].depth;
                }
            }

            StreamWriter temp = new StreamWriter(Path.Combine(rootdir, "temp.txt"), append: true); //writing into temp.txt all the youtube links that are not for videos (but for channels, playlists, etc.)

            i = 0; //writing the content of the deepest folders first, and deleting the lines from inputarray[] that were written
            for (int q = deepestdepth; q > 0; q--) //writing the content of the deepest folders first, and deleting the lines from inputarray[] that were written
            {
                for (int j = 0; j < numberoffolders; j++) //going through all the folders
                {
                    if (folders[j].depth == q) //choosing only folders with the same depth: they cannot overlap with each other
                    {
                        if (folders[j].endingline - folders[j].startline > 2)
                        {
                            //google side bug of duplicating all folders and bookmarks, resulting in 3 line long empty folders as well as not empty folders, which contain two copies of every bookmark.
                            //shouldn't have too much of an effect on the end results,
                            //just divide most numbers by 2. yt-dlp already uses archive.txt, so only lookup time is wasted, not downloads
                            Console.WriteLine(folders[j].name + " " + folders[j].endingline + " " + folders[j].startline + " " + (folders[j].endingline - folders[j].startline));
                            StreamWriter writer = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"), append: false);
                            StreamWriter complexnotsimple = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + "complexnotsimple.txt"), append: true); //writing all the youtube links that are not for videos (but for channels, playlists, etc.) in the given folder
                            int linknumbercounter = 0;
                            for (int qq = folders[j].startline; qq < folders[j].endingline + 1; qq++) //going through all the lines that are in the given folder
                            {
                                if (inputarray[qq] != null)
                                {
                                    line = inputarray[qq].Trim().Split(' ');
                                    if (line[0].Trim() == "<DT><A")
                                    {
                                        if (line[1].Contains("www.youtube.com")) //only write lines that are youtube links
                                        {
                                            string linkthatisbeingexamined = line[1].Trim().Substring(6, line[1].Trim().Length - 7);
                                            bool iscomplicated = false;
                                            if (linkthatisbeingexamined.Substring(24, 8) == "playlist") //filtering the links with the consecutive ifs to find if they are for videos or else (channels, playlists, etc.)
                                            {
                                                complexnotsimple.WriteLine(linkthatisbeingexamined);
                                                temp.WriteLine(linkthatisbeingexamined);
                                                iscomplicated = true;
                                            }
                                            if (linkthatisbeingexamined.Substring(24, 4) == "user")
                                            {
                                                complexnotsimple.WriteLine(linkthatisbeingexamined);
                                                temp.WriteLine(linkthatisbeingexamined);
                                                iscomplicated = true;
                                            }
                                            if (linkthatisbeingexamined.Substring(24, 7) == "channel")
                                            {
                                                complexnotsimple.WriteLine(linkthatisbeingexamined);
                                                temp.WriteLine(linkthatisbeingexamined);
                                                iscomplicated = true;
                                            }
                                            if (linkthatisbeingexamined.Substring(24, 7) == "results") //youtube search result was bookmarked
                                            {
                                                //not saving search results to complexnotsimple
                                                temp.WriteLine(linkthatisbeingexamined);
                                                iscomplicated = true;
                                            }
                                            if (linkthatisbeingexamined.Substring(24, 1) == "@")
                                            {
                                                complexnotsimple.WriteLine(linkthatisbeingexamined);
                                                temp.WriteLine(linkthatisbeingexamined);
                                                iscomplicated = true;
                                            }
                                            if (linkthatisbeingexamined.Substring(24, 2) == "c/")
                                            {
                                                complexnotsimple.WriteLine(linkthatisbeingexamined);
                                                temp.WriteLine(linkthatisbeingexamined);
                                                iscomplicated = true;
                                            }
                                            if (iscomplicated == false)
                                            {
                                                writer.WriteLine(linkthatisbeingexamined);
                                            }
                                            i++;
                                            linknumbercounter++;
                                        }
                                        line = inputarray[qq].Trim().Split('>');
                                        //writer.WriteLine(line[2].Substring(0,line[2].Length-3)); //writes the name of the bookmark //to write into same line use writer.Write()
                                        inputarray[qq] = null;
                                    }
                                    else
                                    {
                                        if (folders[j].startline != qq && folders[j].endingline != qq) //in this line there is no link (eg. its not a bookmark, but juts folder ending line)
                                        {
                                            //Console.WriteLine("no hit: " + qq);
                                        }
                                    }
                                }
                            }
                            writer.Flush();
                            writer.Close();
                            complexnotsimple.Flush();
                            complexnotsimple.Close();
                            folders[j].numberoflinks = linknumbercounter; //gives count of how many youtube links were found - also contains complex links (not videos, but channels, playlists, etc.)
                            if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + "complexnotsimple.txt")).Length == 0) //if the txt reamined empty it is deleted
                            {
                                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + "complexnotsimple.txt"));
                            }
                            if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + ".txt")).Length == 0) //if the txt reamined empty it is deleted
                            {
                                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"));
                                Console.WriteLine("Deleted txt of " + folders[j].name);
                            }
                        }
                    }
                }
                Console.WriteLine("Finished writing depth: " + q);
            }
            temp.Flush();
            temp.Close();

            Console.WriteLine("\n\n");
            Console.WriteLine("The following folders were found");
            int depthsymbolcounter = 0; //dump all the folder info to console
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
                Console.WriteLine(" youtube links." + m);
            }
            Console.WriteLine(i + " youtube links were found, written into " + numberoffolders + " folders.");
            Console.WriteLine("Waiting for enter to confirm findings");
            Console.ReadKey();

            //check if yt-dlp is in the root folder, on the path or not available
            string ytdlp_path = "";
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
                }
            }
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
                    Process process;
                    string command = "if (which grep); then echo \"true\"; else echo \"false\"; fi";
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
                    process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        Console.WriteLine("output :: " + e.Data);
                        if (e.Data != null && e.Data.Contains("true"))
                        {
                            ytdlp_path = "yt-dlp"; //yt-dlp is on the path
                        }
                        else
                        {
                            throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing.");
                        }
                    });
                    process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => Console.WriteLine("error :: " + e.Data);
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    Console.WriteLine("ExitCode: {0}", process.ExitCode);
                    process.Close();
                    ytdlp_path = "yt-dlp.exe";
                }
            }

            string extensionforscript = ""; //writing scripts
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                extensionforscript = ".bat";
                for (int q = 1; q < numberoffolders + 1; q++) //writing bat files for every folder
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
                for (int q = 1; q < numberoffolders + 1; q++) //writing sh files for every folder
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
                Console.WriteLine("MacOS not supported");
                System.Environment.Exit(1);
            }

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
            /*
            Console.WriteLine("Running the scripts");
            Console.Read();
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
                        File.AppendAllText(Path.Combine(folders[j].folderpath, "log.txt"), e.Data + Environment.NewLine);
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

            */




            Console.WriteLine("Press enter to exit");
            Console.Read();
        }


    }
    public class Folderclass //defining the folderclass class to create an object array from it
    {
        public int startline;
        public string name;
        public int depth;
        public int endingline;
        public string folderpath;
        public int numberoflinks;
        public List<string> urls;
    }
}
