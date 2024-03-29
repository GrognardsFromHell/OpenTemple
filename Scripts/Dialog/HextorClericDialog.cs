
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

namespace Scripts.Dialog;

[DialogScript(339)]
public class HextorClericDialog : HextorCleric, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 11:
            case 31:
            case 61:
            case 81:
            case 481:
            case 523:
                originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
            case 3:
            case 12:
            case 22:
            case 32:
            case 62:
            case 82:
            case 482:
                originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
            case 4:
            case 13:
            case 23:
            case 33:
            case 63:
            case 83:
            case 483:
                originalScript = "pc.stat_level_get( stat_deity ) == 9";
                return pc.GetStat(Stat.deity) == 9;
            case 21:
                originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or  game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
            case 41:
                originalScript = "game.quests[84].state == qs_mentioned";
                return GetQuestState(84) == QuestState.Mentioned;
            case 42:
                originalScript = "game.global_vars[960] == 2";
                return GetGlobalVar(960) == 2;
            case 43:
                originalScript = "game.global_vars[960] == 1";
                return GetGlobalVar(960) == 1;
            case 44:
                originalScript = "game.global_flags[844] == 1 and game.quests[84].state == qs_accepted";
                return GetGlobalFlag(844) && GetQuestState(84) == QuestState.Accepted;
            case 45:
                originalScript = "game.quests[78].state == qs_accepted and game.global_vars[929] == 2";
                return GetQuestState(78) == QuestState.Accepted && GetGlobalVar(929) == 2;
            case 131:
            case 151:
            case 161:
            case 171:
                originalScript = "pc.money_get() >= 1000000";
                return pc.GetMoney() >= 1000000;
            case 132:
            case 152:
            case 162:
            case 172:
                originalScript = "pc.money_get() <= 999900 and pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetMoney() <= 999900 && pc.GetGender() == Gender.Male;
            case 133:
            case 153:
            case 163:
            case 173:
                originalScript = "pc.money_get() <= 999900 and pc.stat_level_get( stat_gender ) == gender_female and not npc_get(npc,4)";
                return pc.GetMoney() <= 999900 && pc.GetGender() == Gender.Female && !ScriptDaemon.npc_get(npc, 4);
            case 134:
            case 154:
            case 164:
            case 174:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female and npc_get(npc,4)";
                return pc.GetGender() == Gender.Female && ScriptDaemon.npc_get(npc, 4);
            case 135:
            case 155:
            case 165:
            case 175:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetGender() == Gender.Male;
            case 136:
            case 156:
            case 166:
            case 176:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female and not npc_get(npc,4)";
                return pc.GetGender() == Gender.Female && !ScriptDaemon.npc_get(npc, 4);
            case 137:
            case 157:
            case 167:
            case 177:
                originalScript = "not npc_get(npc,1)";
                return !ScriptDaemon.npc_get(npc, 1);
            case 138:
            case 158:
            case 168:
            case 178:
                originalScript = "not npc_get(npc,2)";
                return !ScriptDaemon.npc_get(npc, 2);
            case 139:
            case 159:
            case 169:
            case 179:
                originalScript = "not npc_get(npc,3)";
                return !ScriptDaemon.npc_get(npc, 3);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
                originalScript = "game.global_flags[938] = 1";
                SetGlobalFlag(938, true);
                break;
            case 60:
                originalScript = "game.quests[84].state = qs_completed";
                SetQuestState(84, QuestState.Completed);
                break;
            case 71:
                originalScript = "pc.money_adj(1000000)";
                pc.AdjustMoney(1000000);
                break;
            case 90:
                originalScript = "game.global_vars[929] = 1";
                SetGlobalVar(929, 1);
                break;
            case 100:
                originalScript = "game.global_vars[929] = 2";
                SetGlobalVar(929, 2);
                break;
            case 137:
            case 157:
            case 167:
            case 177:
                originalScript = "npc_set(npc,1)";
                ScriptDaemon.npc_set(npc, 1);
                break;
            case 138:
            case 158:
            case 168:
            case 178:
                originalScript = "npc_set(npc,2)";
                ScriptDaemon.npc_set(npc, 2);
                break;
            case 139:
            case 159:
            case 169:
            case 179:
                originalScript = "npc_set(npc,3)";
                ScriptDaemon.npc_set(npc, 3);
                break;
            case 191:
                originalScript = "pc.money_adj(-1000000)";
                pc.AdjustMoney(-1000000);
                break;
            case 200:
                originalScript = "game.global_vars[929] = 3";
                SetGlobalVar(929, 3);
                break;
            case 210:
                originalScript = "game.quests[84].state = qs_mentioned";
                SetQuestState(84, QuestState.Mentioned);
                break;
            case 260:
                originalScript = "npc_set(npc,4)";
                ScriptDaemon.npc_set(npc, 4);
                break;
            case 290:
                originalScript = "game.global_vars[928] = 1; game.party[0].reputation_add(51)";
                SetGlobalVar(928, 1);
                PartyLeader.AddReputation(51);
                ;
                break;
            case 291:
                originalScript = "game.fade(3000,4047,0,3)";
                Fade(3000, 4047, 0, 3);
                break;
            case 300:
                originalScript = "game.global_vars[928] = 2";
                SetGlobalVar(928, 2);
                break;
            case 523:
                originalScript = "game.quests[77].state = qs_botched; npc.attack( pc )";
                SetQuestState(77, QuestState.Botched);
                npc.Attack(pc);
                ;
                break;
            case 530:
                originalScript = "game.quests[84].state = qs_accepted; game.global_vars[960] = 1";
                SetQuestState(84, QuestState.Accepted);
                SetGlobalVar(960, 1);
                ;
                break;
            case 531:
                originalScript = "thaddeus_countdown(npc,pc)";
                thaddeus_countdown(npc, pc);
                break;
            case 540:
                originalScript = "game.global_vars[960] = 3";
                SetGlobalVar(960, 3);
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
            default:
                skillChecks = default;
                return false;
        }
    }
}