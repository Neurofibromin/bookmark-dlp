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
    public partial class DownloadingViewModel : ViewModelBase
    {
        [ObservableProperty]
        private SettingsStruct _activeSettings;

        [ObservableProperty]
        private string _fileSource;


        public ObservableCollection<ObsFolderclass> FolderCollection { get; set; }



        public DownloadingViewModel()
        {
            ActiveSettings = AppSettings._settings;
            FolderCollection = new ObservableCollection<ObsFolderclass>();
        }
        
        public async Task LoadFoldersFromFile()
        {
            string filePath = FileSource;
            var tempcollection = new ObservableCollection<ObsFolderclass>();
            List<Folderclass> folders = Import.SmartImport(filePath);
            foreach (Folderclass folder in folders)
            {
                ObsFolderclass onefolder = new ObsFolderclass(folder);
                tempcollection.Add(onefolder);
            }
            FolderCollection = tempcollection;
        }
        
        public void ReBindSettings()
        {
            ActiveSettings = AppSettings._settings;
        }
        
        
    }
}
