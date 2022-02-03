
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(287)]
    public class RonaldDialog : Ronald, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                    originalScript = "pc.stat_level_get( stat_deity ) == 16";
                    return pc.GetStat(Stat.deity) == 16;
                case 12:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                case 13:
                    originalScript = "(pc.follower_atmax() == 0) and game.global_vars[693] == 0 and not anyone( pc.group_list(), \"has_follower\", 8002 )";
                    return (!pc.HasMaxFollowers()) && GetGlobalVar(693) == 0 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8002));
                case 14:
                    originalScript = "(pc.follower_atmax() == 0) and anyone( pc.group_list(), \"has_follower\", 8002 )";
                    return (!pc.HasMaxFollowers()) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8002));
                case 15:
                    originalScript = "(pc.follower_atmax() == 0) and game.global_vars[693] == 3 and not anyone( pc.group_list(), \"has_follower\", 8002 )";
                    return (!pc.HasMaxFollowers()) && GetGlobalVar(693) == 3 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8002));
                case 16:
                    originalScript = "(pc.follower_atmax() == 0) and game.global_vars[693] == 2 and not anyone( pc.group_list(), \"has_follower\", 8002 )";
                    return (!pc.HasMaxFollowers()) && GetGlobalVar(693) == 2 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8002));
                case 17:
                    originalScript = "(pc.follower_atmax() == 0) and game.global_vars[693] == 1 and not anyone( pc.group_list(), \"has_follower\", 8002 )";
                    return (!pc.HasMaxFollowers()) && GetGlobalVar(693) == 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8002));
                case 54:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 55:
                    originalScript = "pc.stat_level_get(stat_race) == race_halforc and pc.stat_level_get( stat_deity ) == 16";
                    return pc.GetRace() == RaceId.half_orc && pc.GetStat(Stat.deity) == 16;
                case 61:
                case 104:
                case 325:
                case 336:
                case 911:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 62:
                    originalScript = "pc.money_get() < 10000";
                    return pc.GetMoney() < 10000;
                case 101:
                    originalScript = "pc.money_get() >= 10000 and game.global_vars[692] == 5";
                    return pc.GetMoney() >= 10000 && GetGlobalVar(692) == 5;
                case 106:
                    originalScript = "pc.money_get() >= 5000 and game.global_vars[692] == 6";
                    return pc.GetMoney() >= 5000 && GetGlobalVar(692) == 6;
                case 124:
                    originalScript = "game.global_vars[692] == 4";
                    return GetGlobalVar(692) == 4;
                case 171:
                    originalScript = "game.global_vars[692] == 1 or game.global_vars[692] == 2";
                    return GetGlobalVar(692) == 1 || GetGlobalVar(692) == 2;
                case 172:
                    originalScript = "game.global_vars[692] != 1 and game.global_vars[692] != 2";
                    return GetGlobalVar(692) != 1 && GetGlobalVar(692) != 2;
                case 211:
                case 212:
                case 364:
                case 367:
                    originalScript = "(pc.follower_atmax() == 0)";
                    return (!pc.HasMaxFollowers());
                case 261:
                case 262:
                    originalScript = "(pc.follower_atmax() == 0) and game.global_vars[693] == 4";
                    return (!pc.HasMaxFollowers()) && GetGlobalVar(693) == 4;
                case 263:
                case 264:
                    originalScript = "(pc.follower_atmax() == 0) and game.global_vars[693] != 4";
                    return (!pc.HasMaxFollowers()) && GetGlobalVar(693) != 4;
                case 265:
                case 266:
                case 942:
                case 943:
                case 944:
                    originalScript = "game.global_flags[833] == 1";
                    return GetGlobalFlag(833);
                case 269:
                    originalScript = "game.global_vars[693] == 4";
                    return GetGlobalVar(693) == 4;
                case 281:
                    originalScript = "game.global_vars[693] == 3";
                    return GetGlobalVar(693) == 3;
                case 282:
                    originalScript = "game.global_vars[693] == 1";
                    return GetGlobalVar(693) == 1;
                case 283:
                    originalScript = "game.global_vars[693] == 2";
                    return GetGlobalVar(693) == 2;
                case 284:
                    originalScript = "game.global_vars[693] == 0";
                    return GetGlobalVar(693) == 0;
                case 301:
                    originalScript = "game.global_vars[693] == 3 and (pc.follower_atmax() == 0)";
                    return GetGlobalVar(693) == 3 && (!pc.HasMaxFollowers());
                case 302:
                    originalScript = "game.global_vars[693] == 1 and (pc.follower_atmax() == 0)";
                    return GetGlobalVar(693) == 1 && (!pc.HasMaxFollowers());
                case 303:
                    originalScript = "game.global_vars[693] == 2 and (pc.follower_atmax() == 0)";
                    return GetGlobalVar(693) == 2 && (!pc.HasMaxFollowers());
                case 304:
                    originalScript = "game.global_vars[693] == 0 and (pc.follower_atmax() == 0)";
                    return GetGlobalVar(693) == 0 && (!pc.HasMaxFollowers());
                case 321:
                case 322:
                case 881:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 323:
                case 324:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) <= 5";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 5;
                case 341:
                    originalScript = "pc.money_get() >= 5000";
                    return pc.GetMoney() >= 5000;
                case 392:
                    originalScript = "game.global_flags[21] != 1";
                    return !GetGlobalFlag(21);
                case 393:
                    originalScript = "game.global_flags[21] == 1";
                    return GetGlobalFlag(21);
                case 402:
                case 1058:
                    originalScript = "npc.area == 1 or npc.area == 3";
                    return npc.GetArea() == 1 || npc.GetArea() == 3;
                case 403:
                case 1059:
                    originalScript = "npc.area != 1 and npc.area != 3";
                    return npc.GetArea() != 1 && npc.GetArea() != 3;
                case 426:
                case 427:
                case 437:
                case 438:
                case 454:
                case 455:
                case 1095:
                case 1099:
                case 1133:
                    originalScript = "pc.stat_level_get(stat_level_bard) >= 1";
                    return pc.GetStat(Stat.level_bard) >= 1;
                case 501:
                    originalScript = "(npc.item_find(6070) != OBJ_HANDLE_NULL) and (npc.item_find(6056) != OBJ_HANDLE_NULL)";
                    return (npc.FindItemByName(6070) != null) && (npc.FindItemByName(6056) != null);
                case 502:
                    originalScript = "npc.item_find(6064) != OBJ_HANDLE_NULL";
                    return npc.FindItemByName(6064) != null;
                case 505:
                    originalScript = "(npc.item_find(6056) == OBJ_HANDLE_NULL) and (npc.item_find(6070) != OBJ_HANDLE_NULL)";
                    return (npc.FindItemByName(6056) == null) && (npc.FindItemByName(6070) != null);
                case 512:
                case 514:
                    originalScript = "npc.stat_level_get(stat_level_cleric) < 2";
                    return npc.GetStat(Stat.level_cleric) < 2;
                case 513:
                case 515:
                    originalScript = "npc.stat_level_get(stat_level_cleric) >= 2";
                    return npc.GetStat(Stat.level_cleric) >= 2;
                case 531:
                    originalScript = "pc.money_get() >= 6000 and pc.skill_level_get(npc, skill_appraise) >= 10";
                    throw new NotSupportedException("Conversion failed.");
                case 532:
                    originalScript = "pc.money_get() >= 1500 and (npc.item_find(6056) != OBJ_HANDLE_NULL)";
                    return pc.GetMoney() >= 1500 && (npc.FindItemByName(6056) != null);
                case 533:
                    originalScript = "pc.money_get() >= 8000";
                    return pc.GetMoney() >= 8000;
                case 561:
                    originalScript = "pc.money_get() >= 6500";
                    return pc.GetMoney() >= 6500;
                case 562:
                    originalScript = "pc.money_get() >= 4500 and pc.skill_level_get(npc, skill_appraise) >= 10";
                    throw new NotSupportedException("Conversion failed.");
                case 711:
                case 714:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 712:
                case 713:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) < 7";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) < 7;
                case 719:
                case 975:
                case 1004:
                case 1063:
                case 2002:
                case 2034:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 721:
                case 722:
                case 2001:
                case 2033:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 751:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                case 752:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) < 6";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) < 6;
                case 756:
                case 757:
                    originalScript = "game.global_flags[131] == 1";
                    return GetGlobalFlag(131);
                case 807:
                    originalScript = "pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 882:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) < 6";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) < 6;
                case 941:
                    originalScript = "game.global_flags[833] != 1";
                    return !GetGlobalFlag(833);
                case 974:
                case 1003:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 2006:
                case 2061:
                    originalScript = "npc.map == 5012 or npc.map == 5013";
                    return npc.GetMap() == 5012 || npc.GetMap() == 5013;
                case 2007:
                    originalScript = "npc.map != 5012 and npc.map != 5013";
                    return npc.GetMap() != 5012 && npc.GetMap() != 5013;
                case 2042:
                    originalScript = "game.global_vars[692] == 2 or game.global_vars[692] == 3";
                    return GetGlobalVar(692) == 2 || GetGlobalVar(692) == 3;
                case 2043:
                    originalScript = "game.global_vars[692] != 2 and game.global_vars[692] != 3";
                    return GetGlobalVar(692) != 2 && GetGlobalVar(692) != 3;
                case 2064:
                    originalScript = "npc.map == 5011";
                    return npc.GetMap() == 5011;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_vars[115] = 1";
                    SetGlobalVar(115, 1);
                    break;
                case 25:
                case 330:
                case 350:
                case 915:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 31:
                    originalScript = "game.global_vars[692] = 4; argue_lareth(npc,pc,630)";
                    SetGlobalVar(692, 4);
                    argue_lareth(npc, pc, 630);
                    ;
                    break;
                case 35:
                    originalScript = "game.global_vars[692] = 1";
                    SetGlobalVar(692, 1);
                    break;
                case 40:
                    originalScript = "game.global_vars[692] = 2";
                    SetGlobalVar(692, 2);
                    break;
                case 56:
                case 65:
                    originalScript = "game.global_vars[692] = 3";
                    SetGlobalVar(692, 3);
                    break;
                case 70:
                    originalScript = "pc.money_adj(-10000); pc.follower_add( npc )";
                    pc.AdjustMoney(-10000);
                    pc.AddFollower(npc);
                    ;
                    break;
                case 75:
                case 337:
                    originalScript = "game.global_vars[692] = 5";
                    SetGlobalVar(692, 5);
                    break;
                case 101:
                case 325:
                case 336:
                case 911:
                    originalScript = "pc.money_adj(-10000)";
                    pc.AdjustMoney(-10000);
                    break;
                case 106:
                case 341:
                    originalScript = "pc.money_adj(-5000)";
                    pc.AdjustMoney(-5000);
                    break;
                case 190:
                    originalScript = "pc.follower_remove( npc ); game.global_vars[692] = 3";
                    pc.RemoveFollower(npc);
                    SetGlobalVar(692, 3);
                    ;
                    break;
                case 195:
                    originalScript = "pc.follower_remove( npc ); game.global_vars[692] = 0";
                    pc.RemoveFollower(npc);
                    SetGlobalVar(692, 0);
                    ;
                    break;
                case 271:
                case 272:
                    originalScript = "game.global_vars[692] = 4; argue_lareth(npc,pc,200)";
                    SetGlobalVar(692, 4);
                    argue_lareth(npc, pc, 200);
                    ;
                    break;
                case 300:
                    originalScript = "game.global_vars[692] == 0";
                    SetGlobalVar(692, 0);
                    break;
                case 316:
                case 551:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 342:
                    originalScript = "game.global_vars[692] = 6";
                    SetGlobalVar(692, 6);
                    break;
                case 355:
                case 360:
                    originalScript = "game.global_vars[692] = 0";
                    SetGlobalVar(692, 0);
                    break;
                case 371:
                case 372:
                    originalScript = "game.global_vars[692] = 4; argue_lareth(npc,pc,360)";
                    SetGlobalVar(692, 4);
                    argue_lareth(npc, pc, 360);
                    ;
                    break;
                case 380:
                case 1015:
                    originalScript = "pc.follower_remove( npc ); game.global_vars[692] = 1";
                    pc.RemoveFollower(npc);
                    SetGlobalVar(692, 1);
                    ;
                    break;
                case 381:
                    originalScript = "run_off( npc, pc ); argue_lareth(npc,pc,180)";
                    run_off(npc, pc);
                    argue_lareth(npc, pc, 180);
                    ;
                    break;
                case 520:
                    originalScript = "pc.follower_remove( npc ); pc.follower_add( npc )";
                    pc.RemoveFollower(npc);
                    pc.AddFollower(npc);
                    ;
                    break;
                case 532:
                    originalScript = "pc.money_adj(-1500); equip_leather(npc,pc)";
                    pc.AdjustMoney(-1500);
                    equip_leather(npc, pc);
                    ;
                    break;
                case 533:
                    originalScript = "pc.money_adj(-8000); equip_all(npc,pc)";
                    pc.AdjustMoney(-8000);
                    equip_all(npc, pc);
                    ;
                    break;
                case 537:
                    originalScript = "pc.money_adj(-6000); equip_all(npc,pc)";
                    pc.AdjustMoney(-6000);
                    equip_all(npc, pc);
                    ;
                    break;
                case 550:
                    originalScript = "game.global_vars[692] = 1; pc.follower_remove( npc )";
                    SetGlobalVar(692, 1);
                    pc.RemoveFollower(npc);
                    ;
                    break;
                case 561:
                    originalScript = "pc.money_adj(-6500); equip_rest(npc,pc)";
                    pc.AdjustMoney(-6500);
                    equip_rest(npc, pc);
                    ;
                    break;
                case 562:
                    originalScript = "pc.money_adj(-4500); equip_rest(npc,pc)";
                    pc.AdjustMoney(-4500);
                    equip_rest(npc, pc);
                    ;
                    break;
                case 611:
                case 612:
                    originalScript = "war(npc,pc)";
                    war(npc, pc);
                    break;
                case 621:
                case 622:
                    originalScript = "switch_to_cuthbert(npc,pc,30)";
                    switch_to_cuthbert(npc, pc, 30);
                    break;
                case 751:
                    originalScript = "argue_tuelk(npc,pc,310)";
                    argue_tuelk(npc, pc, 310);
                    break;
                case 752:
                    originalScript = "argue_tuelk(npc,pc,300)";
                    argue_tuelk(npc, pc, 300);
                    break;
                case 778:
                case 781:
                case 791:
                case 811:
                case 813:
                case 815:
                case 816:
                case 817:
                case 818:
                case 821:
                case 823:
                case 825:
                case 827:
                case 828:
                    originalScript = "argue_tuelk(npc,pc,40)";
                    argue_tuelk(npc, pc, 40);
                    break;
                case 855:
                    originalScript = "game.global_vars[692] = 7";
                    SetGlobalVar(692, 7);
                    break;
                case 866:
                    originalScript = "pc.follower_remove( npc ); run_off(npc,pc); argue_tuelk(npc,pc,160)";
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    argue_tuelk(npc, pc, 160);
                    ;
                    break;
                case 2050:
                case 2052:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
                    break;
                case 2051:
                case 2053:
                    originalScript = "game.global_vars[692] = 9";
                    SetGlobalVar(692, 9);
                    break;
                case 2054:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 2055:
                    originalScript = "game.global_vars[692] = 8; pc.follower_remove( npc )";
                    SetGlobalVar(692, 8);
                    pc.RemoveFollower(npc);
                    ;
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
                case 54:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                case 321:
                case 322:
                case 881:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                case 711:
                case 714:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                case 751:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
