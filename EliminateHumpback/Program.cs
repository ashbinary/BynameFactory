using System.Collections.Specialized;
using NintendoTools.FileFormats.Byml;
using Compressers = NintendoTools.Compression;
using DataTypes = NintendoTools.FileFormats;

namespace EliminateHumpback {
    class Program {
        public static void Main() {
            
        }

        public static ValueNode[] GetBynameOrder(string dialect, string version, string type, string gender) {
            IList<DataTypes.Sarc.SarcFile> decompFile = Parsers.ParseSarc(Parsers.DecompressZstd($"asset/{version}/Pack/Bootup.Nin_NX_NVN.pack.zs"));
            Console.WriteLine($"Found file at asset/{version}/Pack/Bootup.Nin_NX_NVN.pack.zs");

            foreach (var file in decompFile) {
                if (file.Name == $"Gyml/BynameOrder/{type}_{dialect}{gender}.spl__BynameOrder.bgyml") {
                    BymlFile parsedFile = Parsers.ParseByml(new MemoryStream(file.Content));
                    ArrayNode parsedData = (ArrayNode) parsedFile.RootNode.Find("Labels");

                    return (ValueNode[]) parsedData.ToArray();
                }
            }

            return Array.Empty<ValueNode>();
        }

        public static IList<DataTypes.Msbt.MsbtMessage>? GetBynameData(string dialect, string version, string fileversion, string type)
        {
            var decompFile = Parsers.ParseSarc(Parsers.DecompressZstd($"asset/{version}/Mals/{dialect}.Product.{fileversion}.sarc.zs"));
            Console.WriteLine($"Found file at asset/{version}/Mals/{dialect}.Product.{version.Replace(".", string.Empty)}.sarc.zs");

            var matchingFile = decompFile.FirstOrDefault(file => file.Name == $"CommonMsg/Byname/Byname{type}.msbt");

            return matchingFile != null ? Parsers.ParseMsbt(new MemoryStream(matchingFile.Content)) : null;
        }
    }

    class Parsers {
        public static Stream DecompressZstd(string fileInput) {
            using Stream input = File.OpenRead(fileInput);
                return new Compressers.Zstd.ZstdDecompressor().Decompress(input);
        }

        public static IList<DataTypes.Sarc.SarcFile> ParseSarc(Stream stream) {
            return new DataTypes.Sarc.SarcFileParser().Parse(stream); 
        }

        public static IList<DataTypes.Msbt.MsbtMessage> ParseMsbt(Stream stream) {
            return new DataTypes.Msbt.MsbtFileParser().Parse(stream);
        } 

        public static BymlFile ParseByml(Stream stream) {
            return new BymlFileParser().Parse(stream);
        }
    }
}