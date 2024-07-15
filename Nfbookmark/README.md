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
using Nfbookmark;

//set verbosity
Nfbookmark.Methods.verbostiy = Methods.Verbosity.Warning;
//get list of places where browser might be installed
List<BrowserLocations> maybeLocations = Import.GetBrowserLocations();
//get list of places where browsers are actually installed
List<BrowserLocations> actualLocations = Import.GetBrowserBookmarkFilesPaths();
//list found browsers and how many profiles they have
foreach (BrowserLocations location in actualLocations) 
{
    Console.WriteLine(location.browsername + " " + location.foundFiles.Count());
}
```

## Main Methods

<!-- The main functions provided in this library -->

The main functions provided by this library are:

* `Nfbookmark.Import.SmartImport`
* `Nfbookmark.Import.JsonIntake`
* `Nfbookmark.Import.SqlIntake`
* `Nfbookmark.Import.HtmlTakeoutIntake`
* `Nfbookmark.Import.GetBrowserBookmarkFilesPaths`
* `Nfbookmark.Import.GetBrowserLocations`
* `Nfbookmark.Import.QueryChosenBookmarksFile`

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Nfbookmark.DataStructures.Folderclass`
* `Nfbookmark.DataStructures.Bookmark`
* `Nfbookmark.DataStructures.BrowserLocations`

## Program using this package

* [bookmark-dlp](https://github.com/Neurofibromin/bookmark-dlp)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Nfbookmark is released as open source under the [GPLv3 license](https://www.gnu.org/licenses/gpl-3.0.en.html). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/Neurofibromin/bookmark-dlp).
