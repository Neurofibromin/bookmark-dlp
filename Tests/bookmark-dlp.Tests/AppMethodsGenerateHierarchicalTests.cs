using Xunit;
using Nfbookmark;
using System.Collections.Generic;
using System.Linq;

namespace bookmark_dlp.Tests;

public class AppMethodsGenerateHierarchicalTests
{
    [Fact]
    public void GenerateHierarchical_WithSimpleParentChild_CreatesCorrectStructure()
    {
        // Arrange
        var folders = new List<Folderclass>
        {
            new Folderclass { id = 0, name = "Root", depth = 0, parentId = 0 },
            new Folderclass { id = 1, name = "Child 1", depth = 1, parentId = 0, childrenIds = new List<int>() },
            new Folderclass { id = 2, name = "Child 2", depth = 1, parentId = 0, childrenIds = new List<int>() }
        };

        // Act
        var result = AppMethods.GenerateHierarchicalFolderclassesFromList(folders);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // one root folder

        var root = result.First();
        Assert.Equal("Root", root.Name);
        Assert.NotNull(root.Children);
        Assert.Equal(2, root.Children.Count);
        Assert.Contains(root.Children, child => child.Name == "Child 1");
        Assert.Contains(root.Children, child => child.Name == "Child 2");
    }

    [Fact]
    public void GenerateHierarchical_WithMultiLevelStructure_CreatesCorrectNesting()
    {
        // Arrange
        var folders = new List<Folderclass>
        {
            new Folderclass { id = 0, name = "Grandparent", depth = 0, parentId = 0, childrenIds = {1} },
            new Folderclass { id = 1, name = "Parent", depth = 1, parentId = 0, childrenIds = {2} },
            new Folderclass { id = 2, name = "Child", depth = 2, parentId = 1, childrenIds = new List<int>() }
        };

        // Act
        var result = AppMethods.GenerateHierarchicalFolderclassesFromList(folders);

        // Assert
        Assert.Single(result);

        var grandparent = result.First();
        Assert.Equal("Grandparent", grandparent.Name);
        Assert.Single(grandparent.Children);

        var parent = grandparent.Children.First();
        Assert.Equal("Parent", parent.Name);
        Assert.Single(parent.Children);

        var child = parent.Children.First();
        Assert.Equal("Child", child.Name);
        Assert.Empty(child.Children);
    }

    [Fact]
    public void GenerateHierarchical_WithMultipleRoots_ReturnsAllRoots()
    {
        // Arrange
        var folders = new List<Folderclass>
        {
            new Folderclass { id = 0, name = "Root 1", depth = 0, parentId = 0 },
            new Folderclass { id = 1, name = "Root 2", depth = 0, parentId = 0 },
            new Folderclass { id = 2, name = "Child of Root 2", depth = 1, parentId = 1, childrenIds = new List<int>() },
            new Folderclass { id = 3, name = "Root 3", depth = 0, parentId = 0 }
        };

        // Act
        var result = AppMethods.GenerateHierarchicalFolderclassesFromList(folders);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, root => root.Name == "Root 1");
        Assert.Contains(result, root => root.Name == "Root 2");
        Assert.Contains(result, root => root.Name == "Root 3");

        var root2 = result.First(r => r.Name == "Root 2");
        Assert.Single(root2.Children);
        Assert.Equal("Child of Root 2", root2.Children.First().Name);
    }
}