using System;
using System.IO;

namespace NintendoTools.FileFormats.Bcsv;

/// <summary>
/// A collection of extension methods for <see cref="IBcsvSerializer"/> classes.
/// </summary>
public static class BcsvSerializerExtensions
{
    /// <summary>
    /// Serializes a <see cref="BcsvFile"/> object.
    /// </summary>
    /// <param name="serializer">The <see cref="IBcsvSerializer"/> instance to use.</param>
    /// <param name="bymlFile">The <see cref="BcsvFile"/> object to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IBcsvSerializer serializer, BcsvFile bymlFile)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (bymlFile is null) throw new ArgumentNullException(nameof(bymlFile));

        using var writer = new StringWriter();
        serializer.Serialize(writer, bymlFile);
        return writer.ToString();
    }
}