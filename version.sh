#!/bin/bash

# PKGBUILD
# README.md links
# workflows on top in env
# manpage.md
# Directory.Build.props for .csproj files
# bookmark-dlp.pupnet.conf
# bookmark-dlp.spec
# package.nix

#TODO: nix version update fix
#TODO: add innosetup support

# Check if the correct number of arguments is provided
if [ "$#" -ne 1 ]; then
    echo "Usage: $0 <new_version>"
    exit 1
fi

NEW_VERSION=$1
PKGBUILD_FILE=./packaging/PKGBUILD
README_FILE=./README.md
MANPAGE_FILE=./bookmark-dlp.manpage.md
NEW_DATE=$(date +'%Y. %m. %d')
DIRECTORY_BUILD_PROPS_FILE=./Directory.Build.props
PUPNET_CONF_FILE=./bookmark-dlp.pupnet.conf
SPEC_FILE2=./packaging/bookmark-dlp_for_copr.spec
NIX_FILE=./packaging/package.nix
PKGBUILD_SUM_UPDATER=./PKGBUILD_sum_update.sh

# Validate the PKGBUILD file exists
if [ ! -f "$PKGBUILD_FILE" ]; then
    echo "Error: File '$PKGBUILD_FILE' not found."
    exit 1
fi
# Replace version numbers (_tag, pkgver) in the PKGBUILD file
sed -i -E \
    -e "s/^_tag=[0-9]+\.[0-9]+\.[0-9]+/_tag=${NEW_VERSION}/" \
    -e "s/^pkgver=[0-9]+\.[0-9]+\.[0-9]+/pkgver=${NEW_VERSION}/" \
    "$PKGBUILD_FILE"
echo "Version updated to $NEW_VERSION in $PKGBUILD_FILE."

# Validate the README.md file exists
if [ ! -f "$README_FILE" ]; then
    echo "Error: File '$README_FILE' not found."
    exit 1
fi
# Replace all occurrences of the version in the file
sed -i -E "s/bookmark-dlp-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-${NEW_VERSION}/g" "$README_FILE"
# Also replace occurrences like "bookmark-dlp_0.4.0" (underscores in certain links)
sed -i -E "s/bookmark-dlp_[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp_${NEW_VERSION}/g" "$README_FILE"
sed -i -E "s/bookmark-dlp-win-x86-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-win-x86-${NEW_VERSION}/g" "$README_FILE"
sed -i -E "s/bookmark-dlp-win-x64-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-win-x64-${NEW_VERSION}/g" "$README_FILE"
sed -i -E "s/bookmark-dlp-win-arm64-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-win-arm64-${NEW_VERSION}/g" "$README_FILE"
sed -i -E "s/bookmark-dlp-linux-x64-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-linux-x64-${NEW_VERSION}/g" "$README_FILE"
sed -i -E "s/bookmark-dlp-linux-arm64-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-linux-arm64-${NEW_VERSION}/g" "$README_FILE"
sed -i -E "s/bookmark-dlp-osx-x64-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-osx-x64-${NEW_VERSION}/g" "$README_FILE"
sed -i -E "s/bookmark-dlp-osx-arm64-[0-9]+\.[0-9]+\.[0-9]+/bookmark-dlp-osx-arm64-${NEW_VERSION}/g" "$README_FILE"
echo "Version updated to $NEW_VERSION in $README_FILE."

# Validate the manpage.md file exists
if [ ! -f "$MANPAGE_FILE" ]; then
    echo "Error: File '$MANPAGE_FILE' not found."
    exit 1
fi
# Replace the version number in the header
sed -i -E "s/^% bookmark-dlp\(1\) v[0-9]+\.[0-9]+\.[0-9]+/% bookmark-dlp(1) v${NEW_VERSION}/" "$MANPAGE_FILE"
# Replace the date below the header
sed -i -E "s/^[0-9]{4}\. [0-9]{2}\. [0-9]{2}/${NEW_DATE}/" "$MANPAGE_FILE"
echo "Version updated to $NEW_VERSION and date updated to $NEW_DATE in $MANPAGE_FILE."

# Validate the Directory Build props file exists
if [ ! -f "$DIRECTORY_BUILD_PROPS_FILE" ]; then
    echo "Error: File '$DIRECTORY_BUILD_PROPS_FILE' not found."
    exit 1
fi
# Replace the version inside the <Version> tag
sed -i -E "s|<Version>[0-9]+\.[0-9]+\.[0-9]+</Version>|<Version>${NEW_VERSION}</Version>|" "$DIRECTORY_BUILD_PROPS_FILE"
echo "Version updated to $NEW_VERSION in $DIRECTORY_BUILD_PROPS_FILE."

# Validate the PupNet configuration file exists
if [ ! -f "$PUPNET_CONF_FILE" ]; then
    echo "Error: File '$PUPNET_CONF_FILE' not found."
    exit 1
fi
# Replace the AppVersionRelease value
sed -i -E "s/^AppVersionRelease = [0-9]+\.[0-9]+\.[0-9]+/AppVersionRelease = ${NEW_VERSION}/" "$PUPNET_CONF_FILE"
echo "AppVersionRelease updated to $NEW_VERSION in $PUPNET_CONF_FILE."

# Validate the copr spec file exists
if [ ! -f "$SPEC_FILE2" ]; then
    echo "Error: File '$SPEC_FILE2' not found."
    exit 1
fi
# Replace the Version value
sed -i -E "s/^Version:        [0-9]+\.[0-9]+\.[0-9]+/Version:        ${NEW_VERSION}/" "$SPEC_FILE2"
echo "Version updated to $NEW_VERSION in $SPEC_FILE2."

# Regenerate manpages:
if ! command -v pandoc 2>&1 >/dev/null
then
    echo "pandoc could not be found, not generating manpages"
else
    pandoc -s -f markdown -t man "$MANPAGE_FILE" -o bookmark-dlp.1  
fi

# Validate the PKGBUILD_SUM_UPDATER file exists
if [ ! -f "$PKGBUILD_SUM_UPDATER" ]; then
    echo "Error: File '$PKGBUILD_SUM_UPDATER' not found."
    exit 1
fi
# Replace the Version value
sed -i -E "s/^Version=[0-9]+\.[0-9]+\.[0-9]+/Version=${NEW_VERSION}/" "$PKGBUILD_SUM_UPDATER"
echo "Version updated to $NEW_VERSION in $PKGBUILD_SUM_UPDATER."


# Validate the nix file exists
if [ ! -f "$NIX_FILE" ]; then
    echo "Error: File '$NIX_FILE' not found."
    exit 1
fi
# Replace the Version value
sed -i -E "s/(version = \")[0-9]+\.[0-9]+\.[0-9]+(\";)/\1${NEW_VERSION}\2/" "$NIX_FILE"
#old: sed -i -E "s/^version = "[0-9]\+\.[0-9]\+\.[0-9]\+"/version = "${NEW_VERSION}/"" "$NIX_FILE"
echo "Version updated to $NEW_VERSION in $NIX_FILE."