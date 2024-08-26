using System.Net;
using DotnetCat.IO.Pipelines;

namespace DotnetCat.Utils;

/// <summary>
///  DotnetCat command-line arguments.
/// </summary>
internal class CmdLineArgs
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public CmdLineArgs()
    {
        Debug = Listen = Verbose = false;

        PipeVariant = PipeType.Stream;
        TransOpt = TransferOpt.None;
        Port = 44444;

        Address = IPAddress.Any;
    }

    /// <summary>
    ///  Enable verbose exceptions.
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    ///  Run server and listen for inbound connection.
    /// </summary>
    public bool Listen { get; set; }

    /// <summary>
    ///  Using executable pipeline.
    /// </summary>
    public bool UsingExe => !ExePath.IsNullOrEmpty();

    /// <summary>
    ///  Enable verbose console output.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    ///  Pipeline variant.
    /// </summary>
    public PipeType PipeVariant { get; set; }

    /// <summary>
    ///  File transfer option.
    /// </summary>
    public TransferOpt TransOpt { get; set; }

    /// <summary>
    ///  Connection port number.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///  Executable file path.
    /// </summary>
    public string? ExePath { get; set; }

    /// <summary>
    ///  Transfer file path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    ///  Hostname of the connection IPv4 address.
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    ///  User-defined string payload.
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    ///  IPv4 address to use for connection.
    /// </summary>
    public IPAddress Address { get; set; }
}
