using System;
using System.IO;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for serializing <see cref="BymlFile"/> objects to YAML.
/// </summary>
public class BymlYamlSerializer : IBymlSerializer
{
    #region IBymlSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BymlFile bymlFile)
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (bymlFile is null) throw new ArgumentNullException(nameof(bymlFile));

        using var yamlWriter = new YamlTextWriter(writer);

        yamlWriter.WriteStartDocument();
        yamlWriter.WritePropertyName("version");
        yamlWriter.WriteValue(bymlFile.Version);

        yamlWriter.WritePropertyName("rootNode");
        WriteNode(yamlWriter, bymlFile.RootNode);
    }
    #endregion

    #region private methods
    //writes the YAML elements for a given node
    private static void WriteNode(YamlTextWriter writer, Node node)
    {
        switch (node)
        {
            case DictionaryNode dict:
                writer.WriteStartDictionary();
                foreach (var item in dict)
                {
                    writer.WritePropertyName(item.Key);
                    WriteNode(writer, item.Value);
                }
                writer.WriteEndDictionary();
                break;
            case ArrayNode array:
                writer.WriteStartArray();
                foreach (var item in array) WriteNode(writer, item);
                writer.WriteEndArray();
                break;
            case ValueNode value:
                writer.WriteValue(value.GetValue());
                break;
            case PathNode path:
                writer.WriteStartDictionary();
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
                writer.WriteEndDictionary();
                break;
            case BinaryNode binary:
                writer.WriteStartDictionary();
                writer.WritePropertyName("size");
                writer.WriteValue(binary.Size);
                writer.WritePropertyName("data");
                writer.WriteValue(binary.Data.ToHexString(true));
                writer.WriteEndDictionary();
                break;
            case BinaryParamNode binaryParam:
                writer.WriteStartDictionary();
                writer.WritePropertyName("size");
                writer.WriteValue(binaryParam.Size);
                writer.WritePropertyName("param");
                writer.WriteValue(binaryParam.Param);
                writer.WritePropertyName("data");
                writer.WriteValue(binaryParam.Data.ToHexString(true));
                writer.WriteEndDictionary();
                break;
            case NullNode:
                writer.WriteNull();
                break;
        }
    }
    #endregion
}