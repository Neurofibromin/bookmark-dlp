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
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using bookmark_dlp.Models;
using Classic.Avalonia.Theme;
using Material.Styles.Themes;
using NfLogger;



namespace bookmark_dlp.ViewModels
{
    /// <summary>
    /// TODO: autodetect yt-dlp.config file at folders where
    /// 1) program is called from
    /// 2) program executable is found in
    /// 3) output folder
    /// 4) yt-dlp executable is found in
    /// 5) yt-dlp default folder (somewhere in .local?
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        [ObservableProperty] 
        private SettingsStruct _activeSettings;
        
        private readonly IAppSettings _appSettings; //maybe shouldn't be readonly?
        
        public SettingsViewModel(IAppSettings appSettings) 
        {
            _appSettings = appSettings;
            _activeSettings = _appSettings.Settings;
        }

        // Parameterless constructor for XAML designer support
        public SettingsViewModel() : this(new AppSettings()) {}

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
                var folder = await DoOpenFolderPickerAsync();
                if (folder != null)
                {
                    ActiveSettings.Outputfolder = folder.TryGetLocalPath();
                }
                else { }
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
            var result = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Choose output folder for saving the videos",
            });
            return result?.Count >= 1 ? result[0] : null;
        }
        
        [RelayCommand]
        private async Task ChooseYtdlpBinary(CancellationToken token)
        {
            ErrorMessages?.Clear();
            try
            {
                var file = await DoOpenFilePickerAsync("Select yt-dlp executable");
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
            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
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
                var file = await DoOpenFilePickerAsync("Open yt-dlp.conf file");
                if (file != null)
                {
                    string? newconffile = file.TryGetLocalPath();
                    if (newconffile != null && !ActiveSettings.Yt_dlp_configfiles.Contains(newconffile))
                    {
                        ActiveSettings.Yt_dlp_configfiles.Add(newconffile);
                    }
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
            var a = new StyleInclude(semiTreeData);
            a.Source = semiTreeData;
            Application.Current.Styles.Add(a);
            
            Uri localIcons = new Uri("avares://bookmark-dlp/Assets/Icons.axaml");
            var b = new StyleInclude(localIcons);
            b.Source = localIcons;
            Application.Current.Styles.Add(b);
        }
        
        [RelayCommand]
        private void ChangeThemeToFluent()
        {
            Application.Current!.Styles.Clear();
            Application.Current.Styles.Add(new FluentTheme());
            Uri semiTreeData = new Uri("avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml");
            var a = new StyleInclude(semiTreeData);
            a.Source = semiTreeData;
            Application.Current.Styles.Add(a);
            
            Uri localIcons = new Uri("avares://bookmark-dlp/Assets/Icons.axaml");
            var b = new StyleInclude(localIcons);
            b.Source = localIcons;
            Application.Current.Styles.Add(b);
        }
        
        [RelayCommand]
        private void ChangeThemeToSimple()
        {
            Application.Current!.Styles.Clear();
            Application.Current.Styles.Add(new SimpleTheme());
            Uri semiTreeData = new Uri("avares://Semi.Avalonia.TreeDataGrid/Index.axaml");
            var a = new StyleInclude(semiTreeData);
            a.Source = semiTreeData;
            Application.Current.Styles.Add(a);
            
            Uri localIcons = new Uri("avares://bookmark-dlp/Assets/Icons.axaml");
            var b = new StyleInclude(localIcons);
            b.Source = localIcons;
            Application.Current.Styles.Add(b);
        }
    }
}
