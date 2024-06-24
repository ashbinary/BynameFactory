namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for a path-type node.
/// </summary>
public class PathNode : Node
{
    /// <inheritdoc/>
    public override byte Type => NodeTypes.Path;

    /// <summary>
    /// Gets or sets the X coordinate of the position.
    /// </summary>
    public float PositionX { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the position.
    /// </summary>
    public float PositionY { get; set; }

    /// <summary>
    /// Gets or sets the Z coordinate of the position.
    /// </summary>
    public float PositionZ { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate of the normal.
    /// </summary>
    public float NormalX { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the normal.
    /// </summary>
    public float NormalY { get; set; }

    /// <summary>
    /// Gets or sets the Z coordinate of the normal.
    /// </summary>
    public float NormalZ { get; set; }
}