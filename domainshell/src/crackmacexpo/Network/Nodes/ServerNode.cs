using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.Utils;

namespace DotnetCat.Network.Nodes;

/// <summary>
///  TCP network socket server node.
/// </summary>
internal class ServerNode : Node
{
    private Socket? _listener;  // Listener socket

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ServerNode() : base(IPAddress.Any) => _listener = null;

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ServerNode(CmdLineArgs args) : base(args) => _listener = null;

    /// <summary>
    ///  Release the unmanaged object resources.
    /// </summary>
    ~ServerNode() => Dispose();

    /// <summary>
    ///  Listen for an inbound TCP connection on the underlying listener socket.
    /// </summary>
    public override void Connect()
    {
        _ = Address ?? throw new ArgumentNullException(nameof(Address));

        IPEndPoint? remoteEP = null;
        IPEndPoint localEP = new(Address, Port);

        ValidateArgsCombinations();
        BindListener(localEP);

        try  // Listen for an inbound connection
        {
            _listener?.Listen(1);
            Style.Info($"Listening for incoming connections on {localEP}...");

            if (_listener is not null)
            {
                Client.Client = _listener.Accept();
            }
            NetStream = Client.GetStream();

            if (Args.UsingExe && !StartProcess(ExePath))
            {
                PipeError(Except.ExeProcess, ExePath);
            }

            remoteEP = Client.Client.RemoteEndPoint as IPEndPoint;
            Style.Info($"Connected to {remoteEP}");

            base.Connect();
            WaitForExit();

            Console.WriteLine();
            Style.Info($"Connection to {remoteEP} closed");
        }
        catch (SocketException ex)  // Socket error occurred
        {
            PipeError(Net.GetExcept(ex), new HostEndPoint(remoteEP), ex);
        }
        catch (IOException ex)      // Connection was reset
        {
            PipeError(Except.ConnectionReset, new HostEndPoint(remoteEP), ex);
        }

        Dispose();
    }

    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    public override void PipeError(Except type,
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
    public override void PipeError(Except type,
                                   string? arg,
                                   Exception? ex = default,
                                   Level level = default) {
        Dispose();
        Error.Handle(type, arg, ex, level);
    }

    /// <summary>
    ///  Release all the underlying unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        _listener?.Close();
        base.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Bind the underlying listener socket to the given IPv4 endpoint.
    /// </summary>
    private void BindListener(IPEndPoint ep)
    {
        _ = ep ?? throw new ArgumentNullException(nameof(ep));

        _listener = new Socket(AddressFamily.InterNetwork,
                               SocketType.Stream,
                               ProtocolType.Tcp);

        try  // Bind the listener socket
        {
            _listener.Bind(ep);
        }
        catch (SocketException ex)
        {
            PipeError(Net.GetExcept(ex), ep.ToString(), ex);
        }
    }
}
