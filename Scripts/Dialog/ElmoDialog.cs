
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
    [DialogScript(91)]
    public class ElmoDialog : Elmo, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 15:
                case 96:
                case 193:
                    Trace.Assert(originalScript == "game.global_vars[501] == 2");
                    return GetGlobalVar(501) == 2;
                case 21:
                case 24:
                    Trace.Assert(originalScript == "pc.money_get() >= 100");
                    return pc.GetMoney() >= 100;
                case 61:
                case 62:
                    Trace.Assert(originalScript == "pc.money_get() >= 20000 and not pc.follower_atmax()");
                    return pc.GetMoney() >= 20000 && !pc.HasMaxFollowers();
                case 63:
                case 64:
                    Trace.Assert(originalScript == "pc.money_get() < 20000");
                    return pc.GetMoney() < 20000;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 71:
                case 72:
                    Trace.Assert(originalScript == "game.global_flags[999]==0");
                    return !GetGlobalFlag(999);
                case 73:
                case 74:
                case 75:
                    Trace.Assert(originalScript == "game.global_flags[999]==1");
                    return GetGlobalFlag(999);
                case 91:
                    Trace.Assert(originalScript == "game.global_flags[66] == 0");
                    return !GetGlobalFlag(66);
                case 92:
                case 93:
                    Trace.Assert(originalScript == "game.global_flags[66] == 1 and not pc.follower_atmax()");
                    return GetGlobalFlag(66) && !pc.HasMaxFollowers();
                case 94:
                case 95:
                    Trace.Assert(originalScript == "game.global_flags[66] == 1 and pc.follower_atmax()");
                    return GetGlobalFlag(66) && pc.HasMaxFollowers();
                case 97:
                case 194:
                    Trace.Assert(originalScript == "game.quests[97].state == qs_completed");
                    return GetQuestState(97) == QuestState.Completed;
                case 102:
                case 105:
                    Trace.Assert(originalScript == "npc.area == 1");
                    return npc.GetArea() == 1;
                case 103:
                case 106:
                    Trace.Assert(originalScript == "npc.area == 3");
                    return npc.GetArea() == 3;
                case 107:
                case 108:
                    Trace.Assert(originalScript == "not npc.has_met( pc ) and game.global_flags[67] == 0");
                    return !npc.HasMet(pc) && !GetGlobalFlag(67);
                case 109:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8020)");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8020));
                case 121:
                case 122:
                    Trace.Assert(originalScript == "game.global_flags[70] == 0 and game.global_flags[71] == 0");
                    return !GetGlobalFlag(70) && !GetGlobalFlag(71);
                case 123:
                case 124:
                    Trace.Assert(originalScript == "game.global_flags[70] == 1 and game.global_flags[71] == 0 and game.global_flags[72] == 0");
                    return GetGlobalFlag(70) && !GetGlobalFlag(71) && !GetGlobalFlag(72);
                case 131:
                case 132:
                    Trace.Assert(originalScript == "game.global_flags[71] == 0");
                    return !GetGlobalFlag(71);
                case 203:
                case 204:
                    Trace.Assert(originalScript == "npc.leader_get() != OBJ_HANDLE_NULL");
                    return npc.GetLeader() != null;
                case 205:
                    Trace.Assert(originalScript == "game.quests[12].state != qs_unknown");
                    return GetQuestState(12) != QuestState.Unknown;
                case 241:
                    Trace.Assert(originalScript == "game.quests[97].state == qs_accepted");
                    return GetQuestState(97) == QuestState.Accepted;
                case 242:
                    Trace.Assert(originalScript == "game.quests[97].state != qs_accepted");
                    return GetQuestState(97) != QuestState.Accepted;
                case 401:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8014 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8014));
                case 402:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 8014 )");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8014));
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
                case 601:
                case 602:
                    Trace.Assert(originalScript == "npc.hit_dice_num == 4");
                    return GameSystems.Critter.GetHitDiceNum(npc) == 4;
                case 603:
                case 604:
                    Trace.Assert(originalScript == "npc.hit_dice_num > 4 and (npc.item_find(6049) or npc.item_find(6051) or npc.item_find(4098))");
                    return GameSystems.Critter.GetHitDiceNum(npc) > 4 && (npc.FindItemByName(6049) != null || npc.FindItemByName(6051) != null || npc.FindItemByName(4098) != null);
                case 711:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 14037 )");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14037));
                case 712:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 14037 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(14037));
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
                    Trace.Assert(originalScript == "game.global_vars[101] = 1");
                    SetGlobalVar(101, 1);
                    break;
                case 21:
                case 24:
                    Trace.Assert(originalScript == "pc.money_adj(-100)");
                    pc.AdjustMoney(-100);
                    break;
                case 70:
                    Trace.Assert(originalScript == "pc.follower_add(npc); game.global_flags[66] = 1");
                    pc.AddFollower(npc);
                    SetGlobalFlag(66, true);
                    ;
                    break;
                case 123:
                case 124:
                    Trace.Assert(originalScript == "game.global_flags[71] = 1");
                    SetGlobalFlag(71, true);
                    break;
                case 210:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 331:
                    Trace.Assert(originalScript == "make_otis_talk(npc,pc,420)");
                    make_otis_talk(npc, pc, 420);
                    break;
                case 341:
                case 342:
                    Trace.Assert(originalScript == "make_otis_talk(npc,pc,430)");
                    make_otis_talk(npc, pc, 430);
                    break;
                case 351:
                    Trace.Assert(originalScript == "make_otis_talk(npc,pc,440)");
                    make_otis_talk(npc, pc, 440);
                    break;
                case 401:
                    Trace.Assert(originalScript == "make_otis_talk(npc,pc,600)");
                    make_otis_talk(npc, pc, 600);
                    break;
                case 402:
                    Trace.Assert(originalScript == "make_lila_talk(npc,pc,95)");
                    make_lila_talk(npc, pc, 95);
                    break;
                case 561:
                case 562:
                    Trace.Assert(originalScript == "switch_to_thrommel(npc,pc)");
                    switch_to_thrommel(npc, pc);
                    break;
                case 581:
                    Trace.Assert(originalScript == "game.global_flags[999] = 1; elmo_joins_first_time(npc,pc,1)");
                    SetGlobalFlag(999, true);
                    elmo_joins_first_time(npc, pc, true);
                    ;
                    break;
                case 582:
                    Trace.Assert(originalScript == "game.global_flags[999] = 1;elmo_joins_first_time(npc,pc,1)");
                    SetGlobalFlag(999, true);
                    elmo_joins_first_time(npc, pc, true);
                    ;
                    break;
                case 583:
                case 584:
                    Trace.Assert(originalScript == "elmo_joins_first_time(npc,pc,0)");
                    elmo_joins_first_time(npc, pc, false);
                    break;
                case 603:
                case 604:
                    Trace.Assert(originalScript == "equip_transfer( npc, pc )");
                    equip_transfer(npc, pc);
                    break;
                case 712:
                    Trace.Assert(originalScript == "make_Fruella_talk(npc,pc,700)");
                    make_Fruella_talk(npc, pc, 700);
                    break;
                case 751:
                    Trace.Assert(originalScript == "make_saduj_talk(npc,pc,60)");
                    make_saduj_talk(npc, pc, 60);
                    break;
                case 761:
                    Trace.Assert(originalScript == "make_saduj_talk(npc,pc,70)");
                    make_saduj_talk(npc, pc, 70);
                    break;
                case 22000:
                    Trace.Assert(originalScript == "game.global_vars[900] = 32");
                    SetGlobalVar(900, 32);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
