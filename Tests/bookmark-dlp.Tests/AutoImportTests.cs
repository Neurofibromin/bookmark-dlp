using Nfbookmark;
namespace bookmark_dlp.Tests;

public class AutoImportTests
{
    private static List<Folderclass> GenerateFolderClasses()
    {
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass
            {
                id = 0,
                name = "RootFolder",
                depth = 0,
                parentId = -1, // Root folder has no parent
                urls = new List<string> { "https://root.example.com" }
            },
            new Folderclass
            {
                id = 1,
                name = "SubFolder1",
                depth = 1,
                parentId = 0,
                urls = new List<string> { "https://sub1.example.com", "https://sub1.docs.example.com" }
            },
            new Folderclass
            {
                id = 2,
                name = "SubFolder2",
                depth = 1,
                parentId = 0,
                urls = new List<string>() // No URLs for this folder
            },
            new Folderclass
            {
                id = 3,
                name = "SubSubFolder1",
                depth = 2,
                parentId = 1,
                urls = new List<string> { "https://subsub1.example.com" }
            },
            new Folderclass
            {
                id = 4,
                name = "SubSubFolder2",
                depth = 2,
                parentId = 1,
                urls = new List<string> { "https://subsub2.example.com" }
            },
            new Folderclass
            {
                id = 5,
                name = "EmptyFolder",
                depth = 1,
                parentId = 0,
                urls = new List<string>()
            }
        };

        return folders;
    }
}