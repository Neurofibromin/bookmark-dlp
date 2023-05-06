using Bookmark_dlp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

internal class Program
{
    private static void Main(string[] args)
    {
        //use default location for chrome bookmarks file
        string filePath = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            filePath = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Bookmarks";
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
        List<Bookmark> bookmarks = JObject.Parse(text)["roots"]["bookmark_bar"]["children"].ToObject<List<Bookmark>>();

        //convert to list of folders with set depth and name
        List<Folderclass> folders = bookmarksToFolderClasses(bookmarks,0, Directory.GetCurrentDirectory());

        //print all that with indentations
        foreach (Folderclass bookmark in folders) { 
            Console.WriteLine(new string('-',bookmark.depth) + " " + bookmark.name); 
        }

    }

    public static List<Folderclass> bookmarksToFolderClasses(List<Bookmark> bookmarks, int depth, string path)
    {
        List<Folderclass> folders = new List<Folderclass>();
        //recursively go through children and collect into one big list
        foreach(Bookmark bookmark in bookmarks) {
            if (bookmark.type == "url")
            {
                folders.Add(new Folderclass()
                {
                    name = bookmark.name,
                    depth = depth
                });
                //TODO export bookmark.url
            }
            else if (bookmark.type == "folder")
            {
                Console.WriteLine("Extracting bookmarks from folder: " + bookmark.name);
                //some recursion to traverse tree of bookmarks
                List<Folderclass> childFolders = bookmarksToFolderClasses(bookmark.children, depth + 1, Path.Combine(path, bookmark.name));
                //concatentate to current bookmark's list of children
                folders = folders.Concat(childFolders).ToList();
            }
            
            else
            {
                throw new InvalidDataException("Unknown bookmark of type: " + bookmark.type);
            }
        }
        return folders;
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