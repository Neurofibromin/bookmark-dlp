using System;
using System.Collections.Generic;
using System.Text;

namespace bookmark_dlp
{
    public partial class Methods
    {
        public static void LogVerbose(string message, Verbosity messageurgency = Verbosity.info)
        {
                if (messageurgency <= verbosity)
                {
                    Console.WriteLine(messageurgency.ToString() + ": " + message);
                }
        }

        public enum Verbosity { critical = 0, error = 1, warning = 2, info = 3, debug = 4, trace = 5 }
        public static Verbosity verbosity = Verbosity.info;
    }
}
