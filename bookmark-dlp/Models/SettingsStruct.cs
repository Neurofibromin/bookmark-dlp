using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using NfLogger;

namespace bookmark_dlp.Models;


/// <summary>
///     Struct of all settings value types used in the program.
/// </summary>
public partial class SettingsStruct : ObservableObject
{
    [ObservableProperty] public bool _canChangeSettings;
    [ObservableProperty] public bool _concurrentDownloads;
    [ObservableProperty] public bool _cookiesAutoextract;
    [ObservableProperty] public bool _downloadChannels;
    [ObservableProperty] public bool _downloadPlaylists;
    [ObservableProperty] public bool _downloadShorts;
    [ObservableProperty] public string? _manualImportFileLocation;
    [ObservableProperty] public bool _manualImportUsed;
    [ObservableProperty] public string? _outputFolder;
    [ObservableProperty] public string? _selectedYtDlpConfigFile;
    [ObservableProperty] public string? _ytDlpBinaryPath;
    [ObservableProperty] public ObservableCollection<string> _ytDlpConfigFiles;
    [ObservableProperty] public bool _ytDlpExecutableNotFound;
    [ObservableProperty] public AppTheme _selectedTheme;

    public SettingsStruct(string? cmanualimportfilelocation,
        bool cmanualImportUsed,
        string? coutputfolder,
        bool cytdlp_executable_not_found,
        bool cdownloadPlaylists,
        bool cdownloadShorts,
        bool cdownloadChannels,
        bool cconcurrent_downloads,
        bool ccookies_autoextract,
        string? cyt_dlp_binary_path,
        bool ccanChangeSettings,
        ObservableCollection<string> cytDlpConfigFiles,
        string? cselectedYtDlpConfigFile,
        AppTheme cselectedTheme)
    {
        _manualImportFileLocation = cmanualimportfilelocation;
        _manualImportUsed = cmanualImportUsed;
        _outputFolder = coutputfolder;
        _ytDlpExecutableNotFound = cytdlp_executable_not_found;
        _downloadPlaylists = cdownloadPlaylists;
        _downloadShorts = cdownloadShorts;
        _downloadChannels = cdownloadChannels;
        _concurrentDownloads = cconcurrent_downloads;
        _cookiesAutoextract = ccookies_autoextract;
        _ytDlpBinaryPath = cyt_dlp_binary_path;
        _canChangeSettings = ccanChangeSettings;
        _ytDlpConfigFiles = cytDlpConfigFiles;
        _selectedYtDlpConfigFile = cselectedYtDlpConfigFile;
        _selectedTheme = cselectedTheme;
    }

    /// <summary>
    ///     Parameterless ctor needed by JsonSerializer.Deserialize
    /// </summary>
    public SettingsStruct()
    {
        _manualImportFileLocation = null;
        _manualImportUsed = false;
        _outputFolder = null;
        _ytDlpExecutableNotFound = true;
        _downloadPlaylists = false;
        _downloadShorts = false;
        _downloadChannels = false;
        _concurrentDownloads = false;
        _cookiesAutoextract = false;
        _ytDlpBinaryPath = null;
        _canChangeSettings = true;
        _ytDlpConfigFiles = new ObservableCollection<string>();
        _selectedYtDlpConfigFile = null;
        _selectedTheme = AppTheme.Fluent;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("SettingsStruct:");
        sb.AppendLine($"  Manual Import File Location: {ManualImportFileLocation ?? "N/A"}");
        sb.AppendLine($"  Manual Import Used: {ManualImportUsed}");
        sb.AppendLine($"  Output Folder: {OutputFolder ?? "N/A"}");
        sb.AppendLine($"  Yt-dlp Executable Not Found: {YtDlpExecutableNotFound}");
        sb.AppendLine($"  Download Playlists: {DownloadPlaylists}");
        sb.AppendLine($"  Download Shorts: {DownloadShorts}");
        sb.AppendLine($"  Download Channels: {DownloadChannels}");
        sb.AppendLine($"  Concurrent Downloads: {ConcurrentDownloads}");
        sb.AppendLine($"  Cookies Auto Extract: {CookiesAutoextract}");
        sb.AppendLine($"  yt-dlp Binary Path: {YtDlpBinaryPath ?? "N/A"}");
        sb.AppendLine($"  Can Change Settings: {CanChangeSettings}");
        sb.AppendLine($"  Selected yt-dlp Config File: {SelectedYtDlpConfigFile ?? "N/A"}");

        sb.AppendLine("  yt-dlp Config Files:");
        if (YtDlpConfigFiles is { Count: > 0 })
        {
            foreach (string file in YtDlpConfigFiles)
            {
                sb.AppendLine($"    - {file}");
            }
        }
        else
            sb.AppendLine("    No config files found.");
        sb.AppendLine($"  Chosen application theme: {SelectedTheme.ToString()}");

        return sb.ToString();
    }

    public static SettingsStruct GetDefaultSettings()
    {
        var outputFolder = Directory.GetCurrentDirectory();
        return new SettingsStruct(
            cmanualimportfilelocation: "", cmanualImportUsed: false,
            coutputfolder: outputFolder,
            cytdlp_executable_not_found: true,
            cdownloadPlaylists: false, cdownloadShorts: false,
            cdownloadChannels: false,
            cconcurrent_downloads: false, ccookies_autoextract: false,
            cyt_dlp_binary_path: YtdlpInterfacing.Yt_dlp_pathfinder(Directory.GetCurrentDirectory()),
            ccanChangeSettings: true, cytDlpConfigFiles: YtdlpInterfacing.Yt_dlp_configfinder(output_folder: outputFolder),
            cselectedYtDlpConfigFile: null,
            cselectedTheme: AppTheme.Fluent);
    }
}