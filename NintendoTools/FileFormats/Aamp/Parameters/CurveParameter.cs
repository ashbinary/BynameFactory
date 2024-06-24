using System.Collections.Generic;

namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class representing a curve parameter in an AAMP file.
/// </summary>
public class CurveParameter : Parameter
{
    /// <summary>
    /// Gets or sets a list of <see cref="CurveValue"/> items for this curve parameter.
    /// </summary>
    public IList<CurveValue> Curves { get; set; } = new List<CurveValue>();
}