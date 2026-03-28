using Xunit;

namespace bookmark_dlp.Tests;

public class YtdlpInterfacingTests
{
    #region PlaylistIdExtractorTests

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=12345678912&list=PL123456789123456789-4568789123&index=1",
        "PL123456789123456789-4568789123")]
    [InlineData("https://www.youtube.com/watch?v=abcd1234", null)]
    [InlineData("https://www.youtube.com/watch?list=PL9876543210987654321", "PL9876543210987654321")]
    [InlineData("https://youtube.com/watch?v=abcd&list=PL123&feature=share", "PL123")]
    [InlineData("https://www.youtube.com/playlist?list=PL456789123", "PL456789123")]
    public void ExtractPlaylistId_ValidAndInvalidUrls_ReturnsExpectedResult(string url, string expectedPlaylistId)
    {
        // Act
        string result = YtdlpInterfacing.ExtractPlaylistId(url);

        // Assert
        Assert.Equal(expectedPlaylistId, result);
    }

    [Fact]
    public void ExtractPlaylistId_EmptyUrl_ReturnsNull()
    {
        // Arrange
        string url = string.Empty;

        // Act
        string result = YtdlpInterfacing.ExtractPlaylistId(url);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractPlaylistId_NullUrl_ReturnsNull()
    {
        // Arrange
        string url = null;

        // Act
        string result = YtdlpInterfacing.ExtractPlaylistId(url);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractPlaylistId_InvalidUrlFormat_ReturnsNull()
    {
        // Arrange
        string url = "not-a-valid-url";

        // Act
        string result = YtdlpInterfacing.ExtractPlaylistId(url);

        // Assert
        Assert.Null(result);
    }

    #endregion PlaylistIdExtractorTests
}