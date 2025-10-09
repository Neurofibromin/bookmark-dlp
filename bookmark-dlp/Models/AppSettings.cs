using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using NfLogger;
using System.IO;
using System;
using System.Linq;

namespace bookmark_dlp.Models
{
    public class AppSettings : IAppSettings
    {
        private string? _configloc;
        
        public SettingsStruct Settings { get; private set; }
        
        public AppSettings(String? configpath_location = null)
        {
            _configloc = configpath_location ?? AppMethods.ConfigFileLocation();
            
            Settings = SettingsStruct.GetDefaultSettings();

            if (!string.IsNullOrEmpty(_configloc) && File.Exists(_configloc))
            {
                LoadFromFile(_configloc);
            }
            else
            {
                Logger.LogVerbose("Config file does not exist or location not set, going with defaults");
            }
            Settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void LoadFromFile(string configPath)
        {
            try
            {
                string jsonimportstring = File.ReadAllText(configPath);
                var imported = JsonSerializer.Deserialize<SettingsStruct>(jsonimportstring);
                Settings = ValidateImportedSettingsBeforeUse(imported) ?? SettingsStruct.GetDefaultSettings();
                Logger.LogVerbose("Config import successful", Logger.Verbosity.Info);
            }
            catch(Exception ex)
            {
                Logger.LogVerbose($"Settings could not be deserialized: {ex.Message}. Falling back to default settings.", Logger.Verbosity.Error);
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
                var directory = Path.GetDirectoryName(_configloc);
                if (directory != null) Directory.CreateDirectory(directory);

                File.WriteAllText(_configloc, jsonstringexport);
            }
        }
        
        private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Yt_dlp_binary_path) && Settings.Yt_dlp_binary_path != null)
            {
                YtdlpInterfacing.YtdlpPath = Settings.Yt_dlp_binary_path;
                Settings.Yt_dlp_configfiles = new ObservableCollection<string>(YtdlpInterfacing.Yt_dlp_configfinder(Directory.GetCurrentDirectory(), Settings.Yt_dlp_binary_path, Settings.Outputfolder));
                Logger.LogVerbose("Ytdlp path changed in YtdlpInterfacing");
            }
            SaveToFile();
        }
        
        public void SetConfigFileLocation(string? configfilelocation)
        {
            _configloc = configfilelocation;
        }

        /// <summary>
        /// Checks imported settingsstruct before it can be used by the program. Handles null values.
        /// Validates whether all files denoted in the settings exist, if not they get replaced by default value.
        /// </summary>
        /// <param name="importedsettings">the struct from the json string</param>
        /// <returns>validated settingsstruct with ytdlpbinarypath, ytdlpexecutablenotfound, manualimportfilelocation, manualimportused, outputfolder and ytdlpconfigfiles set as appropriate</returns>
        private SettingsStruct? ValidateImportedSettingsBeforeUse(SettingsStruct? importedsettings)
        {
            if (importedsettings == null)
                return importedsettings;
            if (importedsettings.Yt_dlp_binary_path != null)
                if (!File.Exists(importedsettings.Yt_dlp_binary_path))
                {
                    importedsettings.Yt_dlp_binary_path = SettingsStruct.GetDefaultSettings().Yt_dlp_binary_path;
                    importedsettings.Ytdlp_executable_not_found = SettingsStruct.GetDefaultSettings().Ytdlp_executable_not_found;
                }
            if (importedsettings.Manualimportfilelocation != null)
                if (!File.Exists(importedsettings.Manualimportfilelocation))
                {
                    importedsettings.Manualimportfilelocation = null;
                    importedsettings.ManualImportUsed = false;
                }
            if (importedsettings.Outputfolder != null)
                if(!Directory.Exists(importedsettings.Outputfolder))
                    importedsettings.Outputfolder = SettingsStruct.GetDefaultSettings().Outputfolder;
            
            var filesToRemove = importedsettings.Yt_dlp_configfiles.Where(file => !File.Exists(file)).ToList();
            foreach (var file in filesToRemove)
            {
                importedsettings.Yt_dlp_configfiles.Remove(file);
            }
            
            return importedsettings;
        }

        public void ResetSettingsToDefault()
        {
            var defaultSettings = SettingsStruct.GetDefaultSettings();
            Settings.Manualimportfilelocation = defaultSettings.Manualimportfilelocation;
            Settings.ManualImportUsed = defaultSettings.ManualImportUsed;
            Settings.Outputfolder = defaultSettings.Outputfolder;
            Settings.Ytdlp_executable_not_found = defaultSettings.Ytdlp_executable_not_found;
            Settings.DownloadPlaylists = defaultSettings.DownloadPlaylists;
            Settings.DownloadShorts = defaultSettings.DownloadShorts;
            Settings.DownloadChannels = defaultSettings.DownloadChannels;
            Settings.Concurrent_downloads = defaultSettings.Concurrent_downloads;
            Settings.Cookies_autoextract = defaultSettings.Cookies_autoextract;
            Settings.Yt_dlp_binary_path = defaultSettings.Yt_dlp_binary_path;
            Settings.CanChangeSettings = defaultSettings.CanChangeSettings;
            Settings.Selected_yt_dlp_configfile = defaultSettings.Selected_yt_dlp_configfile;
            Settings.Yt_dlp_configfiles = new ObservableCollection<string>(defaultSettings.Yt_dlp_configfiles);
        }
        
        /// <summary>
        /// Finalizer
        /// </summary>
        ~AppSettings()
        {
            if(_configloc != null){ SaveToFile(); } //only save to file if a config file is present or was chosen
        }
    }

    /// <summary>
    /// Struct of all settings value types used in the program.
    /// </summary>
    public partial class SettingsStruct : ObservableObject
    {
        [ObservableProperty] public string? manualimportfilelocation;
        [ObservableProperty] public bool manualImportUsed;
        [ObservableProperty] public string? outputfolder;
        [ObservableProperty] public bool ytdlp_executable_not_found;
        [ObservableProperty] public bool downloadPlaylists;
        [ObservableProperty] public bool downloadShorts;
        [ObservableProperty] public bool downloadChannels;
        [ObservableProperty] public bool concurrent_downloads;
        [ObservableProperty] public bool cookies_autoextract;
        [ObservableProperty] public string? yt_dlp_binary_path;
        [ObservableProperty] public bool canChangeSettings;
        [ObservableProperty] public string? selected_yt_dlp_configfile;
        [ObservableProperty] public ObservableCollection<string> yt_dlp_configfiles;
        
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
            ObservableCollection<string> cyt_dlp_configfiles,
            string? cselected_yt_dlp_configfile)
        {
            manualimportfilelocation = cmanualimportfilelocation;
            manualImportUsed = cmanualImportUsed;
            outputfolder = coutputfolder;
            ytdlp_executable_not_found = cytdlp_executable_not_found;
            downloadPlaylists = cdownloadPlaylists;
            downloadShorts = cdownloadShorts;
            downloadChannels = cdownloadChannels;
            concurrent_downloads = cconcurrent_downloads;
            cookies_autoextract = ccookies_autoextract;
            yt_dlp_binary_path = cyt_dlp_binary_path;
            canChangeSettings = ccanChangeSettings;
            yt_dlp_configfiles = cyt_dlp_configfiles;
            selected_yt_dlp_configfile = cselected_yt_dlp_configfile;
        }

        /// <summary>
        /// Parameterless ctor needed by JsonSerializer.Deserialize
        /// </summary>
        public SettingsStruct()
        {
            manualimportfilelocation = null;
            manualImportUsed = false;
            outputfolder = null;
            ytdlp_executable_not_found = true;
            downloadPlaylists = false;
            downloadShorts = false;
            downloadChannels = false;
            concurrent_downloads = false;
            cookies_autoextract = false;
            yt_dlp_binary_path = null;
            canChangeSettings = true;
            yt_dlp_configfiles = new ObservableCollection<string>();
            selected_yt_dlp_configfile = null;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SettingsStruct:");
            sb.AppendLine($"  Manual Import File Location: {Manualimportfilelocation ?? "N/A"}");
            sb.AppendLine($"  Manual Import Used: {ManualImportUsed}");
            sb.AppendLine($"  Output Folder: {Outputfolder ?? "N/A"}");
            sb.AppendLine($"  Yt-dlp Executable Not Found: {Ytdlp_executable_not_found}");
            sb.AppendLine($"  Download Playlists: {DownloadPlaylists}");
            sb.AppendLine($"  Download Shorts: {DownloadShorts}");
            sb.AppendLine($"  Download Channels: {DownloadChannels}");
            sb.AppendLine($"  Concurrent Downloads: {Concurrent_downloads}");
            sb.AppendLine($"  Cookies Auto Extract: {Cookies_autoextract}");
            sb.AppendLine($"  yt-dlp Binary Path: {Yt_dlp_binary_path ?? "N/A"}");
            sb.AppendLine($"  Can Change Settings: {CanChangeSettings}");
            sb.AppendLine($"  Selected yt-dlp Config File: {Selected_yt_dlp_configfile ?? "N/A"}");

            sb.AppendLine("  yt-dlp Config Files:");
            if (Yt_dlp_configfiles is { Count: > 0 })
            {
                foreach (var file in Yt_dlp_configfiles)
                {
                    sb.AppendLine($"    - {file}");
                }
            }
            else
            {
                sb.AppendLine("    No config files found.");
            }

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
                ccanChangeSettings: true, cyt_dlp_configfiles: YtdlpInterfacing.Yt_dlp_configfinder(output_folder: outputFolder),
                cselected_yt_dlp_configfile: null);
        }
    }
    
    public interface IAppSettings
    {
        SettingsStruct Settings { get; }
        void ResetSettingsToDefault();
    }
}
