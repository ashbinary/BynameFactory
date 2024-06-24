using System;
using System.IO;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for parsing BYML files.
/// </summary>
public class BymlFileParser : IFileParser<BymlFile>
{
    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public BymlFile Parse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var reader = new FileReader(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a BYML file.");

        var bymlFile = new BymlFile {Version = reader.ReadInt16At(0x02)};

        //read header
        ReadHeader(reader, out var tables, out var rootNodeOffset);
        if (rootNodeOffset == 0)
        {
            bymlFile.RootNode = new NullNode();
            return bymlFile;
        }

        bymlFile.RootNode = reader.ReadByteAt(rootNodeOffset) switch
        {
            NodeTypes.Array      => ReadArrayNode(reader, rootNodeOffset, tables),
            NodeTypes.Dictionary => ReadDictionaryNode(reader, rootNodeOffset, tables),
            _ => throw new InvalidDataException("Invalid root node data type.")
        };

        return bymlFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a BYML file
    private static bool CanParse(FileReader reader)
    {
        switch (reader.ReadStringAt(0, 2))
        {
            case "BY":
                reader.BigEndian = true;
                return true;
            case "YB":
                reader.BigEndian = false;
                return true;
            default:
                return false;
        }
    }

    //parses header and tables
    private static void ReadHeader(FileReader reader, out Tables tables, out long rootNodeOffset)
    {
        tables = new Tables
        {
            Names = Array.Empty<string>(),
            Strings = Array.Empty<string>()
        };

        var nameTableOffset = reader.ReadInt32At(0x04);
        var stringTableOffset = reader.ReadInt32();
        var pathTableOffset = reader.ReadInt32();
        var hasPaths = false;

        if (nameTableOffset > 0)
        {
            var type = reader.ReadByteAt(nameTableOffset);
            if (type == NodeTypes.StringTable) tables.Names = ReadStringTable(reader, reader.Position);
        }

        if (stringTableOffset > 0)
        {
            var type = reader.ReadByteAt(stringTableOffset);
            if (type == NodeTypes.StringTable) tables.Strings = ReadStringTable(reader, reader.Position);
        }

        if (pathTableOffset > 0) //only appears to exist in MarioKart 8
        {
            var type = reader.ReadByteAt(pathTableOffset);
            if (type == NodeTypes.PathTable)
            {
                hasPaths = true;
                var length = reader.ReadInt32(3);

                var offsets = new uint[length + 1];
                for (var i = 0; i < offsets.Length; ++i) offsets[i] = reader.ReadUInt32();

                tables.Paths = new PathNode[length];
                for (var i = 0; i < length; ++i)
                {
                    tables.Paths[i] = new PathNode
                    {
                        PositionX = reader.ReadSingleAt(offsets[i]),
                        PositionY = reader.ReadSingle(),
                        PositionZ = reader.ReadSingle(),
                        NormalX = reader.ReadSingle(),
                        NormalY = reader.ReadSingle(),
                        NormalZ = reader.ReadSingle()
                    };
                }
            }
        }

        rootNodeOffset = reader.ReadUInt32At(hasPaths ? 0x10 : 0x0c);
    }

    //parse a string table
    private static string[] ReadStringTable(FileReader reader, long offset)
    {
        var length = reader.ReadUInt32At(offset, 3);
        var strings = new string[length];

        var offsets = new uint[length + 1];
        for (var i = 0; i < offsets.Length; ++i) offsets[i] = reader.ReadUInt32();

        for (var i = 0; i < length; ++i) strings[i] = reader.ReadString((int)(offsets[i + 1] - offsets[i]));

        return strings;
    }

    //parses a generic node
    private static Node ReadNode(FileReader reader, long offset, byte type, Tables tables) => type switch
    {
        NodeTypes.String => ReadStringNode(reader, offset, tables),
        NodeTypes.Binary => tables.Paths is null ? ReadBinaryNode(reader, offset) : ReadPathNode(reader, offset, tables),
        NodeTypes.BinaryParam => ReadBinaryParamNode(reader, offset),
        NodeTypes.Array => ReadArrayNode(reader, offset, tables),
        NodeTypes.Dictionary => ReadDictionaryNode(reader, offset, tables),
        NodeTypes.Bool => ReadBoolNode(reader, offset),
        NodeTypes.Int => ReadIntNode(reader, offset),
        NodeTypes.Float => ReadFloatNode(reader, offset),
        NodeTypes.UInt => ReadUIntNode(reader, offset),
        NodeTypes.Long => ReadLongNode(reader, offset),
        NodeTypes.ULong => ReadULongNode(reader, offset),
        NodeTypes.Double => ReadDoubleNode(reader, offset),
        NodeTypes.Null => new NullNode(),
        _ => throw new FileLoadException($"Unknown node type: 0x{type:x2}")
    };

    //parses an array node
    private static ArrayNode ReadArrayNode(FileReader reader, long offset, Tables tables)
    {
        var length = reader.ReadUInt32At(offset + 1, 3);
        var types = reader.ReadBytes((int)length);
        reader.Align(4);

        var valueOffset = reader.Position;

        var node = new ArrayNode();
        for (uint i = 0; i < length; ++i)
        {
            var nodeOffset = valueOffset + i * 4;
            var value = types[i] == NodeTypes.Array || types[i] == NodeTypes.Dictionary ? reader.ReadUInt32At(nodeOffset) : nodeOffset;

            var childNode = ReadNode(reader, value, types[i], tables);
            node.Add(childNode);
        }

        return node;
    }

    //parses a dictionary node
    private static DictionaryNode ReadDictionaryNode(FileReader reader, long offset, Tables tables)
    {
        var length = reader.ReadUInt32At(offset + 1, 3);

        var node = new DictionaryNode();
        for (uint i = 0; i < length; ++i)
        {
            var nodeOffset = offset + 4 + i * 8;
            var name = tables.Names[reader.ReadInt32At(nodeOffset, 3)];
            var type = reader.ReadByte();
            var value = type is NodeTypes.Array or NodeTypes.Dictionary ? reader.ReadUInt32() : nodeOffset + 4;

            var childNode = ReadNode(reader, value, type, tables);
            node.Add(name, childNode);
        }

        return node;
    }

    //parses a string value node
    private static Node ReadStringNode(FileReader reader, long offset, Tables tables)
    {
        return new ValueNode<string> {Type = NodeTypes.String, Value = tables.Strings[reader.ReadInt32At(offset)]};
    }

    //parses a path value node
    private static Node ReadPathNode(FileReader reader, long offset, Tables tables)
    {
        return tables.Paths![reader.ReadInt32At(offset)];
    }

    //parses a binary value node
    private static Node ReadBinaryNode(FileReader reader, long offset)
    {
        var size = reader.ReadInt32At(reader.ReadInt32At(offset));

        return new BinaryNode
        {
            Size = size,
            Data = reader.ReadBytes(size)
        };
    }

    //parses a binary param value node
    private static Node ReadBinaryParamNode(FileReader reader, long offset)
    {
        var size = reader.ReadInt32At(reader.ReadInt32At(offset));

        return new BinaryParamNode
        {
            Size = size,
            Param = reader.ReadInt32(),
            Data = reader.ReadBytes(size)
        };
    }

    //parses a bool value node
    private static Node ReadBoolNode(FileReader reader, long offset)
    {
        return new ValueNode<bool> {Type = NodeTypes.Bool, Value = reader.ReadUInt32At(offset) == 1};
    }

    //parses an int value node
    private static Node ReadIntNode(FileReader reader, long offset)
    {
        return new ValueNode<int> {Type = NodeTypes.Int, Value = reader.ReadInt32At(offset)};
    }

    //parses a float value node
    private static Node ReadFloatNode(FileReader reader, long offset)
    {
        return new ValueNode<float> {Type = NodeTypes.Float, Value = reader.ReadSingleAt(offset)};
    }

    //parses an uint value node
    private static Node ReadUIntNode(FileReader reader, long offset)
    {
        return new ValueNode<uint> {Type = NodeTypes.UInt, Value = reader.ReadUInt32At(offset)};
    }

    //parses an long value node
    private static Node ReadLongNode(FileReader reader, long offset)
    {
        return new ValueNode<long> {Type = NodeTypes.Long, Value = reader.ReadInt64At(reader.ReadUInt32At(offset))};
    }

    //parses an ulong value node
    private static Node ReadULongNode(FileReader reader, long offset)
    {
        return new ValueNode<ulong> {Type = NodeTypes.ULong, Value = reader.ReadUInt64At(reader.ReadUInt32At(offset))};
    }

    //parses an double value node
    private static Node ReadDoubleNode(FileReader reader, long offset)
    {
        return new ValueNode<double> {Type = NodeTypes.Double, Value = reader.ReadDoubleAt(reader.ReadUInt32At(offset))};
    }
    #endregion

    #region helper classes
    private class Tables
    {
        public string[] Names { get; set; } = null!;
        public string[] Strings { get; set; } = null!;
        public PathNode[]? Paths { get; set; }
    }
    #endregion
}