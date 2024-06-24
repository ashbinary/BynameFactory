namespace NintendoTools.FileFormats.Aamp;

/// <summary>
/// A class representing a curve parameter value in an AAMP file.
/// </summary>
public class CurveValue
{
    //???
    public uint[] IntValues { get; set; } = null!;

    //???
    public float[] FloatValues { get; set; } = null!;
}