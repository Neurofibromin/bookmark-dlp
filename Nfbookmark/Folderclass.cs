using System.Collections.Generic;
using System.Linq;

namespace Nfbookmark
{
    /// <summary>
    /// The main datatype used by the library, represents one bookmarks folder.
    /// </summary>
    public class Folderclass
    {
        /// <summary>
        /// For HTML: the line number in which the folder starts in the html. <br/>
        /// For JSON: (chromium-based): the folder id, same as the folder[totalyoutubelinknumber] index. <br/>
        /// For SQL: (firefox-based): the bookmark id of the folder in the sql db
        /// </summary>
        public int startline;
        /// <summary>
        /// The name of the folder. Can be empty.
        /// </summary>
        public string name;
        /// <summary>
        /// Depth of folder compared to other folders. Root has a depth of 0.
        /// </summary>
        public int depth;
        /// <summary>
        /// Should only be used in html intake.
        /// </summary>
        public int endingline;
        /// <summary>
        /// The filesystem folder representing the bookmark folder. Not used by default
        /// </summary>
        public string folderpath;
        /// <summary>
        /// All links count: video and yt-short links and playlist and channel <br/>
        /// Channel is missing if a single video from channel is missing. <br/>
        /// Playlist is missing if a single video from playlist is missing.
        /// </summary>
        public List<YTLink> LinksWithMissingVideos = new List<YTLink>();
        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public List<YTLink> LinksWithNoMissingVideos = new List<YTLink>();
        /// <summary>
        /// List of all the URLs in the given folder. May be empty.
        /// </summary>
        public List<string> urls = new List<string>();
        /// <summary>
        /// YTLink list from urls that contains the link type.
        /// </summary>
        public List<YTLink> links = new List<YTLink>();
        /// <summary>
        /// The id of current folder
        /// </summary>
        public int id; //same as list index in the List<Folderclass>
        /// <summary>
        /// The id of the parent folder of current folder
        /// </summary>
        public int parentId;
        /// <summary>
        /// The ids of children folders of current folder
        /// </summary>
        public List<int> childrenIds = new List<int>();
        /// <summary>
        /// Field used by the observable class mostly,
        /// checks if user wants the direct content of the folder downloaded.
        /// Children folders are unaffected.
        /// </summary>
        public bool wantDownloaded = true;
        /// <summary>
        /// Only video and yt-short links, no playlist or channel links count
        /// </summary>
        public int numberOfVideosDirectlyWanted;
        /// <summary>
        /// Only playlist or channel links count, no video and yt-short links
        /// </summary>
        public int numberOfVideosIndirectlyWanted;
        /// <summary>
        /// All wanted videos count, be it playlist or channel links or video or yt-short
        /// </summary>
        public int numberOfDirectlyWantedVideosFound;
        /// <summary>
        /// All wanted videos count, be it playlist or channel links or video or yt-short
        /// </summary>
        public int numberOfIndirectlyWantedVideosFound;
        /// <summary>
        /// Only video files not found in want list now count. <br/>
        /// Eg.
        ///     videos downloaded from channel earlier that have since been deleted, (only channel was bookmarked)
        ///     videos that were downloaded but later unbookmarked <br/>
        /// But not:
        ///     videos that were bookmarked and downloaded and are still bookmarked, but no longer can be downloaded because they were removed from youtube 
        /// </summary>
        public int numberOfOtherVideosFound;
        
        public override string ToString()
        {
            return $"Name:{name}, id:{id}, depth:{depth}, number of urls:{urls.Count}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Folderclass other)
            {
                return id == other.id &&
                       parentId == other.parentId &&
                       depth == other.depth &&
                       startline == other.startline &&
                       endingline == other.endingline &&
                       folderpath == other.folderpath &&
                       wantDownloaded == other.wantDownloaded &&
                       numberOfVideosDirectlyWanted == other.numberOfVideosDirectlyWanted &&
                       numberOfVideosIndirectlyWanted == other.numberOfVideosIndirectlyWanted &&
                       numberOfDirectlyWantedVideosFound == other.numberOfDirectlyWantedVideosFound &&
                       numberOfIndirectlyWantedVideosFound == other.numberOfIndirectlyWantedVideosFound &&
                       numberOfOtherVideosFound == other.numberOfOtherVideosFound &&
                       name == other.name &&
                       Enumerable.SequenceEqual(urls, other.urls) &&
                       Enumerable.SequenceEqual(childrenIds, other.childrenIds) &&
                       Enumerable.SequenceEqual(LinksWithMissingVideos, other.LinksWithMissingVideos) &&
                       Enumerable.SequenceEqual(LinksWithNoMissingVideos, other.LinksWithNoMissingVideos) &&
                       Enumerable.SequenceEqual(links, other.links);
            }
            return false;
        }

        protected bool Equals(Folderclass other)
        {
            return startline == other.startline && name == other.name && depth == other.depth &&
                   endingline == other.endingline && folderpath == other.folderpath &&
                   Equals(LinksWithMissingVideos, other.LinksWithMissingVideos) &&
                   Equals(LinksWithNoMissingVideos, other.LinksWithNoMissingVideos) && Equals(urls, other.urls) &&
                   Equals(links, other.links) && id == other.id && parentId == other.parentId &&
                   Equals(childrenIds, other.childrenIds) && wantDownloaded == other.wantDownloaded &&
                   numberOfVideosDirectlyWanted == other.numberOfVideosDirectlyWanted &&
                   numberOfVideosIndirectlyWanted == other.numberOfVideosIndirectlyWanted &&
                   numberOfDirectlyWantedVideosFound == other.numberOfDirectlyWantedVideosFound &&
                   numberOfIndirectlyWantedVideosFound == other.numberOfIndirectlyWantedVideosFound &&
                   numberOfOtherVideosFound == other.numberOfOtherVideosFound;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = startline;
                hashCode = (hashCode * 397) ^ (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ depth;
                hashCode = (hashCode * 397) ^ endingline;
                hashCode = (hashCode * 397) ^ (folderpath != null ? folderpath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LinksWithMissingVideos != null ? LinksWithMissingVideos.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LinksWithNoMissingVideos != null ? LinksWithNoMissingVideos.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (urls != null ? urls.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (links != null ? links.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ id;
                hashCode = (hashCode * 397) ^ parentId;
                hashCode = (hashCode * 397) ^ (childrenIds != null ? childrenIds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ wantDownloaded.GetHashCode();
                hashCode = (hashCode * 397) ^ numberOfVideosDirectlyWanted;
                hashCode = (hashCode * 397) ^ numberOfVideosIndirectlyWanted;
                hashCode = (hashCode * 397) ^ numberOfDirectlyWantedVideosFound;
                hashCode = (hashCode * 397) ^ numberOfIndirectlyWantedVideosFound;
                hashCode = (hashCode * 397) ^ numberOfOtherVideosFound;
                return hashCode;
            }
        }
    }
}