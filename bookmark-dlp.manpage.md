% bookmark-dlp(1) v0.4.3

2025. 11. 13

# NAME
**bookmark-dlp** - Program for downloading bookmarked YouTube videos.

# SYNOPSIS
|    `$ bookmark-dlp --source SOURCE --target TARGET [OPTIONS]`

# DESCRIPTION
Utility program for downloading bookmarked YouTube links. A program to replicate the folder structure of your Chrome/Brave/Firefox/etc. bookmarks and download all YouTube videos/playlists amongst the bookmarks.

# MODES

## Graphical
|    `$ bookmark-dlp`

## CLI
|    `bookmark-dlp --console --source SOURCE --outputfolder TARGET `
|    `bookmark-dlp [-h | --help ]`
Get the Bookmarks.html from a Google takeout or from a browser export. The yt-dlp executable can be found at <https://github.com/yt-dlp/yt-dlp#installation> or probably in your distribution's repository.


# CONFIGURATION
Config for bookmark-dlp can be local found in current directory, named "bookmark-dlp.conf" or in the directory of the assembly.
If no local config is provided, OS config directory is checked. On linux the $XDG_CONFIG_HOME/bookmark-dlp/bookmark-dlp.conf location.
If no config is found at startup popup asks for location for new config file. The config file is json.

yt-dlp binary locations are checked in the following order: 1. program is called from, 2. program executable is found in, 3. chosen output folder, 4. path

yt-dlp.config is sought in the locations set by the project: https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#configuration

# INSTALLATION

## Arch
`yay bookmark-dlp` or
`paru bookmark-dlp`

## Fedora and RHEL-like 
```
sudo dnf copr enable neurofibromin/bookmark-dlp
sudo dnf install bookmark-dlp
```

## OpenSUSE
## Debian
## Ubuntu
## Nix

# OPTIONS
allow-playlists
: downloads playlists

# EXAMPLES
WIP

# FILES
$XDG_CONFIG_HOME/bookmark-dlp/bookmark-dlp.conf

# DEPENDENCIES AND CAVEATS
yt-dlp, .net9.0

# HISTORY
WIP

# BUGS
Report issues at: <https://github.com/Neurofibromin/bookmark-dlp/issues>

# AUTHOR
Neurofibromin <https://github.com/Neurofibromin>

# SEE ALSO
Website: <https://github.com/Neurofibromin/bookmark-dlp>

# COPYRIGHT
Copyright (C) 2023-2025 Neurofibromin

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, only version 3 of the License. This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
