namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class holding information about a BYML file.
/// </summary>
public class BymlFile
{
    #region public properties
    /// <summary>
    /// Gets the version of the BYML file.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets the root <see cref="Node"/> object.
    /// </summary>
    public Node RootNode { get; set; } = null!;
    #endregion

    #region public methods
    /// <inheritdoc cref="Node.Find"/>
    public Node? Find(string path) => RootNode.Find(path);

    /// <inheritdoc cref="Node.Find{T}"/>
    public T? Find<T>(string path) where T : Node => RootNode.Find<T>(path);
    #endregion
}