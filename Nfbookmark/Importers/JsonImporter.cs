using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nfbookmark.Importers;
using NfLogger;

namespace Nfbookmark.Importers
{
    /// <summary>
    /// Imports bookmarks from a JSON file (used for Chromium-based browsers).
    /// </summary>
    public class JsonImporter : IBookmarkImporter
    {
        
        /// <summary>
        /// Imports bookmarks from a json file. Used for chromium based browsers <br/>
        /// Fills:
        /// <list type="bullet">
        /// <item> startline </item>
        /// <item> urls </item>
        /// <item> name </item>
        /// <item> depth </item>
        /// <item> parentId </item>
        /// <item> endingline </item>
        /// </list>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of bookmark folders containing all the bookmarks</returns>
        public List<Folderclass> Import(string filePath)
        {
            string text = "";
            Bookmark bookmark_bar;
            Bookmark other;
            Bookmark synced;
            Logger.LogVerbose("JSON intake start", Logger.Verbosity.Debug);
            try { text = File.ReadAllText(filePath); }
            catch (Exception ex) when (ex is FileLoadException || ex is FileNotFoundException || ex is IOException)
            {
                Logger.LogVerbose($"JSON file could not be read: {ex.Message}", Logger.Verbosity.Error);
                return null;
            }

            try
            {
                JsonDocument doc = JsonDocument.Parse(text);
                JsonElement roots_Element;

                if (!doc.RootElement.TryGetProperty("roots", out roots_Element))
                {
                    Logger.LogVerbose("Invalid JSON: 'roots' property missing.", Logger.Verbosity.Error);
                    return null;
                }

                JsonElement bookmarks_bar_Element;
                JsonElement other_Element;
                JsonElement synced_Element;
                if (!roots_Element.TryGetProperty("bookmark_bar", out bookmarks_bar_Element) ||
                    !roots_Element.TryGetProperty("other", out other_Element) ||
                    !roots_Element.TryGetProperty("synced", out synced_Element))
                {
                    Logger.LogVerbose("Invalid JSON: Required bookmark properties are missing.", Logger.Verbosity.Error);
                    return null;
                }
                var options = new JsonSerializerOptions { IncludeFields = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }; //by default no fields only properties, by default no num from string conversion
                bookmark_bar = System.Text.Json.JsonSerializer.Deserialize<Bookmark>(bookmarks_bar_Element, options);
                other = System.Text.Json.JsonSerializer.Deserialize<Bookmark>(other_Element, options);
                synced = System.Text.Json.JsonSerializer.Deserialize<Bookmark>(synced_Element, options);
            }
            catch (Exception ex)
            {
                Logger.LogVerbose($"Parsing the Json failed: {ex.Message}", Logger.Verbosity.Error);
                return null;
            }
/*
             * Newtonsoft.Json:
            Bookmark bookmark_bar = JObject.Parse(text)["roots"]["bookmark_bar"].ToObject<Bookmark>();
            Bookmark other = JObject.Parse(text)["roots"]["other"].ToObject<Bookmark>();
            Bookmark synced = JObject.Parse(text)["roots"]["synced"].ToObject<Bookmark>();*/
            
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
                    date_added =
                        Convert.ToInt64(bookmark_bar.date_added) -
                        2, //just setting a time that was slightly earlier than the bookmark_bar creation
                    date_last_used = Convert.ToInt64("0"), //not used by chrome apparently
                    date_modified = Convert.ToInt64("0"), //not much used by chrome apparently
                    children = new List<Bookmark> { bookmark_bar, other, synced }
                };
            /* the structure of the file:
            {
             "checksum": "12345678912345678912345678912345",
             "roots": {
                "bookmark_bar": {
                        childred:[ ], /////////here are all the bookmarks generally
                        "date_added": "123456789123456789",
                        "date_last_used": "0",
                        "date_modified": "123456789123456789",
                        "guid": "guid-123456789123456789",
                        "id": "1",
                        "name": "Bookmarks bar",
                        "type": "folder"
                    },
                    "other": {
                        "children": [  ],
                        "date_added": "123456789123456789",
                        "date_last_used": "0",
                        "date_modified": "0",
                        "guid": "guid-123456789123456789",
                        "id": "2",
                        "name": "Other bookmarks",
                        "type": "folder"
                    },
                    "synced": {
                        "children": [  ],
                        "date_added": "123456789123456789",
                        "date_last_used": "0",
                        "date_modified": "0",
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

            List<Folderclass> folders = new List<Folderclass>();
            Legacy.GlobalState globalState = new Legacy.GlobalState();

            Bookmark bookmarkroot = root;
            Folderclass thisBookmark = new Folderclass
            {
                startline = 0,
                urls = new List<string>(),
                name = bookmarkroot.name,
                depth = 0
            };
            int depth = 0;
            folders.Add(thisBookmark); //later overwritten with the complete version of root, but necessary(?) so the first one is the root
            foreach (Bookmark child in bookmarkroot.children)
            {
                if (child.type == "url")
                {
                    thisBookmark.urls.Add(child.url);
                    //Console.WriteLine("URL: " + child.url);
                }
                else if (child.type == "folder")
                {
                    //Console.WriteLine("Root folder: " + child.name);
                    folders.Add(Childfinder(child, depth + 1, ref globalState, ref folders));
                }
            }

            thisBookmark.endingline = globalState.endingline;
            globalState.endingline++;
            folders[0] = thisBookmark;

            foreach (Folderclass parent in folders)
            {
                foreach (Folderclass child in folders)
                {
                    if (parent.depth == child.depth - 1 && parent.startline < child.startline &&
                        child.endingline < parent.endingline)
                    {
                        child.parentId = parent.id;
                    }
                }
            }
            return folders;
        }

        /// <summary>
        /// Helper function for finding the children of a given folder <br/>
        /// Fills:
        /// <list type="bullet">
        /// <item> startline </item>
        /// <item> urls </item>
        /// <item> name </item>
        /// <item> depth </item>
        /// <item> id </item>
        /// <item> endingline </item>
        /// </list>
        /// </summary>
        /// <param name="current">The folder whose children we are searching for</param>
        /// <param name="depth">How deeply nested the current folder is</param>
        /// <param name="globalState">Used to keep track of ending lines (which folders are closed)</param>
        /// <param name="folders">The list of folders found so far</param>
        /// <returns></returns>
        private Folderclass Childfinder(Bookmark current, int depth, ref Legacy.GlobalState globalState, ref List<Folderclass> folders)
        {
            globalState.folderid++;
            Folderclass thisBookmark = new Folderclass
            {
                startline = globalState.folderid,
                id = globalState.folderid,
                urls = new List<string>(),
                name = current.name,
                depth = depth
            };
            //Console.WriteLine("Started childfinder with current folder: {1}, id:{0}, depth:{2}", globals.folderid, current.name, depth);
            foreach (Bookmark child in current.children)
            {
                if (child.type == "url")
                {
                    thisBookmark.urls.Add(child.url);
                    //Console.WriteLine("Child URL: " + child.url);
                }
                else if (child.type == "folder")
                {
                    //Console.WriteLine(current.name + "'s child Folder: " + child.name);
                    folders.Add(Childfinder(child, depth + 1, ref globalState, ref folders));
                }
            }
            thisBookmark.endingline = globalState.endingline;
            globalState.endingline++;
            //Console.WriteLine("{0} has {1} links. Folderid: {2} depth: {3}", current.name, numberoflinks, globals.folderid, depth);
            //Console.WriteLine("Thisbookmark {0} has {1} links. Folderid: {2} depth: {3}", thisBookmark.name, thisBookmark.numberoflinks, thisBookmark.startline, thisBookmark.depth);
            return thisBookmark;
        }
    }
}