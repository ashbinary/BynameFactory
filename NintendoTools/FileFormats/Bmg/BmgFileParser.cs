using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NintendoTools.FileFormats.Msbt;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Bmg;

/// <summary>
/// A class for parsing BMG files.
/// </summary>
public class BmgFileParser : IFileParser<IList<MsbtMessage>>
{
    #region private members
    private readonly string? _language;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="BmgFileParser"/> class without a language.
    /// </summary>
    public BmgFileParser() : this(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BmgFileParser"/> class with the given language.
    /// Each parsed <see cref="MsbtMessage"/> object will have the given language assigned once parsed.
    /// </summary>
    /// <param name="language">The language to use.</param>
    public BmgFileParser(string? language) => _language = language;
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
        if (!CanParse(reader)) throw new InvalidDataException("File is not a BMG file.");

        //parse file metadata and header
        GetMetaData(reader, out var sectionCount, out _, out var encoding, out var decoder);

        //parse messages
        var messages = new List<MsbtMessage>();
        var messageInfo = new List<Tuple<uint, byte[]>>();
        var content = new List<string>();
        var functions = new List<List<Tuple<uint, byte[]>>>();
        var ids = new List<uint>();

        long sectionOffset = 0x20;
        for (var i = 0; i < sectionCount; ++i)
        {
            reader.JumpTo(sectionOffset);
            reader.Align(32);

            var type = reader.ReadString(4, Encoding.ASCII);
            var sectionSize = reader.ReadUInt32();
            sectionOffset += sectionSize;

            switch (type)
            {
                case "INF1":
                    ParseInf1(reader, messageInfo);
                    break;
                case "DAT1":
                    ParseDat1(reader, sectionSize, messageInfo, encoding, decoder, content, functions);
                    break;
                case "MID1":
                    ParseMid1(reader, ids);
                    break;
            }
        }

        //compile messages
        if (ids.Count > 0)
        {
            for (var i = 0; i < content.Count; ++i)
            {
                messages.Add(new MsbtMessage
                {
                    Label = ids[i].ToString(),
                    Attribute = messageInfo[i].Item2,
                    Text = content[i],
                    Functions = functions[i],
                    Language = _language,
                    Encoding = encoding
                });
            }
        }
        else
        {
            var format = "D" + (content.Count - 1).ToString().Length;
            for (var i = 0; i < content.Count; ++i)
            {
                messages.Add(new MsbtMessage
                {
                    Label = i.ToString(format),
                    Attribute = messageInfo[i].Item2,
                    Text = content[i],
                    Functions = functions[i],
                    Language = _language,
                    Encoding = encoding
                });
            }
        }

        messages.Sort((m1, m2) => int.Parse(m1.Label) - int.Parse(m2.Label));

        return messages;
    }
    #endregion

    #region private methods
    //verifies that the file is a BMG file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(0, 8) == "MESGbmg1";

    //parses meta data
    private static void GetMetaData(FileReader reader, out uint sectionCount, out uint fileSize, out Encoding encoding, out TextDecoder decoder)
    {
        //sanity check -> section count is usually less than 10; some older BMG files use LE
        if (reader.ReadUInt32At(12) > 10) reader.BigEndian = true;

        fileSize = reader.ReadUInt32At(8);
        sectionCount = reader.ReadUInt32At(12);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        switch (reader.ReadByteAt(16))
        {
            case 0:
            case 1:
                encoding = Encoding.GetEncoding(1252);
                decoder = ParseSingleByte;
                break;
            case 2:
                encoding = reader.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;
                decoder = ParseDoubleByte;
                break;
            case 3:
                encoding = Encoding.GetEncoding("Shift-JIS");
                decoder = ParseSingleByte;
                break;
            case 4:
                encoding = Encoding.UTF8;
                decoder = ParseSingleByte;
                break;
            default:
                encoding = reader.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;
                decoder = ParseDoubleByte;
                break;
        }
    }

    //parse INF1 type sections (message info)
    private static void ParseInf1(FileReader reader, ICollection<Tuple<uint, byte[]>> messageInfo)
    {
        var entryCount = reader.ReadUInt16();
        var entrySize = reader.ReadUInt16();
        //var fileId = reader.ReadUInt16();
        //var defaultColor = reader.ReadByte();
        reader.Skip(4);

        for (var i = 0; i < entryCount; ++i)
        {
            var offset = reader.ReadUInt32();
            var attributes = reader.ReadBytes(entrySize - 4);
            messageInfo.Add(new Tuple<uint, byte[]>(offset, attributes));
        }
    }

    //parse DAT1 type sections (message content)
    private static void ParseDat1(FileReader reader, uint sectionSize, IList<Tuple<uint, byte[]>> messageInfo, Encoding encoding, TextDecoder decoder, ICollection<string> content, ICollection<List<Tuple<uint, byte[]>>> functions)
    {
        var sectionStart = reader.Position;
        var sectionEnd = reader.Position + sectionSize - 1;

        for (var i = 0; i < messageInfo.Count; ++i)
        {
            var messageStart = messageInfo[i].Item1;
            var messageEnd = i + 1 < messageInfo.Count ? messageInfo[i + 1].Item1 : sectionEnd;

            reader.JumpTo(sectionStart + messageStart);

            var message = new StringBuilder();
            var messageFunctions = new List<Tuple<uint, byte[]>>();

            //parse message text
            var data = reader.ReadBytes((int) (messageEnd - messageStart));
            decoder(data, encoding, message, messageFunctions);

            content.Add(message.ToString());
            functions.Add(messageFunctions);
        }
    }

    //delegate for text parsing
    private delegate void TextDecoder(byte[] data, Encoding encoding, StringBuilder message, IList<Tuple<uint, byte[]>> messageFunctions);

    //parse text section with single-byte encoding
    private static void ParseSingleByte(byte[] data, Encoding encoding, StringBuilder message, IList<Tuple<uint, byte[]>> messageFunctions)
    {
        var lastCharIndex = 0;
        for (var i = 0; i < data.Length; ++i)
        {
            if (data[i] == 0x00) //end of message
            {
                message.Append(encoding.GetChars(data, lastCharIndex, i - lastCharIndex));
                break;
            }

            if (data[i] == 0x1A) //start of function call
            {
                message.Append(encoding.GetChars(data, lastCharIndex, i - lastCharIndex));
                message.Append("{{").Append(messageFunctions.Count).Append("}}");

                var argLen = data[++i] - 3;
                var hash = data[++i];
                var args = new byte[argLen];
                for (var k = 0; k < argLen; ++k) args[k] = data[++i];
                messageFunctions.Add(new Tuple<uint, byte[]>(hash, args));

                lastCharIndex = i + 1;
            }
        }
    }

    //parse text section with double-byte encoding
    private static void ParseDoubleByte(byte[] data, Encoding encoding, StringBuilder message, IList<Tuple<uint, byte[]>> messageFunctions)
    {
        var lastCharIndex = 0;
        for (var i = 1; i < data.Length; i += 2)
        {
            if (data[i - 1] == 0x00 && data[i] == 0x00) //end of message
            {
                message.Append(encoding.GetChars(data, lastCharIndex, i - lastCharIndex - 1));
                break;
            }

            if (data[i - 1] == 0x00 && data[i] == 0x1A || data[i - 1] == 0x1A && data[i] == 0x00) //start of function call
            {
                message.Append(encoding.GetChars(data, lastCharIndex, i - lastCharIndex - 1));
                message.Append("{{").Append(messageFunctions.Count).Append("}}");

                var argLen = data[++i] - 4;
                var hash = data[++i];
                var args = new byte[argLen];
                for (var k = 0; k < argLen; ++k) args[k] = data[++i];
                messageFunctions.Add(new Tuple<uint, byte[]>(hash, args));

                lastCharIndex = i + 1;
            }
        }
    }

    //parse MID1 type sections (message IDs)
    private static void ParseMid1(FileReader reader, ICollection<uint> ids)
    {
        var entryCount = reader.ReadUInt16();
        //var format = reader.ReadByte();
        //var formatType = reader.ReadByte();
        reader.Skip(6);

        for (var i = 0; i < entryCount; i++) ids.Add(reader.ReadUInt32());
    }
    #endregion
}