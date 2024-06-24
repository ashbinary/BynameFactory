using System.Collections;
using System.Collections.Generic;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for an array-type node.
/// </summary>
public class ArrayNode : Node, IEnumerable<Node>
{
    #region private members
    private readonly List<Node> _nodes = new();
    #endregion

    #region public properties
    /// <inheritdoc/>
    public override byte Type => NodeTypes.Array;

    /// <summary>
    /// Gets the number of <see cref="Node"/> objects in this array.
    /// </summary>
    public int Count => _nodes.Count;

    /// <summary>
    /// Retrieves a <see cref="Node"/> object with a given index from the array.
    /// </summary>
    /// <param name="index">The zero-based index of the <see cref="Node"/> object to retrieve.</param>
    /// <returns>The <see cref="Node"/> object with the given index.</returns>
    public Node this[int index] => _nodes[index];
    #endregion

    #region public methods
    /// <summary>
    /// Adds a new <see cref="Node"/> object to the end of the array.
    /// </summary>
    /// <param name="node">The <see cref="Node"/> object to add.</param>
    public void Add(Node node) => _nodes.Add(node);
    #endregion

    #region IEnumerable interface
    /// <inheritdoc />
    public IEnumerator<Node> GetEnumerator() => _nodes.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}