using System;
using System.Collections.Generic;
using System.Text;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// An XML implementation of a <see cref="IMsbtFormatProvider"/>.<br/>
/// Message format:<br/>
/// - Encodes non-XML compliant characters.<br/>
/// Function format:<br/>
/// - Empty function name: <c>string.Empty</c><br/>
/// - Without arguments: &lt;<c>functionName</c>/&gt;<br/>
/// - With arguments: &lt;<c>functionName</c> <c>arg.Name</c>="<c>arg.Value</c>"/&gt;
/// </summary>
public class MsbtXmlFormatProvider : IMsbtFormatProvider
{
    #region private members
    private static readonly Dictionary<string, string> XmlChars = new()
    {
        {"\"", "&quot;"},
        {"&", "&amp;"},
        {"'", "&apos;"},
        {"<", "&lt;"},
        {">", "&gt;"}
    };
    #endregion

    #region IMsbtFormatProvider interface
    /// <inheritdoc />
    public string FormatMessage(MsbtMessage message, string rawText)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (rawText is null) throw new ArgumentNullException(nameof(rawText));

        foreach (var val in XmlChars)
        {
            rawText = rawText.Replace(val.Key, val.Value);
        }
        return rawText;
    }

    /// <inheritdoc />
    public string FormatFunction(MsbtMessage message, string functionName, IEnumerable<MsbtFunctionArgument> arguments)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrEmpty(functionName)) return string.Empty;

        var sb = new StringBuilder();
        sb.Append('<').Append(functionName);

        foreach (var arg in arguments)
        {
            sb.Append(' ').Append(arg.Name).Append("=\"").Append(arg.Value).Append('\"');
        }

        sb.Append("/>");
        return sb.ToString();
    }
    #endregion
}