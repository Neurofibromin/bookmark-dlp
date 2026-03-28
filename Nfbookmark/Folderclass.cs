using System;
using System.Collections.Generic;
using System.Linq;

namespace Nfbookmark
{
    /// <summary>
    /// The main datatype used by the library, represents one bookmarks folder.
    /// </summary>
    public class Folderclass : IEquatable<Folderclass>
    {
        /// <summary>
        /// For HTML: the line number in which the folder starts in the html. <br/>
        /// For JSON: (chromium-based): the folder id, same as the folder[totalyoutubelinknumber] index. <br/>
        /// For SQL: (firefox-based): the bookmark id of the folder in the sql db
        /// </summary>
        public int StartLine;
        /// <summary>
        /// The name of the folder. Can be empty.
        /// </summary>
        public string name;
        /// <summary>
        /// Depth of folder compared to other folders. Root has a depth of 0.
        /// </summary>
        public int depth;
        /// <summary>
        /// The filesystem folder representing the bookmark folder. Not used by default
        /// </summary>
        public string folderpath;
        /// <summary>
        /// List of all the URLs in the given folder. May be empty.
        /// </summary>
        public List<string> urls = new List<string>();
        /// <summary>
        /// YTLink list from urls that contains the link type.
        /// </summary>
        public IReadOnlyList<YTLink> Links = null;
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
        public DownloadStatus downloadStatus { get; set; } = new DownloadStatus();
        
        public Folderclass() { }
        
        /// <summary>
        /// "Copy and Update" constructor for creating a new instance with updated links.
        /// </summary>
        public Folderclass(Folderclass original, IReadOnlyList<YTLink> newLinks)
        {
            this.StartLine = original.StartLine;
            this.name = original.name;
            this.depth = original.depth;
            this.folderpath = original.folderpath;
            this.urls = original.urls;
            this.id = original.id;
            this.parentId = original.parentId;
            this.childrenIds = original.childrenIds;
            this.downloadStatus = original.downloadStatus;
        
            // Use the new value for the 'Links' property
            this.Links = newLinks;
        }
        
        
        public override string ToString()
        {
            return $"Name:{name}, id:{id}, depth:{depth}, number of urls:{urls.Count}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Folderclass)obj);
        }

        public bool Equals(Folderclass other)
        {
            //TODO: nullrefexception can occur here
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            bool start = id == other.id &&
                         parentId == other.parentId &&
                         depth == other.depth &&
                         StartLine == other.StartLine &&
                         folderpath == other.folderpath &&
                         downloadStatus.Equals(other.downloadStatus) &&
                         name == other.name;
            if (urls != null)
                start = start && urls.SequenceEqual(other.urls);
            else
            {
                if (other.urls != null)
                    return  false;
            }
            if (childrenIds != null)
                start = start && childrenIds.SequenceEqual(other.childrenIds);
            else
            {
                if (other.childrenIds != null)
                    return  false;
            }
            if (Links != null)
                start = start && Links.SequenceEqual(other.Links);
            else
            {
                if (other.Links != null)
                    return  false;
            }
            return start;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * 23 + id.GetHashCode();
                hashCode = hashCode * 23 + parentId.GetHashCode();
                hashCode = hashCode * 23 + name.GetHashCode();
                hashCode = hashCode * 23 + depth.GetHashCode();
                hashCode = hashCode * 23 + StartLine.GetHashCode();
                hashCode = hashCode * 23 + (folderpath != null ? folderpath.GetHashCode() : 0);
                hashCode = hashCode * 23 + (downloadStatus != null ? downloadStatus.GetHashCode() : 0);
                if (urls != null)
                {
                    foreach (var url in urls)
                    {
                        hashCode = hashCode * 23 + (url != null ? url.GetHashCode() : 0);
                    }
                }
                if (childrenIds != null)
                {
                    foreach (var childId in childrenIds)
                    {
                        hashCode = hashCode * 23 + childId.GetHashCode();
                    }
                }
                if (Links != null)
                {
                    foreach (var link in Links)
                    {
                        hashCode = hashCode * 23 + link.GetHashCode();
                    }
                }
                return hashCode;
            }
        }
    }
}