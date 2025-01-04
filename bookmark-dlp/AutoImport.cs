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
using NfLogger;

namespace bookmark_dlp
{
    internal class AutoImport
    {
        /// <summary>
        /// Fills the member_ids field of a YTLink. Queries channel and playlist data mostly.
        /// </summary>
        /// <param name="link">Link being checked for "children" - eg. channel asked for its videos. If called on a single video and the video does not have its yt-id filled, it gets corrected. But yt-id should already have been filled by Functions.UrlsToYTLinks()</param>
        public static void GetVideoIds(ref YTLink link)
        {
            // TODO: GetVideoIds from channel and playlist urls
            if (link.linktype == Linktype.Video)
            {
            }

            if (link.linktype == Linktype.Short)
            {
            }

            if (link.linktype == Linktype.Search)
            {
                return;
            }

            // yt-dlp --flat-playlist "https://www.youtube.com/@channelname/" --get-id
            /* Error:
             * WARNING: [youtube:tab] HTTP Error 404: Not Found. Retrying (1/3)...
               WARNING: [youtube:tab] HTTP Error 404: Not Found. Retrying (2/3)...
               WARNING: [youtube:tab] HTTP Error 404: Not Found. Retrying (3/3)...
               WARNING: [youtube:tab] Unable to download webpage: HTTP Error 404: Not Found (caused by <HTTPError 404: Not Found>). Giving up after 3 retries
               WARNING: [youtube:tab] YouTube said: ERROR - Requested entity was not found.
               WARNING: [youtube:tab] HTTP Error 404: Not Found. Retrying (1/3)...
               WARNING: [youtube:tab] YouTube said: ERROR - Requested entity was not found.
               WARNING: [youtube:tab] HTTP Error 404: Not Found. Retrying (2/3)...
               WARNING: [youtube:tab] YouTube said: ERROR - Requested entity was not found.
               WARNING: [youtube:tab] HTTP Error 404: Not Found. Retrying (3/3)...
               WARNING: [youtube:tab] YouTube said: ERROR - Requested entity was not found.
               Good output:
               the ids, one in each line.
             */
            List<string> videoIds = new List<string>();
            if (link.linktype == Linktype.Playlist)
            {
            }

            if (link.linktype == Linktype.Channel_c)
            {
            }

            if (link.linktype == Linktype.Channel_user)
            {
            }

            if (link.linktype == Linktype.Channel_channel)
            {
            }

            if (link.linktype == Linktype.Channel_at)
            {
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
                folders[j].numberoflinks =
                    linknumbercounter; //gives count of how many youtube links were found in this folder
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


/*
    public partial class ObsFolderclass : ObservableObject
    {
        //NOTE: Not in agreement with Folderclass, eg. Children and children are different types. 
        [ObservableProperty] private int _startline; //for html: the line number in which the folder starts in the html.
                        //json(autoimport intake chrome): the folder id, same as the folder[totalyoutubelinknumber] index.
                        //firefox-sql: the bookmark id of the folder in the sql db
        [ObservableProperty] private string _name;
        [ObservableProperty] private int _depth;
        [ObservableProperty] private int _endingline;
        [ObservableProperty] private string _folderpath;
        [ObservableProperty] private int _numberoflinks;
        [ObservableProperty] private int _numberofmissinglinks;
        [ObservableProperty] private List<string> _urls;
        [ObservableProperty] private int _id; //same as array index
        [ObservableProperty] private int _parent;
        [ObservableProperty] private bool _wantDownloaded;
        [ObservableProperty] private int _numberOfVideosDirectlyWanted;
        [ObservableProperty] private int _numberOfVideosIndirectlyWanted;
        [ObservableProperty] private int _numberOfVideosAllWanted;
        [ObservableProperty] private int _numberOfWantedVideosFound;
        [ObservableProperty] private int _numberOfOtherVideosFound;
        [ObservableProperty] private int _numberOfAllVideosFound;
        [ObservableProperty] private List<YTLink> _missinglinks;
        [ObservableProperty] private List<string> _missingurls;
        [ObservableProperty] private List<YTLink> _foundlinks;
        [ObservableProperty] private List<string> _foundurls;

        /// <summary>
        /// From now on only members of the observable class
        /// </summary>
        public ObservableCollection<ObsFolderclass> Children = new ObservableCollection<ObsFolderclass>();


        // ReSharper disable once ConvertToPrimaryConstructor
        public ObsFolderclass(Folderclass other)
        {
            _startline = other.startline;
            _name = other.name;
            _depth = other.depth;
            _endingline = other.endingline;
            _folderpath = other.folderpath;
            _numberoflinks = other.numberoflinks;
            _numberofmissinglinks = other.numberofmissinglinks;
            _urls = other.urls;
            _id = other.id;
            _parent = other.parent;
            _wantDownloaded = other.wantDownloaded;
            _numberOfVideosDirectlyWanted = other.numberOfVideosDirectlyWanted;
            _numberOfVideosIndirectlyWanted = other.numberOfVideosIndirectlyWanted;
            _numberOfVideosAllWanted = other.numberOfVideosAllWanted;
            _numberOfWantedVideosFound = other.numberOfWantedVideosFound;
            _numberOfOtherVideosFound = other.numberOfOtherVideosFound;
            _numberOfAllVideosFound = other.numberOfAllVideosFound;
            _missinglinks = other.missinglinks;
            _missingurls = other.missingurls;
            _foundlinks = other.foundlinks;
            _foundurls = other.foundurls;
            // children = new ObservableCollection<Folderclass>();
        }
    }
*/
}