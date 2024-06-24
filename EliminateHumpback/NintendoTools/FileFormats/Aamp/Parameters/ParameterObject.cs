using System.Collections;
using System.Collections.Generic;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class representing a parameter object in an AAMP file.
/// </summary>
public class ParameterObject : IEnumerable<Parameter>
{
    #region public properties
    /// <summary>
    /// Gets or sets the name of the object.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets a list of <see cref="Parameter"/> instances.
    /// </summary>
    public IList<Parameter> Parameters { get; } = new List<Parameter>();
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<Parameter> GetEnumerator() => Parameters.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}