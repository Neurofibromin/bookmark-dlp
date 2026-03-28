using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace Nfbookmark.Importers
{
    /// <summary>
    /// Imports bookmarks from a JSON file (used for Chromium-based browsers).
    /// </summary>
    public class JsonImporter : IBookmarkImporter
    {
        private readonly ILogger Log = Serilog.Log.ForContext<JsonImporter>();

        /// <summary>
        /// Imports bookmarks from a json file. Used for chromium based browsers <br/>
        /// Fills:
        /// <list type="bullet">
        /// <item> StartLine </item>
        /// <item> urls </item>
        /// <item> name </item>
        /// <item> depth </item>
        /// <item> parentId </item>
        /// <item> endingline </item>
        /// </list>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of bookmark folders containing all the bookmarks</returns>
        public List<ImportedFolder> UnsafeImport(string filePath)
        {
            string text;
            Bookmark bookmark_bar;
            Bookmark other;
            Bookmark synced;
            Log.Debug("JSON intake start");
            try { text = File.ReadAllText(filePath); }
            catch (Exception ex) when (ex is FileLoadException || ex is FileNotFoundException || ex is IOException)
            {
                Log.Error(ex, "JSON file could not be read: {FilePath}", filePath);
                return null;
            }

            try
            {
                JsonDocument doc = JsonDocument.Parse(text);
                JsonElement roots_Element;

                if (!doc.RootElement.TryGetProperty("roots", out roots_Element))
                {
                    Log.Error("Invalid JSON: 'roots' property missing.");
                    return null;
                }

                JsonElement bookmarks_bar_Element;
                JsonElement other_Element;
                JsonElement synced_Element;
                if (!roots_Element.TryGetProperty("bookmark_bar", out bookmarks_bar_Element) ||
                    !roots_Element.TryGetProperty("other", out other_Element) ||
                    !roots_Element.TryGetProperty("synced", out synced_Element))
                {
                    Log.Error("Invalid JSON: Required bookmark properties are missing.");
                    return null;
                }
                var options = new JsonSerializerOptions { IncludeFields = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }; //by default no fields only properties, by default no num from string conversion
                bookmark_bar = bookmarks_bar_Element.Deserialize<Bookmark>(options);
                other = other_Element.Deserialize<Bookmark>(options);
                synced = synced_Element.Deserialize<Bookmark>(options);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Parsing the Json failed");
                return null;
            }
            synced.name = "Synced Bookmarks"; //has to be renamed, because google puts "Mobile bookmarks" in json and "Synced Bookmarks" in html
            other.name = "Other Bookmarks"; //has to be renamed, because google puts "Other bookmarks" in json and "Other Bookmarks" in html (diff: capitalisation!)
            bookmark_bar.name = "Bookmark Bar"; //has to be renamed, because google puts "Bookmarks bar" in json and "Bookmark Bar" in html (diff: capitalisation!, plural)
            //note: the naming MUST be consistent, so if html and autoimport are both used in the same directory videos will not get downloaded twice
            Bookmark root = new Bookmark
                // the root is not actually a bookmark json object, it just contains the 3 json objects of other, synced and bookmarks_bar
                // as such here a root json object is created, which will contain those three as children
                {
                    name = "Bookmarks",
                    guid = Guid.NewGuid().ToString(), //adding new guid to the root
                    id = Convert.ToInt16("0"), //id for : bookmark_bar=1, other=2, synced=3
                    type = "folder",
                    DateAdded =
                        Convert.ToInt64(bookmark_bar.DateAdded) -
                        2, //just setting a time that was slightly earlier than the bookmark_bar creation
                    DateLastUsed = Convert.ToInt64("0"), //not used by chrome apparently
                    DateModified = Convert.ToInt64("0"), //not much used by chrome apparently
                    Children = new List<Bookmark> { bookmark_bar, other, synced }
                };
            /* the structure of the file:
            {
             "checksum": "12345678912345678912345678912345",
             "roots": {
                "bookmark_bar": {
                        childred:[ ], /////////here are all the bookmarks generally
                        "DateAdded": "123456789123456789",
                        "DateLastUsed": "0",
                        "DateModified": "123456789123456789",
                        "guid": "guid-123456789123456789",
                        "id": "1",
                        "name": "Bookmarks bar",
                        "type": "folder"
                    },
                    "other": {
                        "children": [  ],
                        "DateAdded": "123456789123456789",
                        "DateLastUsed": "0",
                        "DateModified": "0",
                        "guid": "guid-123456789123456789",
                        "id": "2",
                        "name": "Other bookmarks",
                        "type": "folder"
                    },
                    "synced": {
                        "children": [  ],
                        "DateAdded": "123456789123456789",
                        "DateLastUsed": "0",
                        "DateModified": "0",
                        "guid": "guid-123456789123456789",
                        "id": "3",
                        "name": "Mobile bookmarks",
                        "type": "folder"
                    }
             },
             "sync_metadata": "123456789#__4000000_char_long_string",
             "version": 1
           }
            */
            
            List<ImportedFolder> folders = new List<ImportedFolder>();
            int folderIdCounter = 0;

            // Create the top-level root Folderclass
            ImportedFolder rootFolder = new ImportedFolder
            {
                Id = folderIdCounter++,
                ParentId = -1, // No parent for the root
                Name = root.name,
                Depth = 0,
                StartLine = root.id
            };
            folders.Add(rootFolder);
            
            foreach (Bookmark child in root.Children)
            {
                if (child.type == "url")
                {
                    rootFolder.urls.Add(child.url);
                }
                else if (child.type == "folder")
                {
                    // Pass the root folder's ID as the parentId
                    Childfinder(child, rootFolder.Id, 1, ref folderIdCounter, folders);
                }
            }
    
            return folders;
        }

        /// <summary>
        /// Helper function for finding the children of a given folder <br/>
        /// Fills:
        /// <list type="bullet">
        /// <item> StartLine </item>
        /// <item> urls </item>
        /// <item> name </item>
        /// <item> depth </item>
        /// <item> id </item>
        /// <item> parentId </item>
        /// </list>
        /// </summary>
        /// <param name="current">The folder whose children we are searching for</param>
        /// <param name="depth">How deeply nested the current folder is</param>
        /// <param name="folderIdCounter">Used to keep track of global folder count</param>
        /// <param name="allFolders">The list of folders found so far</param>
        private void Childfinder(Bookmark current, int parentId, int depth, ref int folderIdCounter, List<ImportedFolder> allFolders)
        {
            ImportedFolder thisFolder = new ImportedFolder
            {
                Id = folderIdCounter++,
                ParentId = parentId,
                Name = current.name,
                Depth = depth,
                StartLine = current.id
            };
            allFolders.Add(thisFolder);

            foreach (Bookmark child in current.Children)
            {
                if (child.type == "url")
                {
                    // assert urls is not null?
                    thisFolder.urls.Add(child.url);
                }
                else if (child.type == "folder")
                {
                    // Pass the current folder's ID as the parent for its children.
                    Childfinder(child, thisFolder.Id, depth + 1, ref folderIdCounter, allFolders);
                }
            }
        }
    }
}