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
        var folders = new List<ImportedFolder>
        {
            new ImportedFolder { Id = 0, Name = "Root", Depth = 0, ParentId = 0, ChildrenIds = new List<int> { 1, 2 } },
            new ImportedFolder { Id = 1, Name = "Child 1", Depth = 1, ParentId = 0, ChildrenIds = new List<int>() },
            new ImportedFolder { Id = 2, Name = "Child 2", Depth = 1, ParentId = 0, ChildrenIds = new List<int>() }
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
        var folders = new List<ImportedFolder>
        {
            new ImportedFolder { Id = 0, Name = "Grandparent", Depth = 0, ParentId = 0, ChildrenIds = new List<int> { 1 } },
            new ImportedFolder { Id = 1, Name = "Parent", Depth = 1, ParentId = 0, ChildrenIds = new List<int> { 2 } },
            new ImportedFolder { Id = 2, Name = "Child", Depth = 2, ParentId = 1, ChildrenIds = new List<int>() }
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
        var folders = new List<ImportedFolder>
        {
            new ImportedFolder { Id = 0, Name = "Root 1", Depth = 0, ParentId = 0, ChildrenIds = new List<int>() },
            new ImportedFolder { Id = 1, Name = "Root 2", Depth = 0, ParentId = 0, ChildrenIds = new List<int> { 2 } },
            new ImportedFolder { Id = 2, Name = "Child of Root 2", Depth = 1, ParentId = 1, ChildrenIds = new List<int>() },
            new ImportedFolder { Id = 3, Name = "Root 3", Depth = 0, ParentId = 0, ChildrenIds = new List<int>() }
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