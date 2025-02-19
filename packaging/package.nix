{
  lib,
  stdenvNoCC,
  buildDotnetModule,
  fetchFromGitHub,
  dotnetCorePackages,
  makeDesktopItem,
  copyDesktopItems,
  makeWrapper,
  lttng-ust,
  numactl,
  xorg,
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
  nugetDeps = ./deps.json; #stored in the nixpkgs repo pkgs/by-name/bo/bookmark-dlp/deps.json, generated with nuget-to-json

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.runtime_9_0;

  nativeBuildInputs = [
    copyDesktopItems
    makeWrapper
  ];

  runtimeDeps = [
    yt-dlp
    lttng-ust
    numactl
    xorg.libXi
  ];

  executables = [ "bookmark-dlp" ];
  
  preBuild = ''
      patch bookmark-dlp.sln < nix-patch.patch
  '';
  
  installPhase = ''
      # Create the icon directory in the $out/share/icons/hicolor directory
      mkdir -p $out/share/icons/hicolor/{16x16,24x24,32x32,48x48,64x64,96x96,128x128,256x256,512x512,1024x1024,scalable}/apps
  
      # Copy the icons to the appropriate directories
      cp bookmark-dlp/Assets/bookmark-dlp.16.png $out/share/icons/hicolor/16x16/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.24.png $out/share/icons/hicolor/24x24/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.32.png $out/share/icons/hicolor/32x32/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.48.png $out/share/icons/hicolor/48x48/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.64.png $out/share/icons/hicolor/64x64/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.96.png $out/share/icons/hicolor/96x96/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.128.png $out/share/icons/hicolor/128x128/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.256.png $out/share/icons/hicolor/256x256/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.512.png $out/share/icons/hicolor/512x512/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.1024.png $out/share/icons/hicolor/1024x1024/apps/bookmark-dlp.png
      cp bookmark-dlp/Assets/bookmark-dlp.svg $out/share/icons/hicolor/scalable/apps/bookmark-dlp.svg  # For the SVG icon
  
      # Install the desktop entry
      install -Dm 0755 bookmark-dlp.desktop $out/share/applications/bookmark-dlp.desktop
    '';

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
