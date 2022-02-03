
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog;

[DialogScript(123)]
public class HartschDialog : Hartsch, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "game.quests[43].state == qs_mentioned";
                return GetQuestState(43) == QuestState.Mentioned;
            case 4:
            case 5:
                originalScript = "game.quests[43].state >= qs_accepted";
                return GetQuestState(43) >= QuestState.Accepted;
            case 25:
            case 26:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 9";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
            case 205:
            case 206:
                originalScript = "game.quests[43].state == qs_accepted";
                return GetQuestState(43) == QuestState.Accepted;
            case 207:
            case 208:
                originalScript = "game.global_flags[119] == 0 and game.quests[44].state == qs_accepted and ( not anyone( pc.group_list(), \"has_item\", 5807 ) ) and game.global_flags[120] == 0";
                return !GetGlobalFlag(119) && GetQuestState(44) == QuestState.Accepted && (!pc.GetPartyMembers().Any(o => o.HasItemByName(5807))) && !GetGlobalFlag(120);
            case 209:
            case 210:
                originalScript = "game.global_flags[115] == 0 and game.global_flags[116] == 0 and game.global_flags[119] == 1 and game.quests[44].state == qs_accepted";
                return !GetGlobalFlag(115) && !GetGlobalFlag(116) && GetGlobalFlag(119) && GetQuestState(44) == QuestState.Accepted;
            case 211:
            case 212:
                originalScript = "game.quests[45].state == qs_accepted";
                return GetQuestState(45) == QuestState.Accepted;
            case 263:
            case 264:
                originalScript = "pc.skill_level_get(npc, skill_bluff) >= 10";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
            case 283:
            case 284:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 6";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 41:
            case 51:
            case 52:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 250:
                originalScript = "game.map_flags( 5066, 2, 1 )";
                // FIXME: map_flags;
                break;
            case 263:
            case 264:
                originalScript = "game.global_flags[124] = 1";
                SetGlobalFlag(124, true);
                break;
            case 270:
                originalScript = "game.map_flags( 5067, 0, 1 )";
                // FIXME: map_flags;
                break;
            case 280:
                originalScript = "game.map_flags( 5067, 1, 1 )";
                // FIXME: map_flags;
                break;
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            case 25:
            case 26:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                return true;
            case 263:
            case 264:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                return true;
            case 283:
            case 284:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}