{ lib, stdenv, fetchFromGitHub, dotnetCorePackages, makeWrapper }:

stdenv.mkDerivation rec {
  pname = "bookmark-dlp";
  version = "0.4.1";

  src = fetchFromGitHub {
    owner = "Neurofibromin";
    repo = "bookmark-dlp";
    rev = "0.4.1";
    sha256 = "0123456789123456789";
  };

  nativeBuildInputs = [ dotnetCorePackages.sdk_9_0 makeWrapper ];

  buildPhase = ''
    runHook preBuild
    dotnet publish bookmark-dlp/bookmark-dlp.csproj -c Release -o $out/bin --self-contained false
    runHook postBuild
  '';

  installPhase = ''
    runHook preInstall
    mkdir -p $out/bin
    cp -r $out/bin/* $out/bin/
    chmod +x $out/bin/bookmark-dlp
    wrapProgram $out/bin/bookmark-dlp --prefix PATH : ${lib.makeBinPath [ dotnetCorePackages.runtime_9_0 ]}
    runHook postInstall
  '';

  meta = with lib; {
    description = "Download bookmarked youtube links using yt-dlp";
    homepage = "https://github.com/Neurofibromin/bookmark-dlp";
    license = licenses.gpl3Only;
    maintainers = with maintainers; [ Neurofibromin ];
    platforms = platforms.linux;
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
