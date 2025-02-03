{
  lib,
  stdenvNoCC,
  buildDotnetModule,
  fetchFromGitHub,
  dotnetCorePackages,
  makeDesktopItem,
  copyDesktopItems,
  makeWrapper,
  alsa-lib,
  lttng-ust,
  numactl,
  xorg,
  udev,
  yt-dlp,
  nativeWayland ? false,
}:

#TODO: remove unnecessary deps, add icon

buildDotnetModule rec {
  pname = "bookmark-dlp";
  version = "0.4.1";
  src = ./bookmark-dlp;
 # src = fetchFromGitHub {
 #   owner = "Neurofibromin";
 #   repo = "bookmark-dlp";
 #   rev = "0.4.1";
 #   sha256 = "ykT9X43uUxKdy03IwDmfAIxyPAXxRSJvBlK4SzuBQUk=";
 # };

  packNupkg = true;
  projectFile = "./bookmark-dlp/bookmark-dlp.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.runtime_9_0;

  nativeBuildInputs = [
    copyDesktopItems
    makeWrapper
  ];

  runtimeDeps = [
    yt-dlp
    alsa-lib
    lttng-ust
    numactl
    xorg.libXi
    udev
  ];

  executables = [ "bookmark-dlp" ];

  meta = {
    description = "Download bookmarked youtube links using yt-dlp";
    homepage = "https://github.com/Neurofibromin/bookmark-dlp";
    license = lib.licenses.gpl3Only;
    maintainers = with lib.maintainers; [ neurofibromin ];
    platforms = [ "x86_64-linux" ];
    mainProgram = "bookmark-dlp";
  };
}

{
  neurofibromin = {
    name = "Neurofibromin";
    github = "Neurofibromin";
    githubId = 125222560;
    keys = [{
      fingerprint = "9F9B FE94 618A D266 67BD 2821 4F67 1AFA D8D4 428B";
    }];
  };
}
