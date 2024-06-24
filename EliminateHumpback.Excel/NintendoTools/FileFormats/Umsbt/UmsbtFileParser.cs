using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NintendoTools.FileFormats.Msbt;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Umsbt;

/// <summary>
/// A class for parsing UMSBT archives.
/// </summary>
public class UmsbtFileParser : IFileParser<IDictionary<string, IList<MsbtMessage>>>
{
    #region private members
    private readonly string[] _languages;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UmsbtFileParser"/> class without any language mappings.
    /// </summary>
    public UmsbtFileParser() : this(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmsbtFileParser"/> class with the given languages.
    /// Each language maps to the MSBT file within the archive in the same order.
    /// </summary>
    /// <param name="languages">The list of languages found in the UMSBT archive.</param>
    public UmsbtFileParser(IEnumerable<string>? languages) => _languages = languages is null ? Array.Empty<string>() : languages.ToArray();
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public IDictionary<string, IList<MsbtMessage>> Parse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var reader = new FileReader(fileStream);

        //find file offsets and sizes
        GetMetaData(reader, out var offsets, out var sizes);

        //read UMSBT files
        var result = new Dictionary<string, IList<MsbtMessage>>();
        for (var i = 0; i < offsets.Length; ++i)
        {
            var language = i < _languages.Length ? _languages[i] : null;
            var parser = new MsbtFileParser(language);
            var stream = new StreamSpan(reader.BaseStream, offsets[i], sizes[i]);
            IList<MsbtMessage> messages;
            try
            {
                messages = parser.Parse(stream);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("File is not an UMSBT file.", ex);
            }

            result.Add(language ?? $"{i+1:D3}", messages);
        }

        return result;
    }
    #endregion

    #region private methods
    //verifies that the file is a UMSBT file
    private static bool CanParse(FileReader reader)
    {
        GetMetaData(reader, out var offsets, out var sizes);
        if (offsets.Length == 0) return false;

        var parser = new MsbtFileParser();
        for (var i = 0; i < offsets.Length; ++i)
        {
            var stream = new StreamSpan(reader.BaseStream, offsets[i], sizes[i]);
            if (!parser.CanParse(stream)) return false;
        }

        return true;
    }

    //parses meta data
    private static void GetMetaData(FileReader reader, out int[] offsets, out int[] sizes)
    {
        var offsetList = new List<int>();
        var sizeList = new List<int>();

        var dataStart = reader.ReadInt32At(0);
        reader.Position = 0;

        try
        {
            while (reader.Position < dataStart)
            {
                var offset = reader.ReadInt32();
                var size = reader.ReadInt32();
                if (offset <= 0 || size <= 0) break;

                offsetList.Add(offset);
                sizeList.Add(size);
            }

            offsets = offsetList.ToArray();
            sizes = sizeList.ToArray();
        }
        catch
        {
            offsets = Array.Empty<int>();
            sizes = Array.Empty<int>();
        }
    }
    #endregion
}