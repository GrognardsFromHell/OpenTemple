
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
    [DialogScript(112)]
    public class MurflesDialog : Murfles, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 8:
                case 9:
                case 357:
                case 358:
                    Trace.Assert(originalScript == "game.quests[31].state == qs_completed");
                    return GetQuestState(31) == QuestState.Completed;
                case 22:
                case 23:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 7;
                case 53:
                case 54:
                case 121:
                case 122:
                case 135:
                case 136:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 55:
                case 56:
                case 131:
                case 132:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 61:
                case 62:
                    Trace.Assert(originalScript == "game.story_state == 3");
                    return StoryState == 3;
                case 63:
                case 64:
                    Trace.Assert(originalScript == "game.story_state == 4");
                    return StoryState == 4;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "game.story_state == 5");
                    return StoryState == 5;
                case 67:
                case 68:
                case 253:
                case 254:
                    Trace.Assert(originalScript == "game.story_state == 6");
                    return StoryState == 6;
                case 81:
                case 82:
                case 91:
                case 92:
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.global_flags[98] == 0");
                    return !GetGlobalFlag(98);
                case 83:
                case 84:
                case 93:
                case 94:
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.global_flags[98] == 1");
                    return GetGlobalFlag(98);
                case 123:
                case 124:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 125:
                case 126:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 133:
                case 134:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_level_druid ) >= 1");
                    return pc.GetStat(Stat.level_druid) >= 1;
                case 195:
                case 196:
                    Trace.Assert(originalScript == "game.global_flags[306] == 1 and game.global_flags[307] == 0");
                    return GetGlobalFlag(306) && !GetGlobalFlag(307);
                case 321:
                case 322:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
                case 325:
                case 326:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-10)");
                    npc.AdjustReaction(pc, -10);
                    break;
                case 43:
                    Trace.Assert(originalScript == "buttin(npc,pc,380)");
                    buttin(npc, pc, 380);
                    break;
                case 60:
                case 320:
                    Trace.Assert(originalScript == "game.global_vars[127] = 1");
                    SetGlobalVar(127, 1);
                    break;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.global_flags[98] = 1; npc.item_transfer_to_by_proto(pc,5008)");
                    SetGlobalFlag(98, true);
                    npc.TransferItemByProtoTo(pc, 5008);
                    ;
                    break;
                case 171:
                case 172:
                case 231:
                case 232:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 173:
                case 174:
                    Trace.Assert(originalScript == "make_hate( npc, pc )");
                    make_hate(npc, pc);
                    break;
                case 195:
                case 196:
                    Trace.Assert(originalScript == "game.global_flags[307] = 1");
                    SetGlobalFlag(307, true);
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.global_flags[313] = 1");
                    SetGlobalFlag(313, true);
                    break;
                case 330:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 22000:
                    Trace.Assert(originalScript == "game.global_vars[906] = 32");
                    SetGlobalVar(906, 32);
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
                case 23:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
