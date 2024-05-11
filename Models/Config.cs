using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_dlp.Models
{
    public class AppSettings : Object
    {
        
        public string? Htmlfilelocation { get; set; }
        public bool htmlImportUsed {  get; set; }
        public string outputfolder { get; set; }
        public string[] browserlist {  get; set; }
        public string selectedBrowser { get; set; }
        public bool ytdlp_executable_not_found {  get; set; }
    }
}
