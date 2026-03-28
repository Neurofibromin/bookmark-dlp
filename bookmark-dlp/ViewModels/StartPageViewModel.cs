using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nfbookmark;
using Serilog;

namespace bookmark_dlp.ViewModels;

public partial class StartPageViewModel : ViewModelBase
{
    private readonly ILogger Log = Serilog.Log.ForContext<StartPageViewModel>();
    
    [ObservableProperty] private SettingsStruct _activeSettings;
    [ObservableProperty] private string[] _browserList = { "Firefox", "Chrome", "Safari" };
    [ObservableProperty] private string? _chosenBrowser;
    [ObservableProperty] private bool _enableImportButton;
    [ObservableProperty] private string[] _errorMessage = { "No browsers found" };
    [ObservableProperty] private string? _fileText;
    [ObservableProperty] private string? _importButtonToolTip = "No source selected";

    [ObservableProperty]
    private List<string>
        availableBrowserBookmarkPaths; //= (List<string>)(from browser in AutoImport.FindBrowserBookmarkFilesPaths() select browser.foundFiles)

    public StartPageViewModel(IAppSettings appSettings)
    {
        AvailableBrowserBookmarkPaths = BrowserLocations.GetBrowserBookmarkFilesPaths()?
            .SelectMany(browser => browser.FoundBookmarkFilePaths)
            .ToList() ?? new List<string>();
        Log.Information("Found {BrowserCount} available browser bookmark profiles.", AvailableBrowserBookmarkPaths.Count);

        if (YtdlpInterfacing.Yt_dlp_pathfinder(Directory.GetCurrentDirectory()) != null)
        {
            appSettings.Settings.YtDlpExecutableNotFound = false;
            Log.Debug("yt-dlp executable found in the current directory.");
        }
        _activeSettings = appSettings.Settings;
        ActiveSettings.PropertyChanged += ActiveSettings_PropertyChanged;
        ShouldEnableImportButton();
    }

    public StartPageViewModel() : this(new AppSettings())
    {
    }

    partial void OnChosenBrowserChanged(string? value)
    {
        if (value != null)
        {
            Log.Information("User chose browser bookmark source: {BrowserPath}", value);
            ActiveSettings.ManualImportUsed = false;
            ActiveSettings.ManualImportFileLocation = null;
        }

        ShouldEnableImportButton();
    }

    private void ActiveSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsStruct.ManualImportFileLocation))
        {
            if (!string.IsNullOrWhiteSpace(ActiveSettings.ManualImportFileLocation))
            {
                Log.Debug("ManualImportFileLocation changed, clearing ChosenBrowser.");
                ChosenBrowser = null;
                ShouldEnableImportButton();
            }
        }
    }

    private void ShouldEnableImportButton()
    {
        EnableImportButton = ActiveSettings.ManualImportUsed || !string.IsNullOrEmpty(ChosenBrowser);
        if (!EnableImportButton)
            ImportButtonToolTip = "No source selected";
        else
            ImportButtonToolTip = "Import bookmarks";
    }

    [RelayCommand]
    private async Task OpenFile(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            Log.Debug("Opening file picker for manual bookmark import.");
            IStorageFile? file = await DoOpenFilePickerAsync();
            if (file != null)
            {
                var path = file.TryGetLocalPath();
                Log.Information("User selected manual import file: {FilePath}", path);
                ActiveSettings.ManualImportUsed = true;
                ActiveSettings.ManualImportFileLocation = path;
            }
            else
            {
                Log.Debug("User cancelled the file selection.");
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "An exception occurred while opening the file picker.");
            ErrorMessages?.Add(e.Message);
        }
    }

    private async Task<IStorageFile?> DoOpenFilePickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
        {
            Log.Error("Missing StorageProvider instance.");
            throw new NullReferenceException("Missing StorageProvider instance.");
        }

        IReadOnlyList<IStorageFile>? files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open html file with bookmarks",
            AllowMultiple = false
        });

        return files?.Count >= 1 ? files[0] : null;
    }

    [RelayCommand]
    private async Task OpenFolder(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            Log.Debug("Opening folder picker for output folder.");
            IStorageFolder? folder = await DoOpenFolderPickerAsync();
            if (folder != null)
            {
                var path = folder.TryGetLocalPath();
                Log.Information("User selected new output folder: {OutputFolder}", path);
                ActiveSettings.OutputFolder = path;
            }
            else
            {
                Log.Debug("User cancelled the folder selection.");
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "An exception occurred while opening the folder picker.");
            ErrorMessages?.Add(e.Message);
        }

        if (ActiveSettings.YtDlpExecutableNotFound)
        {
            Log.Debug("Re-checking for yt-dlp executable after folder change.");
            var foundPath = YtdlpInterfacing.Yt_dlp_pathfinder(ActiveSettings.OutputFolder);
            if (foundPath != null)
            {
                Log.Information("yt-dlp executable found at {YtdlpPath}", foundPath);
                ActiveSettings.YtDlpExecutableNotFound = false;
            }
            else
            {
                Log.Warning("yt-dlp executable still not found after folder change.");
            }
        }
    }

    private async Task<IStorageFolder?> DoOpenFolderPickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
        {
            Log.Error("Missing StorageProvider instance.");
            throw new NullReferenceException("Missing StorageProvider instance.");
        }
            
        IReadOnlyList<IStorageFolder>? result = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose output folder for saving the videos"
        });
        if (result?.Count >= 1) return result[0];

        return null;
    }
}