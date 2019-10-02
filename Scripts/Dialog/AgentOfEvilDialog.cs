
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

namespace Scripts.Dialog
{
    [DialogScript(86)]
    public class AgentOfEvilDialog : AgentOfEvil, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 8:
                    Trace.Assert(originalScript == "game.global_flags[241] == 0");
                    return !GetGlobalFlag(241);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "game.quests[15].state == qs_accepted and game.global_flags[241] == 0 and game.global_flags[31] == 0");
                    return GetQuestState(15) == QuestState.Accepted && !GetGlobalFlag(241) && !GetGlobalFlag(31);
                case 6:
                case 7:
                    Trace.Assert(originalScript == "game.quests[15].state == qs_accepted and game.global_flags[241] == 1 and game.global_flags[31] == 0");
                    return GetQuestState(15) == QuestState.Accepted && GetGlobalFlag(241) && !GetGlobalFlag(31);
                case 13:
                case 14:
                    Trace.Assert(originalScript == "game.global_flags[65] == 1");
                    return GetGlobalFlag(65);
                case 22:
                case 31:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 5;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 2");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 2;
                case 83:
                case 84:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 85:
                case 86:
                case 95:
                case 96:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
                case 91:
                case 92:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 3");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 3;
                case 93:
                case 94:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 105:
                case 106:
                case 115:
                case 116:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 7 and game.global_flags[65] == 1");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7 && GetGlobalFlag(65);
                case 171:
                case 211:
                case 212:
                case 222:
                case 251:
                case 252:
                case 341:
                case 342:
                    Trace.Assert(originalScript == "game.global_flags[197] == 1");
                    return GetGlobalFlag(197);
                case 172:
                case 213:
                case 214:
                case 225:
                case 226:
                case 255:
                case 256:
                case 345:
                case 346:
                    Trace.Assert(originalScript == "game.global_flags[197] == 0");
                    return !GetGlobalFlag(197);
                case 193:
                case 194:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 7;
                case 201:
                case 202:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
                case 203:
                case 204:
                case 231:
                case 232:
                    Trace.Assert(originalScript == "pc.money_get() >= 50000");
                    return pc.GetMoney() >= 50000;
                case 205:
                case 206:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 4 and pc.money_get() >= 10000");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4 && pc.GetMoney() >= 10000;
                case 261:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD and pc.skill_level_get(npc,skill_bluff) >= 2) or game.party_alignment != LAWFUL_GOOD");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD && pc.GetSkillLevel(npc, SkillId.bluff) >= 2) || PartyAlignment != Alignment.LAWFUL_GOOD;
                case 291:
                case 292:
                case 383:
                case 384:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 2");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 2;
                case 301:
                case 308:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_strength) >= 18");
                    return pc.GetStat(Stat.strength) >= 18;
                case 303:
                case 309:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_dexterity) >= 18");
                    return pc.GetStat(Stat.dexterity) >= 18;
                case 304:
                case 310:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 4");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 4;
                case 305:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) >= 18");
                    return pc.GetStat(Stat.charisma) >= 18;
                case 402:
                    Trace.Assert(originalScript == "game.global_flags[241] == 1");
                    return GetGlobalFlag(241);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 4:
                case 5:
                case 8:
                    Trace.Assert(originalScript == "game.global_flags[241] = 1");
                    SetGlobalFlag(241, true);
                    break;
                case 53:
                case 54:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-5)");
                    npc.AdjustReaction(pc, -5);
                    break;
                case 180:
                case 185:
                case 210:
                case 220:
                case 250:
                case 340:
                case 390:
                    Trace.Assert(originalScript == "game.global_flags[31] = 1");
                    SetGlobalFlag(31, true);
                    break;
                case 181:
                case 186:
                case 211:
                case 212:
                case 221:
                case 222:
                case 223:
                case 224:
                case 361:
                case 391:
                case 392:
                case 395:
                    Trace.Assert(originalScript == "run_off(npc,pc); game.global_flags[528] = 1");
                    run_off(npc, pc);
                    SetGlobalFlag(528, true);
                    ;
                    break;
                case 203:
                case 204:
                case 231:
                case 232:
                    Trace.Assert(originalScript == "pc.money_adj(-50000)");
                    pc.AdjustMoney(-50000);
                    break;
                case 205:
                case 206:
                    Trace.Assert(originalScript == "pc.money_adj(-10000)");
                    pc.AdjustMoney(-10000);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 22:
                case 31:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 5);
                    return true;
                case 81:
                case 82:
                case 261:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 2);
                    return true;
                case 83:
                case 84:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                case 85:
                case 86:
                case 95:
                case 96:
                case 105:
                case 106:
                case 115:
                case 116:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                    return true;
                case 91:
                case 92:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 3);
                    return true;
                case 93:
                case 94:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                case 193:
                case 194:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 7);
                    return true;
                case 201:
                case 202:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 5);
                    return true;
                case 205:
                case 206:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                    return true;
                case 291:
                case 292:
                case 383:
                case 384:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 2);
                    return true;
                case 304:
                case 310:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
