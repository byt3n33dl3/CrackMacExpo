using System;
using System.Linq;
using DotnetCat.Errors;
using DotnetCat.Network.Nodes;
using DotnetCat.Utils;

namespace DotnetCat;

/// <summary>
///  Primary application startup object.
/// </summary>
internal class Program
{
    private static CmdLineArgs? _args;  // Command-line arguments

    private static Parser? _parser;     // Command-line argument parser

    /// <summary>
    ///  Network socket node.
    /// </summary>
    public static Node? SockNode { get; private set; }

    /// <summary>
    ///  Static application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        _parser = new Parser();
        Console.Title = $"DotnetCat ({Parser.Repo})";

        // Display help information and exit
        if (args.IsNullOrEmpty() || Parser.NeedsHelp(args))
        {
            _parser.PrintHelp();
        }

        InitializeNode(args);
        ConnectNode();

        Console.WriteLine();
        Environment.Exit(0);
    }

    /// <summary>
    ///  Parse the given command-line argument array and initialize
    ///  the primary socket node.
    /// </summary>
    private static void InitializeNode(string[] args)
    {
        if (args.Contains("-"))
        {
            Error.Handle(Except.InvalidArgs, "-", true);
        }

        if (args.Contains("--"))
        {
            Error.Handle(Except.InvalidArgs, "--", true);
        }

        if (_parser is null)
        {
            throw new InvalidOperationException("Null argument parser");
        }

        _args = _parser.Parse(args);
        SockNode = _args.Listen ? new ServerNode(_args) : new ClientNode(_args);
    }

    /// <summary>
    ///  Connect the primary network client node or server node.
    /// </summary>
    private static void ConnectNode()
    {
        if (SockNode is null)
        {
            throw new InvalidOperationException("Null socket node specified");
        }
        SockNode?.Connect();
    }
}
