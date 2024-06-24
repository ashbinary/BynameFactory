using System.IO;

namespace NintendoTools.Compression;

/// <summary>
/// The interface for decompressor types.
/// </summary>
public interface ICompressor
{
    /// <summary>
    /// Compresses a stream.
    /// </summary>
    /// <param name="fileStream">The stream to compress.</param>
    /// <returns>A compressed stream.</returns>
    public Stream Compress(Stream fileStream);
}