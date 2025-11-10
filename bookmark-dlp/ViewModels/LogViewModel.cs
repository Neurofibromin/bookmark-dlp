using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using NfLogger;

namespace bookmark_dlp.ViewModels;

public partial class LogViewModel : ViewModelBase
{
    private readonly Models.ObservableStream _logStream;
    [ObservableProperty] private string _logs;
    public LogViewModel()
    {
        _logs = "";
        _logStream = App.UiLogStream; 
        _logStream.DataWritten += OnDataWritten;
 
        Logger.AddStream(_logStream);
    }
 
    private void OnDataWritten(object sender, string newData)
    {
        // Append the new data to the logs
        Logs += newData;
    }
}