using Xunit;
using Nfbookmark;

namespace bookmark_dlp.Tests;

public class AutoImportLinkFromUrlTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", Linktype.Video, "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/abcdef12345", Linktype.Short, "abcdef12345")]
    [InlineData("https://www.youtube.com/@MrBeast", Linktype.Channel_at, "MrBeast")]
    [InlineData("https://www.youtube.com/playlist?list=PL_z_8CaS__t-sk_111111sYf-i3wE1S-l", Linktype.Playlist, "PL_z_8CaS__t-sk_111111sYf-i3wE1S-l")]
    [InlineData("https://www.youtube.com/c/SomeChannelName", Linktype.Channel_c, "SomeChannelName")]
    [InlineData("https://www.youtube.com/channel/UC-lHJZR3Gqxm24_Vd_AJ5Yw", Linktype.Channel_channel, "UC-lHJZR3Gqxm24_Vd_AJ5Yw")]
    [InlineData("https://www.youtube.com/user/oldchannel", Linktype.Channel_user, "oldchannel")]
    public void LinkFromUrl_WithValidYouTubeUrls_ParsesCorrectly(string url, Linktype expectedType, string expectedId)
    {
        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedType, result.Value.linktype);
        Assert.Equal(expectedId, result.Value.yt_id);
    }
    
    [Fact]
    public void LinkFromUrl_WithNonYouTubeUrl_ReturnsNull()
    {
        // Arrange
        var url = "https://www.google.com";

        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void LinkFromUrl_WithMalformedYouTubeUrl_ThrowsInvalidLinkException()
    {
        // Arrange
        var url = "https://www.youtube.com/watch?v=";

        // Act & Assert
        Assert.Throws<InvalidLinkException>(() => AutoImport.LinkFromUrl(url));
    }

    [Fact]
    public void LinkFromUrl_WithMalformedShortsUrl_ThrowsInvalidLinkException()
    {
        // Arrange
        var url = "https://www.youtube.com/shorts/";

        // Act & Assert
        Assert.Throws<InvalidLinkException>(() => AutoImport.LinkFromUrl(url));
    }
}