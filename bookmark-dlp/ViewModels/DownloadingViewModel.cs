using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using bookmark_dlp.Models;
using NfLogger;
using Nfbookmark;

namespace bookmark_dlp.ViewModels
{
    public partial class DownloadingViewModel : ViewModelBase
    {
        [ObservableProperty]
        private SettingsStruct _activeSettings;

        [ObservableProperty]
        private string _fileSource;

        private List<Folderclass> _folders;
        /*public ObservableCollection<ObsFolderclass> FolderCollection { get; set; }
        public ObservableCollection<ObsFolderclass> OldHierarchicalFolderCollection { get; set; }*/
        public ObservableCollection<HierarchicalFolderclass> HierarchicalFolderCollection { get; set; }
        
        private static IconConverter? s_iconConverter;
        [ObservableProperty] private HierarchicalTreeDataGridSource<HierarchicalFolderclass>? _treeSource;
        
        // public HierarchicalTreeDataGridSource<HierarchicalFolderclass> TreeSource => _treeSource;

        
        // private ITreeDataGridSource<HierarchicalFolderclass> _source;
        // private HierarchicalFolderclass? _root;
        
        /*public ITreeDataGridSource<HierarchicalFolderclass> Source
        {
            get => _source;
            private set => this.SetPropertyAndNotifyOnCompletion(ref _source, value);
        }*/
        
        public DownloadingViewModel()
        {
            ActiveSettings = AppSettings._settings;
        }
        
        public async Task LoadFoldersFromFile()
        {
            if(FileSource == null)
                throw new ArgumentNullException(nameof(FileSource));
            _folders = Import.SmartImport(FileSource);
            /*FolderCollection ??= new ObservableCollection<ObsFolderclass>();
            OldHierarchicalFolderCollection = new ObservableCollection<ObsFolderclass>();

            // Generating Observable FolderCollection from folders
            foreach (Folderclass folder in _folders)
            {
                ObsFolderclass onefolder = new ObsFolderclass(folder);
                FolderCollection.Add(onefolder);
            }

            // Generating Hierarchical Observable FolderCollection from folders
            foreach (Folderclass folder in _folders.Where(a => a.depth == 0))
            {
                ObsFolderclass onefolder = new ObsFolderclass(folder);
                OldHierarchicalFolderCollection.Add(onefolder);
                Logger.LogVerbose("added: " + folder.name, Logger.Verbosity.Trace);
            }
            foreach (Folderclass folder in _folders.Where(a => a.depth != 0).OrderBy(a => a.depth))
            {
                Logger.LogVerbose("examining " + folder.name + " " + folder.id + " parent:" + folder.parent, Logger.Verbosity.Trace);
                ObsFolderclass onefolder = new ObsFolderclass(folder);
                foreach (ObsFolderclass parent in OldHierarchicalFolderCollection)
                {
                    if (parent.Id == folder.parent) { 
                        Logger.LogVerbose("Found parent: " + parent.Name, Logger.Verbosity.Trace);
                        parent.Children.Add(onefolder);
                    }
                }
                // HierarchcalFolderCollection.Single(parent => parent.Id == folder.parent).Children.Add(onefolder);
                Logger.LogVerbose("added: " + folder.name, Logger.Verbosity.Trace);
            }*/
            
            HierarchicalFolderCollection = AppMethods.GenerateHierarchicalFolderclassesFromList(_folders);
            _treeSource = CreateTreeSource();
        }

        public void ReBindSettings()
        {
            ActiveSettings = AppSettings._settings;
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

            public object? Convert(IList<object?> values, Type targetType, object parameter, CultureInfo culture)
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

        public static IMultiValueConverter FileIconConverter
        {
            get
            {
                Logger.LogVerbose("Getting FileIconConverter", Logger.Verbosity.Trace);
                if (s_iconConverter is null)
                {
                    Logger.LogVerbose("FileIconConverter is NULL", Logger.Verbosity.Trace);
                    // StreamGeometry folderIcon = StreamGeometry.Parse(Application.Current.Resources["folder_regular"].ToString());
                    // Application.Current.TryGetResource("folder_regular", out var folderIconregular);
                    bool a = Application.Current.Styles.TryGetResource("folder_regular",theme: Application.Current.ActualThemeVariant , value: out var folderIconregular);
                    bool b = Application.Current.Styles.TryGetResource("folder_open_regular",theme: Application.Current.ActualThemeVariant , value: out var folderIconopen);
                    if (a && b)
                        Logger.LogVerbose("FileIconConverter found", Logger.Verbosity.Trace);
                    else
                        Logger.LogVerbose("FileIconConverter not found", Logger.Verbosity.Error);
                    Logger.LogVerbose("Found resources: " + folderIconopen.ToString(), Logger.Verbosity.Trace);
                    // StreamGeometry folderIcon = StreamGeometry.Parse("M17.0606622,9 C17.8933043,9 18.7000032,9.27703406 19.3552116,9.78392956 L19.5300545,9.92783739 L22.116207,12.1907209 C22.306094,12.356872 22.5408581,12.4608817 22.7890575,12.4909364 L22.9393378,12.5 L40.25,12.5 C42.2542592,12.5 43.8912737,14.0723611 43.994802,16.0508414 L44,16.25 L44,35.25 C44,37.2542592 42.4276389,38.8912737 40.4491586,38.994802 L40.25,39 L7.75,39 C5.74574083,39 4.10872626,37.4276389 4.00519801,35.4491586 L4,35.25 L4,12.75 C4,10.7457408 5.57236105,9.10872626 7.55084143,9.00519801 L7.75,9 L17.0606622,9 Z M22.8474156,14.9988741 L20.7205012,17.6147223 C20.0558881,18.4327077 19.0802671,18.9305178 18.0350306,18.993257 L17.8100737,19 L6.5,18.999 L6.5,35.25 C6.5,35.8972087 6.99187466,36.4295339 7.62219476,36.4935464 L7.75,36.5 L40.25,36.5 C40.8972087,36.5 41.4295339,36.0081253 41.4935464,35.3778052 L41.5,35.25 L41.5,16.25 C41.5,15.6027913 41.0081253,15.0704661 40.3778052,15.0064536 L40.25,15 L22.8474156,14.9988741 Z M17.0606622,11.5 L7.75,11.5 C7.10279131,11.5 6.5704661,11.9918747 6.50645361,12.6221948 L6.5,12.75 L6.5,16.499 L17.8100737,16.5 C18.1394331,16.5 18.4534488,16.3701335 18.6858203,16.1419575 L18.7802162,16.0382408 L20.415,14.025 L17.883793,11.8092791 C17.693906,11.643128 17.4591419,11.5391183 17.2109425,11.5090636 L17.0606622,11.5 Z");
                    // StreamGeometry folderOpenIcon = StreamGeometry.Parse("M20 9.50195V8.74985C20 7.50721 18.9926 6.49985 17.75 6.49985H12.0247L9.64368 4.51995C9.23959 4.18393 8.73063 3.99997 8.20509 3.99997H4.24957C3.00724 3.99997 2 5.00686 1.99957 6.24919L1.99561 17.7492C1.99518 18.9921 3.00266 20 4.24561 20H4.27196C4.27607 20 4.28019 20 4.28431 20H18.4693C19.2723 20 19.9723 19.4535 20.167 18.6745L21.9169 11.6765C22.1931 10.5719 21.3577 9.50195 20.2192 9.50195H20ZM4.24957 5.49997H8.20509C8.38027 5.49997 8.54993 5.56129 8.68462 5.6733L11.2741 7.82652C11.4088 7.93852 11.5784 7.99985 11.7536 7.99985H17.75C18.1642 7.99985 18.5 8.33563 18.5 8.74985V9.50195H6.42385C5.39136 9.50195 4.49137 10.2047 4.241 11.2064L3.49684 14.1837L3.49957 6.24971C3.49971 5.8356 3.83546 5.49997 4.24957 5.49997ZM5.69623 11.5701C5.77969 11.2362 6.07969 11.002 6.42385 11.002H20.2192C20.3819 11.002 20.5012 11.1548 20.4617 11.3126L18.7119 18.3107C18.684 18.4219 18.584 18.5 18.4693 18.5H4.28431C4.12167 18.5 4.00233 18.3472 4.04177 18.1894L5.69623 11.5701Z");
                    /* maybe instead:
                     * public static T Get<T>(string resourceName)
                       {
                           try
                           {
                               var success = Application.Current.Resources.TryGetValue(resourceName, out var outValue);

                               if(success && outValue is T)
                               {
                                   return (T)outValue;
                               }
                               else
                               {
                                   return default(T);
                               }
                           }
                           catch
                           {
                               return default(T);
                           }
                       }
                     */
                    /*
                     * Or bind the staticresource directly?
                     */
                    // Logger.LogVerbose("Streamgeometry: " + folderIcon.ToString(), Logger.Verbosity.Debug);
                    // s_iconConverter = new IconConverter(folderOpenIcon, folderIcon);
                    s_iconConverter = new IconConverter((StreamGeometry) folderIconopen, (StreamGeometry) folderIconregular);
                }
                return s_iconConverter;
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
                    /*new HierarchicalExpanderColumn<HierarchicalFolderclass>(
                        new TextColumn<HierarchicalFolderclass, string?>(
                            "Name",
                            x => x.Name,
                            options: new()
                            {
                                CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Name),
                                CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Name),
                                IsTextSearchEnabled = true,
                            }),
                        x => x.Children,
                        x => x.HasChildren,
                        x => x.IsExpanded),*/
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
