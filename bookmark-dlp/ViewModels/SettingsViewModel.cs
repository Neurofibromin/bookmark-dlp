using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform.Storage;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using bookmark_dlp.Models;
using Classic.Avalonia.Theme;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace bookmark_dlp.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ILogger Log = Serilog.Log.ForContext<SettingsViewModel>();
    private readonly IAppSettings _appSettings; //maybe shouldn't be readonly?

    [ObservableProperty] private SettingsStruct _activeSettings;

    public SettingsViewModel(IAppSettings appSettings)
    {
        _appSettings = appSettings;
        _activeSettings = _appSettings.Settings;
    }

    // Parameterless constructor for XAML designer support
    public SettingsViewModel() : this(new AppSettings())
    {
    }

    [RelayCommand]
    private void RestoreDefaultSettings()
    {
        Log.Information("Restoring settings to default.");
        _appSettings.ResetSettingsToDefault();
    }

    [RelayCommand]
    private async Task ChooseOutputFolder(CancellationToken token)
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
                Log.Debug("User cancelled the output folder selection.");
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "An exception occurred while choosing the output folder.");
            ErrorMessages?.Add(e.Message);
        }

        if (ActiveSettings.YtDlpExecutableNotFound)
        {
            Log.Debug("Searching for yt-dlp in the new output folder since it was not found previously.");
            var foundPath = YtdlpInterfacing.Yt_dlp_pathfinder(ActiveSettings.OutputFolder);
            if (foundPath != null)
            {
                Log.Information("yt-dlp executable found in new output folder at {YtdlpPath}", foundPath);
                ActiveSettings.YtDlpExecutableNotFound = false;
                ActiveSettings.YtDlpBinaryPath = foundPath;
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
        return result?.Count >= 1 ? result[0] : null;
    }

    [RelayCommand]
    private async Task ChooseYtdlpBinary(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            Log.Debug("Opening file picker for yt-dlp binary.");
            IStorageFile? file = await DoOpenFilePickerAsync("Select yt-dlp executable");
            if (file != null)
            {
                var path = file.TryGetLocalPath();
                Log.Information("User selected new yt-dlp binary path: {YtdlpPath}", path);
                ActiveSettings.YtDlpBinaryPath = path;
                ActiveSettings.YtDlpExecutableNotFound = false;
            }
            else
            {
                Log.Debug("User cancelled the yt-dlp binary selection.");
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "An exception occurred while choosing the yt-dlp binary.");
            ErrorMessages?.Add(e.Message);
        }
    }

    private async Task<IStorageFile?> DoOpenFilePickerAsync(string? title)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
        {
            Log.Error("Missing StorageProvider instance.");
            throw new NullReferenceException("Missing StorageProvider instance.");
        }
        IReadOnlyList<IStorageFile>? files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        return files?.Count >= 1 ? files[0] : null;
    }

    [RelayCommand]
    private async Task ChooseConfFile(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            Log.Debug("Opening file picker for yt-dlp config file.");
            IStorageFile? file = await DoOpenFilePickerAsync("Open yt-dlp.conf file");
            if (file != null)
            {
                string? newConfFile = file.TryGetLocalPath();
                if (newConfFile != null)
                {
                    if (!ActiveSettings.YtDlpConfigFiles.Contains(newConfFile))
                    {
                        Log.Information("Adding new yt-dlp config file: {ConfigFile}", newConfFile);
                        ActiveSettings.YtDlpConfigFiles.Add(newConfFile);
                    }
                    else
                    {
                        Log.Debug("Selected config file {ConfigFile} is already in the list.", newConfFile);
                    }
                }
            }
            else
            {
                Log.Debug("User cancelled the config file selection.");
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "An exception occurred while choosing a config file.");
            ErrorMessages?.Add(e.Message);
        }
    }

    [RelayCommand]
    private void ChangeThemeToClassic()
    {
        if (ActiveSettings.SelectedTheme == AppTheme.Classic)
        {
            Log.Debug("Theme is already Classic, no change needed.");
            return;
        }
        Log.Information("Changing theme to Classic.");
        ThemeManager.ApplyTheme(AppTheme.Classic);
        ActiveSettings.SelectedTheme = AppTheme.Classic;
    }

    [RelayCommand]
    private void ChangeThemeToFluent()
    {
        if (ActiveSettings.SelectedTheme == AppTheme.Fluent)
        {
            Log.Debug("Theme is already Fluent, no change needed.");
            return;
        }
        Log.Information("Changing theme to Fluent.");
        ThemeManager.ApplyTheme(AppTheme.Fluent);
        ActiveSettings.SelectedTheme = AppTheme.Fluent;
    }

    [RelayCommand]
    private void ChangeThemeToSimple()
    {
        if (ActiveSettings.SelectedTheme == AppTheme.Simple)
        {
            Log.Debug("Theme is already Simple, no change needed.");
            return;
        }
        Log.Information("Changing theme to Simple.");
        ThemeManager.ApplyTheme(AppTheme.Simple);
        ActiveSettings.SelectedTheme = AppTheme.Simple;
    }
}