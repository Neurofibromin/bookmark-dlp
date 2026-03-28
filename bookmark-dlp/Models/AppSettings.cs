using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace bookmark_dlp.Models;

public class AppSettings : IAppSettings
{
    private readonly ILogger Log = Serilog.Log.ForContext<AppSettings>();
    private string? _configloc;

    public AppSettings(string? configpath_location = null)
    {
        _configloc = configpath_location ?? AppMethods.ConfigFileLocation();

        Settings = SettingsStruct.GetDefaultSettings();

        if (!string.IsNullOrEmpty(_configloc) && File.Exists(_configloc))
            LoadFromFile(_configloc);
        else
            Log.Information("Config file does not exist or location not set, going with defaults");
        Settings.PropertyChanged += SettingsOnPropertyChanged;
    }

    public SettingsStruct Settings { get; private set; }

    public void ResetSettingsToDefault()
    {
        SettingsStruct defaultSettings = SettingsStruct.GetDefaultSettings();
        Settings.ManualImportFileLocation = defaultSettings.ManualImportFileLocation;
        Settings.ManualImportUsed = defaultSettings.ManualImportUsed;
        Settings.OutputFolder = defaultSettings.OutputFolder;
        Settings.YtDlpExecutableNotFound = defaultSettings.YtDlpExecutableNotFound;
        Settings.DownloadPlaylists = defaultSettings.DownloadPlaylists;
        Settings.DownloadShorts = defaultSettings.DownloadShorts;
        Settings.DownloadChannels = defaultSettings.DownloadChannels;
        Settings.ConcurrentDownloads = defaultSettings.ConcurrentDownloads;
        Settings.CookiesAutoextract = defaultSettings.CookiesAutoextract;
        Settings.YtDlpBinaryPath = defaultSettings.YtDlpBinaryPath;
        Settings.CanChangeSettings = defaultSettings.CanChangeSettings;
        Settings.SelectedYtDlpConfigFile = defaultSettings.SelectedYtDlpConfigFile;
        Settings.SelectedTheme = defaultSettings.SelectedTheme;
        Settings.YtDlpConfigFiles = new ObservableCollection<string>(defaultSettings.YtDlpConfigFiles);
    }

    private void LoadFromFile(string configPath)
    {
        try
        {
            string jsonimportstring = File.ReadAllText(configPath);
            SettingsStruct? imported = JsonSerializer.Deserialize<SettingsStruct>(jsonimportstring);
            Settings = ValidateImportedSettingsBeforeUse(imported) ?? SettingsStruct.GetDefaultSettings();
            Log.Information("Config import successful from {ConfigPath}", configPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Settings could not be deserialized from {ConfigPath}. Falling back to default settings.", configPath);
            Settings = SettingsStruct.GetDefaultSettings();
            _configloc = null; // Protect corrupt file from being overwritten
        }
    }

    private void SaveToFile()
    {
        if (_configloc != null)
        {
            try
            {
                string jsonstringexport = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                // Ensure directory exists before writing
                string? directory = Path.GetDirectoryName(_configloc);
                if (directory != null) Directory.CreateDirectory(directory);

                File.WriteAllText(_configloc, jsonstringexport);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save settings to {ConfigLocation}", _configloc);
            }
        }
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.YtDlpBinaryPath) && Settings.YtDlpBinaryPath != null)
        {
            YtdlpInterfacing.YtdlpPath = Settings.YtDlpBinaryPath;
            Settings.YtDlpConfigFiles = new ObservableCollection<string>(
                YtdlpInterfacing.Yt_dlp_configfinder(Directory.GetCurrentDirectory(), Settings.YtDlpBinaryPath,
                    Settings.OutputFolder));
            Log.Debug("Ytdlp path changed in YtdlpInterfacing to {YtdlpPath}", Settings.YtDlpBinaryPath);
        }

        SaveToFile();
    }

    public void SetConfigFileLocation(string? configfilelocation)
    {
        _configloc = configfilelocation;
    }

    /// <summary>
    ///     Checks imported settingsstruct before it can be used by the program. Handles null values.
    ///     Validates whether all files denoted in the settings exist, if not they get replaced by default value.
    /// </summary>
    /// <param name="importedsettings">the struct from the json string</param>
    /// <returns>
    ///     validated settingsstruct with ytdlpbinarypath, ytdlpexecutablenotfound, _manualImportFileLocation,
    ///     manualimportused, _outputFolder and ytdlpconfigfiles set as appropriate
    /// </returns>
    private SettingsStruct? ValidateImportedSettingsBeforeUse(SettingsStruct? importedsettings)
    {
        if (importedsettings == null)
            return null;

        if (importedsettings.YtDlpBinaryPath != null && !File.Exists(importedsettings.YtDlpBinaryPath))
        {
            Log.Warning("yt-dlp binary path not found at {YtdlpPath}, reverting to default.", importedsettings.YtDlpBinaryPath);
            importedsettings.YtDlpBinaryPath = SettingsStruct.GetDefaultSettings().YtDlpBinaryPath;
            importedsettings.YtDlpExecutableNotFound = SettingsStruct.GetDefaultSettings().YtDlpExecutableNotFound;
        }

        if (importedsettings.ManualImportFileLocation != null && !File.Exists(importedsettings.ManualImportFileLocation))
        {
            Log.Warning("Manual import file not found at {ManualImportFileLocation}, clearing setting.", importedsettings.ManualImportFileLocation);
            importedsettings.ManualImportFileLocation = null;
            importedsettings.ManualImportUsed = false;
        }

        if (importedsettings.OutputFolder != null && !Directory.Exists(importedsettings.OutputFolder))
        {
            Log.Warning("Output folder not found at {OutputFolder}, reverting to default.", importedsettings.OutputFolder);
            importedsettings.OutputFolder = SettingsStruct.GetDefaultSettings().OutputFolder;
        }

        List<string> filesToRemove = importedsettings.YtDlpConfigFiles.Where(file => !File.Exists(file)).ToList();
        foreach (string file in filesToRemove)
        {
            Log.Warning("yt-dlp config file not found at {ConfigFile}, removing from list.", file);
            importedsettings.YtDlpConfigFiles.Remove(file);
        }

        return importedsettings;
    }
}

public interface IAppSettings
{
    SettingsStruct Settings { get; }
    void ResetSettingsToDefault();
}