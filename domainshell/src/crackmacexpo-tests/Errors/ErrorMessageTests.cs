using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Errors;

namespace DotnetCatTests.Errors;

/// <summary>
///  Unit tests for class <see cref="ErrorMessage"/>.
/// </summary>
[TestClass]
public class ErrorMessageTests
{
#region ConstructorTests
    /// <summary>
    ///  Assert that <see cref="ErrorMessage.Message"/> is updated
    ///  when the object is constructed with a valid input message.
    /// </summary>
    [DataTestMethod]
    [DataRow("test: %")]
    [DataRow("test: {}")]
    [DataRow("test: %, {}")]
    public void ErrorMessage_ValidMessage_SetsMessage(string expected)
    {
        ErrorMessage errorMsg = new(expected);
        string actual = errorMsg.Message;

        Assert.AreEqual(actual, expected, $"Expected property value: '{expected}'");
    }

    /// <summary>
    ///  Assert that an <c>ArgumentException</c> is thrown when the
    ///  object is constructed with a built input message.
    /// </summary>
    [DataTestMethod]
    [DataRow("test")]
    [DataRow("error")]
    [DataRow("test message")]
    public void ErrorMessage_BuiltMessage_ThrowsArgumentException(string msg)
    {
        Func<ErrorMessage> func = () => _ = new ErrorMessage(msg);
        Assert.ThrowsException<ArgumentException>(func);
    }

    /// <summary>
    ///  Assert that an <c>ArgumentNullException</c> is thrown when the
    ///  object is constructed with an empty or blank input message.
    /// </summary>
    [DataTestMethod]
    [DataRow("")]
    [DataRow("  ")]
    public void ErrorMessage_EmptyMessage_ThrowsArgumentNullException(string msg)
    {
        Func<ErrorMessage> func = () => _ = new ErrorMessage(msg);
        Assert.ThrowsException<ArgumentNullException>(func);
    }

    /// <summary>
    ///  Assert that an <c>ArgumentNullException</c> is thrown when
    ///  the object is constructed with a null input message.
    /// </summary>
    [TestMethod]
    public void ErrorMessage_NullMessage_ThrowsArgumentNullException()
    {
        string? msg = null;

    #nullable disable
        Func<ErrorMessage> func = () => _ = new ErrorMessage(msg);
    #nullable enable

        Assert.ThrowsException<ArgumentNullException>(func);
    }
#endregion // ConstructorTests

#region MethodTests
    /// <summary>
    ///  Assert that <see cref="ErrorMessage.Message"/> is correctly
    ///  updated when the input argument is valid.
    /// </summary>
    [DataTestMethod]
    [DataRow("test: '%'", "", "test: ''")]
    [DataRow("test: '%'", " ", "test: ' '")]
    [DataRow("test: '{}'", null, "test: ''")]
    [DataRow("test: '%', '{}'", "data", "test: 'data', 'data'")]
    public void Build_ValidArg_BuildsMessage(string msg, string? arg, string expected)
    {
        ErrorMessage errorMsg = new(msg);

        _ = errorMsg.Build(arg);
        string actual = errorMsg.Message;

        Assert.AreEqual(actual, expected);
    }

    /// <summary>
    ///  Assert that the correctly built error message value is
    ///  returned when the input argument is valid.
    /// </summary>
    [DataTestMethod]
    [DataRow("test: '%'", "", "test: ''")]
    [DataRow("test: '%'", " ", "test: ' '")]
    [DataRow("test: '{}'", null, "test: ''")]
    [DataRow("test: '%', '{}'", "data", "test: 'data', 'data'")]
    public void Build_ValidArg_ReturnsExpected(string msg, string? arg, string expected)
    {
        ErrorMessage errorMsg = new(msg);
        string actual = errorMsg.Build(arg);

        Assert.AreEqual(actual, expected);
    }

    /// <summary>
    ///  Assert that an <c>InvalidOperationException</c> is thrown when
    ///  <see cref="ErrorMessage.Message"/> is already built.
    /// </summary>
    [DataTestMethod]
    [DataRow("test: '%'", "")]
    [DataRow("test: '%'", " ")]
    [DataRow("test: '{}'", null)]
    [DataRow("test: '%', '{}'", "data")]
    public void Build_MsgBuilt_ThrowsInvalidOperationException(string msg, string? arg)
    {
        ErrorMessage errorMsg = new(msg);
        _ = errorMsg.Build(arg);

        Func<string> func = () => _ = errorMsg.Build(arg);

        Assert.ThrowsException<InvalidOperationException>(func);
    }
#endregion // MethodTests
}
