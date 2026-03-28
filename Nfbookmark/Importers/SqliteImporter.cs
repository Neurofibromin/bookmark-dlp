using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Nfbookmark.Importers
{
    /// <summary>
    /// Imports bookmarks from a SQLite database file (used for Firefox).
    /// </summary>
    public class SqliteImporter : IBookmarkImporter
    {
        private readonly ILogger Log = Serilog.Log.ForContext<SqliteImporter>();

        /// <summary>
        /// Takes all bookmarks and folders from an sql file and creates a list to hold them <br/>
        /// Fills:
        /// <list type="bullet">
        /// <item> id </item>
        /// <item> depth </item>
        /// <item> parentId </item>
        /// <item> name </item>
        /// <item> StartLine </item>
        /// <item> urls </item>
        /// </list>
        /// </summary>
        /// <param name="filePath">Sqlite file location</param>
        /// <returns>List of bookmark folders that have their children in them or null</returns>
        public List<ImportedFolder> UnsafeImport(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error("SQLite file not found at: {FilePath}", filePath);
                return null;
            }

            // Create a temporary copy of the database to avoid file locking issues
            string tempFilePath = Path.GetTempFileName();
            try
            {
                File.Copy(filePath, tempFilePath, overwrite: true);
                filePath = tempFilePath;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create a temporary copy of the SQLite database.");
                return null;
            }
            
            // docs: https://kb.mozillazine.org/Places.sqlite
            // https://stackoverflow.com/questions/11769524/how-can-i-restore-firefox-bookmark-files-from-sqlite-files
            Dictionary<int, int> parentid = new Dictionary<int, int>(); //parentid[i] = the id of the parent folder of the bookmark with the id i
            List<Bookmark> bookmarks = new List<Bookmark>();
            using (var connection = new SqliteConnection("Data Source=" + filePath + ";mode=ReadOnly"))
            {
                try
                {
                    connection.Open();
                    Log.Verbose("SQL database opened: {FilePath}", filePath);
                }
                catch (SqliteException ex)
                {
                    Log.Error(ex, "Failed to open SQLite database: {FilePath}", filePath);
                    return null;
                }
                
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT moz_places.url, moz_bookmarks.title, moz_bookmarks.id, moz_bookmarks.parent, moz_bookmarks.type, dateAdded, lastModified
                FROM moz_bookmarks left join moz_places on moz_bookmarks.fk = moz_places.id
                WHERE moz_bookmarks.title<>''
                ";

                using (var reader = command.ExecuteReader()) //the order of the variables in SELECT is the same order in which they are returned, coloumn by coloumn
                {
                    while (reader.Read())
                    {
                        Bookmark thisone = new Bookmark();
                        string type = "";
                        if (!reader.IsDBNull(0)) //only try to convert the coloumn value to string with getstring if it is not null - otherwise error
                        {
                            thisone.url = reader.GetString(0);
                        }
                        if (!reader.IsDBNull(1))
                        {
                            thisone.name = reader.GetString(1);
                        }
                        if (!reader.IsDBNull(2))
                        {
                            thisone.id = reader.GetInt32(2);
                        }
                        if (!reader.IsDBNull(3))
                        {
                            parentid[thisone.id] = reader.GetInt32(3);
                        }
                        if (!reader.IsDBNull(4))
                        {
                            type = reader.GetString(4);
                        }
                        if (!reader.IsDBNull(5))
                        {
                            thisone.DateAdded = reader.GetInt64(5);
                        }
                        if (!reader.IsDBNull(6))
                        {
                            thisone.DateModified = reader.GetInt64(6);
                        }
                        if (type.Contains("2"))
                        {
                            thisone.type = "folder";
                            //Console.WriteLine("url:" + thisone.url + " name:" + thisone.name + " pid:" + parentid[thisone.id] + " id:" + thisone.id + " type:" + thisone.type + "|" + thisone.DateAdded + "|" + thisone.DateModified);
                        }
                        else if (type.Contains("1"))
                        {
                            thisone.type = "url";
                        }
                        else
                        {
                            Log.Error("Bookmark '{BookmarkName}' is not of type folder or url - undefined.", thisone.name);
                        }
                        bookmarks.Add(thisone);
                    }
                }
            }
            Log.Verbose("SQL Read finished");
            //sqlite3 places.sqlite "select '<a href=''' || url || '''>' || moz_bookmarks.title || '</a><br/>' as ahref from moz_bookmarks left join moz_places on fk=moz_places.id where url<>'' and moz_bookmarks.title<>''" > t1.html
            //trying to place the data from the Bookmark object into a Folderclass[] object
            //in the sql only parent ids are given, not children, so the process has to be reversed compared to the json
            foreach (Bookmark bookmark in bookmarks.ToList<Bookmark>()) //must use tolist<> to avoid "Collection was modified; enumeration operation may not execute" when removing item from bookmarks
            {
                if (bookmark.type == "url") //urls have no children, it is safe to add them to their parent folders (even if they are not at the deepest depth
                {
                    bookmarks.Single(a => a.id == parentid[bookmark.id]).Children.Add(bookmark); //bookmark added to their parent's .childrenIds list
                    bookmarks.Remove(bookmark); //bookmark is removed from the sql_list (as it is already in its parent's list
                }
                //only folders remain in the sql_list
            }
            File.Delete(tempFilePath);
            List<ImportedFolder> folders = BookmarkToFolderclasses(bookmarks, parentid); //converts the List<Bookmark> to List<Folderclasses>
            return folders;
        }

        /// <summary>
        /// Gets List of Bookmarks that only has folder bookmarks in it and fills the appropriate values. Only for SqlIntake <br/>
        /// Fills:
        /// <list type="bullet">
        /// <item> id </item>
        /// <item> depth </item>
        /// <item> parentId </item>
        /// <item> name </item>
        /// <item> StartLine </item>
        /// <item> urls </item>
        /// </list>
        /// </summary>
        /// <param name="bookmarks">Contains only folders</param>
        /// <param name="parentid">parentid[i] = the sql id of the parent folder of the bookmark with the id i</param>
        /// <returns>List of Folderclasses with most values filled (parent, depth) or null</returns>
        private List<ImportedFolder> BookmarkToFolderclasses(List<Bookmark> bookmarks, Dictionary<int, int> parentid)
        {
            // only folders remain in the sql_list
            if (bookmarks.Count == 0)
            {
                Log.Warning("No bookmark folders found after processing the SQLite database.");
                return null;
            }

            List<ImportedFolder> folders = new List<ImportedFolder>();
            // now bookmarks contains all the Bookmark objects for every folder.
            // These should now be united into one Bookmarkroot by adding them as each other's children from deepest depth upwards.
            // but instead they are just converted into folderclasses - this is also fine
            int folderid = 0;
            foreach (Bookmark bookmark in bookmarks)
            {
                ImportedFolder currentfolder = new ImportedFolder
                {
                    Id = folderid,
                    Name = bookmark.name,
                    StartLine = bookmark.id,
                    urls = new List<string>()
                };
                foreach (Bookmark urlbookmark in bookmark.Children)
                {
                    currentfolder.urls.Add(urlbookmark.url); //adding the url of each child to the url list of their parent
                }

                // i refers to the id (from the sql) of the folder that is being examined. folderid will be its new id, so every folderid refers to folders and there is no gap between them:
                // examples:
                // folder toolbars id: 2 folderid: 1
                // folder a id: 7 folderid: 2
                // folder b id: 15 folderid: 3
                // folder c id: 43175 folderid: 4
                // the difference is large because id was also given in the sql db for url bookmarks, while now only folder bookmarks are examined, so large gaps are expected


                // i - id of the examined folder, parentid[i] - id of the examined folder's parent :
                // we are looking for the folder that has this parentid[i] as its startingline (refers to the original id),
                // so "folders.SingleOrDefault(a => a.StartLine == parentid[i])" refers to the parent of the examined folder
                ImportedFolder parent = folders.SingleOrDefault(a => a.StartLine == parentid[bookmark.id]);
                if (parent != null)
                {
                    currentfolder.Depth = parent.Depth + 1; //the given folders depth is the depth of their parent folder + 1
                    currentfolder.ParentId = parent.Id; // parentid[bookmark.id] should be the same?
                }
                else // the folder has no parent
                {
                    currentfolder.Depth = 1;
                }

                //Console.WriteLine("Name: {0} ID: {1} Numberoflinks: {2} Depth: {3}", folders[folderid].name, folders[folderid].StartLine, folders[folderid].numberoflinks, folders[folderid].depth);

                folderid++;
                folders.Add(currentfolder);
            }
            if (folders.Any())
            {
                folders[0].Name = "root";
            }
            return folders;
        }
    }
}