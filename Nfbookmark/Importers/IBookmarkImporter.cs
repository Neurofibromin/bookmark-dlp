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
        protected List<ImportedFolder> UnsafeImport(string filePath);

        public List<ImportedFolder> Import(string filePath)
        {
            var raw = UnsafeImport(filePath);
            return ImportValidator.ValidateFolderNames(raw ?? new List<ImportedFolder>());
        }
    }
}