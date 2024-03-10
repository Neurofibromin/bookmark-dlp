# bookmark-dlp
Utility program for downloading bookmarked youtube links using yt-dlp. A program to replicate the folder structure of your Chrome/Brave/Firefox/etc. bookmarks and call yt-dlp to download all youtube videos amongst the bookmarks.

## Usage
Download the executable for your system from [releases](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest). Run it. Select your browser profile to auto import bookmarks or load a Google Takeout Chrome bookmarks html file. Currently "exports" from browsers do not work. Choose an output folder for the downloaded files - this cannot be a network folder on some platforms. If the yt-dlp executable not found automatically, select the location in the settings. If you do not have the newest version you can get it [here](https://github.com/yt-dlp/yt-dlp#installation).

## CLI Usage
Put the Bookmarks.html and the **yt-dlp(.exe) into the same directory as the executable**. Get the Bookmarks.html from a Google takeout. The yt-dlp executable can be found [here](https://github.com/yt-dlp/yt-dlp#installation).<br/>
If you do not provide the Bookmarks.html the program will check for default data directories of several browsers. More detail below.<br>
Run the executable: <br/>
Windows: in CMD or PowerShell: `bookmark-dlp-8.0.x.exe --console` <br/>
Linux: in terminal in the directory: `./bookmark-dlp-linux-x64-8.0.x --console`
More flags/usage info: `bookmark-dlp-.. --help`

## How it works
The program first checks if a Bookmarks.html exists within its root directory/where it was called from. If yes, that is the input file. If no, default locations for some browsers (Chrome, Brave and Firefox<sup>unstable</sup> currently) are checked, to find any browser profiles with bookmarks.<br/>
When a bookmark containing file is found or provided, the content is taken in, and the folder structure of the bookmarks bar is replicated on disk, in the root(running) directory.
The youtube links in the given folder are written into a \${foldername}.txt file. Complex youtube links (not links for video, but for channels and playlists etc) are placed in a separate \${foldername}.complex.txt.<br/>
A helper script (.bat or .sh) is also generated for each folder. If a folder contains no links (and no files at all) it is deleted. The helper scripts are called, these in turn call yt-dlp. The presence of yt-dlp binaries is checked in the root(running) directory (and the PATH (kind of)).
The scripts run one at a time, in sequence. I recommend also providing a **yt-dlp.conf file into program root** (running) directory, and enabling the archive function - that way bookmark-dlp can be run from the same directory again, and only new files get downloaded.
If the same directory is used for different profiles things can get written into same directories, but probably nothing should be lost.

## Releases
Windows and linux compatible, written in C#, builds for x86 and ARM available. <br/>
Build your own: this project is open source

### Status
[![.NET](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/prerelease.yml/badge.svg)](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/prerelease.yml)
[![.NET](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Neurofibromin/bookmark-dlp/actions/workflows/dotnet.yml)

### Aims
<br>CI/CD<br/>
- [ ] OpenSuse Build Service
- [ ] Flathub
- [ ] Debian
- [ ] Arch/AUR
- [ ] Fedora
- [ ] GitLab
<br>new features<br/>
- [ ] tray icon
- [ ] systemd module
- [ ] dot dekstop file
- [ ] add manpages

### Build instructions
Install dependencies: [dotnet](https://dotnet.microsoft.com/en-us/download)
```
git clone -b master https://github.com/Neurofibromin/bookmark-dlp bookmark-dlp
cd bookmark-dlp
dotnet restore
dotnet publish bookmark-dlp.sln --configuration Release
```
| Windows  | Linux | OSX (semi-supported) |
| ------------- | ------------- | ------------- |
| [x64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-8.0.x.exe) | [x64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-linux-x64-8.0.x) | [x64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-osx-x64-8.0.x)
| [x32](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-win-x86-8.0.x.exe) | N/A | N/A |
| [arm64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-win-arm64-8.0.x.exe) | [arm64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-linux-arm64-8.0.x) | [arm64](https://github.com/Neurofibromin/bookmark-dlp/releases/download/latest/bookmark-dlp-osx-arm64-8.0.x) |
