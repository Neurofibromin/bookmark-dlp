﻿using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bookmark_dlp;
using Microsoft.Data.Sqlite;

internal class AutoImport
{
    /// <summary>
    /// todo:
    /// 
    /// handle complexnotsimple and temp streamwriters better
    /// the linknumbercounters are increased for complex links as well
    /// 
    /// handle youtube shorts in both download and check
    /// 
    /// check if folders.linknumber refers to all links or youtube links in autoimport (to all links probably, as Writelinkstotxt is not returning the changed values
    /// 
    /// there is some bug with checkformissing, out of range maybe?
    /// allfailed.txt wrong place and name
    /// create archive.shouldbe.txt and compare to archive that way?
    /// 
    /// chrome export bookmarks uses different layout html than google takeout
    ///
    /// writing something to a log file? maybe try to write everything to dated log files?
    /// 
    /// check if folders/files already existing is a problem
    /// 
    /// find mac firefox path
    /// 
    /// check browser paths for flatpaks
    /// 
    /// add safari and chromium and opera and vivaldi support
    /// 
    /// </summary>


    public static void AutoMain()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string rootdir = Directory.GetCurrentDirectory(); //current directory
        bool wantcomplex = Methods.Wantcomplex();
        string filePath = FindfilePath();
        Folderclass[] folders;
        if (filePath.Contains("sqlite"))
        {
            folders = Methods.Sqlintake(filePath);
            Environment.Exit(1);
        }
        else
        {
            folders = Intake(filePath);
        }
        int numberoffolders = Globals.folderid;
        folders = Createfolderstructure(folders, rootdir); //because the folders[].folderpath is changed, the whole structure must be returned (or made global).

        int deepestdepth = 0; //Finding the deepest folder depth
        for (int q = 1; q < numberoffolders + 1; q++)
        {
            if (deepestdepth < folders[q].depth)
            {
                deepestdepth = folders[q].depth;
            }
        }
        Writelinkstotxt(folders, numberoffolders, rootdir, wantcomplex);
        Methods.Dumptoconsole(folders, numberoffolders, Globals.totalyoutubelinknumber);
        string ytdlp_path = Methods.Yt_dlp_pathfinder(rootdir); //finding path to yt-dlp binary
        Methods.Scriptwriter(folders, numberoffolders, ytdlp_path); //writing the scripts that call yt-dlp and add .txt with the links in the arguments //NOT the method that creates the .txt files
        Methods.Deleteemptyfolders(folders, rootdir, numberoffolders, deepestdepth); //deletes the folders from the folder structure that are empty (no youtube links were written into them)
        Console.WriteLine("Running the scripts after ENTER.");
        Console.ReadKey();
        Methods.Runningthescripts(folders, numberoffolders); //runs the script that calls yt-dlp: downloads all the videos
        Methods.Checkformissing(rootdir, folders, numberoffolders);
        Methods.Dumptoconsole(folders, numberoffolders, Globals.totalyoutubelinknumber);
        System.Environment.Exit(1); //leaving the program, so it does not contiue running according to Program.cs
    }

    public static void Writelinkstotxt(Folderclass[] folders, int numberoffolders, string rootdir, bool wantcomplex)
    {
        StreamWriter temp = new StreamWriter(Path.Combine(rootdir, "temp.txt"), append: true); //writing into temp.txt all the youtube links that are not for videos (but for channels, playlists, etc.)
        int i = 0;
        for (int j = 0; j < numberoffolders + 1; j++)
        {
            StreamWriter writer = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"), append: false);
            StreamWriter complexnotsimple = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"), append: true); //writing all the youtube links that are not for videos (but for channels, playlists, etc.) in the given folder
            int linknumbercounter = 0;
            foreach (string url in folders[j].urls)
            {
                string linkthatisbeingexamined = url;
                if (linkthatisbeingexamined.Contains("www.youtube.com")) //only write lines that are youtube links
                {
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
        temp.Flush();
        temp.Close();
        Globals.totalyoutubelinknumber = i;
        Console.WriteLine("Total number of youtube links found: " + i);
    }

    public static Folderclass[] Createfolderstructure(Folderclass[] folders, string rootdir)
    {
        //creating the folder structure and storing the access paths to the folders[].folderpath object array
        //because the folders[].folderpath is changed, the whole structure must be returned (or made global).
        Directory.SetCurrentDirectory(rootdir);
        for (int m = 0; m < Globals.folderid + 1; m++)
        {
            if (m > 0)
            {

                if (folders[m].depth > folders[m - 1].depth) //more depth than previous folder
                {
                    Directory.SetCurrentDirectory(folders[m - 1].name);
                    Directory.CreateDirectory(folders[m].name);
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
                    Directory.CreateDirectory(folders[m].name);
                    Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                    folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                    Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                }

                if (folders[m].depth == folders[m - 1].depth) //the same depth as the previous folder
                {
                    Directory.CreateDirectory(folders[m].name);
                    Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                    folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                    Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                }

            }
            else //it is the first folder
            {
                System.IO.Directory.CreateDirectory(folders[m].name);
                Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
            }
        }
        return folders;
    }

    public static string FindfilePath()
    {
        string filePath = "";
        string profilespath = "";
        List<string> filepaths = new List<string>();

        //use default location for bookmarks file
        //Chrome
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //string windir = Environment.SystemDirectory; // C:\windows\system32
            //string windrive = Path.GetPathRoot(Environment.SystemDirectory); // C:\
            //filePath = windrive + "\\Users\\" + Environment.UserName + "\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Bookmarks";
            /*filePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data\\Default\\Bookmarks");
            if (File.Exists(filePath)) { 
                Console.WriteLine("File found! " + "Filepath in chrome: " + filePath);
                filepaths.Add(filePath);
            }
            else { Console.WriteLine(($"Bookmarks file not found at " + filePath)); } */
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data\\");
            //Console.WriteLine("appdata: " + System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            //Console.WriteLine("profilespath: " + profilespath);
            int n = 0;
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    //Console.WriteLine("profile: " + profile);
                    if (File.Exists(Path.Combine(profile, "Bookmarks")))
                    {
                        //For every chrome profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        Console.WriteLine("File found! " + "Filepath in Chrome: " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        n++;
                    }
                }
                if (n == 0)
                {
                    Console.WriteLine(($"No Bookmarks file found in Chrome"));
                }
            }
            else
            {
                Console.WriteLine("Chrome install folder not found");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            /*filePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome/Default/Bookmarks");
            if (File.Exists(filePath))
            {
                Console.WriteLine("File found! " + "Filepath in chrome: " + filePath);
                filepaths.Add(filePath);
            }
            else { Console.WriteLine(($"Bookmarks file not found at " + filePath)); }*/
            int n = 0;
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome");
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "Bookmarks")))
                    {
                        //For every chrome profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        Console.WriteLine("File found! " + "Filepath in Chrome: " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        n++;
                    }
                }
                if (n == 0) { Console.WriteLine(($"Bookmarks file not found in Chrome")); }
            }
            else
            {
                Console.WriteLine("Chrome install folder not found");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ///Users/$User/Library/Application Support/Google/Chrome/Default
            int n = 0;
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome");
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "Bookmarks")))
                    {
                        //For every chrome profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        Console.WriteLine("File found! " + "Filepath in Chrome: " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        n++;
                    }
                }
                if (n == 0) { Console.WriteLine(($"Bookmarks file not found in Chrome")); }
            }
            else
            {
                Console.WriteLine("Chrome install folder not found");
            }
        }

        //Brave Browser
        //%localappdata%\BraveSoftware\Brave-Browser\User Data\Default\
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware\\Brave-Browser\\User Data\\");
            int n = 0;
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "Bookmarks")))
                    {
                        //For every Brave profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        Console.WriteLine("File found! " + "Filepath in Brave: " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        n++;
                    }
                }
                if (n == 0)
                {
                    Console.WriteLine(($"No Bookmarks file found in Brave browser"));
                }
            }
            else
            {
                Console.WriteLine("Brave-browser install folder not found");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //$User/.config/BraveSoftware/Brave-Browser/Default/Bookmarks.
            int n = 0;
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BraveSoftware/Brave-Browser/");
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "Bookmarks")))
                    {
                        //For every chrome profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        Console.WriteLine("File found! " + "Filepath in Brave: " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        n++;
                    }
                }
                if (n == 0) { Console.WriteLine(($"Bookmarks file not found in Brave")); }
            }
            else
            {
                Console.WriteLine("Brave-browser install folder not found");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ///$HOME/Library/Application Support/BraveSoftware/Brave-Browser/Default/Bookmarks
            int n = 0;
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/BraveSoftware/Brave-Browser");
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "Bookmarks")))
                    {
                        //For every chrome profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        Console.WriteLine("File found! " + "Filepath in Brave: " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        n++;
                    }
                }
                if (n == 0) { Console.WriteLine(($"Bookmarks file not found in Brave")); }
            }
            else
            {
                Console.WriteLine("Brave-browser install folder not found");
            }
        }

        //Firefox
        //Finding the sqlite databases
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ///C:\Windows.old\Users\<UserName>\AppData\Roaming\Mozilla\Firefox\Profiles\<filename.default>\places.sqlite
            //filePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\<filename.default>\\places.sqlite");
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\");
            int n = 0;
            //Console.WriteLine("profilespath " + profilespath);
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "places.sqlite")))
                    {
                        //For every firefox profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        Console.WriteLine("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        n++;
                    }
                }
            }
            else
            {
                Console.WriteLine("Firefox install folder not found");
            }
            if (n == 0) { Console.WriteLine(($"Bookmarks file not found in Firefox")); }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            ///home/$User/snap/firefox/common/.mozilla/firefox/aaaa.default/places.sqlite/
            ///home/$User/.mozilla/firefox/aaaa.default/places.sqlite
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "snap/firefox/common/.mozilla/firefox/");
            int n = 0;
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "places.sqlite")))
                    {
                        //For every firefox profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        Console.WriteLine("File found! " + "Filepath in snap firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        n++;
                    }
                }
            }
            else
            {
                Console.WriteLine("Firefox snap install not found");
            }
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".mozilla/firefox");
            if (Directory.Exists(profilespath))
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "places.sqlite")))
                    {
                        //For every firefox profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        Console.WriteLine("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        n++;
                    }
                }
            }
            else
            {
                Console.WriteLine("Firefox native install not found");
            }
            if (n == 0) { Console.WriteLine(($"Bookmarks file not found in Firefox")); }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            //#####################################################################################################################################################################################################################
            /*profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".mozilla/firefox");
            if (Directory.Exists(profilespath)
            {
                foreach (string profile in Directory.GetDirectories(profilespath))
                {
                    if (File.Exists(Path.Combine(profile, "places.sqlite")))
                    {
                        //For every firefox profile that has bookmarks
                        filepaths.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        Console.WriteLine("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        n++;
                    }
                }
            }
            else
            {
                Console.WriteLine("Firefox install folder not found.");
            }
            if (n == 0) { Console.WriteLine(($"Bookmarks file not found in Firefox")); }*/
            Console.WriteLine("WIP");
        }

        if (filepaths.Count == 0)
        {
            throw new FileNotFoundException($"No bookmarks file was found by autoimport in any of the locations");
        }
        Console.WriteLine("Which browser bookmarks would you like to use?\nWrite the number");
        int m = 0;
        foreach (string path in filepaths)
        {
            Console.WriteLine(m + ". path: " + path);
            m++;
        }
        filePath = filepaths.ElementAt(Convert.ToInt32(Console.ReadLine()));
        Console.WriteLine("Chosen path: " + filePath);
        return filePath;
    }

    public static Folderclass[] Intake(string filePath)
    {
        Console.WriteLine("Autoimport intake start");
        string text = File.ReadAllText(filePath);
        Bookmark bookmark_bar = JObject.Parse(text)["roots"]["bookmark_bar"].ToObject<Bookmark>();
        Bookmark other = JObject.Parse(text)["roots"]["other"].ToObject<Bookmark>();
        Bookmark synced = JObject.Parse(text)["roots"]["synced"].ToObject<Bookmark>();
        synced.name = "Synced Bookmarks"; //has to be renamed, because google puts "Mobile bookmarks" in json and "Synced Bookmarks" in html
        other.name = "Other Bookmarks"; //has to be renamed, because google puts "Other bookmarks" in json and "Other Bookmarks" in html (diff: capitalisation!)
        bookmark_bar.name = "Bookmark Bar"; //has to be renamed, because google puts "Bookmarks bar" in json and "Bookmark Bar" in html (diff: capitalisation!, plural)
        //note: the naming MUST be consistent, so if html and autoimport are both used in the same directory videos will not get downloaded twice
        Console.WriteLine(bookmark_bar.date_added);
        Bookmark root = new Bookmark
        ///the root is not actually a bookmark json object, it just contains the 3 json objects of other, synced and bookmarks_bar
        ///
        ///as such here a root json object is created, which will contain those three as children
        {
            name = "Bookmarks",
            guid = Guid.NewGuid().ToString(), //adding new guid to the root
            id = Convert.ToInt16("0"), //id for : bookmark_bar=1, other=2, synced=3
            type = "folder",
            date_added = Convert.ToInt64(bookmark_bar.date_added) - 2, //just setting a time that was slightly earlier than the bookmark_bar creation
            date_last_used = Convert.ToInt64("0"), //not used by chrome apparently
            date_modified = Convert.ToInt64("0"), //not much used by chrome apparently
            children = new List<Bookmark> { bookmark_bar, other, synced }
        };
        if (false) //just to hide the long comment in the IDE
        {
            /* the structure of the file:
            {
             "checksum": "12345678912345678912345678912345",
             "roots": {
                "bookmark_bar": {
                        childred:[ ], /////////here are all the bookmarks generally
                        "date_added": "123456789123456789",
                        "date_last_used": "0",
                        "date_modified": "123456789123456789",
                        "guid": "guid-123456789123456789",
                        "id": "1",
                        "name": "Bookmarks bar",
                        "type": "folder"
                    },
                    "other": {
                        "children": [  ],
                        "date_added": "123456789123456789",
                        "date_last_used": "0",
                        "date_modified": "0",
                        "guid": "guid-123456789123456789",
                        "id": "2",
                        "name": "Other bookmarks",
                        "type": "folder"
                    },
                    "synced": {
                        "children": [  ],
                        "date_added": "123456789123456789",
                        "date_last_used": "0",
                        "date_modified": "0",
                        "guid": "guid-123456789123456789",
                        "id": "3",
                        "name": "Mobile bookmarks",
                        "type": "folder"
                    }
             },
             "sync_metadata": "123456789#__4000000_char_long_string",
             "version": 1
           }
            */
        }
        Bookmark bookmarkroot = root;
        Folderclass thisBookmark = new Folderclass();
        thisBookmark.startline = Globals.folderid;
        thisBookmark.urls = new List<string>();
        int numberoflinks = 0;
        int depth = 0;
        foreach (Bookmark child in bookmarkroot.children)
        {
            if (child.type == "url")
            {
                thisBookmark.urls.Add(child.url);
                //Console.WriteLine("URL: " + child.url);
                numberoflinks++;
            }
            else if (child.type == "folder")
            {
                //Console.WriteLine("Root folder: " + child.name);
                Globals.folderclasses.Add(Childfinder(child, depth+1));
            }
        }
        thisBookmark.name = bookmarkroot.name;
        thisBookmark.numberoflinks = numberoflinks;
        thisBookmark.depth = 0;
        thisBookmark.endingline = Globals.endingline;
        Globals.endingline++;
        Globals.folderclasses.Add(thisBookmark);
        Folderclass[] folders = Convertlisttoarray(Globals.folderclasses);
        return folders;
    }

    public static Folderclass Childfinder(Bookmark current, int depth)
    {
        Folderclass thisBookmark = new Folderclass();
        Globals.folderid++;
        thisBookmark.startline = Globals.folderid;
        thisBookmark.urls = new List<string>();
        //Console.WriteLine("Started childfinder with current folder: {1}, id:{0}, depth:{2}", globals.folderid, current.name, depth);
        int numberoflinks = 0;
        foreach (Bookmark child in current.children)
        {
            if (child.type == "url")
            {
                thisBookmark.urls.Add(child.url);
                //Console.WriteLine("Child URL: " + child.url);
                numberoflinks++;
            }
            else if (child.type == "folder")
            {
                //Console.WriteLine(current.name + "'s child Folder: " + child.name);
                Globals.folderclasses.Add(Childfinder(child, depth+1));
            }
        }
        thisBookmark.name = current.name;
        thisBookmark.numberoflinks = numberoflinks;
        thisBookmark.depth = depth;
        thisBookmark.endingline = Globals.endingline;
        Globals.endingline++;
        //Console.WriteLine("{0} has {1} links. Folderid: {2} depth: {3}", current.name, numberoflinks, globals.folderid, depth);
        //Console.WriteLine("Thisbookmark {0} has {1} links. Folderid: {2} depth: {3}", thisBookmark.name, thisBookmark.numberoflinks, thisBookmark.startline, thisBookmark.depth);
        return thisBookmark;
    }

    public static Folderclass[] Convertlisttoarray(List<Folderclass> folderclasses)
    {
        Folderclass[] folders = new Folderclass[Globals.folderid + 1];
        for (int q = 0; q < Globals.folderid + 1; q++)
        {
            folders[q] = new Folderclass();
        }
        for (int i = 0; i < Globals.folderid + 1; i++)
        {
            foreach (Folderclass folderclass in folderclasses)
            {
                if (folderclass.startline == i)
                {
                    //Console.WriteLine(folderclass.startline + "==" + i);
                    folders[i].name = folderclass.name;
                    folders[i].depth = folderclass.depth;
                    folders[i].startline = folderclass.startline;
                    folders[i].endingline = folderclass.endingline;
                    folders[i].numberoflinks = folderclass.numberoflinks;
                    folders[i].urls = folderclass.urls;
                    //Console.WriteLine("name " + folders[i].name + "==" + folderclass.name);
                    //Console.WriteLine("depth " + folders[i].depth + "==" + folderclass.depth);
                    //Console.WriteLine("startline " + folders[i].startline + "==" + folderclass.startline);
                    //Console.WriteLine("numberoflinks " + folders[i].numberoflinks + "==" + folderclass.numberoflinks);
                }
            }
        }
        return folders;
    }

    public static class Globals
    {
        public static int folderid = 0; //used a lot instead of numberoffolders, maybe not ideal?
        public static int totalyoutubelinknumber;
        public static int startline;
        public static string name;
        public static int depth = 0;
        public static int endingline = 0;
        public static string folderpath;
        public static int numberoflinks;
        public static List<Folderclass> folderclasses = new List<Folderclass>();
        public static List<Bookmark> sql_Bookmarks = new List<Bookmark>();
    }
}

public class Bookmark
{
    public Int64 date_added;
    public Int64 date_last_used;
    public Int64 date_modified; //only where type = folder
    public string guid;
    public Int16 id;
    public string name;
    public string type;
    public string url; //only where type = url
    public List<Bookmark> children; //only where type = folder
}

/*public class Sql_bookmark
{
    public string url;
    public string title;
    public int parent;
    public Int16 id;
    public string type;
    public Int64 dateadded;
    public Int64 lastmodified;
}*/