using NfLogger;
using Xunit;

namespace Nfbookmark.Tests
{
    public class LoggerTest
    {
        [Fact]
        public void Logger_Is_default_verbosity_Info()
        {
            Assert.Equal(Logger.Verbosity.Info, Logger.verbosity);
        }

        [Fact]
        public void Logger_CheckStreamLogging()
        {
            // Arrange
            using MemoryStream logStream = new MemoryStream();
            Logger.AddStream(logStream);

            // Act - Log a Trace message (should not be written due to verbosity level)
            Logger.LogVerbose("This is a trace message", Logger.Verbosity.Trace);

            // Assert - Check the stream, it should not have anything written to it
            Assert.Equal(0, logStream.Length);

            // Act - Log a Critical message (should be written to the stream)
            Logger.LogVerbose("This is a critical message", Logger.Verbosity.Critical);

            // Assert - Check the stream, it should have the critical message written to it
            logStream.Seek(0, SeekOrigin.Begin); // Reset the position of the stream for reading
            using StreamReader reader = new StreamReader(logStream);
            string logContent = reader.ReadToEnd();
            Assert.Contains("This is a critical message", logContent);
        }
    }    
}