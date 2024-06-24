using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class for parsing AAMP files.
/// </summary>
public class AampFileParser : IFileParser<AampFile>
{
    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public AampFile Parse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var reader = new FileReader(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not an AAMP file.");

        var aampFile = new AampFile {Version = reader.ReadInt32At(0x04)};

        var flags = reader.ReadUInt32();
        reader.BigEndian = (flags & 1 << 0) == 0;
        var encoding = (flags & 1 << 1) == 0 ? Encoding.ASCII : Encoding.UTF8;
        //var fileSize = reader.ReadUInt32();

        //var rootVersion = reader.ReadUInt32At(0x10);
        var rootOffset = reader.ReadUInt32At(0x14);
        //var listCount = reader.ReadUInt32At(0x18);
        //var objCount = reader.ReadUInt32At(0x1c);
        //var paramCount = reader.ReadUInt32At(0x20);
        //var dataSize = reader.ReadUInt32(0x24);
        //var stringSize = reader.ReadUInt32(0x28);
        //var dataType = reader.ReadStringAt(0x30, (int) rootOffset);

        reader.JumpTo(0x30 + rootOffset);
        aampFile.Root = ReadList(reader, encoding);

        return aampFile;
    }
    #endregion

    #region private methods
    //verifies that the file is an AAMP file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(0, 4) == "AAMP";

    //read data as list node
    private static ParameterList ReadList(FileReader reader, Encoding encoding)
    {
        var offset = reader.Position;
        var list = new ParameterList {Name = reader.ReadHexString(4)};
        var listOffset = reader.ReadUInt16() * 4;
        var listCount = reader.ReadUInt16();
        var objOffset = reader.ReadUInt16() * 4;
        var objCount = reader.ReadUInt16();

        for (var i = 0; i < listCount; ++i)
        {
            reader.JumpTo(offset + listOffset + i * 12);
            list.Lists.Add(ReadList(reader, encoding));
        }

        for (var i = 0; i < objCount; ++i)
        {
            reader.JumpTo(offset + objOffset + i * 8);
            list.Objects.Add(ReadObject(reader, encoding));
        }

        return list;
    }

    //read data as object node
    private static ParameterObject ReadObject(FileReader reader, Encoding encoding)
    {
        var offset = reader.Position;
        var obj = new ParameterObject {Name = reader.ReadHexString(4)};
        var paramOffset = reader.ReadUInt16() * 4;
        var paramCount = reader.ReadUInt16();

        for (var i = 0; i < paramCount; ++i)
        {
            reader.JumpTo(offset + paramOffset + i * 8);
            obj.Parameters.Add(ReadParameter(reader, encoding));
        }

        return obj;
    }

    //read data as parameter
    private static Parameter ReadParameter(FileReader reader, Encoding encoding)
    {
        var offset = reader.Position;
        var name = reader.ReadHexString(4);
        var dataOffset = reader.ReadUInt32(3) * 4;
        var type = reader.ReadByte();

        Parameter parameter;
        switch (type)
        {
            case ParameterTypes.Bool:
                parameter = new ValueParameter
                {
                    Value = reader.ReadUInt32At(offset + dataOffset) != 0
                };
                break;
            case ParameterTypes.Int32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadInt32At(offset + dataOffset)
                };
                break;
            case ParameterTypes.UInt32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadUInt32At(offset + dataOffset)
                };
                break;
            case ParameterTypes.Float32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadSingleAt(offset + dataOffset)
                };
                break;
            case ParameterTypes.String32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadTerminatedStringAt(offset + dataOffset, encoding, 32)
                };
                break;
            case ParameterTypes.String64:
                parameter = new ValueParameter
                {
                    Value = reader.ReadTerminatedStringAt(offset + dataOffset, encoding, 64)
                };
                break;
            case ParameterTypes.String256:
                parameter = new ValueParameter
                {
                    Value = reader.ReadTerminatedStringAt(offset + dataOffset, encoding, 256)
                };
                break;
            case ParameterTypes.StringReference:
                parameter = new ValueParameter
                {
                    Value = reader.ReadHexStringAt(offset + dataOffset, 4)
                };
                break;
            case ParameterTypes.BinaryBuffer:
                parameter = new ValueParameter
                {
                    Value = reader.ReadBytes((int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.Int32Buffer:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadInt32(), (int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.UInt32Buffer:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadUInt32(), (int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.Float32Buffer:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadSingle(), (int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.Color:
                parameter = new ColorParameter
                {
                    Red = reader.ReadSingle(),
                    Green = reader.ReadSingle(),
                    Blue = reader.ReadSingle(),
                    Alpha = reader.ReadSingle()
                };
                break;
            case ParameterTypes.Vector2:
            case ParameterTypes.Vector3:
            case ParameterTypes.Vector4:
            case ParameterTypes.Quat:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadSingle(), GetValueArraySize(type))
                };
                break;
            case ParameterTypes.Curve1:
            case ParameterTypes.Curve2:
            case ParameterTypes.Curve3:
            case ParameterTypes.Curve4:
                parameter = new CurveParameter
                {
                    Curves = BuildList(() => new CurveValue
                    {
                        IntValues = BuildArray(() => reader.ReadUInt32(), 2),
                        FloatValues = BuildArray(() => reader.ReadSingle(), 30)
                    }, GetValueArraySize(type))
                };
                break;
            default:
                parameter = new ValueParameter
                {
                    Value = type
                };
                break;
        }

        parameter.Name = name;
        parameter.Type = type;
        return parameter;
    }

    private static int GetValueArraySize(byte type) => type switch
    {
        ParameterTypes.Curve1  => 1,
        ParameterTypes.Vector2 => 2,
        ParameterTypes.Curve2  => 2,
        ParameterTypes.Vector3 => 3,
        ParameterTypes.Curve3  => 3,
        ParameterTypes.Vector4 => 4,
        ParameterTypes.Quat    => 4,
        ParameterTypes.Curve4  => 4,
        _ => 0
    };

    private static T[] BuildArray<T>(Func<T> read, int length)
    {
        var data = new T[length];
        for (var i = 0; i < length; ++i) data[i] = read.Invoke();
        return data;
    }

    private static IList<T> BuildList<T>(Func<T> read, int length)
    {
        var data = new List<T>();
        for (var i = 0; i < length; ++i) data.Add(read.Invoke());
        return data;
    }
    #endregion
}