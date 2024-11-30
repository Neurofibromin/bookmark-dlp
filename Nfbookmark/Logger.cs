using System;
using System.Collections.Generic;
using System.IO;

namespace NfLogger
{
    /// <summary>
    /// Used for logging to console or optionally to files.
    /// Standard output is added as log target by default.
    /// </summary>
    public static class Logger
    {
        private static List<Stream> LogStreams;
        private static List<string> LogFiles;
        private static List<StreamWriter> LogWriters;
        public static event EventHandler ProcessExit;
        
        /// <summary>
        /// Measure of how verbose something should be. Default: info
        /// </summary>
        public enum Verbosity
        {
            /// <summary>
            /// Only thrown exceptions
            /// </summary>
            Critical = 0,
            Error = 1,
            /// <summary>
            /// Default is warning
            /// </summary>
            Warning = 2,
            Info = 3,
            Debug = 4,
            Trace = 5
        }
        
        /// <summary>
        /// Measure of how verbose the communication of the library should be
        /// </summary>
        public static Verbosity verbosity;
        static Logger()
        {
            // Initialize lists
            LogStreams = new List<Stream>();
            LogFiles = new List<string>();
            LogWriters = new List<StreamWriter>();

            // Add standard output as a default log stream
            Stream stdout = Console.OpenStandardOutput();
            LogStreams.Add(stdout);

            // Attach the ProcessExit event handler
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            verbosity = Verbosity.Info;
        }

        // Finalizers don't exist for static classes, so ProcessExit event handling is required
        private static void OnProcessExit(object sender, EventArgs e)
        {
            // Flush and close all StreamWriter objects
            foreach (StreamWriter writer in LogWriters)
            {
                writer.Flush();
                writer.Close();
            }

            // Dispose all streams
            foreach (Stream stream in LogStreams)
            {
                stream.Dispose();
            }
        }
        
        
        /// <summary>
        /// Log messages to console considering urgency of message and chosen verbosity by program
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// <param name="messageUrgency">The urgency of the message</param>
        public static void LogVerbose(string message, Verbosity messageUrgency = Verbosity.Info)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (messageUrgency <= verbosity)
            {
                foreach (StreamWriter writer in LogWriters)
                {
                    writer.WriteLine($"{messageUrgency}: {message}");
                }
            }
        }

        /// <summary>
        /// Adds stream to logging targets, if stream was already present among them does nothing.
        /// </summary>
        /// <param name="stream">stream to be added</param>
        public static void AddStream(Stream stream)
        {
            if (!LogStreams.Contains(stream))
            {
                LogStreams.Add(stream);
                GenerateStreamWriters();    
            }
        }

        /// <summary>
        /// Adds file to logging targets, if file was already present among them does nothing.
        /// </summary>
        /// <param name="file">file to be added</param>
        public static void AddFile(string file)
        {
            if (!LogFiles.Contains(file))
            {
                LogFiles.Add(file);
                GenerateStreamWriters();
            }
        }

        /// <summary>
        /// Regenerates the StreamWriters for LogWriters from LogFiles and LogStreams
        /// </summary>
        private static void GenerateStreamWriters()
        {
            foreach (StreamWriter writer in LogWriters)
            {
                writer.Flush();
                writer.Close();
            }
            LogWriters.Clear();

            foreach (string file in LogFiles)
            {
                StreamWriter writer = new StreamWriter(file, append: true);
                writer.AutoFlush = true;
                LogWriters.Add(writer);
            }

            foreach (Stream stream in LogStreams)
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                LogWriters.Add(writer);
            }
        }
    }
}