using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// A class holding information about a MSBT message.
/// </summary>
public class MsbtMessage
{
    #region private members
    private static readonly IMsbtFunctionTable DefaultTable = new MsbtDefaultFunctionTable();
    private static readonly IMsbtFormatProvider DefaultFormatProvider = new MsbtDefaultFormatProvider();
    #endregion

    #region public properties
    /// <summary>
    /// The label of the message.
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// Additional attribute data of the message.
    /// </summary>
    public byte[]? Attribute { get; set; }

    /// <summary>
    /// The message text.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// A list of function hashes and their values.
    /// </summary>
    public IList<Tuple<uint, byte[]>> Functions { get; set; } = null!;

    /// <summary>
    /// The language of the message.
    /// This property is not set during parsing and must be set manually.
    /// If set, <see cref="IMsbtFunctionTable"/> and <see cref="IMsbtFormatProvider"/> implementation can use it to further improve parsing.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The encoding used for parsing the message.
    /// </summary>
    public Encoding Encoding { get; set; } = null!;
    #endregion

    #region public methods
    /// <inheritdoc/>
    public override string ToString() => Text;

    /// <summary>
    /// Converts the message text to a clean string. All function call templates are removed.
    /// </summary>
    /// <returns>A cleaned string.</returns>
    public string ToCleanString() => Regex.Replace(Text, @"{{\d+}}", string.Empty, RegexOptions.Compiled);

    /// <summary>
    /// Converts the message text, adding function call declarations and values.
    /// Uses instances of the <see cref="MsbtDefaultFunctionTable"/> and <see cref="MsbtDefaultFormatProvider"/> classes to convert and format.
    /// </summary>
    /// <returns>A converted string.</returns>
    public string ToCompiledString() => ToCompiledString(DefaultTable, DefaultFormatProvider);

    /// <summary>
    /// Converts the message text, adding function call declarations and values.
    /// </summary>
    /// <param name="table">The function table to use for function lookup.</param>
    /// <param name="formatProvider">The format provider to use for string formatting.</param>
    /// <returns>A converted string.</returns>
    public string ToCompiledString(IMsbtFunctionTable table, IMsbtFormatProvider formatProvider)
    {
        if (table is null) throw new ArgumentNullException(nameof(table));
        if (formatProvider is null) throw new ArgumentNullException(nameof(formatProvider));

        var result = new StringBuilder(formatProvider.FormatMessage(this, Text));

        for (var i = 0; i < Functions.Count; ++i)
        {
            var (hash, args) = Functions[i];
            table.GetFunction(this, hash, args, out var functionName, out var functionArgs);
            result = result.Replace("{{" + i + "}}", formatProvider.FormatFunction(this, functionName, functionArgs));
        }

        return result.ToString();
    }
    #endregion
}