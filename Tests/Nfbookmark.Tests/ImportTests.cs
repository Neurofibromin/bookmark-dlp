using System.Text;
using Nfbookmark.Importers;
using Xunit;

namespace Nfbookmark.Tests;

public class ImportTests
{
    private static readonly string TestDataFilesFolder =
        Path.Combine(AppContext.BaseDirectory, "test_data_files");

    private readonly ITestOutputHelper _output;

    public ImportTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Compares two lists of ImportedFolder objects and asserts their equality.
    /// Provides detailed output on any discrepancies found.
    /// </summary>
    /// <param name="expected">The expected list of folders.</param>
    /// <param name="actual">The actual list of folders produced by the test.</param>
    private void AssertFolderListsAreEqual(List<ImportedFolder> expected, List<ImportedFolder> actual)
    {
        if (expected == null || actual == null)
        {
            Assert.True(expected == null && actual == null, "One list is null while the other is not.");
            return;
        }

        Assert.True(expected.Count == actual.Count, $"Expected {expected.Count} folders, but got {actual.Count}.");

        var sortedExpected = expected.OrderBy(f => f.StartLine).ThenBy(f => f.Name).ToList();
        var sortedActual = actual.OrderBy(f => f.StartLine).ThenBy(f => f.Name).ToList();

        for (int i = 0; i < sortedExpected.Count; i++)
        {
            var e = sortedExpected[i];
            var a = sortedActual[i];

            Assert.True(FoldersAreEqual(e, a), GetDifferenceString(e, a));
        }
    }

    /// <summary>
    /// Value-equality check for ImportedFolder, since Equals currently throws NotImplementedException.
    /// </summary>
    private static bool FoldersAreEqual(ImportedFolder a, ImportedFolder b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;

        return a.Id == b.Id &&
               a.ParentId == b.ParentId &&
               a.Depth == b.Depth &&
               a.StartLine == b.StartLine &&
               a.Name == b.Name &&
               a.urls.OrderBy(u => u).SequenceEqual(b.urls.OrderBy(u => u)) &&
               a.ChildrenIds.OrderBy(id => id).SequenceEqual(b.ChildrenIds.OrderBy(id => id));
    }

    /// <summary>
    /// Generates a detailed string comparing two ImportedFolder objects to highlight their differences.
    /// </summary>
    private string GetDifferenceString(ImportedFolder expected, ImportedFolder actual)
    {
        if (FoldersAreEqual(expected, actual)) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"\nASSERTION FAILED: Folder objects are not equal.");
        sb.AppendLine($"--> Expected Name: '{expected.Name}', ID: {expected.Id}");
        sb.AppendLine($"--> Actual Name:   '{actual.Name}', ID: {actual.Id}");

        if (expected.Id != actual.Id)           sb.AppendLine($"  [Mismatch] ID: Expected='{expected.Id}', Actual='{actual.Id}'");
        if (expected.ParentId != actual.ParentId) sb.AppendLine($"  [Mismatch] ParentId: Expected='{expected.ParentId}', Actual='{actual.ParentId}'");
        if (expected.Depth != actual.Depth)     sb.AppendLine($"  [Mismatch] Depth: Expected='{expected.Depth}', Actual='{actual.Depth}'");
        if (expected.Name != actual.Name)       sb.AppendLine($"  [Mismatch] Name: Expected='{expected.Name}', Actual='{actual.Name}'");
        if (expected.StartLine != actual.StartLine) sb.AppendLine($"  [Mismatch] StartLine: Expected='{expected.StartLine}', Actual='{actual.StartLine}'");

        var expectedUrls = expected.urls.OrderBy(u => u).ToList();
        var actualUrls = actual.urls.OrderBy(u => u).ToList();
        if (!expectedUrls.SequenceEqual(actualUrls))
        {
            sb.AppendLine("  [Mismatch] URLs are different.");
            sb.AppendLine($"    Expected ({expectedUrls.Count}): [{string.Join(", ", expectedUrls.Take(3))}{(expectedUrls.Count > 3 ? "..." : "")}]");
            sb.AppendLine($"    Actual   ({actualUrls.Count}): [{string.Join(", ", actualUrls.Take(3))}{(actualUrls.Count > 3 ? "..." : "")}]");
        }

        var expectedChildren = expected.ChildrenIds.OrderBy(id => id).ToList();
        var actualChildren = actual.ChildrenIds.OrderBy(id => id).ToList();
        if (!expectedChildren.SequenceEqual(actualChildren))
        {
            sb.AppendLine("  [Mismatch] Children IDs are different.");
            sb.AppendLine($"    Expected ({expectedChildren.Count}): [{string.Join(", ", expectedChildren)}]");
            sb.AppendLine($"    Actual   ({actualChildren.Count}): [{string.Join(", ", actualChildren)}]");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Prints a compact representation of a folder list to the test output, replacing the
    /// Legacy.PrintToStream call which now requires a fully resolved MappedFolder/ResolvedFolder pipeline.
    /// </summary>
    private void PrintFoldersToOutput(List<ImportedFolder> folders)
    {
        if (folders == null || folders.Count == 0)
        {
            _output.WriteLine("No folders to display.");
            return;
        }

        foreach (var folder in folders.OrderBy(f => f.Id))
        {
            string indent = new string(' ', folder.Depth * 2);
            _output.WriteLine($"{indent}[{folder.Id}] '{folder.Name}' depth:{folder.Depth} parentId:{folder.ParentId} urls:{folder.urls.Count} startLine:{folder.StartLine}");
        }
        _output.WriteLine($"Altogether {folders.Count} folders were found.");
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
        IBookmarkImporter importer = new JsonImporter();
        List<ImportedFolder> folders = importer.Import(realpath);

        PrintFoldersToOutput(folders);

        Assert.NotNull(folders);
        Assert.True(folders.Count > 0);
    }

    [Theory]
    [InlineData("chromium_linux/exported_bookmarks.html")]
    public void HtmlExportedChromiumTest(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        IBookmarkImporter importer = new HtmlExportImporter();
        List<ImportedFolder> folders = importer.Import(realpath);
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
        IBookmarkImporter importer = new HtmlTakeoutImporter();
        List<ImportedFolder> folders = importer.Import(realpath);
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
        IBookmarkImporter importer = new JsonImporter();
        var folders = importer.Import(realpath);
        Assert.NotNull(folders);
    }

    [Theory]
    [InlineData("firefox_linux/bookmarks.html")]
    public void Html_Firefox_Test(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        IBookmarkImporter importer = new HtmlExportImporter();
        var folders = importer.Import(realpath);
        Assert.NotNull(folders);
    }

    [Theory]
    [InlineData("firefox_linux/places.sqlite")]
    public void Sql_Firefox_Test(string path)
    {
        string realpath = Path.Combine(TestDataFilesFolder, path);
        Assert.True(File.Exists(realpath));
        IBookmarkImporter importer = new SqliteImporter();
        List<ImportedFolder> folders = importer.Import(realpath);
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
        var smartImportResult = BookmarkImporterFactory.SmartImport(realpath);
        IBookmarkImporter jsonimporter = new JsonImporter();
        var jsonIntakeResult = jsonimporter.Import(realpath);

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

        var smartImportResult = BookmarkImporterFactory.SmartImport(realpath);
        IBookmarkImporter htmlimporter = new HtmlExportImporter();
        var htmlIntakeResult = htmlimporter.Import(realpath);

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
        var smartImportResult = BookmarkImporterFactory.SmartImport(realpath);
        IBookmarkImporter sqlimporter = new SqliteImporter();
        var sqlIntakeResult = sqlimporter.Import(realpath);

        Assert.NotNull(smartImportResult);
        Assert.NotNull(sqlIntakeResult);

        AssertFolderListsAreEqual(sqlIntakeResult, smartImportResult);
    }

    #endregion SmartImport
}