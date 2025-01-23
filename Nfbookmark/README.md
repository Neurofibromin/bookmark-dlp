## About

<!-- A description of the package and where one can find more documentation -->

Provides general functions for importing bookmarks from different browsers across platforms.

## Key Features

<!-- The key features of this package -->

* Get list of installed browsers and browser profiles
* Import bookmarks from said browser profiles
* Import bookmarks from Google Takeout .html files
* Use the imported bookmarks for your purposes

## Supported platforms:
	- OSX x64 and ARM
	- Windows x64, x86 and ARM
	- Linux x64 and ARM
## Supported browsers:
    - Chrome
    - Chrome_beta
    - Chrome_canary
    - Brave
    - Chromium
    - Vivaldi
    - Edge
    - Opera
    - Firefox
## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->



Import all bookmarks from Chrome:
```csharp
using System;
using bookmark_dlp;

//set verbosity
bookmark_dlp.Logger.verbosity = Logger.Verbosity.Warning;
//get list of places where browser might be installed
List<BrowserLocations> maybeLocations = BrowserLocations.GetBrowserLocations();
//get list of places where browsers are actually installed
List<BrowserLocations> actualLocations = BrowserLocations.GetBrowserBookmarkFilesPaths();
//list found browsers and how many profiles they have
foreach (BrowserLocations location in actualLocations) 
{
    Console.WriteLine(location.browsername + " " + location.foundProfiles.Count);
    Console.WriteLine(location);
}
foreach (string location in actualLocations[0].foundProfiles)
{
    Console.WriteLine(location);
}
string path = actualLocations[0].foundProfiles[0];
Console.WriteLine(path);
List<Folderclass> folders = Import.SmartImport(path);
Functions.PrintToConsole(folders);
// One liner to showcase:
Functions.PrintToConsole(Import.SmartImport(Import.GetBrowserBookmarkFilesPaths()[0].foundProfiles[0]));
```

## Main Functions

<!-- The main functions provided in this library -->

The main functions provided by this library are:

* `bookmark_dlp.Import.SmartImport`
* `bookmark_dlp.Import.JsonIntake`
* `bookmark_dlp.Import.SqlIntake`
* `bookmark_dlp.Import.HtmlTakeoutIntake`
* `bookmark_dlp.BrowserLocations.GetBrowserBookmarkFilesPaths`
* `bookmark_dlp.BrowserLocations.GetBrowserLocations`
* `bookmark_dlp.BrowserLocations.QueryChosenBookmarksFile`
* `bookmark_dlp.Functions.FoldernameValidation`
* `bookmark_dlp.Functions.Createfolderstructure`
* `bookmark_dlp.Functions.Deleteemptyfolders`
* `bookmark_dlp.Functions.PrintToConsole`
Additionally:
* logging capacity:
* `NfLogger.Logger.LogVerbose()`
* `NfLogger.Logger.AddStream`
* `NfLogger.Logger.RemoveStream`
* `NfLogger.Logger.AddFile`
* `NfLogger.Logger.RemoveFile`

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `bookmark_dlp.DataStructures.Folderclass`
* `bookmark_dlp.DataStructures.Bookmark`
* `bookmark_dlp.DataStructures.BrowserLocations`

## Program using this package

* [bookmark-dlp](https://github.com/Neurofibromin/bookmark-dlp)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Nfbookmark is released as open source under the [GPLv3 license](https://www.gnu.org/licenses/gpl-3.0.en.html). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/Neurofibromin/bookmark-dlp).
