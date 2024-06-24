using System;
using System.Collections.Generic;
using NintendoTools.FileFormats.Bcsv.Attributes;
using NintendoTools.Hashing;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Bcsv.Converters;

/// <summary>
/// A converter to convert hex strings to CRC32-hashed <see langword="enum"/> values.
/// Uses the name of the <see langword="enum"/> value or a <see cref="BcsvCrc32EnumNameAttribute"/> value to compute the CRC32 hash.
/// </summary>
public class BcsvCrc32EnumConverter : IBcsvConverter
{
    #region private members
    private Dictionary<string, Enum>? _valueCache;
    private readonly IHashAlgorithm _hashAlgorithm = new Crc32Hash();
    #endregion

    #region IBcsvConverter interface
    /// <inheritdoc/>
    public object? Convert(byte[] data, Type targetType)
    {
        if (!targetType.IsEnum) throw new InvalidCastException("Cannot convert non-enum type to enum.");
        _valueCache ??= BuildCache(targetType);

        Array.Reverse(data);
        var hash = data.ToHexString();
        return _valueCache.TryGetValue(hash, out var enumValue) ? enumValue : default;
    }
    #endregion

    #region private methods
    private Dictionary<string, Enum> BuildCache(Type enumType)
    {
        var cache = new Dictionary<string, Enum>();

        foreach (Enum value in Enum.GetValues(enumType))
        {
            var name = value.ToString();

            var info = enumType.GetMember(name);
            var attr = info[0].GetCustomAttributes(typeof(BcsvCrc32EnumNameAttribute), true);
            if (attr.Length > 0 && attr[0] is BcsvCrc32EnumNameAttribute crc32Name) name = crc32Name.Name;

            cache.Add(_hashAlgorithm.Compute(name), value);
        }

        return cache;
    }
    #endregion
}