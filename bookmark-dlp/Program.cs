using Avalonia;
using NfLogger;
using Serilog;
using Serilog.Events;

namespace bookmark_dlp;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        #region serilog

        // 1. Configure Serilog right at the start.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Verbose) // Capture everything; we can filter at the sink level.
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .Enrich.FromLogContext() // This is crucial for adding context like SourceContext
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}")
            .WriteTo.File("bookmark-dlp-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}")
            .CreateLogger();

        #endregion
        
        
#if CLIMODE
#if DEBUG
            Logger.verbosity = Logger.Verbosity.Trace;
            Logger.LogVerbose("Program started in DEBUG mode");
            Log.Information("Application started in DEBUG mode");
#else
            Logger.verbosity = Logger.Verbosity.Warning;
#endif
            AppMethods.programUI = AppMethods.ProgramUI.CLI;
            Logger.LogVerbose("Program started in CLI mode");
            Log.Information("Application started in CLI mode");
            CoreLogic.CoreLogicMain(args); }
#else
#if DEBUG
        Logger.verbosity = Logger.Verbosity.Trace;
        Logger.LogVerbose("Program started in DEBUG mode");
        Log.Information("Application started in DEBUG mode");
#else
            Logger.verbosity = Logger.Verbosity.Warning;
#endif
        Logger.LogVerbose("Program started in GUI mode", Logger.Verbosity.Debug);
        Log.Information("Application started in GUI mode");
        if (args.Length == 0)
        {
            //
            // If WindowsOperations.SetWindowMode(WindowMode.Hidden) is used the console invoking the program will close after the program has started. This is undesirable
            // when the user intentionally launches ./bookmark-dlp from the terminal, but desirable if the app is launched by just double clicking on the executable.
            // Will leave it in for now, as launching from the terminal is a lot less likely in my opinion.
            //
            // WindowsOperations.SetWindowMode(WindowMode.Hidden);
            AppMethods.programUI = AppMethods.ProgramUI.GUI;
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        else
        {
            AppMethods.programUI = AppMethods.ProgramUI.CLI;
            CoreLogic.CoreLogicMain(args);
        }
        Log.CloseAndFlush();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
#endif
}