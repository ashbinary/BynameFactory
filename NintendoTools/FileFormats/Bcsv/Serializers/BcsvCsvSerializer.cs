using System;
using System.IO;
using System.Linq;

namespace NintendoTools.FileFormats.Bcsv;

/// <summary>
/// A class for serializing <see cref="BcsvFile"/> objects to CSV.
/// </summary>
public class BcsvCsvSerializer : IBcsvSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets the separator character that should be used.
    /// The default value is '<c>,</c>'.
    /// </summary>
    public string Separator { get; set; } = ",";
    #endregion

    #region IBcsvSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BcsvFile bcsvFile)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (bcsvFile is null) throw new ArgumentNullException(nameof(bcsvFile));

        for (var i = 0; i < bcsvFile.Columns; ++i)
        {
            if (i > 0) writer.Write(Separator);
            writer.Write(bcsvFile.HeaderInfo[i].NewHeaderName);
        }
        writer.WriteLine();

        foreach (var entry in bcsvFile)
        {
            for (var i = 0; i < entry.Length; ++i)
            {
                if (i > 0) writer.Write(Separator);
                if (entry[i] is null) continue;

                var text = entry[i]?.ToString() ?? string.Empty;
                var wrapText = text.Contains(Separator) || text.Contains('\n');
                if (wrapText && text.Contains('"')) text = text.Replace("\"", "\"\"");
                writer.Write(wrapText ? '"' + text + '"' : text);
            }

            writer.WriteLine();
        }

        writer.Flush();
        writer.Close();
    }
    #endregion
}