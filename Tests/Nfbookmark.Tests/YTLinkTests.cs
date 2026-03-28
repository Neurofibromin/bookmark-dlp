using Xunit;
using Nfbookmark;

namespace Nfbookmark.Tests;

public class YTLinkTests
{
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
            MemberIds = new List<string> { "a", "b", "c" },
            MemberIdsFound = new List<string> { "a", "c" },
            MemberIdsNotFound = new List<string> { "b" }
        };

        YTLink link2 = new YTLink
        {
            url = "https://www.youtube.com/playlist?list=123456789123456789",
            linktype = Linktype.Playlist,
            yt_id = "123456789123456789",
            MemberIds = new List<string> { "a", "b", "c" },
            MemberIdsFound = new List<string> { "a", "c" },
            MemberIdsNotFound = new List<string> { "b" }
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
}