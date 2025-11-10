using System;
using System.Collections.Generic;
using System.IO;
using Nfbookmark.Importers;
using Serilog;

namespace Nfbookmark
{
    /// <summary>
    /// A factory class to select and use the correct bookmark importer based on file type.
    /// </summary>
    public static class BookmarkImporterFactory
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(BookmarkImporterFactory));

        /// <summary>
        /// Smart import, gives file path and will automatically select import function.
        /// </summary>
        /// <param name="filePath">Path to the html/json/sqlite file containing the bookmarks.</param>
        /// <returns>A list of Folderclass objects, with 0 elements if import fails.</returns>
        public static List<Folderclass> SmartImport(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error("File not found: {FilePath}", filePath);
                return new List<Folderclass>();
            }

            Log.Information("Starting SmartImport for: {FilePath}", filePath);

            try
            {
                // A quick check to see if the file is readable.
                using (var stream = File.OpenRead(filePath))
                {
                    if (stream.Length == 0)
                    {
                        Log.Warning("Bookmark file is empty: {FilePath}", filePath);
                    }
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Log.Error(e, "Could not read file for import: {FilePath}", filePath);
                return new List<Folderclass>();
            }

            IBookmarkImporter importer;

            switch (Path.GetExtension(filePath).ToLower())
            {
                case ".json":
                    Log.Debug("Selected JsonImporter for {FilePath}", filePath);
                    importer = new JsonImporter();
                    break;
                
                case ".sqlite":
                    Log.Debug("Selected SqliteImporter for {FilePath}", filePath);
                    importer = new SqliteImporter();
                    break;
                
                case ".html":
                    // Differentiate between Takeout and Exported HTML
                    try
                    {
                        string[] lines = File.ReadAllLines(filePath);
                        // The original logic to differentiate based on indentation of the third line
                        if (lines.Length > 2 && lines[2].StartsWith("   "))
                        {
                            Log.Debug("Detected browser-exported HTML format for {FilePath}", filePath);
                            importer = new HtmlExportImporter();
                        }
                        else
                        {
                            Log.Debug("Detected Google Takeout HTML format for {FilePath}", filePath);
                            importer = new HtmlTakeoutImporter();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Could not read HTML file {FilePath} to determine format.", filePath);
                        return new List<Folderclass>();
                    }
                    break;
                
                default:
                    // Handle files with no extension, like Chrome's "Bookmarks" file
                    if (Path.GetFileName(filePath) == "Bookmarks")
                    {
                        Log.Debug("Selected JsonImporter for Chrome's 'Bookmarks' file at {FilePath}", filePath);
                        importer = new JsonImporter();
                        break;
                    }
                    Log.Warning("Unsupported file type for import: {FilePath}", filePath);
                    return new List<Folderclass>();
            }

            try
            {
                return importer.Import(filePath);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "An unexpected error occurred during import of {FilePath}", filePath);
                return new List<Folderclass>();
            }
        }
    }
}