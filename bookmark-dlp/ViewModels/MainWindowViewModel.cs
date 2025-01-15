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


namespace bookmark_dlp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty] private ViewModelBase _selectedTab;
        [ObservableProperty] private ViewModelBase? previousViewModel;
        
        [ObservableProperty] private StartPageViewModel myStartPageViewModel;
        [ObservableProperty] private SettingsViewModel mySettingsViewModel;
        [ObservableProperty] private LogViewModel myLogViewModel;
        [ObservableProperty] private DownloadingViewModel myDownloadingViewModel;
        
        [ObservableProperty] private bool _startPageEnabled = true;
        [ObservableProperty] private bool _settingsEnabled = true;
        [ObservableProperty] private bool _logEnabled = true;
        [ObservableProperty] private bool _downloadingEnabled = false;
        [ObservableProperty] private bool _importSuccess = false;
        
        public MainWindowViewModel()
        {
            // Initialize ViewModels
            mySettingsViewModel = new SettingsViewModel();
            myStartPageViewModel = new StartPageViewModel();
            myDownloadingViewModel = new DownloadingViewModel();
            myLogViewModel = new LogViewModel();
            
            PreviousViewModel = MyStartPageViewModel;
            /*MySettingsViewModel.ActiveSettings.PropertyChanged += ActiveSettingsOnPropertyChanged;
            MyStartPageViewModel.ActiveSettings.PropertyChanged += ActiveSettingsOnPropertyChanged;*/
            // Set initial selected tab
            SelectedTab = MyStartPageViewModel;
        }

        /*private void ActiveSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // MySettingsViewModel.ReBindSettings();
            // MyStartPageViewModel.ReBindSettings();
            // MyDownloadingViewModel.ReBindSettings();
            //Logger.LogVerbose("Active settings changed, rebinding from MainWindowViewModel", Logger.Verbosity.Trace);
        }*/

        public async Task SettingsCommand()
        {
            PreviousViewModel = SelectedTab;
            SelectedTab = MySettingsViewModel;
        }

        public async Task GoForward()
        {
            PreviousViewModel = SelectedTab;
            // MyDownloadingViewModel.ReBindSettings();
            MyDownloadingViewModel.FileSource = AppSettings._settings.ManualImportUsed
                ? AppSettings._settings.Manualimportfilelocation
                : MyStartPageViewModel.ChosenBrowser;
            ImportSuccess = await MyDownloadingViewModel.LoadFoldersFromFile();
            if (ImportSuccess)
            {
                DownloadingEnabled = true;
                SelectedTab = MyDownloadingViewModel;
            }
            else
            {
                MyStartPageViewModel.EnableImportButton = false;
                MyStartPageViewModel.ImportButtonToolTip = "Import Failed";
            }
        }

        public void BackToStartPage()
        {
            PreviousViewModel = SelectedTab;
            SelectedTab = MyStartPageViewModel;
        }
    }
}
