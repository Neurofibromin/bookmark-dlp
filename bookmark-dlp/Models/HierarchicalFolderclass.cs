using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Nfbookmark;


namespace bookmark_dlp.Models;
public partial class HierarchicalFolderclass : ObservableObject
{
    [ObservableProperty] private int _startline;
    [ObservableProperty] private string _name;
    [ObservableProperty] private int _depth;
    [ObservableProperty] private string? _folderpath;
    [ObservableProperty] private List<YTLink> _linksWithMissingVideos;
    [ObservableProperty] private List<YTLink> _linksWithNoMissingVideos;
    [ObservableProperty] private List<string> _urls;
    [ObservableProperty] private IReadOnlyList<YTLink>? _links;
    [ObservableProperty] private int _id;
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
    /// Constructor from ImportedFolder — used for initial tree display before links are resolved.
    /// Download status fields are initialized to defaults.
    /// </summary>
    public HierarchicalFolderclass(ImportedFolder folder)
    {
        _startline = folder.StartLine;
        _name = folder.Name;
        _depth = folder.Depth;
        _folderpath = null; // not yet mapped to filesystem
        _urls = folder.urls;
        _links = null; // not yet resolved
        _id = folder.Id;
        _parentId = folder.ParentId;
        _childrenIds = folder.ChildrenIds;
        _linksWithMissingVideos = new List<YTLink>();
        _linksWithNoMissingVideos = new List<YTLink>();
        _wantDownloaded = true;
        _numberOfVideosDirectlyWanted = 0;
        _numberOfVideosIndirectlyWanted = 0;
        _numberOfDirectlyWantedVideosFound = 0;
        _numberOfIndirectlyWantedVideosFound = 0;
        _numberOfOtherVideosFound = 0;
        _children = new ObservableCollection<HierarchicalFolderclass>();
        _hasChildren = folder.ChildrenIds.Count > 0;
        _estimatedSize = 0;
    }
    
    public HierarchicalFolderclass(ResolvedFolder folder)
    {
        throw new NotImplementedException();
    }
    
    public static Comparison<HierarchicalFolderclass?> SortAscending<T>(Func<HierarchicalFolderclass, T> selector)
    {
        return (x, y) =>
        {
            if (x is null || y is null) return x is null ? (y is null ? 0 : -1) : 1;
            return Comparer<T>.Default.Compare(selector(x), selector(y));
        };
    }

    public static Comparison<HierarchicalFolderclass?> SortDescending<T>(Func<HierarchicalFolderclass, T> selector)
    {
        return (x, y) =>
        {
            if (x is null || y is null) return x is null ? (y is null ? 0 : 1) : -1;
            return Comparer<T>.Default.Compare(selector(y), selector(x));
        };
    }

    public override string ToString()
    {
        return $"Name:{Name}, id:{Id}, depth:{Depth}, number of urls:{Urls.Count}";
    }
}