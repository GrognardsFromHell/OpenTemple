using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Dialog;

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
        // Unused Arcanum leftovers, see 0x10036ee0:
        // "gd_sto_m2m", // 28
        // "gd_sto_m2f", // 29
        // "gd_sto_f2f", // 31
        // "gd_sto_f2m", // 32
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

    public string GetNpcLine(GameObject speaker, GameObject listener, int fromKey, int toKey)
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

    /// <summary>
    /// 1000: Asking for money
    /// 2000: Not enough money
    /// 7000: no rumors available
    /// 8000: "initial great greeting"
    /// 9000: "initial good greeting"
    /// 10000: "initial neutral greeting"
    /// 11000: "further great greeting"
    /// 12000: "further good greeting"
    /// 13000: "further neutral greeting"
    /// 14000: "offer healing"
    /// 16000: greeting pc in underwear
    /// 17000: greeting pc in barbarian armor
    /// 18000: fear of associates
    /// 19000: body spell / daemon / polymorphed pc
    /// 20000: polymorphed (animal?) pc
    /// 21000: mirror imaged pc
    /// 22000: invisible pc
    /// </summary>
    [TempleDllLocation(0x10036b20)]
    public string GetNpcClassBasedLine(GameObject npc, GameObject pc, int baseLine)
    {
        var npcGender = npc.GetGender();
        var pcGender = pc.GetGender();

        int dialogFileIdx;
        if ( GameSystems.Critter.CritterIsLowInt(npc) )
        {
            if ( npcGender == Gender.Male )
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 20 : 21;
            }
            else
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 23 : 22;
            }
        }
        else
        {
            if ( npcGender == Gender.Male )
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 2 : 3;
            }
            else
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 5 : 4;
            }
        }

        return GetRandomLineInRange(dialogFileIdx, baseLine, baseLine + 49);

    }

    /// <summary>
    /// </summary>
    [TempleDllLocation(0x10036c80)]
    public string GetNpcRaceBasedLine(GameObject npc, GameObject pc, int baseLine)
    {
        var npcGender = npc.GetGender();
        var pcGender = pc.GetGender();

        int dialogFileIdx;
        if ( GameSystems.Critter.CritterIsLowInt(npc) )
        {
            if ( npcGender == Gender.Male )
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 24 : 25;
            }
            else
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 27 : 26;
            }
        }
        else
        {
            if ( npcGender == Gender.Male )
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 8 : 9;
            }
            else
            {
                dialogFileIdx = (pcGender == Gender.Male) ? 11 : 10;
            }

            // Only normally intelligent NPCs get different lines for each race
            // TODO: This seems just broken since it uses the NPC's race and is borked
            baseLine += ((npc.GetRace() != RaceId.human) ? 50 : 0);
        }

        return GetRandomLineInRange(dialogFileIdx, baseLine, baseLine + 49);

    }

    // 1000 -> Thanks/appreciation
    // 2000 -> Asking for rumors
    [TempleDllLocation(0x10035240)]
    public string GetPcClassBasedLine(int baseLine, DialogState state)
    {
        int fileIndex;
        if ( GameSystems.Critter.CritterIsLowInt(state.Pc) )
        {
            fileIndex = (state.NPC.GetGender() == Gender.Male) ? 18 : 19;
        }
        else
        {
            fileIndex = (state.NPC.GetGender() == Gender.Male) ? 6 : 7;
        }

        var line = GetRandomLineInRange(fileIndex, baseLine, baseLine + 49);
        return GameSystems.Dialog.ResolveLineTokens(state, line);
    }

    // 0-99 -> Yes
    // 100-199 -> No
    // 200-299 -> Sorry
    // 300-399 -> Barter
    // 400-499 -> Exit
    // 600-600 -> Continue
    // 800-899 -> Forget it
    // 1500-1599 -> Request more info
    // 1700-1799 -> Ask for story state
    [TempleDllLocation(0x10035190)]
    public string GetPcLine(DialogState state, int minLine, int maxLine)
    {
        int fileIndex;
        if ( GameSystems.Critter.CritterIsLowInt(state.Pc) )
        {
            fileIndex = (state.NPC.GetGender() == Gender.Male) ? 16 : 17;
        }
        else
        {
            fileIndex = (state.NPC.GetGender() == Gender.Male) ? 0 : 1;
        }

        var line = GetRandomLineInRange(fileIndex, minLine, maxLine);
        return GameSystems.Dialog.ResolveLineTokens(state, line);
    }

}