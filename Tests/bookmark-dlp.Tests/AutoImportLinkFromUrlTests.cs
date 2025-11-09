using System;
using Nfbookmark;
using Xunit;

namespace bookmark_dlp.Tests;

public class AutoImportLinkFromUrlTests
{
    // A. Tests for non-YouTube URLs or invalid formats
    // =================================================================

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("https://vimeo.com/12345678")]
    [InlineData("http://dailymotion.com/video/abc123")]
    [InlineData("Just some random text")]
    [InlineData("")]
    [InlineData(null)]
    public void LinkFromUrl_ShouldReturnNull_ForNonYouTubeUrls(string url)
    {
        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]      // Mobile URL
    [InlineData("https://music.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")] // Music URL
    [InlineData("http://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]   // HTTP instead of HTTPS
    public void LinkFromUrl_ShouldThrowException_ForUnsupportedYouTubeUrlFormats(string url, string expectedId)
    {
        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Linktype.Video, result.Value.linktype);
        Assert.Equal(expectedId, result.Value.yt_id);
    }
    
    // B. Tests for Standard Video URLs (/watch?v=...)
    // =================================================================

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")] // Standard video
    [InlineData("https://www.youtube.com/watch?v=y_Z_ds-y-fI&t=120s", "y_Z_ds-y-fI")] // With timestamp
    [InlineData("https://www.youtube.com/watch?v=fffggg2cccd&list=PL_z_8CaS__t-sk_111111sYf-i3wE1S-l&index=5", "fffggg2cccd")] // Video within a playlist
    [InlineData("https://www.youtube.com/watch?v=E-EffeFE_12&feature=youtu.be", "E-EffeFE_12")] // With feature parameter
    public void LinkFromUrl_ShouldParseVideoUrlsCorrectly(string url, string expectedId)
    {
        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Linktype.Video, result.Value.linktype);
        Assert.Equal(expectedId, result.Value.yt_id);
    }

    // C. Tests for YouTube Shorts URLs (/shorts/...)
    // =================================================================

    [Theory]
    [InlineData("https://www.youtube.com/shorts/abcdef12345", "abcdef12345")] // Standard short
    [InlineData("https://www.youtube.com/shorts/ZYz-z24Y2zY?feature=share", "ZYz-z24Y2zY")] // With feature parameter
    public void LinkFromUrl_ShouldParseShortsUrlsCorrectly(string url, string expectedId)
    {
        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Linktype.Short, result.Value.linktype);
        Assert.Equal(expectedId, result.Value.yt_id);
    }

    // D. Tests for various Channel URL formats
    // =================================================================

    [Theory]
    [InlineData("https://www.youtube.com/@MrBeast", Linktype.Channel_at, "MrBeast")] // Handle
    [InlineData("https://www.youtube.com/@MrBeast/videos", Linktype.Channel_at, "MrBeast")] // Handle with /videos suffix
    [InlineData("https://www.youtube.com/c/SomeChannelName", Linktype.Channel_c, "SomeChannelName")] // Custom URL
    [InlineData("https://www.youtube.com/c/SomeChannelName/videos", Linktype.Channel_c, "SomeChannelName")] // Custom URL
    [InlineData("https://www.youtube.com/channel/UC-lHJZR3Gqxm24_Vd_AJ5Yw", Linktype.Channel_channel, "UC-lHJZR3Gqxm24_Vd_AJ5Yw")] // Channel ID
    [InlineData("https://www.youtube.com/channel/UC-lHJZR3Gqxm24_Vd_AJ5Yw/videos", Linktype.Channel_channel, "UC-lHJZR3Gqxm24_Vd_AJ5Yw")] // Channel ID
    [InlineData("https://www.youtube.com/user/oldchannelname", Linktype.Channel_user, "oldchannelname")] // Legacy username
    [InlineData("https://www.youtube.com/user/oldchannelname/videos", Linktype.Channel_user, "oldchannelname")] // Legacy username
    public void LinkFromUrl_ShouldParseAllChannelTypesCorrectly(string url, Linktype expectedType, string expectedId)
    {
        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedType, result.Value.linktype);
        Assert.Equal(expectedId, result.Value.yt_id);
    }
    
    // E. Tests for Playlist URLs
    // =================================================================
    
    [Theory]
    [InlineData("https://www.youtube.com/playlist?list=PL_z_8CaS__t-sk_111111sYf-i3wE1S-l", "PL_z_8CaS__t-sk_111111sYf-i3wE1S-l")] // Standard playlist
    [InlineData("https://www.youtube.com/playlist?list=OLAK1uy_kfe234n5m6JFVV78t1YTE9P_R0A1-23bA&si=some_other_param", "OLAK1uy_kfe234n5m6JFVV78t1YTE9P_R0A1-23bA")] // Playlist with extra params
    public void LinkFromUrl_ShouldParsePlaylistUrlsCorrectly(string url, string expectedId)
    {
        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Linktype.Playlist, result.Value.linktype);
        Assert.Equal(expectedId, result.Value.yt_id);
    }
    
    // F. Tests for other URL types
    // =================================================================

    [Fact]
    public void LinkFromUrl_ShouldIdentifySearchUrlCorrectly()
    {
        // Arrange
        var url = "https://www.youtube.com/results?search_query=testing";

        // Act
        var result = AutoImport.LinkFromUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Linktype.Search, result.Value.linktype);
        Assert.Null(result.Value.yt_id); // Search results don't have a singular ID
    }

    // G. Tests for malformed but potentially valid YouTube URLs (should throw)
    // =================================================================

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=")]
    [InlineData("https://www.youtube.com/watch?invalidparam=123")]
    [InlineData("https://www.youtube.com/shorts/")]
    [InlineData("https://www.youtube.com/@/")]
    [InlineData("https://www.youtube.com/c/")]
    [InlineData("https://www.youtube.com/channel/")]
    [InlineData("https://www.youtube.com/user/")]
    [InlineData("https://www.youtube.com/playlist?list=")]
    public void LinkFromUrl_ShouldThrowInvalidLinkException_ForMalformedUrls(string url)
    {
        // Act & Assert
        Assert.Null(AutoImport.LinkFromUrl(url));
    }
    
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
        Assert.Null(AutoImport.LinkFromUrl(url));
    }

    [Fact]
    public void LinkFromUrl_WithMalformedShortsUrl_ThrowsInvalidLinkException()
    {
        // Arrange
        var url = "https://www.youtube.com/shorts/";

        // Act & Assert
        Assert.Null(AutoImport.LinkFromUrl(url));
    }
}