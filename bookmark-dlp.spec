Name:           bookmark-dlp
Version:        0.4.1
Release:        %autorelease
Summary:        Utility program for downloading bookmarked YouTube links using yt-dlp

License:        GPL-3.0-only
URL:            https://github.com/Neurofibromin/bookmark-dlp
Source:         https://github.com/Neurofibromin/bookmark-dlp/archive/refs/tags/%{version}.tar.gz

Source0: ftp://ftp.example.com/pub/foo/%{name}-%{version}.tar.gz
Source1: ftp://ftp.example.com/pub/foo/%{name}-%{version}.tar.gz.asc
Source2: https://www.example.com/gpgkey-0123456789ABCDEF0123456789ABCDEF.gpg

BuildRequires:  gcc
BuildRequires:  glibc
BuildRequires:  git
BuildRequires:  dotnet-sdk-9.0
BuildRequires:  gnupg2
BuildRequires:  desktop-file-utils
Requires:       gcc-libs
Requires:       glibc
Requires:       yt-dlp
%description
Small utility program for downloading bookmarked YouTube links using yt-dlp.

%prep
%autosetup -n %{name}-%{version}
%{gpgverify} --keyring='%{SOURCE2}' --signature='%{SOURCE1}' --data='%{SOURCE0}'
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
    -o %{_builddir}/%{name}-%{version} \
    --verbosity quiet

%check
dotnet test ./Tests/bookmark-dlp.Tests/ \
    --no-restore \
    --framework net8.0 \
    --verbosity quiet

%install
install -d %{_builddir}/usr/bin
install -d %{_builddir}/usr/lib/%{name}
cp -r bookmark-dlp/bookmark-dlp/bin/Release/net8.0/linux-x64/publish/* %{_builddir}/usr/lib/%{name}/
ln -s /usr/lib/%{name}/%{name} %{_builddir}/usr/bin/%{name}
install -d %{_builddir}/usr/share/applications
# install -m 644 bookmark-dlp.desktop %{_builddir}/usr/share/applications/
desktop-file-install                                    \
--dir=%{buildroot}%{_datadir}/applications              \
%{SOURCE3}

%files
%license LICENSE
%doc README.md
%{_mandir}/man1/bookmark-dlp.1
%{_bindir}/%{name}
/usr/share/applications/bookmark-dlp.desktop

%changelog
%autochangelog