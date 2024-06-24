namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// Class that holds BYML node types.
/// </summary>
public static class NodeTypes
{
    public const byte String = 0xa0;
    public const byte Path = 0xa1;
    public const byte Binary = 0xa1;
    public const byte BinaryParam = 0xa2;

    public const byte Array = 0xc0;
    public const byte Dictionary = 0xc1;
    public const byte StringTable = 0xc2;
    public const byte PathTable = 0xc3;

    public const byte Bool = 0xd0;
    public const byte Int = 0xd1;
    public const byte Float = 0xd2;
    public const byte UInt = 0xd3;
    public const byte Long = 0xd4;
    public const byte ULong = 0xd5;
    public const byte Double = 0xd6;

    public const byte Null = 0xff;
}