using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NintendoTools.Utils;

namespace NintendoTools.FileFormats.Sarc;

/// <summary>
/// A class for parsing SARC archives.
/// </summary>
public class SarcFileParser : IFileParser<IList<SarcFile>>
{
    #region public methods
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public IList<SarcFile> Parse(Stream fileStream)
    {
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));

        var reader = new FileReader(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a SARC file.");

        //parse meta data
        var dataOffset = reader.ReadInt32At(12);
        var fileCount = reader.ReadInt16At(26);
        var hashKey = reader.ReadUInt32();

        var files = new Dictionary<SarcFile, uint>();

        //parse files
        if (reader.ReadStringAt(20, 4, Encoding.ASCII) == "SFAT")
        {
            for (var i = 0; i < fileCount; ++i)
            {
                reader.JumpTo(32 + i * 16);
                var fileHash = reader.ReadBytes(4);
                //var nameOffset = reader.ReadUInt32(3) / 4; //index into SFNT array
                //var hashIndex = reader.ReadByte(); //increments for each hash collision
                reader.Skip(4);
                var fileOffset = reader.ReadInt32();
                var endOfFile = reader.ReadInt32();

                var file = new SarcFile
                {
                    Name = fileHash.ToHexString(),
                    Content = reader.ReadBytesAt(dataOffset + fileOffset, endOfFile - fileOffset)
                };

                files.Add(file, BitConverter.ToUInt32(fileHash, 0));
            }
        }

        //parse file names
        if (reader.ReadStringAt(32 + fileCount * 16, 4, Encoding.ASCII) == "SFNT")
        {
            var fileNameOffset = 40 + fileCount * 16;
            var fileNames = reader.ReadBytesAt(fileNameOffset, dataOffset - fileNameOffset);

            var index = 0;
            var lastNameIndex = 0;
            while (index < fileNames.Length)
            {
                if (fileNames[index] == '\0' || index + 1 == fileNames.Length) //end of a file name or padding
                {
                    if (index - lastNameIndex > 0)
                    {
                        //compute hash from file name
                        uint hash = 0;
                        for (var i = lastNameIndex; i < index; ++i)
                        {
                            hash = hash * hashKey + (uint) (sbyte) fileNames[i];
                        }

                        //find hash and replace name
                        var fileName = Encoding.UTF8.GetString(fileNames, lastNameIndex, index - lastNameIndex);
                        foreach (var entry in files)
                        {
                            if (entry.Value == hash) entry.Key.Name = fileName;
                        }
                    }

                    lastNameIndex = index + 1;
                }

                ++index;
            }
        }

        return files.Keys.ToList();
    }
    #endregion

    #region private methods
    //verifies that the file is a SARC archive
    private static bool CanParse(FileReader reader)
    {
        if (reader.ReadUInt16At(6) == 65534) reader.BigEndian = true;
        return reader.ReadStringAt(0, 4) == "SARC";
    }
    #endregion
}