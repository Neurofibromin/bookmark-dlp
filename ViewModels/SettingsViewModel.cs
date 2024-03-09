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



namespace bookmark_dlp.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        [ObservableProperty]
        public bool downloadPlaylists = false;
        [ObservableProperty]
        public bool downloadShorts = false;
        [ObservableProperty]
        public bool downloadChannels = false;
        [ObservableProperty]
        public bool concurrent_downloads = false;
        [ObservableProperty]
        public bool cookies_autoextract = false;
        [ObservableProperty]
        public string? yt_dlp_binary_path = "";

        
        public SettingsViewModel() {
            Console.WriteLine("Settings!");
        
        }

        [RelayCommand]
        public async Task ChooseYtdlpBinary(CancellationToken token)
        {
            ErrorMessages?.Clear();
            try
            {
                var file = await DoOpenFilePickerAsync();
                Yt_dlp_binary_path = file.Path.ToString();
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
