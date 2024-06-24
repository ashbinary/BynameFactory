using System;
using System.IO;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A collection of extension methods for <see cref="IAampSerializer"/> classes.
/// </summary>
public static class AampSerializerExtensions
{
    /// <summary>
    /// Serializes a <see cref="AampFile"/> object.
    /// </summary>
    /// <param name="serializer">The <see cref="IAampSerializer"/> instance to use.</param>
    /// <param name="aampFile">The <see cref="AampFile"/> object to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IAampSerializer serializer, AampFile aampFile)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (aampFile is null) throw new ArgumentNullException(nameof(aampFile));

        using var writer = new StringWriter();
        serializer.Serialize(writer, aampFile);
        return writer.ToString();
    }
}