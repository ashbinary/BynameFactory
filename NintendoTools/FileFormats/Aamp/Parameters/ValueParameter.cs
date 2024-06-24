namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class representing a value parameter in an AAMP file.
/// </summary>
public class ValueParameter : Parameter
{
    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    public object Value { get; set; } = default!;
}