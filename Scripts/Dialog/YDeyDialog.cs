
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
    [DialogScript(111)]
    public class YDeyDialog : YDey, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "(game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)");
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 7:
                case 8:
                    Trace.Assert(originalScript == "game.quests[31].state == qs_accepted and group_average_level(pc) <= 6 and anyone( pc.group_list(), \"has_follower\", 8014 )");
                    return GetQuestState(31) == QuestState.Accepted && Utilities.group_average_level(pc) <= 6 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8014));
                case 53:
                    Trace.Assert(originalScript == "((game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)) and (game.story_state == 3)");
                    return ((PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD)) && (StoryState == 3);
                case 54:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_NEUTRAL) or ((game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)) and (game.story_state == 3)");
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || ((PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD)) && (StoryState == 3);
                case 71:
                case 72:
                    Trace.Assert(originalScript == "(game.story_state == 3)");
                    return (StoryState == 3);
                case 182:
                case 184:
                case 191:
                case 193:
                case 392:
                case 394:
                    Trace.Assert(originalScript == "npc_get(npc, 1) == 1");
                    return ScriptDaemon.npc_get(npc, 1);
                case 183:
                case 185:
                case 192:
                case 194:
                case 393:
                case 395:
                    Trace.Assert(originalScript == "npc_get(npc, 1) == 0");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 211:
                case 212:
                    Trace.Assert(originalScript == "game.global_flags[306] == 0");
                    return !GetGlobalFlag(306);
                case 215:
                case 216:
                    Trace.Assert(originalScript == "game.global_flags[306] == 1 and game.global_flags[98] == 0 and game.quests[11].state == qs_accepted");
                    return GetGlobalFlag(306) && !GetGlobalFlag(98) && GetQuestState(11) == QuestState.Accepted;
                case 221:
                case 222:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 223:
                case 224:
                case 245:
                case 246:
                    Trace.Assert(originalScript == "game.global_flags[98] == 0 and game.quests[11].state == qs_accepted");
                    return !GetGlobalFlag(98) && GetQuestState(11) == QuestState.Accepted;
                case 252:
                case 253:
                case 254:
                case 255:
                    Trace.Assert(originalScript == "game.global_flags[100] == 1");
                    return GetGlobalFlag(100);
                case 501:
                    Trace.Assert(originalScript == "pc.money_get() >= 1500000");
                    return pc.GetMoney() >= 1500000;
                case 502:
                    Trace.Assert(originalScript == "pc.money_get() < 1500000");
                    return pc.GetMoney() < 1500000;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 80:
                    Trace.Assert(originalScript == "game.areas[4] = 1; game.story_state = 4");
                    MakeAreaKnown(4);
                    StoryState = 4;
                    ;
                    break;
                case 151:
                    Trace.Assert(originalScript == "buttin(npc,pc,450)");
                    buttin(npc, pc, 450);
                    break;
                case 170:
                    Trace.Assert(originalScript == "test_adding_two_followers( pc, npc )");
                    test_adding_two_followers(pc, npc);
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.global_vars[126] = 1");
                    SetGlobalVar(126, 1);
                    break;
                case 186:
                case 187:
                case 195:
                case 196:
                case 396:
                case 397:
                    Trace.Assert(originalScript == "buttin(npc,pc,460)");
                    buttin(npc, pc, 460);
                    break;
                case 220:
                    Trace.Assert(originalScript == "game.global_flags[306] = 1; game.global_vars[126] = 1");
                    SetGlobalFlag(306, true);
                    SetGlobalVar(126, 1);
                    ;
                    break;
                case 260:
                    Trace.Assert(originalScript == "game.global_flags[100] = 1; npc_set(npc, 2); pc.follower_add(npc)");
                    SetGlobalFlag(100, true);
                    ScriptDaemon.npc_set(npc, 2);
                    pc.AddFollower(npc);
                    ;
                    break;
                case 383:
                case 384:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 390:
                    Trace.Assert(originalScript == "game.global_vars[126] = 1; test_adding_two_followers( pc, npc )");
                    SetGlobalVar(126, 1);
                    test_adding_two_followers(pc, npc);
                    ;
                    break;
                case 401:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 501:
                    Trace.Assert(originalScript == "pc.money_adj(-1500000)");
                    pc.AdjustMoney(-1500000);
                    break;
                case 511:
                case 512:
                    Trace.Assert(originalScript == "create_item_in_inventory(6325,pc)");
                    Utilities.create_item_in_inventory(6325, pc);
                    break;
                case 22000:
                    Trace.Assert(originalScript == "game.global_vars[905] = 32");
                    SetGlobalVar(905, 32);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
