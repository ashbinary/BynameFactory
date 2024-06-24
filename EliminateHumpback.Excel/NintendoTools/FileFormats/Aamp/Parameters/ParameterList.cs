using System.Collections.Generic;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class representing a parameter list in an AAMP file.
/// </summary>
public class ParameterList
{
    #region public properties
    /// <summary>
    /// Gets or sets the name of the list.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets a list of <see cref="ParameterObject"/> instances.
    /// </summary>
    public IList<ParameterObject> Objects { get; } = new List<ParameterObject>();

    /// <summary>
    /// Gets a list of <see cref="ParameterList"/> instances.
    /// </summary>
    public IList<ParameterList> Lists { get; } = new List<ParameterList>();
    #endregion
}