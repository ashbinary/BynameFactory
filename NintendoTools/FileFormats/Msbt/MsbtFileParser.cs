using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// A class for parsing MSBT files.
/// </summary>
public class MsbtFileParser : IFileParser<IList<MsbtMessage>>
{
    #region private members
    private readonly string? _language;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="MsbtFileParser"/> class without a language.
    /// </summary>
    public MsbtFileParser() : this(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MsbtFileParser"/> class with the given language.
    /// Each parsed <see cref="MsbtMessage"/> object will have the given language assigned once parsed.
    /// </summary>
    /// <param name="language">The language to use.</param>
    public MsbtFileParser(string? language) => _language = language;
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public IList<MsbtMessage> Parse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var reader = new FileReader(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a MSBT file.");

        //parse file metadata and header
        GetMetaData(reader, out var sectionCount, out _, out _, out var encoding);

        //parse messages
        var messages = new List<MsbtMessage>();
        var labels = new List<string>();
        var indices = new List<uint>();
        var attributes = new List<byte[]>();
        var content = new List<string>();
        var functions = new List<List<Tuple<uint, byte[]>>>();

        long sectionOffset = 0x20;
        for (var i = 0; i < sectionCount; ++i)
        {
            reader.JumpTo(sectionOffset);
            reader.Align(16);

            var type = reader.ReadString(4, Encoding.ASCII);
            var sectionSize = reader.ReadUInt32();
            sectionOffset += 0x10 + (sectionSize + 0xF & ~0xF);

            switch (type)
            {
                case "NLI1":
                    //ParseNli1(reader, ids, indices);
                    break;
                case "LBL1":
                    ParseLbl1(reader, labels, indices);
                    break;
                case "ATR1":
                    ParseAtr1(reader, attributes);
                    break;
                case "TXT2":
                    ParseTxt2(reader, encoding, sectionSize, content, functions);
                    break;
            }
        }

        //compile messages
        if (labels.Count > 0)
        {
            for (var i = 0; i < labels.Count; ++i)
            {
                var index = (int)indices[i];

                var message = new MsbtMessage
                {
                    Label = labels[i],
                    Attribute = index < attributes.Count ? attributes[index] : null,
                    Text = content[index],
                    Functions = functions[index],
                    Language = _language,
                    Encoding = encoding
                };

                messages.Add(message);
            }
        }
        else
        {
            var format = "D" + (content.Count - 1).ToString().Length;
            for (var i = 0; i < content.Count; ++i)
            {
                var message = new MsbtMessage
                {
                    Label = i.ToString(format),
                    Attribute = i < attributes.Count ? attributes[i] : null,
                    Text = content[i],
                    Functions = functions[i],
                    Language = _language,
                    Encoding = encoding
                };

                messages.Add(message);
            }
        }

        messages.Sort((m1, m2) => string.Compare(m1.Label, m2.Label, StringComparison.OrdinalIgnoreCase));

        return messages;
    }
    #endregion

    #region private methods
    //verifies that the file is a MSBT file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(0, 8, Encoding.ASCII) == "MsgStdBn";

    //parses meta data
    private static void GetMetaData(FileReader reader, out uint sectionCount, out uint fileSize, out int version, out Encoding encoding)
    {
        var byteOrder = reader.ReadUInt16At(1);
        if (byteOrder == 0xFEFF) reader.BigEndian = true;

        var encodingFlag = reader.ReadByteAt(12);
        if (encodingFlag == 0x00) encoding = Encoding.UTF8;
        else if (reader.BigEndian) encoding = Encoding.BigEndianUnicode;
        else encoding = Encoding.Unicode;

        version = reader.ReadByte(13);

        sectionCount = reader.ReadUInt32At(14);
        fileSize = reader.ReadUInt32At(18);
    }

    //parse NLI1 type sections (message id + index)
    private static void ParseNli1(FileReader reader, ICollection<uint> ids, ICollection<int> indices)
    {
        reader.Skip(8);
        var entryCount = reader.ReadUInt32();

        for (var i = 0; i < entryCount; ++i)
        {
            ids.Add(reader.ReadUInt32());
            indices.Add(reader.ReadInt32());
        }
    }

    //parse LBL1 type sections (message label + index)
    private static void ParseLbl1(FileReader reader, ICollection<string> labels, ICollection<uint> indices)
    {
        reader.Skip(8);
        var position = reader.Position;
        var entryCount = reader.ReadUInt32();

        for (var i = 0; i < entryCount; ++i)
        {
            //group header
            reader.JumpTo(position + 4 + i * 8);
            var labelCount = reader.ReadUInt32();
            var offset = reader.ReadUInt32();

            //labels
            reader.JumpTo(position + offset);
            for (var j = 0; j < labelCount; ++j)
            {
                var length = reader.ReadByte();
                labels.Add(reader.ReadString(length));
                indices.Add(reader.ReadUInt32());
            }
        }
    }

    //parse ATR1 type sections
    private static void ParseAtr1(FileReader reader, ICollection<byte[]> attributes)
    {
        reader.Skip(8);
        var entryCount = reader.ReadUInt32();
        var attributeSize = reader.ReadUInt32();

        for (var i = 0; i < entryCount; ++i)
        {
            var attribute = reader.ReadBytes((int)attributeSize);
            attributes.Add(attribute);
        }
    }

    //parse TXT2 type sections (message content)
    private static void ParseTxt2(FileReader reader, Encoding encoding, long sectionSize, ICollection<string> content, ICollection<List<Tuple<uint, byte[]>>> functions)
    {
        reader.Skip(8);
        var position = reader.Position;
        var entryCount = reader.ReadUInt32();
        var offsets = ReadArray(reader, (int)entryCount);

        for (var i = 0; i < entryCount; ++i)
        {
            //Get the start and end position
            var startPos = offsets[i] + position;
            var endPos = i + 1 < entryCount ? position + offsets[i + 1] : position + sectionSize;

            //parse message text
            reader.JumpTo(startPos);
            var message = new StringBuilder();

            //check bytes for function calls
            var messageFunctions = new List<Tuple<uint, byte[]>>();
            while (reader.Position < endPos)
            {
                var nextChar = reader.ReadInt16();

                switch (nextChar)
                {
                    case 0x0E: //start of function call
                        message.Append("{{").Append(messageFunctions.Count).Append("}}");
                        var hash = reader.ReadUInt32();
                        var argLen = reader.ReadUInt16();
                        messageFunctions.Add(new Tuple<uint, byte[]>(hash, reader.ReadBytes(argLen)));
                        break;
                    case 0x0F: //closing function/tag
                        message.Append("{{").Append(messageFunctions.Count).Append("}}");
                        messageFunctions.Add(new Tuple<uint, byte[]>((uint)nextChar, reader.ReadBytes(4)));
                        break;
                    default:
                        if (nextChar != 0x00) //we don't like those
                        {
                            message.Append(encoding.GetString(BitConverter.GetBytes(nextChar)));
                        }
                        break;
                }
            }

            content.Add(message.ToString());
            functions.Add(messageFunctions);
        }
    }

    //read a uint array
    private static uint[] ReadArray(FileReader reader, int count)
    {
        var result = new uint[count];
        for (var i = 0; i < result.Length; ++i) result[i] = reader.ReadUInt32();
        return result;
    }
    #endregion
}