using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transmit arbitrary string data.
/// </summary>
internal class TextPipe : SocketPipe
{
    private readonly string _statusMsg;  // Completion status message

    private MemoryStream _memoryStream;  // Memory stream buffer

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public TextPipe(CmdLineArgs args, StreamWriter? dest) : base(args)
    {
        _statusMsg = "Payload successfully transmitted";
        _memoryStream = new MemoryStream();

        Dest = dest ?? throw new ArgumentNullException(nameof(dest));
        Source = new StreamReader(_memoryStream);
    }

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~TextPipe() => Dispose();

    /// <summary>
    ///  String network payload.
    /// </summary>
    protected string Payload
    {
        get => Args.Payload ?? string.Empty;
        set
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(value));
            }

            _memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
            Args.Payload = value;
        }
    }

    /// <summary>
    ///  Release all the underlying unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        _memoryStream?.Dispose();
        base.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Asynchronously transfer the user-defined string payload
    ///  between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;

        StringBuilder data = new(await ReadToEndAsync());
        await WriteAsync(data, token);

        if (Args.Verbose)
        {
            Style.Output(_statusMsg);
        }

        Disconnect();
        Dispose();
    }
}
