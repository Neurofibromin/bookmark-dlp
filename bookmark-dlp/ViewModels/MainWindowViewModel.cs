using Avalonia.Controls;
using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace bookmark_dlp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /*[ObservableProperty] private bool _startPageEnabled = true;
    [ObservableProperty] private bool _settingsEnabled = true;
    [ObservableProperty] private bool _logEnabled = true;
    [ObservableProperty] private bool _downloadingEnabled = false;*/
    [ObservableProperty] private bool _importSuccess;
    [ObservableProperty] private DownloadingViewModel _myDownloadingViewModel;
    [ObservableProperty] private LogViewModel _myLogViewModel;
    [ObservableProperty] private SettingsViewModel _mySettingsViewModel;

    [ObservableProperty] private StartPageViewModel _myStartPageViewModel;
    [ObservableProperty] private TabItem? _previousSelectedTab;
    [ObservableProperty] private TabItem _selectedTab;

    [ObservableProperty] private List<TabItem> _tabItems;

    public MainWindowViewModel(StartPageViewModel startPageViewModel,
        SettingsViewModel settingsViewModel,
        LogViewModel logViewModel,
        DownloadingViewModel downloadingViewModel)
    {
        // Initialize ViewModels
        _myStartPageViewModel = startPageViewModel;
        _mySettingsViewModel = settingsViewModel;
        _myLogViewModel = logViewModel;
        _myDownloadingViewModel = downloadingViewModel;

        _tabItems = GetTabItems();
        _selectedTab = TabItems.First(x => x.Content == MyStartPageViewModel);
        PreviousSelectedTab = _selectedTab;
    }

    // Parameterless constructor for XAML designer support
    public MainWindowViewModel()
    {
        AppSettings appSettings = new AppSettings();
        _myStartPageViewModel = new StartPageViewModel(appSettings);
        _mySettingsViewModel = new SettingsViewModel(appSettings);
        _myLogViewModel = new LogViewModel();
        _myDownloadingViewModel = new DownloadingViewModel(appSettings);

        _tabItems = GetTabItems();
        _selectedTab = _tabItems.First();
    }

    private List<TabItem> GetTabItems()
    {
        /*<TabItem Name="Sources" Header="Sources" Content="{Binding MyStartPageViewModel }" IsEnabled="{Binding StartPageEnabled}"/>
            <TabItem Name="Imported" Header="Imported" Content="{Binding MyDownloadingViewModel}" IsEnabled="{Binding DownloadingEnabled}"/>
            <TabItem Name="Log" Header="Log" Content="{Binding MyLogViewModel}" IsEnabled="{Binding LogEnabled}"/>
            <TabItem Name="Settings" Header="Settings" Content="{Binding MySettingsViewModel}" IsEnabled="{Binding SettingsEnabled}"/>*/
        return new List<TabItem>
        {
            new TabItem { Header = "Sources", Content = MyStartPageViewModel, IsEnabled = true },
            new TabItem { Header = "Imported", Content = MyDownloadingViewModel, IsEnabled = false },
            new TabItem { Header = "Log", Content = MyLogViewModel, IsEnabled = true },
            new TabItem { Header = "Settings", Content = MySettingsViewModel, IsEnabled = true }
        };
    }

    public void SettingsCommand()
    {
        PreviousSelectedTab = SelectedTab;
        SelectedTab = TabItems.First(x => x.Content == MySettingsViewModel);
    }

    public void GoForward()
    {
        //PreviousViewModel = SelectedTab;
        MyDownloadingViewModel.FileSource = MyStartPageViewModel.ActiveSettings.ManualImportUsed
            ? MyStartPageViewModel.ActiveSettings.Manualimportfilelocation
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