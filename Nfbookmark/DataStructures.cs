using System;
using System.Collections.Generic;
using NfLogger;

namespace bookmark_dlp
{
    /// <summary>
    /// Not used for now. A more refined wrapper around just a list of folders, intended to represent all bookmarks found in a single browser profile.
    /// Feels a bit redundant when Bookmark already exists.
    /// </summary>
    public class Bookmarks
    {
        public List<Folderclass> folders = new List<Folderclass>();
        public int numberofurlbookmarks;
        public int numberofbookmarks;
        
        public Bookmarks() { numberofbookmarks = numberofurlbookmarks + folders.Count; }
    }
    
    public enum Linktype 
    {
        Video, //regular video, default
        Channel_user, // if (linkthatisbeingexamined.Substring(24, 4) == "user")
        Channel_channel, // if (linkthatisbeingexamined.Substring(24, 7) == "channel")
        Channel_at, // if (linkthatisbeingexamined.Substring(24, 1) == "@")
        Channel_c, // if (linkthatisbeingexamined.Substring(24, 2) == "c/")
        Short, // if (linkthatisbeingexamined.Substring(24, 6) == "shorts")
        Playlist, // if (linkthatisbeingexamined.Substring(24, 8) == "playlist")
        Search // if (linkthatisbeingexamined.Substring(24, 7) == "results") //youtube search result was bookmarked
    }

    /// <summary>
    /// Youtube links
    /// </summary>
    public struct YTLink
    {
        public string url;
        public Linktype linktype;
        public string yt_id;
        public string channel_id;
        public string playlist_id;
        /// <summary>
        /// Contains yt ids of videos in the playlist or uploaded by the channel. Only used it linktype is Channel_* or Playlist. 
        /// </summary>
        public List<string> member_ids;
    }

    /// <summary>
    /// The main datatype used by the library, represents one bookmarks folder.
    /// </summary>
    public class Folderclass //defining the folderclass class to create an object list from it
    {
        /// <summary>
        /// For HTML: the line number in which the folder starts in the html. <br/>
        /// For JSON: (chromium-based): the folder id, same as the folder[totalyoutubelinknumber] index. <br/>
        /// For SQL: (firefox-based): the bookmark id of the folder in the sql db
        /// </summary>
        public int startline;
        /// <summary>
        /// The name of the folder. Can be empty.
        /// </summary>
        public string name;
        /// <summary>
        /// Depth of folder compared to other folders. Root has a depth of 0.
        /// </summary>
        public int depth;
        /// <summary>
        /// Should only be used in html intake.
        /// </summary>
        public int endingline;
        /// <summary>
        /// The filesystem folder representing the bookmark folder. Not used by default
        /// </summary>
        public string folderpath;
        /// <summary>
        /// = urls.Count()
        /// </summary>
        public int numberoflinks;
        /// <summary>
        /// All links count: video and yt-short links and playlist and channel
        /// Channel is missing if a single video from channel is missing.
        /// Playlist is missing if a single video from playlist is missing.
        /// </summary>
        public int numberofmissinglinks;
        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public List<YTLink> missinglinks = new List<YTLink>();
        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public List<string> missingurls = new List<string>();
        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public List<YTLink> foundlinks = new List<YTLink>();
        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public List<string> foundurls = new List<string>();
        /// <summary>
        /// List of all the URLs in the given folder. May be empty.
        /// </summary>
        public List<string> urls = new List<string>();
        /// <summary>
        /// YTLink list from urls that contains the link type.
        /// </summary>
        public List<YTLink> links = new List<YTLink>();
        /// <summary>
        /// The id of current folder
        /// </summary>
        public int id; //same as array index
        /// <summary>
        /// The id of the parent folder of current folder
        /// </summary>
        public int parent;
        /// <summary>
        /// The ids of children folders of current folder
        /// </summary>
        public List<int> children = new List<int>();
        /// <summary>
        /// Field used by the observable class mostly,
        /// checks if user wants the direct content of the folder downloaded.
        /// Children folders are unaffected.
        /// </summary>
        public bool wantDownloaded = true;
        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public int numberOfVideosDirectlyWanted;
        /// <summary>
        /// Only playlist or channel links count, no video and yt-short links
        /// </summary>
        public int numberOfVideosIndirectlyWanted;
        /// <summary>
        /// All wanted videos count, be it playlist or channel links or video or yt-short
        /// </summary>
        public int numberOfVideosAllWanted;
        /// <summary>
        /// All wanted videos count, be it playlist or channel links or video or yt-short
        /// </summary>
        public int numberOfWantedVideosFound;
        /// <summary>
        /// Only video files not found in want list now count.
        /// Eg.
        ///     videos downloaded from channel earlier that have since been deleted, (only channel was bookmarked)
        ///     videos that were downloaded but later unbookmarked
        /// But not:
        ///     videos that were bookmarked and downloaded and are still bookmarked, but no longer can be downloaded because they were removed from youtube 
        /// </summary>
        public int numberOfOtherVideosFound;
        /// <summary>
        /// Every video file in directory counts towards this.
        /// </summary>
        public int numberOfAllVideosFound;
        
        
        public override string ToString()
        {
            return $"Name:{name}, id:{id}, depth:{depth}, number of urls:{urls.Count}";
        }
        
        // int startline
        // string name
        // int depth
        // int endingline
        // string folderpath
        // int numberoflinks
        // int numberofmissinglinks
        // List<YTLink> missinglinks
        // List<string> missingurls
        // List<YTLink> foundlinks
        // List<string> foundurls
        // List<string> urls
        // List<YTLink> links
        // int id
        // int parent
        // List<int> children
        // bool wantDownloaded
        // int numberOfVideosDirectlyWanted
        // int numberOfVideosIndirectlyWanted
        // int numberOfVideosAllWanted
        // int numberOfWantedVideosFound
        // int numberOfOtherVideosFound
        // int numberOfAllVideosFound
    }

    /// <summary>
    /// One bookmark. Only used when importing from SQL or JSON (not for html)
    /// </summary>
    public class Bookmark
    {
        public Int64 date_added;
        public Int64 date_last_used;
        public Int64 date_modified; //only where type = folder
        public string guid;
        public int id;
        public string name;
        public string type;
        public string url; //only where type = url
        public List<Bookmark> children = new List<Bookmark>(); //only where type = folder

        public override string ToString()
        {
            return $"name:{name} type:{type}, id:{id}";
        }
    }

    /// <summary>
    /// Representing the default locations (filepaths) where one browser might store its
    /// user profiles.
    /// </summary>
    public partial class BrowserLocations
    {
        public string browsername;
        public string windows_profilespath = "";
        public List<string> linux_profilespath = new List<string>();
        public List<string> osx_profilespath = new List<string>();
        //public Int16 profilesfound = 0;
        public List<string> hardcodedpaths = new List<string>();
        /// <summary>
        /// List of paths to FILES containing bookmarks (one file for one browser profile usually)
        /// </summary>
        public List<string> foundProfiles = new List<string>();
        /// <summary>
        /// Type of this browser: Chromium of Firefox based
        /// </summary>
        public BrowserType browserType = BrowserType.none;

        public override string ToString()
        {
            return $"Name:{browsername}, found profiles:{foundProfiles.ToString()}";
        }
    }

    /// <summary>
    /// Type: Chromium of Firefox based
    /// </summary>
    public enum BrowserType { none, chromiumbased, firefoxbased }

    /// <summary>
    /// Legacy code, but still used. Basically globaly variables but in a struct that is passed to functions as necessary.
    /// </summary>
    struct GlobalState
    {
        public int folderid; //used a lot instead of numberoffolders, maybe not ideal?
        //public int totalyoutubelinknumber;
        //public int startline;
        //public int depth;
        public int endingline;
    }
}