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
        [ObservableProperty] private TabItem _selectedTab;
        [ObservableProperty] private TabItem? _previousSelectedTab;
        
        [ObservableProperty] private StartPageViewModel _myStartPageViewModel;
        [ObservableProperty] private SettingsViewModel _mySettingsViewModel;
        [ObservableProperty] private LogViewModel _myLogViewModel;
        [ObservableProperty] private DownloadingViewModel _myDownloadingViewModel;
        
        /*[ObservableProperty] private bool _startPageEnabled = true;
        [ObservableProperty] private bool _settingsEnabled = true;
        [ObservableProperty] private bool _logEnabled = true;
        [ObservableProperty] private bool _downloadingEnabled = false;*/
        [ObservableProperty] private bool _importSuccess = false;

        [ObservableProperty] private List<TabItem> _tabItems;
        
        public MainWindowViewModel( StartPageViewModel startPageViewModel, 
            SettingsViewModel settingsViewModel, 
            LogViewModel logViewModel, 
            DownloadingViewModel downloadingViewModel)
        {
            // Initialize ViewModels
            _myStartPageViewModel = startPageViewModel;
            _mySettingsViewModel = settingsViewModel;
            _myLogViewModel = logViewModel;
            _myDownloadingViewModel = downloadingViewModel;
            
            TabItems = GetTabItems();
            SelectedTab = TabItems.First(x => x.Content == MyStartPageViewModel);
            PreviousSelectedTab = SelectedTab;
        }
        
        public MainWindowViewModel() {}

        private List<TabItem> GetTabItems()
        {
            /*<TabItem Name="Sources" Header="Sources" Content="{Binding MyStartPageViewModel }" IsEnabled="{Binding StartPageEnabled}"/>
                <TabItem Name="Imported" Header="Imported" Content="{Binding MyDownloadingViewModel}" IsEnabled="{Binding DownloadingEnabled}"/>
                <TabItem Name="Log" Header="Log" Content="{Binding MyLogViewModel}" IsEnabled="{Binding LogEnabled}"/>
                <TabItem Name="Settings" Header="Settings" Content="{Binding MySettingsViewModel}" IsEnabled="{Binding SettingsEnabled}"/>*/
            List<TabItem> tabItems = new List<TabItem>()
            {
                new TabItem()
                {
                    Header = "Sources",
                    Content = MyStartPageViewModel,
                    IsEnabled = true
                },
                new TabItem()
                {
                    Header = "Imported",
                    Content = MyDownloadingViewModel,
                    IsEnabled = false
                },
                new TabItem()
                {
                    Header = "Log",
                    Content = MyLogViewModel,
                    IsEnabled = true
                },
                new TabItem()
                {
                    Header = "Settings",
                    Content = MySettingsViewModel,
                    IsEnabled = true
                },
            };
            return tabItems;
        }
        
        public void SettingsCommand()
        {
            PreviousSelectedTab = SelectedTab;
            SelectedTab = TabItems.First(x => x.Content == MySettingsViewModel);
        }

        public void GoForward()
        {
            //PreviousViewModel = SelectedTab;
            MyDownloadingViewModel.FileSource = AppSettings._settings.ManualImportUsed
                ? AppSettings._settings.Manualimportfilelocation
                : MyStartPageViewModel.ChosenBrowser;
            ImportSuccess = MyDownloadingViewModel.LoadFoldersFromFile();
            if (ImportSuccess)
            {
                TabItems.First(x => x.Content == MyDownloadingViewModel).IsEnabled = true;
                SelectedTab = TabItems.First(x => x.Content == MyDownloadingViewModel);
            }
            else
            {
                TabItems.First(x => x.Content == MyDownloadingViewModel).IsEnabled = false;
                MyStartPageViewModel.EnableImportButton = false;
                MyStartPageViewModel.ImportButtonToolTip = "Import Failed";
            }
        }

        public void BackToStartPage()
        {
            PreviousSelectedTab = SelectedTab;
            SelectedTab = TabItems.First(x => x.Content == MyStartPageViewModel);
        }

        /*partial void OnSelectedTabChanged(TabItem? value)
        {
            //Console.WriteLine(SelectedTab.Header);
        }*/
    }
}
