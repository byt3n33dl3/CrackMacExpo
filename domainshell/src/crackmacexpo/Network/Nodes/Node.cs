using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotnetCat.Contracts;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Shell;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes;

/// <summary>
///  Abstract TCP network socket node. This is the base class for all
///  socket nodes in the <c>DotnetCat.Network.Nodes</c> namespace.
/// </summary>
internal abstract class Node : ISockErrorHandled
{
    private readonly List<SocketPipe> _pipes;  // TCP socket pipelines

    private bool _validArgsCombos;             // Valid command-line arguments

    private string? _hostName;                 // Target hostname

    private Process? _process;                 // Executable process

    private StreamReader? _netReader;          // TCP stream reader

    private StreamWriter? _netWriter;          // TCP stream writer

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected Node()
    {
        _pipes = [];
        _validArgsCombos = false;

        Args = new CmdLineArgs();
        Client = new TcpClient();

        Port = 44444;
        Verbose = false;
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected Node(IPAddress addr, int port = 44444) : this()
    {
        Address = addr;
        HostName = addr.ToString();
        Port = port;
    }

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    protected Node(CmdLineArgs args) : this() => Args = args;

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~Node() => Dispose();

    /// <summary>
    ///  Enable verbose console output.
    /// </summary>
    public bool Verbose
    {
        get => Args.Verbose;
        set => Args.Verbose = value;
    }

    /// <summary>
    ///  Network port number.
    /// </summary>
    public int Port
    {
        get => Args.Port;
        set
        {
            if (!Net.IsValidPort(value))
            {
                throw new ArgumentException("Invalid port", nameof(value));
            }
            Args.Port = value;
        }
    }

    /// <summary>
    ///  Executable file path.
    /// </summary>
    public string? ExePath
    {
        get => Args.ExePath;
        set => Args.ExePath = value;
    }

    /// <summary>
    ///  Transfer file path.
    /// </summary>
    public string? FilePath
    {
        get => Args.FilePath;
        set => Args.FilePath = value;
    }

    /// <summary>
    ///  Network hostname.
    /// </summary>
    public string HostName
    {
        get => _hostName ?? Address?.ToString() ?? string.Empty;
        set => _hostName = value ?? Address?.ToString() ?? string.Empty;
    }

    /// <summary>
    ///  IPv4 network address.
    /// </summary>
    public IPAddress? Address
    {
        get => Args.Address;
        set => Args.Address = value ?? IPAddress.Any;
    }

    /// <summary>
    ///  TCP socket client.
    /// </summary>
    public TcpClient Client { get; set; }

    /// <summary>
    ///  File transfer option.
    /// </summary>
    protected bool Transfer => Args.TransOpt is not TransferOpt.None;

    /// <summary>
    ///  Using an executable pipeline.
    /// </summary>
    protected bool UsingExe => Args.UsingExe;

    /// <summary>
    ///  Command-line arguments.
    /// </summary>
    protected CmdLineArgs Args { get; set; }

    /// <summary>
    ///  TCP network stream.
    /// </summary>
    protected NetworkStream? NetStream { get; set; }

    /// <summary>
    ///  Initialize and run a new executable process on the local system.
    /// </summary>
    public bool StartProcess(string? exe)
    {
        (string? path, bool exists) = FileSys.ExistsOnPath(exe);

        if (!exists)
        {
            Dispose();
            Error.Handle(Except.ExePath, exe, true);
        }

        _process = new Process
        {
            StartInfo = Command.GetExeStartInfo(ExePath = path)
        };
        return _process.Start();
    }

    /// <summary>
    ///  Activate asynchronous communication between the source and
    ///  destination streams in each of the underlying pipelines.
    /// </summary>
    public virtual void Connect()
    {
        _ = NetStream ?? throw new ArgumentNullException(nameof(NetStream));

        if (!_validArgsCombos)
        {
            ValidateArgsCombinations();
        }

        AddPipes(Args.PipeVariant);
        _pipes?.ForEach(p => p?.Connect());
    }

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    public virtual void PipeError(Except type,
                                  HostEndPoint target,
                                  Exception? ex = default,
                                  Level level = default) {
        Dispose();
        Error.Handle(type, target.ToString(), ex, level);
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
    ///  Release all the underlying unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
        _pipes?.ForEach(p => p?.Dispose());

        _process?.Dispose();
        _netReader?.Dispose();
        _netWriter?.Dispose();

        Client?.Close();
        NetStream?.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Validate the underlying command-line argument combinations.
    /// </summary>
    protected void ValidateArgsCombinations()
    {
        if (!_validArgsCombos)
        {
            // Combination: --exec, --output/--send
            if (UsingExe && Transfer)
            {
                Console.WriteLine(Parser.Usage);
                PipeError(Except.ArgsCombo, "--exec, --output/--send");
            }

            bool isTextPipe = !Args.Payload.IsNullOrEmpty();

            // Combination: --exec, --text
            if (UsingExe && isTextPipe)
            {
                Console.WriteLine(Parser.Usage);
                PipeError(Except.ArgsCombo, "--exec, --text");
            }

            // Combination: --text, --output/--send
            if (isTextPipe && Transfer)
            {
                Console.WriteLine(Parser.Usage);
                PipeError(Except.ArgsCombo, "--text, --output/--send");
            }

            if (Args.PipeVariant is PipeType.Status)
            {
                // Invalid combo: --listen, --zero-io
                if (Program.SockNode is ServerNode)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--listen, --zero-io");
                }

                // Combination: --zero-io, --text
                if (isTextPipe)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--zero-io, --text");
                }

                // Combination: --zero-io, --output/--send
                if (Transfer)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--zero-io, --output/--send");
                }

                // Combination: --exec, --zero-io
                if (UsingExe)
                {
                    Console.WriteLine(Parser.Usage);
                    PipeError(Except.ArgsCombo, "--exec, --zero-io");
                }
            }

            _validArgsCombos = true;
        }
    }

    /// <summary>
    ///  Initialize the underlying pipelines based on the given pipeline type.
    /// </summary>
    protected void AddPipes(PipeType pipeType)
    {
        if (NetStream is null || !NetStream.CanRead || !NetStream.CanWrite)
        {
            throw new InvalidOperationException(nameof(NetStream));
        }

        _netWriter = new StreamWriter(NetStream)
        {
            AutoFlush = true
        };
        _netReader = new StreamReader(NetStream);

        _pipes.AddRange(MakePipes(pipeType));
    }

    /// <summary>
    ///  Wait for the underlying pipeline(s) to be disconnected or
    ///  the system command shell process to exit.
    /// </summary>
    protected void WaitForExit(int msPollDelay = 100)
    {
        do
        {
            Task.Delay(msPollDelay).Wait();

            if (ProcessExited() || !PipelinesConnected())
            {
                break;
            }
        }
        while (Client.Connected);
    }

    /// <summary>
    ///  Initialize a new list of pipelines from the given pipeline type.
    /// </summary>
    private List<SocketPipe> MakePipes(PipeType type)
    {
        List<SocketPipe> pipelines = [];

        switch (type)
        {
            case PipeType.Stream:
                pipelines.AddRange(MakeStreamPipes());
                break;
            case PipeType.File:
                pipelines.Add(MakeFilePipe());
                break;
            case PipeType.Process:
                pipelines.AddRange(MakeProcessPipes());
                break;
            case PipeType.Status:
                pipelines.Add(new StatusPipe(Args, _netWriter));
                break;
            case PipeType.Text:
                pipelines.Add(new TextPipe(Args, _netWriter));
                break;
            default:
                break;
        }
        return pipelines;
    }

    /// <summary>
    ///  Initialize a new array of console stream pipelines.
    /// </summary>
    private StreamPipe[] MakeStreamPipes() => new[]
    {
        new StreamPipe(_netReader, new StreamWriter(Console.OpenStandardOutput())
        {
            AutoFlush = true
        }),
        new StreamPipe(new StreamReader(Console.OpenStandardInput()), _netWriter)
    };

    /// <summary>
    ///  Initialize a new array of executable process pipelines.
    /// </summary>
    private ProcessPipe[] MakeProcessPipes() => new[]
    {
        new ProcessPipe(Args, _netReader, _process?.StandardInput),
        new ProcessPipe(Args, _process?.StandardOutput, _netWriter),
        new ProcessPipe(Args, _process?.StandardError, _netWriter)
    };

    /// <summary>
    ///  Initialize a new file pipeline.
    /// </summary>
    private FilePipe MakeFilePipe()
    {
        if (Args.TransOpt is TransferOpt.None)
        {
            throw new InvalidOperationException(nameof(Args.TransOpt));
        }
        FilePipe filePipe;

        // Pipe data from socket to file
        if (Args.TransOpt is TransferOpt.Collect)
        {
            filePipe = new FilePipe(Args, _netReader);
        }
        else  // Pipe data from file to socket
        {
            filePipe = new FilePipe(Args, _netWriter);
        }
        return filePipe;
    }

    /// <summary>
    ///  Determine whether the underlying command shell process exited.
    /// </summary>
    private bool ProcessExited() => UsingExe && (_process?.HasExited ?? false);

    /// <summary>
    ///  Determine whether any of the non-null underlying pipelines are connected.
    /// </summary>
    private bool PipelinesConnected() => _pipes.Any(p => p?.Connected ?? false);
}
