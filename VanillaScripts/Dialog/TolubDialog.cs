
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
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

namespace VanillaScripts.Dialog
{
    [DialogScript(116)]
    public class TolubDialog : Tolub, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 202:
                case 206:
                case 210:
                case 214:
                    originalScript = "game.global_vars[10] == 1";
                    return GetGlobalVar(10) == 1;
                case 6:
                case 7:
                    originalScript = "npc.has_met(pc) and game.quests[36].state == qs_accepted";
                    return npc.HasMet(pc) && GetQuestState(36) == QuestState.Accepted;
                case 8:
                    originalScript = "npc.has_met(pc) and game.global_flags[290] == 0";
                    return npc.HasMet(pc) && !GetGlobalFlag(290);
                case 11:
                case 12:
                    originalScript = "game.global_flags[290] == 0";
                    return !GetGlobalFlag(290);
                case 13:
                case 14:
                case 21:
                case 22:
                case 32:
                case 33:
                case 221:
                case 222:
                case 231:
                case 232:
                case 241:
                case 242:
                case 265:
                case 266:
                    originalScript = "game.quests[36].state == qs_accepted";
                    return GetQuestState(36) == QuestState.Accepted;
                case 41:
                case 45:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 9";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 9;
                case 71:
                case 72:
                    originalScript = "pc.money_get() >= 100000";
                    return pc.GetMoney() >= 100000;
                case 73:
                case 74:
                    originalScript = "pc.money_get() < 100000";
                    return pc.GetMoney() < 100000;
                case 141:
                case 142:
                    originalScript = "game.global_vars[10] <= 1";
                    return GetGlobalVar(10) <= 1;
                case 201:
                case 205:
                case 209:
                case 213:
                case 351:
                case 352:
                    originalScript = "game.global_vars[10] == 0";
                    return GetGlobalVar(10) == 0;
                case 203:
                case 207:
                case 211:
                case 215:
                    originalScript = "game.global_vars[10] == 2";
                    return GetGlobalVar(10) == 2;
                case 204:
                case 208:
                case 212:
                case 216:
                    originalScript = "game.global_vars[10] == 3";
                    return GetGlobalVar(10) == 3;
                case 301:
                    originalScript = "game.global_flags[101] == 0";
                    return !GetGlobalFlag(101);
                case 302:
                case 303:
                    originalScript = "game.global_flags[101] == 1";
                    return GetGlobalFlag(101);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                case 72:
                    originalScript = "pc.money_adj(-100000)";
                    pc.AdjustMoney(-100000);
                    break;
                case 80:
                case 130:
                case 321:
                    originalScript = "game.quests[36].state = qs_completed";
                    SetQuestState(36, QuestState.Completed);
                    break;
                case 150:
                case 200:
                    originalScript = "game.quests[40].state = qs_mentioned";
                    SetQuestState(40, QuestState.Mentioned);
                    break;
                case 161:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 240:
                    originalScript = "game.global_vars[10] = 1";
                    SetGlobalVar(10, 1);
                    break;
                case 261:
                case 262:
                case 281:
                case 282:
                    originalScript = "game.quests[40].state = qs_accepted; brawl(npc,pc)";
                    SetQuestState(40, QuestState.Accepted);
                    brawl(npc, pc);
                    ;
                    break;
                case 280:
                    originalScript = "game.global_flags[101] = 1";
                    SetGlobalFlag(101, true);
                    break;
                case 290:
                    originalScript = "game.quests[40].state = qs_botched";
                    SetQuestState(40, QuestState.Botched);
                    break;
                case 300:
                    originalScript = "game.global_vars[10] = 3; game.quests[40].state = qs_completed; pc.reputation_add( 14 )";
                    SetGlobalVar(10, 3);
                    SetQuestState(40, QuestState.Completed);
                    pc.AddReputation(14);
                    ;
                    break;
                case 311:
                    originalScript = "pc.money_adjust(10000)";
                    throw new NotSupportedException("Conversion failed.");
                case 330:
                    originalScript = "game.global_vars[10] = 2; game.quests[40].state = qs_botched";
                    SetGlobalVar(10, 2);
                    SetQuestState(40, QuestState.Botched);
                    ;
                    break;
                case 350:
                    originalScript = "game.global_flags[290] = 1";
                    SetGlobalFlag(290, true);
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
                case 41:
                case 45:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 9);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
