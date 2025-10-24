using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bookmark_dlp.Models;

/// <summary>
/// A Stream wrapper that inherits from MemoryStream and raises an event whenever data is written to it.
/// This is used to provide real-time updates to the LogViewModel without polling.
/// </summary>
public class ObservableStream : MemoryStream
{
    public event EventHandler<string>? DataWritten;

    public override void Write(byte[] buffer, int offset, int count)
    {
        base.Write(buffer, offset, count);
        string newData = Encoding.UTF8.GetString(buffer, offset, count);
        DataWritten?.Invoke(this, newData);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await base.WriteAsync(buffer, offset, count, cancellationToken);

        string newData = Encoding.UTF8.GetString(buffer, offset, count);
        DataWritten?.Invoke(this, newData);
    }
}