## About

<!-- A description of the package and where one can find more documentation -->

Provides general functions for importing bookmarks from different browsers across platforms.

## Key Features

<!-- The key features of this package -->

* Get list of installed browsers and browser profiles
* Import bookmarks from said browser profiles
* Import bookmarks from Google Takeout .html files
* Import bookmarks from browser exported bookmarks .html files
* Use the imported bookmarks for your purposes

## Supported platforms:

	- OSX x64 and ARM
	- Windows x64, x86 and ARM
	- Linux x64 and ARM

## Supported browsers:

    - Chrome (Stable, Beta, Canary)
    - Chromium
    - Brave
    - Vivaldi
    - Edge
    - Opera
    - Firefox

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

### Prerequisites
This library uses **Serilog** for logging. You should configure a logger in your application startup if you wish to see library output.

### Code Example

Import all bookmarks from all detected browsers:

```csharp
using System;
using System.Collections.Generic;
using Nfbookmark;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Warning()
    .CreateLogger();
// Get a list of places where browser might be installed
List<BrowserLocations> maybeLocations = BrowserLocations.GetBrowserLocations();
// Get a list of actual bookmark files found on the system
List<BrowserLocations> actualLocations = BrowserLocations.GetBrowserBookmarkFilesPaths();

foreach (BrowserLocations location in actualLocations) 
{
    Console.WriteLine($"Browser: {location.BrowserName}");
    
    foreach (string path in location.FoundBookmarkFilePaths)
    {
        Console.WriteLine($"Importing from: {path}");
        
        // Import bookmarks using the Factory automatically determines if it is JSON, SQLite, or HTML
        List<Folderclass> folders = BookmarkImporterFactory.SmartImport(path);

        // Pretty print the structure to Console
        Legacy.PrintToStream(folders);
        
        // Example: Create a physical folder structure from bookmarks
        // FolderManager.CreateFolderStructure(folders, "./MyBookmarksBackup");
    }
}
```

## Main Functions

<!-- The main functions provided in this library -->

### Importing
* `Nfbookmark.BookmarkImporterFactory.SmartImport(string filePath)`: The primary entry point. Detects file type and imports bookmarks.

### Browser Discovery
* `Nfbookmark.BrowserLocations.GetBrowserBookmarkFilesPaths()`: Returns a list of browsers with valid bookmark files found on the system.
* `Nfbookmark.BrowserLocations.GetDefaultBrowserConfigurations()`: Returns the default search paths for supported browsers.
* `Nfbookmark.BrowserLocations.QueryChosenBookmarksFile(...)`: Helper to interactively ask a user via Console which file to load.

### Folder Management
* `Nfbookmark.FolderManager.ValidateFolderNames(...)`: Sanitizes bookmark folder names for use as filesystem directory names.
* `Nfbookmark.FolderManager.CreateFolderStructure(...)`: Creates a physical directory tree matching the bookmark structure.
* `Nfbookmark.FolderManager.DeleteEmptyFolders(...)`: Cleans up empty directories in the provided structure.

### Utilities
* `Nfbookmark.Legacy.PrintToStream(...)`: Pretty-prints the bookmark hierarchy to Console or a Stream.

## Logging

This library uses [Serilog](https://serilog.net/). To capture logs from `Nfbookmark`, configure `Log.Logger` in your application before calling library functions.

## Main Types

<!-- The main types provided in this library -->

* `Nfbookmark.Folderclass`: Represents a bookmark folder (contains URL strings and child Folder IDs).
* `Nfbookmark.YTLink`: Represents a parsed YouTube link with metadata (Type, ID).
* `Nfbookmark.BrowserLocations`: Contains paths and profile information for a specific browser.
* `Nfbookmark.Bookmark`: Represents a single bookmark (may be folder).

## Program using this package

* [bookmark-dlp](https://github.com/Neurofibromin/bookmark-dlp)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Nfbookmark is released as open source under the [GPLv3 license](https://www.gnu.org/licenses/gpl-3.0.en.html).
Bug reports and contributions are welcome at [the GitHub repository](https://github.com/Neurofibromin/bookmark-dlp).
