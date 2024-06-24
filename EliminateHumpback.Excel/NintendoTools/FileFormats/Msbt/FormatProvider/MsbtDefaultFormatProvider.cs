﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// Default implementation of a <see cref="IMsbtFormatProvider"/>.<br/>
/// Message format: <c>message.Label</c> : <c>formattedText</c><br/>
/// Function format:<br/>
/// - Empty function name: <c>string.Empty</c><br/>
/// - Without arguments: {{<c>functionName</c>}}<br/>
/// - With arguments: {{<c>functionName</c> <c>arg.Name</c>="<c>arg.Value</c>"}}
/// </summary>
public class MsbtDefaultFormatProvider : IMsbtFormatProvider
{
    #region IMsbtFormatProvider interface
    /// <inheritdoc />
    public string FormatMessage(MsbtMessage message, string rawText) => rawText ?? throw new ArgumentNullException(nameof(rawText));

    /// <inheritdoc />
    public string FormatFunction(MsbtMessage message, string functionName, IEnumerable<MsbtFunctionArgument> arguments)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrEmpty(functionName)) return string.Empty;

        var sb = new StringBuilder();
        sb.Append("{{").Append(functionName);

        foreach (var arg in arguments)
        {
            sb.Append(' ').Append(arg.Name).Append("=\"").Append(arg.Value).Append('\"');
        }

        sb.Append("}}");
        return sb.ToString();
    }
    #endregion
}