
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
    [DialogScript(11)]
    public class FarmerWifeDialog : FarmerWife, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 4:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 5:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[6].state <= qs_mentioned");
                    return npc.HasMet(pc) && GetQuestState(6) <= QuestState.Mentioned;
                case 6:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[6].state >= qs_accepted");
                    return npc.HasMet(pc) && GetQuestState(6) >= QuestState.Accepted;
                case 13:
                case 18:
                    Trace.Assert(originalScript == "game.global_flags[67] == 0");
                    return !GetGlobalFlag(67);
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.quests[6].state == qs_mentioned");
                    return GetQuestState(6) == QuestState.Mentioned;
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.global_flags[2] == 0");
                    return !GetGlobalFlag(2);
                case 105:
                case 106:
                    Trace.Assert(originalScript == "game.quests[6].state == qs_unknown and game.global_flags[2] == 1");
                    return GetQuestState(6) == QuestState.Unknown && GetGlobalFlag(2);
                case 122:
                case 123:
                    Trace.Assert(originalScript == "game.global_flags[14] == 1");
                    return GetGlobalFlag(14);
                case 501:
                case 510:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    Trace.Assert(originalScript == "game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 1011:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 1012:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 1013:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 1014:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 1015:
                case 1016:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 20:
                    Trace.Assert(originalScript == "game.global_flags[2] = 1");
                    SetGlobalFlag(2, true);
                    break;
                case 40:
                case 84:
                    Trace.Assert(originalScript == "game.quests[6].state = qs_mentioned");
                    SetQuestState(6, QuestState.Mentioned);
                    break;
                case 41:
                case 42:
                case 43:
                case 85:
                case 86:
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.quests[6].state = qs_accepted");
                    SetQuestState(6, QuestState.Accepted);
                    break;
                case 110:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+10)");
                    npc.AdjustReaction(pc, +10);
                    break;
                case 1003:
                case 1004:
                case 1017:
                case 1018:
                case 1131:
                case 1132:
                case 2110:
                case 2111:
                    Trace.Assert(originalScript == "npc.critter_flag_set( OCF_MUTE ); npc.attack( pc )");
                    npc.SetCritterFlag(CritterFlag.MUTE);
                    npc.Attack(pc);
                    ;
                    break;
                case 1130:
                    Trace.Assert(originalScript == "game.global_flags[813] = 1");
                    SetGlobalFlag(813, true);
                    break;
                case 1133:
                case 1134:
                    Trace.Assert(originalScript == "npc.critter_flag_set( OCF_MUTE ); run_off( npc, pc )");
                    npc.SetCritterFlag(CritterFlag.MUTE);
                    run_off(npc, pc);
                    ;
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
                case 1011:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 1012:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 1013:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                case 1014:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                case 1015:
                case 1016:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
