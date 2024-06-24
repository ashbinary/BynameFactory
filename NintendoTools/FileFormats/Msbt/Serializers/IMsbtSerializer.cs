using System.Collections.Generic;
using System.IO;

namespace NintendoTools.FileFormats.Msbt;

/// <summary>
/// An interface for serializing a collection of <see cref="MsbtMessage"/> objects.
/// </summary>
public interface IMsbtSerializer
{
    #region properties
    /// <summary>
    /// Gets or sets the function table to use.
    /// </summary>
    IMsbtFunctionTable FunctionTable { get; set; }

    /// <summary>
    /// Gets or sets the message format provider to use.
    /// </summary>
    IMsbtFormatProvider FormatProvider { get; set; }
    #endregion

    #region methods
    /// <summary>
    /// Serializes a collection of <see cref="MsbtMessage"/> objects.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="messages">The collection of <see cref="MsbtMessage"/> objects to serialize.</param>
    void Serialize(TextWriter writer, IEnumerable<MsbtMessage> messages);

    /// <summary>
    /// Serializes a collection of <see cref="MsbtMessage"/> objects from multiple languages.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="messages">The dictionary of language strings and <see cref="MsbtMessage"/> objects to serialize.</param>
    void Serialize(TextWriter writer, IDictionary<string, IEnumerable<MsbtMessage>> messages);
    #endregion
}