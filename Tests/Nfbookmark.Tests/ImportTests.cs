using System.Dynamic;
using System.Reflection;
using bookmark_dlp;
using Xunit;
using Xunit.Abstractions;

namespace Nfbookmark.Tests
{
    public class ImportTests
    {
        static string test_data_files_prefix = @"../../../../test_data_files/";
        static string test_data_files_folder = Path.Combine(Directory.GetCurrentDirectory(), test_data_files_prefix);
        private readonly ITestOutputHelper _output;

        public ImportTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DataFilesExistTest()
        {
            Assert.True(Directory.Exists(test_data_files_folder));
            Assert.True(Directory.Exists(Path.Combine(test_data_files_folder, "chromium_linux")));
            Assert.True(Directory.Exists(Path.Combine(test_data_files_folder, "firefox_linux")));
        }
        
        #region Chromium

        [Theory]
        [InlineData("chromium_linux/bookmarks.json")]
        [InlineData("chromium_linux/exported_bookmarks.html")]
        public void JsonChromiumTest(string path)
        {
            string realpath = Path.Combine(test_data_files_folder, path);
            Bookmarks bookmark = new Bookmarks();
            Assert.Empty(bookmark.folders);
        }

        #endregion Chromium

        #region Firefox

        [Theory]
        [InlineData("firefox_linux/places.sqlite")]
        [InlineData("firefox_linux/saved_bookmarks.json")]
        [InlineData("firefox_linux/bookmarks.html")]
        public void Json_Firefox_Test(string path)
        {
            string realpath = Path.Combine(test_data_files_folder, path);
            Bookmarks bookmark = new Bookmarks();
            Assert.Empty(bookmark.folders);
        }

        #endregion Firefox

        #region SmartImport

        [Theory]
        [InlineData("chromium_linux/bookmarks.json")]
        [InlineData("firefox_linux/saved_bookmarks.json")]
        public void SmartImport_JsonTest(string path)
        {
            string realpath = Path.Combine(test_data_files_folder, path);
            Assert.True(File.Exists(realpath));
            Assert.True(Import.SmartImport(realpath).Equals(Import.JsonIntake(realpath)));
            Assert.Equal(Import.SmartImport(realpath), Import.JsonIntake(realpath));
        }
        
        [Theory]
        [InlineData("firefox_linux/places.sqlite")]
        public void SmartImport_SqlTest(string path)
        {
            string realpath = Path.Combine(test_data_files_folder, path);
            Assert.True(File.Exists(realpath));
            Assert.True(Import.SmartImport(realpath).Equals(Import.SqlIntake(realpath)));
            Assert.Equal(Import.SmartImport(realpath), Import.SqlIntake(realpath));
        }

        #endregion SmartImport
    }
}