using System.Collections.Generic;

namespace Nfbookmark
{
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
}