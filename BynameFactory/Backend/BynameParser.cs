using System.Data.SqlTypes;
using BymlView;
using OatmealDome.NinLib.MessageStudio;
using Fushigi.SARC;
using ZstdSharp;

namespace BynameFactory {
    class BynameParser {
        public static BynameList parseBynames(String MalsPath, String BootupPath, BynameInput BynameData) {

            // Setup Mals file to be parsed.
            byte[] MalsFile = File.ReadAllBytes(MalsPath);
            
            using var Decompress = new Decompressor();
            var MalsZSTD = Decompress.Unwrap(MalsFile);
            SARC MalsSARC = new(new MemoryStream(MalsZSTD.ToArray()));
            var MSBTData = MalsSARC.OpenFile($"CommonMsg/Byname/Byname{Enum.GetName(BynameData.Type)}.msbt");

            Msbt MSBTParsed = new(new MemoryStream(MSBTData));
            var KeyData = MSBTParsed.Keys.ToArray();

            // Setup Bootup Pack file to be parsed.
            byte[] BootupPack = File.ReadAllBytes(BootupPath);

            var BootupZSTD = Decompress.Unwrap(BootupPack);
            SARC BootupSARC = new(new MemoryStream(BootupZSTD.ToArray()));

            var BynameBootup = BootupSARC.OpenFile($"Gyml/BynameOrder/{Enum.GetName(BynameData.Type)}_{Enum.GetName(BynameData.Language)}{(BynameData.Gender != null ? "_" + BynameData.Gender.ToString() : "")}.spl__BynameOrder.bgyml");
            if (BynameBootup == null) return null;

            Byml BynameByml = new(new MemoryStream(BynameBootup));
            var BynameRoot = (BynameByml.Root as BymlHashTable).Pairs[0].Value;

            var BynameArray = (BynameRoot as BymlArrayNode).getArray();

            // Add data to BynameList
            List<Byname> BynamesOrg = []; // Future-proofing - Org stands for Organized
            int Indexer = 1;

            BynamesOrg.Add(new Byname(0, -2147483647, "START"));

            foreach (BymlNode<String> BynameNode in BynameArray) {
                BynamesOrg.Add(new Byname(
                    Indexer,
                    int.Parse(BynameNode.Data.Contains("_") ? BynameNode.Data[..4] : BynameNode.Data),
                    MSBTParsed.ContainsKey(BynameNode.Data) ? MSBTParsed.GetWithoutTags(BynameNode.Data) : "NO BYNAME"
                ));
                Indexer++;
            }

            BynamesOrg.Add(new Byname(0, 2147483647, "END"));

            BynameList BynameAssets = new BynameList(
                BynameData.Type,
                BynameData.Gender,
                BynameData.Language,
                BynamesOrg
            );

            //foreach (String IndexTag in BynameRoot) {
            //    Console.WriteLine(IndexTag);
            //}
            return BynameAssets;
        }
    }

    class BynameInput {
        public OrderKind Type;
        public Gender? Gender;
        public Language Language;
        public String GameVersion = "100";

        public BynameInput(OrderKind type, Gender? gender, Language language, String gameVersion) {
            Type = type;
            Gender = gender;
            Language = language;
            GameVersion = gameVersion;
        }
    }
}