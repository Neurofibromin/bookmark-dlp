using System;
using System.Collections.Generic;
using System.IO;

namespace NfLogger
{
    /// <summary>
    ///     Used for logging to streams and to files.
    ///     Standard output is added as log target by default.
    /// </summary>
    public static class Logger
    {
        /*
         For making every Log destination independently verbose:
         internal struct StreamWithVerbosity
        {
            public Stream Stream;
            public Verbosity Verbosity;

            StreamWithVerbosity(Stream stream, Verbosity verbosity)
            {
                Stream = stream;
                Verbosity = verbosity;
            }
        }*/
        
        private static List<Stream> _logStreams;
        private static List<string> _logFiles;
        private static List<StreamWriter> _logWriters;
        // public static event EventHandler ProcessExit;
        
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
            Warning = 2,
            /// <summary>
            /// Default is Info
            /// </summary>
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
            _logStreams = new List<Stream>();
            _logFiles = new List<string>();
            _logWriters = new List<StreamWriter>();

            // Add standard output as a default log stream
            Stream stdout = Console.OpenStandardOutput();
            _logStreams.Add(stdout);

            // Attach the ProcessExit event handler
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            verbosity = Verbosity.Info;
        }

        // Finalizers don't exist for static classes, so ProcessExit event handling is required
        private static void OnProcessExit(object sender, EventArgs e)
        {
            // Flush and close all StreamWriter objects
            foreach (StreamWriter writer in _logWriters)
            {
                writer.Flush();
                writer.Close();
            }

            // Dispose all streams
            foreach (Stream stream in _logStreams)
            {
                stream.Dispose();
            }
        }
        
        
        /// <summary>
        ///     Log messages to console considering urgency of message and chosen verbosity by program
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// <param name="messageUrgency">The urgency of the message</param>
        public static void LogVerbose(string message, Verbosity messageUrgency = Verbosity.Info)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (messageUrgency <= verbosity)
            {
                foreach (StreamWriter writer in _logWriters)
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
            if (!_logStreams.Contains(stream))
            {
                _logStreams.Add(stream);
                GenerateStreamWriters();    
            }
        }
        
        /// <summary>
        /// Removes stream from logging targets, if stream was not present among them does nothing.
        /// </summary>
        /// <param name="stream">stream to be removed</param>
        public static void RemoveStream(Stream stream)
        {
            if (_logStreams.Contains(stream))
            {
                _logStreams.Remove(stream);
                GenerateStreamWriters();
            }
        }

        /// <summary>
        ///     Adds file to logging targets, if file was already present among them does nothing.
        /// </summary>
        /// <param name="file">file to be added</param>
        public static void AddFile(string file)
        {
            if (!_logFiles.Contains(file))
            {
                _logFiles.Add(file);
                GenerateStreamWriters();
            }
        }

        /// <summary>
        ///     Removes file from logging targets, if file was not present among them does nothing.
        /// </summary>
        /// <param name="file">file to be removed</param>
        public static void RemoveFile(string file)
        {
            if (_logFiles.Contains(file))
            {
                _logFiles.Remove(file);
                GenerateStreamWriters();
            }
        }

        /// <summary>
        ///     Regenerates the StreamWriters for LogWriters from LogFiles and LogStreams
        /// </summary>
        private static void GenerateStreamWriters()
        {
            foreach (StreamWriter writer in _logWriters)
            {
                writer.Flush();
                writer.Close();
            }
            _logWriters.Clear();

            foreach (string file in _logFiles)
            {
                StreamWriter writer = new StreamWriter(file, append: true);
                writer.AutoFlush = true;
                _logWriters.Add(writer);
            }

            foreach (Stream stream in _logStreams)
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                _logWriters.Add(writer);
            }
        }
    }
}