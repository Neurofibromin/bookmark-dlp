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
    public partial class MainWindowViewModel : ViewModelBase
    {


        [ObservableProperty]
        public ViewModelBase? contentViewModel;
        [ObservableProperty]
        public ViewModelBase? previousViewModel;


        public MainWindowViewModel()
        {

            StartPage = new StartPageViewModel();
            Settings = new SettingsViewModel();
            Downloading = new DownloadingViewModel();

            ContentViewModel = StartPage;
            PreviousViewModel = StartPage;

        }

        public StartPageViewModel StartPage { get; set; }
        public DownloadingViewModel Downloading { get; set; }
        public SettingsViewModel Settings { get; set; }
        public void GoBack()
        {
            (PreviousViewModel, ContentViewModel) = (ContentViewModel, PreviousViewModel);
        }

        public void SettingsCommand()
        {
            PreviousViewModel = ContentViewModel;
            ContentViewModel = Settings;
        }

        public void GoForward()
        {
            PreviousViewModel = ContentViewModel;
            ContentViewModel = Downloading;
        }

        public void BackToStartPage()
        {
            PreviousViewModel = ContentViewModel;
            ContentViewModel = StartPage;
        }
    }
}
