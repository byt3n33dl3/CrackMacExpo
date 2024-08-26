using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetCat.Utils;

namespace DotnetCatTests.Utils;

/// <summary>
///  Unit tests for class <see cref="Parser"/>.
/// </summary>
[TestClass]
public class ParserTests
{
#region MethodTests
    /// <summary>
    ///  Assert that an input command-line argument array containing a
    ///  help flag or flag alias (`-?`, `-h`, `--help`) returns true.
    /// </summary>
    [DataTestMethod]
    [DataRow("-?")]
    [DataRow("-h")]
    [DataRow("--help")]
    [DataRow("-d?", "--listen")]
    [DataRow("-hz", "--debug")]
    [DataRow("-v", "--help")]
    [DataRow("-vp", "22", "-?", "localhost")]
    [DataRow("--exec", "pwsh.exe", "-h", "localhost")]
    [DataRow("--listen", "--help", "localhost")]
    public void NeedsHelp_HelpFlag_ReturnsTrue(params string[] args)
    {
        bool actual = Parser.NeedsHelp(args);
        Assert.IsTrue(actual, "Expected a help flag or flag alias");
    }

    /// <summary>
    ///  Assert that an input command-line argument array not containing
    ///  a help flag or flag alias (`-?`, `-h`, `--help`) returns false.
    /// </summary>
    [DataTestMethod]
    [DataRow("-vp", "22", "-e", "pwsh.exe")]
    [DataRow("--debug", "--send", "~/data.txt", "localhost")]
    [DataRow("--listen", "-do", "~/recv_data.txt")]
    public void NeedsHelp_NoHelpFlag_ReturnsFalse(params string[] args)
    {
        bool actual = Parser.NeedsHelp(args);
        Assert.IsFalse(actual, "Did not expect a help flag or flag alias");
    }
#endregion // MethodTests
}
