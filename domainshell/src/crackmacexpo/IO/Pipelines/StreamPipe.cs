using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Shell;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer standard console stream data.
/// </summary>
internal class StreamPipe : SocketPipe
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public StreamPipe(StreamReader? src, StreamWriter? dest) : base()
    {
        Source = src ?? throw new ArgumentNullException(nameof(src));
        Dest = dest ?? throw new ArgumentNullException(nameof(dest));
    }

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~StreamPipe() => Dispose();

    /// <summary>
    ///  Asynchronously transfer console stream data between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        StringBuilder data = new();

        int charsRead;
        Connected = true;

        if (Client is not null)
        {
            while (Client.Connected)
            {
                if (token.IsCancellationRequested)
                {
                    Disconnect();
                    break;
                }

                charsRead = await ReadAsync(token);
                data.Append(Buffer.ToArray(), 0, charsRead);

                if (!Client.Connected || charsRead <= 0)
                {
                    Disconnect();
                    break;
                }
                data = data.ReplaceLineEndings();

                // Clear the console screen buffer
                if (Command.IsClearCmd(data.ToString()))
                {
                    Sequence.ClearScreen();
                    await WriteAsync(NewLine, token);
                }
                else  // Send the command
                {
                    await WriteAsync(data, token);
                }
                data.Clear();
            }
            Dispose();
        }
    }
}
