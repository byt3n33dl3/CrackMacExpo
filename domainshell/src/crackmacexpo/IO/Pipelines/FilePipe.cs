using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Errors;
using DotnetCat.Utils;

namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Unidirectional socket pipeline used to transfer file data.
/// </summary>
internal class FilePipe : SocketPipe, IErrorHandled
{
    private readonly TransferOpt _transfer;  // File transfer option

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, StreamReader? src) : base(args)
    {
        if (args.FilePath.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(args));
        }
        _transfer = TransferOpt.Collect;

        Source = src ?? throw new ArgumentNullException(nameof(src));

        Dest = new StreamWriter(CreateFile(FilePath))
        {
            AutoFlush = true
        };
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public FilePipe(CmdLineArgs args, StreamWriter? dest) : base(args)
    {
        if (args.FilePath.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(args));
        }
        _transfer = TransferOpt.Transmit;

        Dest = dest ?? throw new ArgumentNullException(nameof(dest));
        Source = new StreamReader(OpenFile(FilePath));
    }

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~FilePipe() => Dispose();

    /// <summary>
    ///  Enable verbose console output.
    /// </summary>
    public bool Verbose => Args.Verbose;

    /// <summary>
    ///  Source or destination path.
    /// </summary>
    public string FilePath
    {
        get => Args.FilePath ??= string.Empty;
        set => Args.FilePath = value ?? string.Empty;
    }

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    public virtual void PipeError(Except type,
                                  string? arg,
                                  Exception? ex = default,
                                  Level level = default) {
        Dispose();
        Error.Handle(type, arg, ex, level);
    }

    /// <summary>
    ///  Create or overwrite a file at the given file path for writing.
    /// </summary>
    protected FileStream CreateFile(string path)
    {
        if (path.IsNullOrEmpty())
        {
            PipeError(Except.EmptyPath, "-o/--output");
        }
        string? parentPath = FileSys.ParentPath(path);

        if (!FileSys.DirectoryExists(parentPath))
        {
            PipeError(Except.DirectoryPath, parentPath);
        }

        return new FileStream(path,
                              FileMode.Create,
                              FileAccess.Write,
                              FileShare.Write,
                              bufferSize: BUFFER_SIZE,
                              useAsync: true);
    }

    /// <summary>
    ///  Open an existing file at the given file path for reading.
    /// </summary>
    protected FileStream OpenFile(string? path)
    {
        if (path.IsNullOrEmpty())
        {
            PipeError(Except.EmptyPath, "-s/--send");
        }
        FileInfo info = new(path ?? string.Empty);

        if (!info.Exists)
        {
            PipeError(Except.FilePath, info.FullName);
        }

        return new FileStream(info.FullName,
                              FileMode.Open,
                              FileAccess.Read,
                              FileShare.Read,
                              bufferSize: 4096,
                              useAsync: true);
    }

    /// <summary>
    ///  Asynchronously perform the file transfer between the underlying streams.
    /// </summary>
    protected override async Task ConnectAsync(CancellationToken token)
    {
        Connected = true;
        StringBuilder data = new();

        if (Verbose && _transfer is TransferOpt.Collect)
        {
            Style.Info($"Downloading socket data to '{FilePath}'...");
        }
        else if (Verbose && _transfer is TransferOpt.Transmit)
        {
            Style.Info($"Transmitting '{FilePath}' data...");
        }

        data.Append(await ReadToEndAsync());
        await WriteAsync(data, token);

        if (Verbose && _transfer is TransferOpt.Collect)
        {
            Style.Output($"File successfully downloaded to '{FilePath}'");
        }
        else if (Verbose && _transfer is TransferOpt.Transmit)
        {
            Style.Output("File successfully transmitted");
        }

        Disconnect();
        Dispose();
    }
}
