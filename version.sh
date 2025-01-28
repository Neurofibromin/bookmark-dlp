#!/bin/bash

# PKGBUILD
# README.md links
# workflows on top in env
# manpage.md
# .csproj files
# bookmark-dlp.pupnet.conf
# bookmark-dlp.spec

# Check if the correct number of arguments is provided
if [ "$#" -ne 1 ]; then
    echo "Usage: $0 <new_version>"
    exit 1
fi

NEW_VERSION=$1
PKGBUILD_FILE=./PKGBUILD
README_FILE=./README.md
NUGET_YML_FILE=./.github/workflows/nuget.yml
PRERELEASE_YML_FILE=./.github/workflows/prerelease.yml
PRERELEASE_GITEA_YML_FILE=./.github/workflows/prerelease_gitea.yml
MANPAGE_FILE=./bookmark-dlp.manpage.md
NEW_DATE=$(date +'%Y. %m. %d')
NFBOOKMARK_CSPROJ_FILE=./Nfbookmark/Nfbookmark.csproj
BOOKMARK_DLP_CSPROJ_FILE=./bookmark-dlp/bookmark-dlp.csproj
PUPNET_CONF_FILE=./bookmark-dlp.pupnet.conf
SPEC_FILE=./bookmark-dlp.spec

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
echo "Version updated to $NEW_VERSION in $README_FILE."

# Validate the nuget.yml file exists
if [ ! -f "$NUGET_YML_FILE" ]; then
    echo "Error: File '$NUGET_YML_FILE' not found."
    exit 1
fi
# Replace the version in the VERSION environment variable
sed -i -E "s/^  VERSION: \"[0-9]+\.[0-9]+\.[0-9]+\"/  VERSION: \"${NEW_VERSION}\"/" "$NUGET_YML_FILE"
echo "VERSION updated to $NEW_VERSION in $NUGET_YML_FILE."

# Validate the prerelease.yml file exists
if [ ! -f "$PRERELEASE_YML_FILE" ]; then
    echo "Error: File '$PRERELEASE_YML_FILE' not found."
    exit 1
fi
# Replace the version in the VERSION environment variable
sed -i -E "s/^  VERSION: \"[0-9]+\.[0-9]+\.[0-9]+\"/  VERSION: \"${NEW_VERSION}\"/" "$PRERELEASE_YML_FILE"
echo "VERSION updated to $NEW_VERSION in $PRERELEASE_YML_FILE."

# Validate the prerelease_gitea.yml file exists
if [ ! -f "$PRERELEASE_GITEA_YML_FILE" ]; then
    echo "Error: File '$PRERELEASE_GITEA_YML_FILE' not found."
    exit 1
fi
# Replace the version in the VERSION environment variable
sed -i -E "s/^  VERSION: \"[0-9]+\.[0-9]+\.[0-9]+\"/  VERSION: \"${NEW_VERSION}\"/" "$PRERELEASE_GITEA_YML_FILE"
echo "VERSION updated to $NEW_VERSION in $PRERELEASE_GITEA_YML_FILE."

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

# Validate the Nfbookmark csproj file exists
if [ ! -f "$NFBOOKMARK_CSPROJ_FILE" ]; then
    echo "Error: File '$NFBOOKMARK_CSPROJ_FILE' not found."
    exit 1
fi
# Replace the version inside the <Version> tag
sed -i -E "s|<Version>[0-9]+\.[0-9]+\.[0-9]+</Version>|<Version>${NEW_VERSION}</Version>|" "$NFBOOKMARK_CSPROJ_FILE"
echo "Version updated to $NEW_VERSION in $NFBOOKMARK_CSPROJ_FILE."

# Validate the bookmark-dlp csproj file exists
if [ ! -f "$BOOKMARK_DLP_CSPROJ_FILE" ]; then
    echo "Error: File '$BOOKMARK_DLP_CSPROJ_FILE' not found."
    exit 1
fi
# Replace the version inside the <Version> tag
sed -i -E "s|<Version>[0-9]+\.[0-9]+\.[0-9]+</Version>|<Version>${NEW_VERSION}</Version>|" "$BOOKMARK_DLP_CSPROJ_FILE"
echo "Version updated to $NEW_VERSION in $BOOKMARK_DLP_CSPROJ_FILE."

# Validate the configuration file exists
if [ ! -f "$PUPNET_CONF_FILE" ]; then
    echo "Error: File '$PUPNET_CONF_FILE' not found."
    exit 1
fi
# Replace the AppVersionRelease value
sed -i -E "s/^AppVersionRelease = [0-9]+\.[0-9]+\.[0-9]+/AppVersionRelease = ${NEW_VERSION}/" "$PUPNET_CONF_FILE"
echo "AppVersionRelease updated to $NEW_VERSION in $PUPNET_CONF_FILE."

# Validate the configuration file exists
if [ ! -f "$SPEC_FILE" ]; then
    echo "Error: File '$SPEC_FILE' not found."
    exit 1
fi
# Replace the Version value
sed -i -E "s/^Version:        [0-9]+\.[0-9]+\.[0-9]+/Version:        ${NEW_VERSION}/" "$SPEC_FILE"
echo "Version updated to $NEW_VERSION in $SPEC_FILE."