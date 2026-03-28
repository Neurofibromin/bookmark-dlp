using System;
using System.Collections.Generic;
using System.Linq;

namespace Nfbookmark;

public class ImportedFolder : IEquatable<ImportedFolder>
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
    public string Name;
    /// <summary>
    /// Depth of folder compared to other folders. Root has a depth of 0.
    /// </summary>
    public int Depth;
    /// <summary>
    /// The id of current folder
    /// </summary>
    public int Id; //same as list index in the List<Folderclass>
    /// <summary>
    /// The id of the parent folder of current folder
    /// </summary>
    public int ParentId;
    /// <summary>
    /// List of all the URLs in the given folder. May be empty.
    /// </summary>
    public List<string> urls = new List<string>();
    /// <summary>
    /// The ids of children folders of current folder
    /// </summary>
    public List<int> ChildrenIds = new List<int>();

    public ImportedFolder() {}

    public ImportedFolder(int lstartLine, string lname, int ldepth, int lid, int lparentId, List<string> lurls,
        List<int> lchildrenIds)
    {
        StartLine = lstartLine;
        Name = lname;
        Depth = ldepth;
        Id = lid;
        ParentId = lparentId;
        urls = lurls;
        ChildrenIds = lchildrenIds;
    }

    public ImportedFolder(ImportedFolder folder)
    {
        StartLine = folder.StartLine;
        Name = folder.Name;
        Id = folder.Id;
        Depth = folder.Depth;
        ParentId = folder.ParentId;
        ChildrenIds = new List<int>(folder.ChildrenIds);
        urls = new List<string>(folder.urls);
    }

    public bool Equals(ImportedFolder? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return StartLine == other.StartLine
               && Name == other.Name
               && Depth == other.Depth
               && Id == other.Id
               && ParentId == other.ParentId
               && urls.SequenceEqual(other.urls)
               && ChildrenIds.SequenceEqual(other.ChildrenIds);
    }

    public override bool Equals(object? obj) => Equals(obj as ImportedFolder);

    public override int GetHashCode() =>
        HashCode.Combine(StartLine, Name, Depth, Id, ParentId);

    public static bool operator ==(ImportedFolder? left, ImportedFolder? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ImportedFolder? left, ImportedFolder? right) =>
        !(left == right);

    public override string ToString()
    {
        return $"Name:{Name}, id:{Id}, depth:{Depth}, number of urls:{urls.Count}";
    }
}