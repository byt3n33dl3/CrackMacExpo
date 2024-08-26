using System;
using System.Diagnostics.CodeAnalysis;
using DotnetCat.IO;
using DotnetCat.Network;
using DotnetCat.Utils;

namespace DotnetCat.Errors;

/// <summary>
///  Error and exception utility class.
/// </summary>
internal static class Error
{
    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Error() => Debug = false;

    /// <summary>
    ///  Enable verbose exceptions.
    /// </summary>
    public static bool Debug { get; set; }

    /// <summary>
    ///  Handle user-defined exceptions related to DotnetCat and exit.
    /// </summary>
    [DoesNotReturn]
    public static void Handle(Except exType, string? arg, Exception? ex = default)
    {
        Handle(exType, arg, false, ex);
    }

    /// <summary>
    ///  Handle user-defined exceptions related to DotnetCat and exit.
    /// </summary>
    [DoesNotReturn]
    public static void Handle(Except exType,
                              string? arg,
                              Exception? ex,
                              Level level = default) {

        Handle(exType, arg, false, ex, level);
    }

    /// <summary>
    ///  Handle user-defined exceptions related to DotnetCat and exit.
    /// </summary>
    [DoesNotReturn]
    public static void Handle(Except exType,
                              string? arg,
                              bool showUsage,
                              Exception? ex = default,
                              Level level = default) {

        _ = arg ?? throw new ArgumentNullException(nameof(arg));

        // Display program usage
        if (showUsage)
        {
            Console.WriteLine(Parser.Usage);
        }
        ErrorMessage errorMsg = MakeErrorMessage(exType, arg);

        // Print warning/error message
        if (level is Level.Warn)
        {
            Style.Warn(errorMsg.Message);
        }
        else
        {
            Style.Error(errorMsg.Message);
        }

        // Print debug information
        if (Debug && ex is not null)
        {
            if (ex is AggregateException aggregateEx)
            {
                ex = Net.GetException(aggregateEx) ?? ex;
            }
            string header = $"----[ {ex?.GetType().FullName} ]----";

            Console.WriteLine($"""
                {header}
                {ex?.ToString()}
                {new string('-', header.Length)}
                """
            );
        }

        Console.WriteLine();
        Environment.Exit(1);
    }

    /// <summary>
    ///  Get a new error message that corresponds to the given
    ///  exception enumeration type.
    /// </summary>
    private static ErrorMessage MakeErrorMessage(Except exType,
                                                 string? arg = default) {
        ErrorMessage message = new(exType switch
        {
            Except.AddressInUse       => "The endpoint is already in use: %",
            Except.ArgsCombo          => "Invalid argument combination: %",
            Except.ConnectionAborted  => "Local software aborted connection to %",
            Except.ConnectionRefused  => "Connection was actively refused by %",
            Except.ConnectionReset    => "Connection was reset by %",
            Except.DirectoryPath      => "Unable to locate parent directory: '%'",
            Except.EmptyPath          => "A value is required for option(s): %",
            Except.ExePath            => "Unable to locate executable file: '%'",
            Except.ExeProcess         => "Unable to launch executable process: %",
            Except.FilePath           => "Unable to locate file path: '%'",
            Except.HostNotFound       => "Unable to resolve hostname: '%'",
            Except.HostUnreachable    => "Unable to reach host %",
            Except.InvalidArgs        => "Unable to validate argument(s): %",
            Except.InvalidPort        => "'%' is not a valid port number",
            Except.NamedArgs          => "Missing value for named argument(s): %",
            Except.NetworkDown        => "The network is down: %",
            Except.NetworkReset       => "Connection to % was lost in network reset",
            Except.NetworkUnreachable => "The network is unreachable: %",
            Except.Payload            => "Invalid payload data: %",
            Except.RequiredArgs       => "Missing required argument(s): %",
            Except.SocketError        => "Unspecified socket error occurred: %",
            Except.StringEol          => "Missing EOL in argument(s): %",
            Except.TimedOut           => "Socket timeout occurred: %",
            Except.UnknownArgs        => "One or more unknown arguments: %",
            Except.Unhandled or _     => "Unhandled exception occurred: %"
        });

        if (!arg.IsNullOrEmpty())
        {
            message.Build(arg);
        }
        return message;
    }
}
