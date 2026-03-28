using Xunit;
using Nfbookmark;
using System.Collections.Generic;

namespace bookmark_dlp.Tests;

public class AppMethodsCountVideosTests
{
    /// <summary>
    /// Helper to create a ResolvedFolder with the given links, hiding the ImportedFolder→MappedFolder→ResolvedFolder chain.
    /// </summary>
    private static ResolvedFolder MakeResolvedFolder(IReadOnlyList<YTLink> links, int id = 0)
    {
        var imported = new ImportedFolder { Id = id, Name = $"TestFolder{id}", Depth = 0 };
        var mapped = new MappedFolder(imported);
        return new ResolvedFolder(mapped, links);
    }

    [Fact]
    public void CountWantedVideos_WithOnlyDirectLinks_CalculatesCorrectly()
    {
        // Arrange
        var folders = new List<ResolvedFolder>
        {
            MakeResolvedFolder(new List<YTLink>
            {
                new YTLink { linktype = Linktype.Video },
                new YTLink { linktype = Linktype.Short },
                new YTLink { linktype = Linktype.Video }
            })
        };

        // Act
        AppMethods.CountWantedVideos(folders);

        // Assert
        var folder = folders[0];
        Assert.Equal(3, folder.DownloadStatus.NumberOfVideosDirectlyWanted);
        Assert.Equal(0, folder.DownloadStatus.NumberOfVideosIndirectlyWanted);
    }

    [Fact]
    public void CountWantedVideos_WithOnlyIndirectLinks_CalculatesCorrectly()
    {
        // Arrange
        var folders = new List<ResolvedFolder>
        {
            MakeResolvedFolder(new List<YTLink>
            {
                new YTLink { linktype = Linktype.Playlist, MemberIds = new List<string> { "p1", "p2", "p3" } },
                new YTLink { linktype = Linktype.Channel_c, MemberIds = new List<string> { "c1", "c2" } }
            })
        };

        // Act
        AppMethods.CountWantedVideos(folders);

        // Assert
        var folder = folders[0];
        Assert.Equal(0, folder.DownloadStatus.NumberOfVideosDirectlyWanted);
        Assert.Equal(5, folder.DownloadStatus.NumberOfVideosIndirectlyWanted); // 3 from playlist + 2 from channel
    }

    [Fact]
    public void CountWantedVideos_WithMixedLinks_CalculatesCorrectly()
    {
        // Arrange
        var folders = new List<ResolvedFolder>
        {
            MakeResolvedFolder(new List<YTLink>
            {
                // Direct links
                new YTLink { linktype = Linktype.Video },
                new YTLink { linktype = Linktype.Short },

                // Indirect links
                new YTLink { linktype = Linktype.Playlist, MemberIds = new List<string> { "p1", "p2", "p3", "p4" } },
                new YTLink { linktype = Linktype.Channel_at, MemberIds = new List<string> { "c1" } },
                    
                // Other link types that should be ignored
                new YTLink { linktype = Linktype.Search }
            })
        };

        // Act
        AppMethods.CountWantedVideos(folders);

        // Assert
        var folder = folders[0];
        Assert.Equal(2, folder.DownloadStatus.NumberOfVideosDirectlyWanted);
        Assert.Equal(5, folder.DownloadStatus.NumberOfVideosIndirectlyWanted); // 4 from playlist + 1 from channel
    }
}