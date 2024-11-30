using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace bookmark_dlp
{
    /// <summary>
    /// Contains all functions relating to importing from browsers and files
    /// </summary>
    public class Import
    {
        /// <summary>
        /// Smart import, give file path and will automatically select import function
        /// </summary>
        /// <returns>List Folderclass or null </returns>
        public static List<Folderclass> SmartImport(string filePath)
        {
            if (!File.Exists(filePath)) { return null; }
            switch (Path.GetExtension(filePath))
            {
                case ".json":
                    return JsonIntake(filePath);
                    break;
                case ".sqlite":
                    return SqlIntake(filePath);
                    break;
                case ".html":
                    return HtmlTakeoutIntake(filePath);
                    break;
                default:
                    break;
            }
            if (Path.GetFileName(filePath) == "Bookmarks") { return JsonIntake(filePath); } //Chrome-based does not use extension for the Bookmarks file
            return null;
        }

        

        /// <summary>
        /// Imports bookmarks from a json file. Used for chromium based browsers
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of bookmark folders containing all the bookmarks</returns>
        public static List<Folderclass> JsonIntake(string filePath)
        {
            string text = "";
            Bookmark bookmark_bar;
            Bookmark other;
            Bookmark synced;
            Logger.LogVerbose("Autoimport intake start", Logger.Verbosity.Info);
            try { text = File.ReadAllText(filePath); }
            catch (FileLoadException ex) { Logger.LogVerbose($"Json file could not be accessed: {ex.Message}", Logger.Verbosity.Error); return null; }
            catch (FileNotFoundException ex) { Logger.LogVerbose($"Json file could not found: {ex.Message}", Logger.Verbosity.Error); return null; }
            catch (IOException ex) { Logger.LogVerbose($"Json file IOException: {ex.Message}", Logger.Verbosity.Error); return null; }

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
                date_added = Convert.ToInt64(bookmark_bar.date_added) - 2, //just setting a time that was slightly earlier than the bookmark_bar creation
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
            GlobalState globalState = new GlobalState();

            Bookmark bookmarkroot = root;
            Folderclass thisBookmark = new Folderclass();
            thisBookmark.startline = 0;
            thisBookmark.urls = new List<string>();
            int numberoflinks = 0;
            int depth = 0;
            folders.Add(thisBookmark); //later overwritten with the complete version of root, but necessary(?) so the first one is the root
            foreach (Bookmark child in bookmarkroot.children)
            {
                if (child.type == "url")
                {
                    thisBookmark.urls.Add(child.url);
                    //Console.WriteLine("URL: " + child.url);
                    numberoflinks++;
                }
                else if (child.type == "folder")
                {
                    //Console.WriteLine("Root folder: " + child.name);
                    folders.Add(Childfinder(child, depth + 1, ref globalState, ref folders));
                }
            }

            thisBookmark.name = bookmarkroot.name;
            thisBookmark.numberoflinks = numberoflinks;
            thisBookmark.depth = 0;
            thisBookmark.endingline = globalState.endingline;
            globalState.endingline++;
            folders[0] = thisBookmark;

            foreach (Folderclass parent in folders)
            {
                foreach (Folderclass child in folders)
                {
                    if (parent.depth == child.depth - 1 && parent.startline < child.startline && child.endingline < parent.endingline)
                    {
                        child.parent = parent.id;
                    }
                }
            }
            return folders;
        }

        /// <summary>
        /// Helper function for finding the children of a given folder
        /// </summary>
        /// <param name="current">The folder whose children we are searching for</param>
        /// <param name="depth">How deeply nested the current folder is</param>
        /// <param name="globalState">Used to keep track of ending lines (which folders are closed)</param>
        /// <param name="folders">The list of folders found so far</param>
        /// <returns></returns>
        private static Folderclass Childfinder(Bookmark current, int depth, ref GlobalState globalState, ref List<Folderclass> folders)
        {
            Folderclass thisBookmark = new Folderclass();
            globalState.folderid++;
            thisBookmark.startline = globalState.folderid;
            thisBookmark.id = globalState.folderid;
            thisBookmark.urls = new List<string>();
            //Console.WriteLine("Started childfinder with current folder: {1}, id:{0}, depth:{2}", globals.folderid, current.name, depth);
            int numberoflinks = 0;
            foreach (Bookmark child in current.children)
            {
                if (child.type == "url")
                {
                    thisBookmark.urls.Add(child.url);
                    //Console.WriteLine("Child URL: " + child.url);
                    numberoflinks++;
                }
                else if (child.type == "folder")
                {
                    //Console.WriteLine(current.name + "'s child Folder: " + child.name);
                    folders.Add(Childfinder(child, depth + 1, ref globalState, ref folders));
                }
            }
            thisBookmark.name = current.name;
            thisBookmark.numberoflinks = numberoflinks;
            thisBookmark.depth = depth;
            thisBookmark.endingline = globalState.endingline;
            globalState.endingline++;
            //Console.WriteLine("{0} has {1} links. Folderid: {2} depth: {3}", current.name, numberoflinks, globals.folderid, depth);
            //Console.WriteLine("Thisbookmark {0} has {1} links. Folderid: {2} depth: {3}", thisBookmark.name, thisBookmark.numberoflinks, thisBookmark.startline, thisBookmark.depth);
            return thisBookmark;
        }

        /// <summary>
        /// Gets List of Bookmarks that only has folder bookmarks in it and fills the appropriate values. Only for SqlIntake
        /// </summary>
        /// <param name="bookmarks">Contains only folders</param>
        /// <param name="parentid">parentid[i] = the sql id of the parent folder of the bookmark with the id i</param>
        /// <returns>List of Folderclasses with most values filled (parent, depth) or null</returns>
        private static List<Folderclass> Bookmarktofolderclasses(List<Bookmark> bookmarks, Dictionary<int, int> parentid)
        {
            // only folders remain in the sql_list

            if (bookmarks.Count == 0)
            {
                return null;
            }

            List<Folderclass> folders = new List<Folderclass>();
            // now bookmarks contains all the Bookmark objects for every folder.
            // These should now be united into one Bookmarkroot by adding them as each other's children from deepest depth upwards.
            // but instead they are just converted into folderclasses - this is also fine
            int folderid = 0;
            foreach (Bookmark bookmark in bookmarks)
            {
                Folderclass currentfolder = new Folderclass();
                currentfolder.id = folderid;
                currentfolder.name = bookmark.name;
                currentfolder.startline = bookmark.id;
                currentfolder.numberoflinks = bookmark.children.Count();
                currentfolder.urls = new List<string>();

                foreach (Bookmark urlbookmark in bookmark.children)
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
                // so "folders.SingleOrDefault(a => a.startline == parentid[i])" refers to the parent of the examined folder
                Folderclass parent = folders.SingleOrDefault(a => a.startline == parentid[bookmark.id]);
                if (parent != null)
                {
                    currentfolder.depth = parent.depth + 1; //the given folders depth is the depth of their parent folder + 1
                    currentfolder.parent = parent.id; // parentid[bookmark.id] should be the same?
                }
                else // the folder has no parent
                {
                    currentfolder.depth = 1;
                }

                //Console.WriteLine("Name: {0} ID: {1} Numberoflinks: {2} Depth: {3}", folders[folderid].name, folders[folderid].startline, folders[folderid].numberoflinks, folders[folderid].depth);
                
                folderid++;
                folders.Add(currentfolder);
            }
            folders[0].name = "root";
            return folders;
        }

        /// <summary>
        /// Takes all bookmarks and folders from an sql file and creates a list to hold them
        /// </summary>
        /// <param name="filePath">Sqlite file location</param>
        /// <returns>List of bookmark folders that have their children in them or null</returns>
        public static List<Folderclass> SqlIntake(string filePath)
        {
            if (!File.Exists(filePath)) { return null; }
            // docs: https://kb.mozillazine.org/Places.sqlite
            // https://stackoverflow.com/questions/11769524/how-can-i-restore-firefox-bookmark-files-from-sqlite-files
            Dictionary<int, int> parentid = new Dictionary<int, int>(); //parentid[i] = the id of the parent folder of the bookmark with the id i
            List<Bookmark> bookmarks = new List<Bookmark>();
            using (var connection = new SqliteConnection("Data Source=" + filePath))
            {
                connection.Open();
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
                            thisone.date_added = reader.GetInt64(5);
                        }
                        if (!reader.IsDBNull(6))
                        {
                            thisone.date_modified = reader.GetInt64(6);
                        }
                        if (type.Contains("2"))
                        {
                            thisone.type = "folder";
                            //Console.WriteLine("url:" + thisone.url + " name:" + thisone.name + " pid:" + parentid[thisone.id] + " id:" + thisone.id + " type:" + thisone.type + "|" + thisone.date_added + "|" + thisone.date_modified);
                        }
                        else if (type.Contains("1"))
                        {
                            thisone.type = "url";
                        }
                        else
                        {
                            Logger.LogVerbose("This bookmark is not of type folder or url - undefined", Logger.Verbosity.Error);
                        }
                        bookmarks.Add(thisone);
                    }
                }
            }
            //sqlite3 places.sqlite "select '<a href=''' || url || '''>' || moz_bookmarks.title || '</a><br/>' as ahref from moz_bookmarks left join moz_places on fk=moz_places.id where url<>'' and moz_bookmarks.title<>''" > t1.html
            //trying to place the data from the Bookmark object into a Folderclass[] object
            //in the sql only parent ids are given, not children, so the process has to be reversed compared to the json
            foreach (Bookmark bookmark in bookmarks.ToList<Bookmark>()) //must use tolist<> to avoid "Collection was modified; enumeration operation may not execute" when removing item from bookmarks
            {
                if (bookmark.type == "url") //urls have no children, it is safe to add them to their parent folders (even if they are not at the deepest depth
                {
                    bookmarks.Single(a => a.id == parentid[bookmark.id]).children.Add(bookmark); //bookmark added to their parent's .children list
                    bookmarks.Remove(bookmark); //bookmark is removed from the sql_list (as it is already in its parent's list
                }
                //only folders remain in the sql_list
            }

            List<Folderclass> folders = Bookmarktofolderclasses(bookmarks, parentid); //converts the List<Bookmark> to List<Folderclasses>
            return folders;
        }



        /// <summary>
        /// Intake of bookmarks and bookmarkfolders from a Google Takeout Html file
        /// </summary>
        /// <param name="filePath">Html file location</param>
        /// <returns>List of folders that have all their url bookmarks as children</returns>
        public static List<Folderclass> HtmlTakeoutIntake(string filePath)
        {
            // read .html
            string[] inputarray;
            int lineCount;
            try
            {
                // StreamReader reader = new StreamReader(htmlfilepath); //read the file containing all the bookmarks - a single file using chrome export
                lineCount = File.ReadLines(filePath).Count(); //how many lines are there in the file - max number of bookmarks
                inputarray = File.ReadAllLines(filePath); //read whole file into inputarray[] array
            }
            catch (FileLoadException ex) { Logger.LogVerbose($"Html file could not be accessed: {ex.Message}", Logger.Verbosity.Error); return null; }
            catch (FileNotFoundException ex) { Logger.LogVerbose($"Html file could not found: {ex.Message}", Logger.Verbosity.Error); return null; }
            catch (IOException ex) { Logger.LogVerbose($"Html file IOException: {ex.Message}", Logger.Verbosity.Error); return null; }

            if (inputarray[2].Substring(0,3) == "   ")
            {
                Logger.LogVerbose($"The html file {filePath} appears to be an exported one", Logger.Verbosity.Info);
                return HtmlExportIntake(filePath);
            }
            else
            {
                Logger.LogVerbose($"The html file {filePath} appears to be a takeout one", Logger.Verbosity.Info);
            }

            Logger.LogVerbose(inputarray.Length + "/" + lineCount + " lines were read.", Logger.Verbosity.Debug);
            Logger.LogVerbose("The intake has finished!", Logger.Verbosity.Debug);

            /*//Creating the folders[] object array and initialize all its elements, notice that the max number of folders equals the number of lines
            Folderclass[] folders = new Folderclass[lineCount];
            for (int q = 0; q < lineCount; q++)
            {
                folders[q] = new Folderclass();
            }*/

            List<Folderclass> folders = new List<Folderclass>();

            { // scope just for variable visibility
                // Finding all the lines starting with dt h3 (these lines start every folder) and adding the number of these lines (j) to the object array folders[].startline
                // the folders[].startline gives us the number of the first line of the given folder in the inputarray[] array (in is like the endingline in the next loop, just for the start)
                // This also gives us the number of folders (numberoffolders)
                string[] line = new string[1000]; // limitation: there cannot be a line/bookmark with more than 1000 spaces in it. Probably not relevant?
                int numberoffolders = 0;
                for (int j = 0; j < lineCount; j++)
                {
                    line = inputarray[j].Trim().Split(' ');
                    if (line[0].Trim() == "<DT><H3")
                    {
                        Folderclass currentfolder = new Folderclass();
                        currentfolder.startline = j;
                        currentfolder.id = numberoffolders;
                        folders.Add(currentfolder);
                        numberoffolders++;
                    }
                }
            }
            Logger.LogVerbose(folders.Count + " folders were found in the bookmarks", Logger.Verbosity.Debug);

            // Finding the end of the folders (</DL><p>) and adding the line number to the object array (folders[].endingline)
            // Counting the lines from the start while the folders from the back, so even in folders embedded into folders the endingline will be correct
            for (int j = 1; j < lineCount; j++)
            {
                string oneline = inputarray[j].Trim();
                if (oneline == "</DL><p>") // if we find a line that ends a folder
                {
                    for (int m = folders.Count - 1; m >= 0; m--)
                    {
                        if (folders[m].startline < j && folders[m].endingline == 0) //finding the last folder that has a starting line earlier than this endingline, and has not yet been closed
                        {
                            folders[m].endingline = j;
                            break;
                            //break is necessary, because in embedded folders not only the correct folder's startline would be found correct, but all the not-yet closed folders that are already open: all their parent folders
                            //the break prevents parent folders getting the same endingline as their children
                        }
                    }
                }
            }

            // Finding the folder names and adding them to the object array (folders[].name)
            for (int m = 0; m < folders.Count; m++)
            {
                string[] line = inputarray[folders[m].startline].Trim().Split('>');
                int whereisthechar = line[line.Length - 2].IndexOf("<");
                folders[m].name = line[line.Length - 2].Substring(0, whereisthechar);
                //Console.WriteLine(line[line.Length-2].Substring(0,whereisthechar));
                //Console.WriteLine(folders[m].startline + " " + folders[m].name);
                //Console.WriteLine(folders[m] + " line " + whereisthechar + " " + line[line.Length - 2]);
            }

            // Finding the folder depths (how embedded they are) and adding them to the object array (folders[].depth)
            for (int m = 0; m < folders.Count; m++)
            {
                string[] line = inputarray[folders[m].startline].Split('<');
                folders[m].depth = line[0].Length / 8;
            }

            // Add links to their folder
            for (int k = 0; k < folders.Count; k++)
            {
                //google side bug of duplicating all folders and bookmarks, resulting in 3 line long empty folders as well as not empty folders, which contain two copies of every bookmark.
                //shouldn't have too much of an effect on the end results,
                //just divide most numbers by 2. yt-dlp already uses archive.txt, so only lookup time is wasted, not downloads
                //Console.WriteLine(folders[j].name + " " + folders[j].endingline + " " + folders[j].startline + " " + (folders[j].endingline - folders[j].startline));
                if (folders[k].endingline - folders[k].startline > 2)
                {
                    for (int lineindex = folders[k].startline; lineindex < folders[k].endingline + 1; lineindex++) //going through all the lines that are in the given folder
                    {
                        if (inputarray[lineindex] != null)
                        {
                            string[] line = inputarray[lineindex].Trim().Split(' ');
                            if (line[0].Trim() == "<DT><A")
                            {
                                string link = line[1].Trim().Substring(6, line[1].Trim().Length - 7);
                                folders[k].urls.Add(link);
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogVerbose($"Folder {folders[k].name} with id {folders[k].id} has less than 2 lines. Start {folders[k].startline} end {folders[k].endingline}", Logger.Verbosity.Warning);
                }
            }
            return folders;
        }

        /// <summary>
        /// Intake of bookmarks and bookmarkfolders from a browser exported Html file
        /// </summary>
        /// <param name="filePath">Html file location</param>
        /// <returns>List of folders that have all their url bookmarks as children</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<Folderclass> HtmlExportIntake (string filePath)
        {
            // read .html
            string[] inputarray;
            int lineCount;
            try
            {
                // StreamReader reader = new StreamReader(htmlfilepath); //read the file containing all the bookmarks - a single file using chrome export
                lineCount = File.ReadLines(filePath).Count(); //how many lines are there in the file - max number of bookmarks
                inputarray = File.ReadAllLines(filePath); //read whole file into inputarray[] array
            }
            catch (FileLoadException ex) { Logger.LogVerbose($"Html file could not be accessed: {ex.Message}", Logger.Verbosity.Error); return null; }
            catch (FileNotFoundException ex) { Logger.LogVerbose($"Html file could not found: {ex.Message}", Logger.Verbosity.Error); return null; }
            catch (IOException ex) { Logger.LogVerbose($"Html file IOException: {ex.Message}", Logger.Verbosity.Error); return null; }

            List<Folderclass> folders = new List<Folderclass>();

            { // scope just for variable visibility
                // Finding all the lines starting with dt h3 (these lines start every folder) and adding the number of these lines (j) to the object array folders[].startline
                // the folders[].startline gives us the number of the first line of the given folder in the inputarray[] array (in is like the endingline in the next loop, just for the start)
                // This also gives us the number of folders (numberoffolders)
                string[] line = new string[1000]; // limitation: there cannot be a line/bookmark with more than 1000 spaces in it. Probably not relevant?
                int numberoffolders = 0;
                for (int j = 0; j < lineCount; j++)
                {
                    line = inputarray[j].Trim().Split(' ');
                    if (line[0].Trim() == "<DT><H3")
                    {
                        Folderclass currentfolder = new Folderclass();
                        currentfolder.startline = j;
                        currentfolder.id = numberoffolders;
                        folders.Add(currentfolder);
                        numberoffolders++;
                    }
                }
            }
            Logger.LogVerbose(folders.Count + " folders were found in the bookmarks", Logger.Verbosity.Debug);

            // Finding the end of the folders (</DL><p>) and adding the line number to the object array (folders[].endingline)
            // Counting the lines from the start while the folders from the back, so even in folders embedded into folders the endingline will be correct
            for (int j = 1; j < lineCount; j++)
            {
                string oneline = inputarray[j].Trim();
                if (oneline == "</DL><p>") // if we find a line that ends a folder
                {
                    for (int m = folders.Count - 1; m >= 0; m--)
                    {
                        if (folders[m].startline < j && folders[m].endingline == 0) //finding the last folder that has a starting line earlier than this endingline, and has not yet been closed
                        {
                            folders[m].endingline = j;
                            break;
                            //break is necessary, because in embedded folders not only the correct folder's startline would be found correct, but all the not-yet closed folders that are already open: all their parent folders
                            //the break prevents parent folders getting the same endingline as their children
                        }
                    }
                }
            }

            // Finding the folder names and adding them to the object array (folders[].name)
            for (int m = 0; m < folders.Count; m++)
            {
                string[] line = inputarray[folders[m].startline].Trim().Split('>');
                int whereisthechar = line[line.Length - 2].IndexOf("<");
                folders[m].name = line[line.Length - 2].Substring(0, whereisthechar);
                //Console.WriteLine(line[line.Length-2].Substring(0,whereisthechar));
                //Console.WriteLine(folders[m].startline + " " + folders[m].name);
                //Console.WriteLine(folders[m] + " line " + whereisthechar + " " + line[line.Length - 2]);
            }

            // Finding the folder depths (how embedded they are) and adding them to the object array (folders[].depth)
            for (int m = 0; m < folders.Count; m++)
            {
                string[] line = inputarray[folders[m].startline].Split('<');
                folders[m].depth = line[0].Length / 8 + 1;
            }

            // Add links to their folder
            for (int k = 0; k < folders.Count; k++)
            {
                //google side bug of duplicating all folders and bookmarks, resulting in 3 line long empty folders as well as not empty folders, which contain two copies of every bookmark.
                //shouldn't have too much of an effect on the end results,
                //just divide most numbers by 2. yt-dlp already uses archive.txt, so only lookup time is wasted, not downloads
                //Console.WriteLine(folders[j].name + " " + folders[j].endingline + " " + folders[j].startline + " " + (folders[j].endingline - folders[j].startline));
                if (folders[k].endingline - folders[k].startline > 2)
                {
                    for (int lineindex = folders[k].startline; lineindex < folders[k].endingline + 1; lineindex++) //going through all the lines that are in the given folder
                    {
                        if (inputarray[lineindex] != null)
                        {
                            string[] line = inputarray[lineindex].Trim().Split(' ');
                            if (line[0].Trim() == "<DT><A")
                            {
                                string link = line[1].Trim().Substring(6, line[1].Trim().Length - 7);
                                folders[k].urls.Add(link);
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogVerbose($"Folder {folders[k].name} with id {folders[k].id} has less than 2 lines. Start {folders[k].startline} end {folders[k].endingline}", Logger.Verbosity.Warning);
                }
            }
            return folders;
        }
    }
}
