using bookmark_dlp;
using Xunit;
using Nfbookmark;
using NfLogger;

namespace bookmark_dlp.Tests;

public class AppMethodsTests
{
    static List<Folderclass> GenerateFolderClasses()
    {
        var folders = new List<Folderclass>
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
    
    [Fact]
    public void DeleteemptyfoldersFunctionTest()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "testing_now");

        try
        {
            // Arrange: Create test folder structure
            var folders = new List<Folderclass>
            {
                new Folderclass { name = "Root", depth = 0, parentId = -1 },
                new Folderclass { name = "Alfa", depth = 1, parentId = 0 },
                new Folderclass { name = "Bravo", depth = 1, parentId = 0 },
                new Folderclass { name = "Charlie", depth = 2, parentId = 2 },
                new Folderclass { name = "Delta", depth = 3, parentId = 3 },
            };
            Functions.Createfolderstructure(ref folders, rootPath);
            rootPath = Path.Combine(rootPath, "Bookmarks");
            File.Create(Path.Combine(folders[0].folderpath, "test.txt"));
            File.Create(Path.Combine(folders[1].folderpath, "test.txt"));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Alfa")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo", "Charlie")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo", "Charlie", "Delta")));
            
            
            // Act
            AppMethods.Deleteemptyfolders(folders);

            // Assert
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Alfa")));
            Assert.False(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo")));
            Assert.False(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo", "Charlie")));
            Assert.False(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo", "Charlie", "Delta")));
        }
        finally
        {
            Directory.Delete(Path.Combine(rootPath, "../"), true);
        }
    }
}