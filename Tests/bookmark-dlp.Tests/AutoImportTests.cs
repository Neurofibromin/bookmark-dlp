using Nfbookmark;
namespace bookmark_dlp.Tests;

public class AutoImportTests
{
    private static List<MappedFolder> GenerateMappedFolders()
    {
        var importedFolders = new List<ImportedFolder>
        {
            new ImportedFolder
            {
                Id = 0,
                Name = "RootFolder",
                Depth = 0,
                ParentId = -1, // Root folder has no parent
                urls = new List<string> { "https://root.example.com" }
            },
            new ImportedFolder
            {
                Id = 1,
                Name = "SubFolder1",
                Depth = 1,
                ParentId = 0,
                urls = new List<string> { "https://sub1.example.com", "https://sub1.docs.example.com" }
            },
            new ImportedFolder
            {
                Id = 2,
                Name = "SubFolder2",
                Depth = 1,
                ParentId = 0,
                urls = new List<string>() // No URLs for this folder
            },
            new ImportedFolder
            {
                Id = 3,
                Name = "SubSubFolder1",
                Depth = 2,
                ParentId = 1,
                urls = new List<string> { "https://subsub1.example.com" }
            },
            new ImportedFolder
            {
                Id = 4,
                Name = "SubSubFolder2",
                Depth = 2,
                ParentId = 1,
                urls = new List<string> { "https://subsub2.example.com" }
            },
            new ImportedFolder
            {
                Id = 5,
                Name = "EmptyFolder",
                Depth = 1,
                ParentId = 0,
                urls = new List<string>()
            }
        };

        return importedFolders.Select(f => new MappedFolder(f)).ToList();
    }
}