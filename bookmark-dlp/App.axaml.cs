using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.DependencyInjection; 
using Avalonia.Controls;
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
        
        private void ConfigureServices(IServiceCollection services, String? configpath_location)
        {
            services.AddSingleton<IAppSettings>(new AppSettings(configpath_location));
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<StartPageViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<DownloadingViewModel>();
            services.AddTransient<LogViewModel>();
        }

        /// <summary>
        /// Show AskConfigWindow popup if no config found, then show the MainWindow
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!AppMethods.IsConfigPresent())
                {
                    var askConfigViewModel = new AskConfigWindowViewModel();
                    var askConfigWindow = new AskConfigWindow
                    {
                        DataContext = askConfigViewModel,
                    };
                    desktop.MainWindow = askConfigWindow;
                    askConfigWindow.Show();

                    String? configpath_location = null;

                    MessageBus.ButtonClicked += (sender, buttonText) =>
                    {
                        //await Console.Out.WriteLineAsync(buttonText);
                        switch (buttonText)
                        {
                            case "Appdata/local":
                                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                                {
                                    configpath_location = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "bookmark-dlp/bookmark-dlp.conf");
                                }
                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                {
                                    configpath_location = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bookmark-dlp\\bookmark-dlp.conf");
                                }
                                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                                {
                                    configpath_location = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmark-dlp/bookmark-dlp.conf");
                                }
                                break;
                            case "No config":
                                configpath_location = null;
                                break;
                            case "local dir":
                                configpath_location = Path.Combine(Directory.GetCurrentDirectory(), "bookmark-dlp.conf");
                                break;
                            default:
                                configpath_location = null;
                                break;
                        }
                        
                        
                        // var mainWindowVm = new MainWindowViewModel();
                        var collection = new ServiceCollection();
                        ConfigureServices(collection, configpath_location);
                        var services = collection.BuildServiceProvider();
                        
                        MainWindowViewModel mainWindowVm = services.GetRequiredService<MainWindowViewModel>();
                        var mainWindow = new MainWindow
                        {
                            DataContext = mainWindowVm,
                        };
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        askConfigWindow.Close();
                    };
                }
                else 
                {
                    Logger.LogVerbose("Config was found", Logger.Verbosity.Debug);
                    Logger.LogVerbose("Location: " + AppMethods.ConfigFileLocation(), Logger.Verbosity.Debug);
                    // var mainWindowVm = new MainWindowViewModel();
                    var collection = new ServiceCollection();
                    ConfigureServices(collection, AppMethods.ConfigFileLocation());
                    var services = collection.BuildServiceProvider();
                    MainWindowViewModel mainWindowVm = services.GetRequiredService<MainWindowViewModel>();
                    var mainWindow = new MainWindow
                    {
                        DataContext = mainWindowVm,
                    };
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
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