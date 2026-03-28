using System;
using System.Collections.Generic;
using System.Linq;

namespace Nfbookmark
{
    /// <summary>
    /// Youtube links with types and ids
    /// </summary>
    public struct YTLink : IEquatable<YTLink>
    {
        public string url;
        public Linktype linktype;
        public string yt_id;
        // public string channel_id;
        // public string playlist_id;
        /// <summary>
        /// Contains yt ids of videos in the playlist or uploaded by the channel. Only used it linktype is Channel_* or Playlist. 
        /// </summary>
        public List<string> MemberIds;
        public List<string> MemberIdsFound;
        public List<string> MemberIdsNotFound;

        public override string ToString()
        {
            return $"Url: {url}, linktype: {linktype.ToString()}, yt_id: {yt_id}";
        }

        public override bool Equals(object obj)
        {
            return obj is YTLink other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(url, (int)linktype, yt_id);
        }

        public static bool operator ==(YTLink link1, YTLink link2)
        {
            return link1.Equals(link2);
        }

        public static bool operator !=(YTLink link1, YTLink link2)
        {
            return !(link1 == link2);
        }

        public bool Equals(YTLink other)
        {
            if (url != other.url || 
                linktype != other.linktype || 
                yt_id != other.yt_id)
            {
                return false;
            }

            return AreStringListsEqual(MemberIds, other.MemberIds) &&
                   AreStringListsEqual(MemberIdsFound, other.MemberIdsFound) &&
                   AreStringListsEqual(MemberIdsNotFound, other.MemberIdsNotFound);
        }
        
        private static bool AreStringListsEqual(List<string> list1, List<string> list2)
        {
            if (ReferenceEquals(list1, list2)) return true;
            if (list1 is null || list2 is null) return false;
            return list1.SequenceEqual(list2);
        }
    }
    
    public class InvalidLinkException : System.Exception
    {
        public InvalidLinkException() { }
        public InvalidLinkException(string message) : base(message) { }
        public InvalidLinkException(string message, System.Exception inner) : base(message, inner) { }
    }
}