using bookmark_dlp;
using Xunit;
using Xunit.Abstractions;

namespace Nfbookmark.Tests;

public class ImportTests
{
    private static readonly string test_data_files_prefix = @"../../../../test_data_files/";

    private static readonly string test_data_files_folder =
        Path.Combine(Directory.GetCurrentDirectory(), test_data_files_prefix);

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

    //TODO set up html takeoutimport tests
    [Theory]
    [InlineData("chromium_linux/bookmarks.json")]
    public void JsonChromiumTest(string path)
    {
        string realpath = Path.Combine(test_data_files_folder, path);
        List<Folderclass> folders = Import.JsonIntake(realpath);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (StreamWriter writer = new StreamWriter(memoryStream))
            {
                Functions.PrintToStream(folders, false, memoryStream);
                writer.Flush(); // Ensure everything is written to the stream

                memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position
                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    _output.WriteLine(reader.ReadToEnd()); // Log output to xUnit test runner
                }
            }
        }

        Assert.NotNull(folders);
        Assert.True(folders.Count > 0);
        //Assert.Equal(20 , folders.Count); //check this
    }

    [Theory]
    [InlineData("chromium_linux/exported_bookmarks.html")]
    public void HtmlExportedChromiumTest(string path)
    {
        string realpath = Path.Combine(test_data_files_folder, path);
        List<Folderclass> folders = Import.HtmlExportIntake(realpath);
        Assert.NotNull(folders);
        Assert.True(folders.Count > 0);
        Assert.Equal(10, folders.Count); //check this
    }

    #endregion Chromium

    #region Firefox

    [Theory]
    [InlineData("firefox_linux/saved_bookmarks.json")]
    public void Json_Firefox_Test(string path)
    {
        string realpath = Path.Combine(test_data_files_folder, path);
        List<Folderclass> folders = Import.JsonIntake(realpath);
    }

    [Theory]
    [InlineData("firefox_linux/bookmarks.html")]
    public void Html_Firefox_Test(string path)
    {
        string realpath = Path.Combine(test_data_files_folder, path);
        List<Folderclass> folders = Import.HtmlExportIntake(realpath);
    }

    [Theory]
    [InlineData("firefox_linux/places.sqlite")]
    public void Sql_Firefox_Test(string path)
    {
        string realpath = Path.Combine(test_data_files_folder, path);
        List<Folderclass> folders = Import.SqlIntake(realpath);
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
    [InlineData("chromium_linux/exported_bookmarks.html")]
    [InlineData("firefox_linux/bookmarks.html")]
    public void SmartImport_HtmlTest(string path)
    {
        string realpath = Path.Combine(test_data_files_folder, path);
        Assert.True(File.Exists(realpath));
        Assert.True(Import.SmartImport(realpath).Equals(Import.HtmlExportIntake(realpath)));
        Assert.Equal(Import.SmartImport(realpath), Import.HtmlExportIntake(realpath));
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