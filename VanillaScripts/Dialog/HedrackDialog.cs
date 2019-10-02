
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
    [DialogScript(174)]
    public class HedrackDialog : Hedrack, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 13:
                case 14:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 17:
                case 18:
                case 53:
                case 54:
                case 311:
                case 312:
                    Trace.Assert(originalScript == "game.quests[48].state == qs_completed");
                    return GetQuestState(48) == QuestState.Completed;
                case 55:
                case 56:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_gather_information) >= 6 or pc.stat_level_get(stat_level_bard) >= 1");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 6 || pc.GetStat(Stat.level_bard) >= 1;
                case 57:
                case 472:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_cleric) >= 1 and pc.skill_level_get(npc, skill_diplomacy) >= 8");
                    return pc.GetStat(Stat.level_cleric) >= 1 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                case 58:
                case 473:
                    Trace.Assert(originalScript == "( game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and pc.skill_level_get(npc, skill_diplomacy) >= 8");
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "( pc.skill_level_get(npc, skill_gather_information) >= 6 or pc.stat_level_get(stat_level_bard) >= 1 ) and game.story_state <= 4");
                    return (pc.GetSkillLevel(npc, SkillId.gather_information) >= 6 || pc.GetStat(Stat.level_bard) >= 1) && StoryState <= 4;
                case 143:
                case 144:
                    Trace.Assert(originalScript == "game.story_state == 5");
                    return StoryState == 5;
                case 145:
                case 146:
                case 172:
                    Trace.Assert(originalScript == "game.story_state == 6");
                    return StoryState == 6;
                case 147:
                case 148:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_gather_information) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 9;
                case 153:
                case 154:
                    Trace.Assert(originalScript == "game.global_flags[144] == 0");
                    return !GetGlobalFlag(144);
                case 215:
                case 216:
                case 363:
                case 364:
                case 483:
                case 484:
                    Trace.Assert(originalScript == "game.global_flags[182] == 1");
                    return GetGlobalFlag(182);
                case 217:
                case 218:
                case 485:
                case 486:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8034 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8034));
                case 295:
                case 296:
                    Trace.Assert(originalScript == "game.global_flags[182] == 1 and game.quests[58].state >= qs_accepted");
                    return GetGlobalFlag(182) && GetQuestState(58) >= QuestState.Accepted;
                case 301:
                case 302:
                    Trace.Assert(originalScript == "game.global_flags[145] == 0");
                    return !GetGlobalFlag(145);
                case 303:
                case 304:
                    Trace.Assert(originalScript == "game.global_flags[145] == 1 and game.quests[58].state == qs_unknown");
                    return GetGlobalFlag(145) && GetQuestState(58) == QuestState.Unknown;
                case 305:
                case 306:
                    Trace.Assert(originalScript == "game.global_flags[145] == 1 and game.quests[58].state >= qs_mentioned");
                    return GetGlobalFlag(145) && GetQuestState(58) >= QuestState.Mentioned;
                case 361:
                case 362:
                    Trace.Assert(originalScript == "game.global_flags[182] == 0");
                    return !GetGlobalFlag(182);
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
                case 10:
                case 310:
                    Trace.Assert(originalScript == "game.global_flags[145] = 1");
                    SetGlobalFlag(145, true);
                    break;
                case 11:
                case 12:
                case 33:
                case 34:
                case 51:
                case 52:
                case 61:
                case 62:
                case 71:
                case 72:
                case 91:
                case 92:
                case 101:
                case 102:
                case 121:
                case 122:
                case 131:
                case 132:
                case 161:
                case 171:
                case 191:
                case 192:
                case 193:
                case 194:
                case 195:
                case 196:
                case 231:
                case 291:
                case 292:
                case 321:
                case 322:
                case 331:
                case 332:
                case 341:
                case 351:
                case 402:
                case 412:
                case 471:
                case 491:
                case 521:
                case 522:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 20:
                case 30:
                case 40:
                    Trace.Assert(originalScript == "game.global_flags[145] = 1; game.global_flags[144] = 0");
                    SetGlobalFlag(145, true);
                    SetGlobalFlag(144, false);
                    ;
                    break;
                case 21:
                    Trace.Assert(originalScript == "talk_Romag(npc,pc,570)");
                    talk_Romag(npc, pc, 570);
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.global_flags[144] == 1");
                    GetGlobalFlag(144);
                    break;
                case 190:
                    Trace.Assert(originalScript == "summon_Iuz( npc, pc )");
                    summon_Iuz(npc, pc);
                    break;
                case 201:
                    Trace.Assert(originalScript == "talk_Iuz(npc,pc,110)");
                    talk_Iuz(npc, pc, 110);
                    break;
                case 210:
                    Trace.Assert(originalScript == "game.quests[58].state = qs_mentioned");
                    SetQuestState(58, QuestState.Mentioned);
                    break;
                case 211:
                case 212:
                    Trace.Assert(originalScript == "game.quests[58].state = qs_accepted");
                    SetQuestState(58, QuestState.Accepted);
                    break;
                case 215:
                case 216:
                case 217:
                case 218:
                case 390:
                    Trace.Assert(originalScript == "game.quests[58].state = qs_completed");
                    SetQuestState(58, QuestState.Completed);
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.map_flags( 5078, 1, 1 ); give_robes( npc, pc )");
                    // FIXME: map_flags;
                    give_robes(npc, pc);
                    ;
                    break;
                case 260:
                    Trace.Assert(originalScript == "game.story_state = 5");
                    StoryState = 5;
                    break;
                case 270:
                case 440:
                    Trace.Assert(originalScript == "game.story_state = 6");
                    StoryState = 6;
                    break;
                case 411:
                    Trace.Assert(originalScript == "end_game(pc,npc)");
                    end_game(pc, npc);
                    break;
                case 500:
                case 510:
                    Trace.Assert(originalScript == "npc.item_transfer_to( pc, 3021 )");
                    npc.TransferItemByNameTo(pc, 3021);
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
                case 13:
                case 14:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                case 55:
                case 56:
                case 141:
                case 142:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 6);
                    return true;
                case 57:
                case 58:
                case 472:
                case 473:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                case 147:
                case 148:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 9);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
