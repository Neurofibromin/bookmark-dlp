using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bookmark_dlp;
using MintPlayer.PlatformBrowser;
using Microsoft.Data.Sqlite;

internal class AutoImport
{
    /// <summary>
    /// todo:
    /// handle complexnotsimple and temp streamwriters better
    /// the linknumbercounters are increased for complex links as well
    /// handle youtube shorts in both download and check
    /// check if folders.linknumber refers to all links or youtube links in autoimport
    /// there is some bug with checkformissing, out of range maybe?
    /// chrome export bookmarks uses different layout html than google takeout
    /// running the scripts doesn't work for some reason
    /// </summary>


    public static void AutoMain()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string rootdir = Directory.GetCurrentDirectory(); //current directory
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //var InstalledBrowsers = new List<string>();
            List<string> InstalledBrowsers = AutoImport.Getinstalledbrowsers();
            Console.WriteLine(InstalledBrowsers);
        }
        bool wantcomplex = Methods.Wantcomplex();
        string filePath = FindfilePath();
        Folderclass[] folders = Intake(filePath);
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
            /*filePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "\\Google\\Chrome\\User Data\\Default\\Bookmarks");
            if (File.Exists(filePath)) { 
                Console.WriteLine("File found! " + "Filepath in chrome: " + filePath);
                filepaths.Add(filePath);
            }
            else { Console.WriteLine(($"Bookmarks file not found at " + filePath)); } */
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "\\Google\\Chrome\\User Data\\");
            int n = 0;
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
            if (n == 0)
            {
                Console.WriteLine(($"No Bookmarks file found in Chrome"));
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
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ///Users/$User/Library/Application Support/Google/Chrome/Default
            int n = 0;
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome");
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

        //Brave Browser
        //%localappdata%\BraveSoftware\Brave-Browser\User Data\Default\
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "\\BraveSoftware\\Brave-Browser\\User Data\\");
            int n = 0;
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
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //$User/.config/BraveSoftware/Brave-Browser/Default/Bookmarks.
            int n = 0;
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BraveSoftware/Brave-Browser/");
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
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            ///$HOME/Library/Application Support/BraveSoftware/Brave-Browser/Default/Bookmarks
            int n = 0;
            profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/BraveSoftware/Brave-Browser");
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

        if (filepaths.Count == 0)
        {
            throw new FileNotFoundException($"No bookmarks file was found by autoimport in any of the locations");
        }
        Console.WriteLine("Which browser bookmarks would you like to use?\nWrite the number");
        int m = 0;
        foreach (string path in filepaths)
        {
            Console.WriteLine(m + " path");
            m++;
        }
        filePath = filepaths.ElementAt(Convert.ToInt32(Console.ReadLine()));
        return filePath;
    }

    public static Folderclass[] Intake(string filePath)
    {
        Console.WriteLine("Autoimport intake start");
        string text = File.ReadAllText(filePath);
        Bookmark bookmarkroot = JObject.Parse(text)["roots"]["bookmark_bar"].ToObject<Bookmark>();
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

    public static List<string> Getinstalledbrowsers()
    {
        //find which browsers are installed
        var browsers = PlatformBrowser.GetInstalledBrowsers();
        var InstalledBrowsers = new List<string>();
        foreach (var browser in browsers)
        {
            Console.WriteLine($"Browser: {browser.Name}");
            Console.WriteLine($"Executable: {browser.ExecutablePath}");
            Console.WriteLine($"Icon path: {browser.IconPath}");
            Console.WriteLine($"Icon index: {browser.IconIndex}");
            Console.WriteLine();
            InstalledBrowsers.Add(browser.Name);
        }
        return InstalledBrowsers;
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
    }
}

public class Bookmark
{
    public string date_added;
    public string date_last_used;
    public string date_modified; //only where type = folder
    public string guid;
    public string id;
    public string name;
    public string type;
    public string url; //only where type = url
    public List<Bookmark> children; //only where type = folder
}