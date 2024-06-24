using System;
using System.IO;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class for serializing <see cref="AampFile"/> objects to JSON.
/// </summary>
public class AampYamlSerializer : IAampSerializer
{
    #region IAampSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, AampFile aampFile)
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (aampFile is null) throw new ArgumentNullException(nameof(aampFile));

        using var yamlWriter = new YamlTextWriter(writer);

        yamlWriter.WriteStartDocument();
        yamlWriter.WritePropertyName("version");
        yamlWriter.WriteValue(aampFile.Version);
        WriteList(yamlWriter, aampFile.Root);
    }
    #endregion

    #region private methods
    private static void WriteList(YamlTextWriter writer, ParameterList list)
    {
        writer.WritePropertyName(list.Name);
        writer.WriteStartDictionary();
        foreach (var subList in list.Lists)
        {
            WriteList(writer, subList);
        }
        foreach (var obj in list.Objects)
        {
            WriteObject(writer, obj);
        }
        writer.WriteEndDictionary();
    }

    private static void WriteObject(YamlTextWriter writer, ParameterObject obj)
    {
        writer.WritePropertyName(obj.Name);
        writer.WriteStartDictionary();
        foreach (var parameter in obj.Parameters)
        {
            WriteParameter(writer, parameter);
        }
        writer.WriteEndDictionary();
    }

    private static void WriteParameter(YamlTextWriter writer, Parameter parameter)
    {
        writer.WritePropertyName(parameter.Name);
        switch (parameter)
        {
            case ColorParameter color:
                writer.WriteStartArray();
                writer.WriteValue(color.Red);
                writer.WriteValue(color.Green);
                writer.WriteValue(color.Blue);
                writer.WriteValue(color.Alpha);
                writer.WriteEndArray();
                break;
            case CurveParameter curve:
                writer.WriteStartArray();
                foreach (var value in curve.Curves)
                {
                    writer.WriteStartDictionary();
                    writer.WritePropertyName("intValues");
                    writer.WriteStartArray();
                    foreach (var intVal in value.IntValues) writer.WriteValue(intVal);
                    writer.WriteEndArray();
                    writer.WritePropertyName("floatValues");
                    writer.WriteStartArray();
                    foreach (var floatVal in value.FloatValues) writer.WriteValue(floatVal);
                    writer.WriteEndArray();
                    writer.WriteEndDictionary();
                }
                writer.WriteEndArray();
                break;
            case ValueParameter value:
                if (value.Value is Array array)
                {
                    writer.WriteStartArray();
                    foreach (var item in array) writer.WriteValue(item);
                    writer.WriteEndArray();
                }
                else writer.WriteValue(value.Value);
                break;
        }
    }
    #endregion
}