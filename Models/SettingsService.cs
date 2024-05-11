using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_dlp.Models
{

    //does not work

    public interface ISettingsService
    {
        AppSettings LoadSettings();
        void SaveSettings(AppSettings settings);
    }

    public class SettingsService : ISettingsService
    {
        private string _filePath = "config.txt";

        public AppSettings LoadSettings()
        {
            // Implement loading settings from file
            return null;
        }

        public void SaveSettings(AppSettings settings)
        {
            // Implement saving settings to file
        }
    }
}
