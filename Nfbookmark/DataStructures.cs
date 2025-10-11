using System;
using System.Collections.Generic;
using System.Linq;

namespace Nfbookmark
{
    /// <summary>
    ///     Not used for now. A more refined wrapper around just a list of folders, intended to represent all bookmarks found
    ///     in a single browser profile.
    ///     Feels a bit redundant when Bookmark already exists.
    /// </summary>
    public class Bookmarks
    {
        public List<Folderclass> folders = new List<Folderclass>();
        public int numberofurlbookmarks;
        public int numberofbookmarks;
        
        public Bookmarks() { numberofbookmarks = numberofurlbookmarks + folders.Count; }
    }
    
    /// <summary>
    ///     Denotes what kind of youtube structure is linked.
    /// </summary>
    public enum Linktype 
    {
        /// <summary>
        /// regular video, default
        /// </summary>
        Video,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 4) == "user")
        /// </summary>
        Channel_user,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 7) == "channel")
        /// </summary>
        Channel_channel,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 1) == "@")
        /// </summary>
        Channel_at,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 2) == "c/")
        /// </summary>
        Channel_c,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 6) == "shorts")
        /// </summary>
        Short,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 8) == "playlist")
        /// </summary>
        Playlist,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 7) == "results") //youtube search result was bookmarked
        /// </summary>
        Search 
    }

    /// <summary>
    /// Youtube links with types and ids
    /// </summary>
    public struct YTLink
    {
        public string url;
        public Linktype linktype;
        public string yt_id;
        // public string channel_id;
        // public string playlist_id;
        /// <summary>
        /// Contains yt ids of videos in the playlist or uploaded by the channel. Only used it linktype is Channel_* or Playlist. 
        /// </summary>
        public List<string> member_ids;
        public List<string> member_ids_found;
        public List<string> member_ids_not_found;

        public override string ToString()
        {
            return $"Url: {url}, linktype: {linktype.ToString()}, yt_id: {yt_id}";
        }

        public override bool Equals(object obj)
        {
            if (obj is YTLink other)
            {
                return url == other.url &&
                       linktype == other.linktype &&
                       yt_id == other.yt_id &&
                       Enumerable.SequenceEqual(member_ids ?? new List<string>(), other.member_ids ?? new List<string>()) &&
                       Enumerable.SequenceEqual(member_ids_found ?? new List<string>(), other.member_ids_found ?? new List<string>()) &&
                       Enumerable.SequenceEqual(member_ids_not_found ?? new List<string>(), other.member_ids_not_found ?? new List<string>());
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                url,
                linktype,
                yt_id,
                member_ids != null ? string.Join(",", member_ids).GetHashCode() : 0,
                member_ids_found != null ? string.Join(",", member_ids_found).GetHashCode() : 0,
                member_ids_not_found != null ? string.Join(",", member_ids_not_found).GetHashCode() : 0
            );
        }

        public static bool operator ==(YTLink link1, YTLink link2)
        {
            return link1.Equals(link2);
        }

        public static bool operator !=(YTLink link1, YTLink link2)
        {
            return !(link1 == link2);
        }
    }

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

    /// <summary>
    ///     One bookmark. Only used when importing from SQL or JSON (not for html)
    /// </summary>
    public class Bookmark
    {
        public List<Bookmark> children = new List<Bookmark>(); //only where type = folder
        public long date_added;
        public long date_last_used;
        public long date_modified; //only where type = folder
        public string guid;
        public int id;
        public string name;
        public string type;
        public string url; //only where type = url

        public override string ToString()
        {
            return $"name:{name} type:{type}, id:{id}";
        }
    }

    /// <summary>
    /// Representing the default locations (filepaths) where one browser might store its
    /// user profiles.
    /// </summary>
    public partial class BrowserLocations
    {
        public string browsername;
        public string windows_profilespath = "";
        public List<string> linux_profilespath = new List<string>();
        public List<string> osx_profilespath = new List<string>();
        public List<string> hardcodedpaths = new List<string>();
        /// <summary>
        /// List of paths to FILES containing bookmarks (one file for one browser profile usually)
        /// </summary>
        public List<string> foundProfiles = new List<string>();
        /// <summary>
        /// Type of this browser: Chromium of Firefox based
        /// </summary>
        public BrowserType browserType = BrowserType.none;

        public override string ToString()
        {
            return $"Name:{browsername}, found profiles:{foundProfiles.ToString()}";
        }
    }

    /// <summary>
    /// Type: Chromium of Firefox based
    /// </summary>
    public enum BrowserType { none, chromiumbased, firefoxbased }

    /// <summary>
    ///     Legacy code, but still used. Basically globaly variables but in a struct that is passed to functions as necessary.
    /// </summary>
    internal struct GlobalState
    {
        public int folderid; //used a lot instead of numberoffolders, maybe not ideal?
        //public int totalyoutubelinknumber;
        //public int startline;
        //public int depth;
        public int endingline;
    }
    
    public class InvalidLinkException : Exception
    {
        public InvalidLinkException()
        {
        }

        public InvalidLinkException(string message)
            : base(message)
        {
        }

        public InvalidLinkException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}