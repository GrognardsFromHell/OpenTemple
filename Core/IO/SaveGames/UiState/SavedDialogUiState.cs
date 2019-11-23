using System.Collections.Generic;
using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.UiState
{
    public class SavedDialogUiState
    {
        public List<SavedDialogUiLine> Lines { get; set; } = new List<SavedDialogUiLine>();

        [TempleDllLocation(0x1014cd50)]
        public static SavedDialogUiState Read(BinaryReader reader)
        {
            var lineCount = reader.ReadInt32();

            var result = new SavedDialogUiState();
            result.Lines.Capacity = lineCount;
            for (var i = 0; i < lineCount; i++)
            {
                result.Lines.Add(SavedDialogUiLine.Read(reader));
            }

            return result;
        }
    }

    public class SavedDialogUiLine
    {
        public int Flags { get; set; }

        public string Text { get; set; }

        public bool IsPcLine { get; set; }

        [TempleDllLocation(0x1014c680)]
        public static SavedDialogUiLine Read(BinaryReader reader)
        {
            var result = new SavedDialogUiLine();
            var flags = reader.ReadInt32();
            result.IsPcLine = (flags & 1) != 0;
            result.Flags = flags & ~1;
            result.Text = reader.ReadPrefixedString();
            return result;
        }
    }

}