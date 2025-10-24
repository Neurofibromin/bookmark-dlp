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

namespace bookmark_dlp.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
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
        _appSettings.ResetSettingsToDefault();
    }

    [RelayCommand]
    private async Task ChooseOutputFolder(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            IStorageFolder? folder = await DoOpenFolderPickerAsync();
            if (folder != null)
                ActiveSettings.OutputFolder = folder.TryGetLocalPath();
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }

        if (ActiveSettings.YtDlpExecutableNotFound)
        {
            if (YtdlpInterfacing.Yt_dlp_pathfinder(ActiveSettings.OutputFolder) != null)
            {
                ActiveSettings.YtDlpExecutableNotFound = false;
                ActiveSettings.YtDlpBinaryPath = YtdlpInterfacing.Yt_dlp_pathfinder(ActiveSettings.OutputFolder);
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
        return result?.Count >= 1 ? result[0] : null;
    }

    [RelayCommand]
    private async Task ChooseYtdlpBinary(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            IStorageFile? file = await DoOpenFilePickerAsync("Select yt-dlp executable");
            if (file != null)
            {
                ActiveSettings.YtDlpBinaryPath = file.TryGetLocalPath();
                ActiveSettings.YtDlpExecutableNotFound = false;
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    private async Task<IStorageFile?> DoOpenFilePickerAsync(string? title)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");
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
            IStorageFile? file = await DoOpenFilePickerAsync("Open yt-dlp.conf file");
            if (file != null)
            {
                string? newconffile = file.TryGetLocalPath();
                if (newconffile != null && !ActiveSettings.YtDlpConfigFiles.Contains(newconffile))
                    ActiveSettings.YtDlpConfigFiles.Add(newconffile);
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    [RelayCommand]
    private void ChangeThemeToClassic()
    {
        if (ActiveSettings.SelectedTheme == AppTheme.Classic) return;
        ThemeManager.ApplyTheme(AppTheme.Classic);
        ActiveSettings.SelectedTheme = AppTheme.Classic;
    }

    [RelayCommand]
    private void ChangeThemeToFluent()
    {
        if (ActiveSettings.SelectedTheme == AppTheme.Fluent) return;
        ThemeManager.ApplyTheme(AppTheme.Fluent);
        ActiveSettings.SelectedTheme = AppTheme.Fluent;
    }

    [RelayCommand]
    private void ChangeThemeToSimple()
    {
        if (ActiveSettings.SelectedTheme == AppTheme.Simple) return;
        ThemeManager.ApplyTheme(AppTheme.Simple);
        ActiveSettings.SelectedTheme = AppTheme.Simple;
    }
}