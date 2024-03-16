using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_dlp.Models
{
    public static class Config
    {
        
        public static string htmlfilelocation { get; set; }
        public static bool htmlImportUsed {  get; set; }
        public static string outputfolder { get; set; }
        public static string[] browserlist {  get; set; }
        public static string selectedBrowser { get; set; }
        public static bool ytdlp_executable_not_found {  get; set; }
    }
}
