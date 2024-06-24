namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for a node containing binary data.
/// </summary>
public class BinaryNode : Node
{
    /// <inheritdoc/>
    public override byte Type => NodeTypes.Binary;

    /// <summary>
    /// Gets the size value of the node.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the binary data of the node.
    /// </summary>
    public byte[] Data { get; set; } = null!;
}