using Xunit;

namespace Nfbookmark.Tests;

public class DataStructuresTests
{
    [Fact]
    public void Bookmarks_FoldersEmptyAtStartTest()
    {
        Bookmarks bookmark = new Bookmarks();
        Assert.Empty(bookmark.folders);
    }


    #region FolderClass

    [Fact]
    public void Folderclass_ChildrenEmptyAtStartTest()
    {
        Folderclass folder = new Folderclass();
        Assert.Empty(folder.childrenIds);
    }

    [Fact]
    public void Folderclass_StringRepresentationTest()
    {
        Folderclass folder = new Folderclass
        {
            startline = 0,
            name = "test",
            depth = 3,
            endingline = 10,
            folderpath = "/one/two/three/four",
            urls = new List<string>(),
            id = 0,
            parentId = 0
        };
        string strrepr = folder.ToString();
        Assert.Equal("Name:test, id:0, depth:3, number of urls:0", strrepr);
    }

    [Fact]
    public void Folderclass_EqualityEmptyTest()
    {
        Folderclass f1 = new Folderclass();
        Folderclass f2 = new Folderclass();
        Assert.True(f1.Equals(f2));
        Assert.True(f1.Equals(f2));
        Assert.True(f2.Equals(f2));
        Assert.True(f2.Equals(f2));
        Assert.True(f1.Equals(f2));
        Assert.True(f1.Equals(f2));

        Assert.True(f1 == f2);
        Assert.True(f2 == f1);
    }

    [Fact]
    public void Folderclass_EqualityNullTest()
    {
        Folderclass f1 = null;
        Folderclass f2 = null;

        Assert.True(f1 == f2);
        Assert.True(f2 == f1);
    }

    [Fact]
    public void Folderclass_EqualityTest_1()
    {
        Folderclass f1 = new Folderclass();
        Folderclass f2 = new Folderclass();
        Assert.True(f1.Equals(f2));
        Assert.True(f1.Equals(f2));
        Assert.True(f2.Equals(f2));
        Assert.True(f2.Equals(f2));
        Assert.True(f1.Equals(f2));
        Assert.True(f1.Equals(f2));

        Assert.True(f1 == f2);
        Assert.True(f2 == f1);
    }

    #endregion FolderClass

    #region YTLink

    [Fact]
    public void YTLink_EqualityEmptyTest()
    {
        YTLink l1 = new YTLink();
        YTLink l2 = new YTLink();
        Assert.True(l1.Equals(l2));
        Assert.True(l1.Equals(l2));
        Assert.True(l2.Equals(l2));
        Assert.True(l2.Equals(l2));
        Assert.True(l2.Equals(l1));
        Assert.True(l2.Equals(l1));
        Assert.Equal(l1.GetHashCode(), l2.GetHashCode());
        Assert.Equal(l1, l2);
        Assert.True(l1 == l2);
    }

    [Fact]
    public void YTLink_EqualityNullTest()
    {
        YTLink? l1 = null;
        YTLink? l2 = null;
        Assert.Equal(l1, l2);
        Assert.True(l1 == l2);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        YTLink link1 = new YTLink
        {
            url = "https://www.youtube.com/playlist?list=123456789123456789",
            linktype = Linktype.Playlist,
            yt_id = "123456789123456789",
            member_ids = new List<string> { "a", "b", "c" },
            member_ids_found = new List<string> { "a", "c" },
            member_ids_not_found = new List<string> { "b" }
        };

        YTLink link2 = new YTLink
        {
            url = "https://www.youtube.com/playlist?list=123456789123456789",
            linktype = Linktype.Playlist,
            yt_id = "123456789123456789",
            member_ids = new List<string> { "a", "b", "c" },
            member_ids_found = new List<string> { "a", "c" },
            member_ids_not_found = new List<string> { "b" }
        };

        Assert.True(link1.Equals(link2));
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        YTLink link1 = new YTLink { url = "https://example.com/1", yt_id = "12345" };
        YTLink link2 = new YTLink { url = "https://example.com/2", yt_id = "54321" };

        Assert.False(link1.Equals(link2));
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        YTLink link1 = new YTLink { url = "https://example.com", yt_id = "12345", linktype = Linktype.Playlist };
        YTLink link2 = new YTLink { url = "https://example.com", yt_id = "12345", linktype = Linktype.Playlist };

        Assert.Equal(link1.GetHashCode(), link2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHash()
    {
        YTLink link1 = new YTLink { url = "https://example.com/1", yt_id = "12345", linktype = Linktype.Playlist };
        YTLink link2 = new YTLink { url = "https://example.com/2", yt_id = "54321", linktype = Linktype.Playlist };

        Assert.NotEqual(link1.GetHashCode(), link2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHash_2()
    {
        YTLink link1 = new YTLink { url = "https://example.com/1", yt_id = "54321", linktype = Linktype.Video };
        YTLink link2 = new YTLink { url = "https://example.com/1", yt_id = "54321", linktype = Linktype.Playlist };

        Assert.NotEqual(link1.GetHashCode(), link2.GetHashCode());
    }

    #endregion YTLink
}