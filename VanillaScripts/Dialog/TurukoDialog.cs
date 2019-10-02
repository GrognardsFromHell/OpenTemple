
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
    [DialogScript(67)]
    public class TurukoDialog : Turuko, IDialogScript
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
                case 11:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 5:
                case 6:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL and game.quests[28].state != qs_completed and anyone( pc.group_list(), \"has_item\", 2201 )");
                    return PartyAlignment == Alignment.LAWFUL_EVIL && GetQuestState(28) != QuestState.Completed && pc.GetPartyMembers().Any(o => o.HasItemByName(2201));
                case 7:
                case 8:
                    Trace.Assert(originalScript == "(game.quests[28].state == qs_accepted) and (game.story_state == 2)");
                    return (GetQuestState(28) == QuestState.Accepted) && (StoryState == 2);
                case 9:
                case 10:
                    Trace.Assert(originalScript == "(game.quests[28].state == qs_accepted) and (game.story_state > 2)");
                    return (GetQuestState(28) == QuestState.Accepted) && (StoryState > 2);
                case 13:
                case 14:
                    Trace.Assert(originalScript == "game.party_alignment != LAWFUL_EVIL or game.global_flags[67] == 1");
                    return PartyAlignment != Alignment.LAWFUL_EVIL || GetGlobalFlag(67);
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.LAWFUL_EVIL && !GetGlobalFlag(67);
                case 23:
                case 24:
                case 31:
                case 32:
                case 41:
                case 42:
                    Trace.Assert(originalScript == "npc.leader_get() == OBJ_HANDLE_NULL and pc.stat_level_get(stat_level_paladin) == 0 and game.party_alignment != LAWFUL_EVIL");
                    return npc.GetLeader() == null && pc.GetStat(Stat.level_paladin) == 0 && PartyAlignment != Alignment.LAWFUL_EVIL;
                case 25:
                    Trace.Assert(originalScript == "npc.leader_get() == OBJ_HANDLE_NULL and game.story_state == 0 and game.party_alignment != LAWFUL_EVIL");
                    return npc.GetLeader() == null && StoryState == 0 && PartyAlignment != Alignment.LAWFUL_EVIL;
                case 26:
                    Trace.Assert(originalScript == "(npc.leader_get() != OBJ_HANDLE_NULL or game.party_alignment == LAWFUL_EVIL) and game.story_state == 0");
                    return (npc.GetLeader() != null || PartyAlignment == Alignment.LAWFUL_EVIL) && StoryState == 0;
                case 51:
                case 54:
                    Trace.Assert(originalScript == "game.party_size() <= 4 and not pc.follower_atmax() and game.party_alignment != LAWFUL_EVIL");
                    return GameSystems.Party.PartySize <= 4 && !pc.HasMaxFollowers() && PartyAlignment != Alignment.LAWFUL_EVIL;
                case 52:
                case 55:
                    Trace.Assert(originalScript == "game.party_size() > 4 and not pc.follower_atmax() and game.party_alignment != LAWFUL_EVIL");
                    return GameSystems.Party.PartySize > 4 && !pc.HasMaxFollowers() && PartyAlignment != Alignment.LAWFUL_EVIL;
                case 53:
                case 56:
                    Trace.Assert(originalScript == "pc.follower_atmax() and game.party_alignment != LAWFUL_EVIL");
                    return pc.HasMaxFollowers() && PartyAlignment != Alignment.LAWFUL_EVIL;
                case 57:
                case 58:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 72:
                case 73:
                case 101:
                case 107:
                    Trace.Assert(originalScript == "game.story_state == 0");
                    return StoryState == 0;
                case 102:
                case 108:
                    Trace.Assert(originalScript == "game.story_state == 1");
                    return StoryState == 1;
                case 103:
                case 109:
                    Trace.Assert(originalScript == "game.story_state == 2");
                    return StoryState == 2;
                case 104:
                case 110:
                    Trace.Assert(originalScript == "game.story_state == 3");
                    return StoryState == 3;
                case 105:
                case 111:
                    Trace.Assert(originalScript == "game.story_state >= 4");
                    return StoryState >= 4;
                case 131:
                case 134:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6;
                case 132:
                case 135:
                    Trace.Assert(originalScript == "game.story_state >= 2");
                    return StoryState >= 2;
                case 141:
                case 142:
                case 145:
                case 161:
                case 162:
                case 163:
                case 164:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 143:
                case 144:
                case 165:
                case 166:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) > 0");
                    return pc.GetStat(Stat.level_paladin) > 0;
                case 261:
                case 262:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
                case 301:
                case 302:
                    Trace.Assert(originalScript == "npc.area != 1 and npc.area != 3");
                    return npc.GetArea() != 1 && npc.GetArea() != 3;
                case 303:
                case 304:
                    Trace.Assert(originalScript == "npc.area == 1 or npc.area == 3");
                    return npc.GetArea() == 1 || npc.GetArea() == 3;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 6:
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.global_flags[67] = 1");
                    SetGlobalFlag(67, true);
                    break;
                case 70:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 120:
                case 175:
                    Trace.Assert(originalScript == "game.areas[2] = 1; game.story_state = 1");
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 123:
                case 124:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(2)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.quests[28].state = qs_completed; party_transfer_to( npc, 2201 )");
                    SetQuestState(28, QuestState.Completed);
                    Utilities.party_transfer_to(npc, 2201);
                    ;
                    break;
                case 181:
                case 182:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 330:
                case 410:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 421:
                    Trace.Assert(originalScript == "pc.follower_remove(npc); npc.attack(pc)");
                    pc.RemoveFollower(npc);
                    npc.Attack(pc);
                    ;
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
                case 131:
                case 134:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                    return true;
                case 261:
                case 262:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 5);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
