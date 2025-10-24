using System.Collections.Generic;
using System.Linq;

namespace Nfbookmark
{
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
            return System.HashCode.Combine(
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
    
    public class InvalidLinkException : System.Exception
    {
        public InvalidLinkException()
        {
        }

        public InvalidLinkException(string message)
            : base(message)
        {
        }

        public InvalidLinkException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}