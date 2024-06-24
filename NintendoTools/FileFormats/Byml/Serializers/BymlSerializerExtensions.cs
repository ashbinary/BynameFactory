using System;
using System.IO;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A collection of extension methods for <see cref="IBymlSerializer"/> classes.
/// </summary>
public static class BymlSerializerExtensions
{
    /// <summary>
    /// Serializes a <see cref="BymlFile"/> object.
    /// </summary>
    /// <param name="serializer">The <see cref="IBymlSerializer"/> instance to use.</param>
    /// <param name="bymlFile">The <see cref="BymlFile"/> object to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IBymlSerializer serializer, BymlFile bymlFile)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (bymlFile is null) throw new ArgumentNullException(nameof(bymlFile));

        using var writer = new StringWriter();
        serializer.Serialize(writer, bymlFile);
        return writer.ToString();
    }
}