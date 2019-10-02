
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog
{
    [DialogScript(61)]
    public class GremagDialog : Gremag, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 4:
                case 34:
                case 35:
                case 52:
                case 53:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 5;
                case 7:
                case 8:
                case 54:
                case 55:
                    Trace.Assert(originalScript == "game.global_flags[40] == 1");
                    return GetGlobalFlag(40);
                case 9:
                case 10:
                case 56:
                case 57:
                    Trace.Assert(originalScript == "game.global_flags[292] == 1");
                    return GetGlobalFlag(292);
                case 31:
                case 32:
                    Trace.Assert(originalScript == "game.global_flags[41] == 0");
                    return !GetGlobalFlag(41);
                case 36:
                case 37:
                case 46:
                case 47:
                    Trace.Assert(originalScript == "game.global_flags[39] == 0");
                    return !GetGlobalFlag(39);
                case 41:
                case 42:
                    Trace.Assert(originalScript == "game.quests[16].state != qs_completed");
                    return GetQuestState(16) != QuestState.Completed;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 2");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 2;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "pc.money_get() >= 20000");
                    return pc.GetMoney() >= 20000;
                case 115:
                case 116:
                case 212:
                case 216:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 3");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 3;
                case 131:
                case 132:
                    Trace.Assert(originalScript == "pc.money_get() >= 15000");
                    return pc.GetMoney() >= 15000;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 2");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 2;
                case 143:
                case 144:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 4");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 4;
                case 145:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 9;
                case 161:
                case 163:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 181:
                case 182:
                    Trace.Assert(originalScript == "game.areas[2] == 0");
                    return !IsAreaKnown(2);
                case 214:
                case 218:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 2 and game.areas[2] == 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 2 && !IsAreaKnown(2);
                case 219:
                case 220:
                    Trace.Assert(originalScript == "game.quests[15].state == qs_completed");
                    return GetQuestState(15) == QuestState.Completed;
                case 384:
                case 385:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 4");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 4;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[197] = 1");
                    SetGlobalFlag(197, true);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.quests[16].state = qs_mentioned");
                    SetQuestState(16, QuestState.Mentioned);
                    break;
                case 41:
                case 42:
                    Trace.Assert(originalScript == "game.quests[16].state = qs_accepted");
                    SetQuestState(16, QuestState.Accepted);
                    break;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "game.global_flags[39] = 1; pc.money_adj(-20000)");
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-20000);
                    ;
                    break;
                case 131:
                case 132:
                    Trace.Assert(originalScript == "game.global_flags[39] = 1; pc.money_adj(-15000)");
                    SetGlobalFlag(39, true);
                    pc.AdjustMoney(-15000);
                    ;
                    break;
                case 160:
                case 170:
                case 180:
                    Trace.Assert(originalScript == "game.global_flags[41] = 1");
                    SetGlobalFlag(41, true);
                    break;
                case 161:
                case 163:
                    Trace.Assert(originalScript == "pc.money_adj(+20000)");
                    pc.AdjustMoney(+20000);
                    break;
                case 162:
                case 164:
                case 211:
                case 215:
                    Trace.Assert(originalScript == "pc.money_adj(+5000)");
                    pc.AdjustMoney(+5000);
                    break;
                case 171:
                case 172:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,4107)");
                    npc.TransferItemByProtoTo(pc, 4107);
                    break;
                case 185:
                case 240:
                    Trace.Assert(originalScript == "game.areas[2] = 1; game.story_state = 1");
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 186:
                case 187:
                case 241:
                case 242:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(2)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 210:
                    Trace.Assert(originalScript == "game.quests[16].state = qs_completed");
                    SetQuestState(16, QuestState.Completed);
                    break;
                case 212:
                case 216:
                    Trace.Assert(originalScript == "pc.money_adj(+10000)");
                    pc.AdjustMoney(+10000);
                    break;
                case 290:
                    Trace.Assert(originalScript == "game.global_flags[293] = 1");
                    SetGlobalFlag(293, true);
                    break;
                case 370:
                    Trace.Assert(originalScript == "pc.prestige_class_add[3]");
                    throw new NotSupportedException("Conversion failed.");
                case 380:
                    Trace.Assert(originalScript == "game.global_flags[292] = 0");
                    SetGlobalFlag(292, false);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 3:
                case 4:
                case 34:
                case 35:
                case 52:
                case 53:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 5);
                    return true;
                case 81:
                case 82:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 2);
                    return true;
                case 115:
                case 116:
                case 212:
                case 216:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 3);
                    return true;
                case 141:
                case 142:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 2);
                    return true;
                case 143:
                case 144:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 4);
                    return true;
                case 145:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 9);
                    return true;
                case 161:
                case 163:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                case 214:
                case 218:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 2);
                    return true;
                case 384:
                case 385:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
