using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.IO.Pipelines;
using DotnetCat.Network;
using DotnetCat.Shell;

namespace DotnetCat.Utils;

/// <summary>
///  Command-line argument parser and validator.
/// </summary>
internal partial class Parser
{
    private static readonly string _title;  // Application title

    private readonly CmdLineArgs _args;     // Command-line arguments

    private List<string> _argsList;         // Command-line argument list

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Parser() => _title = SysInfo.IsLinux() ? "dncat" : "dncat.exe";

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public Parser()
    {
        _args = new CmdLineArgs();
        _argsList = [];
    }

    /// <summary>
    ///  Application repository URL.
    /// </summary>
    public static string Repo => "https://github.com/vandavey/DotnetCat";

    /// <summary>
    ///  Application usage string.
    /// </summary>
    public static string Usage => $"Usage: {_title} [OPTIONS] TARGET";

    /// <summary>
    ///  Determine whether any of the help flags (`-h`, `-?`, `--help`)
    ///  exist in the given command-line argument array.
    /// </summary>
    public static bool NeedsHelp(string[] args)
    {
        bool needsHelp = (from string arg in args
                          where arg == "--help"
                              || (IsAlias(arg)
                                  && (arg.Contains('h') || arg.Contains('?')))
                          select arg).Any();
        return needsHelp;
    }

    /// <summary>
    ///  Parse and validate all the arguments in the given
    ///  command-line argument array.
    /// </summary>
    public CmdLineArgs Parse(string[] args)
    {
        _argsList = DefragArguments([.. args]);

        ParseCharArgs();
        ParseFlagArgs();
        ParsePositionalArgs();

        return _args;
    }

    /// <summary>
    ///  Write the extended application usage information to the standard
    ///  console output stream and exit the application.
    /// </summary>
    [DoesNotReturn]
    public void PrintHelp()
    {
        Console.WriteLine(GetHelpMessage());
        Environment.Exit(0);
    }

    /// <summary>
    ///  Defragment the given fragmented command-line arguments so
    ///  quoted strings are interpreted as single arguments.
    /// </summary>
    private static List<string> DefragArguments(List<string> args)
    {
        List<string> defragArgs = [];

        // Defragment the given arguments
        for (int i = 0; i < args.Count; i++)
        {
            bool begQuoted = args[i].StartsWithValue('\'');

            if (!begQuoted || (begQuoted && args[i].EndsWithValue('\'')))
            {
                defragArgs.Add(args[i]);
                continue;
            }

            if (i == args.Count - 1)
            {
                defragArgs.Add(args[i]);
                break;
            }

            // Locate terminating argument and parse the range
            for (int j = i + 1; j < args.Count; j++)
            {
                if (args[j].EndsWithValue('\''))
                {
                    string argStr = args.ToArray()[i..(j + 1)].Join(" ");

                    // Remove leading and trailing quotes
                    if (argStr.Length >= 2)
                    {
                        argStr = argStr[1..^1];
                    }
                    defragArgs.Add(argStr);

                    i = j;
                    break;
                }
            }
        }

        return defragArgs;
    }

    /// <summary>
    ///  Determine whether the given argument is a command-line flag alias
    ///  argument. Flag alias arguments begin with one dash (`-f`).
    /// </summary>
    private static bool IsAlias(string arg) => AliasRegex().IsMatch(arg);

    /// <summary>
    ///  Determine whether the argument in the given tuple is a command-line flag
    ///  alias argument. Flag alias arguments begin with one dash (`-f`).
    /// </summary>
    private static bool IsAlias((int index, string arg) tuple) => IsAlias(tuple.arg);

    /// <summary>
    ///  Determine whether the given argument is a command-line flag
    ///  argument. Flag arguments begin with two dashes (`--foo`).
    /// </summary>
    private static bool IsFlag(string arg) => FlagRegex().IsMatch(arg);

    /// <summary>
    ///  Determine whether the argument in the given tuple is a command-line
    ///  flag argument. Flag arguments begin with two dashes (`--foo`).
    /// </summary>
    private static bool IsFlag((int index, string arg) tuple) => IsFlag(tuple.arg);

    /// <summary>
    ///  Parse the named flag alias arguments in the underlying command-line
    ///  argument list. Flag alias arguments begin with one dash (`-f`).
    /// </summary>
    private void ParseCharArgs()
    {
        List<int> processedIndexes = [];

        foreach ((int index, string arg) in _argsList.Enumerate(IsAlias))
        {
            foreach (char ch in arg)
            {
                switch (ch)
                {
                    case '-':
                        continue;
                    case 'l':
                        _args.Listen = true;
                        break;
                    case 'v':
                        _args.Verbose = true;
                        break;
                    case 'z':
                        _args.PipeVariant = PipeType.Status;
                        break;
                    case 'd':
                        _args.Debug = _args.Verbose = Error.Debug = true;
                        break;
                    case 'p':
                        _args.Port = GetPort(index, arg);
                        processedIndexes.Add(index + 1);
                        break;
                    case 'e':
                        _args.ExePath = GetExecutable(index, arg);
                        processedIndexes.Add(index + 1);
                        break;
                    case 'o':
                        _args.TransOpt = TransferOpt.Collect;
                        _args.FilePath = GetTransferPath(index, arg);
                        processedIndexes.Add(index + 1);
                        break;
                    case 's':
                        _args.TransOpt = TransferOpt.Transmit;
                        _args.FilePath = GetTransferPath(index, arg);
                        processedIndexes.Add(index + 1);
                        break;
                    case 't':
                        _args.Payload = GetTextPayload(index, arg);
                        processedIndexes.Add(index + 1);
                        break;
                    default:
                        Error.Handle(Except.UnknownArgs, $"-{ch}", true);
                        break;
                }
            }

            processedIndexes.Add(index);
        }

        RemoveProcessedArgs(processedIndexes);
    }

    /// <summary>
    ///  Parse the named flag arguments in the underlying command-line
    ///  argument list. Flag arguments begin with two dashes (`--foo`).
    /// </summary>
    private void ParseFlagArgs()
    {
        List<int> processedIndexes = [];

        foreach ((int index, string arg) in _argsList.Enumerate(IsFlag))
        {
            switch (arg)
            {
                case "--listen":
                    _args.Listen = true;
                    break;
                case "--verbose":
                    _args.Verbose = true;
                    break;
                case "--debug":
                    _args.Debug = _args.Verbose = Error.Debug = true;
                    break;
                case "--zero-io":
                    _args.PipeVariant = PipeType.Status;
                    break;
                case "--port":
                    _args.Port = GetPort(index, arg);
                    processedIndexes.Add(index + 1);
                    break;
                case "--exec":
                    _args.ExePath = GetExecutable(index, arg);
                    processedIndexes.Add(index + 1);
                    break;
                case "--text":
                    _args.Payload = GetTextPayload(index, arg);
                    processedIndexes.Add(index + 1);
                    break;
                case "--output":
                    _args.TransOpt = TransferOpt.Collect;
                    _args.FilePath = GetTransferPath(index, arg);
                    processedIndexes.Add(index + 1);
                    break;
                case "--send":
                    _args.TransOpt = TransferOpt.Transmit;
                    _args.FilePath = GetTransferPath(index, arg);
                    processedIndexes.Add(index + 1);
                    break;
                default:
                    Error.Handle(Except.UnknownArgs, arg, true);
                    break;
            }
            processedIndexes.Add(index);
        }

        RemoveProcessedArgs(processedIndexes);
    }

    /// <summary>
    ///  Remove processed command-line arguments according to the given indexes.
    /// </summary>
    private void RemoveProcessedArgs(IEnumerable<int> indexes)
    {
        int delta = 0;

        foreach (int index in indexes.Order())
        {
            _argsList.RemoveAt(index - delta++);
        }
    }

    /// <summary>
    ///  Parse the positional arguments in the underlying command-line argument
    ///  list. Positional arguments do not begin with `-` or `--`.
    /// </summary>
    private void ParsePositionalArgs()
    {
        switch (_argsList.Count)
        {
            case 0:   // Missing TARGET
            {
                if (!_args.Listen)
                {
                    Error.Handle(Except.RequiredArgs, "TARGET", true);
                }
                break;
            }
            case 1:   // Validate TARGET
            {
                if (_argsList[0].StartsWith('-'))
                {
                    Error.Handle(Except.UnknownArgs, _argsList[0], true);
                }
                Exception? ex = null;

                // Parse the connection IPv4 address
                if (IPAddress.TryParse(_argsList[0], out IPAddress? addr))
                {
                    _args.Address = addr;
                }
                else  // Resolve the hostname
                {
                    (_args.Address, ex) = Net.ResolveName(_argsList[0]);
                }
                _args.HostName = _argsList[0];

                if (_args.Address == IPAddress.Any)
                {
                    Error.Handle(Except.HostNotFound, _argsList[0], true, ex: ex);
                }
                break;
            }
            default:  // Unexpected arguments
            {
                string argsStr = _argsList.Join(", ");

                if (_argsList[0].StartsWithValue('-'))
                {
                    Error.Handle(Except.UnknownArgs, argsStr, true);
                }
                Error.Handle(Except.InvalidArgs, argsStr, true);
                break;
            }
        }
    }

    /// <summary>
    ///  Get the extended application usage information message.
    /// </summary>
    private string GetHelpMessage()
    {
        string helpMessage = $"""
            DotnetCat ({Repo})
            {Usage}{SysInfo.Eol}
            Remote command shell application{SysInfo.Eol}
            Positional Arguments:
              TARGET                    Remote or local IPv4 address{SysInfo.Eol}
            Optional Arguments:
              -h/-?,   --help           Show this help message and exit
              -v,      --verbose        Enable verbose console output
              -d,      --debug          Output verbose error information
              -l,      --listen         Listen for incoming connections
              -z,      --zero-io        Report connection status only
              -p PORT, --port PORT      Specify port to use for endpoint.
                                        (Default: 44444)
              -e EXEC, --exec EXEC      Executable process file path
              -o PATH, --output PATH    Receive file from remote host
              -s PATH, --send PATH      Send local file or folder
              -t DATA, --text DATA      Send string to remote host{SysInfo.Eol}
            Usage Examples:
              {_title} --listen --exec powershell.exe
              {_title} -d -p 44444 localhost
              {_title} -vo test.txt -p 2009 192.168.1.9{SysInfo.Eol}
            """;
        return helpMessage;
    }

    /// <summary>
    ///  Determine whether the given index is a valid index of an argument
    ///  in the underlying command-line argument list.
    /// </summary>
    private bool ValidIndex(int index) => index >= 0 && index < _argsList.Count;

    /// <summary>
    ///  Get the value of the argument located at the given index in
    ///  the underlying command-line argument list.
    /// </summary>
    private string ArgsValueAt(int index)
    {
        if (!ValidIndex(index))
        {
            Error.Handle(Except.NamedArgs, _argsList[index - 1], true);
        }
        return _argsList[index];
    }

    /// <summary>
    ///  Parse and validate the network port number argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private int GetPort(int index, string flag)
    {
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, flag, true);
        }
        string portStr = ArgsValueAt(index + 1);

        if (!int.TryParse(portStr, out int port) || !Net.IsValidPort(port))
        {
            Console.WriteLine(Usage);
            Error.Handle(Except.InvalidPort, portStr);
        }
        return port;
    }

    /// <summary>
    ///  Parse and validate the executable path argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private string? GetExecutable(int index, string flag)
    {
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, flag, true);
        }

        string exec = ArgsValueAt(index + 1);
        (string? path, bool exists) = FileSys.ExistsOnPath(exec);

        if (!exists)
        {
            Error.Handle(Except.ExePath, exec, true);
        }
        _args.PipeVariant = PipeType.Process;

        return path;
    }

    /// <summary>
    ///  Parse and validate the transfer file path argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private string GetTransferPath(int index, string flag)
    {
        if (_args.TransOpt is TransferOpt.None)
        {
            throw new InvalidOperationException("File transfer option must be set");
        }

        // No corresponding argument value
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, flag, true);
        }
        string? path = FileSys.ResolvePath(ArgsValueAt(index + 1));

        // File path resolution failure
        if (path.IsNullOrEmpty())
        {
            Error.Handle(Except.FilePath, path, true);
        }
        string? parentPath = FileSys.ParentPath(path);

        // Parent path must exist for both collection and transmission
        if (parentPath.IsNullOrEmpty() || !FileSys.DirectoryExists(parentPath))
        {
            Error.Handle(Except.DirectoryPath, parentPath, true);
        }

        // File must exist to be transmitted
        if (!FileSys.FileExists(path) && _args.TransOpt is TransferOpt.Transmit)
        {
            Error.Handle(Except.FilePath, path, true);
        }
        _args.PipeVariant = PipeType.File;

        return path;
    }

    /// <summary>
    ///  Parse and validate the arbitrary payload argument in the underlying
    ///  command-line argument list using the given flag or flag alias index.
    /// </summary>
    private string GetTextPayload(int index, string flag)
    {
        if (!ValidIndex(index + 1))
        {
            Error.Handle(Except.NamedArgs, flag, true);
        }
        string data = ArgsValueAt(index + 1);

        if (data.IsNullOrEmpty())
        {
            Error.Handle(Except.Payload, flag, true);
        }
        _args.PipeVariant = PipeType.Text;

        return data;
    }

    /// <summary>
    ///  Command-line flag alias argument regular expression.
    /// </summary>
    [GeneratedRegex("^-([?]|[A-Z]|[a-z])+$")]
    private static partial Regex AliasRegex();

    /// <summary>
    ///  Command-line flag argument regular expression.
    /// </summary>
    [GeneratedRegex("^--([A-Z]|[a-z])+$")]
    private static partial Regex FlagRegex();
}
