namespace BymlView.Writer
{
    public interface IBymlData
    {
        void MakeIndex();
        int CalcPackSize();
        BymlNodeId GetTypeCode();
        bool IsContainer();
        void Write(Stream stream);
    }
}
