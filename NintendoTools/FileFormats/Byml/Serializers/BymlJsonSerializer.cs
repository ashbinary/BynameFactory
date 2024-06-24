using System;
using System.IO;
using Newtonsoft.Json;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for serializing <see cref="BymlFile"/> objects to JSON.
/// </summary>
public class BymlJsonSerializer : IBymlSerializer
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

        using var jsonWriter = new JsonTextWriter(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("version");
        jsonWriter.WriteValue(bymlFile.Version);

        jsonWriter.WritePropertyName("rootNode");
        WriteNode(jsonWriter, bymlFile.RootNode, jsonWriter.Formatting);

        jsonWriter.WriteEndObject();
    }
    #endregion

    #region private methods
    //writes the JSON elements for a given node
    private static void WriteNode(JsonWriter writer, Node node, Formatting defaultFormatting)
    {
        switch (node)
        {
            case DictionaryNode dict:
                writer.Formatting = defaultFormatting;
                writer.WriteStartObject();
                foreach (var item in dict)
                {
                    writer.WritePropertyName(item.Key);
                    WriteNode(writer, item.Value, defaultFormatting);
                }
                writer.WriteEndObject();
                break;
            case ArrayNode array:
                writer.Formatting = defaultFormatting;
                writer.WriteStartArray();
                writer.Formatting = Formatting.None;
                foreach (var item in array) WriteNode(writer, item, defaultFormatting);
                writer.WriteEndArray();
                writer.Formatting = defaultFormatting;
                break;
            case ValueNode value:
                writer.WriteValue(value.GetValue());
                break;
            case PathNode path:
                writer.Formatting = defaultFormatting;
                writer.WriteStartObject();
                writer.WritePropertyName("positionX");
                writer.WriteValue(path.PositionX);
                writer.WritePropertyName("positionY");
                writer.WriteValue(path.PositionY);
                writer.WritePropertyName("positionZ");
                writer.WriteValue(path.PositionZ);
                writer.WritePropertyName("normalX");
                writer.WriteValue(path.NormalX);
                writer.WritePropertyName("normalY");
                writer.WriteValue(path.NormalY);
                writer.WritePropertyName("normalZ");
                writer.WriteValue(path.NormalZ);
                writer.WriteEndObject();
                break;
            case BinaryNode binary:
                writer.WriteStartObject();
                writer.WritePropertyName("size");
                writer.WriteValue(binary.Size);
                writer.WritePropertyName("data");
                writer.WriteValue(binary.Data.ToHexString(true));
                writer.WriteEndObject();
                break;
            case BinaryParamNode binaryParam:
                writer.WriteStartObject();
                writer.WritePropertyName("size");
                writer.WriteValue(binaryParam.Size);
                writer.WritePropertyName("param");
                writer.WriteValue(binaryParam.Param);
                writer.WritePropertyName("data");
                writer.WriteValue(binaryParam.Data.ToHexString(true));
                writer.WriteEndObject();
                break;
            case NullNode:
                writer.WriteNull();
                break;
        }
    }
    #endregion
}