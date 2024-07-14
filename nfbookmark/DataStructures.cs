using System;
using System.Collections.Generic;

namespace bookmark_dlp
{
    public class Bookmarks
    {
        public List<Folderclass> folders = new List<Folderclass>();
        public int numberofurlbookmarks;
        public int numberofbookmarks;

        Bookmarks() { numberofbookmarks = numberofurlbookmarks + folders.Count; }
    }

    public class Folderclass //defining the folderclass class to create an object array from it
    {
        public int startline; //for html: the line number in which the folder starts in the html. json(autoimport intake chrome): the folder id, same as the folder[totalyoutubelinknumber] index. firefox-sql: the bookmark id of the folder in the sql db
        public string name;
        public int depth;
        public int endingline;
        public string folderpath;
        public int numberoflinks;
        public int numberofmissinglinks;
        public List<string> urls = new List<string>();
        public int id; //same as array index
        public int parent; //the parent folder of current folder
        public List<int> children = new List<int>(); //the children folders of current folder
    }

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

    public class BrowserLocations
    {
        public string browsername;
        public string windows_profilespath = "";
        public List<string> linux_profilespath = new List<string>();
        public List<string> osx_profilespath = new List<string>();
        public Int16 linksfound = 0;
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