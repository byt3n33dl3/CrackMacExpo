using System;
using System.IO;
using DotnetCat.Utils;

namespace DotnetCat.IO;

/// <summary>
///  Application console output style utility class.
/// </summary>
internal static class Style
{
    /// <summary>
    ///  Write the given error message to the standard error stream.
    /// </summary>
    public static void Error(string msg) => Status(Level.Error, msg);

    /// <summary>
    ///  Write the given completion status message to the standard output stream.
    /// </summary>
    public static void Output(string msg) => Status(Level.Output, msg);

    /// <summary>
    ///  Write the given informational message to the standard output stream.
    /// </summary>
    public static void Info(string msg) => Status(Level.Info, msg);

    /// <summary>
    ///  Write the given warning status message to the standard error stream.
    /// </summary>
    public static void Warn(string msg) => Status(Level.Warn, msg);

    /// <summary>
    ///  Write the given status message to a standard console stream
    ///  based on the given console output level.
    /// <summary>
    private static void Status(Level level, string msg)
    {
        if (msg.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(msg));
        }

        using TextWriter stream = level switch
        {
            Level.Error or Level.Warn       => Console.Error,
            Level.Info or Level.Output or _ => Console.Out
        };

        Status status = new(StatusColor(level), level);
        string symbol = Sequence.GetColorStr(status.Symbol, status.Color);

        stream.WriteLine($"{symbol} {msg}");
    }

    /// <summary>
    ///  Get the console color associated with the given output level.
    /// </summary>
    private static ConsoleColor StatusColor(Level level) => level switch
    {
        Level.Error     => ConsoleColor.Red,
        Level.Output    => ConsoleColor.Green,
        Level.Warn      => ConsoleColor.Yellow,
        Level.Info or _ => ConsoleColor.Cyan
    };
}
