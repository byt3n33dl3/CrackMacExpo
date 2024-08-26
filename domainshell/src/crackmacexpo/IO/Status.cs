using System;

namespace DotnetCat.IO;

/// <summary>
///  Console status message configuration information.
/// </summary>
internal class Status
{
    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public Status(ConsoleColor color, Level level)
    {
        Level = level;
        EscSequence = Sequence.GetColorStr(Color = color);

        Symbol = level switch
        {
            Level.Error     => "[x]",
            Level.Output    => "[+]",
            Level.Warn      => "[!]",
            Level.Info or _ => "[*]"
        };
    }

    /// <summary>
    ///  Status symbol color.
    /// </summary>
    public ConsoleColor Color { get; }

    /// <summary>
    ///  Status output level.
    /// </summary>
    public Level Level { get; }

    /// <summary>
    ///  Foreground color sequence.
    /// </summary>
    public string EscSequence { get; }

    /// <summary>
    ///  Status prefix symbol.
    /// </summary>
    public string Symbol { get; }
}
