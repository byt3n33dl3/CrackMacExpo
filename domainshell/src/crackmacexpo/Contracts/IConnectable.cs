using System;

namespace DotnetCat.Contracts;

/// <summary>
///  Interface for enforcing mechanisms to connect and release unmanaged object resources.
/// </summary>
internal interface IConnectable : IDisposable
{
    /// <summary>
    ///  Connect the unmanaged resource(s).
    /// </summary>
    void Connect();
}
