namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class holding information about a AAMP file.
/// </summary>
public class AampFile
{
    /// <summary>
    /// Gets the version of the AAMP file.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets the root parameter list of the AAMP file.
    /// </summary>
    public ParameterList Root { get; set; } = null!;
}