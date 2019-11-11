
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
    [DialogScript(177)]
    public class ScorppDialog : Scorpp, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                case 15:
                case 16:
                case 53:
                case 54:
                    originalScript = "game.quests[58].state >= qs_mentioned";
                    return GetQuestState(58) >= QuestState.Mentioned;
                case 13:
                case 14:
                case 343:
                case 344:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 17:
                case 18:
                case 35:
                case 36:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female and pc.stat_level_get(stat_charisma) >= 16 and game.global_flags[322] == 1";
                    return pc.GetGender() == Gender.Female && pc.GetStat(Stat.charisma) >= 16 && GetGlobalFlag(322);
                case 33:
                case 34:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 11";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
                case 55:
                case 56:
                    originalScript = "game.story_state == 5";
                    return StoryState == 5;
                case 57:
                case 58:
                    originalScript = "game.story_state <= 4";
                    return StoryState <= 4;
                case 59:
                case 60:
                    originalScript = "game.story_state >= 6";
                    return StoryState >= 6;
                case 133:
                case 134:
                    originalScript = "game.global_flags[145] == 0";
                    return !GetGlobalFlag(145);
                case 276:
                case 277:
                case 285:
                case 286:
                case 295:
                case 296:
                case 305:
                case 306:
                case 324:
                case 325:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL and not pc.follower_atmax()";
                    return npc.GetLeader() == null && !pc.HasMaxFollowers();
                case 278:
                case 287:
                case 297:
                case 307:
                case 326:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 279:
                case 288:
                case 298:
                case 308:
                case 327:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 311:
                    originalScript = "pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 312:
                case 313:
                    originalScript = "pc.stat_level_get(stat_level_paladin) == 1";
                    return pc.GetStat(Stat.level_paladin) == 1;
                case 331:
                case 332:
                case 471:
                case 472:
                case 491:
                case 492:
                    originalScript = "pc.stat_level_get(stat_race) == race_human";
                    return pc.GetRace() == RaceId.human;
                case 333:
                case 334:
                case 473:
                case 474:
                case 493:
                case 494:
                    originalScript = "pc.stat_level_get(stat_race) == race_dwarf";
                    return pc.GetRace() == RaceId.derro;
                case 335:
                case 336:
                case 475:
                case 476:
                case 495:
                case 496:
                    originalScript = "pc.stat_level_get(stat_race) == race_elf or pc.stat_level_get(stat_race) == race_halfelf";
                    return pc.GetRace() == RaceId.aquatic_elf || pc.GetRace() == RaceId.halfelf;
                case 337:
                case 338:
                case 477:
                case 478:
                case 497:
                case 498:
                    originalScript = "pc.stat_level_get(stat_race) == race_orc or pc.stat_level_get(stat_race) == race_halforc";
                    // TODO: race_orc does not exist???
                    return pc.GetRace() == RaceId.half_orc;
                case 339:
                case 340:
                case 479:
                case 499:
                case 500:
                    originalScript = "pc.stat_level_get(stat_race) == race_gnome";
                    return pc.GetRace() == RaceId.svirfneblin;
                case 341:
                case 342:
                case 501:
                case 502:
                    originalScript = "pc.stat_level_get(stat_race) == race_halfling";
                    return pc.GetRace() == RaceId.tallfellow;
                case 483:
                case 484:
                    originalScript = "game.global_flags[182] == 1";
                    return GetGlobalFlag(182);
                case 485:
                case 486:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8034 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8034));
                case 503:
                case 504:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 9";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 9;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 10:
                    originalScript = "game.global_vars[136] = 1";
                    SetGlobalVar(136, 1);
                    break;
                case 21:
                case 51:
                case 52:
                case 71:
                case 81:
                case 82:
                case 91:
                case 92:
                case 101:
                case 111:
                case 135:
                case 136:
                case 141:
                case 142:
                case 153:
                case 154:
                case 171:
                case 172:
                case 181:
                case 191:
                case 192:
                case 351:
                case 361:
                case 362:
                case 371:
                case 381:
                case 391:
                case 401:
                case 402:
                case 433:
                case 434:
                case 461:
                case 462:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 211:
                case 212:
                    originalScript = "pc.follower_remove( npc ); npc.attack( pc )";
                    pc.RemoveFollower(npc);
                    npc.Attack(pc);
                    ;
                    break;
                case 311:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                default:
                    originalScript = null;
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 13:
                case 14:
                case 343:
                case 344:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                case 33:
                case 34:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                    return true;
                case 503:
                case 504:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 9);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
