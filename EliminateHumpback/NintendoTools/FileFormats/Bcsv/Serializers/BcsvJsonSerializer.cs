using System;
using System.IO;
using Newtonsoft.Json;

namespace NintendoTools.FileFormats.Bcsv;

/// <summary>
/// A class for serializing <see cref="BcsvFile"/> objects to JSON.
/// </summary>
public class BcsvJsonSerializer : IBcsvSerializer
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

    #region IBcsvSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BcsvFile bcsvFile)
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (bcsvFile is null) throw new ArgumentNullException(nameof(bcsvFile));

        using var jsonWriter = new JsonTextWriter(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        jsonWriter.WriteStartArray();

        foreach (var entry in bcsvFile)
        {
            jsonWriter.WriteStartObject();

            for (var i = 0; i < entry.Length; ++i)
            {
                jsonWriter.WritePropertyName(bcsvFile.HeaderInfo[i].NewHeaderName);
                jsonWriter.WriteValue(entry[i] ?? string.Empty);
            }

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndArray();
    }
    #endregion
}