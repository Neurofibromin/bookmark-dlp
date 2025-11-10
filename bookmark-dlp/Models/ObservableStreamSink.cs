using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace bookmark_dlp.Models;

/// <summary>
/// A Serilog sink that writes log events to the provided ObservableStream.
/// This allows the UI to react to new log messages in real-time.
/// </summary>
public class ObservableStreamSink : ILogEventSink
{
    private readonly ObservableStream _stream;
    private readonly ITextFormatter _formatter;

    public ObservableStreamSink(ObservableStream stream, ITextFormatter formatter){ 
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    public void Emit(LogEvent logEvent)
    {
        // The sink's responsibility is to format the log event and write it to the target.
        // We use a StringWriter as an intermediary to format the event.
        using var writer = new StringWriter();
        _formatter.Format(logEvent, writer);
            
        // Write the formatted string to our custom ObservableStream.
        // The stream will then raise its DataWritten event, which the UI is listening to.
        using var streamWriter = new StreamWriter(_stream, leaveOpen: true);
        streamWriter.Write(writer.ToString());
        streamWriter.Flush();
    }
}