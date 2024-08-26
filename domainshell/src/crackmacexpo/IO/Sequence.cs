using System;
using DotnetCat.Shell.WinApi;
using DotnetCat.Utils;

namespace DotnetCat.IO;

/// <summary>
///  Virtual terminal control sequence utility class.
/// </summary>
internal static class Sequence
{
    private const string ESC = "\x1b";

    private const string CLEAR = $"{ESC}[H{ESC}[2J{ESC}[3J";

    private const string RESET = $"{ESC}[0m";

    /// <summary>
    ///  Initialize the static class members.
    /// </summary>
    static Sequence() => ConsoleApi.EnableVirtualTerm();

    /// <summary>
    ///  Clear the current console screen buffer.
    /// </summary>
    public static void ClearScreen() => Console.Write(CLEAR);

    /// <summary>
    ///  Get the ANSI foreground color SGR control sequence that
    ///  corresponds to the given console color.
    /// </summary>
    public static string GetColorStr(ConsoleColor color) => GetColorSequence(color);

    /// <summary>
    ///  Style the given message using ANSI SGR control sequences so the
    ///  foreground color is changed and terminated by a reset sequence.
    /// </summary>
    public static string GetColorStr(string msg, ConsoleColor color)
    {
        if (msg.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(msg));
        }
        return $"{GetColorSequence(color)}{msg}{RESET}";
    }

    /// <summary>
    ///  Get the ANSI foreground color SGR control sequence that
    ///  corresponds to the given console color.
    /// </summary>
    private static string GetColorSequence(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black       => $"{ESC}[0;30m",
            ConsoleColor.DarkBlue    => $"{ESC}[0;34m",
            ConsoleColor.DarkGreen   => $"{ESC}[0;32m",
            ConsoleColor.DarkCyan    => $"{ESC}[0;36m",
            ConsoleColor.DarkRed     => $"{ESC}[0;31m",
            ConsoleColor.DarkMagenta => $"{ESC}[0;35m",
            ConsoleColor.DarkYellow  => $"{ESC}[0;33m",
            ConsoleColor.Gray        => $"{ESC}[0;37m",
            ConsoleColor.DarkGray    => $"{ESC}[1;30m",
            ConsoleColor.Blue        => $"{ESC}[1;34m",
            ConsoleColor.Green       => $"{ESC}[38;2;166;226;46m",
            ConsoleColor.Cyan        => $"{ESC}[38;2;0;255;255m",
            ConsoleColor.Red         => $"{ESC}[38;2;246;0;0m",
            ConsoleColor.Magenta     => $"{ESC}[1;35m",
            ConsoleColor.Yellow      => $"{ESC}[38;2;250;230;39m",
            ConsoleColor.White       => $"{ESC}[1;37m",
            _                        => RESET
        };
    }
}
