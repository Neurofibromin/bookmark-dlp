using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nfbookmark;

namespace bookmark_dlp.ViewModels;

public partial class StartPageViewModel : ViewModelBase
{
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
            .SelectMany(browser => browser.foundProfiles)
            .ToList() ?? new List<string>();

        if (YtdlpInterfacing.Yt_dlp_pathfinder(Directory.GetCurrentDirectory()) != null)
            appSettings.Settings.Ytdlp_executable_not_found = false;
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
            ActiveSettings.ManualImportUsed = false;
            ActiveSettings.Manualimportfilelocation = null;
        }

        ShouldEnableImportButton();
    }

    private void ActiveSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsStruct.Manualimportfilelocation))
        {
            if (!string.IsNullOrWhiteSpace(ActiveSettings.Manualimportfilelocation))
            {
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
            IStorageFile? file = await DoOpenFilePickerAsync();
            if (file != null)
            {
                ActiveSettings.ManualImportUsed = true;
                ActiveSettings.Manualimportfilelocation = file.TryGetLocalPath();
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    private async Task<IStorageFile?> DoOpenFilePickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

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
            IStorageFolder? folder = await DoOpenFolderPickerAsync();
            if (folder != null)
            {
                // AppSettings._settings.Outputfolder = folder.TryGetLocalPath();
                ActiveSettings.Outputfolder = folder.TryGetLocalPath();
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }

        if (ActiveSettings.Ytdlp_executable_not_found)
        {
            if (YtdlpInterfacing.Yt_dlp_pathfinder(ActiveSettings.Outputfolder) != null)
            {
                // await Console.Out.WriteLineAsync("thisone");
                // AppSettings._settings.Ytdlp_executable_not_found = false;
                ActiveSettings.Ytdlp_executable_not_found = false;
            }
        }
    }

    private async Task<IStorageFolder?> DoOpenFolderPickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        IReadOnlyList<IStorageFolder>? result = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose output folder for saving the videos"
        });
        if (result?.Count >= 1) return result[0];

        return null;
    }
}