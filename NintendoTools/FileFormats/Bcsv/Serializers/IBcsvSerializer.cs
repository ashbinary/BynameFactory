using System.IO;

namespace NintendoTools.FileFormats.Bcsv;

/// <summary>
/// An interface for serializing <see cref="BcsvFile"/> objects.
/// </summary>
public interface IBcsvSerializer
{
    /// <summary>
    /// Serializes a <see cref="BcsvFile"/> object.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="bcsvFile">The <see cref="BcsvFile"/> object to serialize.</param>
    void Serialize(TextWriter writer, BcsvFile bcsvFile);
}