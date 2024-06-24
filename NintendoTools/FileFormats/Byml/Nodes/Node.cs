namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// The base class for all nodes.
/// </summary>
public abstract class Node
{
    #region public properties
    /// <summary>
    /// Gets or sets the type of the node.
    /// </summary>
    public virtual byte Type { get; set; } = NodeTypes.Null;
    #endregion

    #region public methods
    /// <summary>
    /// Finds a child <see cref="Node"/> element from a given path.
    /// Path elements have to be separated by a '/'.
    /// </summary>
    /// <param name="path">The path to browse.</param>
    /// <returns>The <see cref="Node"/> object from the given path; returns <see langword="null"/> if no node was found.</returns>
    public Node? Find(string path)
    {
        var parts = path.Split('/');
        var node = this;

        foreach (var element in parts)
        {
            if (node is null) break;

            switch (node)
            {
                case DictionaryNode dict:
                    node = dict.Contains(element) ? dict[element] : null;
                    break;
                case ArrayNode array:
                    var index = int.Parse(element);
                    node = array.Count > index ? array[index] : null;
                    break;
                default:
                    node = null;
                    break;
            }
        }

        return node;
    }

    /// <summary>
    /// Finds a child <see cref="Node"/> element from a given path.
    /// Path elements have to be separated by a '/'.
    /// </summary>
    /// <param name="path">The path to browse.</param>
    /// <returns>The <see cref="Node"/> object from the given path; returns <see langword="null"/> if no node was found.</returns>
    public T? Find<T>(string path) where T : Node => Find(path) as T;
    #endregion
}