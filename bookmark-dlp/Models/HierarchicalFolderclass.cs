using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;


namespace bookmark_dlp.Models
{
    public partial class HierarchicalFolderclass : ObservableObject
    {
        //TODO: Not in agreement with Folderclass, eg. Children and children are different types. 
        [ObservableProperty] private int _startline; //for html: the line number in which the folder starts in the html.
                        //json(autoimport intake chrome): the folder id, same as the folder[totalyoutubelinknumber] index.
                        //firefox-sql: the bookmark id of the folder in the sql db
        [ObservableProperty] private string _name;
        [ObservableProperty] private int _depth;
        [ObservableProperty] private int _endingline;
        [ObservableProperty] private string _folderpath;
        [ObservableProperty] private List<string> _urls;
        [ObservableProperty] private int _id; //same as array index
        [ObservableProperty] private int _parent;
        [ObservableProperty] private bool _wantDownloaded;
        [ObservableProperty] private int _numberOfVideosDirectlyWanted;
        [ObservableProperty] private int _numberOfVideosIndirectlyWanted;
        [ObservableProperty] private int _numberOfOtherVideosFound;
        [ObservableProperty] private List<YTLink> _missinglinks;
        [ObservableProperty] private List<YTLink> _foundlinks;
        
        [ObservableProperty] private bool _hasChildren = false;
        [ObservableProperty] private bool _isExpanded = false;
        internal ObservableCollection<HierarchicalFolderclass>? _children;
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
            _urls = other.urls;
            _id = other.id;
            _parent = other.parent;
            _wantDownloaded = other.wantDownloaded;
            _numberOfVideosDirectlyWanted = other.numberOfVideosDirectlyWanted;
            _numberOfVideosIndirectlyWanted = other.numberOfVideosIndirectlyWanted;
            _numberOfOtherVideosFound = other.numberOfOtherVideosFound;
            _missinglinks = other.LinksWithMissingVideos;
            _foundlinks = other.LinksWithNoMissingVideos;
            _children = new ObservableCollection<HierarchicalFolderclass>();
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
                else if (x is null)
                    return 1;
                else if (y is null)
                    return -1;
                else
                    return Comparer<T>.Default.Compare(selector(y), selector(x));
            };
        }
        
        public override string ToString()
        {
            return $"Name:{Name}, id:{Id}, depth:{Depth}, number of urls:{Urls.Count}";
        }
    }    
}