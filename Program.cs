using System.Collections.Specialized;
using System.Runtime.InteropServices;
using Compressers = NintendoTools.Compression;
using DataTypes = NintendoTools.FileFormats;

namespace EliminateHumpback {
    class Program {
        
        public static void Main() {
            Console.OutputEncoding = System.Text.Encoding.Default;

            String[] dialects = {"CNzh", "EUde", "EUen", "EUes", "EUfr", "EUit", "EUnl", "EUru", "JPja", "KRko", "TWzh", "USen", "USes", "USfr"};
            String[] versions = {"1.0.0", "1.1.0", "1.1.1", "1.2.1", "2.0.0", "2.0.1", "2.1.0", "2.1.1", "3.0.0", "3.0.1", "3.1.0", "3.1.1", "4.0.1", "4.0.2", "4.1.0", "5.0.0", "5.0.1", "5.1.0", "5.2.0"};
            String[] fileversions = {"100", "110", "110", "120", "200", "200", "200", "200", "300", "300", "310", "310", "400", "400", "410", "500", "500", "510", "520"};

            String[] dialectsMin = {"JPja", "USen"};

                    foreach (string dial in dialects)
                        ObtainBynames(dial, "7.2.0", "720", "Subject", "");
        }

        public static void ObtainBynames(string dialect, string version, string fileversion, string type, string gender) {
            OrderedDictionary Byname = GetBynames(dialect, version, fileversion, type, gender);

            if (Byname.Count < 1) {
                Console.WriteLine("No file found!");
            } else {
                List<int> NoneValues = new();

                for (int index = 0; index < Byname.Count; index++)
                    if (Byname[index] == null) NoneValues.Add(index);
                
                String[] BynameKeys = new String[Byname.Count];
                Byname.Keys.CopyTo(BynameKeys, 0);

                Directory.CreateDirectory($"file/{type}/{version}");
                using (StreamWriter outputFile = new StreamWriter($"file/{type}/{version}/BynameData.{dialect}.{"7.2.0".Replace(".", string.Empty)}.{type}{gender}.txt")) {
                    foreach (int value in NoneValues) {
                        int[] offsetValue = {1, 1};

                        try {
                            while (Byname[value + offsetValue[0]] == null) { offsetValue[0] += 1; }
                        } catch (System.ArgumentOutOfRangeException) {
                            offsetValue[0] = 0;
                        }

                        try {
                            while (Byname[value - offsetValue[1]] == null) { offsetValue[1] += 1; }
                        } catch (System.ArgumentOutOfRangeException) {
                            offsetValue[1] = 0;
                        }

                        Console.WriteLine(Byname[value - offsetValue[1]]);

                        outputFile.WriteLine(
                            BynameKeys[value] + " -> " +
                            (offsetValue[1] != 0 ? Byname[value - offsetValue[1]] : "START") + " - " +
                            (offsetValue[0] != 0 ? Byname[value + offsetValue[0]] : "END")
                        );
                    }
                }
            }

        }

        public static OrderedDictionary GetBynames(string dialect, string version, string fileversion, string type, string gender) {
            DataTypes.Byml.BymlFile BynameOrder = GetBynameOrder(dialect, version, type, gender);
            IList<DataTypes.Msbt.MsbtMessage> BynameData = GetBynameData(dialect, version, fileversion, type);

            DataTypes.Byml.ArrayNode BynameOrderList;

            try {
                BynameOrderList = (DataTypes.Byml.ArrayNode) BynameOrder.RootNode.Find("Labels");
            } catch (System.NullReferenceException) { return new OrderedDictionary(); }

            OrderedDictionary dictionary = new();
            for (int order = 0; order < BynameOrderList.Count; order++)
                dictionary.Add(((DataTypes.Byml.ValueNode<String>) BynameOrderList[order]).Value, null);

            for (int data = 0; data < BynameData.Count; data++)
                dictionary[BynameData[data].Label] = BynameData[data].ToCleanString();

            return dictionary;
        }


        public static DataTypes.Byml.BymlFile GetBynameOrder(string dialect, string version, string type, string gender) {
            IList<DataTypes.Sarc.SarcFile> decompFile = Parsers.ParseSarc(Parsers.DecompressZstd($"asset/{version}/Pack/Bootup.Nin_NX_NVN.pack.zs"));
            Console.WriteLine($"Found file at asset/{version}/Pack/Bootup.Nin_NX_NVN.pack.zs");

            for (int i = 0; i < decompFile.Count; i++) {
                if (decompFile[i].Name == $"Gyml/BynameOrder/{type}_{dialect}{gender}.spl__BynameOrder.bgyml")
                    return Parsers.ParseByml(new MemoryStream(decompFile[i].Content));
            }

            DataTypes.Byml.BymlFile failedCheck = new();
            failedCheck.Version = 1992; // lol
            return failedCheck;
        }

        // CURSE OF NULL DEREFERENECE 𓀀 𓀁 𓀂 𓀃 𓀄 𓀅 𓀆 𓀇 𓀈 𓀉 𓀊 𓀋 𓀌 𓀍 𓀎 𓀏 𓀐 𓀑 𓀒 𓀓 𓀔 𓀕 𓀖 𓀗 𓀘 𓀙 𓀚 𓀛 𓀜 𓀝 𓀞 𓀟 𓀠 𓀡 𓀢 𓀣 𓀤 
        public static IList<DataTypes.Msbt.MsbtMessage> GetBynameData(string dialect, string version, string fileversion, string type) {
            IList<DataTypes.Sarc.SarcFile> decompFile = Parsers.ParseSarc(Parsers.DecompressZstd($"asset/{version}/Mals/{dialect}.Product.{fileversion}.sarc.zs"));
            Console.WriteLine($"Found file at asset/{version}/Mals/{dialect}.Product.{version.Replace(".", string.Empty)}.sarc.zs");

            for (int i = 0; i < decompFile.Count; i++) {
                if (decompFile[i].Name == $"CommonMsg/Byname/Byname{type}.msbt")
                    return Parsers.ParseMsbt(new MemoryStream(decompFile[i].Content));
            }

            return null;
        }
    }

    class Parsers {
        public static Stream DecompressZstd(String fileInput) {
            using Stream input = File.OpenRead(fileInput);
                return new Compressers.Zstd.ZstdDecompressor().Decompress(input);
        }

        public static IList<DataTypes.Sarc.SarcFile> ParseSarc(Stream stream) {
            return new DataTypes.Sarc.SarcFileParser().Parse(stream); 
        }

        public static IList<DataTypes.Msbt.MsbtMessage> ParseMsbt(Stream stream) {
            return new DataTypes.Msbt.MsbtFileParser().Parse(stream);
        } 

        public static DataTypes.Byml.BymlFile ParseByml(Stream stream) {
            return new DataTypes.Byml.BymlFileParser().Parse(stream);
        }
    }
}