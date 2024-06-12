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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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

        /*public bool downloadPlaylists = false;
        public bool downloadShorts = false;
        public bool downloadChannels = false;
        public bool concurrent_downloads = false;
        public bool cookies_autoextract = false;
        public string? yt_dlp_binary_path = "";*/

        [ObservableProperty]
        private SettingsStruct _activeSettings;

        public SettingsViewModel() {
            // Console.WriteLine("jsonrepr: " + AppSettings.GetJsonStringRepresentation());
            // Console.WriteLine("In orig at vmcreation: " + JsonConvert.SerializeObject(AppSettings._settings));
            ActiveSettings = new SettingsStruct(AppSettings._settings);
            // Console.WriteLine("In settingsviewmodel at creation: " + JsonConvert.SerializeObject(ActiveSettings));
        }

        public async Task SaveActiveSettings()
        {
            AppSettings._settings = new SettingsStruct(ActiveSettings);
        }
        
        public void ReBindSettings()
        {
            ActiveSettings = new SettingsStruct(AppSettings._settings);
        }

        [RelayCommand]
        public async Task RestoreDefaultSettings()
        {
            ActiveSettings = new SettingsStruct(AppSettings.defaultsettings);
        }
        
        
        [RelayCommand]
        public async Task ChooseOutputFolder(CancellationToken token)
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
            if (Methods.Yt_dlp_pathfinder(ActiveSettings.Outputfolder) != null)
            {
                // await Console.Out.WriteLineAsync("thisone");
                ActiveSettings.Ytdlp_executable_not_found = false;
                //ActiveSettings.Ytdlp_executable_not_found = false;
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
        

        [RelayCommand]
        public async Task ChooseYtdlpBinary(CancellationToken token)
        {
            ErrorMessages?.Clear();
            try
            {
                var file = await DoOpenFilePickerAsync();
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



    }
}
