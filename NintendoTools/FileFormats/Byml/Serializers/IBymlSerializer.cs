using System.IO;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// An interface for serializing <see cref="BymlFile"/> objects.
/// </summary>
public interface IBymlSerializer
{
    /// <summary>
    /// Serializes a <see cref="BymlFile"/> object.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="bymlFile">The <see cref="BymlFile"/> object to serialize.</param>
    void Serialize(TextWriter writer, BymlFile bymlFile);
}