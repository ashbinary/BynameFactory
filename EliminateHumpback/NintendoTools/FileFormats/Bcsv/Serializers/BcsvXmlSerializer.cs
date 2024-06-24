using System;
using System.IO;
using System.Xml;

namespace NintendoTools.FileFormats.Bcsv;

/// <summary>
/// A class for serializing <see cref="BcsvFile"/> objects to XML.
/// </summary>
public class BcsvXmlSerializer : IBcsvSerializer
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

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();

        foreach (var entry in bcsvFile)
        {
            xmlWriter.WriteStartElement("entry");

            for (var i = 0; i < entry.Length; ++i)
            {
                xmlWriter.WriteStartElement(bcsvFile.HeaderInfo[i].NewHeaderName);
                xmlWriter.WriteRaw(entry[i]?.ToString() ?? string.Empty);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndDocument();
    }
    #endregion
}