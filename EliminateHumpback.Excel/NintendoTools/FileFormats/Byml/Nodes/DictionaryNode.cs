using System.Collections;
using System.Collections.Generic;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// A class for an dictionary-type node.
/// </summary>
public class DictionaryNode : Node, IEnumerable<KeyValuePair<string, Node>>
{
    #region private members
    private readonly Dictionary<string, Node> _nodes = new();
    #endregion

    #region public properties
    /// <inheritdoc/>
    public override byte Type => NodeTypes.Dictionary;

    /// <summary>
    /// Gets the number of <see cref="Node"/> objects in this dictionary.
    /// </summary>
    public int Count => _nodes.Count;

    /// <summary>
    /// Retrieves a <see cref="Node"/> object with a given name/key from the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="Node"/> object to retrieve.</param>
    /// <returns>The <see cref="Node"/> object with the given index.</returns>
    public Node this[string key] => _nodes[key];
    #endregion

    #region public methods
    /// <summary>
    /// Determines whether a <see cref="Node"/> object with a given name/key exists in the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="Node"/> object to find.</param>
    /// <returns>A value indicating whether the <see cref="Node"/> object was found.</returns>
    public bool Contains(string key) => _nodes.ContainsKey(key);

    /// <summary>
    /// Adds a new <see cref="Node"/> object to the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="Node"/> object to add.</param>
    /// <param name="node">The <see cref="Node"/> object to add.</param>
    public void Add(string key, Node node) => _nodes.Add(key, node);

    /// <summary>
    /// Removes a <see cref="Node"/> object from the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="Node"/> object to remove.</param>
    public void Remove(string key) => _nodes.Remove(key);

    /// <summary>
    /// Changes the name/key of a <see cref="Node"/> object in the dictionary.
    /// </summary>
    /// <param name="oldKey">The old name/key of the <see cref="Node"/> object.</param>
    /// <param name="newKey">The new name/key of the <see cref="Node"/> object.</param>
    public void Rename(string oldKey, string newKey)
    {
        var node = _nodes[oldKey];
        Remove(oldKey);
        Add(newKey, node);
    }
    #endregion

    #region IEnumerable interface
    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, Node>> GetEnumerator() => _nodes.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}