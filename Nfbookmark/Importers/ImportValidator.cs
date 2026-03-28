using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace Nfbookmark.Importers
{
    public class ImportValidator
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<ImportValidator>();
        
        private static readonly char[] AdditionalInvalidChars = { '/', ':', '?', '<', '>', '*', '|', '\\', '"' };
        private static readonly HashSet<char> InvalidChars = new HashSet<char>(Path.GetInvalidFileNameChars().Union(AdditionalInvalidChars));
        
        private static readonly Regex ReservedDosNames = new Regex(
            @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])$", 
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        ///     Attempt to make sure all bookmark folder names are good for filesystems folder names. Creates copy of the list without changing it. If needed, changes the
        ///     folder.name value<br />
        ///     Necessary because bookmark folders can have 1) empty names 2) the same names 3) contain not allowed characters or
        ///     character combinations<br />
        ///     Requires:
        ///     name
        ///     parentId
        ///     Validates and sanitizes bookmark folder names for cross-platform file system compatibility.
        ///     Creates a new list of mutated objects to ensure deterministic state.
        /// </summary>
        public static List<ImportedFolder> ValidateFolderNames(List<ImportedFolder> folders)
        {
            var validFolders = new List<ImportedFolder>(folders.Capacity);

            foreach (var folder in folders)
            {
                string originalName = folder.Name ?? string.Empty;
                string newName = SanitizeFolderName(originalName);

                // Fallback mechanics if sanitization destroys the name
                if (string.IsNullOrWhiteSpace(newName) || ReservedDosNames.IsMatch(newName))
                {
                    newName = $"ID{folder.Id}";
                    Log.Warning("Folder name '{OriginalName}' was invalid or reserved. Renaming to fallback '{NewName}'.", originalName, newName);
                }

                // Create a defensive copy with the sanitized name
                var validFolder = new ImportedFolder(folder)
                {
                    Name = newName
                };
                
                validFolders.Add(validFolder);
            }

            return ResolveDuplicateNames(validFolders);
        }

        private static string SanitizeFolderName(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // 1. Strip invalid characters in a single pass using a character buffer (zero-allocation ideally)
            var cleanNameChars = input.Where(c => !InvalidChars.Contains(c)).ToArray();
            string newName = new string(cleanNameChars);

            // 2. Windows strictly forbids trailing spaces and periods in directory names
            newName = newName.TrimEnd(' ', '.');

            // 3. Prevent names starting with a period (often treated as hidden/system folders on *nix)
            if (newName.StartsWith("."))
            {
                newName = newName.TrimStart('.');
            }

            return newName;
        }

        private static List<ImportedFolder> ResolveDuplicateNames(List<ImportedFolder> validFolders)
        {
            // If two folders have the same name and same parent, they will collide on the file system.
            var duplicateFolderGroups = validFolders
                .GroupBy(f => new { f.Name, f.ParentId })
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateFolderGroups)
            {
                Log.Warning("Found duplicate folder name '{FolderName}' under parentId {ParentId}. Appending IDs to ensure uniqueness.", group.Key.Name, group.Key.ParentId);
                
                // Keep the first one pristine, append IDs to subsequent duplicates
                bool isFirst = true;
                foreach (var folder in group)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        continue;
                    }
                    folder.Name = $"{folder.Name}_ID{folder.Id}";
                }
            }

            return validFolders;
        }
    }
}