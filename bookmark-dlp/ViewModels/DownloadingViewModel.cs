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
using Serilog;

namespace bookmark_dlp.ViewModels;

public partial class DownloadingViewModel : ViewModelBase
{
    private readonly ILogger Log = Serilog.Log.ForContext<DownloadingViewModel>();
    
    [ObservableProperty] private SettingsStruct _activeSettings;
    [ObservableProperty] private string? _fileSource;
    private List<ImportedFolder>? _importedFolders;
    private List<MappedFolder>? _mappedFolders;
    private List<ResolvedFolder>? _resolvedFolders;
    [ObservableProperty] private HierarchicalTreeDataGridSource<HierarchicalFolderclass>? _treeSource;

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
        if (_importedFolders == null)
            throw new NoNullAllowedException(
                "List<ImportedFolder> _importedFolders must not be null when starting the status query.");
        _mappedFolders = FolderManager.CreateFolderStructure(_importedFolders, ActiveSettings.OutputFolder);
        _resolvedFolders = AutoImport.LinksFromUrls(_mappedFolders);
        AppMethods.CountWantedVideos(_resolvedFolders);
        AppMethods.CheckCurrentFilesystemState(_resolvedFolders);
    }

    [RelayCommand]
    private void First()
    {
        Log.Fatal("Executing debug step: First");
        if (_importedFolders != null)
        {
            _mappedFolders = FolderManager.CreateFolderStructure(_importedFolders, ActiveSettings.OutputFolder);
        }
        else
        {
            Log.Warning("Debug step First executed but _importedFolders is null.");
        }
    }
    
    [RelayCommand]
    private void Second()
    {
        Log.Fatal("Executing debug step: Second");
        if (_mappedFolders != null)
        {
            _resolvedFolders = AutoImport.LinksFromUrls(_mappedFolders);
        }
        else
        {
            Log.Warning("Debug step Second executed but _mappedFolders is null.");
        }
    }
    
    [RelayCommand]
    private void Third()
    {
        Log.Fatal("Executing debug step: Third");
        if (_resolvedFolders != null)
        {
            AppMethods.CountWantedVideos(_resolvedFolders);
        }
        else
        {
            Log.Warning("Debug step Third executed but _resolvedFolders is null.");
        }
    }
    
    [RelayCommand]
    private void Fourth()
    {
        Log.Fatal("Executing debug step: Fourth");
        if (_resolvedFolders != null)
        {
            AppMethods.CheckCurrentFilesystemState(_resolvedFolders);
        }
        else
        {
            Log.Warning("Debug step Fourth executed but _resolvedFolders is null.");
        }
    }

    /// <summary>
    ///     Starts the import and loads the imported folders to TreeDataGrid
    /// </summary>
    /// <returns>true if successful, false if import failed/empty</returns>
    public bool LoadFoldersFromFile()
    {
        if (FileSource == null)
        {
            Log.Error("FileSource is null, cannot load folders.");
            return false;
        }
        try
        {
            Log.Information("Starting bookmark import from {FileSource}...", FileSource);
            _importedFolders = BookmarkImporterFactory.SmartImport(FileSource);
        }
        catch (Exception e)
        {
            Log.Error(e, "An unhandled exception occurred during bookmark import from {FileSource} by DownloadingViewModel", FileSource);
            return false;
        }

        if (_importedFolders == null || _importedFolders.Count == 0)
        {
            Log.Error("Bookmark import from {FileSource} resulted in no folders.", FileSource);
            return false;
        }
        
        Log.Information("Successfully imported {FolderCount} folders.", _importedFolders.Count);
        HierarchicalFolderCollection = AppMethods.GenerateHierarchicalFolderclassesFromList(_importedFolders);
        TreeSource = CreateTreeSource();
        return true;
    }

    #region TreeDataGrid

    private ITreeDataGridRowSelectionModel<HierarchicalFolderclass> GetRowSelection(ITreeDataGridSource source)
    {
        return source.Selection as ITreeDataGridRowSelectionModel<HierarchicalFolderclass> ??
               throw new InvalidOperationException("Expected a row selection model.");
    }

    private HierarchicalTreeDataGridSource<HierarchicalFolderclass>? CreateTreeSource()
    {
        if (HierarchicalFolderCollection is null)
        {
            Log.Error("HierarchicalFolderCollection is null.");
            return null;
        }
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