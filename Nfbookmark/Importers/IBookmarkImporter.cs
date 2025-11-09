using System.Collections.Generic;

namespace Nfbookmark.Importers
{
    public interface IBookmarkImporter
    {
        /// <summary>
        /// Imports bookmarks from the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the bookmark file.</param>
        /// <returns>A list of Folderclass objects representing the imported bookmarks.</returns>
        List<Folderclass> Import(string filePath);
    }
}