using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NfLogger;

namespace Nfbookmark
{
    /// <summary>
    /// Type: Chromium of Firefox based
    /// </summary>
    public enum BrowserType { none, chromiumbased, firefoxbased }
    
    /// <summary>
    /// Representing the default locations (filepaths) where one browser might store its
    /// user profiles.
    /// </summary>
    public class BrowserLocations
    {
        public string BrowserName { get; set; } = "";
        public string WindowsProfilesPath { get; set; } = "";
        public List<string> LinuxProfilesPaths { get; set; } = new List<string>();
        public List<string> OsxProfilesPaths { get; set; } = new List<string>();
        public List<string> HardcodedPaths { get; set; } = new List<string>();
        
        /// <summary>
        /// List of paths to FILES containing bookmarks (one file for one browser profile usually)
        /// </summary>
        public List<string> FoundBookmarkFilePaths { get; set; } = new List<string>();
        
        /// <summary>
        /// Type of this browser: Chromium of Firefox based
        /// </summary>
        public BrowserType BrowserType { get; set; } = BrowserType.none;

        public override string ToString()
        {
            return $"Name:{BrowserName}, found profiles:{FoundBookmarkFilePaths.ToString()}";
        }
        
        /// <summary>
        /// Gives list of default locations for profiles for Linux, OSX and Windows
        /// </summary>
        /// <returns>List of folder paths where profile folders may be located (not just for installed browsers)</returns>
        public static List<BrowserLocations> GetBrowserLocations()
        {
            BrowserLocations Chrome = new BrowserLocations
            {
                BrowserName = "Chrome",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome") },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Google", "Chrome") },
            };
            BrowserLocations Chrome_beta = new BrowserLocations
            {
                BrowserName = "Chrome-beta",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome Beta", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-beta") },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Google", "Chrome Beta") }
            };
            BrowserLocations Chrome_canary = new BrowserLocations
            {
                BrowserName = "Chrome-canary",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome SxS", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-unstable") }, //technically its called chrome unstable on linux, but its the same thing
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Google", "Chrome Canary") }
            };
            BrowserLocations Brave = new BrowserLocations
            {
                BrowserName = "Brave-browser",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BraveSoftware", "Brave-Browser") },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "BraveSoftware", "Brave-Browser") }
            };
            BrowserLocations Chromium = new BrowserLocations
            {
                //great docs: https://chromium.googlesource.com/chromium/src/+/master/docs/user_data_dir.md
                BrowserName = "chromium",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chromium", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "chromium") },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Chromium") }
            };
            BrowserLocations Vivaldi = new BrowserLocations()
            {
                BrowserName = "Vivaldi",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vivaldi", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vivaldi") },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Vivaldi") }
            };
            BrowserLocations Edge = new BrowserLocations()
            {
                BrowserName = "Microsoft Edge",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "microsoft-edge") },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Microsoft Edge") }
                //.config/microsoft-edge/Default/Bookmarks
                // OSX:  /Users/username/Library/Application Support/Microsoft Edge/profilefolder/
                // C:\Users\<Current-user>\AppData\Local\Microsoft\Edge\User Data\Default.
            };
            BrowserLocations Opera = new BrowserLocations()
            {
                BrowserName = "Opera",
                BrowserType = BrowserType.chromiumbased,
                //C:\Users\%username%\AppData\Roaming\Opera Software\Opera Stable\Bookmarks is the Bookmarks file
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software"),
                LinuxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "opera") },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Opera Software") },
                HardcodedPaths = new List<string>()
                {
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "opera", "Bookmarks"),
                }
            };
            BrowserLocations Firefox = new BrowserLocations
            {
                BrowserName = "Firefox",
                BrowserType = BrowserType.firefoxbased,
                WindowsProfilesPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles"),
                LinuxProfilesPaths = new List<string> { 
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "snap", "firefox", "common", ".mozilla", "firefox"),
                    Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".mozilla", "firefox"),
                },
                OsxProfilesPaths = new List<string> { Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Firefox", "Profiles") },
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
                foreach (string path in browser.FoundBookmarkFilePaths)
                {
                    Logger.LogVerbose(m + ". path: " + path);
                    possibleFilePaths.Add(path);
                    m++;
                }
            }
            
            int chosenindexInt;
            while (true)
            {
                string chosenindex = Console.ReadLine();
                try
                {
                    chosenindexInt = int.Parse(chosenindex ?? throw new InvalidOperationException());
                    if (chosenindexInt < 1 || chosenindexInt >= m) { throw new IndexOutOfRangeException(); }
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
        private static BrowserLocations CheckChromiumBasedLocations(BrowserLocations browser)
        {
            Logger.LogVerbose("Checking browser locations for " + browser.BrowserName, Logger.Verbosity.Debug);
            if (browser.BrowserType != BrowserType.chromiumbased) { return CheckFirefoxLocations(browser); }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Directory.Exists(browser.WindowsProfilesPath))
                {
                    foreach (string profile in Directory.GetDirectories(browser.WindowsProfilesPath))
                    {
                        if (File.Exists(Path.Combine(profile, "Bookmarks")))
                        {
                            browser.FoundBookmarkFilePaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            Logger.LogVerbose("File found! Filepath in " + browser.BrowserName + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                        }
                    }
                    if (browser.FoundBookmarkFilePaths.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.BrowserName)); }
                }
                else if (browser.HardcodedPaths.Count != 0)
                {
                    foreach (string hardpath in browser.HardcodedPaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.FoundBookmarkFilePaths.Add(hardpath);
                            Logger.LogVerbose("File found! Filepath in " + browser.BrowserName + ": " + hardpath);
                        }

                    }
                    if (browser.FoundBookmarkFilePaths.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.BrowserName)); }
                }
                else { Logger.LogVerbose(browser.BrowserName + " install folder not found"); }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                foreach (string installlocation in browser.LinuxProfilesPaths)
                {
                    if (Directory.Exists(installlocation))
                    {
                        foreach (string profile in Directory.GetDirectories(installlocation))
                        {
                            if (File.Exists(Path.Combine(profile, "Bookmarks")))
                            {
                                //For every chrome profile that has bookmarks
                                browser.FoundBookmarkFilePaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                Logger.LogVerbose("File found! Filepath in " + browser.BrowserName + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            }
                        }
                        if (browser.FoundBookmarkFilePaths.Count == 0) { Logger.LogVerbose(($"Bookmarks file not found in " + browser.BrowserName)); }
                    }
                }
                if (browser.HardcodedPaths.Count != 0)
                {
                    foreach (string hardpath in browser.HardcodedPaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.FoundBookmarkFilePaths.Add(hardpath);
                            Logger.LogVerbose("File found! Filepath in " + browser.BrowserName + ": " + hardpath);
                        }

                    }
                    if (browser.FoundBookmarkFilePaths.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.BrowserName)); }
                }
                else { Logger.LogVerbose(browser.BrowserName + " install folder not found"); }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                foreach (string  installlocation in browser.OsxProfilesPaths)
                {
                    if (Directory.Exists(installlocation))
                    {
                        foreach (string profile in Directory.GetDirectories(installlocation))
                        {
                            if (File.Exists(Path.Combine(profile, "Bookmarks")))
                            {
                                browser.FoundBookmarkFilePaths.Add(Convert.ToString(Path.Combine(profile, "Bookmarks")));
                                Logger.LogVerbose("File found! Filepath in " + browser.BrowserName + ": " + Convert.ToString(Path.Combine(profile, "Bookmarks")));
                            }
                        }
                        if (browser.FoundBookmarkFilePaths.Count == 0) { Logger.LogVerbose(($"Bookmarks file not found in " + browser.BrowserName)); }
                    }
                }
                if (browser.HardcodedPaths.Count != 0)
                {
                    foreach (string hardpath in browser.HardcodedPaths)
                    {
                        if (File.Exists(hardpath))
                        {
                            browser.FoundBookmarkFilePaths.Add(hardpath);
                            Logger.LogVerbose("File found! Filepath in " + browser.BrowserName + ": " + hardpath);
                        }

                    }
                    if (browser.FoundBookmarkFilePaths.Count == 0) { Logger.LogVerbose(($"No Bookmarks file found in " + browser.BrowserName)); }
                }
                else { Logger.LogVerbose(browser.BrowserName + " install folder not found"); }
            }
            return browser;
        }

        /// <summary>
        /// Helper function for CheckChromiumBasedLocations
        /// </summary>
        /// <param name="browser">Firefox BrowserLocations object with folder paths</param>
        /// <returns>Firefox BrowserLocations object that has FoundBookmarkFilePaths filled</returns>
        private static BrowserLocations CheckFirefoxLocations(BrowserLocations browser)
        {
            if (browser.BrowserType != BrowserType.firefoxbased) { return  browser; }
            //Finding the sqlite databases
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // C:\Windows.old\Users\<UserName>\AppData\Roaming\Mozilla\Firefox\Profiles\<filename.default>\places.sqlite
                // filePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\<filename.default>\\places.sqlite");
                // profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\");
                // Console.WriteLine("profilespath " + profilespath);
                if (Directory.Exists(browser.WindowsProfilesPath))
                {
                    foreach (string profile in Directory.GetDirectories(browser.WindowsProfilesPath))
                    {
                        if (File.Exists(Path.Combine(profile, "places.sqlite")))
                        {
                            //For every firefox profile that has bookmarks
                            browser.FoundBookmarkFilePaths.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            Logger.LogVerbose("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                        }
                    }
                }
                else { Logger.LogVerbose($"{browser.BrowserName} install folder not found"); }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // /home/$User/snap/firefox/common/.mozilla/firefox/aaaa.default/places.sqlite/
                // /home/$User/.mozilla/firefox/aaaa.default/places.sqlite
                // string profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "snap/firefox/common/.mozilla/firefox/");
                foreach(string profilespath in browser.LinuxProfilesPaths)
                {
                    // for every firefox install present (eg. snap, flatpak, system package)
                    if (Directory.Exists(profilespath))
                    {
                        foreach (string profile in Directory.GetDirectories(profilespath))
                        {
                            if (File.Exists(Path.Combine(profile, "places.sqlite")))
                            {
                                //For every firefox profile that has bookmarks
                                browser.FoundBookmarkFilePaths.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                Logger.LogVerbose("File found! " + "Filepath in " + browser.BrowserName + ": " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            }
                        }
                    }
                    else { Logger.LogVerbose($"{browser.BrowserName} at {profilespath} not found"); }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // /Users/<username>/Library/Application Support/Firefox/Profiles/<profile folder>
                // profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Firefox/Profiles");
                foreach (string installlocation in browser.OsxProfilesPaths)
                {
                    if (Directory.Exists(installlocation))
                    {
                        foreach (string profile in Directory.GetDirectories(installlocation))
                        {
                            if (File.Exists(Path.Combine(profile, "places.sqlite")))
                            {
                                //For every firefox profile that has bookmarks
                                browser.FoundBookmarkFilePaths.Add(Convert.ToString(Path.Combine(profile, "places.sqlite")));
                                Logger.LogVerbose("File found! " + "Filepath in Firefox: " + Convert.ToString(Path.Combine(profile, "places.sqlite")));
                            }
                        }
                    }
                    else { Logger.LogVerbose("Firefox install folder not found."); }
                }
            }
            if (browser.FoundBookmarkFilePaths.Count == 0) { Logger.LogVerbose($"Bookmarks file not found in {browser.BrowserName}"); }
            return browser;
        }

        /// <summary>
        /// Complete list of browsers with profiles, supports crossplatform. Only shows installed browsers that have profiles.
        /// </summary>
        /// <returns>List with all browser and their paths that have any browser profiles</returns>
        public static List<BrowserLocations> GetBrowserBookmarkFilesPaths()
        {
            List<BrowserLocations> browserLocations = GetBrowserLocations(); //gets list of supported browsers
            return browserLocations
                .Select(browser => CheckChromiumBasedLocations(browser)) // Check each browser
                .Where(browser => browser.FoundBookmarkFilePaths.Count != 0) // Filter browsers with profiles
                .ToList();
        }
    }
}