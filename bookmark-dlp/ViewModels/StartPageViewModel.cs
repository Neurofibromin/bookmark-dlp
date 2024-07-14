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
using bookmark_dlp.Models;



namespace bookmark_dlp.ViewModels
{
    public partial class StartPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        public List<string> availableBrowserBookmarkPaths; //= (List<string>)(from browser in AutoImport.FindBrowserBookmarkFilesPaths() select browser.foundFiles)

        [ObservableProperty]
        public string[] browserlist = { "Firefox", "Chrome", "Safari" };
        [ObservableProperty]
        public string? chosenBrowser;

        [ObservableProperty] public string[] _errorMessage = { "No browsers found", };

        [ObservableProperty]
        private SettingsStruct _activeSettings;



        public StartPageViewModel()
        {
            var temp = new List<string>();
            List<BrowserLocations> temp2 = Import.GetBrowserBookmarkFilesPaths();

            if (temp2 != null)
            {
                foreach (BrowserLocations browser in temp2)
                {
                    foreach (string path in browser.foundFiles)
                    {
                        temp.Add(path);
                    }
                }
            }
            
            AvailableBrowserBookmarkPaths = temp;
            
            if (AppMethods.Yt_dlp_pathfinder(Directory.GetCurrentDirectory()) != null) { AppSettings._settings.Ytdlp_executable_not_found = false; }
            ActiveSettings = new SettingsStruct(AppSettings._settings);
        }

        public void ReBindSettings()
        {
            ActiveSettings = new SettingsStruct(AppSettings._settings);
        }

        public async Task SaveActiveSettings()
        {
            AppSettings._settings = new SettingsStruct(ActiveSettings);
        }

        [ObservableProperty] private string? _fileText;


        [RelayCommand]
        public async Task OpenFile(CancellationToken token)
        {
            ActiveSettings.HtmlImportUsed = true;
            ErrorMessages?.Clear();
            try
            {
                var file = await DoOpenFilePickerAsync();
                if (file != null)
                {
                    ActiveSettings.Htmlfilelocation = file.TryGetLocalPath();
                    // AppSettings._settings.Htmlfilelocation = ActiveSettings.Htmlfilelocation;
                }
                else { }
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

            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open html file with bookmarks",
                AllowMultiple = false
            });

            return files?.Count >= 1 ? files[0] : null;
        }


        [RelayCommand]
        public async Task OpenFolder(CancellationToken token)
        {
            ErrorMessages?.Clear();
            try
            {
                var folder = await DoOpenFolderPickerAsync();
                if (folder != null)
                {
                    // AppSettings._settings.Outputfolder = folder.TryGetLocalPath();
                    ActiveSettings.Outputfolder = folder.TryGetLocalPath();
                }
                else { }
            }
            catch (Exception e)
            {
                ErrorMessages?.Add(e.Message);
            }
            if (AppMethods.Yt_dlp_pathfinder(ActiveSettings.Outputfolder) != null)
            {
                // await Console.Out.WriteLineAsync("thisone");
                // AppSettings._settings.Ytdlp_executable_not_found = false;
                ActiveSettings.Ytdlp_executable_not_found = false;
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
            if (result?.Count >= 1)
            {
                return (IStorageFolder?)result[0];
            }
            else
            {
                return null;
            }
        }



    }
}
