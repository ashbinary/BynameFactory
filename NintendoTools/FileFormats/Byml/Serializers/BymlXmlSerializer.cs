using System;
using System.IO;
using System.Xml;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for serializing <see cref="BymlFile"/> objects to XML.
/// </summary>
public class BymlXmlSerializer : IBymlSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets number of indentation characters that should be used.
    /// '<c>0</c>' disables indentation.
    /// The default value is <c>2</c>.
    /// </summary>
    public int Indentation { get; set; } = 2;

    /// <summary>
    /// Gets or sets the indentation character that should be used.
    /// The default value is '<c> </c>'.
    /// </summary>
    public char IndentChar { get; set; } = ' ';
    #endregion

    #region IBymlSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BymlFile bymlFile)
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (bymlFile is null) throw new ArgumentNullException(nameof(bymlFile));

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("byml");

        xmlWriter.WriteStartElement("version");
        xmlWriter.WriteValue(bymlFile.Version);
        xmlWriter.WriteEndElement();

        WriteNode(xmlWriter, bymlFile.RootNode);

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }
    #endregion

    #region private methods
    //writes the XML elements for a given node
    private static void WriteNode(XmlWriter writer, Node node)
    {
        switch (node)
        {
            case DictionaryNode dict:
                writer.WriteStartElement("dict");
                foreach (var item in dict)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("name", item.Key);
                    WriteNode(writer, item.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                break;
            case ArrayNode array:
                writer.WriteStartElement("array");
                for (var i = 0; i < array.Count; ++i)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("index", i.ToString());
                    WriteNode(writer, array[i]);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                break;
            case ValueNode value:
                writer.WriteStartElement("value");
                writer.WriteAttributeString("type", $"xs:{GetType(value)}");
                if (value.GetValue() is { } val) writer.WriteValue(val);
                writer.WriteEndElement();
                break;
            case PathNode path:
                writer.WriteStartElement("path");
                writer.WriteStartElement("positionX");
                writer.WriteValue(path.PositionX);
                writer.WriteEndElement();
                writer.WriteStartElement("positionY");
                writer.WriteValue(path.PositionY);
                writer.WriteEndElement();
                writer.WriteStartElement("positionZ");
                writer.WriteValue(path.PositionZ);
                writer.WriteEndElement();
                writer.WriteStartElement("normalX");
                writer.WriteValue(path.NormalX);
                writer.WriteEndElement();
                writer.WriteStartElement("normalY");
                writer.WriteValue(path.NormalY);
                writer.WriteEndElement();
                writer.WriteStartElement("normalZ");
                writer.WriteValue(path.NormalZ);
                writer.WriteEndElement();
                writer.WriteEndElement();
                break;
            case BinaryNode binary:
                writer.WriteStartElement("binary");
                writer.WriteAttributeString("size", binary.Size.ToString());
                writer.WriteValue(binary.Data.ToHexString(true));
                writer.WriteEndElement();
                break;
            case BinaryParamNode binaryParam:
                writer.WriteStartElement("binary");
                writer.WriteAttributeString("size", binaryParam.Size.ToString());
                writer.WriteAttributeString("param", binaryParam.Param.ToString());
                writer.WriteValue(binaryParam.Data.ToHexString(true));
                writer.WriteEndElement();
                break;
            case NullNode:
                writer.WriteStartElement("null");
                writer.WriteEndElement();
                break;
        }
    }

    //gets XML standard data type from value
    private static string GetType(ValueNode node)
    {
        switch (node.GetValue())
        {
            case bool: return "boolean";
            case sbyte: return "byte";
            case byte: return "unsignedByte";
            case short: return "short";
            case ushort: return "unsignedShort";
            case int: return "int";
            case uint: return "unsignedInt";
            case long: return "long";
            case ulong: return "unsignedLong";
            case float: return "float";
            case double: return "double";
            case string: return "string";
            case null: return "null";
            default: return "undefined";
        }
    }
    #endregion
}