namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for a node containing binary data with params.
/// </summary>
public class BinaryParamNode : Node
{
    /// <inheritdoc/>
    public override byte Type => NodeTypes.BinaryParam;

    /// <summary>
    /// Gets the size value of the node.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets the param value of the node.
    /// </summary>
    public int Param { get; set; }

    /// <summary>
    /// Gets or sets the binary data of the node.
    /// </summary>
    public byte[] Data { get; set; } = null!;
}