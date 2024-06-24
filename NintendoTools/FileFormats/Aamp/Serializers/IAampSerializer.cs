using System.IO;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// An interface for serializing <see cref="AampFile"/> objects.
/// </summary>
public interface IAampSerializer
{
    /// <summary>
    /// Serializes a <see cref="AampFile"/> object.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="aampFile">The <see cref="AampFile"/> object to serialize.</param>
    void Serialize(TextWriter writer, AampFile aampFile);
}