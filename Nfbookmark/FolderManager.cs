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
        public static List<MappedFolder> CreateFolderStructure(List<ImportedFolder> folders, string rootdir)
        {
            if(String.IsNullOrEmpty(rootdir))
                throw new ArgumentNullException(nameof(rootdir));
            Log.Information("Creating folder structure in root directory: {RootDir}", rootdir);

            if (!Directory.Exists(rootdir)) 
                Directory.CreateDirectory(rootdir);
            
            string bookmarksRoot = Path.Combine(rootdir, "Bookmarks");
            Directory.CreateDirectory(bookmarksRoot);
            
            List<MappedFolder> mappedFolders = new List<MappedFolder>();
            foreach (ImportedFolder varfol in folders)   
            {
                mappedFolders.Add(new MappedFolder(varfol));
            }
            
            var folderMap = mappedFolders.ToDictionary(f => f.Id);

            foreach (MappedFolder folder in mappedFolders.OrderBy(f => f.Depth))
            {
                if (folder.Depth != 0)
                {
                    if (!folderMap.TryGetValue(folder.ParentId, out var parentFolder))
                    {
                        Log.Error("Folder '{FolderName}' with parentId {ParentId} appears to have no parent in the list.", folder.Name, folder.ParentId);
                        throw new InvalidDataException($"Could not find parent for folder {folder.Name}");
                    }

                    if (folder.Depth != parentFolder.Depth + 1)
                    {
                        Log.Error("Depth of folder '{FolderName}' ({FolderDepth}) is not 1 more than its parent '{ParentName}' depth ({ParentDepth}).", folder.Name, folder.Depth, parentFolder.Name, parentFolder.Depth);
                        throw new InvalidDataException($"Depth of folder {folder.Name} is incorrect.");
                    }

                    if (string.IsNullOrEmpty(parentFolder.FolderPath) || !Directory.Exists(parentFolder.FolderPath))
                    {
                        Log.Error("Parent directory for folder '{FolderName}' does not exist: {ParentPath}", folder.Name, parentFolder.FolderPath);
                        throw new InvalidDataException($"Parent directory does not exist for {folder.Name}.");
                    }
                    
                    folder.FolderPath = Path.Combine(parentFolder.FolderPath, folder.Name);
                }
                else
                {
                    folder.FolderPath = Path.Combine(bookmarksRoot, folder.Name);
                }

                Directory.CreateDirectory(folder.FolderPath);
                Log.Verbose("Created folder path for '{FolderName}': {FolderPath}", folder.Name, folder.FolderPath);
            }
            return mappedFolders;
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
        public static void DeleteEmptyFolders(List<MappedFolder> folders)
        {
            int deletedCount = 0;
            int deepestDepth = folders.Any() ? folders.Max(f => f.Depth) : 0;

            for (int depth = deepestDepth; depth >= 0; depth--)
            {
                foreach (MappedFolder folder in folders.Where(f => f.Depth == depth)) //TODO: this for+foreach could be a groupby 
                {
                    if (string.IsNullOrEmpty(folder.FolderPath) || !Directory.Exists(folder.FolderPath))
                        continue;

                    if (!Directory.EnumerateFileSystemEntries(folder.FolderPath).Any())
                    {
                        try
                        {
                            Directory.Delete(folder.FolderPath);
                            deletedCount++;
                            Log.Debug("Deleted empty folder: {FolderPath}", folder.FolderPath);
                        }
                        catch (IOException e)
                        {
                            Log.Error(e, "Could not delete empty folder: {FolderPath}", folder.FolderPath);
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