using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Shell;

namespace DotnetCatTests.Shell;

/// <summary>
///  Unit tests for utility class <see cref="Command"/>.
/// </summary>
[TestClass]
public class CommandTests
{
#region MethodTests
    /// <summary>
    ///  Assert that a valid input environment variable name returns the
    ///  value of the corresponding variable in the local system.
    /// </summary>
    [DataTestMethod]
    [DataRow("PATH")]
#if WINDOWS
    [DataRow("USERNAME")]
    [DataRow("USERPROFILE")]
#elif LINUX
    [DataRow("HOME")]
    [DataRow("USER")]
#endif // WINDOWS
    public void GetEnvVariable_ValidEnvVariableName_ReturnsExpected(string name)
    {
        string? expected = Environment.GetEnvironmentVariable(name);
        string? actual = Command.GetEnvVariable(name);

        Assert.AreEqual(actual, expected, $"Incorrect value for variable '{name}'");
    }

    /// <summary>
    ///  Assert that an invalid input environment variable name returns null.
    /// </summary>
    [DataTestMethod]
    [DataRow("DotnetCatTest_Test")]
    [DataRow("DotnetCatTest_Data")]
    public void GetEnvVariable_InvalidEnvVariableName_ReturnsNull(string name)
    {
        string? actual = Command.GetEnvVariable(name);
        Assert.IsNull(actual, $"Value for variable '{name}' should be null");
    }

    /// <summary>
    ///  Assert that a null input environment variable name causes an
    ///  <c>ArgumentNullException</c> to be thrown.
    /// </summary>
    [TestMethod]
    public void GetEnvVariable_NullEnvVariableName_ThrowsArgumentNullException()
    {
        string? name = null;

    #nullable disable
        Func<string> func = () => Command.GetEnvVariable(name);
    #nullable enable

        Assert.ThrowsException<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that a non-null input shell name returns a
    ///  new <c>ProcessStartInfo</c> object.
    /// </summary>
    [DataTestMethod]
    [DataRow("test")]
    [DataRow("data.exe")]
    public void GetExeStartInfo_NonNullShell_ReturnsNewStartInfo(string shell)
    {
        ProcessStartInfo actual = Command.GetExeStartInfo(shell);
        Assert.IsNotNull(actual, "Resulting startup information should not be null");
    }

    /// <summary>
    ///  Assert that a null input shell name causes an
    ///  <c>ArgumentNullException</c> to be thrown.
    /// </summary>
    [TestMethod]
    public void GetExeStartInfo_NullShell_ThrowsArgumentNullException()
    {
        string? shell = null;
        Func<ProcessStartInfo> func = () => Command.GetExeStartInfo(shell);

        Assert.ThrowsException<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that a valid input clear-screen command name returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("clear")]
    [DataRow("cls\n")]
    [DataRow("Clear-Host")]
    public void IsClearCmd_ValidCommand_ReturnsTrue(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsTrue(actual, $"'{command}' should be a clear-screen command");
    }

    /// <summary>
    ///  Assert that an invalid input clear-screen command name returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow("sudo")]
    [DataRow("cat\n")]
    [DataRow("Get-Location")]
    public void IsClearCmd_InvalidCommand_ReturnsFalse(string command)
    {
        bool actual = Command.IsClearCmd(command);
        Assert.IsFalse(actual, $"'{command}' should not be a clear-screen command");
    }
#endregion // MethodTests
}
