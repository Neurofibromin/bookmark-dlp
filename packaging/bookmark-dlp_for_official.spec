%global debug_package %{nil}
Name:           bookmark-dlp
Version:        0.4.1
Release:        %autorelease
Summary:        Utility program for downloading bookmarked YouTube links using yt-dlp

License:        GPL-3.0-only
URL:            https://github.com/Neurofibromin/bookmark-dlp
Source0:         https://github.com/Neurofibromin/bookmark-dlp/archive/refs/tags/%{version}.tar.gz
Source1: https://www.nuget.org/api/v2/package/Avalonia/11.0.0#Avalonia.11.0.0.nupkg
Source2: https://www.nuget.org/api/v2/package/Avalonia/11.2.3#Avalonia.11.2.3.nupkg
Source3: https://www.nuget.org/api/v2/package/Avalonia.Angle.Windows.Natives/2.1.22045.20230930#Avalonia.Angle.Windows.Natives.2.1.22045.20230930.nupkg
Source4: https://www.nuget.org/api/v2/package/Avalonia.BuildServices/0.0.29#Avalonia.BuildServices.0.0.29.nupkg
Source5: https://www.nuget.org/api/v2/package/Avalonia.Controls.ColorPicker/11.2.3#Avalonia.Controls.ColorPicker.11.2.3.nupkg
Source6: https://www.nuget.org/api/v2/package/Avalonia.Controls.DataGrid/11.2.3#Avalonia.Controls.DataGrid.11.2.3.nupkg
Source7: https://www.nuget.org/api/v2/package/Avalonia.Controls.TreeDataGrid/11.1.0#Avalonia.Controls.TreeDataGrid.11.1.0.nupkg
Source8: https://www.nuget.org/api/v2/package/Avalonia.Desktop/11.2.3#Avalonia.Desktop.11.2.3.nupkg
Source9: https://www.nuget.org/api/v2/package/Avalonia.Diagnostics/11.2.3#Avalonia.Diagnostics.11.2.3.nupkg
Source10: https://www.nuget.org/api/v2/package/Avalonia.Fonts.Inter/11.2.3#Avalonia.Fonts.Inter.11.2.3.nupkg
Source11: https://www.nuget.org/api/v2/package/Avalonia.FreeDesktop/11.2.3#Avalonia.FreeDesktop.11.2.3.nupkg
Source12: https://www.nuget.org/api/v2/package/Avalonia.Native/11.2.3#Avalonia.Native.11.2.3.nupkg
Source13: https://www.nuget.org/api/v2/package/Avalonia.Remote.Protocol/11.2.3#Avalonia.Remote.Protocol.11.2.3.nupkg
Source14: https://www.nuget.org/api/v2/package/Avalonia.Skia/11.2.3#Avalonia.Skia.11.2.3.nupkg
Source15: https://www.nuget.org/api/v2/package/Avalonia.Themes.Fluent/11.2.3#Avalonia.Themes.Fluent.11.2.3.nupkg
Source16: https://www.nuget.org/api/v2/package/Avalonia.Themes.Simple/11.2.3#Avalonia.Themes.Simple.11.2.3.nupkg
Source17: https://www.nuget.org/api/v2/package/Avalonia.Win32/11.2.3#Avalonia.Win32.11.2.3.nupkg
Source18: https://www.nuget.org/api/v2/package/Avalonia.X11/11.2.3#Avalonia.X11.11.2.3.nupkg
Source19: https://www.nuget.org/api/v2/package/CommandLineParser/2.9.1#CommandLineParser.2.9.1.nupkg
Source20: https://www.nuget.org/api/v2/package/CommunityToolkit.Mvvm/8.4.0#CommunityToolkit.Mvvm.8.4.0.nupkg
Source21: https://www.nuget.org/api/v2/package/coverlet.collector/6.0.4#coverlet.collector.6.0.4.nupkg
Source22: https://www.nuget.org/api/v2/package/HarfBuzzSharp/7.3.0.3#HarfBuzzSharp.7.3.0.3.nupkg
Source23: https://www.nuget.org/api/v2/package/HarfBuzzSharp.NativeAssets.Linux/7.3.0.3#HarfBuzzSharp.NativeAssets.Linux.7.3.0.3.nupkg
Source24: https://www.nuget.org/api/v2/package/HarfBuzzSharp.NativeAssets.macOS/7.3.0.3#HarfBuzzSharp.NativeAssets.macOS.7.3.0.3.nupkg
Source25: https://www.nuget.org/api/v2/package/HarfBuzzSharp.NativeAssets.WebAssembly/7.3.0.3#HarfBuzzSharp.NativeAssets.WebAssembly.7.3.0.3.nupkg
Source26: https://www.nuget.org/api/v2/package/HarfBuzzSharp.NativeAssets.Win32/7.3.0.3#HarfBuzzSharp.NativeAssets.Win32.7.3.0.3.nupkg
Source27: https://www.nuget.org/api/v2/package/MicroCom.Runtime/0.11.0#MicroCom.Runtime.0.11.0.nupkg
Source28: https://www.nuget.org/api/v2/package/Microsoft.AspNetCore.App.Ref/3.1.10#Microsoft.AspNetCore.App.Ref.3.1.10.nupkg
Source29: https://www.nuget.org/api/v2/package/Microsoft.AspNetCore.App.Ref/5.0.0#Microsoft.AspNetCore.App.Ref.5.0.0.nupkg
Source30: https://www.nuget.org/api/v2/package/Microsoft.AspNetCore.App.Ref/6.0.36#Microsoft.AspNetCore.App.Ref.6.0.36.nupkg
Source31: https://www.nuget.org/api/v2/package/Microsoft.AspNetCore.App.Ref/7.0.20#Microsoft.AspNetCore.App.Ref.7.0.20.nupkg
Source32: https://www.nuget.org/api/v2/package/Microsoft.AspNetCore.App.Runtime.linux-x64/9.0.0#Microsoft.AspNetCore.App.Runtime.linux-x64.9.0.0.nupkg
Source33: https://www.nuget.org/api/v2/package/Microsoft.Bcl.AsyncInterfaces/9.0.1#Microsoft.Bcl.AsyncInterfaces.9.0.1.nupkg
Source34: https://www.nuget.org/api/v2/package/Microsoft.CodeCoverage/17.12.0#Microsoft.CodeCoverage.17.12.0.nupkg
Source35: https://www.nuget.org/api/v2/package/Microsoft.Data.Sqlite/9.0.1#Microsoft.Data.Sqlite.9.0.1.nupkg
Source36: https://www.nuget.org/api/v2/package/Microsoft.Data.Sqlite.Core/9.0.1#Microsoft.Data.Sqlite.Core.9.0.1.nupkg
Source37: https://www.nuget.org/api/v2/package/Microsoft.NET.ILLink.Tasks/9.0.0#Microsoft.NET.ILLink.Tasks.9.0.0.nupkg
Source38: https://www.nuget.org/api/v2/package/Microsoft.NET.Test.Sdk/17.12.0#Microsoft.NET.Test.Sdk.17.12.0.nupkg
Source39: https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Host.linux-x64/9.0.0#Microsoft.NETCore.App.Host.linux-x64.9.0.0.nupkg
Source40: https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Ref/3.1.0#Microsoft.NETCore.App.Ref.3.1.0.nupkg
Source41: https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Ref/5.0.0#Microsoft.NETCore.App.Ref.5.0.0.nupkg
Source42: https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Ref/6.0.36#Microsoft.NETCore.App.Ref.6.0.36.nupkg
Source43: https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Ref/7.0.20#Microsoft.NETCore.App.Ref.7.0.20.nupkg
Source44: https://www.nuget.org/api/v2/package/Microsoft.NETCore.App.Runtime.linux-x64/9.0.0#Microsoft.NETCore.App.Runtime.linux-x64.9.0.0.nupkg
Source45: https://www.nuget.org/api/v2/package/Microsoft.NETCore.Platforms/1.1.0#Microsoft.NETCore.Platforms.1.1.0.nupkg
Source46: https://www.nuget.org/api/v2/package/Microsoft.NETFramework.ReferenceAssemblies/1.0.3#Microsoft.NETFramework.ReferenceAssemblies.1.0.3.nupkg
Source47: https://www.nuget.org/api/v2/package/Microsoft.NETFramework.ReferenceAssemblies.net481/1.0.3#Microsoft.NETFramework.ReferenceAssemblies.net481.1.0.3.nupkg
Source48: https://www.nuget.org/api/v2/package/Microsoft.TestPlatform.ObjectModel/17.12.0#Microsoft.TestPlatform.ObjectModel.17.12.0.nupkg
Source49: https://www.nuget.org/api/v2/package/Microsoft.TestPlatform.TestHost/17.12.0#Microsoft.TestPlatform.TestHost.17.12.0.nupkg
Source50: https://www.nuget.org/api/v2/package/NETStandard.Library/2.0.3#NETStandard.Library.2.0.3.nupkg
Source51: https://www.nuget.org/api/v2/package/Newtonsoft.Json/13.0.1#Newtonsoft.Json.13.0.1.nupkg
Source52: https://www.nuget.org/api/v2/package/SkiaSharp/2.88.9#SkiaSharp.2.88.9.nupkg
Source53: https://www.nuget.org/api/v2/package/SkiaSharp.NativeAssets.Linux/2.88.9#SkiaSharp.NativeAssets.Linux.2.88.9.nupkg
Source54: https://www.nuget.org/api/v2/package/SkiaSharp.NativeAssets.macOS/2.88.9#SkiaSharp.NativeAssets.macOS.2.88.9.nupkg
Source55: https://www.nuget.org/api/v2/package/SkiaSharp.NativeAssets.WebAssembly/2.88.9#SkiaSharp.NativeAssets.WebAssembly.2.88.9.nupkg
Source56: https://www.nuget.org/api/v2/package/SkiaSharp.NativeAssets.Win32/2.88.9#SkiaSharp.NativeAssets.Win32.2.88.9.nupkg
Source57: https://www.nuget.org/api/v2/package/SQLitePCLRaw.bundle_e_sqlite3/2.1.10#SQLitePCLRaw.bundle_e_sqlite3.2.1.10.nupkg
Source58: https://www.nuget.org/api/v2/package/SQLitePCLRaw.core/2.1.10#SQLitePCLRaw.core.2.1.10.nupkg
Source59: https://www.nuget.org/api/v2/package/SQLitePCLRaw.lib.e_sqlite3/2.1.10#SQLitePCLRaw.lib.e_sqlite3.2.1.10.nupkg
Source60: https://www.nuget.org/api/v2/package/SQLitePCLRaw.provider.dynamic_cdecl/2.1.10#SQLitePCLRaw.provider.dynamic_cdecl.2.1.10.nupkg
Source61: https://www.nuget.org/api/v2/package/SQLitePCLRaw.provider.e_sqlite3/2.1.10#SQLitePCLRaw.provider.e_sqlite3.2.1.10.nupkg
Source62: https://www.nuget.org/api/v2/package/System.Buffers/4.5.1#System.Buffers.4.5.1.nupkg
Source63: https://www.nuget.org/api/v2/package/System.IO.Pipelines/8.0.0#System.IO.Pipelines.8.0.0.nupkg
Source64: https://www.nuget.org/api/v2/package/System.IO.Pipelines/9.0.1#System.IO.Pipelines.9.0.1.nupkg
Source65: https://www.nuget.org/api/v2/package/System.Memory/4.5.3#System.Memory.4.5.3.nupkg
Source66: https://www.nuget.org/api/v2/package/System.Memory/4.5.5#System.Memory.4.5.5.nupkg
Source67: https://www.nuget.org/api/v2/package/System.Numerics.Vectors/4.4.0#System.Numerics.Vectors.4.4.0.nupkg
Source68: https://www.nuget.org/api/v2/package/System.Numerics.Vectors/4.5.0#System.Numerics.Vectors.4.5.0.nupkg
Source69: https://www.nuget.org/api/v2/package/System.Reactive/5.0.0#System.Reactive.5.0.0.nupkg
Source70: https://www.nuget.org/api/v2/package/System.Reflection.Metadata/1.6.0#System.Reflection.Metadata.1.6.0.nupkg
Source71: https://www.nuget.org/api/v2/package/System.Runtime.CompilerServices.Unsafe/4.5.3#System.Runtime.CompilerServices.Unsafe.4.5.3.nupkg
Source72: https://www.nuget.org/api/v2/package/System.Runtime.CompilerServices.Unsafe/6.0.0#System.Runtime.CompilerServices.Unsafe.6.0.0.nupkg
Source73: https://www.nuget.org/api/v2/package/System.Text.Encodings.Web/9.0.1#System.Text.Encodings.Web.9.0.1.nupkg
Source74: https://www.nuget.org/api/v2/package/System.Text.Json/9.0.1#System.Text.Json.9.0.1.nupkg
Source75: https://www.nuget.org/api/v2/package/System.Threading.Tasks.Extensions/4.5.4#System.Threading.Tasks.Extensions.4.5.4.nupkg
Source76: https://www.nuget.org/api/v2/package/System.ValueTuple/4.5.0#System.ValueTuple.4.5.0.nupkg
Source77: https://www.nuget.org/api/v2/package/Tmds.DBus.Protocol/0.20.0#Tmds.DBus.Protocol.0.20.0.nupkg
Source78: https://www.nuget.org/api/v2/package/xunit/2.9.3#xunit.2.9.3.nupkg
Source79: https://www.nuget.org/api/v2/package/xunit.abstractions/2.0.3#xunit.abstractions.2.0.3.nupkg
Source80: https://www.nuget.org/api/v2/package/xunit.analyzers/1.18.0#xunit.analyzers.1.18.0.nupkg
Source81: https://www.nuget.org/api/v2/package/xunit.assert/2.9.3#xunit.assert.2.9.3.nupkg
Source82: https://www.nuget.org/api/v2/package/xunit.core/2.9.3#xunit.core.2.9.3.nupkg
Source83: https://www.nuget.org/api/v2/package/xunit.extensibility.core/2.9.3#xunit.extensibility.core.2.9.3.nupkg
Source84: https://www.nuget.org/api/v2/package/xunit.extensibility.execution/2.9.3#xunit.extensibility.execution.2.9.3.nupkg
Source85: https://www.nuget.org/api/v2/package/xunit.runner.visualstudio/3.0.1#xunit.runner.visualstudio.3.0.1.nupkg

# Source0: ftp://ftp.example.com/pub/foo/%{name}-%{version}.tar.gz
# Source1: ftp://ftp.example.com/pub/foo/%{name}-%{version}.tar.gz.asc
# Source2: https://www.example.com/gpgkey-0123456789ABCDEF0123456789ABCDEF.gpg

BuildRequires:  gcc
BuildRequires:  glibc
BuildRequires:  git
BuildRequires:  dotnet-sdk-9.0
BuildRequires:  wget
BuildRequires:  unzip
# BuildRequires:  gnupg2
BuildRequires:  desktop-file-utils
Requires:       yt-dlp
Requires:       glibc
Requires:       libstdc++
Requires:       libgcc
%description
Small utility program for downloading bookmarked YouTube links using yt-dlp.

%prep
echo currentdir: $PWD
echo sourcedir: %{_sourcedir}
echo builddir: %{_builddir}
cp %{_sourcedir}/%{version}.tar.gz  %{name}-%{version}.tar.gz 
tar -xf %{name}-%{version}.tar.gz #creates %{_builddir}/%{name}-%{version} directory
# %{gpgverify} --keyring='%{SOURCE2}' --signature='%{SOURCE1}' --data='%{SOURCE0}'
cd %{_builddir}/%{name}-%{version}
export NUGET_PACKAGES=%{_sourcedir}/nuget
export DOTNET_NOLOGO=true
export DOTNET_CLI_TELEMETRY_OPTOUT=true
dotnet restore --locked-mode --source %{_sourcedir} bookmark-dlp.sln

%build
cd %{_builddir}/%{name}-%{version}
export MSBUILDDISABLENODEREUSE=1
dotnet publish bookmark-dlp/bookmark-dlp.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --framework net9.0 \
    -o %{_builddir}/%{name}-%{version}/publish \
    --verbosity quiet

%check
cd %{_builddir}/%{name}-%{version}
dotnet test ./Tests/bookmark-dlp.Tests/ \
    --no-restore \
    --framework net9.0 \
    --verbosity quiet

%install
%define __strip /bin/true
install -d %{buildroot}%{_bindir}
install -d %{buildroot}%{_libdir}/%{name}
cp -r %{_builddir}/%{name}-%{version}/publish/* %{buildroot}%{_libdir}/%{name}/

# Install icons and create directories automatically
install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.16.png \
    %{buildroot}%{_datadir}/icons/hicolor/16x16/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.24.png \
    %{buildroot}%{_datadir}/icons/hicolor/24x24/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.32.png \
    %{buildroot}%{_datadir}/icons/hicolor/32x32/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.48.png \
    %{buildroot}%{_datadir}/icons/hicolor/48x48/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.64.png \
    %{buildroot}%{_datadir}/icons/hicolor/64x64/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.96.png \
    %{buildroot}%{_datadir}/icons/hicolor/96x96/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.128.png \
    %{buildroot}%{_datadir}/icons/hicolor/128x128/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.256.png \
    %{buildroot}%{_datadir}/icons/hicolor/256x256/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.512.png \
    %{buildroot}%{_datadir}/icons/hicolor/512x512/apps/bookmark-dlp.png

install -Dm0644 %{_builddir}/%{name}-%{version}/bookmark-dlp/Assets/bookmark-dlp.svg \
    %{buildroot}%{_datadir}/icons/hicolor/scalable/apps/bookmark-dlp.svg

ln -s %{_libdir}/%{name}/%{name} %{buildroot}%{_bindir}/%{name}
install -d %{buildroot}%{_datadir}/applications
install -d %{buildroot}%{_mandir}/man1
install -m 644 %{_builddir}/%{name}-%{version}/bookmark-dlp.1 %{buildroot}%{_mandir}/man1/
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
%{_datadir}/icons/hicolor/16x16/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/24x24/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/32x32/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/48x48/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/64x64/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/96x96/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/128x128/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/256x256/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/512x512/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/1024x1024/apps/bookmark-dlp.png
%{_datadir}/icons/hicolor/scalable/apps/bookmark-dlp.svg

%changelog
%autochangelog