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
        [ObservableProperty] private int _numberoflinks;
        [ObservableProperty] private int _numberofmissinglinks;
        [ObservableProperty] private List<string> _urls;
        [ObservableProperty] private int _id; //same as array index
        [ObservableProperty] private int _parent;
        [ObservableProperty] private bool _wantDownloaded;
        [ObservableProperty] private int _numberOfVideosDirectlyWanted;
        [ObservableProperty] private int _numberOfVideosIndirectlyWanted;
        [ObservableProperty] private int _numberOfVideosAllWanted;
        [ObservableProperty] private int _numberOfWantedVideosFound;
        [ObservableProperty] private int _numberOfOtherVideosFound;
        [ObservableProperty] private int _numberOfAllVideosFound;
        [ObservableProperty] private List<YTLink> _missinglinks;
        [ObservableProperty] private List<string> _missingurls;
        [ObservableProperty] private List<YTLink> _foundlinks;
        [ObservableProperty] private List<string> _foundurls;
        
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
            _numberoflinks = other.numberoflinks;
            _numberofmissinglinks = other.numberofmissinglinks;
            _urls = other.urls;
            _id = other.id;
            _parent = other.parent;
            _wantDownloaded = other.wantDownloaded;
            _numberOfVideosDirectlyWanted = other.numberOfVideosDirectlyWanted;
            _numberOfVideosIndirectlyWanted = other.numberOfVideosIndirectlyWanted;
            _numberOfVideosAllWanted = other.numberOfVideosAllWanted;
            _numberOfWantedVideosFound = other.numberOfWantedVideosFound;
            _numberOfOtherVideosFound = other.numberOfOtherVideosFound;
            _numberOfAllVideosFound = other.numberOfAllVideosFound;
            _missinglinks = other.missinglinks;
            _missingurls = other.missingurls;
            _foundlinks = other.foundlinks;
            _foundurls = other.foundurls;
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