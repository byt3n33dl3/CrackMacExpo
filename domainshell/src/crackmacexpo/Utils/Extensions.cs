using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using DotnetCat.Shell;

namespace DotnetCat.Utils;

/// <summary>
///  Utility class for user-defined extension methods.
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///  Determine whether a string is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return str is null || str.Trim().Length == 0;
    }

    /// <summary>
    ///  Determine whether a collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? values)
    {
        return values is null || !values.Any();
    }

    /// <summary>
    ///  Determine whether a string ends with the given character.
    /// </summary>
    public static bool EndsWithValue(this string? str, char value)
    {
        return str?.EndsWith(value) ?? false;
    }

    /// <summary>
    ///  Determine whether a string ends with the given substring.
    /// </summary>
    public static bool EndsWithValue(this string? str, string? value)
    {
        bool endsWith = false;

        if (str is not null && value is not null)
        {
            endsWith = str.EndsWith(value);
        }
        return endsWith;
    }

    /// <summary>
    ///  Determine whether a string starts with the given character.
    /// </summary>
    public static bool StartsWithValue(this string? str, char value)
    {
        return str?.StartsWith(value) ?? false;
    }

    /// <summary>
    ///  Determine whether a string starts with the given substring.
    /// </summary>
    public static bool StartsWithValue(this string? str, string? value)
    {
        bool startsWith = false;

        if (str is not null && value is not null)
        {
            startsWith = str.StartsWith(value);
        }
        return startsWith;
    }

    /// <summary>
    ///  Determine whether a string is equal to another
    ///  string when all string casing is ignored.
    /// </summary>
    public static bool NoCaseEquals(this string? str, string? value)
    {
        return str?.ToLower() == value?.ToLower();
    }

    /// <summary>
    ///  Enumerate the elements of a collection as a collection of
    ///  tuples containing each element's index and value.
    /// </summary>
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T>? values)
    {
        IEnumerable<(int, T)> results = [];

        if (!values.IsNullOrEmpty())
        {
            results = values.Select((T v, int i) => (i, v));
        }
        return results;
    }

    /// <summary>
    ///  Enumerate the elements of a collection as a collection of tuples
    ///  containing each element's index and value, then filter the results.
    /// </summary>
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T>? values,
                                                     Func<(int, T), bool> filter) {
        return values.Enumerate().Where(filter);
    }

    /// <summary>
    ///  Join each element of a collection separated by the given delimiter.
    /// </summary>
    public static string Join<T>(this IEnumerable<T>? values,
                                 string? delim = default) {

        if (values.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(values));
        }
        return string.Join(delim, values);
    }

    /// <summary>
    ///  Join each element of a collection separated by the default system EOL.
    /// </summary>
    public static string JoinLines<T>(this IEnumerable<T>? values)
    {
        return Join(values, SysInfo.Eol);
    }

    /// <summary>
    ///  Normalize all the newline substrings in a string builder.
    /// </summary>
    public static StringBuilder ReplaceLineEndings(this StringBuilder sb)
    {
        return new StringBuilder(sb.ToString().ReplaceLineEndings());
    }
}
