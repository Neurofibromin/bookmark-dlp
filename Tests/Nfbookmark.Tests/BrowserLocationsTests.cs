using Xunit;

namespace Nfbookmark.Tests;

public class BrowserLocationsTests
{
    [Fact]
    public void Get_List_from_GetBrowserLocations()
    {
        List<BrowserLocations> browserLocations = BrowserLocations.GetDefaultBrowserConfigurations();
        Assert.NotEmpty(browserLocations);
        Assert.Equal("Chrome", browserLocations.First().BrowserName);
        foreach (BrowserLocations browser in browserLocations)
        {
            Assert.NotNull(browser.WindowsProfilesPath);
            Assert.NotEmpty(browser.LinuxProfilesPaths);
            Assert.NotEmpty(browser.OsxProfilesPaths);
        }
    }
}