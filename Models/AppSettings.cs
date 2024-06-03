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
        private static AppSettings _instance = new();
        public static SettingsStruct _settings = new SettingsStruct();
        public static string? configloc = Methods.ConfigFileLocation();

        private readonly SettingsStruct defaultsettings = new SettingsStruct
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
        
        //Constructor
        protected AppSettings()
        {
            configloc = Methods.ConfigFileLocation();
            if (File.Exists(configloc))
            {
                string jsonimportstring = File.ReadAllText(configloc);
                try
                {
                    SettingsStruct imported = JsonConvert.DeserializeObject<SettingsStruct>(jsonimportstring);
                    if (imported == null) { throw new NullReferenceException(); }
                    _settings = imported;
                    Console.WriteLine("import successful");
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
                Console.WriteLine(_settings.ytdlp_executable_not_found + " " + _settings.Ytdlp_executable_not_found);
            }
        }
        
        public static AppSettings GetAppSettings()
        {
            return _instance;
        }

        public static void SaveToFile()
        {
            //if configloc already exists overwrite it, if not create it?
            Console.WriteLine(_settings.ytdlp_executable_not_found + " " + _settings.Ytdlp_executable_not_found);
            string jsonstringexport = JsonConvert.SerializeObject(_settings);
            if (configloc != null)
            {
                File.Delete(configloc);
                StreamWriter write = new StreamWriter(configloc);
                write.Write(jsonstringexport);
                write.Close();
            }
            Console.WriteLine("Saved");
        }

        //Finalizer
        ~AppSettings()
        {
            if(configloc != null){ SaveToFile();} //only save to file if a config file is present or was chosen
            
        }
        
        public static string? GetStringExample()
        {
            return _settings.Outputfolder;
        }
        
    }

    public partial class SettingsStruct : ObservableObject
    {
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
