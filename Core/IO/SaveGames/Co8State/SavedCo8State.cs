using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpicyTemple.Core.IO.SaveGames.Co8State
{
    public class SavedCo8State
    {
        public Dictionary<string, string> PersistentData { get; } = new Dictionary<string, string>();

        public Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();

        public Dictionary<string, int> Vars { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, string> StringVars { get; set; } = new Dictionary<string, string>();

        public static SavedCo8State Load(string co8Path)
        {
            var result = new SavedCo8State();

            using var reader = new StreamReader(co8Path, Encoding.Default);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int splitChar = line.IndexOf('|');
                if (splitChar == -1)
                {
                    throw new CorruptSaveException($"Invalid line '{line}' in Co8 save file {co8Path}");
                }

                var keySpan = line.AsSpan(0, splitChar);
                var value = line.Substring(splitChar + 1);

                if (keySpan.StartsWith("Flaggg"))
                {
                    result.Flags[keySpan.Slice(6).ToString()] = int.Parse(value) != 0;
                }
                else if (keySpan.StartsWith("Varrr"))
                {
                    result.Vars[keySpan.Slice(5).ToString()] = int.Parse(value);
                }
                else if (keySpan.StartsWith("Stringgg"))
                {
                    result.StringVars[keySpan.Slice(8).ToString()] = value;
                }
                else
                {
                    result.PersistentData[new string(keySpan)] = value;
                }
            }

            return result;
        }
    }
}