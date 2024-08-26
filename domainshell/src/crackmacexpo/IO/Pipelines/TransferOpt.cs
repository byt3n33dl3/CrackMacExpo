namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Socket pipeline file transfer option enumeration type.
/// </summary>
internal enum TransferOpt : byte
{
    None,      // No redirection
    Collect,   // Redirect source stream data to a file stream.
    Transmit   // Redirect file stream data to a destination stream.
}
