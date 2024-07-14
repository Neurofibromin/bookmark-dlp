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

internal class AutoImport
{
    public static int WritelinkstotxtFromFolderclasses(ref List<Folderclass> folders, string rootdir, bool downloadPlaylists, bool downloadShorts, bool downloadChannels)
    {
        StreamWriter temp = new StreamWriter(Path.Combine(rootdir, "temp.txt"), append: true); //writing into temp.txt all the youtube links that are not for videos (but for channels, playlists, etc.)
        int totalyoutubelinknumber = 0;
        for (int j = 0; j < folders.Count; j++)
        {
            StreamWriter writer = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"), append: false);
            StreamWriter complexnotsimple = new StreamWriter(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"), append: true); //writing all the youtube links that are not for videos (but for channels, playlists, etc.) in the given folder
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
                    if (linkthatisbeingexamined.Substring(24, 8) == "playlist") //filtering the links with the consecutive ifs to find if they are for videos or else (channels, playlists, etc.)
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
                    if (linkthatisbeingexamined.Substring(24, 7) == "results") //youtube search result was bookmarked
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
            folders[j].numberoflinks = linknumbercounter; //gives count of how many youtube links were found in this folder
            totalyoutubelinknumber += linknumbercounter; //increase total link number by number of links found in this folder
            if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt")).Length == 0) //if the txt reamined empty it is deleted
            {
                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"));
            }
            if (new FileInfo(Path.Combine(folders[j].folderpath, folders[j].name + ".txt")).Length == 0) //if the txt remained empty it is deleted
            {
                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".txt"));
                Methods.LogVerbose($"Deleted txt of {folders[j].name}", Methods.Verbosity.trace);
            }
            /*if (!wantcomplex)
            {
                File.Delete(Path.Combine(folders[j].folderpath, folders[j].name + ".complex.txt"));
            }*/
        }
        temp.Flush();
        temp.Close();
        Methods.LogVerbose("Total number of youtube links found: " + totalyoutubelinknumber, Methods.Verbosity.info);
        return totalyoutubelinknumber;
    }

    public static void Createfolderstructure(ref List<Folderclass> folders, string rootdir)
    {
        //creating the folder structure and storing the access paths to the folders[].folderpath object array
        if (!Directory.Exists(rootdir)) { Directory.CreateDirectory(rootdir); }
        Directory.SetCurrentDirectory(rootdir);
        System.IO.Directory.CreateDirectory("Bookmarks");
        Directory.SetCurrentDirectory("Bookmarks");
        for (int m = 0; m < folders.Count; m++)
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
    }




    /*
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
                    folders[i].name = folderclass.name;
                    folders[i].depth = folderclass.depth;
                    folders[i].startline = folderclass.startline;
                    folders[i].endingline = folderclass.endingline;
                    folders[i].numberoflinks = folderclass.numberoflinks;
                    folders[i].urls = folderclass.urls;
                    folders[i].id = folderclass.id;
                    folders[i].parent = folderclass.parent;
                    folders[i].children = folderclass.children;
                }
            }
        }
        return folders;
    }

    public static List<Folderclass> Convertarraytolist(Folderclass[] folderclasses)
    {
        List<Folderclass> folderlist = new List<Folderclass>();
        for (int i = 0; i < folderclasses.Length; i++)
        {
            try
            {
                if (folderclasses[i].id != null)
                {
                    folderlist.Add(folderclasses[i]);
                }
            }
            catch
            {
                continue;
            }
        }
        return folderlist;
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
    }*/
}



public partial class ObsFolderclass : ObservableObject
{
    [ObservableProperty]
    public int startline; //for html: the line number in which the folder starts in the html. json(autoimport intake chrome): the folder id, same as the folder[totalyoutubelinknumber] index. firefox-sql: the bookmark id of the folder in the sql db
    [ObservableProperty]
    public string name;
    [ObservableProperty]
    public int depth;
    [ObservableProperty]
    public int endingline;
    [ObservableProperty]
    public string folderpath;
    [ObservableProperty]
    public int numberoflinks;
    [ObservableProperty]
    public int numberofmissinglinks;
    [ObservableProperty]
    public List<string> urls = new List<string>();
    [ObservableProperty]
    public int id; //same as array index
    [ObservableProperty]
    public int parent;
    [ObservableProperty]
    public List<int> children = new List<int>();
    [ObservableProperty]
    public bool wantdownloaded = true;


    public ObsFolderclass(Folderclass other) 
    {
        startline = other.startline;
        name = other.name;
        depth = other.depth;
        endingline = other.endingline;
        folderpath = other.folderpath;
        numberoflinks = other.numberoflinks;
        numberofmissinglinks = other.numberofmissinglinks;
        urls = other.urls;
        id = other.id;
        parent = other.parent;
        children = other.children;
    }
}