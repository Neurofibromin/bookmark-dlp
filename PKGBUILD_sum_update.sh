#!/bin/bash

PKGBUILD_FILE=./PKGBUILD
VERSION=0.4.1
SOURCE="https://github.com/Neurofibromin/bookmark-dlp/archive/refs/tags/${VERSION}.tar.gz"
TAR_FILE=./"${VERSION}.tar.gz"

if ! command -v b2sum 2>&1 >/dev/null
then
    echo "b2sum not installed"
    exit 1      
fi
if ! command -v wget 2>&1 >/dev/null
then
    echo "wget not installed"
    exit 1      
fi

# Validate the PKGBUILD file exists
if [ ! -f "$PKGBUILD_FILE" ]; then
    echo "Error: File '$PKGBUILD_FILE' not found."
    exit 1
fi

wget -O "$TAR_FILE" "$SOURCE"
# get b2sum of tar.gz:
# e.g. b2sum 0.4.1.tar.gz 
# 5d6d98068b0a330f09144e916bcc99d2aac92ae83efe0fa19f2ea5065f299d8140242771c5d7755898ac720b86fbc42e8407eccbda811870671460b568f695fb  0.4.1.tar.gz
NEW_HASH=$(b2sum "$TAR_FILE" | awk '{print $1}')
rm $TAR_FILE

# Replace old hash in the PKGBUILD file
# e.g. old hash in PKGBUILD:
# b2sums=('e8407eccbda811870671460b568f695fb5d6d98068b0a330f09144e916bcc99d2aac92ae83efe0fa19f2ea5065f299d8140242771c5d7755898ac720b86fbc42')
sed -i -E "s#(b2sums=\()'.*'#\1'${NEW_HASH}'#" "$PKGBUILD_FILE"
echo "Hash updated to $NEW_HASH in $PKGBUILD_FILE."