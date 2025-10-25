using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Nfbookmark.Tests;

public class ImportTests
{
    private static readonly string TestDataFilesPrefix = @"../../../../test_data_files/";

    private static readonly string TestDataFilesFolder =
        Path.Combine(Directory.GetCurrentDirectory(), TestDataFilesPrefix);

    private readonly ITestOutputHelper _output;

    public ImportTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Compares two lists of Folderclass objects and asserts their equality.
    /// Provides detailed output on any discrepancies found.
    /// </summary>
    /// <param name="expected">The expected list of folders.</param>
    /// <param name="actual">The actual list of folders produced by the test.</param>
    private void AssertFolderListsAreEqual(List<Folderclass> expected, List<Folderclass> actual)
    {
        // Handle cases where one or both lists might be null.
        if (expected == null || actual == null)
        {
            Assert.True(expected == null && actual == null, "One list is null while the other is not.");
            return;
        }

        // Provide a clear message if counts differ.
        Assert.True(expected.Count == actual.Count, $"Expected {expected.Count} folders, but got {actual.Count}.");

        // Order lists to ensure a consistent comparison.
        var sortedExpected = expected.OrderBy(f => f.startline).ThenBy(f => f.name).ToList();
        var sortedActual = actual.OrderBy(f => f.startline).ThenBy(f => f.name).ToList();

        for (int i = 0; i < sortedExpected.Count; i++)
        {
            var e = sortedExpected[i];
            var a = sortedActual[i];

            // Use the comprehensive Equals method, and if it fails, get a detailed difference string.
            Assert.True(e.Equals(a), GetDifferenceString(e, a));
        }
    }

    /// <summary>
    /// Generates a detailed string comparing two Folderclass objects to highlight their differences.
    /// </summary>
    /// <param name="expected">The expected Folderclass object.</param>
    /// <param name="actual">The actual Folderclass object.</param>
    /// <returns>A detailed string of differences.</returns>
    private string GetDifferenceString(Folderclass expected, Folderclass actual)
    {
        if (expected.Equals(actual)) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"\nASSERTION FAILED: Folder objects are not equal.");
        sb.AppendLine($"--> Expected Name: '{expected.name}', ID: {expected.id}");
        sb.AppendLine($"--> Actual Name:   '{actual.name}', ID: {actual.id}");

        if (expected.id != actual.id) sb.AppendLine($"  [Mismatch] ID: Expected='{expected.id}', Actual='{actual.id}'");
        if (expected.parentId != actual.parentId) sb.AppendLine($"  [Mismatch] ParentId: Expected='{expected.parentId}', Actual='{actual.parentId}'");
        if (expected.depth != actual.depth) sb.AppendLine($"  [Mismatch] Depth: Expected='{expected.depth}', Actual='{actual.depth}'");
        if (expected.name != actual.name) sb.AppendLine($"  [Mismatch] Name: Expected='{expected.name}', Actual='{actual.name}'");
        if (expected.startline != actual.startline) sb.AppendLine($"  [Mismatch] StartLine: Expected='{expected.startline}', Actual='{actual.startline}'");
        if (expected.endingline != actual.endingline) sb.AppendLine($"  [Mismatch] EndingLine: Expected='{expected.endingline}', Actual='{actual.endingline}'");
        if (expected.folderpath != actual.folderpath) sb.AppendLine($"  [Mismatch] FolderPath: Expected='{expected.folderpath}', Actual='{actual.folderpath}'");

        var expectedUrls = expected.urls.OrderBy(u => u).ToList();
        var actualUrls = actual.urls.OrderBy(u => u).ToList();
        if (!expectedUrls.SequenceEqual(actualUrls))
        {
            sb.AppendLine("  [Mismatch] URLs are different.");
            sb.AppendLine($"    Expected ({expectedUrls.Count}): [{string.Join(", ", expectedUrls.Take(3))}{(expectedUrls.Count > 3 ? "..." : "")}]");
            sb.AppendLine($"    Actual   ({actualUrls.Count}): [{string.Join(", ", actualUrls.Take(3))}{(actualUrls.Count > 3 ? "..." : "")}]");
        }

        var expectedChildren = expected.childrenIds.OrderBy(id => id).ToList();
        var actualChildren = actual.childrenIds.OrderBy(id => id).ToList();
        if (!expectedChildren.SequenceEqual(actualChildren))
        {
            sb.AppendLine("  [Mismatch] Children IDs are different.");
            sb.AppendLine($"    Expected ({expectedChildren.Count}): [{string.Join(", ", expectedChildren)}]");
            sb.AppendLine($"    Actual   ({actualChildren.Count}): [{string.Join(", ", actualChildren)}]");
        }

        return sb.ToString();
    }
    
    [Fact]
    public void DataFilesExistTest()
    {
        Assert.True(Directory.Exists(TestDataFilesFolder));
        Assert.True(Directory.Exists(Path.Combine(TestDataFilesFolder, "chromium_linux")));
        Assert.True(Directory.Exists(Path.Combine(TestDataFilesFolder, "firefox_linux")));
    }

    #region Chromium
    
    [Theory]
    [InlineData("chromium_linux/bookmarks.json")]
    public void JsonChromiumTest(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        List<Folderclass> folders = Import.JsonIntake(realpath);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (StreamWriter writer = new StreamWriter(memoryStream))
            {
                Legacy.PrintToStream(folders, false, memoryStream);
                writer.Flush();

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(memoryStream))
                {
                    _output.WriteLine(reader.ReadToEnd());
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
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        List<Folderclass> folders = Import.HtmlExportIntake(realpath);
        Assert.NotNull(folders);
        Assert.True(folders.Count > 0);
        Assert.Equal(20, folders.Count);
    }
    
    [Theory]
    [InlineData("chromium_linux/dummy_takeout_bookmarks.html")]
    public void HtmlTakeoutChromiumTest(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        List<Folderclass> folders = Import.HtmlExportIntake(realpath);
        Assert.NotNull(folders);
        Assert.True(folders.Count > 0);
        Assert.Equal(20, folders.Count);
    }

    #endregion Chromium

    #region Firefox

    [Theory]
    [InlineData("firefox_linux/saved_bookmarks.json")]
    public void Json_Firefox_Test(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        var folders = Import.JsonIntake(realpath);
        Assert.NotNull(folders);
    }

    [Theory]
    [InlineData("firefox_linux/bookmarks.html")]
    public void Html_Firefox_Test(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        var folders = Import.HtmlExportIntake(realpath);
        Assert.NotNull(folders);
    }

    [Theory]
    [InlineData("firefox_linux/places.sqlite")]
    public void Sql_Firefox_Test(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        List<Folderclass> folders = Import.SqlIntake(realpath);
        Assert.NotNull(folders);
    }

    #endregion Firefox

    #region SmartImport

    [Theory]
    [InlineData("chromium_linux/bookmarks.json")]
    [InlineData("firefox_linux/saved_bookmarks.json")]
    public void SmartImport_JsonTest(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        var smartImportResult = Import.SmartImport(realpath);
        var jsonIntakeResult = Import.JsonIntake(realpath);

        _output.WriteLine($"Comparing results for {path}");
        Assert.NotNull(smartImportResult);
        Assert.NotNull(jsonIntakeResult);
        
        AssertFolderListsAreEqual(jsonIntakeResult, smartImportResult);
    }

    [Theory]
    [InlineData("chromium_linux/exported_bookmarks.html")]
    [InlineData("firefox_linux/bookmarks.html")]
    public void SmartImport_HtmlTest(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        
        var smartImportResult = Import.SmartImport(realpath);
        var htmlIntakeResult = Import.HtmlExportIntake(realpath); 
        
        _output.WriteLine($"Comparing results for {path}");
        Assert.NotNull(smartImportResult);
        Assert.NotNull(htmlIntakeResult);

        AssertFolderListsAreEqual(htmlIntakeResult, smartImportResult);
    }

    [Theory]
    [InlineData("firefox_linux/places.sqlite")]
    public void SmartImport_SqlTest(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        var smartImportResult = Import.SmartImport(realpath);
        var sqlIntakeResult = Import.SqlIntake(realpath);

        Assert.NotNull(smartImportResult);
        Assert.NotNull(sqlIntakeResult);

        AssertFolderListsAreEqual(sqlIntakeResult, smartImportResult);
    }

    #endregion SmartImport
}