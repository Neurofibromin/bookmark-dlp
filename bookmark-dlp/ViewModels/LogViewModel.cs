using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
using bookmark_dlp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System;
using bookmark_dlp.Models;
using NfLogger;


namespace bookmark_dlp.ViewModels
{
    public partial class LogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _logs;
        
        private readonly MemoryStream _logStream;
        private long _lastReadPosition;
        
        public LogViewModel()
        {
            _logs = "";
            _logStream = new MemoryStream();
            Logger.AddStream(_logStream);
            StartPolling();
        }
        
        private async void StartPolling()
        {
            while (true)
            {
                await Task.Delay(100); // Adjust polling interval as needed
                ReadNewDataFromStream();
            }
        }

        private void ReadNewDataFromStream()
        {
            // Check if there's new data in the stream
            long currentReadPosition = _logStream.Length;
            if (currentReadPosition <= _lastReadPosition) return;
            // Seek to the last read position and read the new data
            _logStream.Seek(_lastReadPosition, SeekOrigin.Begin);
            var buffer = new byte[currentReadPosition - _lastReadPosition];
            _logStream.Read(buffer, 0, buffer.Length);

            // Convert the new data to a string and update logs
            var newData = Encoding.UTF8.GetString(buffer);
            Logs += newData;

            // Update the read position
            _lastReadPosition = currentReadPosition;
        }
        
        /* solution using event capable streams (must create ObservableStream wrapper for Stream)
         * private readonly ObservableStream _logStream;

           public LogViewModel()
           {
               _logs = "";
               _logStream = new ObservableStream();
               _logStream.DataWritten += OnDataWritten;

               Logger.AddStream(_logStream); // Assuming Logger.AddStream accepts a Stream
           }

           private void OnDataWritten(object sender, string newData)
           {
               // Append the new data to the logs
               _logs += newData;
           }
         * 
         */
    }    
}