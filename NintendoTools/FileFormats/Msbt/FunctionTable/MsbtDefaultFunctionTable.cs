using System;
using System.Collections.Generic;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// Default implementation of a <see cref="IMsbtFunctionTable"/>.
/// Returns function name as fun_<c>hash</c> and argument data as single argument converted to hex string.
/// </summary>
public class MsbtDefaultFunctionTable : IMsbtFunctionTable
{
    /// <inheritdoc/>
    public void GetFunction(MsbtMessage message, uint hash, byte[] args, out string functionName, out IEnumerable<MsbtFunctionArgument> functionArgs)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        functionName = $"fun_{hash:X8}";
        var argList = new List<MsbtFunctionArgument>();
        if (args.Length > 0) argList.Add(new MsbtFunctionArgument("arg", args.ToHexString(true)));
        functionArgs = argList;
    }
}