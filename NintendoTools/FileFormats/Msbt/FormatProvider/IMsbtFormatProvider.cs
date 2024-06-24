using System.Collections.Generic;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// An interface for the format provider of MSBT messages and functions.
/// </summary>
public interface IMsbtFormatProvider
{
    /// <summary>
    /// Formats the raw text of a <see cref="MsbtMessage"/>.
    /// </summary>
    /// <param name="message">The <see cref="MsbtMessage"/> object.</param>
    /// <param name="rawText">The raw message text.</param>
    /// <returns>A formatted string representing a <see cref="MsbtMessage"/>.</returns>
    string FormatMessage(MsbtMessage message, string rawText);

    /// <summary>
    /// Formats a MSBT function and its arguments.
    /// </summary>
    /// <param name="message">The <see cref="MsbtMessage"/> object.</param>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="arguments">The list of function arguments.</param>
    /// <returns>A formatted string representing a MSBT function.</returns>
    string FormatFunction(MsbtMessage message, string functionName, IEnumerable<MsbtFunctionArgument> arguments);
}