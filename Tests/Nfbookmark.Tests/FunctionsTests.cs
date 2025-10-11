using Xunit;

namespace Nfbookmark.Tests;

public class FunctionsTests
{
    #region DeleteEmtpyFoldersFunctionTests

    [Fact]
    public void DeleteemptyfoldersFunctionTest()
    {
        string rootPath = Path.Combine(Path.GetTempPath(), "testing_now");

        try
        {
            // Arrange: Create test folder structure
            List<Folderclass> folders = new List<Folderclass>
            {
                new Folderclass { name = "Root", depth = 0, parentId = -1 },
                new Folderclass { name = "Alfa", depth = 1, parentId = 0 },
                new Folderclass { name = "Bravo", depth = 1, parentId = 0 },
                new Folderclass { name = "Charlie", depth = 2, parentId = 2 },
                new Folderclass { name = "Delta", depth = 3, parentId = 3 }
            };
            FolderManager.Createfolderstructure(folders, rootPath);
            rootPath = Path.Combine(rootPath, "Bookmarks");
            File.Create(Path.Combine(folders[0].folderpath, "test.txt"));
            File.Create(Path.Combine(folders[1].folderpath, "test.txt"));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Alfa")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo", "Charlie")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "Root", "Bravo", "Charlie", "Delta")));


            // Act
            FolderManager.Deleteemptyfolders(folders);

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

    #endregion DeleteEmtpyFoldersFunctionTests

    #region FoldernameValidationFunctionTests

    [Fact]
    public void FoldernameValidation_RemovesForbiddenCharacters()
    {
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = "folder/1", id = 1, depth = 0, parentId = 0 },
            new Folderclass { name = "folder?2", id = 2, depth = 0, parentId = 0 }
        };
        FolderManager.FoldernameValidation(folders);
        Assert.Equal("folder1", folders[0].name);
        Assert.Equal("folder2", folders[1].name);
    }

    [Fact]
    public void FoldernameValidation_HandlesEmptyNames()
    {
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = "", id = 1, depth = 0, parentId = 0 }
        };
        FolderManager.FoldernameValidation(folders);
        Assert.Equal("ID1", folders[0].name);
    }

    [Fact]
    public void FoldernameValidation_HandlesSpacesAndPeriods()
    {
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = " . ", id = 1, depth = 0, parentId = 0 }
        };
        FolderManager.FoldernameValidation(folders);
        Assert.Equal("ID1", folders[0].name);
    }

    [Fact]
    public void FoldernameValidation_HandlesNamesStartingWithPeriods()
    {
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = ".hidden", id = 1, depth = 0, parentId = 0 }
        };
        FolderManager.FoldernameValidation(folders);
        Assert.Equal("ID1", folders[0].name);
    }

    [Fact]
    public void FoldernameValidation_HandlesDuplicateNamesAtSameDepthAndParent()
    {
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = "duplicate", id = 1, depth = 0, parentId = 0 },
            new Folderclass { name = "duplicate", id = 2, depth = 0, parentId = 0 }
        };
        FolderManager.FoldernameValidation(folders);
        Assert.Equal("duplicateID1", folders[0].name);
        Assert.Equal("duplicateID2", folders[1].name);
    }

    #endregion FoldernameValidationFunctionTests

    #region CreatefolderstructureFunctionTests

    [Fact]
    public void CreateFolderStructure_CreatesFoldersCorrectly()
    {
        // Arrange
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = "Folder1", depth = 0, parentId = 0 },
            new Folderclass { name = "SubFolder1", depth = 1, parentId = 0 },
            new Folderclass { name = "SubFolder2", depth = 1, parentId = 0 },
            new Folderclass { name = "SubSubSubFolder1", depth = 3, parentId = 5 },
            new Folderclass { name = "SubSubFolder1-1", depth = 2, parentId = 2 },
            new Folderclass { name = "SubSubFolder2", depth = 2, parentId = 1 },
            new Folderclass { name = "SubSubFolder1-2", depth = 2, parentId = 2 }
        };
        Assert.Equal(7, folders.Count);
        string rootDir = Path.Combine(Path.GetTempPath(), "RootTestDir");
        Directory.CreateDirectory(rootDir);
        try
        {
            // Act
            FolderManager.Createfolderstructure(folders, rootDir);

            // Assert
            string expectedPath1 = Path.Combine(rootDir, "Bookmarks", "Folder1");
            string expectedPath2 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder1");
            string expectedPath3 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder2");
            string expectedPath4 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder2", "SubSubFolder1-1");
            string expectedPath5 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder2", "SubSubFolder1-2");
            string expectedPath6 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder1", "SubSubFolder2");
            string expectedPath7 = Path.Combine(rootDir, "Bookmarks", "Folder1", "SubFolder1", "SubSubFolder2",
                "SubSubSubFolder1");


            Assert.True(Directory.Exists(expectedPath1));
            Assert.True(Directory.Exists(expectedPath2));
            Assert.True(Directory.Exists(expectedPath3));
            Assert.True(Directory.Exists(expectedPath4));
            Assert.True(Directory.Exists(expectedPath5));
            Assert.True(Directory.Exists(expectedPath6));
            Assert.True(Directory.Exists(expectedPath7));

            Assert.Equal(expectedPath1, folders[0].folderpath);
            Assert.Equal(expectedPath2, folders[1].folderpath);
            Assert.Equal(expectedPath3, folders[2].folderpath);
            Assert.Equal(expectedPath4, folders[4].folderpath);
            Assert.Equal(expectedPath5, folders[6].folderpath);
            Assert.Equal(expectedPath6, folders[5].folderpath);
            Assert.Equal(expectedPath7, folders[3].folderpath);
        }
        finally
        {
            // Cleanup
            Directory.Delete(rootDir, true);
        }
    }

    [Fact]
    public void CreateFolderStructure_HandlesEmptyRootDirectory()
    {
        // Arrange
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = "Folder1", depth = 0 }
        };

        string rootDir = Path.Combine(Path.GetTempPath(), "RootTestDir2");

        try
        {
            // Act
            FolderManager.Createfolderstructure(folders, rootDir);

            // Assert
            string expectedPath = Path.Combine(rootDir, "Bookmarks", "Folder1");

            Assert.True(Directory.Exists(expectedPath));
            Assert.Equal(expectedPath, folders[0].folderpath);
        }
        finally
        {
            // Cleanup
            Directory.Delete(rootDir, true);
        }
    }

    [Fact]
    public void CreateFolderStructure_HandlesDepthChangesCorrectly()
    {
        // Arrange
        List<Folderclass> folders = new List<Folderclass>
        {
            new Folderclass { name = "RootFolder", depth = 0 },
            new Folderclass { name = "ChildFolder", depth = 1, parentId = 0 },
            new Folderclass { name = "SiblingFolder", depth = 1, parentId = 0 },
            new Folderclass { name = "ChildOfSibling", depth = 2, parentId = 2 }
        };

        string rootDir = Path.Combine(Path.GetTempPath(), "RootTestDir3");
        Directory.CreateDirectory(rootDir);

        try
        {
            // Act
            FolderManager.Createfolderstructure(folders, rootDir);

            // Assert
            string expectedPathRoot = Path.Combine(rootDir, "Bookmarks", "RootFolder");
            string expectedPathChild = Path.Combine(rootDir, "Bookmarks", "RootFolder", "ChildFolder");
            string expectedPathSibling = Path.Combine(rootDir, "Bookmarks", "RootFolder", "SiblingFolder");
            string expectedPathChildOfSibling =
                Path.Combine(rootDir, "Bookmarks", "RootFolder", "SiblingFolder", "ChildOfSibling");

            Assert.True(Directory.Exists(expectedPathRoot));
            Assert.True(Directory.Exists(expectedPathChild));
            Assert.True(Directory.Exists(expectedPathSibling));
            Assert.True(Directory.Exists(expectedPathChildOfSibling));

            Assert.Equal(expectedPathRoot, folders[0].folderpath);
            Assert.Equal(expectedPathChild, folders[1].folderpath);
            Assert.Equal(expectedPathSibling, folders[2].folderpath);
            Assert.Equal(expectedPathChildOfSibling, folders[3].folderpath);
        }
        finally
        {
            // Cleanup
            Directory.Delete(rootDir, true);
        }
    }

    #endregion CreatefolderstructureFunctionTests
}