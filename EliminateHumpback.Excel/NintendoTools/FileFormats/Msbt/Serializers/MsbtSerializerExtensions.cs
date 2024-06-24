using System;
using System.Collections.Generic;
using System.IO;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// A collection of extension methods for <see cref="IMsbtSerializer"/> classes.
/// </summary>
public static class MsbtSerializerExtensions
{
    /// <summary>
    /// Serializes a collection of <see cref="MsbtMessage"/> objects from multiple languages.
    /// </summary>
    /// <param name="serializer">The <see cref="IMsbtSerializer"/> instance to use.</param>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="messages">The dictionary of language strings and <see cref="MsbtMessage"/> objects to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static void Serialize(this IMsbtSerializer serializer, TextWriter writer, IDictionary<string, IList<MsbtMessage>> messages)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        var castedMessages = new Dictionary<string, IEnumerable<MsbtMessage>>();
        foreach (var entry in messages) castedMessages.Add(entry.Key, entry.Value);

        serializer.Serialize(writer, castedMessages);
    }

    /// <summary>
    /// Serializes a collection of <see cref="MsbtMessage"/> objects.
    /// </summary>
    /// <param name="serializer">The <see cref="IMsbtSerializer"/> instance to use.</param>
    /// <param name="messages">The collection of <see cref="MsbtMessage"/> objects to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IMsbtSerializer serializer, IEnumerable<MsbtMessage> messages)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        using var writer = new StringWriter();
        serializer.Serialize(writer, messages);
        return writer.ToString();
    }

    /// <summary>
    /// Serializes a collection of <see cref="MsbtMessage"/> objects from multiple languages.
    /// </summary>
    /// <param name="serializer">The <see cref="IMsbtSerializer"/> instance to use.</param>
    /// <param name="messages">The dictionary of language strings and <see cref="MsbtMessage"/> objects to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IMsbtSerializer serializer, IDictionary<string, IEnumerable<MsbtMessage>> messages)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        using var writer = new StringWriter();
        serializer.Serialize(writer, messages);
        return writer.ToString();
    }

    /// <summary>
    /// Serializes a collection of <see cref="MsbtMessage"/> objects from multiple languages.
    /// </summary>
    /// <param name="serializer">The <see cref="IMsbtSerializer"/> instance to use.</param>
    /// <param name="messages">The dictionary of language strings and <see cref="MsbtMessage"/> objects to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IMsbtSerializer serializer, IDictionary<string, IList<MsbtMessage>> messages)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        var castedMessages = new Dictionary<string, IEnumerable<MsbtMessage>>();
        foreach (var entry in messages) castedMessages.Add(entry.Key, entry.Value);

        using var writer = new StringWriter();
        serializer.Serialize(writer, castedMessages);
        return writer.ToString();
    }
}