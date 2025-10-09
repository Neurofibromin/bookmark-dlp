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

/// <summary>
///     TODO: autodetect yt-dlp.config file at folders where
///     1) program is called from
///     2) program executable is found in
///     3) output folder
///     4) yt-dlp executable is found in
///     5) yt-dlp default folder (somewhere in .local?
/// </summary>
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
                ActiveSettings.Outputfolder = folder.TryGetLocalPath();
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }

        if (ActiveSettings.Ytdlp_executable_not_found)
        {
            if (YtdlpInterfacing.Yt_dlp_pathfinder(ActiveSettings.Outputfolder) != null)
            {
                ActiveSettings.Ytdlp_executable_not_found = false;
                ActiveSettings.Yt_dlp_binary_path = YtdlpInterfacing.Yt_dlp_pathfinder(ActiveSettings.Outputfolder);
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
                ActiveSettings.Yt_dlp_binary_path = file.TryGetLocalPath();
                ActiveSettings.Ytdlp_executable_not_found = false;
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
                if (newconffile != null && !ActiveSettings.Yt_dlp_configfiles.Contains(newconffile))
                    ActiveSettings.Yt_dlp_configfiles.Add(newconffile);
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
        Application.Current!.Styles.Clear();
        Application.Current.Styles.Add(new ClassicTheme());
        Uri semiTreeData = new Uri("avares://Semi.Avalonia.TreeDataGrid/Index.axaml");
        StyleInclude a = new StyleInclude(semiTreeData);
        a.Source = semiTreeData;
        Application.Current.Styles.Add(a);

        Uri localIcons = new Uri("avares://bookmark-dlp/Assets/Icons.axaml");
        StyleInclude b = new StyleInclude(localIcons);
        b.Source = localIcons;
        Application.Current.Styles.Add(b);
    }

    [RelayCommand]
    private void ChangeThemeToFluent()
    {
        Application.Current!.Styles.Clear();
        Application.Current.Styles.Add(new FluentTheme());
        Uri semiTreeData = new Uri("avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml");
        StyleInclude a = new StyleInclude(semiTreeData);
        a.Source = semiTreeData;
        Application.Current.Styles.Add(a);

        Uri localIcons = new Uri("avares://bookmark-dlp/Assets/Icons.axaml");
        StyleInclude b = new StyleInclude(localIcons);
        b.Source = localIcons;
        Application.Current.Styles.Add(b);
    }

    [RelayCommand]
    private void ChangeThemeToSimple()
    {
        Application.Current!.Styles.Clear();
        Application.Current.Styles.Add(new SimpleTheme());
        Uri semiTreeData = new Uri("avares://Semi.Avalonia.TreeDataGrid/Index.axaml");
        StyleInclude a = new StyleInclude(semiTreeData);
        a.Source = semiTreeData;
        Application.Current.Styles.Add(a);

        Uri localIcons = new Uri("avares://bookmark-dlp/Assets/Icons.axaml");
        StyleInclude b = new StyleInclude(localIcons);
        b.Source = localIcons;
        Application.Current.Styles.Add(b);
    }
}