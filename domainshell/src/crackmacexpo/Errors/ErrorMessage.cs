using System;
using DotnetCat.Utils;

namespace DotnetCat.Errors;

/// <summary>
///  Custom error message specifically related to DotnetCat.
/// </summary>
internal class ErrorMessage
{
    private string? _message;  // Error message

    /// <summary>
    ///  Initialize the object.
    /// </summary>
    public ErrorMessage(string msg) => Message = msg;

    /// <summary>
    ///  Error message string.
    /// </summary>
    public string Message
    {
        get => _message ??= string.Empty;
        private set
        {
            if (value.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (MsgBuilt(value))
            {
                throw new ArgumentException("Message already built", nameof(value));
            }
            _message = value;
        }
    }

    /// <summary>
    ///  Interpolate the given argument in the underlying message string.
    /// </summary>
    public string Build(string? arg)
    {
        if (MsgBuilt())
        {
            throw new InvalidOperationException("Underlying message already built");
        }
        return _message = Message.Replace("%", arg).Replace("{}", arg);
    }

    /// <summary>
    ///  Determine whether the given message string contains
    ///  any format specifier substrings (`%`, `{}`).
    /// </summary>
    private static bool MsgBuilt(string msg)
    {
        return !msg.Contains('%') && !msg.Contains("{}");
    }

    /// <summary>
    ///  Determine whether the underlying message string contains
    ///  any format specifier substrings (`%`, `{}`).
    /// </summary>
    private bool MsgBuilt() => MsgBuilt(Message);
}
