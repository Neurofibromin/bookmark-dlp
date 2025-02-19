using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using NfLogger;

namespace bookmark_dlp.Models
{
    /// <summary>
    /// Stores a global AppSettings state. ViewModels build their own from the global state and return their changes to the global state.
    /// Built from config file if present. Otherwise defaults are loaded.
    /// </summary>
    public sealed class AppSettings
    {
        /// <summary>
        /// Singleton!
        /// </summary>
        public static SettingsStruct _settings = SettingsStruct.GetDefaultSettings();
        private static string? configloc;
        private static AppSettings _instance = new AppSettings();
        
        //Constructor
        private AppSettings()
        {
            configloc = AppMethods.ConfigFileLocation();
            if (File.Exists(configloc))
            {
                SettingsStruct? imported;
                try
                {
                    string jsonimportstring = File.ReadAllText(configloc);
                    imported = JsonSerializer.Deserialize<SettingsStruct>(jsonimportstring);
                    imported = ValidateImportedSettingsBeforeUse(imported);
                }
                catch
                {
                    Logger.LogVerbose("Settings could not be deserialized, fallback to default settings. Not overwriting corrupt file!", Logger.Verbosity.Error);
                    imported = null;
                }
                if (imported != null)
                {
                    _settings = imported;
                    Logger.LogVerbose("Config import successful", Logger.Verbosity.Info);
                }
                else
                {
                    _settings = SettingsStruct.GetDefaultSettings();
                    configloc = null; //to protect file from overwrite
                }   
            }
            else
            {
                Logger.LogVerbose("Config file does not exist, going with defaults");
                _settings = SettingsStruct.GetDefaultSettings(); //no config file, so set configs to default value
                configloc = null;
            }
            _settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_settings.Yt_dlp_binary_path) && _settings.Yt_dlp_binary_path != null)
            {
                YtdlpInterfacing.YtdlpPath = _settings.Yt_dlp_binary_path;
                _settings.Yt_dlp_configfiles = new ObservableCollection<string>(YtdlpInterfacing.Yt_dlp_configfinder(Directory.GetCurrentDirectory(), _settings.Yt_dlp_binary_path));
                Logger.LogVerbose("Ytdlp path changed in YtdlpInterfacing");
            }
            SaveToFile();
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
            foreach (string file in importedsettings.Yt_dlp_configfiles)
            {
                if(!File.Exists(file))
                    importedsettings.Yt_dlp_configfiles.Remove(file);    
            }    
            
            return importedsettings;
        }

        public static AppSettings GetAppSettings()
        {
            return _instance;
        }

        public static void ResetSettingsToDefault()
        {
            _settings.ManualImportUsed = false;
            _settings.Manualimportfilelocation = "";
            _settings.Outputfolder = Directory.GetCurrentDirectory();
            _settings.Ytdlp_executable_not_found = true;
            _settings.DownloadPlaylists = false; 
            _settings.DownloadShorts = false; 
            _settings.DownloadChannels = false; 
            _settings.Concurrent_downloads = false;
            _settings.Cookies_autoextract = false; 
            _settings.Yt_dlp_binary_path = YtdlpInterfacing.Yt_dlp_pathfinder(Directory.GetCurrentDirectory());
            _settings.Yt_dlp_configfiles = YtdlpInterfacing.Yt_dlp_configfinder(Directory.GetCurrentDirectory(), _settings.Yt_dlp_binary_path);
        }


        private static void SaveToFile()
        {
            //if configloc already exists overwrite it, if not create it
            if (configloc != null) //only save to file if a config file is present or was chosen
            {
                string jsonstringexport = JsonSerializer.Serialize(_settings);
                File.Delete(configloc);
                StreamWriter write = new StreamWriter(configloc, append:false, encoding:Encoding.UTF8);
                write.Write(jsonstringexport);
                write.Close();
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~AppSettings()
        {
            if(configloc != null){ SaveToFile(); } //only save to file if a config file is present or was chosen
        }
        
        /// <summary>
        /// Gets JSON string representation of whole AppSettings state in current form. Used by ViewModels to build local Settings state.
        /// </summary>
        /// <returns>Json string of settings.</returns>
        public static string GetJsonStringRepresentation()
        {
            string a = JsonSerializer.Serialize(_settings);
            return a;
        }

        public static void SetConfigFileLocation(string? configfilelocation)
        {
            configloc = configfilelocation;
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
        
        /*public SettingsStruct(SettingsStruct other)
        {
            manualimportfilelocation = other.manualimportfilelocation;
            manualImportUsed = other.manualImportUsed;
            outputfolder = other.outputfolder;
            ytdlp_executable_not_found = other.ytdlp_executable_not_found;
            downloadPlaylists = other.downloadPlaylists;
            downloadShorts = other.downloadShorts;
            downloadChannels = other.downloadChannels;
            concurrent_downloads = other.concurrent_downloads;
            cookies_autoextract = other.cookies_autoextract;
            yt_dlp_binary_path = other.yt_dlp_binary_path;
            canChangeSettings = other.canChangeSettings;
            yt_dlp_configfiles = new ObservableCollection<string>(other.yt_dlp_configfiles == null ? new List<string>() : other.yt_dlp_configfiles);
        }*/

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
            return new SettingsStruct(
                cmanualimportfilelocation: "", cmanualImportUsed: false,
                coutputfolder: Directory.GetCurrentDirectory(),
                cytdlp_executable_not_found: true,
                cdownloadPlaylists: false, cdownloadShorts: false,
                cdownloadChannels: false,
                cconcurrent_downloads: false, ccookies_autoextract: false,
                cyt_dlp_binary_path: YtdlpInterfacing.Yt_dlp_pathfinder(Directory.GetCurrentDirectory()),
                ccanChangeSettings: true, cyt_dlp_configfiles: YtdlpInterfacing.Yt_dlp_configfinder(),
                cselected_yt_dlp_configfile: null);
        }
    }
}
