using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using NfLogger;

namespace bookmark_dlp.Models;

public class AppSettings : IAppSettings
{
    private string? _configloc;

    public AppSettings(string? configpath_location = null)
    {
        _configloc = configpath_location ?? AppMethods.ConfigFileLocation();

        Settings = SettingsStruct.GetDefaultSettings();

        if (!string.IsNullOrEmpty(_configloc) && File.Exists(_configloc))
            LoadFromFile(_configloc);
        else
            Logger.LogVerbose("Config file does not exist or location not set, going with defaults");
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
        Settings.YtDlpConfigFiles = new ObservableCollection<string>(defaultSettings.YtDlpConfigFiles);
    }

    private void LoadFromFile(string configPath)
    {
        try
        {
            string jsonimportstring = File.ReadAllText(configPath);
            SettingsStruct? imported = JsonSerializer.Deserialize<SettingsStruct>(jsonimportstring);
            Settings = ValidateImportedSettingsBeforeUse(imported) ?? SettingsStruct.GetDefaultSettings();
            Logger.LogVerbose("Config import successful");
        }
        catch (Exception ex)
        {
            Logger.LogVerbose($"Settings could not be deserialized: {ex.Message}. Falling back to default settings.",
                Logger.Verbosity.Error);
            Settings = SettingsStruct.GetDefaultSettings();
            _configloc = null; // Protect corrupt file from being overwritten
        }
    }

    public void SaveToFile()
    {
        if (_configloc != null)
        {
            string jsonstringexport = JsonSerializer.Serialize(Settings);
            // Ensure directory exists before writing
            string? directory = Path.GetDirectoryName(_configloc);
            if (directory != null) Directory.CreateDirectory(directory);

            File.WriteAllText(_configloc, jsonstringexport);
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
            Logger.LogVerbose("Ytdlp path changed in YtdlpInterfacing");
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
            return importedsettings;
        if (importedsettings.YtDlpBinaryPath != null)
        {
            if (!File.Exists(importedsettings.YtDlpBinaryPath))
            {
                importedsettings.YtDlpBinaryPath = SettingsStruct.GetDefaultSettings().YtDlpBinaryPath;
                importedsettings.YtDlpExecutableNotFound =
                    SettingsStruct.GetDefaultSettings().YtDlpExecutableNotFound;
            }
        }

        if (importedsettings.ManualImportFileLocation != null)
        {
            if (!File.Exists(importedsettings.ManualImportFileLocation))
            {
                importedsettings.ManualImportFileLocation = null;
                importedsettings.ManualImportUsed = false;
            }
        }

        if (importedsettings.OutputFolder != null)
        {
            if (!Directory.Exists(importedsettings.OutputFolder))
                importedsettings.OutputFolder = SettingsStruct.GetDefaultSettings().OutputFolder;
        }

        List<string> filesToRemove = importedsettings.YtDlpConfigFiles.Where(file => !File.Exists(file)).ToList();
        foreach (string file in filesToRemove)
        {
            importedsettings.YtDlpConfigFiles.Remove(file);
        }

        return importedsettings;
    }

    /// <summary>
    ///     Finalizer
    /// </summary>
    ~AppSettings()
    {
        if (_configloc != null) SaveToFile(); //only save to file if a config file is present or was chosen
    }
}

public interface IAppSettings
{
    SettingsStruct Settings { get; }
    void ResetSettingsToDefault();
}
