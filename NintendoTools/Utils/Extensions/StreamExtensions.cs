using System.IO;

namespace NintendoTools.Utils;

internal static class StreamExtensions
{
    //converts a stream to byte-array
    public static byte[] ToArray(this Stream stream)
    {
        using var memoryStream = stream as MemoryStream ?? new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}