using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bookmark_extract_youtube_links;
using MintPlayer.PlatformBrowser;


internal class AutoImport
{
    public static List<string> getinstalledbrowsers()
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

    public static List<Folderclass> getAppdataFolders()
    {
        //use default location for chrome bookmarks file
        string filePath = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string windir = Environment.SystemDirectory; // C:\windows\system32
            string windrive = Path.GetPathRoot(Environment.SystemDirectory); // C:\
            filePath = windrive + "\\Users\\" + Environment.UserName + "\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Bookmarks";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            filePath = "~/.config/google-chrome/Default/Bookmarks";
        }
        else 
        {
            throw new NotSupportedException("OS not supported");
        }
        string text = File.ReadAllText(filePath);

        //Deserialize using JSON.NET, may need separate install
        Bookmark bookmarks = JObject.Parse(text)["roots"]["bookmark_bar"].ToObject<Bookmark>();

        //convert to list of folders with set depth and name
        List<Folderclass> folders = bookmarksToFolderclass(bookmarks, 0, Directory.GetCurrentDirectory());

        //print all that with indentations
        /*foreach (Bookmark bookmark in bookmarks) { 
            //Console.WriteLine(new string('-',bookmark.depth) + " " + bookmark.name); 
        }*/
        return folders;
    }

    public static List<Folderclass> bookmarksToFolderclass(Bookmark bookmark, int depth, string path)
    {
        List<Folderclass> folderclasses = new List<Folderclass>();
        Folderclass thisBookmark = new Folderclass();
        
        foreach (Bookmark child in bookmark.children)
        {
            if (child.type == "url")
            {
                thisBookmark.urls.Add(child.url);
            }
            else if (child.type == "folder")
            {
                folderclasses.Concat(bookmarksToFolderclass(child, depth + 1, path + "\\" + child.name));
            }
        }
        folderclasses.Add(thisBookmark);
        return folderclasses;
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
