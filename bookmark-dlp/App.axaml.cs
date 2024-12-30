using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using System.Runtime.InteropServices;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using bookmark_dlp.Models;
using bookmark_dlp.ViewModels;
using bookmark_dlp.Views;
using NfLogger;

namespace bookmark_dlp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                if (!AppMethods.IsConfigPresent())
                {
                    var askConfigViewModel = new AskConfigWindowViewModel();
                    var askConfigWindow = new AskConfigWindow
                    {
                        DataContext = askConfigViewModel,
                    };
                    desktop.MainWindow = askConfigWindow;
                    askConfigWindow.Show();

                    MessageBus.ButtonClicked += async (sender, buttonText) =>
                    {
                        await Console.Out.WriteLineAsync(buttonText);
                        switch (buttonText)
                        {
                            case "Appdata/local":
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                                {
                                    string configpath_osx = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "bookmark-dlp/bookmark-dlp.conf");
                                    AppSettings.configloc = configpath_osx;
                                }
                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                {
                                    string configpath_windows = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bookmark-dlp\\bookmark-dlp.conf");
                                    AppSettings.configloc = configpath_windows;
                                }
                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                {
                                    string configpath_linux = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmark-dlp/bookmark-dlp.conf");
                                    AppSettings.configloc = configpath_linux;
                                }
                                break;
                            case "No config":
                                AppSettings.configloc = null;
                                break;
                            case "local dir":
                                AppSettings.configloc = Path.Combine(Directory.GetCurrentDirectory(), "bookmark-dlp.conf");
                                break;
                            default:
                                AppSettings.configloc = null;
                                throw new Exception("Should not have happened");
                                Environment.Exit(1);
                            
                        }
                        
                        if (false)
                        {
                            var mainWindowVM = new MainWindowViewModel();
                            var MainWindow = new MainWindow
                            {
                                DataContext = mainWindowVM,
                            };
                            desktop.MainWindow = MainWindow;
                            MainWindow.Show();
                        }
                        else
                        {
                            var TabsWindowVM = new TabsMainWindowViewModel();
                            var TabsMainWindow = new TabsMainWindow()
                            {
                                DataContext = TabsWindowVM,
                            };
                            desktop.MainWindow = TabsMainWindow;
                            TabsMainWindow.Show();
                        }
                        
                        askConfigWindow.Close();
                    };
                }
                else 
                {
                    Console.WriteLine("Config was found");
                    Console.WriteLine("Location: " + AppMethods.ConfigFileLocation());
                    if (false)
                    {
                        var mainWindowVM = new MainWindowViewModel();
                        var MainWindow = new MainWindow
                        {
                            DataContext = mainWindowVM,
                        };
                        desktop.MainWindow = MainWindow;
                        MainWindow.Show();
                    }
                    else
                    {
                        var TabsWindowVM = new TabsMainWindowViewModel();
                        var TabsMainWindow = new TabsMainWindow()
                        {
                            DataContext = TabsWindowVM,
                        };
                        desktop.MainWindow = TabsMainWindow;
                        TabsMainWindow.Show();
                    }
                }
            }
            else
            {
                Logger.LogVerbose("App not desktop, unable to initialize");
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}