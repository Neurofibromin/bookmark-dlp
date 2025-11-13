using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace Nfbookmark
{
    public class FolderManager
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(FolderManager));

        /// <summary>
        ///     Attempt to make sure all bookmark folder names are good for filesystems folder names. If needed, changes the
        ///     folder.name value<br />
        ///     Necessary because bookmark folders can have 1) empty names 2) the same names 3) contain not allowed characters or
        ///     character combinations<br />
        ///     Requires:
        ///     Name
        ///     Parent
        /// </summary>
        /// <param name="folders">The bookmark folders to operate on, folder names may be changed</param>
        public static void ValidateFolderNames(List<Folderclass> folders)
        {
            string[] forbiddenCharacters = { "/", ":", "?", "<", ">", "*", "|", "\\", "\"" };
            
            foreach (Folderclass folder in folders)
            {
                string originalName = folder.name;
                string newName = folder.name;

                foreach (string ch in forbiddenCharacters)
                {
                    if (newName.Contains(ch))
                    {
                        newName = newName.Replace(ch, string.Empty);
                        Log.Warning("Folder name '{OriginalName}' contained illegal character '{Character}'. Renaming to '{NewName}'.", originalName, ch, newName);
                    }
                }

                if (newName.Trim().Replace(" ", string.Empty).Distinct().Count() == 1 && newName.Trim().Replace(" ", string.Empty).Distinct().First() == '.') 
                {
                    newName = $"ID{folder.id}";
                    Log.Warning("Folder name '{OriginalName}' contained only spaces and periods. Renaming to '{NewName}'.", originalName, newName);
                }
                
                if (newName.StartsWith("."))
                {
                    newName = $"ID{folder.id}";
                    Log.Warning("Folder name '{OriginalName}' started with a period. Renaming to '{NewName}'.", originalName, newName);
                }
                
                if (string.IsNullOrWhiteSpace(newName))
                {
                    newName = $"ID{folder.id}";
                    Log.Warning("Folder name '{OriginalName}' was whitespace. Renaming to '{NewName}'.", originalName, newName);
                }

                folder.name = newName;
            }

            // If two folders have the same name and same parent (and same depth)
            var duplicateFolderGroups = folders
                .GroupBy(f => new { f.name, f.parentId })
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateFolderGroups)
            {
                Log.Warning("Found duplicate folder name '{FolderName}' with parentId {ParentId}. Appending IDs to differentiate.", group.Key.name, group.Key.parentId);
                foreach (var folder in group)
                {
                    folder.name = $"{folder.name}ID{folder.id}";
                }
            }
            // for (int i = 0; i < folders.Count - 1; i++)
            // {
            //     for (int j = i + 1; j < folders.Count; j++)
            //     {
            //         if (string.Equals(folders[i].name, folders[j].name, StringComparison.CurrentCultureIgnoreCase) &&
            //             folders[i].depth == folders[j].depth &&
            //             folders[i].parentId == folders[j].parentId)
            //         {
            //             folders[j].name = folders[j].name + $"ID{folders[j].id}";
            //             folders[i].name = folders[i].name + $"ID{folders[i].id}";
            //         }
            //     }
            // }
        }

        /// <summary>
        ///     Creating the folder structure on filesystems and storing the access paths to folders[].folderpath <br />
        ///     Requires:
        ///     <list type="bullet">
        ///         <item> Name </item>
        ///         <item> Depth </item>
        ///         <item> Parent </item>
        ///     </list>
        ///     Fills:
        ///     <list type="bullet">
        ///         <item> Folderpath </item>
        ///     </list>
        /// </summary>
        /// <param name="folders">Bookmark folders to be made into filesystem folders</param>
        /// <param name="rootdir">Filesystems directory to contain all the folders</param>
        public static void CreateFolderStructure(List<Folderclass> folders, string rootdir)
        {
            Log.Information("Creating folder structure in root directory: {RootDir}", rootdir);
            ValidateFolderNames(folders);

            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);
            
            string bookmarksRoot = Path.Combine(rootdir, "Bookmarks");
            Directory.CreateDirectory(bookmarksRoot);

            var folderMap = folders.ToDictionary(f => f.id);

            foreach (Folderclass folder in folders.OrderBy(f => f.depth))
            {
                if (folder.depth != 0)
                {
                    if (!folderMap.TryGetValue(folder.parentId, out var parentFolder))
                    {
                        Log.Error("Folder '{FolderName}' with parentId {ParentId} appears to have no parent in the list.", folder.name, folder.parentId);
                        throw new InvalidDataException($"Could not find parent for folder {folder.name}");
                    }

                    if (folder.depth != parentFolder.depth + 1)
                    {
                        Log.Error("Depth of folder '{FolderName}' ({FolderDepth}) is not 1 more than its parent '{ParentName}' depth ({ParentDepth}).", folder.name, folder.depth, parentFolder.name, parentFolder.depth);
                        throw new InvalidDataException($"Depth of folder {folder.name} is incorrect.");
                    }

                    if (string.IsNullOrEmpty(parentFolder.folderpath) || !Directory.Exists(parentFolder.folderpath))
                    {
                        Log.Error("Parent directory for folder '{FolderName}' does not exist: {ParentPath}", folder.name, parentFolder.folderpath);
                        throw new InvalidDataException($"Parent directory does not exist for {folder.name}.");
                    }
                    
                    folder.folderpath = Path.Combine(parentFolder.folderpath, folder.name);
                }
                else
                {
                    folder.folderpath = Path.Combine(bookmarksRoot, folder.name);
                }

                Directory.CreateDirectory(folder.folderpath);
                Log.Verbose("Created folder path for '{FolderName}': {FolderPath}", folder.name, folder.folderpath);
            }
        }

        /// <summary>
        ///     Delete filesystem folders that are associated with bookmark folders if
        ///     1) filesystems folder has no files AND
        ///     2) filesystem folder has no folders <br />
        ///     Requires:
        ///     <list type="bullet">
        ///         <item> name </item>
        ///         <item> depth </item>
        ///         <item> parentId </item>
        ///         <item> folderpath </item>
        ///     </list>
        /// </summary>
        /// <param name="folders"></param>
        public static void DeleteEmptyFolders(List<Folderclass> folders)
        {
            int deletedCount = 0;
            int deepestDepth = folders.Any() ? folders.Max(f => f.depth) : 0;

            for (int depth = deepestDepth; depth >= 0; depth--)
            {
                foreach (Folderclass folder in folders.Where(f => f.depth == depth))
                {
                    if (string.IsNullOrEmpty(folder.folderpath) || !Directory.Exists(folder.folderpath))
                        continue;

                    if (!Directory.EnumerateFileSystemEntries(folder.folderpath).Any())
                    {
                        try
                        {
                            Directory.Delete(folder.folderpath);
                            deletedCount++;
                            Log.Debug("Deleted empty folder: {FolderPath}", folder.folderpath);
                        }
                        catch (IOException e)
                        {
                            Log.Error(e, "Could not delete empty folder: {FolderPath}", folder.folderpath);
                        }
                    }
                }
            }
            
            if (deletedCount > 0)
            {
                Log.Information("{Count} empty folders deleted.", deletedCount);
            }
            else
            {
                Log.Debug("No empty folders found to delete.");
            }
        }
    }
}