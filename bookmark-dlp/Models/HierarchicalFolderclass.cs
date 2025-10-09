using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;


namespace bookmark_dlp.Models;
public partial class HierarchicalFolderclass : ObservableObject
{
    [ObservableProperty] private int _startline; //for html: the line number in which the folder starts in the html.
                    //json(autoimport intake chrome): the folder id, same as the folder[totalyoutubelinknumber] index.
                    //firefox-sql: the bookmark id of the folder in the sql db
    [ObservableProperty] private string _name;
    [ObservableProperty] private int _depth;
    [ObservableProperty] private int _endingline;
    [ObservableProperty] private string _folderpath;
    [ObservableProperty] private List<YTLink> _linksWithMissingVideos;
    [ObservableProperty] private List<YTLink> _linksWithNoMissingVideos;
    [ObservableProperty] private List<string> _urls;
    [ObservableProperty] private List<YTLink> _links;
    [ObservableProperty] private int _id; //same as array index
    [ObservableProperty] private int _parentId;
    [ObservableProperty] private List<int> _childrenIds;
    [ObservableProperty] private bool _wantDownloaded;
    [ObservableProperty] private int _numberOfVideosDirectlyWanted;
    [ObservableProperty] private int _numberOfVideosIndirectlyWanted;
    [ObservableProperty] private int _numberOfDirectlyWantedVideosFound;
    [ObservableProperty] private int _numberOfIndirectlyWantedVideosFound;
    [ObservableProperty] private int _numberOfOtherVideosFound;
    
    [ObservableProperty] private bool _hasChildren;
    [ObservableProperty] private bool _isExpanded = false;
    internal ObservableCollection<HierarchicalFolderclass>? _children;
    /// <summary>
    /// Size of all videos wanted by folder (not by its children) in bytes 
    /// </summary>
    [ObservableProperty] private int _estimatedSize;
    
    /// <summary>
    /// The folders that are children to the given folder are in this list. Hierarchical, tree structure.
    /// </summary>
    public IReadOnlyList<HierarchicalFolderclass>? Children => _children;
    
    /// <summary>
    /// Recursive constructor
    /// </summary>
    /// <param name="other"></param>
    // ReSharper disable once ConvertToPrimaryConstructor
    public HierarchicalFolderclass(Folderclass other)
    {
        _startline = other.startline;
        _name = other.name;
        _depth = other.depth;
        _endingline = other.endingline;
        _folderpath = other.folderpath;
        _linksWithMissingVideos = other.LinksWithMissingVideos;
        _linksWithNoMissingVideos = other.LinksWithNoMissingVideos;
        _urls = other.urls;
        _links = other.links;
        _id = other.id;
        _parentId = other.parentId;
        _childrenIds = other.childrenIds;
        _wantDownloaded = other.wantDownloaded;
        _numberOfVideosDirectlyWanted = other.numberOfVideosDirectlyWanted;
        _numberOfVideosIndirectlyWanted = other.numberOfVideosIndirectlyWanted;
        _numberOfDirectlyWantedVideosFound = other.numberOfDirectlyWantedVideosFound;
        _numberOfIndirectlyWantedVideosFound = other.numberOfIndirectlyWantedVideosFound;
        _numberOfOtherVideosFound = other.numberOfOtherVideosFound;
        _children = new ObservableCollection<HierarchicalFolderclass>();
        _hasChildren = other.childrenIds.Count > 0;
        _estimatedSize = 0;
    }
    
    public static Comparison<HierarchicalFolderclass?> SortAscending<T>(Func<HierarchicalFolderclass, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return -1;
            else if (y is null)
                return 1;
            else
                return Comparer<T>.Default.Compare(selector(x), selector(y));
        };
    }

    public static Comparison<HierarchicalFolderclass?> SortDescending<T>(Func<HierarchicalFolderclass, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            if (x is null)
                return 1;
            if (y is null)
                return -1;
            return Comparer<T>.Default.Compare(selector(y), selector(x));
        };
    }

    public override string ToString()
    {
        return $"Name:{Name}, id:{Id}, depth:{Depth}, number of urls:{Urls.Count}";
    }
}