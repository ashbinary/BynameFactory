using System.Runtime.InteropServices;

namespace BynameFactory {
    class BynameList {
        OrderKind Type;
        Gender? Gender;
        Language Language;
        public List<Byname> Bynames;

        public BynameList(OrderKind TypeT, Gender? GenderT, Language LanguageT, List<Byname> bynames) {
            Type = TypeT;
            Gender = GenderT;
            Language = LanguageT;
            Bynames = bynames;
        }
    }

    class Byname {
        public int Index;
        public int ID;
        public string Data;

        public Byname(int index, int id, string data) {
            Index = index;
            ID = id;
            Data = data;
        }
    }

    enum OrderKind {
        Adjective,
        Subject,
    }

    enum Gender {
        Feminine,
        Masculine,
        Neuter
    }

    enum Language {
        CNzh,
        EUde,
        EUen,
        EUes,
        EUfr,
        EUit,
        EUnl,
        EUru,
        JPja,
        KRko,
        TWzh,
        USen,
        USes,
        USfr,   
    }
}