using System.Collections.Generic;

namespace NintendoTools.Utils;

internal static class StackExtensions
{
    public static bool TryPeek<T>(this Stack<T> stack, out T? value)
    {
        if (stack.Count > 0)
        {
            value = stack.Peek();
            return true;
        }

        value = default;
        return false;
    }
}