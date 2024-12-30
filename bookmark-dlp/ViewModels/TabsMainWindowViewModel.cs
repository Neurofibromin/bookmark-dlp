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
    public partial class TabsMainWindowViewModel : ViewModelBase
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
        
        public TabsMainWindowViewModel()
        {
            // Initialize ViewModels
            mySettingsViewModel = new SettingsViewModel();
            myStartPageViewModel = new StartPageViewModel();
            myDownloadingViewModel = new DownloadingViewModel();
            myLogViewModel = new LogViewModel();
            
            PreviousViewModel = MyStartPageViewModel;

            // Set initial selected tab
            SelectedTab = MyStartPageViewModel;
        }

        public void GoBack()
        {
            (PreviousViewModel, SelectedTab) = (SelectedTab, PreviousViewModel);
            if (PreviousViewModel == MySettingsViewModel)
            {
                MySettingsViewModel.SaveActiveSettings();
                AppSettings.SaveToFile();
                MyStartPageViewModel.ReBindSettings(); //TODO: should raise some propertychanged event instead
                MyDownloadingViewModel.ReBindSettings();
            }
        }
        
        public void SaveSettings()
        {
                MySettingsViewModel.SaveActiveSettings();
                AppSettings.SaveToFile();
                MyStartPageViewModel.ReBindSettings(); //TODO: should raise some propertychanged event instead
                MyDownloadingViewModel.ReBindSettings();
        }

        public async Task SettingsCommand()
        {
            PreviousViewModel = SelectedTab;
            SelectedTab = MySettingsViewModel;
            if (PreviousViewModel == MyStartPageViewModel)
            {
                await MyStartPageViewModel.SaveActiveSettings();
                MySettingsViewModel.ReBindSettings();
            }
        }

        public async Task GoForward()
        {
            PreviousViewModel = SelectedTab;
            
            await MyStartPageViewModel.SaveActiveSettings();
            MyDownloadingViewModel.ReBindSettings();
            //TODO: make the goforwardbutton disabled when no file is selected
            if (AppSettings._settings.HtmlImportUsed)
            {
                MyDownloadingViewModel.FileSource = AppSettings._settings.Htmlfilelocation;
            }
            else
            {
                MyDownloadingViewModel.FileSource = MyStartPageViewModel.ChosenBrowser;
            }
            await MyDownloadingViewModel.LoadFoldersFromFile();
            SelectedTab = MyDownloadingViewModel;
        }

        public void BackToStartPage()
        {
            PreviousViewModel = SelectedTab;
            SelectedTab = MyStartPageViewModel;
        }
    }
}
