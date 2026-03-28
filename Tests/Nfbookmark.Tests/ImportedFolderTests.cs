using Xunit;
using Nfbookmark;

namespace Nfbookmark.Tests;

public class ImportedFolderTests
{
    [Fact]
    public void ImportedFolder_ChildrenEmptyAtStartTest()
    {
        ImportedFolder folder = new ImportedFolder();
        Assert.Empty(folder.ChildrenIds);
    }

    [Fact]
    public void ImportedFolder_StringRepresentationTest()
    {
        ImportedFolder folder = new ImportedFolder
        {
            StartLine = 0,
            Name = "test",
            Depth = 3,
            urls = new List<string>(),
            Id = 0,
            ParentId = 0
        };
        string strrepr = folder.ToString();
        Assert.Equal("Name:test, id:0, depth:3, number of urls:0", strrepr);
    }

    [Fact]
    public void ImportedFolder_EqualityEmptyTest()
    {
        ImportedFolder f1 = new ImportedFolder();
        ImportedFolder f2 = new ImportedFolder();
        
        Assert.True(f1.Equals(f2));
        Assert.True(f2.Equals(f1));
        Assert.True(f2.Equals(f2));
        
        Assert.False(f1 == f2);
        Assert.False(f2 == f1);
    }

    [Fact]
    public void ImportedFolder_EqualityNullTest()
    {
        ImportedFolder? f1 = null;
        ImportedFolder? f2 = null;

        Assert.True(f1 == f2);
        Assert.True(f2 == f1);
    }

    [Fact]
    public void ImportedFolder_Equals_PopulatedInstances_ReturnsTrue()
    {
        ImportedFolder f1 = new ImportedFolder
        {
            Id = 1,
            ParentId = 0,
            Depth = 2,
            StartLine = 10,
            Name = "MyFolder",
            urls = new List<string> { "https://example.com" },
            ChildrenIds = new List<int> { 2, 3 }
        };
        ImportedFolder f2 = new ImportedFolder
        {
            Id = 1,
            ParentId = 0,
            Depth = 2,
            StartLine = 10,
            Name = "MyFolder",
            urls = new List<string> { "https://example.com" },
            ChildrenIds = new List<int> { 2, 3 }
        };

        Assert.True(f1.Equals(f2));
        Assert.True(f2.Equals(f1));
    }

    [Fact]
    public void ImportedFolder_Equals_DifferentName_ReturnsFalse()
    {
        ImportedFolder f1 = new ImportedFolder { Id = 1, Name = "Folder A" };
        ImportedFolder f2 = new ImportedFolder { Id = 1, Name = "Folder B" };

        Assert.False(f1.Equals(f2));
    }

    [Fact]
    public void ImportedFolder_Equals_DifferentId_ReturnsFalse()
    {
        ImportedFolder f1 = new ImportedFolder { Id = 1, Name = "Folder" };
        ImportedFolder f2 = new ImportedFolder { Id = 2, Name = "Folder" };

        Assert.False(f1.Equals(f2));
    }

    [Fact]
    public void ImportedFolder_Equals_DifferentUrls_ReturnsFalse()
    {
        ImportedFolder f1 = new ImportedFolder { Id = 1, urls = new List<string> { "https://a.com" } };
        ImportedFolder f2 = new ImportedFolder { Id = 1, urls = new List<string> { "https://b.com" } };

        Assert.False(f1.Equals(f2));
    }

    [Fact]
    public void ImportedFolder_Equals_DifferentChildren_ReturnsFalse()
    {
        ImportedFolder f1 = new ImportedFolder { Id = 1, ChildrenIds = new List<int> { 2 } };
        ImportedFolder f2 = new ImportedFolder { Id = 1, ChildrenIds = new List<int> { 3 } };

        Assert.False(f1.Equals(f2));
    }
}