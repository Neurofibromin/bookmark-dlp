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
using NfLogger;
using Nfbookmark;

namespace bookmark_dlp.ViewModels
{
    public partial class DownloadingViewModel : ViewModelBase
    {
        [ObservableProperty]
        private SettingsStruct _activeSettings;

        [ObservableProperty]
        private string _fileSource;


        public ObservableCollection<ObsFolderclass> FolderCollection { get; set; }

        public ObservableCollection<ObsFolderclass> HierarchcalFolderCollection { get; set; }



        public DownloadingViewModel()
        {
            ActiveSettings = AppSettings._settings;
            FolderCollection = new ObservableCollection<ObsFolderclass>();
            HierarchcalFolderCollection = new ObservableCollection<ObsFolderclass>();
        }
        
        public async Task LoadFoldersFromFile()
        {
            List<Folderclass> folders = Import.SmartImport(FileSource);
            FolderCollection = new ObservableCollection<ObsFolderclass>();
            HierarchcalFolderCollection = new ObservableCollection<ObsFolderclass>();


            foreach (Folderclass folder in folders)
            {
                ObsFolderclass onefolder = new ObsFolderclass(folder);
                FolderCollection.Add(onefolder);
            }



            
            foreach (Folderclass folder in folders.Where(a => a.depth == 0))
            {
                ObsFolderclass onefolder = new ObsFolderclass(folder);
                HierarchcalFolderCollection.Add(onefolder);
                Console.WriteLine("added: " + folder.name);
            }
            foreach (Folderclass folder in folders.Where(a => a.depth != 0).OrderBy(a => a.depth))
            {
                Console.WriteLine("examining " + folder.name + " " + folder.id + " parent:" + folder.parent);
                ObsFolderclass onefolder = new ObsFolderclass(folder);
                foreach (ObsFolderclass parent in HierarchcalFolderCollection)
                {
                    if (parent.Id == folder.parent) { 
                        Console.WriteLine("Found parent: " + parent.Name);
                        parent.Children.Add(onefolder);
                    }
                }
                // HierarchcalFolderCollection.Single(parent => parent.Id == folder.parent).Children.Add(onefolder);
                Console.WriteLine("added: " + folder.name);
            }/**/

        }

        public void ReBindSettings()
        {
            ActiveSettings = AppSettings._settings;
        }
        
        
    }
}
