using System;
using System.IO;
using ZstdNet;

namespace NintendoTools.Compression.Zstd;

/// <summary>
/// A class for Zstandard compression.
/// </summary>
public class ZstdCompressor : ICompressor
{
    #region private members
    private readonly CompressionOptions _options;
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdDecompressor"/> class.
    /// </summary>
    public ZstdCompressor() => _options = new CompressionOptions(3);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdDecompressor"/> class.
    /// </summary>
    /// <param name="compressionLevel">The level of data compression</param>
    public ZstdCompressor(int compressionLevel) => _options = new CompressionOptions(compressionLevel);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdDecompressor"/> class with a given decompression dictionary.
    /// </summary>
    /// <param name="dict">The compression dictionary to use.</param>
    public ZstdCompressor(byte[] dict) => _options = new CompressionOptions(dict);

    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdDecompressor"/> class with a given decompression dictionary.
    /// </summary>
    /// <param name="dict">The compression dictionary to use.</param>
    /// <param name="compressionLevel">The level of data compression</param>
    public ZstdCompressor(byte[] dict, int compressionLevel) => _options = new CompressionOptions(dict, compressionLevel);
    #endregion

    #region ICompressor interface
    /// <inheritdoc/>
    public Stream Compress(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        fileStream.Position = 0;
        var resultStream = new MemoryStream();
        using var compressor = new CompressionStream(fileStream, _options);
        compressor.CopyTo(resultStream);

        return resultStream;
    }
    #endregion
}