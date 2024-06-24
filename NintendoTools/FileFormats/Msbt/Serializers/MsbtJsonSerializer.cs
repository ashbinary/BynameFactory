using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// A class for serializing a collection of <see cref="MsbtMessage"/> objects to JSON.
/// </summary>
public class MsbtJsonSerializer : IMsbtSerializer
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
    /// Determines whether the serialized result should be an object where each message is a property instead of an array of message objects.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool WriteAsObject { get; set; }

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtJsonFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IEnumerable<MsbtMessage> messages)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        using var jsonWriter = new JsonTextWriter(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        if (WriteAsObject) //write one big object, only containing labels and texts
        {
            jsonWriter.WriteStartObject();

            foreach (var message in messages)
            {
                jsonWriter.WritePropertyName(message.Label);
                jsonWriter.WriteValue(message.ToCompiledString(FunctionTable, FormatProvider));
            }

            jsonWriter.WriteEndObject();
        }
        else //write array of full message objects
        {
            jsonWriter.WriteStartArray();

            foreach (var message in messages)
            {
                jsonWriter.WriteStartObject();

                jsonWriter.WritePropertyName("label");
                jsonWriter.WriteValue(message.Label);

                if (!IgnoreAttributes)
                {
                    jsonWriter.WritePropertyName("attribute");
                    jsonWriter.WriteValue(message.Attribute.ToHexString(true));
                }

                jsonWriter.WritePropertyName("text");
                jsonWriter.WriteValue(message.ToCompiledString(FunctionTable, FormatProvider));

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }
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

        using var jsonWriter = new JsonTextWriter(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        jsonWriter.WriteStartArray();

        //ensure original sort order persists
        foreach (var orderMessage in messages.Values.First())
        {
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("label");
            jsonWriter.WriteValue(orderMessage.Label);

            if (!IgnoreAttributes)
            {
                jsonWriter.WritePropertyName("attribute");
                jsonWriter.WriteValue(orderMessage.Attribute.ToHexString(true));
            }

            jsonWriter.WritePropertyName("locale");
            jsonWriter.WriteStartObject();
            for (var i = 0; i < remappedMessages[orderMessage.Label].Length; ++i)
            {
                jsonWriter.WritePropertyName(languages[i]);
                var message = remappedMessages[orderMessage.Label][i];
                jsonWriter.WriteValue(message is null ? string.Empty : message.ToCompiledString(FunctionTable, FormatProvider));
            }
            jsonWriter.WriteEndObject();

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndArray();
    }
    #endregion
}