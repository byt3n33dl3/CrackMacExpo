namespace DotnetCat.IO.Pipelines;

/// <summary>
///  Socket pipeline enumeration type.
/// </summary>
internal enum PipeType : byte
{
    Stream,   // Standard console stream socket pipeline
    File,     // File transfer socket pipeline
    Process,  // Executable process socket pipeline
    Status,   // Connection status socket pipeline
    Text      // Text transmission socket pipeline
}
