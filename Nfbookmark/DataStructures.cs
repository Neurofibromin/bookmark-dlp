using System;
using System.Collections.Generic;

namespace bookmark_dlp
{
    /// <summary>
    /// Not used for now. A more refined wrapper around just a list of folders, intended to represent all bookmarks found in a single browser profile
    /// </summary>
    public class Bookmarks
    {
        public List<Folderclass> folders = new List<Folderclass>();
        public int numberofurlbookmarks;
        public int numberofbookmarks;

        Bookmarks() { numberofbookmarks = numberofurlbookmarks + folders.Count; }
    }

    /// <summary>
    /// The main datatype used by the library, represents one bookmarks folder.
    /// </summary>
    public class Folderclass //defining the folderclass class to create an object array from it
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
        public int depth;
        public int endingline;
        /// <summary>
        /// The filesystem folder representing the bookmark folder. Not used by default
        /// </summary>
        public string folderpath;
        public int numberoflinks;
        public int numberofmissinglinks;
        /// <summary>
        /// List of all the URLs in the given folder. May be empty.
        /// </summary>
        public List<string> urls = new List<string>();
        public int id; //same as array index
        /// <summary>
        /// the id of the parent folder of current folder
        /// </summary>
        public int parent;
        /// <summary>
        /// the id of children folders of current folder
        /// </summary>
        public List<int> children = new List<int>();

        public override string ToString()
        {
            return $"Name:{name}, id:{id}, depth:{depth}, number of urls:{urls.Count}";
        }
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
    /// Representing the default locations (filepaths) where one browser might store its user profiles.
    /// </summary>
    public class BrowserLocations
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

    struct GlobalState
    {
        public int folderid; //used a lot instead of numberoffolders, maybe not ideal?
        //public int totalyoutubelinknumber;
        //public int startline;
        //public int depth;
        public int endingline;
    }
}