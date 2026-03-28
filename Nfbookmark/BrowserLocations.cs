using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Serilog;

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
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(BrowserLocations));
        
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
            return $"Name:{BrowserName}, found profiles:{string.Join(", ", FoundBookmarkFilePaths)}";
        }
        
        /// <summary>
        /// Gives list of default locations for profiles for Linux, OSX and Windows
        /// </summary>
        /// <returns>List of folder paths where profile folders may be located (not just for installed browsers)</returns>
        public static List<BrowserLocations> GetDefaultBrowserConfigurations()
        {
            var chrome = new BrowserLocations
            {
                BrowserName = "Chrome",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome") },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Google", "Chrome") },
            };
            var chromeBeta = new BrowserLocations
            {
                BrowserName = "Chrome-beta",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome Beta", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-beta") },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Google", "Chrome Beta") }
            };
            var chromeCanary = new BrowserLocations
            {
                BrowserName = "Chrome-canary",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome SxS", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "google-chrome-unstable") }, //technically its called chrome unstable on linux, but its the same thing
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Google", "Chrome Canary") }
            };
            var brave = new BrowserLocations
            {
                BrowserName = "Brave-browser",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BraveSoftware", "Brave-Browser") },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "BraveSoftware", "Brave-Browser") }
            };
            var chromium = new BrowserLocations
            {
                //great docs: https://chromium.googlesource.com/chromium/src/+/master/docs/user_data_dir.md
                BrowserName = "chromium",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chromium", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "chromium") },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Chromium") }
            };
            var vivaldi = new BrowserLocations
            {
                BrowserName = "Vivaldi",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vivaldi", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vivaldi") },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Vivaldi") }
            };
            var edge = new BrowserLocations
            {
                BrowserName = "Microsoft Edge",
                BrowserType = BrowserType.chromiumbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "microsoft-edge") },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Microsoft Edge") }
                //.config/microsoft-edge/Default/Bookmarks
                // OSX:  /Users/username/Library/Application Support/Microsoft Edge/profilefolder/
                // C:\Users\<Current-user>\AppData\Local\Microsoft\Edge\User Data\Default.
            };
            var opera = new BrowserLocations
            {
                BrowserName = "Opera",
                BrowserType = BrowserType.chromiumbased,
                //C:\Users\%username%\AppData\Roaming\Opera Software\Opera Stable\Bookmarks is the Bookmarks file
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software"),
                LinuxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "opera") },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Opera Software") },
                HardcodedPaths = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "opera", "Bookmarks"),
                }
            };
            var firefox = new BrowserLocations
            {
                BrowserName = "Firefox",
                BrowserType = BrowserType.firefoxbased,
                WindowsProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles"),
                LinuxProfilesPaths = new List<string> { 
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "snap", "firefox", "common", ".mozilla", "firefox"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".mozilla", "firefox"),
                },
                OsxProfilesPaths = new List<string> { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Firefox", "Profiles") },
            };
            
            return new List<BrowserLocations>
            {
                chrome, chromeBeta, chromeCanary, brave, chromium, vivaldi, edge, opera, firefox
            };
        }

        /// <summary>
        /// Interactively query which file containing bookmarks should be selected
        /// </summary>
        /// <param name="browserLocations">Preferably from GetDefaultBrowserConfigurations()</param>
        /// <returns>Path (string) to chosen file or null</returns>
        public static string QueryChosenBookmarksFile(List<BrowserLocations> browserLocations)
        {
            if (browserLocations == null || !browserLocations.Any())
            {
                Log.Warning("QueryChosenBookmarksFile called with no browser locations.");
                return null;
            }

            var possibleFilePaths = browserLocations
                .SelectMany(browser => browser.FoundBookmarkFilePaths)
                .ToList();

            if (!possibleFilePaths.Any())
            {
                Log.Warning("No bookmark files found to choose from.");
                return null;
            }

            Console.WriteLine("Which browser bookmarks would you like to use? Please enter the number:");
            for (int i = 0; i < possibleFilePaths.Count; i++)
            {
                Console.WriteLine($"{i + 1}. path: {possibleFilePaths[i]}");
            }
            
            while (true)
            {
                string chosenIndex = Console.ReadLine();
                if (int.TryParse(chosenIndex, out int chosenIndexInt) && chosenIndexInt >= 1 && chosenIndexInt <= possibleFilePaths.Count)
                {
                    string chosenFilePath = possibleFilePaths.ElementAt(chosenIndexInt - 1);
                    Log.Information("User chose bookmark file: {FilePath}", chosenFilePath);
                    return chosenFilePath;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number from the list.");
                }
            }
        }

        private static BrowserLocations FindBookmarkFiles(BrowserLocations browser)
        {
            Log.Debug("Checking for {BrowserName} bookmark files.", browser.BrowserName);
            
            switch (browser.BrowserType)
            {
                case BrowserType.chromiumbased:
                    return CheckChromiumBasedLocations(browser);
                case BrowserType.firefoxbased:
                    return CheckFirefoxLocations(browser);
                default:
                    Log.Error("Invalid browser type '{BrowserType}' for {BrowserName}", browser.BrowserType, browser.BrowserName);
                    return null;
            }
        }

        /// <summary>
        /// Checks for existing profiles in the default places for a browser. Supports GNU/Linux, OSX, Windows.
        /// </summary>
        /// <param name="browser">The browser to be examined for profile folders. Expects BroserLocations object with profilespaths filled in already.</param>
        /// <returns>Browserlocations object that has foundfiles and linksfound filled. If no profiles are found then the BrowserLocations object is returned as it was.</returns>
        private static BrowserLocations CheckChromiumBasedLocations(BrowserLocations browser)
        {
            var platformPaths = new List<string>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) platformPaths.Add(browser.WindowsProfilesPath);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) platformPaths.AddRange(browser.LinuxProfilesPaths);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) platformPaths.AddRange(browser.OsxProfilesPaths);

            foreach (var path in platformPaths)
            {
                if (Directory.Exists(path))
                {
                    foreach (string profile in Directory.GetDirectories(path))
                    {
                        string bookmarkFile = Path.Combine(profile, "Bookmarks");
                        if (File.Exists(bookmarkFile))
                        {
                            browser.FoundBookmarkFilePaths.Add(bookmarkFile);
                            Log.Information("Found {BrowserName} bookmark file: {BookmarkPath}", browser.BrowserName, bookmarkFile);
                        }
                    }
                }
                else
                {
                    Log.Debug("{BrowserName} profile directory not found at {ProfilePath}", browser.BrowserName, path);
                }
            }

            foreach (string hardpath in browser.HardcodedPaths)
            {
                if (File.Exists(hardpath))
                {
                    browser.FoundBookmarkFilePaths.Add(hardpath);
                    Log.Information("Found {BrowserName} bookmark file at hardcoded path: {BookmarkPath}", browser.BrowserName, hardpath);
                }
            }

            if (browser.FoundBookmarkFilePaths.Count == 0)
            {
                Log.Debug("No bookmark files found for {BrowserName}", browser.BrowserName);
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
            //Paths:
            //windows
            // C:\Windows.old\Users\<UserName>\AppData\Roaming\Mozilla\Firefox\Profiles\<filename.default>\places.sqlite
            // filePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\<filename.default>\\places.sqlite");
            // profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles\\");
            //linux
            // /home/$User/snap/firefox/common/.mozilla/firefox/aaaa.default/places.sqlite/
            // /home/$User/.mozilla/firefox/aaaa.default/places.sqlite
            // string profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "snap/firefox/common/.mozilla/firefox/");
            //OSX
            // /Users/<username>/Library/Application Support/Firefox/Profiles/<profile folder>
            // profilespath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Firefox/Profiles");

            
            
            if (browser.BrowserType != BrowserType.firefoxbased) return browser;
            //Finding the sqlite databases
            var platformPaths = new List<string>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) platformPaths.Add(browser.WindowsProfilesPath);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) platformPaths.AddRange(browser.LinuxProfilesPaths);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) platformPaths.AddRange(browser.OsxProfilesPaths);

            foreach (var path in platformPaths)
            {
                if (Directory.Exists(path))
                {
                    foreach (string profile in Directory.GetDirectories(path))
                    {
                        string bookmarkFile = Path.Combine(profile, "places.sqlite");
                        if (File.Exists(bookmarkFile))
                        {
                            browser.FoundBookmarkFilePaths.Add(bookmarkFile);
                            Log.Information("Found {BrowserName} bookmark file: {BookmarkPath}", browser.BrowserName, bookmarkFile);
                        }
                    }
                }
                else
                {
                    Log.Debug("{BrowserName} profile directory not found at {ProfilePath}", browser.BrowserName, path);
                }
            }
            
            if (browser.FoundBookmarkFilePaths.Count == 0)
            {
                Log.Debug("No bookmark files found for {BrowserName}", browser.BrowserName);
            }
            return browser;
        }

        /// <summary>
        /// Complete list of browsers with profiles, supports crossplatform. Only shows installed browsers that have profiles.
        /// </summary>
        /// <returns>List with all browser and their paths that have any browser profiles</returns>
        public static List<BrowserLocations> GetBrowserBookmarkFilesPaths()
        {
            List<BrowserLocations> browserLocations = GetDefaultBrowserConfigurations();
            return browserLocations
                .Select(FindBookmarkFiles)
                .Where(browser => browser != null && browser.FoundBookmarkFilePaths.Any())
                .ToList();
        }
    }
}