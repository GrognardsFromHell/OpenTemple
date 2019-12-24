
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
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
    [DialogScript(488)]
    public class AriakasDialog : Ariakas, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                    originalScript = "game.global_flags[282] == 0 and game.global_flags[284] == 0 and not anyone( pc.group_list(), \"has_follower\", 8054 ) and not anyone( pc.group_list(), \"has_follower\", 8071 )";
                    return !GetGlobalFlag(282) && !GetGlobalFlag(284) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071));
                case 22:
                    originalScript = "game.global_flags[282] == 1 and game.global_flags[284] == 0 and not anyone( pc.group_list(), \"has_follower\", 8071 )";
                    return GetGlobalFlag(282) && !GetGlobalFlag(284) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071));
                case 23:
                    originalScript = "game.global_flags[282] == 0 and game.global_flags[284] == 1 and not anyone( pc.group_list(), \"has_follower\", 8054 )";
                    return !GetGlobalFlag(282) && GetGlobalFlag(284) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054));
                case 24:
                    originalScript = "game.global_flags[282] == 1 and game.global_flags[284] == 1";
                    return GetGlobalFlag(282) && GetGlobalFlag(284);
                case 25:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8054 ) and game.global_flags[284] == 0";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && !GetGlobalFlag(284);
                case 26:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8071 ) and game.global_flags[282] == 0";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071)) && !GetGlobalFlag(282);
                case 27:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8054 ) and game.global_flags[284] == 1";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && GetGlobalFlag(284);
                case 28:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8071 ) and game.global_flags[282] == 1";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071)) && GetGlobalFlag(282);
                case 29:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8054 ) and anyone( pc.group_list(), \"has_follower\", 8071 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071));
                case 71:
                    originalScript = "game.global_flags[282] == 0 and not anyone( pc.group_list(), \"has_follower\", 8054 )";
                    return !GetGlobalFlag(282) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054));
                case 72:
                    originalScript = "game.global_flags[284] == 0 and not anyone( pc.group_list(), \"has_follower\", 8071 )";
                    return !GetGlobalFlag(284) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071));
                case 73:
                case 231:
                    originalScript = "game.global_flags[299] == 0";
                    return !GetGlobalFlag(299);
                case 74:
                    originalScript = "game.global_flags[337] == 0";
                    return !GetGlobalFlag(337);
                case 81:
                case 91:
                case 101:
                case 111:
                case 1131:
                    originalScript = "game.global_flags[282] == 0 and not npc_get(npc,1) and not anyone( pc.group_list(), \"has_follower\", 8054 )";
                    return !GetGlobalFlag(282) && !ScriptDaemon.npc_get(npc, 1) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054));
                case 82:
                case 92:
                case 102:
                case 112:
                case 1132:
                    originalScript = "game.global_flags[284] == 0 and not npc_get(npc,2) and not anyone( pc.group_list(), \"has_follower\", 8071 )";
                    return !GetGlobalFlag(284) && !ScriptDaemon.npc_get(npc, 2) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071));
                case 83:
                case 93:
                case 103:
                case 113:
                case 1133:
                    originalScript = "game.global_flags[299] == 0 and not npc_get(npc,5)";
                    return !GetGlobalFlag(299) && !ScriptDaemon.npc_get(npc, 5);
                case 84:
                case 94:
                case 104:
                case 114:
                case 1134:
                    originalScript = "game.global_flags[337] == 0 and not npc_get(npc,4)";
                    return !GetGlobalFlag(337) && !ScriptDaemon.npc_get(npc, 4);
                case 211:
                case 373:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 212:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 232:
                    originalScript = "game.global_flags[299] == 1";
                    return GetGlobalFlag(299);
                case 241:
                case 331:
                    originalScript = "game.global_flags[337] == 0 and game.global_flags[934] == 0 and not anyone( pc.group_list(), \"has_follower\", 8000 )";
                    return !GetGlobalFlag(337) && !GetGlobalFlag(934) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 242:
                case 332:
                    originalScript = "game.global_flags[337] == 0 and (game.global_flags[934] == 1 or anyone( pc.group_list(), \"has_follower\", 8000 ))";
                    return !GetGlobalFlag(337) && (GetGlobalFlag(934) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)));
                case 243:
                case 333:
                    originalScript = "game.global_flags[337] == 1 and game.global_flags[934] == 0 and not anyone( pc.group_list(), \"has_follower\", 8000 )";
                    return GetGlobalFlag(337) && !GetGlobalFlag(934) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 244:
                case 334:
                    originalScript = "game.global_flags[337] == 1 and (game.global_flags[934] == 1 or anyone( pc.group_list(), \"has_follower\", 8000 ))";
                    return GetGlobalFlag(337) && (GetGlobalFlag(934) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)));
                case 281:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 2203 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 282:
                    originalScript = "not anyone( pc.group_list(), \"has_item\", 2203 )";
                    return !pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 371:
                case 621:
                    originalScript = "game.global_flags[511] == 0";
                    return !GetGlobalFlag(511);
                case 372:
                case 622:
                    originalScript = "game.global_flags[511] == 1";
                    return GetGlobalFlag(511);
                case 381:
                case 391:
                case 631:
                case 641:
                    originalScript = "game.global_flags[512] == 0";
                    return !GetGlobalFlag(512);
                case 382:
                case 392:
                case 632:
                case 642:
                    originalScript = "game.global_flags[512] == 1";
                    return GetGlobalFlag(512);
                case 401:
                case 411:
                case 651:
                case 661:
                    originalScript = "game.global_flags[513] == 0";
                    return !GetGlobalFlag(513);
                case 402:
                case 412:
                case 652:
                case 662:
                    originalScript = "game.global_flags[513] == 1";
                    return GetGlobalFlag(513);
                case 421:
                case 431:
                case 671:
                case 681:
                    originalScript = "game.global_flags[514] == 0";
                    return !GetGlobalFlag(514);
                case 422:
                case 432:
                case 672:
                case 682:
                    originalScript = "game.global_flags[514] == 1";
                    return GetGlobalFlag(514);
                case 441:
                case 451:
                case 691:
                case 701:
                    originalScript = "game.global_flags[515] == 0";
                    return !GetGlobalFlag(515);
                case 442:
                case 452:
                case 692:
                case 702:
                    originalScript = "game.global_flags[515] == 1";
                    return GetGlobalFlag(515);
                case 461:
                case 471:
                case 711:
                case 721:
                    originalScript = "game.global_flags[516] == 0";
                    return !GetGlobalFlag(516);
                case 462:
                case 472:
                case 712:
                case 722:
                    originalScript = "game.global_flags[516] == 1";
                    return GetGlobalFlag(516);
                case 481:
                case 491:
                case 731:
                case 741:
                    originalScript = "game.global_flags[517] == 0";
                    return !GetGlobalFlag(517);
                case 482:
                case 492:
                case 732:
                case 742:
                    originalScript = "game.global_flags[517] == 1";
                    return GetGlobalFlag(517);
                case 501:
                case 511:
                case 751:
                case 761:
                    originalScript = "game.global_flags[518] == 0";
                    return !GetGlobalFlag(518);
                case 502:
                case 512:
                case 752:
                case 762:
                    originalScript = "game.global_flags[518] == 1";
                    return GetGlobalFlag(518);
                case 521:
                case 531:
                case 771:
                case 781:
                    originalScript = "game.global_flags[519] == 0";
                    return !GetGlobalFlag(519);
                case 522:
                case 532:
                case 772:
                case 782:
                    originalScript = "game.global_flags[519] == 1";
                    return GetGlobalFlag(519);
                case 541:
                case 551:
                case 791:
                case 801:
                    originalScript = "game.global_flags[520] == 0";
                    return !GetGlobalFlag(520);
                case 542:
                case 552:
                case 792:
                case 802:
                    originalScript = "game.global_flags[520] == 1";
                    return GetGlobalFlag(520);
                case 561:
                case 571:
                case 811:
                case 821:
                    originalScript = "game.global_flags[521] == 0";
                    return !GetGlobalFlag(521);
                case 562:
                case 572:
                case 812:
                case 822:
                    originalScript = "game.global_flags[521] == 1";
                    return GetGlobalFlag(521);
                case 581:
                case 591:
                case 831:
                case 841:
                    originalScript = "game.global_flags[522] == 0";
                    return !GetGlobalFlag(522);
                case 582:
                case 592:
                case 832:
                case 842:
                    originalScript = "game.global_flags[522] == 1";
                    return GetGlobalFlag(522);
                case 1135:
                case 1202:
                case 1212:
                case 1222:
                case 1232:
                    originalScript = "pc.stat_level_get( stat_deity ) == 9 and not npc_get(npc,6)";
                    return pc.GetStat(Stat.deity) == 9 && !ScriptDaemon.npc_get(npc, 6);
                case 1151:
                case 1181:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 4;
                case 1192:
                    originalScript = "pc.stat_level_get( stat_deity ) == 9";
                    return pc.GetStat(Stat.deity) == 9;
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
                    originalScript = "game.global_vars[501] = 2; game.quests[107].state = qs_completed";
                    SetGlobalVar(501, 2);
                    SetQuestState(107, QuestState.Completed);
                    ;
                    break;
                case 71:
                case 81:
                case 91:
                case 101:
                case 111:
                case 1131:
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 72:
                case 82:
                case 92:
                case 102:
                case 112:
                case 1132:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 73:
                case 83:
                case 93:
                case 103:
                case 113:
                case 1133:
                    originalScript = "npc_set(npc,5)";
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 74:
                case 84:
                case 94:
                case 104:
                case 114:
                case 1134:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 130:
                    originalScript = "check_back(npc,pc)";
                    check_back(npc, pc);
                    break;
                case 161:
                    originalScript = "switch_to_old_man( npc, pc, 1)";
                    switch_to_old_man(npc, pc, 1);
                    break;
                case 170:
                case 320:
                    originalScript = "game.global_vars[501] = 4";
                    SetGlobalVar(501, 4);
                    break;
                case 171:
                case 321:
                    originalScript = "switch_to_old_man( npc, pc, 50); start_alarm(npc,pc)";
                    switch_to_old_man(npc, pc, 50);
                    start_alarm(npc, pc);
                    ;
                    break;
                case 181:
                    originalScript = "switch_to_old_man( npc, pc, 60)";
                    switch_to_old_man(npc, pc, 60);
                    break;
                case 200:
                case 1120:
                    originalScript = "set_inside_limiter(npc,pc)";
                    set_inside_limiter(npc, pc);
                    break;
                case 271:
                    originalScript = "hextor_movie_setup(npc,pc)";
                    hextor_movie_setup(npc, pc);
                    break;
                case 280:
                    originalScript = "game.quests[97].state = qs_mentioned";
                    SetQuestState(97, QuestState.Mentioned);
                    break;
                case 311:
                    originalScript = "switch_to_old_man( npc, pc, 70)";
                    switch_to_old_man(npc, pc, 70);
                    break;
                case 871:
                case 1136:
                case 1193:
                case 1203:
                case 1213:
                case 1223:
                case 1233:
                    originalScript = "game.quests[97].state = qs_accepted";
                    SetQuestState(97, QuestState.Accepted);
                    break;
                case 881:
                    originalScript = "game.global_vars[510] = 1";
                    SetGlobalVar(510, 1);
                    break;
                case 891:
                    originalScript = "game.quests[97].state = qs_accepted; game.global_vars[510] = 0";
                    SetQuestState(97, QuestState.Accepted);
                    SetGlobalVar(510, 0);
                    ;
                    break;
                case 910:
                case 920:
                    originalScript = "game.global_vars[501] = 8";
                    SetGlobalVar(501, 8);
                    break;
                case 911:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
                    break;
                case 930:
                    originalScript = "very_bad_things(npc,pc)";
                    very_bad_things(npc, pc);
                    break;
                case 932:
                case 941:
                    originalScript = "npc.runoff(npc.location-3)";
                    npc.RunOff();
                    break;
                case 951:
                    originalScript = "create_item_in_inventory(4252,pc)";
                    Utilities.create_item_in_inventory(4252, pc);
                    break;
                case 952:
                    originalScript = "create_item_in_inventory(4253,pc)";
                    Utilities.create_item_in_inventory(4253, pc);
                    break;
                case 953:
                    originalScript = "create_item_in_inventory(4254,pc)";
                    Utilities.create_item_in_inventory(4254, pc);
                    break;
                case 954:
                    originalScript = "create_item_in_inventory(4255,pc)";
                    Utilities.create_item_in_inventory(4255, pc);
                    break;
                case 962:
                    originalScript = "create_item_in_inventory(4256,pc)";
                    Utilities.create_item_in_inventory(4256, pc);
                    break;
                case 963:
                    originalScript = "create_item_in_inventory(4257,pc)";
                    Utilities.create_item_in_inventory(4257, pc);
                    break;
                case 964:
                    originalScript = "create_item_in_inventory(4258,pc)";
                    Utilities.create_item_in_inventory(4258, pc);
                    break;
                case 965:
                    originalScript = "create_item_in_inventory(4259,pc)";
                    Utilities.create_item_in_inventory(4259, pc);
                    break;
                case 970:
                    originalScript = "game.global_flags[505] = 1";
                    SetGlobalFlag(505, true);
                    break;
                case 980:
                    originalScript = "game.quests[101].state = qs_mentioned";
                    SetQuestState(101, QuestState.Mentioned);
                    break;
                case 981:
                    originalScript = "pc.money_adj(100000)";
                    pc.AdjustMoney(100000);
                    break;
                case 982:
                    originalScript = "game.quests[101].state = qs_accepted";
                    SetQuestState(101, QuestState.Accepted);
                    break;
                case 991:
                    originalScript = "game.global_vars[506] = 2";
                    SetGlobalVar(506, 2);
                    break;
                case 1001:
                    originalScript = "game.global_vars[506] = 1";
                    SetGlobalVar(506, 1);
                    break;
                case 1121:
                case 1141:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 1135:
                case 1192:
                case 1202:
                case 1212:
                case 1222:
                case 1232:
                    originalScript = "npc_set(npc,6)";
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 1153:
                case 1161:
                case 1171:
                case 1183:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
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
                case 1151:
                case 1181:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
