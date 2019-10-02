
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
    [DialogScript(97)]
    public class OtisDialog : Otis, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 4:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and find_container_near(npc,1205) != OBJ_HANDLE_NULL");
                    return npc.HasMet(pc) && Utilities.find_container_near(npc, 1205) != null;
                case 5:
                case 6:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 21:
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.story_state == 3");
                    return StoryState == 3;
                case 22:
                case 23:
                case 24:
                case 25:
                    Trace.Assert(originalScript == "game.story_state >= 4");
                    return StoryState >= 4;
                case 31:
                case 33:
                    Trace.Assert(originalScript == "npc.leader_get() == OBJ_HANDLE_NULL");
                    return npc.GetLeader() == null;
                case 35:
                    Trace.Assert(originalScript == "npc.leader_get() != OBJ_HANDLE_NULL");
                    return npc.GetLeader() != null;
                case 41:
                case 42:
                case 109:
                case 110:
                    Trace.Assert(originalScript == "(game.global_flags[73] == 0) and (npc.leader_get() == OBJ_HANDLE_NULL) and (game.global_flags[360] == 0)");
                    return (!GetGlobalFlag(73)) && (npc.GetLeader() == null) && (!GetGlobalFlag(360));
                case 43:
                case 44:
                case 111:
                case 112:
                    Trace.Assert(originalScript == "(game.global_flags[73] == 0) and (npc.leader_get() == OBJ_HANDLE_NULL) and (game.global_flags[360] == 1) and (pc.follower_atmax())");
                    return (!GetGlobalFlag(73)) && (npc.GetLeader() == null) && (GetGlobalFlag(360)) && (pc.HasMaxFollowers());
                case 45:
                case 46:
                case 113:
                case 114:
                    Trace.Assert(originalScript == "(game.global_flags[73] == 0) and (npc.leader_get() == OBJ_HANDLE_NULL) and (game.global_flags[360] == 1) and (not pc.follower_atmax())");
                    return (!GetGlobalFlag(73)) && (npc.GetLeader() == null) && (GetGlobalFlag(360)) && (!pc.HasMaxFollowers());
                case 51:
                case 52:
                    Trace.Assert(originalScript == "pc.money_get() >= 17500 and not pc.follower_atmax()");
                    return pc.GetMoney() >= 17500 && !pc.HasMaxFollowers();
                case 53:
                case 54:
                    Trace.Assert(originalScript == "pc.money_get() < 17500");
                    return pc.GetMoney() < 17500;
                case 55:
                case 56:
                case 134:
                case 138:
                case 455:
                case 456:
                case 465:
                case 466:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 101:
                case 102:
                    Trace.Assert(originalScript == "((game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)) and (game.global_flags[74] == 1) and (npc.leader_get() == OBJ_HANDLE_NULL)");
                    return ((PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL)) && (GetGlobalFlag(74)) && (npc.GetLeader() == null);
                case 105:
                case 106:
                    Trace.Assert(originalScript == "game.story_state != 3");
                    return StoryState != 3;
                case 121:
                case 124:
                    Trace.Assert(originalScript == "((game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)) and (game.story_state >= 5)");
                    return ((PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD)) && (StoryState >= 5);
                case 122:
                case 125:
                    Trace.Assert(originalScript == "((game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)) and (game.story_state == 4)");
                    return ((PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD)) && (StoryState == 4);
                case 123:
                case 126:
                    Trace.Assert(originalScript == "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != NEUTRAL_GOOD) and (game.party_alignment != CHAOTIC_GOOD)");
                    return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.NEUTRAL_GOOD) && (PartyAlignment != Alignment.CHAOTIC_GOOD);
                case 128:
                case 206:
                    Trace.Assert(originalScript == "find_container_near(npc,1205) != OBJ_HANDLE_NULL");
                    return Utilities.find_container_near(npc, 1205) != null;
                case 131:
                case 135:
                    Trace.Assert(originalScript == "(group_average_level(pc) > 5) and (not pc.follower_atmax())");
                    return (Utilities.group_average_level(pc) > 5) && (!pc.HasMaxFollowers());
                case 132:
                case 136:
                    Trace.Assert(originalScript == "(group_average_level(pc) <= 5) and (not pc.follower_atmax())");
                    return (Utilities.group_average_level(pc) <= 5) && (!pc.HasMaxFollowers());
                case 165:
                case 166:
                    Trace.Assert(originalScript == "((game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)) and (game.global_flags[74] == 1)");
                    return ((PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL)) && (GetGlobalFlag(74));
                case 201:
                case 202:
                    Trace.Assert(originalScript == "game.story_state == 5");
                    return StoryState == 5;
                case 203:
                case 204:
                    Trace.Assert(originalScript == "game.story_state == 6");
                    return StoryState == 6;
                case 211:
                case 213:
                    Trace.Assert(originalScript == "game.global_flags[74] == 0");
                    return !GetGlobalFlag(74);
                case 212:
                case 214:
                    Trace.Assert(originalScript == "game.global_flags[74] == 1");
                    return GetGlobalFlag(74);
                case 251:
                case 252:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 8 and not pc.follower_atmax()");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8 && !pc.HasMaxFollowers();
                case 262:
                case 263:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 4");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 4;
                case 271:
                case 272:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 291:
                case 293:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL);
                case 292:
                case 294:
                    Trace.Assert(originalScript == "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != LAWFUL_NEUTRAL) and (game.party_alignment != LAWFUL_EVIL)");
                    return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.LAWFUL_NEUTRAL) && (PartyAlignment != Alignment.LAWFUL_EVIL);
                case 351:
                case 352:
                    Trace.Assert(originalScript == "npc.area == 3");
                    return npc.GetArea() == 3;
                case 353:
                case 354:
                    Trace.Assert(originalScript == "npc.area != 3");
                    return npc.GetArea() != 3;
                case 453:
                case 454:
                case 463:
                case 464:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 531:
                case 532:
                    Trace.Assert(originalScript == "game.global_flags[94] == 1");
                    return GetGlobalFlag(94);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 30:
                    Trace.Assert(originalScript == "game.areas[4] = 1; game.story_state = 4");
                    MakeAreaKnown(4);
                    StoryState = 4;
                    ;
                    break;
                case 51:
                case 52:
                    Trace.Assert(originalScript == "pc.money_adj(-17500); game.global_flags[360] = 1");
                    pc.AdjustMoney(-17500);
                    SetGlobalFlag(360, true);
                    ;
                    break;
                case 60:
                case 190:
                case 260:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.quests[31].state = qs_mentioned");
                    SetQuestState(31, QuestState.Mentioned);
                    break;
                case 131:
                case 132:
                case 135:
                case 136:
                    Trace.Assert(originalScript == "game.quests[31].state = qs_accepted");
                    SetQuestState(31, QuestState.Accepted);
                    break;
                case 300:
                    Trace.Assert(originalScript == "game.global_flags[74] = 1");
                    SetGlobalFlag(74, true);
                    break;
                case 331:
                case 334:
                case 341:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 360:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 400:
                    Trace.Assert(originalScript == "game.global_flags[72] = 1");
                    SetGlobalFlag(72, true);
                    break;
                case 401:
                    Trace.Assert(originalScript == "make_elmo_talk(npc,pc,310)");
                    make_elmo_talk(npc, pc, 310);
                    break;
                case 421:
                    Trace.Assert(originalScript == "make_elmo_talk(npc,pc,340)");
                    make_elmo_talk(npc, pc, 340);
                    break;
                case 431:
                    Trace.Assert(originalScript == "make_elmo_talk(npc,pc,350)");
                    make_elmo_talk(npc, pc, 350);
                    break;
                case 451:
                case 452:
                    Trace.Assert(originalScript == "talk_to_screng(npc,pc,390)");
                    talk_to_screng(npc, pc, 390);
                    break;
                case 453:
                case 454:
                case 463:
                case 464:
                    Trace.Assert(originalScript == "talk_to_screng(npc,pc,260)");
                    talk_to_screng(npc, pc, 260);
                    break;
                case 455:
                case 456:
                case 465:
                case 466:
                    Trace.Assert(originalScript == "talk_to_screng(npc,pc,270)");
                    talk_to_screng(npc, pc, 270);
                    break;
                case 501:
                case 502:
                    Trace.Assert(originalScript == "switch_to_thrommel(npc,pc)");
                    switch_to_thrommel(npc, pc);
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
                case 251:
                case 252:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 262:
                case 263:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 4);
                    return true;
                case 271:
                case 272:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
