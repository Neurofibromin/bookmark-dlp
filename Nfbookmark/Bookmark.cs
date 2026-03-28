using System.Collections.Generic;

namespace Nfbookmark
{
    /// <summary>
    ///     One bookmark. Only used when importing from SQL or JSON (not for html)
    /// </summary>
    public class Bookmark
    {
        public List<Bookmark> Children = new List<Bookmark>(); //only where type = folder
        public long DateAdded;
        public long DateLastUsed;
        public long DateModified; //only where type = folder
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