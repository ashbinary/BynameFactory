using System;
using System.IO;
using ZstdNet;
using NintendoTools.Utils;

namespace NintendoTools.Compression.Zstd;

/// <summary>
/// A class for Zstandard compression.
/// </summary>
public class ZstdDecompressor : IDecompressor
{
    #region private members
    private readonly DecompressionOptions _options;
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdDecompressor"/> class.
    /// </summary>
    public ZstdDecompressor() => _options = new DecompressionOptions();

    /// <summary>
    /// Initializes a new instance of the <see cref="ZstdDecompressor"/> class with a given decompression dictionary.
    /// </summary>
    /// <param name="dict">The compression dictionary to use.</param>
    public ZstdDecompressor(byte[] dict) => _options = new DecompressionOptions(dict);
    #endregion

    #region IDecompressor interface
    /// <inheritdoc/>
    public bool CanDecompress(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var reader = new FileReader(fileStream, true);
        var magic = reader.ReadBytesAt(0, 4);
        return magic[0] == 0x28 && magic[1] == 0xb5 && magic[2] == 0x2f && magic[3] == 0xfd;
    }

    /// <inheritdoc/>
    public Stream Decompress(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        fileStream.Position = 0;
        var resultStream = new MemoryStream();
        using var decompressor = new DecompressionStream(fileStream, _options);
        decompressor.CopyTo(resultStream);

        return resultStream;
    }
    #endregion
}