using Avalonia;
using bookmark_dlp.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace bookmark_dlp;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            CommandLineInterfaceMain.Entrypoint(args);    
        }
        
        #region serilog

        // 1. Configure Serilog right at the start.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Verbose) // Capture everything; we can filter at the sink level.
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif
            .Enrich.FromLogContext() // This is crucial for adding context like SourceContext
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}")
            .WriteTo.File("bookmark-dlp-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}")
            .WriteTo.Sink(new ObservableStreamSink(App.UiLogStream, 
                new MessageTemplateTextFormatter("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}")))
            .CreateLogger();

        #endregion
        
        

#if DEBUG
        Log.Debug("Application started in DEBUG mode");
#endif
        Log.Debug("Application started in GUI mode");
        //
        // If WindowsOperations.SetWindowMode(WindowMode.Hidden) is used the console invoking the program will close after the program has started. This is undesirable
        // when the user intentionally launches ./bookmark-dlp from the terminal, but desirable if the app is launched by just double clicking on the executable.
        // Will leave it in for now, as launching from the terminal is a lot less likely in my opinion.
        //
        // WindowsOperations.SetWindowMode(WindowMode.Hidden);
        AppMethods.programUI = AppMethods.ProgramUI.GUI;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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

}