namespace BynameFactory {
    class Program {
        public static void Main() {
            Console.OutputEncoding = System.Text.Encoding.Default;

            DirectoryInfo AssetData = new DirectoryInfo("Asset/Data");
            string[] DirectoryVersions = AssetData.GetDirectories().Select(dir => dir.Name).ToArray();

            // Below here is where you can customize the output.
            PrintBynameText("7.1.0", OrderKind.Subject, Language.JPja, null);
        }

        public static void PrintBynameText(string GameVersion, OrderKind BynameType, Language GameLanguage, Gender? UserGender) {
            BynameList BynameData = HandleBynames(GameVersion, BynameType, GameLanguage, UserGender);
            Directory.CreateDirectory($"Asset/Export/{BynameType}/{GameVersion}");
            
            if (BynameData == null) {
                Console.WriteLine($"Failed to find file. [{GameLanguage} - {GameVersion}{(UserGender != null ? ", " + UserGender.ToString() : "")}]");
                return;
            }

            Console.WriteLine($"Found file! [{GameLanguage} - {GameVersion}{(UserGender != null ? ", " + UserGender.ToString() : "")}]");

            using StreamWriter bynameInfo = new($"Asset/Export/{BynameType}/{GameVersion}/BynameData_{GameLanguage}{(UserGender != null ? "_" + UserGender.ToString() : "")}.txt");
            foreach (Byname Tag in BynameData.Bynames) {
                if (Tag.Data == "NO BYNAME") {
                    int currentIndex = Tag.Index;
                    string previousData = "";
                    string nextData = "";

                    // Check the previous indices until a non-"NO BYNAME" data is found
                    for (int i = currentIndex - 1; i >= 0; i--) {
                        if (BynameData.Bynames[i].Data != "NO BYNAME") {
                            previousData = BynameData.Bynames[i].Data;
                            break;
                        }
                    }

                    // Check the next indices until a non-"NO BYNAME" data is found
                    for (int i = currentIndex + 1; i < BynameData.Bynames.Count; i++) {
                        if (BynameData.Bynames[i].Data != "NO BYNAME") {
                            nextData = BynameData.Bynames[i].Data;
                            break;
                        }
                    }
                    bynameInfo.WriteLine($"{Tag.ID} -> {previousData} - {nextData}");
                }
            }   
        }

        public static void PrintBynameExcel(string GameVersion, OrderKind BynameType, Language GameLanguage, Gender? UserGender) {

        }

        public static BynameList HandleBynames(string GameVersion, OrderKind BynameType, Language GameLanguage, Gender? UserGender) {
            string TextFilePath = Directory.GetFiles($"Asset/Data/{GameVersion}/Mals", $"{GameLanguage}*", SearchOption.TopDirectoryOnly)[0];
            string BootupFilePath = $"Asset/Data/{GameVersion}/Pack/Bootup.Nin_NX_NVN.pack.zs";

            BynameList BynameData = BynameParser.parseBynames(TextFilePath, BootupFilePath, 
                new BynameInput(BynameType, UserGender, GameLanguage, GameVersion.Replace(".", ""))
            );

            return BynameData;
        }
    }
}