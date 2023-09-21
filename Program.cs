using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Microsoft.Data.Sqlite;

namespace bookmark_dlp
{
    class Program
    {
        static void Main()
        {
            ///aim: reformat google chrome bookmars.html from google takeouts and browser bookmark exports
            ///download all the youtube videos listed with yt-dlp
            ///maintain folder structure (download all videos into the folder they were bookmarked in

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            string rootdir = Directory.GetCurrentDirectory(); //current directory
            if (!File.Exists(Path.Combine(rootdir, "Bookmarks.html"))) //If no .html in rootdir tries autoimporting from known browser directories
            {
                Console.WriteLine("No Bookmarks.html found in root directory, proceeding with search in installed browser default locations"); //goig to autoimport, as no .html present
                AutoImport.AutoMain(); //should not return from this method
                throw new Exception($"Finish of route");
                Environment.Exit(1); //leaving the program, so it does not contiue running according to Program.cs
            }
            
            //read .html
            StreamReader reader = new StreamReader("Bookmarks.html"); //read the file containing all the bookmarks - a single file using chrome export
            var lineCount = File.ReadLines("Bookmarks.html").Count(); //how many lines are there in the file - max number of bookmarks
                                                                      //read whole file into inputarray[] array line by line
            string oneline;
            bool wantcomplex = Methods.Wantcomplex();
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
            string[] line = new string[1000]; //limitation: there cannot be a line/bookmark with more than 1000 spaces in it. Probably not relevant?
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
                            //Console.WriteLine(folders[j].name + " " + folders[j].endingline + " " + folders[j].startline + " " + (folders[j].endingline - folders[j].startline));
                            StreamWriter writer = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"), append: false);
                            StreamWriter complexnotsimple = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"), append: true); //writing all the youtube links that are not for videos (but for channels, playlists, etc.) in the given folder
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
                            if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt")).Length == 0) //if the txt reamined empty it is deleted
                            {
                                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"));
                            }
                            if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + ".txt")).Length == 0) //if the txt reamined empty it is deleted
                            {
                                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"));
                                Console.WriteLine("Deleted txt of " + folders[j].name);
                            }
                            if (!wantcomplex)
                            {
                                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"));
                            }
                        }
                    }
                }
                Console.WriteLine("Finished writing depth: " + q);
            }
            temp.Flush();
            temp.Close();

            Methods.Dumptoconsole(folders, numberoffolders, i); //dump all the folder info to console
            string ytdlp_path = Methods.Yt_dlp_pathfinder(rootdir); //check if yt-dlp is in the root folder, on the path or not available
            Methods.Scriptwriter(folders, numberoffolders, ytdlp_path);
            Methods.Deleteemptyfolders(folders, rootdir, numberoffolders, deepestdepth);
            Methods.Runningthescripts(folders, numberoffolders);
            //Methods.Checkformissing(rootdir, folders, numberoffolders); //checking if all the desired links have indeed been downloaded, archive.txt integrity as well
            Methods.Dumptoconsole(folders, numberoffolders, i);
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
        public int numberofmissinglinks;
        public List<string> urls = new List<string>();
    }
}
