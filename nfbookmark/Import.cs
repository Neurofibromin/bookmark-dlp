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
                    throw new NotImplementedException();
                    HtmlExportIntake(filePath);
                    HtmlTakeoutIntake(filePath);
                    break;
                default:
                    break;
            }
            if (Path.GetFileName(filePath) == "Bookmarks") { return JsonIntake(filePath); } //Chrome-based does not use extension for the Bookmarks file
            return null;
        }

        /// <summary>
        /// Default locations for profiles for Linux, OSX and Windows
        /// </summary>
        /// <returns>List of folder paths where profile folders may be located (not just for installed browsers)</returns>
        public static List<BrowserLocations> GetBrowserLocations()
        {
            BrowserLocations Chrome = new BrowserLocations
            {
                browsername = "Chrome",
                browserType = BrowserType.chromebased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data\\"),
                //linksfound = "",
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome") },
            };
            BrowserLocations Chrome_beta = new BrowserLocations
            {
                browsername = "Chrome-beta",
                browserType = BrowserType.chromebased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome Beta\\User Data\\"),
                //linksfound = "",
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-beta") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome Beta") }
            };
            BrowserLocations Chrome_canary = new BrowserLocations
            {
                browsername = "Chrome-canary",
                browserType = BrowserType.chromebased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome SxS\\User Data\\"),
                //linksfound = "",
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-unstable") }, //technically its called chrome unstable on linux, but its the same thing
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome Canary") }
            };
            BrowserLocations Brave = new BrowserLocations
            {
                browsername = "Brave-browser",
                browserType = BrowserType.chromebased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware\\Brave-Browser\\User Data\\"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BraveSoftware/Brave-Browser/") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/BraveSoftware/Brave-Browser") }
            };
            BrowserLocations Chromium = new BrowserLocations
            {
                //great docs: https://chromium.googlesource.com/chromium/src/+/master/docs/user_data_dir.md
                browsername = "chromium",
                browserType = BrowserType.chromebased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chromium\\User Data"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "chromium") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Chromium") }
            };
            BrowserLocations Vivaldi = new BrowserLocations()
            {
                browsername = "Vivaldi",
                browserType = BrowserType.chromebased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vivaldi\\User Data"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vivaldi") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Vivaldi") }
            };
            BrowserLocations Edge = new BrowserLocations()
            {
                browsername = "Microsoft Edge",
                browserType = BrowserType.chromebased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "microsoft-edge") }, 
                //.config/microsoft-edge/Default/Bookmarks
                // C:\Users\<Current-user>\AppData\Local\Microsoft\Edge\User Data\Default.
            };
            BrowserLocations Opera = new BrowserLocations()
            {
                browsername = "Opera",
                browserType = BrowserType.chromebased,
                //C:\Users\%username%\AppData\Roaming\Opera Software\Opera Stable\Bookmarks is the Bookmarks file
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software"),
                //opera: .config/opera/Bookmarks
                hardcodedpaths = new List<string>()
                {
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "opera/Bookmarks"),
                }
                //osx has to be checked
            };
            BrowserLocations Firefox = new BrowserLocations
            {
                browsername = "Firefox",
                browserType = BrowserType.firefoxbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\"),
                //linksfound = "",
                linux_profilespath = new List<string> { 
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "snap/firefox/common/.mozilla/firefox/"),
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".mozilla/firefox"),
                },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Firefox/Profiles") },
            };
            List<BrowserLocations> browserLocations = new List<BrowserLocations>
                {
                    Chrome,
                    Chrome_beta,
                    Chrome_canary,
                    Brave,
                    Chromium,
                    Vivaldi,
                    Edge,
                    Opera,
                    Firefox,
                };
            return browserLocations;
        }

        /// <summary>
        /// Interactively query which file containing bookmarks should be selected
        /// </summary>
        /// <param name="browserLocations"></param>
        /// <returns>Path (string) to chosen file or null</returns>
        public static string QueryChosenBookmarksFile(List<BrowserLocations> browserLocations)
        {
            if (browserLocations.Count == 0) { return null; }
            string filePath;
            List<string> filepaths = new List<string>();
            Methods.LogVerbose("Which browser bookmarks would you like to use?\nWrite the number");
            int m = 1;
            foreach (BrowserLocations browser in browserLocations)
            {
                foreach (string path in browser.foundFiles)
                {
                    Methods.LogVerbose(m + ". path: " + path);
                    filepaths.Add(path);
                    m++;
                }
            }
            string chosenindex;
            int chosenindexInt;
            while (true)
            {
                chosenindex = Console.ReadLine();
                try
                {
                    chosenindexInt = int.Parse(chosenindex);
                    if (chosenindexInt < 1 || chosenindexInt >= m) { throw new Exception(); }
                    else { break; }
                }
                catch (Exception)
                {
                    Methods.LogVerbose("Wrong input", Methods.Verbosity.error);
                    continue;
                }
            }
            filePath = filepaths.ElementAt(chosenindexInt - 1);
            Methods.LogVerbose("Chosen path: " + filePath);
            return filePath;
        }

        /// <summary>
        /// Checks for existing profiles in the default places for a browser. Supports GNU/Linux, OSX, Windows.
        /// </summary>
        /// <param name="browser"></param>
        /// <returns>Browserlocations object that has foundfiles and linksfound filled. If no profiles are found then the BrowserLocations object is returned as it was.</returns>
        private static BrowserLocations BrowserCheck(BrowserLocations browser)
        {
            if (browser.browserType != BrowserType.chromebased) { return firefoxBrowserCheck(browser); }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Directory.Exists(browser.windows_profilespath))
                {
                    foreach (string profile in Directory.GetDirectories(browser.windows_profilespath))
                    {
                        if (File.Exists(Path.Combine(profile, "Bookmarks")))
                        {
                            browser.foundFiles.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            Console.WriteLine("File found! Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            browser.linksfound++;
                        }
                    }
                    if (browser.linksfound == 0) { Console.WriteLine(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else if (browser.hardcodedpaths.Count != 0)
                {
                    foreach (string hardpath in browser.hardcodedpaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.foundFiles.Add(hardpath);
                            Console.WriteLine("File found! Filepath in " + browser.browsername + ": " + hardpath);
                            browser.linksfound++;
                        }

                    }
                    if (browser.linksfound == 0) { Console.WriteLine(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else { Console.WriteLine(browser.browsername + " install folder not found"); }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                foreach (string installlocation in browser.linux_profilespath)
                {
                    if (Directory.Exists(installlocation))
                    {
                        foreach (string profile in Directory.GetDirectories(installlocation))
                        {
                            if (File.Exists(Path.Combine(profile, "Bookmarks")))
                            {
                                //For every chrome profile that has bookmarks
                                browser.foundFiles.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                Console.WriteLine("File found! Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                browser.linksfound++;
                            }
                        }
                        if (browser.linksfound == 0) { Console.WriteLine(($"Bookmarks file not found in " + browser.browsername)); }
                    }
                }
                if (browser.hardcodedpaths.Count != 0)
                {
                    foreach (string hardpath in browser.hardcodedpaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.foundFiles.Add(hardpath);
                            Console.WriteLine("File found! Filepath in " + browser.browsername + ": " + hardpath);
                            browser.linksfound++;
                        }

                    }
                    if (browser.linksfound == 0) { Console.WriteLine(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else { Console.WriteLine(browser.browsername + " install folder not found"); }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                foreach (string  installlocation in browser.osx_profilespath)
                {
                    if (Directory.Exists(installlocation))
                    {
                        foreach (string profile in Directory.GetDirectories(installlocation))
                        {
                            if (File.Exists(Path.Combine(profile, "Bookmarks")))
                            {
                                browser.foundFiles.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                Console.WriteLine("File found! Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                browser.linksfound++;
                            }
                        }
                        if (browser.linksfound == 0) { Console.WriteLine(($"Bookmarks file not found in " + browser.browsername)); }
                    }
                }
                if (browser.hardcodedpaths.Count != 0)
                {
                    foreach (string hardpath in browser.hardcodedpaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.foundFiles.Add(hardpath);
                            Console.WriteLine("File found! Filepath in " + browser.browsername + ": " + hardpath);
                            browser.linksfound++;
                        }

                    }
                    if (browser.linksfound == 0) { Console.WriteLine(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else { Console.WriteLine(browser.browsername + " install folder not found"); }
            }
            return browser;
        }

        private static BrowserLocations firefoxBrowserCheck(BrowserLocations browser)
        {
            if (browser.browserType != BrowserType.firefoxbased) { return  browser; }
            //Finding the sqlite databases
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // C:\Windows.old\Users\<UserName>\AppData\Roaming\Mozilla\Firefox\Profiles\<filename.default>\places.sqlite
                // filePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\<filename.default>\\places.sqlite");
                // profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\");
                // Console.WriteLine("profilespath " + profilespath);
                if (Directory.Exists(browser.windows_profilespath))
                {
                    foreach (string profile in Directory.GetDirectories(browser.windows_profilespath))
                    {
                        if (File.Exists(Path.Combine(profile, "places.sqlite")))
                        {
                            //For every firefox profile that has bookmarks
                            browser.foundFiles.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            Console.WriteLine("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            browser.linksfound++;
                        }
                    }
                }
                else { Console.WriteLine("Firefox install folder not found"); }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // /home/$User/snap/firefox/common/.mozilla/firefox/aaaa.default/places.sqlite/
                // /home/$User/.mozilla/firefox/aaaa.default/places.sqlite
                // string profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "snap/firefox/common/.mozilla/firefox/");
                foreach(string profilespath in browser.linux_profilespath)
                {
                    // for every firefox install present (eg. snap, flatpak, system package)
                    if (Directory.Exists(profilespath))
                    {
                        foreach (string profile in Directory.GetDirectories(profilespath))
                        {
                            if (File.Exists(Path.Combine(profile, "places.sqlite")))
                            {
                                //For every firefox profile that has bookmarks
                                browser.foundFiles.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                Console.WriteLine("File found! " + "Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                browser.linksfound++;
                            }
                        }
                    }
                    else { Console.WriteLine("{0} at {1} not found", browser.browsername, profilespath); }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // /Users/<username>/Library/Application Support/Firefox/Profiles/<profile folder>
                // profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Firefox/Profiles");
                foreach (string installlocation in browser.osx_profilespath)
                {
                    if (Directory.Exists(installlocation))
                    {
                        foreach (string profile in Directory.GetDirectories(installlocation))
                        {
                            if (File.Exists(Path.Combine(profile, "places.sqlite")))
                            {
                                //For every firefox profile that has bookmarks
                                browser.foundFiles.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                Console.WriteLine("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                browser.linksfound++;
                            }
                        }
                    }
                    else { Console.WriteLine("Firefox install folder not found."); }
                }
            }
            if (browser.linksfound == 0) { Console.WriteLine($"Bookmarks file not found in {0}", browser.browsername); }
            return browser;
        }

        /// <summary>
        /// Complete list of browsers with profiles, supports crossplatform. Only installed
        /// </summary>
        /// <returns>List with all browser and their paths that have any browser profiles</returns>
        public static List<BrowserLocations> GetBrowserBookmarkFilesPaths()
        {
            List<BrowserLocations> browserLocations = GetBrowserLocations(); //gets list of supported browsers
            List<BrowserLocations> completeBrowserLocations = new List<BrowserLocations>();
            foreach (BrowserLocations browser in browserLocations)
            {
                completeBrowserLocations.Add(BrowserCheck(browser)); //Checks for every browser if it is installed and has profiles in it
            }
            browserLocations = completeBrowserLocations; //completeBrowserLocations had to be introduced because: cannot assign to foreach variable

            //count how many links were found
            int TotalLinksFoundCount = 0;
            completeBrowserLocations = new List<BrowserLocations>();
            foreach (BrowserLocations browser in browserLocations)
            {
                TotalLinksFoundCount += browser.linksfound;
                if (browser.linksfound != 0) { completeBrowserLocations.Add(browser); } //only return with browsers that have profiles
            }
            if (TotalLinksFoundCount == 0)
            {
                return null;
            }
            return completeBrowserLocations;
        }


        public static List<Folderclass> JsonIntake(string filePath)
        {
            string text = "";
            Bookmark bookmark_bar;
            Bookmark other;
            Bookmark synced;
            Methods.LogVerbose("Autoimport intake start", Methods.Verbosity.info);
            try { text = File.ReadAllText(filePath); }
            catch (FileLoadException ex) { Methods.LogVerbose($"Json file could not be accessed: {ex.Message}", Methods.Verbosity.error); return null; }
            catch (FileNotFoundException ex) { Methods.LogVerbose($"Json file could not found: {ex.Message}", Methods.Verbosity.error); return null; }

            try
            {
                JsonDocument doc = JsonDocument.Parse(text);
                JsonElement roots_Element = doc.RootElement.GetProperty("roots");
                JsonElement bookmarks_bar_Element = roots_Element.GetProperty("bookmark_bar");
                JsonElement other_Element = roots_Element.GetProperty("other");
                JsonElement synced_Element = roots_Element.GetProperty("synced");
                var options = new JsonSerializerOptions { IncludeFields = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }; //by default no fields only properties, by default no num from string conversion
                bookmark_bar = System.Text.Json.JsonSerializer.Deserialize<Bookmark>(bookmarks_bar_Element, options);
                other = System.Text.Json.JsonSerializer.Deserialize<Bookmark>(other_Element, options);
                synced = System.Text.Json.JsonSerializer.Deserialize<Bookmark>(synced_Element, options);
            }
            catch (Exception)
            {
                Methods.LogVerbose("Parsing the Json failed.", Methods.Verbosity.error);
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
            ///the root is not actually a bookmark json object, it just contains the 3 json objects of other, synced and bookmarks_bar
            ///as such here a root json object is created, which will contain those three as children
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
            foreach (Folderclass folder in folders)
            {
                foreach (Folderclass ffold in folders)
                {
                    if (folder.depth == ffold.depth - 1 && folder.startline < ffold.startline && folder.endingline < ffold.endingline)
                    {
                        folder.parent = ffold.id;
                    }
                }
            }
            return folders;
        }

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
        /// <param name="parentid">parentid[i] = the id of the parent folder of the bookmark with the id i</param>
        /// <returns>List of Folderclasses with most values filled (parent, depth)</returns>
        private static List<Folderclass> Bookmarktofolderclasses(List<Bookmark> bookmarks, int[] parentid)
        {
            //only folders remain in the sql_list
            /*Folderclass[] folders = new Folderclass[bookmarks.Count + 1]; //declare and initialise the folders[]
            for (int q = 0; q < bookmarks.Count + 1; q++)
            {
                folders[q] = new Folderclass();
            }*/

            List<Folderclass> folders = new List<Folderclass>();
            folders.Add(new Folderclass()); //indexing should start at 1 apparently? So fill 0 with empty.
            // now bookmarks contains all the Bookmark objects for every folder.
            // These should now be united into one Bookmarkroot by adding them as each other's children from deepest depth upwards.
            // but instead they are just converted into folderclasses - this is also fine
            int folderid = 1;
            foreach (Bookmark bookmark in bookmarks)
            {
                Folderclass currentfolder = new Folderclass();
                currentfolder.id = folderid;

                int i = bookmark.id;
                
                // i refers to the id (from the sql) of the folder that is being examined. folderid will be its new id, so every folderid refers to folders and there is no gap between them:
                // examples:
                // folder toolbars id: 2 folderid: 1
                // folder a id: 7 folderid: 2
                // folder b id: 15 folderid: 3
                // folder c id: 43175 folderid: 4
                // the difference is large because id was also given in the sql db for url bookmarks, while now only folder bookmarks are examined, so large gaps are expected


                int index = folders.FindIndex(a => a.startline == parentid[i]);
                // i - id of the examined folder, parentid[i] - id of the examined folder's parent :
                // we are looking for the folder[] that has this parentid[i] as its startingline (refers to the original id),
                // so "folders.SingleOrDefault(a => a.startline == parentid[i])" refers to the parent of the examined folder
                if (index != -1)
                { 
                    currentfolder.depth = folders.SingleOrDefault(a => a.startline == parentid[i]).depth + 1; //the given folders depth is the depth of their parent folder + 1
                    currentfolder.parent = folders.SingleOrDefault(a => a.startline == parentid[i]).id;
                }
                else //if (index == -1), the folder has no parent
                {
                    currentfolder.depth = 1;
                }
                currentfolder.name = bookmark.name;
                currentfolder.startline = bookmark.id;
                currentfolder.numberoflinks = bookmark.children.Count();
                //Console.WriteLine("Name: {0} ID: {1} Numberoflinks: {2} Depth: {3}", folders[folderid].name, folders[folderid].startline, folders[folderid].numberoflinks, folders[folderid].depth);
                foreach (Bookmark urlbookmark in bookmark.children)
                {
                    //adding the url of each child to the url list of their parent
                    currentfolder.urls.Add(urlbookmark.url);
                }
                folderid++;
                folders.Add(currentfolder);
            }
            // AutoImport.Globals.folderid = folderid - 1; //helps for setting numberoffolders later
            folders[1].name = "root"; //TODO: why from index 1?
            return folders;
        }

        /// <summary>
        /// Takes all bookmarks and folders from an sql file and creates a list to hold them
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of bookmark folders that have their children in them or null</returns>
        public static List<Folderclass> SqlIntake(string filePath)
        {
            if (!File.Exists(filePath)) { return null; }
            // docs: https://kb.mozillazine.org/Places.sqlite
            // https://stackoverflow.com/questions/11769524/how-can-i-restore-firefox-bookmark-files-from-sqlite-files
            int[] parentid = new int[File.ReadLines(filePath).Count() + 1]; //parentid[i] = the id of the parent folder of the bookmark with the id i
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
                            thisone.id = Convert.ToInt32(reader.GetString(2));
                        }
                        if (!reader.IsDBNull(3))
                        {
                            parentid[thisone.id] = Convert.ToInt32(reader.GetString(3));
                        }
                        if (!reader.IsDBNull(4))
                        {
                            type = reader.GetString(4);
                        }
                        if (!reader.IsDBNull(5))
                        {
                            thisone.date_added = Convert.ToInt64(reader.GetString(5));
                        }
                        if (!reader.IsDBNull(6))
                        {
                            thisone.date_modified = Convert.ToInt64(reader.GetString(6));
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
                            Methods.LogVerbose("This bookmark is not of type folder or url - undefined", Methods.Verbosity.error);
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

            List<Folderclass> folders = Bookmarktofolderclasses(bookmarks, parentid); //converts the List<Bookmark> to Folderclasses[]
            return folders;
        }



        /// <summary>
        /// Intake of bookmarks and bookmarkfolders from a Google Takeout Html file
        /// </summary>
        /// <param name="htmlfilepath"></param>
        /// <returns>List of folders that have all their url bookmarks as children</returns>
        public static List<Folderclass> HtmlTakeoutIntake(string htmlfilepath)
        {
            // read .html
            string[] inputarray;
            int lineCount;
            try
            {
                // StreamReader reader = new StreamReader(htmlfilepath); //read the file containing all the bookmarks - a single file using chrome export
                lineCount = File.ReadLines(htmlfilepath).Count(); //how many lines are there in the file - max number of bookmarks
                inputarray = File.ReadAllLines(htmlfilepath); //read whole file into inputarray[] array
            }
            catch (FileLoadException ex) { Methods.LogVerbose($"Html file could not be accessed: {ex.Message}", Methods.Verbosity.error); return null; }
            catch (FileNotFoundException ex) { Methods.LogVerbose($"Html file could not found: {ex.Message}", Methods.Verbosity.error); return null; }

            Methods.LogVerbose(inputarray.Length + "/" + lineCount + " lines were read.", Methods.Verbosity.debug);
            Methods.LogVerbose("The intake has finished!", Methods.Verbosity.debug);

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
            Methods.LogVerbose(folders.Count + " folders were found in the bookmarks", Methods.Verbosity.debug);

            // Finding the end of the folders (</DL><p>) and adding the line number to the object array (folders[].endingline)
            // Counting the lines from the start while the folders from the back, so even in folders embedded into folders the endingline will be correct
            for (int j = 1; j < lineCount; j++)
            {
                string oneline = inputarray[j].Trim();
                if (oneline == "</DL><p>") // if we find a line that ends a folder
                {
                    for (int m = folders.Count - 1; m > 0; m--)
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
                    Methods.LogVerbose($"Folder {folders[k].name} with id {folders[k].id} has less than 2 lines. Start {folders[k].startline} end {folders[k].endingline}", Methods.Verbosity.warning);
                }
            }
            return folders;
        }

        public static List<Folderclass> HtmlExportIntake (string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
