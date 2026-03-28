using Nfbookmark;
using Nfbookmark.Importers;
using Xunit;

namespace Nfbookmark.Tests;

public class FolderManagerTests
{
    #region DeleteEmtpyFoldersFunctionTests

    [Fact]
    public void DeleteemptyfoldersFunctionTest()
    {
        string rootPath = Path.Combine(Path.GetTempPath(), "testing_now");

        try
        {
            // Arrange: Create test folder structure
            List<ImportedFolder> folders = new List<ImportedFolder>
            {
                new ImportedFolder { Name = "Root", Depth = 0, ParentId = -1, Id = 0 },
                new ImportedFolder { Name = "Alfa", Depth = 1, ParentId = 0, Id = 1 },
                new ImportedFolder { Name = "Bravo", Depth = 1, ParentId = 0, Id = 2 },
                new ImportedFolder { Name = "Charlie", Depth = 2, ParentId = 2, Id = 3 },
                new ImportedFolder { Name = "Delta", Depth = 3, ParentId = 3, Id = 4 }
            };

            List<MappedFolder> mappedFolders = FolderManager.CreateFolderStructure(folders, rootPath);
            string bookmarksRoot = Path.Combine(rootPath, "Bookmarks");

            // Create files in Root and Alfa to prevent their deletion
            File.Create(Path.Combine(mappedFolders.Single(f => f.Name == "Root").FolderPath, "test.txt")).Dispose();
            File.Create(Path.Combine(mappedFolders.Single(f => f.Name == "Alfa").FolderPath, "test.txt")).Dispose();

            Assert.True(Directory.Exists(Path.Combine(bookmarksRoot, "Root")));
            Assert.True(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Alfa")));
            Assert.True(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Bravo")));
            Assert.True(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Bravo", "Charlie")));
            Assert.True(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Bravo", "Charlie", "Delta")));

            // Act
            FolderManager.DeleteEmptyFolders(mappedFolders);

            // Assert
            Assert.True(Directory.Exists(Path.Combine(bookmarksRoot, "Root")));
            Assert.True(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Alfa")));
            Assert.False(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Bravo")));
            Assert.False(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Bravo", "Charlie")));
            Assert.False(Directory.Exists(Path.Combine(bookmarksRoot, "Root", "Bravo", "Charlie", "Delta")));
        }
        finally
        {
            if (Directory.Exists(rootPath))
                Directory.Delete(rootPath, true);
        }
    }

    #endregion DeleteEmtpyFoldersFunctionTests

    

    #region CreatefolderstructureFunctionTests

    [Fact]
    public void CreateFolderStructure_CreatesFoldersCorrectly()
    {
        // Arrange
        List<ImportedFolder> folders = new List<ImportedFolder>
        {
            new ImportedFolder { Name = "Folder1", Depth = 0, ParentId = 0, Id = 0 },
            new ImportedFolder { Name = "SubFolder1", Depth = 1, ParentId = 0, Id = 1 },
            new ImportedFolder { Name = "SubFolder2", Depth = 1, ParentId = 0, Id = 2 },
            new ImportedFolder { Name = "SubSubSubFolder1", Depth = 3, ParentId = 5, Id = 3 },
            new ImportedFolder { Name = "SubSubFolder1-1", Depth = 2, ParentId = 2, Id = 4 },
            new ImportedFolder { Name = "SubSubFolder2", Depth = 2, ParentId = 1, Id = 5 },
            new ImportedFolder { Name = "SubSubFolder1-2", Depth = 2, ParentId = 2, Id = 6 }
        };
        Assert.Equal(7, folders.Count);
        string rootDir = Path.Combine(Path.GetTempPath(), "RootTestDir");
        Directory.CreateDirectory(rootDir);
        try
        {
            // Act
            List<MappedFolder> mappedFolders = FolderManager.CreateFolderStructure(folders, rootDir);

            // Assert
            string expectedPath1 = Path.Combine(rootDir, "Bookmarks", "Folder1");
            string expectedPath2 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder1");
            string expectedPath3 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder2");
            string expectedPath4 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder2", "SubSubFolder1-1");
            string expectedPath5 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder2", "SubSubFolder1-2");
            string expectedPath6 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder1", "SubSubFolder2");
            string expectedPath7 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder1", "SubSubFolder2", "SubSubSubFolder1");

            Assert.True(Directory.Exists(expectedPath1));
            Assert.True(Directory.Exists(expectedPath2));
            Assert.True(Directory.Exists(expectedPath3));
            Assert.True(Directory.Exists(expectedPath4));
            Assert.True(Directory.Exists(expectedPath5));
            Assert.True(Directory.Exists(expectedPath6));
            Assert.True(Directory.Exists(expectedPath7));

            Assert.Equal(expectedPath1, mappedFolders.Single(f => f.Id == 0).FolderPath);
            Assert.Equal(expectedPath2, mappedFolders.Single(f => f.Id == 1).FolderPath);
            Assert.Equal(expectedPath3, mappedFolders.Single(f => f.Id == 2).FolderPath);
            Assert.Equal(expectedPath4, mappedFolders.Single(f => f.Id == 4).FolderPath);
            Assert.Equal(expectedPath5, mappedFolders.Single(f => f.Id == 6).FolderPath);
            Assert.Equal(expectedPath6, mappedFolders.Single(f => f.Id == 5).FolderPath);
            Assert.Equal(expectedPath7, mappedFolders.Single(f => f.Id == 3).FolderPath);
        }
        finally
        {
            Directory.Delete(rootDir, true);
        }
    }

    [Fact]
    public void CreateFolderStructure_HandlesEmptyRootDirectory()
    {
        // Arrange
        List<ImportedFolder> folders = new List<ImportedFolder>
        {
            new ImportedFolder { Name = "Folder1", Depth = 0, Id = 0 }
        };

        string rootDir = Path.Combine(Path.GetTempPath(), "RootTestDir2");

        try
        {
            // Act
            List<MappedFolder> mappedFolders = FolderManager.CreateFolderStructure(folders, rootDir);

            // Assert
            string expectedPath = Path.Combine(rootDir, "Bookmarks", "Folder1");

            Assert.True(Directory.Exists(expectedPath));
            Assert.Equal(expectedPath, mappedFolders.Single(f => f.Id == 0).FolderPath);
        }
        finally
        {
            Directory.Delete(rootDir, true);
        }
    }

    [Fact]
    public void CreateFolderStructure_HandlesDepthChangesCorrectly()
    {
        // Arrange
        List<ImportedFolder> folders = new List<ImportedFolder>
        {
            new ImportedFolder { Name = "RootFolder", Depth = 0, Id = 0 },
            new ImportedFolder { Name = "ChildFolder", Depth = 1, ParentId = 0, Id = 1 },
            new ImportedFolder { Name = "SiblingFolder", Depth = 1, ParentId = 0, Id = 2 },
            new ImportedFolder { Name = "ChildOfSibling", Depth = 2, ParentId = 2, Id = 3 }
        };

        string rootDir = Path.Combine(Path.GetTempPath(), "RootTestDir3");
        Directory.CreateDirectory(rootDir);

        try
        {
            // Act
            List<MappedFolder> mappedFolders = FolderManager.CreateFolderStructure(folders, rootDir);

            // Assert
            string expectedPathRoot = Path.Combine(rootDir, "Bookmarks", "RootFolder");
            string expectedPathChild = Path.Combine(rootDir, "Bookmarks", "RootFolder", "ChildFolder");
            string expectedPathSibling = Path.Combine(rootDir, "Bookmarks", "RootFolder", "SiblingFolder");
            string expectedPathChildOfSibling = Path.Combine(rootDir, "Bookmarks", "RootFolder", "SiblingFolder", "ChildOfSibling");

            Assert.True(Directory.Exists(expectedPathRoot));
            Assert.True(Directory.Exists(expectedPathChild));
            Assert.True(Directory.Exists(expectedPathSibling));
            Assert.True(Directory.Exists(expectedPathChildOfSibling));

            Assert.Equal(expectedPathRoot, mappedFolders.Single(f => f.Id == 0).FolderPath);
            Assert.Equal(expectedPathChild, mappedFolders.Single(f => f.Id == 1).FolderPath);
            Assert.Equal(expectedPathSibling, mappedFolders.Single(f => f.Id == 2).FolderPath);
            Assert.Equal(expectedPathChildOfSibling, mappedFolders.Single(f => f.Id == 3).FolderPath);
        }
        finally
        {
            Directory.Delete(rootDir, true);
        }
    }

    #endregion CreatefolderstructureFunctionTests
}