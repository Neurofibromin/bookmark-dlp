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
        public static SettingsStruct _settings = new SettingsStruct();
        public static readonly SettingsStruct defaultsettings = new SettingsStruct
        {
            htmlImportUsed = false,
            htmlfilelocation = "",       
            outputfolder = Directory.GetCurrentDirectory(),
            ytdlp_executable_not_found = true,        
            downloadPlaylists = false,        
            downloadShorts = false,        
            downloadChannels = false,        
            concurrent_downloads = false,        
            cookies_autoextract = false,
            yt_dlp_binary_path = AppMethods.Yt_dlp_pathfinder(Directory.GetCurrentDirectory()),
        };
        public static string? configloc = AppMethods.ConfigFileLocation();
        private static AppSettings _instance = new AppSettings();
        
        //Constructor
        protected AppSettings()
        {
            configloc = AppMethods.ConfigFileLocation();
            if (File.Exists(configloc))
            {
                string jsonimportstring = File.ReadAllText(configloc);
                try
                {
                    SettingsStruct imported = JsonSerializer.Deserialize<SettingsStruct>(jsonimportstring);
                    if (imported == null) { throw new NullReferenceException(); }
                    _settings = imported;
                    Logger.LogVerbose("Config import successful");
                }
                catch
                {
                    Logger.LogVerbose("Settings could not be deserialized, fallback to default settings. Not overwriting corrupt file!");
                    _settings = defaultsettings;
                    configloc = null; //to protect file from overwrite
                }
            }
            else
            {
                Logger.LogVerbose("Config file doesnt exist, going with defaults");
                _settings = defaultsettings; //no config file, so set configs to default value
                configloc = null;
            }
        }
        
        public static AppSettings GetAppSettings()
        {
            return _instance;
        }

        

        public static void SaveToFile()
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
        
    }

    public partial class SettingsStruct : ObservableObject
    {
        public SettingsStruct(string chtmlfilelocation, bool chtmlImportUsed, string coutputfolder, bool cytdlp_executable_not_found, 
            bool cdownloadPlaylists, bool cdownloadShorts, bool cdownloadChannels, bool cconcurrent_downloads, bool ccookies_autoextract, string cyt_dlp_binary_path)
        {
            htmlfilelocation = chtmlfilelocation;
            htmlImportUsed = chtmlImportUsed;
            outputfolder = coutputfolder;
            ytdlp_executable_not_found = cytdlp_executable_not_found;
            downloadPlaylists = cdownloadPlaylists;
            downloadShorts = cdownloadShorts;
            downloadChannels = cdownloadChannels;
            concurrent_downloads = cconcurrent_downloads;
            cookies_autoextract = ccookies_autoextract;
            yt_dlp_binary_path = cyt_dlp_binary_path;
        }
        
        public SettingsStruct(SettingsStruct other)
        {
            htmlfilelocation = other.htmlfilelocation;
            htmlImportUsed = other.htmlImportUsed;
            outputfolder = other.outputfolder;
            ytdlp_executable_not_found = other.ytdlp_executable_not_found;
            downloadPlaylists = other.downloadPlaylists;
            downloadShorts = other.downloadShorts;
            downloadChannels = other.downloadChannels;
            concurrent_downloads = other.concurrent_downloads;
            cookies_autoextract = other.cookies_autoextract;
            yt_dlp_binary_path = other.yt_dlp_binary_path;
        }

        public SettingsStruct()
        {
            htmlfilelocation = null;
            htmlImportUsed = false;
            outputfolder = null;
            ytdlp_executable_not_found = true;
            downloadPlaylists = false;
            downloadShorts = false;
            downloadChannels = false;
            concurrent_downloads = false;
            cookies_autoextract = false;
            yt_dlp_binary_path = null;
        }
        
        
        [ObservableProperty]
        public string htmlfilelocation;
        [ObservableProperty]
        public bool htmlImportUsed;
        [ObservableProperty]
        public string outputfolder;


        [ObservableProperty]
        public bool ytdlp_executable_not_found;
        [ObservableProperty]
        public bool downloadPlaylists;
        [ObservableProperty]
        public bool downloadShorts;
        [ObservableProperty]
        public bool downloadChannels;
        [ObservableProperty]
        public bool concurrent_downloads;
        [ObservableProperty]
        public bool cookies_autoextract;
        [ObservableProperty]
        public string? yt_dlp_binary_path;
    }
}
