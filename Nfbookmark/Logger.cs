using System;

namespace bookmark_dlp
{
    /// <summary>
    /// Used for logging to console or optionally to files.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Log messages to console considering urgency of message and chosen verbosity by program
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// <param name="messageUrgency">The urgency of the message</param>
        public static void LogVerbose(string message, Verbosity messageUrgency = Verbosity.info)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (messageUrgency <= verbosity)
            {
                Console.WriteLine($"{messageUrgency}: {message}");
            }
        }

        /// <summary>
        /// Measure of how verbose something should be. Default: info
        /// </summary>
        public enum Verbosity
        {
            /// <summary>
            /// just thrown exceptions
            /// </summary>
            critical = 0,
            error = 1,
            /// <summary>
            /// default is warning
            /// </summary>
            warning = 2,
            info = 3,
            debug = 4,
            trace = 5
        }

        /// <summary>
        /// Measure of how verbose the communication of the library should be
        /// </summary>
        public static Verbosity verbosity = Verbosity.info;
    }
}