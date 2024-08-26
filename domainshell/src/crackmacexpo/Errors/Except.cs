namespace DotnetCat.Errors;

/// <summary>
///  DotnetCat error and exception enumeration type.
/// </summary>
internal enum Except : byte
{
    Unhandled,           // Unhandled exception occurred
    AddressInUse,        // Local endpoint is already in use
    ArgsCombo,           // Invalid argument combination
    ConnectionAborted,   // Socket connection was aborted by software
    ConnectionRefused,   // Socket connection was refused
    ConnectionReset,     // Socket connection was reset
    DirectoryPath,       // Invalid directory file path
    EmptyPath,           // Empty directory file path
    ExePath,             // Invalid executable file path
    ExeProcess,          // Executable process error(s)
    FilePath,            // Invalid file path received (general)
    HostNotFound,        // Invalid IP address or DNS name resolution failure
    HostUnreachable,     // Unable to find route to specified host
    InvalidArgs,         // Invalid command-line argument(s)
    InvalidPort,         // Invalid network port(s) number
    NamedArgs,           // Invalid named (optional) argument(s)
    NetworkDown,         // Local network is down
    NetworkReset,        // Connection dropped in network reset
    NetworkUnreachable,  // Attempted to communicate with unreachable network
    Payload,             // Invalid string payload argument
    RequiredArgs,        // Invalid required (positional) argument(s)
    SocketError,         // Unspecified socket error
    StringEol,           // Unable to determine string EOL (end quote)
    TimedOut,            // Socket connection timeout occurred
    UnknownArgs          // Unknown or unexpected argument(s)
}
