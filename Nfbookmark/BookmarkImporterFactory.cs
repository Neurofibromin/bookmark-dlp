using System;
using System.Collections.Generic;
using System.IO;
using Nfbookmark.Importers;
using NfLogger;

namespace Nfbookmark
{
    /// <summary>
    /// A factory class to select and use the correct bookmark importer based on file type.
    /// </summary>
    public static class BookmarkImporterFactory
    {
        /// <summary>
        /// Smart import, gives file path and will automatically select import function.
        /// </summary>
        /// <param name="filePath">Path to the html/json/sqlite file containing the bookmarks.</param>
        /// <returns>A list of Folderclass objects, or null if import fails.</returns>
        public static List<Folderclass> SmartImport(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Logger.LogVerbose($"File not found: {filePath}", Logger.Verbosity.Error);
                return null;
            }
            Logger.LogVerbose("Starting SmartImport for: " + filePath);
            try
            {
                StreamReader sr = new StreamReader(filePath);
                string tryread = sr.ReadLine();
                sr.Close();
                Logger.LogVerbose("Parsing SmartImport: " + filePath);
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is IOException || e is UnauthorizedAccessException)
                {
                    Logger.LogVerbose(e + " File:" + filePath + "could not be read.");
                }
                throw;
            }

            IBookmarkImporter importer;

            switch (Path.GetExtension(filePath).ToLower())
            {
                case ".json":
                    importer = new JsonImporter();
                    break;
                
                case ".sqlite":
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
                            importer = new HtmlExportImporter();
                        }
                        else
                        {
                            importer = new HtmlTakeoutImporter();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogVerbose($"Could not read HTML file to determine format: {e.Message}", Logger.Verbosity.Error);
                        return null;
                    }
                    break;
                
                default:
                    // Handle files with no extension, like Chrome's "Bookmarks" file
                    if (Path.GetFileName(filePath) == "Bookmarks")
                    {
                        importer = new JsonImporter();
                        break;
                    }
                    Logger.LogVerbose($"Unsupported file type for: {filePath}", Logger.Verbosity.Warning);
                    return null;
            }

            try
            {
                return importer.Import(filePath);
            }
            catch (Exception e)
            {
                Logger.LogVerbose($"An unexpected error occurred during import of {filePath}: {e.Message}", Logger.Verbosity.Critical);
                return null;
            }
        }
    }
}