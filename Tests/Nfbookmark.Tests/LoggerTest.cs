using NfLogger;
using Xunit;

namespace Nfbookmark.Tests
{
    public class LoggerTest
    {
        [Fact]
        public void Is_default_verbosity_warning()
        {
            Assert.Equal(Logger.verbosity, Logger.Verbosity.Info);
        }
    }    
}