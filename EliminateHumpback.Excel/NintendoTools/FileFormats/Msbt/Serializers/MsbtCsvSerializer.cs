using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// A class for serializing a collection of <see cref="MsbtMessage"/> objects to CSV.
/// </summary>
public class MsbtCsvSerializer : IMsbtSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets the separator character that should be used.
    /// The default value is '<c>,</c>'.
    /// </summary>
    public string Separator { get; set; } = ",";

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtDefaultFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IEnumerable<MsbtMessage> messages)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        //write header
        writer.Write("Label");
        if (!IgnoreAttributes)
        {
            writer.Write(Separator);
            writer.Write("Attribute");
        }
        writer.Write(Separator);
        writer.WriteLine("Text");

        //write messages
        foreach (var message in messages)
        {
            writer.Write(message.Label);

            if (!IgnoreAttributes)
            {
                writer.Write(Separator);
                writer.Write(message.Attribute.ToHexString(true));
            }

            writer.Write(Separator);
            var text = message.ToCompiledString(FunctionTable, FormatProvider);
            var wrapText = text.Contains(Separator) || text.Contains('\n');
            if (wrapText && text.Contains('"')) text = text.Replace("\"", "\"\"");
            writer.WriteLine(wrapText ? '"' + text + '"' : text);
        }

        writer.Flush();
        writer.Close();
    }

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IDictionary<string, IEnumerable<MsbtMessage>> messages)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
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

        //write header
        writer.Write("Label");
        if (!IgnoreAttributes)
        {
            writer.Write(Separator);
            writer.Write("Attribute");
        }
        foreach (var language in languages)
        {
            writer.Write(Separator);
            writer.Write(language);
        }
        writer.WriteLine();

        //ensure original sort order persists
        foreach (var orderMessage in messages.Values.First())
        {
            writer.Write(orderMessage.Label);

            if (!IgnoreAttributes)
            {
                writer.Write(Separator);
                writer.Write(orderMessage.Attribute.ToHexString(true));
            }

            foreach (var message in remappedMessages[orderMessage.Label])
            {
                writer.Write(Separator);
                if (message is null) continue;

                var text = message.ToCompiledString(FunctionTable, FormatProvider);
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