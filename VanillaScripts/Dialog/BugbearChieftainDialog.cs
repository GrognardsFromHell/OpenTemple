
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

[DialogScript(157)]
public class BugbearChieftainDialog : BugbearChieftain, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 4:
            case 5:
                originalScript = "game.story_state >= 5";
                return StoryState >= 5;
            case 6:
            case 7:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 8";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
            case 10:
            case 11:
            case 21:
            case 23:
            case 61:
            case 62:
            case 121:
            case 122:
            case 131:
            case 132:
                originalScript = "game.quests[49].state == qs_accepted";
                return GetQuestState(49) == QuestState.Accepted;
            case 22:
            case 24:
                originalScript = "game.quests[46].state >= qs_accepted";
                return GetQuestState(46) >= QuestState.Accepted;
            case 45:
            case 46:
                originalScript = "pc.money_get() >= 50000 and pc.skill_level_get(npc, skill_diplomacy) >= 11";
                return pc.GetMoney() >= 50000 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
            case 71:
            case 72:
            case 163:
            case 164:
            case 221:
            case 222:
            case 241:
            case 242:
                originalScript = "pc.money_get() >= 50000";
                return pc.GetMoney() >= 50000;
            case 73:
            case 74:
            case 223:
            case 224:
                originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 10";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
            case 101:
            case 102:
                originalScript = "game.quests[49].state == qs_accepted and game.global_flags[172] == 0 and game.global_flags[108] == 0";
                return GetQuestState(49) == QuestState.Accepted && !GetGlobalFlag(172) && !GetGlobalFlag(108);
            case 103:
            case 104:
                originalScript = "game.global_flags[108] == 1";
                return GetGlobalFlag(108);
            case 105:
            case 106:
                originalScript = "game.global_flags[108] == 0 and ( game.quests[46].state >= qs_accepted or anyone( pc.group_list(), \"has_wielded\", 3016 ) )";
                return !GetGlobalFlag(108) && (GetQuestState(46) >= QuestState.Accepted || pc.GetPartyMembers().Any(o => o.HasEquippedByName(3016)));
            case 107:
            case 108:
                originalScript = "( game.global_flags[108] == 0 ) and ( not anyone( pc.group_list(), \"has_wielded\", 3016 ) )";
                return (!GetGlobalFlag(108)) && (!pc.GetPartyMembers().Any(o => o.HasEquippedByName(3016)));
            case 109:
            case 110:
                originalScript = "game.quests[49].state == qs_accepted and game.global_flags[172] == 1";
                return GetQuestState(49) == QuestState.Accepted && GetGlobalFlag(172);
            case 165:
            case 166:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 11";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
            case 51:
            case 63:
            case 64:
            case 91:
            case 92:
            case 125:
            case 126:
            case 133:
            case 134:
            case 143:
            case 144:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 60:
            case 130:
            case 180:
            case 190:
                originalScript = "game.global_flags[108] = 1";
                SetGlobalFlag(108, true);
                break;
            case 70:
                originalScript = "game.global_flags[172] = 1";
                SetGlobalFlag(172, true);
                break;
            case 171:
            case 172:
            case 231:
            case 232:
                originalScript = "pc.money_adj(-50000)";
                pc.AdjustMoney(-50000);
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
            case 6:
            case 7:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                return true;
            case 45:
            case 46:
            case 165:
            case 166:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                return true;
            case 73:
            case 74:
            case 223:
            case 224:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}