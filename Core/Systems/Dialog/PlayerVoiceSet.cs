using System;
using System.Collections.Generic;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Dialog
{
    public class PlayerVoiceSet
    {
        public int Key { get; }

        public string Name { get; }

        public string RuleFilename { get; }

        public bool Male { get; }

        // Lines that do not have a context
        private readonly Dictionary<PlayerVoiceLine, List<string>> _lines =
            new Dictionary<PlayerVoiceLine, List<string>>();

        // Lines depending on area
        // Keys 1200-1299
        private readonly Dictionary<int, string> _areaFirstVisited = new Dictionary<int, string>();

        // Lines depending on tagged scenery
        // Keys 1400-1499
        private readonly Dictionary<int, string> _taggedScenerySeen = new Dictionary<int, string>();

        // Lines depending on deity
        // Keys 3000-3099
        private readonly Dictionary<DeityId, string> _divinePowerUsed = new Dictionary<DeityId, string>();

        public PlayerVoiceSet(int key, string name, string ruleFilename)
        {
            Key = key;
            Name = name;
            RuleFilename = ruleFilename;
            Male = ruleFilename.StartsWith("M", StringComparison.InvariantCultureIgnoreCase);

            LoadRules("mes/pcvoice/" + ruleFilename);
        }

        [TempleDllLocation(0x10034b50)]
        private void LoadRules(string path)
        {
            var mesFile = Tig.FS.ReadMesFile(path);

            foreach (var line in (PlayerVoiceLine[]) Enum.GetValues(typeof(PlayerVoiceLine)))
            {
                _lines[line] = LoadLineText(mesFile, line);
            }

            // Load area specific lines
            for (var i = 0; i < 99; i++)
            {
                if (mesFile.TryGetValue(1200 + i, out var text))
                {
                    _areaFirstVisited[i] = text;
                }
            }

            // Load tagged scenery specific lines
            for (var i = 0; i < 99; i++)
            {
                if (mesFile.TryGetValue(1400 + i, out var text))
                {
                    _taggedScenerySeen[i] = text;
                }
            }

            // Load deity specific lines
            for (var i = 0; i < 99; i++)
            {
                if (mesFile.TryGetValue(3000 + i, out var text))
                {
                    _divinePowerUsed[(DeityId) i] = text;
                }
            }
        }

        private List<string> LoadLineText(Dictionary<int, string> mesLines, PlayerVoiceLine line)
        {
            var result = new List<string>();
            var baseKey = GetLineGroupBaseKey(line);
            for (var i = 0; i < 30; i++)
            {
                var key = baseKey + i;
                if (mesLines.TryGetValue(key, out var text))
                {
                    result.Add(text);
                }
            }

            return result;
        }

        public void PickLine(PlayerVoiceLine line, out string text, out int soundId)
        {
            var lines = _lines[line];

            if (lines.Count == 0)
            {
                text = null;
                soundId = -1;
                return;
            }

            var chosenIndex = GameSystems.Random.GetInt(0, lines.Count - 1);
            text = lines[chosenIndex];
            soundId = GetLineGroupBaseKey(line) + chosenIndex;
        }

        public void PickAreaEnteredLine(int areaId, out string text)
        {
            _areaFirstVisited.TryGetValue(areaId, out text);
        }

        public void PickDivinePowerUsedLine(DeityId deity, out string text)
        {
            _divinePowerUsed.TryGetValue(deity, out text);
        }

        private static int GetLineGroupBaseKey(PlayerVoiceLine line)
        {
            return (int) line * 10;
        }
    }
}