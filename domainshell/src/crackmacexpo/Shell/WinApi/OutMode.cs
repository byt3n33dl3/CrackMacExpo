#if WINDOWS
using System;
using DWORD = System.UInt32;

namespace DotnetCat.Shell.WinApi;

/// <summary>
///  Windows API console output mode enumeration type flags.
/// </summary>
[Flags]
internal enum OutMode : DWORD
{
    UNKNOWN_OUTPUT_MODE                = 0x0000,
    ENABLE_PROCESSED_OUTPUT            = 0x0001,
    ENABLE_WRAP_AT_EOL_OUTPUT          = 0x0002,
    ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
    DISABLE_NEWLINE_AUTO_RETURN        = 0x0008,
    ENABLE_LVB_GRID_WORLDWIDE          = 0x0010
}

#endif // WINDOWS
