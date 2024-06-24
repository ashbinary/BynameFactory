using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NintendoTools.FileFormats.Msbt;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Bmg;

/// <summary>
/// A class for serializing a collection of <see cref="MsbtMessage"/> objects to XML.
/// </summary>
public class BmgXmlSerializer : IMsbtSerializer
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

    /// <summary>
    /// Determines whether to ignore attribute values in the output.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool IgnoreAttributes { get; set; }
    #endregion

    #region IMsbtSerializer interface
    /// <inheritdoc />
    public IMsbtFunctionTable FunctionTable { get; set; } = new MsbtDefaultFunctionTable();

    /// <inheritdoc />
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtXmlFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IEnumerable<MsbtMessage> messages)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("bmg");

        foreach (var message in messages)
        {
            xmlWriter.WriteStartElement("message");
            xmlWriter.WriteAttributeString("label", message.Label);
            var attrStr = message.Attribute.ToHexString(true);
            if (!IgnoreAttributes) xmlWriter.WriteAttributeString("attribute", attrStr);
            xmlWriter.WriteRaw(message.ToCompiledString(FunctionTable, FormatProvider));
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IDictionary<string, IEnumerable<MsbtMessage>> messages)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        var languages = messages.Keys.ToArray();

        //merge messages by label
        var remappedMessages = new Dictionary<string, MsbtMessage?[]>();
        foreach (var item in messages)
        {
            foreach (var message in item.Value)
            {
                if (!remappedMessages.ContainsKey(message.Label)) remappedMessages.Add(message.Label, new MsbtMessage?[languages.Length]);
                remappedMessages[message.Label][Array.IndexOf(languages, item.Key)] = message;
            }
        }

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("bmg");

        //ensure original sort order persists
        foreach (var orderMessage in messages.Values.First())
        {
            xmlWriter.WriteStartElement("message");
            xmlWriter.WriteAttributeString("label", orderMessage.Label);

            var attrStr = orderMessage.Attribute.ToHexString(true);
            if (!IgnoreAttributes) xmlWriter.WriteAttributeString("attribute", attrStr);

            for (var i = 0; i < remappedMessages[orderMessage.Label].Length; ++i)
            {
                xmlWriter.WriteStartElement("language");
                xmlWriter.WriteAttributeString("type", languages[i]);
                var message = remappedMessages[orderMessage.Label][i];
                if (message is not null) xmlWriter.WriteRaw(message.ToCompiledString(FunctionTable, FormatProvider));
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }
    #endregion
}