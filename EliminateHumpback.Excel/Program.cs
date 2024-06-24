using System.Collections.Specialized;
using NintendoTools.FileFormats.Byml;
using Compressers = NintendoTools.Compression;
using DataTypes = NintendoTools.FileFormats;
using OfficeOpenXml;
using System;
using System.Drawing.Printing;
using System.Reflection;
using NintendoTools.FileFormats.Msbt;
using OfficeOpenXml.FormulaParsing.Ranges;
using System.Linq.Expressions;

namespace EliminateHumpback {
    class Program {
        public static void Main() {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            String[] dialects = {"CNzh", "EUde", "EUen", "EUes", "EUfr", "EUit", "EUnl", "EUru", "JPja", "KRko", "TWzh", "USen", "USes", "USfr"};
            String[] versions = {"1.0.0", "1.1.0", "1.1.1", "1.2.1", "2.0.0", "2.0.1", "2.1.0", "2.1.1", "3.0.0", "3.0.1", "3.1.0", "3.1.1", "4.0.1", "4.0.2", "4.1.0", "5.0.0", "5.0.1", "5.1.0", "5.2.0"};
            String[] fileversions = {"100", "110", "110", "120", "200", "200", "200", "200", "300", "300", "310", "310", "400", "400", "410", "500", "500", "510", "520"};            

            String sheetType = "Adjective";
            String genderType = "";

            ExcelPackage excelPack = new();

            foreach (string language in dialects) {
                // Create the spreadsheet.
                var spreadsheet = excelPack.Workbook.Worksheets.Add(language);

                // Get the keys from the latest version and order them.
                List<string> bynameKeys = GetBynameOrder(language, versions[^1], sheetType, genderType);
                bynameKeys.Sort();

                // Set the keys in the first column of the spreadsheet.
                foreach (string key in bynameKeys.Cast<string>()) {
                    spreadsheet.Cells["A" + (bynameKeys.IndexOf(key) + 2)].Value = key;
                }

                foreach (string version in versions) {
                    // Create the version labels at the top of the screen.
                    spreadsheet.Cells[alpha[Array.IndexOf(versions, version) + 1] + "1"].Value = version;

                    // Get the byname data and order from version/dialect.
                    List<string> bynameOrder = GetBynameOrder(language, version, sheetType, genderType);
                    Dictionary<string, string> bynameData = GetBynameData(language, version, fileversions[Array.IndexOf(versions, version)], sheetType);

                    // Replace the empty bynames with values of their keys.
                    FillNullBynames(bynameOrder, bynameData);

                    // Insert the byname data into the spreadsheet.
                    for (int i = 0; i < bynameOrder.Count; i++) {
                        int index = bynameKeys.IndexOf(bynameOrder[i]);

                        using var cell = spreadsheet.Cells[alpha[Array.IndexOf(versions, version) + 1] + (index + 2).ToString()]; 
                            cell.Value = bynameData[bynameOrder[i]];
                            if (bynameData[bynameOrder[i]].Contains(" - ")) {
                                cell.Style.Font.Bold = true;
                            }
                    }

                }
            }

            // Save the spreadsheet.
            File.WriteAllBytes("BynameData.xlsx", excelPack.GetAsByteArray());
        }

        public static void FillNullBynames(List<string> bynameOrder, Dictionary<string, string> bynameData) {
            var originalBynameData = bynameData;
            foreach (string byname in bynameOrder) {
                if (!originalBynameData.ContainsKey(byname)) {
                    int bynameIndex = bynameOrder.IndexOf(byname);

                    int[] bynameOffset = {1, 1};

                    try {
                        while(!originalBynameData.ContainsKey(bynameOrder[bynameIndex - bynameOffset[0]])) {
                            bynameOffset[0] += 1;
                        } 
                    } catch (ArgumentOutOfRangeException) {
                        bynameOffset[0] = 0;
                    }

                    try {
                        while(!originalBynameData.ContainsKey(bynameOrder[bynameIndex + bynameOffset[1]])) {
                            bynameOffset[1] += 1;
                        } 
                    } catch (ArgumentOutOfRangeException) {
                        bynameOffset[1] = 0;
                    }

                    Console.WriteLine(byname);

                    var stringOffset0 = bynameOffset[0] != 0 ? originalBynameData[bynameOrder[bynameIndex - bynameOffset[0]]] : "START";
                    var stringOffset1 = bynameOffset[1] != 0 ? originalBynameData[bynameOrder[bynameIndex + bynameOffset[1]]] : "END";

                    if (stringOffset0.Split(" - ").Length > 1) stringOffset0 = String.Join(stringOffset0.Split(" - ")[0], stringOffset0.Split(" - ")[1]);
                    if (stringOffset1.Split(" - ").Length > 1) stringOffset1 = String.Join(stringOffset1.Split(" - ")[0], stringOffset1.Split(" - ")[1]);

                    Console.WriteLine($"{stringOffset0} - {stringOffset1}");

                    bynameData.Add(byname, $"{stringOffset0} - {stringOffset1}");
                }
            }
        }

        public static List<string> GetBynameOrder(string dialect, string version, string type, string gender) {
            IList<DataTypes.Sarc.SarcFile> decompFile = Parsers.ParseSarc(Parsers.DecompressZstd($"asset/{version}/Pack/Bootup.Nin_NX_NVN.pack.zs"));
            Console.WriteLine($"Found file at asset/{version}/Pack/Bootup.Nin_NX_NVN.pack.zs");

            foreach (var file in decompFile) {
                if (file.Name == $"Gyml/BynameOrder/{type}_{dialect}{gender}.spl__BynameOrder.bgyml") {
                    BymlFile parsedFile = Parsers.ParseByml(new MemoryStream(file.Content));
                    ArrayNode parsedData = (ArrayNode) parsedFile.RootNode.Find("Labels");

                    List<string> listData = new();

                    foreach (ValueNode node in parsedData.Cast<ValueNode>()) {
                        listData.Add((string) node.GetValue());
                    }

                    return listData;
                }
            }

            return new List<string>();
        }

        public static Dictionary<string, string> GetBynameData(string dialect, string version, string fileversion, string type)
        {
            var decompFile = Parsers.ParseSarc(Parsers.DecompressZstd($"asset/{version}/Mals/{dialect}.Product.{fileversion}.sarc.zs"));
            Console.WriteLine($"Found file at asset/{version}/Mals/{dialect}.Product.{version.Replace(".", string.Empty)}.sarc.zs");

            var matchingFile = decompFile.FirstOrDefault(file => file.Name == $"CommonMsg/Byname/Byname{type}.msbt");
            var parsedMsbt = Parsers.ParseMsbt(new MemoryStream(matchingFile.Content));

            Dictionary<string, string> msbtPairs = new();
            foreach (MsbtMessage message in parsedMsbt)
                msbtPairs.Add(message.Label, message.ToCleanString());

            return msbtPairs;
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