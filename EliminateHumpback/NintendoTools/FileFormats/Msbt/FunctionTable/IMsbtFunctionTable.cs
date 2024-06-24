using System.Collections.Generic;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// An interface for the MSBT function lookup during message formatting.
/// </summary>
public interface IMsbtFunctionTable
{
    /// <summary>
    /// Gets the name and argument list of a MSBT function.
    /// </summary>
    /// <param name="message">The <see cref="MsbtMessage"/> object.</param>
    /// <param name="hash">The hash-code of the function.</param>
    /// <param name="args">The argument data.</param>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="functionArgs">A list of function arguments.</param>
    void GetFunction(MsbtMessage message, uint hash, byte[] args, out string functionName, out IEnumerable<MsbtFunctionArgument> functionArgs);
}