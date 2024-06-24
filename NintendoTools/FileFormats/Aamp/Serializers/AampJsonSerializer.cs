using System;
using System.IO;
using Newtonsoft.Json;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class for serializing <see cref="AampFile"/> objects to JSON.
/// </summary>
public class AampJsonSerializer : IAampSerializer
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

    #region IAampSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, AampFile aampFile)
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (aampFile is null) throw new ArgumentNullException(nameof(aampFile));

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
        jsonWriter.WriteValue(aampFile.Version);
        WriteList(jsonWriter, aampFile.Root);
        jsonWriter.WriteEndObject();
    }
    #endregion

    #region private methods
    private static void WriteList(JsonWriter writer, ParameterList list)
    {
        writer.WritePropertyName(list.Name);
        writer.WriteStartObject();
        foreach (var subList in list.Lists)
        {
            WriteList(writer, subList);
        }
        foreach (var obj in list.Objects)
        {
            WriteObject(writer, obj);
        }
        writer.WriteEndObject();
    }

    private static void WriteObject(JsonWriter writer, ParameterObject obj)
    {
        writer.WritePropertyName(obj.Name);
        writer.WriteStartObject();
        foreach (var parameter in obj.Parameters)
        {
            WriteParameter(writer, parameter);
        }
        writer.WriteEndObject();
    }

    private static void WriteParameter(JsonWriter writer, Parameter parameter)
    {
        var defaultFormat = writer.Formatting;
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
                    writer.WriteStartObject();
                    writer.WritePropertyName("intValues");
                    writer.WriteStartArray();
                    foreach (var intVal in value.IntValues) writer.WriteValue(intVal);
                    writer.WriteEndArray();
                    writer.WritePropertyName("floatValues");
                    writer.WriteStartArray();
                    foreach (var floatVal in value.FloatValues) writer.WriteValue(floatVal);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                break;
            case ValueParameter value:
                if (value.Value is Array array)
                {
                    writer.Formatting = Formatting.None;
                    writer.WriteStartArray();
                    foreach (var item in array) writer.WriteValue(item);
                    writer.WriteEndArray();
                    writer.Formatting = defaultFormat;
                }
                else writer.WriteValue(value.Value);
                break;
        }
    }
    #endregion
}