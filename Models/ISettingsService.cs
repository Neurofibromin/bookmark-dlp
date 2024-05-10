using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_dlp.Models
{
    public interface ISettingsService
    {
        bool DownloadPlaylists { get; set; }
        bool DownloadShorts { get; set; }
        
        string? YtDlpBinaryPath { get; set; }
        
        bool DownloadChannels { get; set; }

        bool Concurrent_downloads { get; set; }

        bool Cookies_autoextract { get; set; }




    }
}
