using Xunit;
using Nfbookmark;
using System.Collections.Generic;

namespace Nfbookmark.Tests;

public class DownloadStatusTests
{
    private YTLink CreateSampleYTLink(string id)
    {
        return new YTLink
        {
            url = $"https://www.youtube.com/watch?v={id}",
            linktype = Linktype.Video,
            yt_id = id
        };
    }

    private DownloadStatus CreateSampleDownloadStatus()
    {
        return new DownloadStatus
        {
            WantDownloaded = true,
            NumberOfVideosDirectlyWanted = 5,
            NumberOfVideosIndirectlyWanted = 10,
            NumberOfDirectlyWantedVideosFound = 3,
            NumberOfIndirectlyWantedVideosFound = 8,
            NumberOfOtherVideosFound = 2,
            LinksWithMissingVideos = new List<YTLink> { CreateSampleYTLink("missing1"), CreateSampleYTLink("missing2") },
            LinksWithNoMissingVideos = new List<YTLink> { CreateSampleYTLink("found1"), CreateSampleYTLink("found2") }
        };
    }

    [Fact]
    public void Equals_WithTwoIdenticalObjects_ShouldReturnTrue()
    {
        // Arrange
        var status1 = CreateSampleDownloadStatus();
        var status2 = CreateSampleDownloadStatus();

        // Act & Assert
        Assert.True(status1.Equals(status2));
        Assert.Equal(status1, status2);
    }

    [Fact]
    public void GetHashCode_WithTwoIdenticalObjects_ShouldBeEqual()
    {
        // Arrange
        var status1 = CreateSampleDownloadStatus();
        var status2 = CreateSampleDownloadStatus();

        // Act & Assert
        Assert.Equal(status1.GetHashCode(), status2.GetHashCode());
    }

    [Fact]
    public void Equals_WhenWantDownloadedDiffers_ShouldReturnFalse()
    {
        // Arrange
        var status1 = CreateSampleDownloadStatus();
        var status2 = CreateSampleDownloadStatus();
        status2.WantDownloaded = false;

        // Act & Assert
        Assert.False(status1.Equals(status2));
    }
    
    [Fact]
    public void Equals_WhenNumberOfVideosDirectlyWantedDiffers_ShouldReturnFalse()
    {
        // Arrange
        var status1 = CreateSampleDownloadStatus();
        var status2 = CreateSampleDownloadStatus();
        status2.NumberOfVideosDirectlyWanted = 99;

        // Act & Assert
        Assert.False(status1.Equals(status2));
    }

    [Fact]
    public void Equals_WhenLinksWithMissingVideosDiffer_ShouldReturnFalse()
    {
        // Arrange
        var status1 = CreateSampleDownloadStatus();
        var status2 = CreateSampleDownloadStatus();
        status2.LinksWithMissingVideos = new List<YTLink> { CreateSampleYTLink("different_missing_video") };

        // Act & Assert
        Assert.False(status1.Equals(status2));
    }

    [Fact]
    public void GetHashCode_WhenObjectsDiffer_ShouldBeDifferent()
    {
        // Arrange
        var status1 = CreateSampleDownloadStatus();
        var status2 = CreateSampleDownloadStatus();
        status2.NumberOfOtherVideosFound = 123;

        // Act & Assert
        Assert.NotEqual(status1.GetHashCode(), status2.GetHashCode());
    }
}