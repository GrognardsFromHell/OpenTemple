
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
    [DialogScript(143)]
    public class PaidaDialog : Paida, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                case 53:
                case 54:
                    Trace.Assert(originalScript == "game.global_flags[146] == 0");
                    return !GetGlobalFlag(146);
                case 15:
                case 16:
                case 45:
                case 46:
                case 55:
                case 56:
                    Trace.Assert(originalScript == "game.global_flags[146] == 1");
                    return GetGlobalFlag(146);
                case 19:
                case 20:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male");
                    return pc.GetGender() == Gender.Male;
                case 21:
                case 22:
                case 117:
                case 118:
                    Trace.Assert(originalScript == "game.global_flags[158] == 1");
                    return GetGlobalFlag(158);
                case 43:
                case 44:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 12;
                case 83:
                case 84:
                    Trace.Assert(originalScript == "game.quests[20].state == qs_mentioned or game.quests[20].state == qs_accepted");
                    return GetQuestState(20) == QuestState.Mentioned || GetQuestState(20) == QuestState.Accepted;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "npc.area == 1");
                    return npc.GetArea() == 1;
                case 115:
                case 116:
                    Trace.Assert(originalScript == "npc.area != 1");
                    return npc.GetArea() != 1;
                case 121:
                case 122:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 173:
                    Trace.Assert(originalScript == "game.quests[98].state == qs_accepted and game.global_flags[691] != 1");
                    return GetQuestState(98) == QuestState.Accepted && !GetGlobalFlag(691);
                case 183:
                case 192:
                    Trace.Assert(originalScript == "game.quests[98].state == qs_accepted and game.global_flags[691] != 1 and (pc.skill_level_get(npc,skill_diplomacy) >=9 or pc.skill_level_get(npc,skill_bluff) >= 9)");
                    return GetQuestState(98) == QuestState.Accepted && !GetGlobalFlag(691) && (pc.GetSkillLevel(npc, SkillId.diplomacy) >= 9 || pc.GetSkillLevel(npc, SkillId.bluff) >= 9);
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
                    Trace.Assert(originalScript == "game.global_flags[159] = 1; game.global_vars[112] = 1");
                    SetGlobalFlag(159, true);
                    SetGlobalVar(112, 1);
                    ;
                    break;
                case 2:
                case 3:
                    Trace.Assert(originalScript == "LookHedrack(npc,pc,300)");
                    LookHedrack(npc, pc, 300);
                    break;
                case 60:
                    Trace.Assert(originalScript == "game.global_flags[144] = 1");
                    SetGlobalFlag(144, true);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.global_flags[932] = 1");
                    SetGlobalFlag(932, true);
                    break;
                case 87:
                case 88:
                    Trace.Assert(originalScript == "game.global_flags[149] = 1; run_off( npc, pc )");
                    SetGlobalFlag(149, true);
                    run_off(npc, pc);
                    ;
                    break;
                case 91:
                case 92:
                case 101:
                case 102:
                    Trace.Assert(originalScript == "pc.follower_add( npc )");
                    pc.AddFollower(npc);
                    break;
                case 93:
                case 94:
                case 213:
                case 214:
                    Trace.Assert(originalScript == "game.global_flags[149] = 1");
                    SetGlobalFlag(149, true);
                    break;
                case 120:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+30)");
                    npc.AdjustReaction(pc, +30);
                    break;
                case 121:
                case 122:
                    Trace.Assert(originalScript == "game.quests[20].state = qs_botched; pc.follower_remove( npc )");
                    SetQuestState(20, QuestState.Botched);
                    pc.RemoveFollower(npc);
                    ;
                    break;
                case 123:
                case 124:
                    Trace.Assert(originalScript == "game.quests[20].state = qs_completed; game.global_flags[38] = 1");
                    SetQuestState(20, QuestState.Completed);
                    SetGlobalFlag(38, true);
                    ;
                    break;
                case 131:
                case 132:
                case 135:
                case 136:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); game.global_flags[149] = 1");
                    pc.RemoveFollower(npc);
                    SetGlobalFlag(149, true);
                    ;
                    break;
                case 140:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-20)");
                    npc.AdjustReaction(pc, -20);
                    break;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "run_off( npc, pc )");
                    run_off(npc, pc);
                    break;
                case 221:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); run_off( npc, pc )");
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    ;
                    break;
                case 281:
                    Trace.Assert(originalScript == "game.global_flags[691] = 1");
                    SetGlobalFlag(691, true);
                    break;
                case 291:
                    Trace.Assert(originalScript == "create_item_in_inventory(12897,pc)");
                    Utilities.create_item_in_inventory(12897, pc);
                    break;
                case 20000:
                case 22000:
                    Trace.Assert(originalScript == "game.global_vars[902] = 32");
                    SetGlobalVar(902, 32);
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
                case 43:
                case 44:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 12);
                    return true;
                case 183:
                case 192:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 9, SkillId.bluff, 9);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
