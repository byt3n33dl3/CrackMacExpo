using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer executable process data.
/// </summary>
internal class ProcessPipe : SocketPipe
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ProcessPipe(CmdLineArgs args, StreamReader? src, StreamWriter? dest)
        : base(args) {

        Source = src ?? throw new InvalidOperationException(nameof(src));
        Dest = dest ?? throw new InvalidOperationException(nameof(dest));
    }

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~ProcessPipe() => Dispose();

    /// <summary>
    ///  Asynchronously transfer the executable process data
    ///  between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        StringBuilder data = new();

        int charsRead;
        Connected = true;

        while (Client is not null && Client.Connected)
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

            await WriteAsync(data.ReplaceLineEndings(), token);
            data.Clear();
        }

        if (!Args.UsingExe)
        {
            Console.WriteLine();
        }
        Dispose();
    }
}
