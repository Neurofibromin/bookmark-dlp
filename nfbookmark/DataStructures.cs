using System;
using System.Collections.Generic;

namespace bookmark_dlp
{
    /// <summary>
    /// Not used for now. A more refined wrapper aroung just a list of folders, intended to represent all bookmarks found in a single browser profile
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
        /// for html: the line number in which the folder starts in the html. <br/>
        /// json (chrome-based): the folder id, same as the folder[totalyoutubelinknumber] index. <br/>
        /// sql (firefox-based): the bookmark id of the folder in the sql db
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
        /// the children folders of current folder
        /// </summary>
        public List<int> children = new List<int>();
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
        public Int16 profilesfound = 0;
        public List<string> hardcodedpaths = new List<string>();
        public List<string> foundFiles = new List<string>();
        public BrowserType browserType = BrowserType.none;
    }

    public enum BrowserType { none, chromebased, firefoxbased }

    struct GlobalState
    {
        public int folderid; //used a lot instead of numberoffolders, maybe not ideal?
        public int totalyoutubelinknumber;
        public int startline;
        public int depth;
        public int endingline;
    }
}