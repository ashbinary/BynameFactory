using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NintendoTools.Utils;

internal sealed class FileReader : IDisposable
{
    #region private members
    private readonly BinaryReader _reader;
    private bool _disposed;
    #endregion

    #region constructor
    public FileReader(Stream fileStream, bool leaveOpen = false)
    {
        _reader = new BinaryReader(fileStream, Encoding.UTF8, leaveOpen);
        Position = 0;
    }
    #endregion

    #region public properties
    public bool BigEndian { get; set; }

    public long Position
    {
        get => _reader.BaseStream.Position;
        set => _reader.BaseStream.Position = value;
    }

    public Stream BaseStream => _reader.BaseStream;
    #endregion

    #region public methods
    //skips a number of bytes forward or backwards
    public void Skip(int count) => Position += count;

    //jumps to a certain position in the stream
    public void JumpTo(long position) => Position = position;

    //aligns the stream to the next full given block size
    public void Align(int alignment) => _reader.BaseStream.Seek((-Position % alignment + alignment) % alignment, SeekOrigin.Current);

    //reads an array of raw bytes from the stream
    public byte[] ReadBytes(int length) => ReadBytes(length, length);
    public byte[] ReadBytesAt(long position, int length)
    {
        Position = position;
        return ReadBytes(length);
    }

    //reads a value from stream as sbyte
    public sbyte ReadSByte(int length = 1)
    {
        var bytes = ReadBytes(length, 1);
        return (sbyte) bytes[0];
    }
    public sbyte ReadSByteAt(long position, int length = 1)
    {
        Position = position;
        return ReadSByte(length);
    }

    //reads a value from stream as byte
    public byte ReadByte(int length = 1)
    {
        var bytes = ReadBytes(length, 1);
        return bytes[0];
    }
    public byte ReadByteAt(long position, int length = 1)
    {
        Position = position;
        return ReadByte(length);
    }

    //reads a value from stream as short
    public short ReadInt16(int length = 2)
    {
        var bytes = ReadBytes(length, 2, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToInt16(bytes, 0);
    }
    public short ReadInt16At(long position, int length = 2)
    {
        Position = position;
        return ReadInt16(length);
    }

    //reads a value from stream as ushort
    public ushort ReadUInt16(int length = 2)
    {
        var bytes = ReadBytes(length, 2, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToUInt16(bytes, 0);
    }
    public ushort ReadUInt16At(long position, int length = 2)
    {
        Position = position;
        return ReadUInt16(length);
    }

    //reads a value from stream as int
    public int ReadInt32(int length = 4)
    {
        var bytes = ReadBytes(length, 4, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToInt32(bytes, 0);
    }
    public int ReadInt32At(long position, int length = 4)
    {
        Position = position;
        return ReadInt32(length);
    }

    //reads a value from stream as uint
    public uint ReadUInt32(int length = 4)
    {
        var bytes = ReadBytes(length, 4, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToUInt32(bytes, 0);
    }
    public uint ReadUInt32At(long position, int length = 4)
    {
        Position = position;
        return ReadUInt32(length);
    }

    //reads a value from stream as long
    public long ReadInt64(int length = 8)
    {
        var bytes = ReadBytes(length, 8, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToInt64(bytes, 0);
    }
    public long ReadInt64At(long position, int length = 8)
    {
        Position = position;
        return ReadInt64(length);
    }

    //reads a value from stream as ulong
    public ulong ReadUInt64(int length = 8)
    {
        var bytes = ReadBytes(length, 8, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToUInt64(bytes, 0);
    }
    public ulong ReadUInt64At(long position, int length = 8)
    {
        Position = position;
        return ReadUInt64(length);
    }

    //reads a value from stream as float
    public float ReadSingle(int length = 4)
    {
        var bytes = ReadBytes(length, 4, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToSingle(bytes, 0);
    }
    public float ReadSingleAt(long position, int length = 4)
    {
        Position = position;
        return ReadSingle(length);
    }

    //reads a value from stream as double
    public double ReadDouble(int length = 8)
    {
        var bytes = ReadBytes(length, 8, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToDouble(bytes, 0);
    }
    public double ReadDoubleAt(long position, int length = 8)
    {
        Position = position;
        return ReadDouble(length);
    }

    //reads a value from stream as hex string
    public string ReadHexString(int length)
    {
        var bytes = ReadBytes(length, length % 2 + length, !BigEndian);
        return BitConverter.ToString(bytes).Replace("-", "");
    }
    public string ReadHexStringAt(long position, int length)
    {
        Position = position;
        return ReadHexString(length);
    }
    //reads a value from stream as utf8 string
    public string ReadString(int length) => ReadString(length, Encoding.UTF8);
    public string ReadStringAt(long position, int length) => ReadStringAt(position, length, Encoding.UTF8);

    //reads a value from stream as string with a specific encoding
    public string ReadString(int length, Encoding encoding)
    {
        var bytes = ReadBytes(length, 0);
        return encoding.GetString(bytes).TrimEnd('\0');
    }
    public string ReadStringAt(long position, int length, Encoding encoding)
    {
        Position = position;
        return ReadString(length, encoding);
    }

    //reads a value from stream as utf8 string until encountering a null-byte
    public string ReadTerminatedString(int maxLength = -1) => ReadTerminatedString(Encoding.UTF8, maxLength);
    public string ReadTerminatedStringAt(long position, int maxLength = -1) => ReadTerminatedStringAt(position, Encoding.UTF8, maxLength);

    //reads a value from stream as string with a specific encoding until encountering a null-byte
    public string ReadTerminatedString(Encoding encoding, int maxLength = -1)
    {
        var bytes = new List<byte>();
        while (_reader.PeekChar() != '\0' && bytes.Count != maxLength)
        {
            bytes.Add(_reader.ReadByte());
        }
        return encoding.GetString(bytes.ToArray());
    }
    public string ReadTerminatedStringAt(long position, Encoding encoding, int maxLength = -1)
    {
        Position = position;
        return ReadTerminatedString(encoding, maxLength);
    }

    #endregion

    #region private methods
    //reads an array of raw bytes from the stream
    private byte[] ReadBytes(int length, int padding, bool reversed = false)
    {
        var bytes = new byte[length > padding ? length : padding];
        var _ = _reader.Read(bytes, reversed ? bytes.Length - length : 0, length);

        if (reversed) Array.Reverse(bytes);
        return bytes;
    }
    #endregion

    #region IDisposable interface
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _reader.Dispose();
        _disposed = true;
    }
    #endregion
}