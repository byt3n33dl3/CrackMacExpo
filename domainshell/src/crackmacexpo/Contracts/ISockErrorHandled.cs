using System;
using System.Diagnostics.CodeAnalysis;
using DotnetCat.Errors;
using DotnetCat.IO;
using DotnetCat.Network;

namespace DotnetCat.Contracts;

/// <summary>
///  Interface for enforcing mechanisms to release unmanaged socket
///  resources and to exit the application (when an error occurs).
/// </summary>
internal interface ISockErrorHandled : IErrorHandled
{
    /// <summary>
    ///  Dispose of all unmanaged socket resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    void PipeError(Except type,
                   HostEndPoint target,
                   Exception? ex = default,
                   Level level = default);
}
