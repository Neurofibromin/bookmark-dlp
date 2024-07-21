# bookmark-dlp
Utility program for downloading bookmarked youtube links using yt-dlp. It replicates the folder structure of your Chrome/Brave/Firefox/etc. bookmarks and calls yt-dlp to download all YouTube videos among the bookmarks.

## Usage
- Run the executable and select your browser profile to auto-import bookmarks, or load a Google Takeout Chrome bookmarks HTML file.
- Choose an output folder for the downloaded files.
- If necessary, specify the location of the yt-dlp executable in the settings.
- CLI usage options are available.


## Installation & How to get
<em>Make sure yt-dlp is installed: If you do not have the newest version you can get it [here](https://github.com/yt-dlp/yt-dlp#installation).</em>
Download the executable for your system from [releases](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest). Run it.

## Limitations
- Currently "exports" from browsers do not work.
- Output folder cannot be a network folder on some platforms.

## CLI Usage
Put the Bookmarks.html and the **yt-dlp(.exe) into the same directory as the executable**. Get the Bookmarks.html from a Google takeout.<br/>

Run the executable: <br/>
Windows: in CMD or PowerShell: `bookmark-dlp-8.0.x.exe --interactive` <br/>
Linux: in terminal in the directory: `./bookmark-dlp-linux-x64-8.0.x --interactive`
More flags/usage info: `bookmark-dlp-.. --help`

## How it works
1. The program checks for a Bookmarks.html file in its root directory. If not found, it searches default locations for supported browsers.
2. It replicates the folder structure of the bookmarks bar on disk.
3. YouTube links are extracted from the bookmarks and organized into text files.
4. Helper scripts are generated for each folder to call yt-dlp for downloading.
5. Optional: Provide a yt-dlp.conf file in the program root directory for advanced configurations.

## More details
If a Bookmarks.html exists within its root directory/where it was called from, that is the default input file. Default locations for some browsers (see list of supported browsers) are checked, to find any browser profiles with bookmarks.<br/>
When a bookmark containing file is found or provided, the content is taken in, and the folder structure of the bookmarks bar is replicated on disk, in the chosen output directory (root(running) directory by default).
The youtube links in the given folder are written into a \${foldername}.txt file. Complex youtube links (not links for video, but for channels and playlists etc) can be included, but are excluded by default.<br/>
A helper script (.bat or .sh) is also generated for each folder. If a folder contains no links (and no files at all) it is deleted. The helper scripts are called, these in turn call yt-dlp. The presence of yt-dlp binaries is checked details at [Locations for files](#locations-for-files).
The scripts run one at a time, in sequence. I recommend also providing a **yt-dlp.conf file into program root** (running) directory, and enabling the archive function - that way bookmark-dlp can be run from the same directory again, and only new files get downloaded.
If the same directory is used for different profiles things can get written into same directories, but probably nothing should be lost.

### Locations for files
Config for bookmark-dlp:
1. local 
    1. Path.Combine(Directory.GetCurrentDirectory(), "bookmark-dlp.conf");
    2. Assembly.GetExecutingAssembly
2. OS config directory
    * windows = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bookmark-dlp\\bookmark-dlp.conf");
    * linux = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmark-dlp/bookmark-dlp.conf"); 
    * osx = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "bookmark-dlp/bookmark-dlp.conf");
If no config is found at startup popup asks for location for new config file.

yt-dlp:
1. program is called from
2. program executable is found in
3. chosen output folder

accepted filenames:
- osx: { "yt-dlp", "yt-dlp_macos", "yt-dlp_macos_legacy" }
- linux: {"yt-dlp", "yt-dlp_linux", "yt-dlp_linux_aarch64", "yt-dlp_linux_armv7l" }
- windows: "yt-dlp.exe"

yt-dlp.config is sought in the following locations:
1. program is called from
2. program executable is found in
3. chosen output folder
4. yt-dlp executable is found in (even if manually selected)
5. yt-dlp default folder (somewhere in .local?)


## Releases
Windows and linux compatible, written in C#, builds for x86 and ARM available. Flatpaks, DEB and RPM packages are also generated. <br/>
Build your own: this project is open source

### Status
[![.NET](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/prerelease.yml/badge.svg)](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/prerelease.yml)
[![.NET](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/dotnet.yml)
[![.NET](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/nuget.yml/badge.svg)](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/nuget.yml)

### Aims
<br>CI/CD<br/>
- [ ] OpenSuse Build Service
- [ ] Flathub
- [ ] Debian
- [ ] Arch/AUR
- [ ] Fedora
- [ ] [Gitea mirror](N/A)
- [x] [sourceforge mirror](https://sourceforge.net/projects/bookmark-dlp/)
- [x] [NuGet](https://www.nuget.org/packages/nfbookmark)

<br>new features<br/>
- [x] start console and gui from same binary
- [x] better versioning
- [x] refactor bookmarks import to library
- [ ] tray icon
- [ ] systemd module
- [ ] dot dekstop file
- [ ] add manpages
- [ ] logging

<br>Increase support<br/>
- [ ] safari support
- [ ] opera osx support
- [ ] bsd support
- [ ] docker
- [ ] netcore3.1
- [ ] innosetup
- [ ] browsers installed as flatpaks

### Build instructions
Install dependencies: [dotnet](https://dotnet.microsoft.com/en-us/download)
```
git clone -b master https://github.com/Neurofibromin/bookmark-dlp bookmark-dlp
cd bookmark-dlp
dotnet restore
dotnet publish bookmark-dlp/bookmark-dlp.csproj --configuration Release
```

## Standalone releases
| Windows  | Linux | OSX (semi-supported) |
| ------------- | ------------- | ------------- |
| [x64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-8.0.x.exe) | [x64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-linux-x64-8.0.x) | [x64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-osx-x64-8.0.x)
| [x32](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-win-x86-8.0.x.exe) | N/A | N/A |
| [arm64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-win-arm64-8.0.x.exe) | [arm64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-linux-arm64-8.0.x) | [arm64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-osx-arm64-8.0.x) |

### Additional releases:
Linux Installers: <br/>

| package  | x64 | arm64 |
| ------------- | ------------- | ------------- |
| Flatpak	|		[bookmark-dlp-0.3.0.x86_64.flatpak](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-0.2.7-1.x86_64.flatpak)		|	[bookmark-dlp-0.3.0.aarch64.flatpak](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-0.2.7-1.aarch64.flatpak)			|
|    RPM	|		[bookmark-dlp-0.3.0.x86_64.rpm](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp_0.2.7-1.x86_64.rpm)		|	N/A			|
|    DEB	|		[bookmark-dlp-0.3.0.amd64.deb](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp_0.2.7-1_amd64.deb)		|	[bookmark-dlp-0.3.0.arm64.deb](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp_0.2.7-1_arm64.deb)			|
| AppImage	|		[bookmark-dlp-0.3.0.x86_64.AppImage](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-0.2.7-1.x86_64.AppImage)		|	[bookmark-dlp-0.3.0.aarch64.AppImage](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-0.2.7-1.aarch64.AppImage)			|
