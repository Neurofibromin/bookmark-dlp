using Xunit;
using Nfbookmark;
using bookmark_dlp.Models;
using System.Collections.Generic;
using System.Linq;

namespace bookmark_dlp.Tests;

public class HierarchicalFolderclassTests
{
    [Fact]
    public void SortAscending_ByName_SortsCorrectly()
    {
        // Arrange
        var folderC = new HierarchicalFolderclass(new Folderclass { name = "C" });
        var folderA = new HierarchicalFolderclass(new Folderclass { name = "A" });
        var folderB = new HierarchicalFolderclass(new Folderclass { name = "B" });

        var folderList = new List<HierarchicalFolderclass> { folderC, folderA, folderB };

        // Act
        folderList.Sort(HierarchicalFolderclass.SortAscending(f => f.Name));

        // Assert
        Assert.Equal("A", folderList[0].Name);
        Assert.Equal("B", folderList[1].Name);
        Assert.Equal("C", folderList[2].Name);
    }

    [Fact]
    public void SortDescending_ByUrlCount_SortsCorrectly()
    {
        // Arrange
        var folderFewUrls = new HierarchicalFolderclass(new Folderclass 
            { name = "Few", urls = new List<string> { "url1" } });
            
        var folderManyUrls = new HierarchicalFolderclass(new Folderclass 
            { name = "Many", urls = new List<string> { "url1", "url2", "url3", "url4", "url5" } });
            
        var folderSomeUrls = new HierarchicalFolderclass(new Folderclass 
            { name = "Some", urls = new List<string> { "url1", "url2", "url3" } });

        var folderList = new List<HierarchicalFolderclass> { folderFewUrls, folderManyUrls, folderSomeUrls };

        // Act
        folderList.Sort(HierarchicalFolderclass.SortDescending(f => f.Urls.Count));

        // Assert
        Assert.Equal("Many", folderList[0].Name);
        Assert.Equal(5, folderList[0].Urls.Count);
        
        Assert.Equal("Some", folderList[1].Name);
        Assert.Equal(3, folderList[1].Urls.Count);

        Assert.Equal("Few", folderList[2].Name);
        Assert.Single(folderList[2].Urls);
    }
}