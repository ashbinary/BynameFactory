using System;
using System.IO;
using NintendoTools.Utils;

namespace NintendoTools.Compression;

/// <summary>
/// An extension class for <see cref="ICompressor"/> types.
/// </summary>
public static class CompressorExtensions
{
    /// <summary>
    /// Compresses a byte array.
    /// </summary>
    /// <param name="compressor">The <see cref="ICompressor"/> instance to use.</param>
    /// <param name="data">The data to compress.</param>
    /// <returns>The compressed data.</returns>
    public static byte[] Compress(this ICompressor compressor, byte[] data)
    {
        if (compressor is null) throw new ArgumentNullException(nameof(compressor));
        if (data is null) throw new ArgumentNullException(nameof(data));

        var stream = new MemoryStream(data, false);
        var result = compressor.Compress(stream);
        return result.ToArray();
    }
}