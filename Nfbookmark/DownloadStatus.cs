using System.Collections.Generic;
using System.Linq;

namespace Nfbookmark
{
    public class DownloadStatus : System.IEquatable<DownloadStatus>
    {
        /// <summary>
        /// All links count: video and yt-short links and playlist and channel <br/>
        /// Channel is missing if a single video from channel is missing. <br/>
        /// Playlist is missing if a single video from playlist is missing.
        /// </summary>
        public List<YTLink> LinksWithMissingVideos { get; set; } = new List<YTLink>();

        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public List<YTLink> LinksWithNoMissingVideos { get; set; } = new List<YTLink>();

        /// <summary>
        /// Field used by the observable class mostly,
        /// checks if user wants the direct content of the folder downloaded.
        /// Children folders are unaffected.
        /// </summary>
        public bool WantDownloaded { get; set; } = true;

        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public int NumberOfVideosDirectlyWanted { get; set; }

        /// <summary>
        /// Only playlist or channel links count, no video and yt-short links
        /// </summary>
        public int NumberOfVideosIndirectlyWanted { get; set; }

        /// <summary>
        /// All wanted videos count, be it playlist or channel links or video or yt-short
        /// </summary>
        public int NumberOfDirectlyWantedVideosFound { get; set; }

        /// <summary>
        /// All wanted videos count, be it playlist or channel links or video or yt-short
        /// </summary>
        public int NumberOfIndirectlyWantedVideosFound { get; set; }

        /// <summary>
        /// Only video files not found in want list now count. <br/>
        /// Eg.
        ///     videos downloaded from channel earlier that have since been deleted, (only channel was bookmarked)
        ///     videos that were downloaded but later unbookmarked <br/>
        /// But not:
        ///     videos that were bookmarked and downloaded and are still bookmarked, but no longer can be downloaded because they were removed from youtube 
        /// </summary>
        public int NumberOfOtherVideosFound { get; set; }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DownloadStatus)obj);
        }
        
        public bool Equals(DownloadStatus other)
        {
            // Check for null and same instance for performance.
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Compare all relevant properties for value equality.
            return WantDownloaded == other.WantDownloaded &&
                   NumberOfVideosDirectlyWanted == other.NumberOfVideosDirectlyWanted &&
                   NumberOfVideosIndirectlyWanted == other.NumberOfVideosIndirectlyWanted &&
                   NumberOfDirectlyWantedVideosFound == other.NumberOfDirectlyWantedVideosFound &&
                   NumberOfIndirectlyWantedVideosFound == other.NumberOfIndirectlyWantedVideosFound &&
                   NumberOfOtherVideosFound == other.NumberOfOtherVideosFound &&
                   LinksWithMissingVideos.SequenceEqual(other.LinksWithMissingVideos) && //TODO: nullref error here
                   LinksWithNoMissingVideos.SequenceEqual(other.LinksWithNoMissingVideos);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * 23 + WantDownloaded.GetHashCode();
                hashCode = hashCode * 23 + NumberOfVideosDirectlyWanted.GetHashCode();
                hashCode = hashCode * 23 + NumberOfVideosIndirectlyWanted.GetHashCode();
                hashCode = hashCode * 23 + NumberOfDirectlyWantedVideosFound.GetHashCode();
                hashCode = hashCode * 23 + NumberOfIndirectlyWantedVideosFound.GetHashCode();
                hashCode = hashCode * 23 + NumberOfOtherVideosFound.GetHashCode();
                
                if (LinksWithMissingVideos != null)
                {
                    foreach (var item in LinksWithMissingVideos)
                    {
                        hashCode = hashCode * 23 + item.GetHashCode();
                    }
                }
                if (LinksWithNoMissingVideos != null)
                {
                    foreach (var item in LinksWithNoMissingVideos)
                    {
                        hashCode = hashCode * 23 + item.GetHashCode();
                    }
                }
                return hashCode;
            }
        }
    }
}