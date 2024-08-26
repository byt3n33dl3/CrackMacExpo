using System;
using System.Diagnostics;
using System.Linq;
using DotnetCat.IO;
using DotnetCat.Utils;

namespace DotnetCat.Shell;

/// <summary>
///  Command shell and executable process utility class.
/// </summary>
internal static class Command
{
    private static readonly string[] _clsCommands;  // Clear screen commands

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Command() => _clsCommands = ["clear", "clear-host", "cls"];

    /// <summary>
    ///  Get the value of the given environment variable.
    /// </summary>
    public static string? GetEnvVariable(string varName)
    {
        return Environment.GetEnvironmentVariable(varName);
    }

    /// <summary>
    ///  Get process startup information to initialize the given command shell.
    /// </summary>
    public static ProcessStartInfo GetExeStartInfo(string? shell)
    {
        _ = shell ?? throw new ArgumentNullException(nameof(shell));

        ProcessStartInfo startInfo = new(shell)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = FileSys.UserProfile
        };

        // Profile loading only supported on Windows
        if (OperatingSystem.IsWindows())
        {
            startInfo.LoadUserProfile = true;
        }
        return startInfo;
    }

    /// <summary>
    ///  Determine whether the given data contains a clear command.
    /// </summary>
    public static bool IsClearCmd(string command)
    {
        bool clearCommand = false;

        if (!command.IsNullOrEmpty())
        {
            clearCommand = _clsCommands.Contains(ParseCommand(command));
        }
        return clearCommand;
    }

    /// <summary>
    ///  Parse a shell command from the raw command data.
    /// </summary>
    private static string ParseCommand(string data)
    {
        data = data.ReplaceLineEndings(string.Empty).Trim();
        return data.ToLower().Split(SysInfo.Eol)[0];
    }
}
