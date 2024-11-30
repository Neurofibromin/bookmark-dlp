using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using NfLogger;

namespace bookmark_dlp
{
    public partial class BrowserLocations
    {
        /// <summary>
        /// Gives list of default locations for profiles for Linux, OSX and Windows
        /// </summary>
        /// <returns>List of folder paths where profile folders may be located (not just for installed browsers)</returns>
        public static List<BrowserLocations> GetBrowserLocations()
        {
            BrowserLocations Chrome = new BrowserLocations
            {
                browsername = "Chrome",
                browserType = BrowserType.chromiumbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data\\"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome") },
            };
            BrowserLocations Chrome_beta = new BrowserLocations
            {
                browsername = "Chrome-beta",
                browserType = BrowserType.chromiumbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome Beta\\User Data\\"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-beta") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome Beta") }
            };
            BrowserLocations Chrome_canary = new BrowserLocations
            {
                browsername = "Chrome-canary",
                browserType = BrowserType.chromiumbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome SxS\\User Data\\"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-unstable") }, //technically its called chrome unstable on linux, but its the same thing
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Google/Chrome Canary") }
            };
            BrowserLocations Brave = new BrowserLocations
            {
                browsername = "Brave-browser",
                browserType = BrowserType.chromiumbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware\\Brave-Browser\\User Data\\"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BraveSoftware/Brave-Browser/") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/BraveSoftware/Brave-Browser") }
            };
            BrowserLocations Chromium = new BrowserLocations
            {
                //great docs: https://chromium.googlesource.com/chromium/src/+/master/docs/user_data_dir.md
                browsername = "chromium",
                browserType = BrowserType.chromiumbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chromium\\User Data"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "chromium") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Chromium") }
            };
            BrowserLocations Vivaldi = new BrowserLocations()
            {
                browsername = "Vivaldi",
                browserType = BrowserType.chromiumbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vivaldi\\User Data"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vivaldi") },
                osx_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Vivaldi") }
            };
            BrowserLocations Edge = new BrowserLocations()
            {
                browsername = "Microsoft Edge",
                browserType = BrowserType.chromiumbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data"),
                linux_profilespath = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "microsoft-edge") }, 
                //.config/microsoft-edge/Default/Bookmarks
                // C:\Users\<Current-user>\AppData\Local\Microsoft\Edge\User Data\Default.
                //TODO: osx
            };
            BrowserLocations Opera = new BrowserLocations()
            {
                browsername = "Opera",
                browserType = BrowserType.chromiumbased,
                //C:\Users\%username%\AppData\Roaming\Opera Software\Opera Stable\Bookmarks is the Bookmarks file
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software"),
                //opera: .config/opera/Bookmarks
                hardcodedpaths = new List<string>()
                {
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "opera/Bookmarks"),
                }
                //TODO: osx and linux
            };
            BrowserLocations Firefox = new BrowserLocations
            {
                browsername = "Firefox",
                browserType = BrowserType.firefoxbased,
                windows_profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\"),
                //linksfound = "",
                linux_profilespath = new List<string> { 
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "snap/firefox/common/.mozilla/firefox/"),
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".mozilla/firefox"),
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
        /// <param name="browserLocations">Preferably from GetBrowserLocations()</param>
        /// <returns>Path (string) to chosen file or null</returns>
        public static string QueryChosenBookmarksFile(List<BrowserLocations> browserLocations)
        {
            if (browserLocations.Count == 0) { return null; }
            string chosenFilePath;
            List<string> possibleFilePaths = new List<string>();
            Logger.LogVerbose("Which browser bookmarks would you like to use?\nWrite the number");
            int m = 1;
            foreach (BrowserLocations browser in browserLocations)
            {
                foreach (string path in browser.foundProfiles)
                {
                    Logger.LogVerbose(m + ". path: " + path);
                    possibleFilePaths.Add(path);
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
                    Logger.LogVerbose("Wrong input", Logger.Verbosity.Error);
                    continue;
                }
            }
            chosenFilePath = possibleFilePaths.ElementAt(chosenindexInt - 1);
            Logger.LogVerbose("Chosen path: " + chosenFilePath);
            return chosenFilePath;
        }

        /// <summary>
        /// Checks for existing profiles in the default places for a browser. Supports GNU/Linux, OSX, Windows.
        /// </summary>
        /// <param name="browser">The browser to be examined for profile folders. Expects BroserLocations object with profilespaths filled in already.</param>
        /// <returns>Browserlocations object that has foundfiles and linksfound filled. If no profiles are found then the BrowserLocations object is returned as it was.</returns>
        private static BrowserLocations BrowserCheck(BrowserLocations browser)
        {
            Logger.LogVerbose("Checking browser locations for " + browser.browsername, Logger.Verbosity.Debug);
            if (browser.browserType != BrowserType.chromiumbased) { return firefoxBrowserCheck(browser); }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Directory.Exists(browser.windows_profilespath))
                {
                    foreach (string profile in Directory.GetDirectories(browser.windows_profilespath))
                    {
                        if (File.Exists(Path.Combine(profile, "Bookmarks")))
                        {
                            browser.foundProfiles.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            Logger.LogVerbose("File found! Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        }
                    }
                    if (browser.foundProfiles.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else if (browser.hardcodedpaths.Count != 0)
                {
                    foreach (string hardpath in browser.hardcodedpaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.foundProfiles.Add(hardpath);
                            Logger.LogVerbose("File found! Filepath in " + browser.browsername + ": " + hardpath);
                        }

                    }
                    if (browser.foundProfiles.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else { Logger.LogVerbose(browser.browsername + " install folder not found"); }
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
                                browser.foundProfiles.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                Logger.LogVerbose("File found! Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            }
                        }
                        if (browser.foundProfiles.Count == 0) { Logger.LogVerbose(($"Bookmarks file not found in " + browser.browsername)); }
                    }
                }
                if (browser.hardcodedpaths.Count != 0)
                {
                    foreach (string hardpath in browser.hardcodedpaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.foundProfiles.Add(hardpath);
                            Logger.LogVerbose("File found! Filepath in " + browser.browsername + ": " + hardpath);
                        }

                    }
                    if (browser.foundProfiles.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else { Logger.LogVerbose(browser.browsername + " install folder not found"); }
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
                                browser.foundProfiles.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                Logger.LogVerbose("File found! Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            }
                        }
                        if (browser.foundProfiles.Count == 0) { Logger.LogVerbose(($"Bookmarks file not found in " + browser.browsername)); }
                    }
                }
                if (browser.hardcodedpaths.Count != 0)
                {
                    foreach (string hardpath in browser.hardcodedpaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.foundProfiles.Add(hardpath);
                            Logger.LogVerbose("File found! Filepath in " + browser.browsername + ": " + hardpath);
                        }

                    }
                    if (browser.foundProfiles.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.browsername)); }
                }
                else { Logger.LogVerbose(browser.browsername + " install folder not found"); }
            }
            return browser;
        }

        /// <summary>
        /// Helper function for BrowserCheck
        /// </summary>
        /// <param name="browser">Firefox BrowserLocations object with folder paths</param>
        /// <returns>Firefox BrowserLocations object that has foundProfiles filled</returns>
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
                            browser.foundProfiles.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            Logger.LogVerbose("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        }
                    }
                }
                else { Logger.LogVerbose($"{browser.browsername} install folder not found"); }
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
                                browser.foundProfiles.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                Logger.LogVerbose("File found! " + "Filepath in " + browser.browsername + ": " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            }
                        }
                    }
                    else { Logger.LogVerbose($"{browser.browsername} at {profilespath} not found"); }
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
                                browser.foundProfiles.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                Logger.LogVerbose("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            }
                        }
                    }
                    else { Logger.LogVerbose("Firefox install folder not found."); }
                }
            }
            if (browser.foundProfiles.Count == 0) { Logger.LogVerbose($"Bookmarks file not found in {browser.browsername}"); }
            return browser;
        }

        /// <summary>
        /// Complete list of browsers with profiles, supports crossplatform. Only shows installed browsers that have profiles.
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
                TotalLinksFoundCount += browser.foundProfiles.Count;
                if (browser.foundProfiles.Count != 0) { completeBrowserLocations.Add(browser); } //only return with browsers that have profiles
            }
            if (TotalLinksFoundCount == 0)
            {
                return null;
            }
            return completeBrowserLocations;
        }
    }
}