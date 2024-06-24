using System;
using System.IO;
using System.Text;
using NintendoTools.Utils;

namespace NintendoTools.Compression.Yaz0;

/// <summary>
/// A class for Yaz0 compression.
/// </summary>
public class Yaz0Decompressor : IDecompressor
{
    #region IDecompressor interface
    /// <inheritdoc/>
    public bool CanDecompress(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        return CanDecompress(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public Stream Decompress(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var reader = new FileReader(fileStream);
        if (!CanDecompress(reader)) throw new InvalidDataException("Data is not Yaz0 compressed.");

        reader.BigEndian = true;
        var size = reader.ReadUInt32At(4);
        reader.JumpTo(16);

        var result = new byte[size];
        var index = 0;
        byte codeByte = 0;
        var codeBitsLeft = 0;
        while (index < size)
        {
            //get next code byte if required
            if (codeBitsLeft == 0)
            {
                codeByte = reader.ReadByte();
                codeBitsLeft = 8;
            }

            if((codeByte & 0x80) != 0) //direct copy
            {
                result[index++] = reader.ReadByte();
            }
            else //RLE encoded
            {
                var byte1 = reader.ReadByte();
                var byte2 = reader.ReadByte();

                var count = byte1 >> 4;
                if (count == 0) count = reader.ReadByte() + 0x12;
                else count += 2;

                var copyIndex = index - ((((byte1 & 0xF) << 8) | byte2) + 1);
                for(var i = 0; i < count; ++i)
                {
                    result[index++] = result[copyIndex++];
                }
            }

            codeByte <<= 1;
            --codeBitsLeft;
        }


        return new MemoryStream(result);
    }
    #endregion

    #region private methods
    private static bool CanDecompress(FileReader reader) => reader.ReadStringAt(0, 4, Encoding.ASCII) == "Yaz0";
    #endregion
}