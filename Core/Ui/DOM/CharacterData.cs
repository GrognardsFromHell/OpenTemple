namespace OpenTemple.Core.Ui.DOM
{
    public interface CharacterData : INonDocumentTypeChildNode, IChildNode
    {
        string Data { get; set; }

        public int Length => Data.Length;

        string SubstringData(int offset, int count);
        void AppendData(string data);
        void InsertData(int offset, string data);
        void DeleteData(int offset, int count);
        void ReplaceData(int offset, int count, string data);
    }
}