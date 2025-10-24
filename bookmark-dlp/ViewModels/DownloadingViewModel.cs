using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Media;
using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nfbookmark;
using NfLogger;

namespace bookmark_dlp.ViewModels;

public partial class DownloadingViewModel : ViewModelBase
{
    
    [ObservableProperty] private SettingsStruct _activeSettings;
    [ObservableProperty] private string? _fileSource;
    private List<Folderclass>? _folders;
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

    public DownloadingViewModel() : this(new AppSettings())
    {
    }

    private ObservableCollection<HierarchicalFolderclass>? HierarchicalFolderCollection { get; set; }

    [RelayCommand]
    private void GetCurrentStatusWithQueryYT()
    {
        // TODO: this
        // At this point DownloadingView is open and the folders are loaded to TreeDataGrid
        if (_folders == null)
            throw new NoNullAllowedException(
                "List<Folderclass> _folders must not be null when starting the status query.");
        // LoadFoldersFromFile() : Import.SmartImport();
        AutoImport.LinksFromUrls(_folders);
        FolderManager.CreateFolderStructure(_folders, ActiveSettings.Outputfolder);
        AppMethods.CountWantedVideos(ref _folders);
        AppMethods.CheckCurrentFilesystemState(ref _folders);
    }

    /// <summary>
    ///     Starts the import and loads the imported folders to TreeDataGrid
    /// </summary>
    /// <returns>true if successful, false if import failed/empty</returns>
    /// <exception cref="ArgumentNullException">There is no file source available</exception>
    public bool LoadFoldersFromFile()
    {
        if (FileSource == null)
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
        HierarchicalTreeDataGridSource<HierarchicalFolderclass> result =
            new HierarchicalTreeDataGridSource<HierarchicalFolderclass>(HierarchicalFolderCollection)
            {
                Columns =
                {
                    new CheckBoxColumn<HierarchicalFolderclass>(
                        null,
                        x => x.WantDownloaded,
                        (o, v) => o.WantDownloaded = v,
                        options: new CheckBoxColumnOptions<HierarchicalFolderclass>
                        {
                            CanUserResizeColumn = false
                        }),
                    new HierarchicalExpanderColumn<HierarchicalFolderclass>(
                        new TemplateColumn<HierarchicalFolderclass>(
                            "Name",
                            "FileNameCell",
                            width: new GridLength(1, GridUnitType.Star),
                            options: new TemplateColumnOptions<HierarchicalFolderclass>
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
                        options: new TextColumnOptions<HierarchicalFolderclass>
                        {
                            CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Urls.Count),
                            CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Urls.Count)
                        }),
                    new TextColumn<HierarchicalFolderclass, string?>(
                        "Folderpath",
                        x => x.Folderpath,
                        options: new TextColumnOptions<HierarchicalFolderclass>
                        {
                            CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Folderpath),
                            CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Folderpath)
                        }),
                    new TextColumn<HierarchicalFolderclass, int?>(
                        "depth",
                        x => x.Depth,
                        options: new TextColumnOptions<HierarchicalFolderclass>
                        {
                            CompareAscending = HierarchicalFolderclass.SortAscending(x => x.Depth),
                            CompareDescending = HierarchicalFolderclass.SortDescending(x => x.Depth)
                        })
                }
            };
        result.RowSelection!.SingleSelect = false;
        return result;
    }

    #endregion
}