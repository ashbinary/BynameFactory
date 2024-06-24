namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for a null-type node.
/// </summary>
public class NullNode : Node
{
    /// <inheritdoc/>
    public override byte Type => NodeTypes.Null;
}