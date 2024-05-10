using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using bookmark_dlp.Models;
using bookmark_dlp.ViewModels;
using bookmark_dlp.Views;

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

                if (!Methods.IsConfigPresent())
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
                        var mainWindowVM = new MainWindowViewModel();
                        var MainWindow = new MainWindow
                        {
                            DataContext = mainWindowVM,
                        };

                        desktop.MainWindow = MainWindow;
                        MainWindow.Show();
                        askConfigWindow.Close();
                    };
                }
                else 
                {
                    Console.WriteLine("Config was found");
                    var mainWindowVM = new MainWindowViewModel();
                    var MainWindow = new MainWindow
                    {
                        DataContext = mainWindowVM,
                    };

                    desktop.MainWindow = MainWindow;
                    MainWindow.Show();
                }
                







            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}