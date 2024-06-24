using Zstd = NintendoTools.Compression.Zstd;
using Sarc = NintendoTools.FileFormats.Sarc;
using Msbt = NintendoTools.FileFormats.Msbt;
using Byml = NintendoTools.FileFormats.Byml;
using NintendoTools.FileFormats.Byml;

class BynameCheck {

    public static String DIALECT = "JPja";
    public static String VERSION = "500";
    public static String TYPE = "Subject";
    public static String GENDER = "";

    public static void NotMain() {
        Console.OutputEncoding = System.Text.Encoding.Default;
        List<string> FileData = new();

        Console.Write("Choose a dialect.\n>> ");
        DIALECT = Console.ReadLine(); 

        Console.Write("\nChoose a version.\n>> ");
        VERSION = Console.ReadLine(); 

        Console.Write("\nChoose what needs to be found. (Subject/Adjective)\n>> ");
        TYPE = Console.ReadLine(); 

        if (TYPE == "Adjective") {
            Console.Write("\nChoose the gendered term type. (Feminine/Masculine/Common (EUnl only))\n>> ");
            GENDER = Console.ReadLine(); 
        }

        Console.WriteLine("");

        var BynameLang = GetByname("asset/" + DIALECT + ".Product." + VERSION + ".sarc.zs");
        var BynameList = GetBynameList("asset/Bootup.Nin_NX_NVN.pack.zs");
        var RealNode = (ArrayNode) BynameList.RootNode.Find("Labels");

        List<String> BynameOrder = new();
        Dictionary<String, String> BynameData = new();

        for (int i = 0; i < RealNode.Count; i++) {
            BynameOrder.Add(((ValueNode<String>) RealNode[i]).Value);
            // ((NintendoTools.FileFormats.Byml.ArrayNode)(new System.Collections.Generic.IDictionaryDebugView<string, NintendoTools.FileFormats.Byml.Node>(((NintendoTools.FileFormats.Byml.DictionaryNode)BynameList.RootNode)._nodes).Items[0]).Value)._nodes
        }

        for (int i = 0; i < BynameLang.Count; i++) {
            if (BynameLang[i].ToCleanString() != "")
                BynameData.Add(BynameLang[i].Label, BynameLang[i].ToCleanString());
        }

        Console.WriteLine("Obtaining Byname data...");

        for (int i = 0; i < BynameOrder.Count - 1; i++) {
            if (!BynameData.ContainsKey(BynameOrder[i])) {
                int[] offset = {1, 1};

                try {
                    while (!BynameData.ContainsKey(BynameOrder[i + offset[0]])) { offset[0] += 1; }
                } catch (System.ArgumentOutOfRangeException) {
                    offset[0] = 0;
                }

                try {
                    while (!BynameData.ContainsKey(BynameOrder[i - offset[1]])) { offset[1] += 1; }
                } catch (System.ArgumentOutOfRangeException) {
                    offset[1] = 0;
                }

                FileData.Add(BynameOrder[i] + " -> " + 
                    (offset[1] != 0 ? BynameData[BynameOrder[i - offset[1]]] : "START" )+ 
                    " - " + 
                    (offset[0] != 0 ? BynameData[BynameOrder[i + offset[0]]] : "END"));
            }    
        }

        Console.WriteLine("Writing data to file...");

        using (StreamWriter outputFile = new StreamWriter("BynameData." + DIALECT + "." + VERSION + "." + TYPE + ".txt")) {
            foreach (string line in FileData) outputFile.WriteLine(line);
        }
        
        Console.WriteLine("Finished writing to file!\nWrote data to " + "BynameData." + DIALECT + "." + VERSION + "." + TYPE + ".txt.\nPress any key to continue.");
        Console.ReadKey();
    }

    public static IList<Msbt.MsbtMessage> GetByname(String file) {
        Stream decompressed_zstd = null;
        try { decompressed_zstd = Parsers.DecompressZstd(file); }
        catch (FileNotFoundException) { throw new FileNotFoundException(file + " could not be found. Data files should be placed in ../asset/."); }
        var decompressed_sarc = Parsers.ParseSarc(decompressed_zstd);

        for (int i = 0; i < decompressed_sarc.Count; i++) {
            if (decompressed_sarc[i].Name == "CommonMsg/Byname/Byname" + TYPE + ".msbt")
                return Parsers.ParseMsbt(new MemoryStream(decompressed_sarc[i].Content));
        }

        return null;
    }

    public static Byml.BymlFile GetBynameList(String file) {
        Stream decompressed_zstd = null;
        try { decompressed_zstd = Parsers.DecompressZstd(file); }
        catch (FileNotFoundException) { throw new FileNotFoundException(file + " could not be found. Data files should be placed in ../asset/."); }
        var decompressed_sarc = Parsers.ParseSarc(decompressed_zstd);

        for (int i = 0; i < decompressed_sarc.Count; i++) {
            if (decompressed_sarc[i].Name == "Gyml/BynameOrder/" + TYPE + "_" + DIALECT + ".spl__BynameOrder.bgyml")
                return Parsers.ParseByml(new MemoryStream(decompressed_sarc[i].Content));
        }

        return null;
    }
}

class Parsers {
    public static Stream DecompressZstd(String fileInput) {
        using Stream input = File.OpenRead(fileInput);
            return new Zstd.ZstdDecompressor().Decompress(input);
    }

    public static IList<Sarc.SarcFile> ParseSarc(Stream stream) {
        return new Sarc.SarcFileParser().Parse(stream); 
    }

    public static IList<Msbt.MsbtMessage> ParseMsbt(Stream stream) {
        return new Msbt.MsbtFileParser().Parse(stream);
    } 

    public static Byml.BymlFile ParseByml(Stream stream) {
        return new Byml.BymlFileParser().Parse(stream);
    }
}