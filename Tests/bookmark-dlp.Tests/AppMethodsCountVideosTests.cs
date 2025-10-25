using Xunit;
using Nfbookmark;
using System.Collections.Generic;

namespace bookmark_dlp.Tests;

public class AppMethodsCountVideosTests
{
    [Fact]
    public void CountWantedVideos_WithOnlyDirectLinks_CalculatesCorrectly()
    {
        // Arrange
        var folders = new List<Folderclass>
        {
            new Folderclass
            {
                links = new List<YTLink>
                {
                    new YTLink { linktype = Linktype.Video },
                    new YTLink { linktype = Linktype.Short },
                    new YTLink { linktype = Linktype.Video }
                }
            }
        };

        // Act
        AppMethods.CountWantedVideos(ref folders);

        // Assert
        var folder = folders[0];
        Assert.Equal(3, folder.downloadStatus.NumberOfVideosDirectlyWanted);
        Assert.Equal(0, folder.downloadStatus.NumberOfVideosIndirectlyWanted);
    }

    [Fact]
    public void CountWantedVideos_WithOnlyIndirectLinks_CalculatesCorrectly()
    {
        // Arrange
        var folders = new List<Folderclass>
        {
            new Folderclass
            {
                links = new List<YTLink>
                {
                    new YTLink { linktype = Linktype.Playlist, member_ids = new List<string> { "p1", "p2", "p3" } },
                    new YTLink { linktype = Linktype.Channel_c, member_ids = new List<string> { "c1", "c2" } }
                }
            }
        };

        // Act
        AppMethods.CountWantedVideos(ref folders);

        // Assert
        var folder = folders[0];
        Assert.Equal(0, folder.downloadStatus.NumberOfVideosDirectlyWanted);
        Assert.Equal(5, folder.downloadStatus.NumberOfVideosIndirectlyWanted); // 3 from playlist + 2 from channel
    }

    [Fact]
    public void CountWantedVideos_WithMixedLinks_CalculatesCorrectly()
    {
        // Arrange
        var folders = new List<Folderclass>
        {
            new Folderclass
            {
                links = new List<YTLink>
                {
                    // Direct links
                    new YTLink { linktype = Linktype.Video },
                    new YTLink { linktype = Linktype.Short },

                    // Indirect links
                    new YTLink { linktype = Linktype.Playlist, member_ids = new List<string> { "p1", "p2", "p3", "p4" } },
                    new YTLink { linktype = Linktype.Channel_at, member_ids = new List<string> { "c1" } },
                    
                    // Other link types that should be ignored
                    new YTLink { linktype = Linktype.Search }
                }
            }
        };

        // Act
        AppMethods.CountWantedVideos(ref folders);

        // Assert
        var folder = folders[0];
        Assert.Equal(2, folder.downloadStatus.NumberOfVideosDirectlyWanted);
        Assert.Equal(5, folder.downloadStatus.NumberOfVideosIndirectlyWanted); // 4 from playlist + 1 from channel
    }
}