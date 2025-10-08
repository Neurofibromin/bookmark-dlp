using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using bookmark_dlp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using bookmark_dlp.Models;
using NfLogger;
using Nfbookmark;

namespace bookmark_dlp.ViewModels
{
    public partial class DownloadingViewModel : ViewModelBase
    {
        [ObservableProperty] private SettingsStruct _activeSettings;
        [ObservableProperty] private string? _fileSource;
        private List<Folderclass>? _folders;
        private ObservableCollection<HierarchicalFolderclass>? HierarchicalFolderCollection { get; set; }
        private static IconConverter? _iconConverter;
        [ObservableProperty] private HierarchicalTreeDataGridSource<HierarchicalFolderclass>? _treeSource;
        
        // public HierarchicalTreeDataGridSource<HierarchicalFolderclass> TreeSource => _treeSource;
        // private ITreeDataGridSource<HierarchicalFolderclass> _source;
        // private HierarchicalFolderclass? _root;
        /*public ITreeDataGridSource<HierarchicalFolderclass> Source
        {
            get => _source;
            private set => this.SetPropertyAndNotifyOnCompletion(ref _source, value);
        }*/
        
        public DownloadingViewModel(IAppSettings appSettings)
        {
            _activeSettings = appSettings.Settings;
        }
        
        public DownloadingViewModel() : this(new AppSettings()) {}

        [RelayCommand]
        private void GetCurrentStatusWithQueryYT()
        {
            // TODO: this
            // At this point DownloadingView is open and the folders are loaded to TreeDataGrid
            if (_folders == null)
                throw new NoNullAllowedException("List<Folderclass> _folders must not be null when starting the status query.");
            // LoadFoldersFromFile() : Import.SmartImport();
            AutoImport.LinksFromUrls(_folders);
            Functions.Createfolderstructure(_folders, ActiveSettings.Outputfolder);
            AppMethods.CountWantedVideos(ref _folders);
            AppMethods.CheckCurrentFilesystemState(ref _folders);
        }
        
        /// <summary>
        /// Starts the import and loads the imported folders to TreeDataGrid
        /// </summary>
        /// <returns>true if successful, false if import failed/empty</returns>
        /// <exception cref="ArgumentNullException">There is no file source available</exception>
        public bool LoadFoldersFromFile()
        {
            if(FileSource == null)
                throw new ArgumentNullException(nameof(FileSource));
            try
            {
                _folders = Import.SmartImport(FileSource);
            }
            catch (Exception e)
            {
                Logger.LogVerbose(e.Message, Logger.Verbosity.Critical);
                throw;
            }
            if (_folders == null)
                return false;
            HierarchicalFolderCollection = AppMethods.GenerateHierarchicalFolderclassesFromList(_folders);
            TreeSource = CreateTreeSource();
            return true;
        }

        #region IconConverter

        private class IconConverter : IMultiValueConverter
        {
            private readonly StreamGeometry _folderExpanded;
            private readonly StreamGeometry _folderCollapsed;
            
            public IconConverter(StreamGeometry folderExpanded, StreamGeometry folderCollapsed)
            {
                _folderExpanded = folderExpanded;
                _folderCollapsed = folderCollapsed;
            }

            public object? Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
            {
                if (values is null || values.Count < 1)
                    return null;
                if (values[0] is bool isExpanded)
                {
                    return isExpanded ? _folderExpanded : _folderCollapsed;
                }
                return null;
            }
        }

        /// <summary>
        /// Implements IconConverter for folder icons of folderIconopen and folderIconopen
        /// </summary>
        public static IMultiValueConverter FileIconConverter
        {
            get
            {
                // Logger.LogVerbose("Getting FileIconConverter", Logger.Verbosity.Trace);
                if (_iconConverter is null)
                {
                    Logger.LogVerbose("FileIconConverter is NULL", Logger.Verbosity.Trace);
                    bool a = Application.Current!.Styles.TryGetResource("folder_regular",theme: Application.Current.ActualThemeVariant , value: out var folderIconregular);
                    bool b = Application.Current.Styles.TryGetResource("folder_open_regular",theme: Application.Current.ActualThemeVariant , value: out var folderIconopen);
                    
                    if (a && b && folderIconopen is StreamGeometry openFolderGeometry && folderIconregular is StreamGeometry regularFolderGeometry)
                    {
                        Logger.LogVerbose("FileIconConverter found", Logger.Verbosity.Trace);
                        _iconConverter = new IconConverter(openFolderGeometry, regularFolderGeometry);
                    }
                    else
                    {
                        if (a || b)
                        {
                            if (folderIconopen is StreamGeometry openFolderGeometry2)
                            {
                                Logger.LogVerbose("Only folderIconopen found", Logger.Verbosity.Error);
                                _iconConverter = new IconConverter(openFolderGeometry2, new StreamGeometry());
                            }
                            else if (folderIconregular is StreamGeometry regularFolderGeometry2)
                            {
                                Logger.LogVerbose("Only folderIconregular found", Logger.Verbosity.Error);
                                _iconConverter = new IconConverter(new StreamGeometry(), regularFolderGeometry2);
                            }
                            else
                            {
                                Logger.LogVerbose("Failed to load folder icons. Using default values.", Logger.Verbosity.Error);
                                _iconConverter = new IconConverter(new StreamGeometry(), new StreamGeometry()); // Provide default values
                            }
                        }
                        else
                        {
                            Logger.LogVerbose("Failed to load folder icons. Using default values.", Logger.Verbosity.Error);
                            _iconConverter = new IconConverter(new StreamGeometry(), new StreamGeometry()); // Provide default values
                        }
                    }

                    
                    /*if (a && b)
                        Logger.LogVerbose("FileIconConverter found", Logger.Verbosity.Trace);
                    else
                        Logger.LogVerbose("FileIconConverter not found", Logger.Verbosity.Error);
                    Logger.LogVerbose("Found folder icon resources.", Logger.Verbosity.Trace);
                    _iconConverter = new IconConverter((StreamGeometry) folderIconopen, (StreamGeometry) folderIconregular);*/
                }
                return _iconConverter;
            }
        }
        #endregion

        #region TreeDataGrid
        
        private ITreeDataGridRowSelectionModel<HierarchicalFolderclass> GetRowSelection(ITreeDataGridSource source)
        {
            return source.Selection as ITreeDataGridRowSelectionModel<HierarchicalFolderclass> ??
                   throw new InvalidOperationException("Expected a row selection model.");
        }
        
        private HierarchicalTreeDataGridSource<HierarchicalFolderclass> CreateTreeSource()
        {
            if (HierarchicalFolderCollection is null)
                throw new InvalidOperationException("Hierarchical folder collection is null.");
            var result = new HierarchicalTreeDataGridSource<HierarchicalFolderclass>(HierarchicalFolderCollection)
            {
                Columns =
                {
                    new CheckBoxColumn<HierarchicalFolderclass>(
                        null,
                        x => x.WantDownloaded,
                        (o, v) => o.WantDownloaded = v,
                        options: new()
                        {
                            CanUserResizeColumn = false,
                        }),
                    new HierarchicalExpanderColumn<HierarchicalFolderclass>(
                        new TemplateColumn<HierarchicalFolderclass>(
                            "Name",
                            "FileNameCell",
                            width: new GridLength(1, GridUnitType.Star),
                            options: new()
                            {
                                CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Name),
                                CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Name),
                                IsTextSearchEnabled = true,
                                TextSearchValueSelector = x => x.Name
                            }),
                        x => x.Children,
                        x => x.HasChildren,
                        x => x.IsExpanded),
                    new TextColumn<HierarchicalFolderclass, int?>(
                        "NumberofLinks",
                        x => x.Urls.Count,
                        options: new()
                        {
                            CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Urls.Count),
                            CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Urls.Count),
                        }),
                    new TextColumn<HierarchicalFolderclass, string?>(
                        "Folderpath",
                        x => x.Folderpath,
                        options: new()
                        {
                            CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Folderpath),
                            CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Folderpath),
                        }),
                    new TextColumn<HierarchicalFolderclass, int?>(
                        "depth",
                        x => x.Depth,
                        options: new()
                        {
                            CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Depth),
                            CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Depth),
                        }),
                }
            };
            result.RowSelection!.SingleSelect = false;
            return result;
        }
        #endregion
    }
}
