using CommandLine;
using NfLogger;

/// <summary>
/// Parsing of command line options, using an external library.
/// </summary>
class CommandLineOptions()
{
    [Option('s', "sourcehtml", Required = false, HelpText = "Input html file containing bookmarks.")]
    public string? HtmlFileLocation { get; set; }

    [Option('o', "outputfolder", Required = false, HelpText = "Output directory for video files and folder structure. Default: $PWD")]
    public string? Outputfolder { get; set; }

    [Option('v', "verbose", Required = false, Default = false, HelpText = "Prints all messages to standard output.")]
    public bool Verbose { get; set; }

    [Option('i', "interactive", Required = false, Default = false, HelpText = "Prints all messages to standard output. Ignores most other flags.")]
    public bool Interactive { get; set; }

    [Option('p', "downloadPlaylists", Required = false, Default = false, HelpText = "Allows download of playlists.")]
    public bool DownloadPlaylists { get; set; }

    [Option('r', "downloadShorts", Required = false, Default = false, HelpText = "Allows download of \"youtube shorts\" videos.")]
    public bool DownloadShorts { get; set; }

    [Option('c', "downloadChannels", Required = false, Default = false, HelpText = "Allows download of all videos of channels.")]
    public bool DownloadChannels { get; set; }

    [Option('u', "concurrent_downloads", Required = false, Default = false, HelpText = "Allows concurrent download of videos.")]
    public bool Concurrent_downloads { get; set; }

    [Option('e', "cookies_autoextract", Required = false, Default = false, HelpText = "Allows automatic cookie extraction from browser by yt-dlp.")]
    public bool Cookies_autoextract { get; set; }

    [Option('l', "yt_dlp_binary_path", Required = false, HelpText = "Set yt-dlp binary executable path. If not set some default locations are probed. See docs.")]
    public string? Yt_dlp_binary_path { get; set; }

    [Option('b', "browser", Required = false, HelpText = "Browser autoimport setting. Designate one browser eg. \"chrome\" or use -b 1 to autochoose. Ignored when --interactive.")]
    public string? BrowserChosenByFlag { get; set; }

    /*Done automatically by library
     * [Option('h', "help", Required = false, Default = false, HelpText = "Displays help and usage information.")]
    public bool Help { get; set; }*/


}