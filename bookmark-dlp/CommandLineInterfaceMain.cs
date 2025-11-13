using Serilog;
using Serilog.Events;

namespace bookmark_dlp;

public class CommandLineInterfaceMain
{
    public static void Entrypoint(string[] args)
    {
        #region serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Verbose)
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}")
            .WriteTo.File("bookmark-dlp-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}")
            .CreateLogger();

        #endregion
        
        
        
#if DEBUG
        Log.Debug("Application started in DEBUG mode");
#else
            Logger.verbosity = Logger.Verbosity.Warning;
#endif
        AppMethods.programUI = AppMethods.ProgramUI.CLI;
        Log.Debug("Application started in CLI mode");
        CoreLogic.CoreLogicMain(args);
        System.Environment.Exit(0);
    }
}