﻿using Avalonia.Controls.ApplicationLifetimes;
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
    public partial class StartPageViewModel : ViewModelBase
    {

        [ObservableProperty]
        public string? htmlfilelocation = "a";
        [ObservableProperty]
        public bool htmlImportUsed = false;
        [ObservableProperty]
        public string outputfolder = Directory.GetCurrentDirectory().ToString();
        [ObservableProperty]
        public string[] browserlist = { "Firefox", "Chrome", "Safari" };
        [ObservableProperty]
        public bool ytdlp_executable_not_found = true;


        [ObservableProperty] private string? _fileText;

        [RelayCommand]
        public void TestTwo()
        {
            Htmlfilelocation = "hey";
        }


        [RelayCommand]
        public async Task OpenFile(CancellationToken token)
        {
            HtmlImportUsed = true;
            ErrorMessages?.Clear();
            try
            {
                var file = await DoOpenFilePickerAsync();
                Htmlfilelocation = file.Path.ToString();
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
                Outputfolder = folder.Path.ToString();
            }
            catch (Exception e)
            {
                ErrorMessages?.Add(e.Message);
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
                return result[0];
            }
            else
            {
                return null;
            }
        }



    }
}
