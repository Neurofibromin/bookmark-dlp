using Avalonia;
using System;

namespace bookmark_dlp
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ///
                /// If WindowsOperations.SetWindowMode(WindowMode.Hidden) is used the console invoking the program will close after the program has started. This is undesirable
                /// when the user intentionally launches ./bookmark-dlp from the terminal, but desirable if the app is launched by just double clicking on the executable.
                /// Will leave it in for now, as launching from the terminal is a lot less likely in my opinion.
                ///
                //WindowsOperations.SetWindowMode(WindowMode.Hidden);
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                return;
            }
            CoreLogic.CoreLogicMain();
            // Handling other arguments for console application style behaviour
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }


}
