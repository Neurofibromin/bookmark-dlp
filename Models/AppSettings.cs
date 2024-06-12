using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace bookmark_dlp.Models
{
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
            yt_dlp_binary_path = null,
        };
        public static string? configloc = Methods.ConfigFileLocation();
        private static AppSettings _instance = new AppSettings();
        
        
        /*
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AppSettings();
                return _instance;
            }
        }*/
        
        //Constructor
        protected AppSettings()
        {
            configloc = Methods.ConfigFileLocation();
            if (File.Exists(configloc))
            {
                string jsonimportstring = File.ReadAllText(configloc);
                // Console.WriteLine("importing: " + jsonimportstring);
                // Console.WriteLine("\n");
                try
                {
                    SettingsStruct imported = JsonConvert.DeserializeObject<SettingsStruct>(jsonimportstring);
                    if (imported == null) { throw new NullReferenceException(); }
                    _settings = imported;
                    Console.WriteLine("Config import successful");
                    // Console.WriteLine("afterimport: " + JsonConvert.SerializeObject(_settings));
                }
                catch
                {
                    Console.WriteLine("Settings could not be deserialized, fallback to default settings. Not overwriting corrupt file!");
                    _settings = defaultsettings;
                    configloc = null; //to protect file from overwrite
                }
                
            }
            else
            {
                Console.WriteLine("Config file doesnt exist, going with defaults");
                _settings = defaultsettings; //no config file, so set configs to default value
                configloc = null;
                // Console.WriteLine(_settings.ytdlp_executable_not_found); // + " " + _settings.Ytdlp_executable_not_found);
            }
            // Console.WriteLine(_settings.downloadShorts);
        }
        
        public static AppSettings GetAppSettings()
        {
            return _instance;
        }

        

        public static void SaveToFile()
        {
            //if configloc already exists overwrite it, if not create it
            string jsonstringexport = JsonConvert.SerializeObject(_settings);
            if (configloc != null) //only save to file if a config file is present or was chosen
            {
                File.Delete(configloc);
                StreamWriter write = new StreamWriter(configloc);
                // Console.WriteLine("exporting: " + jsonstringexport);
                // Console.WriteLine("\n");
                write.Write(jsonstringexport);
                write.Close();
            }
        }

        //Finalizer
        ~AppSettings()
        {
            if(configloc != null){ SaveToFile();} //only save to file if a config file is present or was chosen
            
        }
        
        public static string GetJsonStringRepresentation()
        {
            string a = JsonConvert.SerializeObject(_settings);
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
