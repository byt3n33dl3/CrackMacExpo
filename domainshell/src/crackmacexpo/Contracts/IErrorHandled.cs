using System;
using System.Diagnostics.CodeAnalysis;
using DotnetCat.Errors;
using DotnetCat.IO;

namespace DotnetCat.Contracts;

/// <summary>
///  Interface for enforcing mechanisms to release unmanaged resources
///  and to exit the application (when an error occurs).
/// </summary>
internal interface IErrorHandled : IConnectable
{
    /// <summary>
    ///  Dispose of all unmanaged resources and handle the given error.
    /// </summary>
    [DoesNotReturn]
    void PipeError(Except type,
                   string arg,
                   Exception? ex = default,
                   Level level = default);
}
