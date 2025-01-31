%global debug_package %{nil}
Name:           bookmark-dlp
Version:        0.4.1
Release:        %autorelease
Summary:        Utility program for downloading bookmarked YouTube links using yt-dlp

License:        GPL-3.0-only
URL:            https://github.com/Neurofibromin/bookmark-dlp
Source:         https://github.com/Neurofibromin/bookmark-dlp/archive/refs/tags/%{version}.tar.gz

# Source0: ftp://ftp.example.com/pub/foo/%{name}-%{version}.tar.gz
# Source1: ftp://ftp.example.com/pub/foo/%{name}-%{version}.tar.gz.asc
# Source2: https://www.example.com/gpgkey-0123456789ABCDEF0123456789ABCDEF.gpg

BuildRequires:  gcc
BuildRequires:  glibc
BuildRequires:  git
BuildRequires:  dotnet-sdk-9.0
# BuildRequires:  gnupg2
BuildRequires:  desktop-file-utils
Requires:       yt-dlp
Requires:       glibc
Requires:       libstdc++
Requires:       libgcc
%description
Small utility program for downloading bookmarked YouTube links using yt-dlp.

%prep
%autosetup -n %{name}-%{version}
# %{gpgverify} --keyring='%{SOURCE2}' --signature='%{SOURCE1}' --data='%{SOURCE0}'
export NUGET_PACKAGES=$PWD/nuget
export DOTNET_NOLOGO=true
export DOTNET_CLI_TELEMETRY_OPTOUT=true
dotnet restore --locked-mode bookmark-dlp.sln

%build
export MSBUILDDISABLENODEREUSE=1
dotnet publish bookmark-dlp/bookmark-dlp.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --framework net9.0 \
    -o %{_builddir}/%{name}-%{version}/publish \
    --verbosity quiet

%check
dotnet test ./Tests/bookmark-dlp.Tests/ \
    --no-restore \
    --framework net9.0 \
    --verbosity quiet

%install
%define __strip /bin/true
install -d %{buildroot}%{_bindir}
install -d %{buildroot}%{_libdir}/%{name}
cp -r %{_builddir}/%{name}-%{version}/publish/* %{buildroot}%{_libdir}/%{name}/
ln -s %{_libdir}/%{name}/%{name} %{buildroot}%{_bindir}/%{name}
install -d %{buildroot}%{_datadir}/applications
install -d %{buildroot}%{_mandir}/man1
install -m 644 bookmark-dlp.1 %{buildroot}%{_mandir}/man1/
# install -m 644 bookmark-dlp.desktop %{_builddir}/usr/share/applications/
desktop-file-install                                    \
--dir=%{buildroot}%{_datadir}/applications              \
bookmark-dlp.desktop
rm -f %{buildroot}%{_libdir}/%{name}/*.pdb
rm -f %{buildroot}%{_libdir}/%{name}/*.xml

%files
%license LICENSE
%doc README.md
%{_mandir}/man1/bookmark-dlp.1.gz
%{_bindir}/%{name}
%{_libdir}/%{name}/bookmark-dlp
%{_datadir}/applications/bookmark-dlp.desktop

%changelog
%autochangelog