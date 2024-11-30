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
        StartPageViewModel myStartPageViewModel;
        // public StartPageViewModel MyStartPageViewModel { get; set; }

        DownloadingViewModel myDownloadingViewModel;
        // public DownloadingViewModel MyDownloadingViewModel { get; set; }

        SettingsViewModel mySettingsViewModel;
        // public SettingsViewModel MySettingsViewModel { get; set; }
        

        [ObservableProperty]
        public ViewModelBase? contentViewModel;
        [ObservableProperty]
        public ViewModelBase? previousViewModel;


        public MainWindowViewModel()
        {
            // Console.WriteLine("In orig at mwvm: " + JsonConvert.SerializeObject(AppSettings._settings));
            mySettingsViewModel = new SettingsViewModel();
            myStartPageViewModel = new StartPageViewModel();
            myDownloadingViewModel = new DownloadingViewModel();

            ContentViewModel = myStartPageViewModel;
            PreviousViewModel = myStartPageViewModel;
        }
        /*
        public StartPageViewModel StartPage { get ; set; }
        public DownloadingViewModel Downloading { get; set; }
        public SettingsViewModel Settings { get; set; }*/
        public void GoBack() //also Bound by the savesettingsbutton
        {
            (PreviousViewModel, ContentViewModel) = (ContentViewModel, PreviousViewModel);
            if (PreviousViewModel == mySettingsViewModel)
            {
                mySettingsViewModel.SaveActiveSettings();
                AppSettings.SaveToFile();
                myStartPageViewModel.ReBindSettings(); //TODO: should raise some propertychanged event instead
                myDownloadingViewModel.ReBindSettings();
            }
        }

        public async Task SettingsCommand()
        {
            PreviousViewModel = ContentViewModel;
            ContentViewModel = mySettingsViewModel;
            if (PreviousViewModel == myStartPageViewModel)
            {
                await myStartPageViewModel.SaveActiveSettings();
                mySettingsViewModel.ReBindSettings();
            }
        }

        public async Task GoForward()
        {
            PreviousViewModel = ContentViewModel;
            
            await myStartPageViewModel.SaveActiveSettings();
            myDownloadingViewModel.ReBindSettings();
            //TODO: make the goforwardbutton disabled when no file is selected
            if (AppSettings._settings.HtmlImportUsed)
            {
                myDownloadingViewModel.FileSource = AppSettings._settings.Htmlfilelocation;
            }
            else
            {
                myDownloadingViewModel.FileSource = myStartPageViewModel.ChosenBrowser;
            }
            await myDownloadingViewModel.LoadFoldersFromFile();
            ContentViewModel = myDownloadingViewModel;
        }

        public void BackToStartPage()
        {
            PreviousViewModel = ContentViewModel;
            ContentViewModel = myStartPageViewModel;
        }
    }
}
