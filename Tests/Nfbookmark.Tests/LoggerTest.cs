using NfLogger;
using Xunit;


public class LoggerTest : IDisposable
{
    private MemoryStream _logStream;

    // Constructor: This runs before each test in the class
    public LoggerTest()
    {
        // Set up a fresh stream for each test
        _logStream = new MemoryStream();
        Logger.AddStream(_logStream);
        
        // Ensure verbosity is at a known state for each test
        Logger.verbosity = Logger.Verbosity.Info;
    }

    // Dispose: This runs after each test in the class
    public void Dispose()
    {
        // Remove the stream from the static logger
        Logger.RemoveStream(_logStream);
        // Dispose the stream itself
        _logStream.Dispose();
    }
    
    [Fact]
    public void Logger_Is_default_verbosity_Info()
    {
        Assert.Equal(Logger.Verbosity.Info, Logger.verbosity);
    }

    [Fact]
    public void Logger_CheckStreamLogging()
    {
        // Arrange is now done in the constructor

        // Act - Log a Trace message (should not be written)
        Logger.LogVerbose("This is a trace message", Logger.Verbosity.Trace);
        Assert.Equal(0, _logStream.Length);

        // Act - Log a Critical message (should be written)
        Logger.LogVerbose("This is a critical message", Logger.Verbosity.Critical);

        // Assert
        _logStream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new StreamReader(_logStream);
        string logContent = reader.ReadToEnd();
        
        // The logger prepends the verbosity level, so check for that too
        Assert.Contains("Critical: This is a critical message", logContent);
    }
}