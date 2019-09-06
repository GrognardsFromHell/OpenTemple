using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Dialog
{
    /// <summary>
    /// ToEE previously loaded these files lazily, but we'd rather do so eagerly (and in parallel).
    /// </summary>
    public class GeneratedDialog
    {
        private readonly Dictionary<int, string>[] _generateDialogContent;

        private readonly int[][] _generatedDialogLineKeys;

        [TempleDllLocation(0x108ec850)]
        private readonly string[] _generateDialogFilenames =
        {
            "gd_pc2m", // 0
            "gd_pc2f", // 1
            "gd_cls_m2m", // 2
            "gd_cls_m2f", // 3
            "gd_cls_f2f", // 4
            "gd_cls_f2m", // 5
            "gd_cls_pc2m", // 6
            "gd_cls_pc2f", // 7
            "gd_rce_m2m", // 8
            "gd_rce_m2f", // 9
            "gd_rce_f2f", // 10
            "gd_rce_f2m", // 11
            "gd_npc_m2m", // 12
            "gd_npc_m2f", // 13
            "gd_npc_f2f", // 14
            "gd_npc_f2m", // 15
            "gd_dumb_pc2m", // 16
            "gd_dumb_pc2f", // 17
            "gd_cls_dumb_pc2m", // 18
            "gd_cls_dumb_pc2f", // 19
            "gd_cls_dumb_m2m", // 20
            "gd_cls_dumb_m2f", // 21
            "gd_cls_dumb_f2f", // 22
            "gd_cls_dumb_f2m", // 23
            "gd_rce_dumb_m2m", // 24
            "gd_rce_dumb_m2f", // 25
            "gd_rce_dumb_f2f", // 26
            "gd_rce_dumb_f2m", // 27
            "gd_sto_m2m", // 28
            "gd_sto_m2f", // 29
            "gd_sto_f2f", // 31
            "gd_sto_f2m", // 32
        };

        public GeneratedDialog()
        {
            _generateDialogContent = _generateDialogFilenames.AsParallel()
                .Select(filename => Tig.FS.ReadMesFile($"mes/{filename}.mes"))
                .ToArray();

            // Pre-generate a sorted list of keys for each of the generated files, because
            // we'll need to randomly select a key within a given range, and this makes that faster
            _generatedDialogLineKeys = _generateDialogContent.Select(d => d.Keys.ToArray()).ToArray();
            foreach (var keyArray in _generatedDialogLineKeys)
            {
                Array.Sort(keyArray);
            }
        }

        public string GetNpcLine(GameObjectBody speaker, GameObjectBody listener, int fromKey, int toKey)
        {
            var speakerGender = (Gender) speaker.GetStat(Stat.gender);
            var listenerGender = (Gender) listener.GetStat(Stat.gender);

            int generatedFileIdx;
            if (speakerGender == Gender.Male)
            {
                generatedFileIdx = listenerGender == Gender.Male ? 12 : 13;
            }
            else
            {
                generatedFileIdx = listenerGender == Gender.Female ? 14 : 15;
            }

            return GetRandomLineInRange(generatedFileIdx, fromKey, toKey);
        }

        private string GetRandomLineInRange(int generatedFileIdx, int fromKey, int toKey)
        {
            var lines = _generateDialogContent[generatedFileIdx];
            var keys = _generatedDialogLineKeys[generatedFileIdx];
            if (SelectRandomKeyFromRange(keys, fromKey, toKey, out var key))
            {
                return lines[key];
            }

            return null;
        }

        private bool SelectRandomKeyFromRange(int[] keys, int from, int to, out int key)
        {
            // Count lines in range
            int foundKeys = 0;
            int firstKeyIdx = -1;
            for (var index = 0; index < keys.Length; index++)
            {
                if (keys[index] >= from && keys[index] <= to)
                {
                    foundKeys++;
                    if (firstKeyIdx == -1)
                    {
                        firstKeyIdx = index;
                    }
                }
            }

            if (foundKeys == 0)
            {
                key = -1;
                return false;
            }

            // This only works because keys are in order
            var pickedIdx = GameSystems.Random.GetInt(firstKeyIdx, firstKeyIdx + foundKeys - 1);
            key = keys[pickedIdx];
            return true;
        }
    }
}