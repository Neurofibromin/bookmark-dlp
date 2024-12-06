using bookmark_dlp;
using Xunit;

namespace Nfbookmark.Tests
{
    public class BrowserLocationsTests
    {
        [Fact]
        public void Get_List_from_GetBrowserLocations()
        {
            List<BrowserLocations> browserLocations = BrowserLocations.GetBrowserLocations();
            Assert.NotEmpty(browserLocations);
            Assert.Equal("Chrome", browserLocations.First().browsername);
            foreach (var browser in browserLocations)
            {
                Assert.NotNull(browser.windows_profilespath);
                Assert.NotEmpty(browser.linux_profilespath);
                Assert.NotEmpty(browser.osx_profilespath);
            }
        }
    }    
}