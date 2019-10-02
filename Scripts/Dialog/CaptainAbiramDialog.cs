
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
    [DialogScript(321)]
    public class CaptainAbiramDialog : CaptainAbiram, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 75:
                case 121:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) or (game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) or (game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) or (game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) || (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) || (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) || (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned);
                case 11:
                    Trace.Assert(originalScript == "not is_daytime() == 0");
                    return !(!Utilities.is_daytime());
                case 12:
                    Trace.Assert(originalScript == "not is_daytime() == 1");
                    return !(Utilities.is_daytime());
                case 22:
                case 53:
                case 62:
                case 72:
                    Trace.Assert(originalScript == "game.global_flags[980] == 0");
                    return !GetGlobalFlag(980);
                case 73:
                    Trace.Assert(originalScript == "game.global_flags[980] == 1");
                    return GetGlobalFlag(980);
                case 74:
                case 94:
                case 104:
                case 114:
                case 424:
                    Trace.Assert(originalScript == "game.global_flags[992] == 0 and npc_get(npc,7) and game.global_vars[979] == 0");
                    return !GetGlobalFlag(992) && ScriptDaemon.npc_get(npc, 7) && GetGlobalVar(979) == 0;
                case 76:
                case 122:
                    Trace.Assert(originalScript == "game.global_vars[945] == 30 and not npc_get(npc,19)");
                    return GetGlobalVar(945) == 30 && !ScriptDaemon.npc_get(npc, 19);
                case 77:
                case 123:
                    Trace.Assert(originalScript == "game.quests[109].state == qs_unknown and game.quests[110].state == qs_mentioned");
                    return GetQuestState(109) == QuestState.Unknown && GetQuestState(110) == QuestState.Mentioned;
                case 78:
                case 124:
                case 681:
                    Trace.Assert(originalScript == "game.global_vars[535] == 1");
                    return GetGlobalVar(535) == 1;
                case 79:
                case 125:
                case 682:
                    Trace.Assert(originalScript == "game.global_vars[535] == 2");
                    return GetGlobalVar(535) == 2;
                case 80:
                case 126:
                    Trace.Assert(originalScript == "game.global_vars[535] == 2 and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalVar(535) == 2 && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 81:
                case 127:
                    Trace.Assert(originalScript == "game.global_vars[535] == 3 and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalVar(535) == 3 && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 82:
                case 128:
                    Trace.Assert(originalScript == "not npc_get(npc,29) and game.quests[109].state == qs_mentioned and game.global_vars[535] == 2 and (game.global_vars[543] >= 3 or game.global_vars[544] >= 3 or game.global_vars[545] >= 3)");
                    return !ScriptDaemon.npc_get(npc, 29) && GetQuestState(109) == QuestState.Mentioned && GetGlobalVar(535) == 2 && (GetGlobalVar(543) >= 3 || GetGlobalVar(544) >= 3 || GetGlobalVar(545) >= 3);
                case 83:
                case 129:
                    Trace.Assert(originalScript == "not npc_get(npc,29) and game.quests[109].state == qs_accepted and game.global_vars[535] == 3 and (game.global_vars[543] >= 3 or game.global_vars[544] >= 3 or game.global_vars[545] >= 3)");
                    return !ScriptDaemon.npc_get(npc, 29) && GetQuestState(109) == QuestState.Accepted && GetGlobalVar(535) == 3 && (GetGlobalVar(543) >= 3 || GetGlobalVar(544) >= 3 || GetGlobalVar(545) >= 3);
                case 84:
                case 130:
                    Trace.Assert(originalScript == "game.global_vars[535] == 3 and (game.global_vars[543] <= 2 and game.global_vars[544] <= 2 and game.global_vars[545] <= 2)");
                    return GetGlobalVar(535) == 3 && (GetGlobalVar(543) <= 2 && GetGlobalVar(544) <= 2 && GetGlobalVar(545) <= 2);
                case 85:
                case 131:
                    Trace.Assert(originalScript == "game.quests[109].state == qs_accepted");
                    return GetQuestState(109) == QuestState.Accepted;
                case 91:
                case 112:
                case 422:
                case 1052:
                case 1282:
                    Trace.Assert(originalScript == "not npc_get(npc,5)");
                    return !ScriptDaemon.npc_get(npc, 5);
                case 92:
                case 102:
                case 423:
                case 1053:
                case 1283:
                    Trace.Assert(originalScript == "not npc_get(npc,6)");
                    return !ScriptDaemon.npc_get(npc, 6);
                case 93:
                case 103:
                case 113:
                case 1054:
                case 1284:
                    Trace.Assert(originalScript == "not npc_get(npc,20)");
                    return !ScriptDaemon.npc_get(npc, 20);
                case 101:
                case 111:
                case 421:
                case 1051:
                case 1281:
                    Trace.Assert(originalScript == "not npc_get(npc,7)");
                    return !ScriptDaemon.npc_get(npc, 7);
                case 132:
                    Trace.Assert(originalScript == "game.quests[78].state == qs_accepted");
                    return GetQuestState(78) == QuestState.Accepted;
                case 133:
                    Trace.Assert(originalScript == "game.quests[62].state == qs_accepted and (game.global_flags[560] == 0 or game.global_flags[561] == 0 or game.global_flags[562] == 0) and not npc_get(npc,30)");
                    return GetQuestState(62) == QuestState.Accepted && (!GetGlobalFlag(560) || !GetGlobalFlag(561) || !GetGlobalFlag(562)) && !ScriptDaemon.npc_get(npc, 30);
                case 161:
                    Trace.Assert(originalScript == "game.quests[77].state == qs_mentioned");
                    return GetQuestState(77) == QuestState.Mentioned;
                case 162:
                    Trace.Assert(originalScript == "(game.global_vars[704] == 1 or game.global_vars[704] == 2) and game.quests[77].state == qs_accepted");
                    return (GetGlobalVar(704) == 1 || GetGlobalVar(704) == 2) && GetQuestState(77) == QuestState.Accepted;
                case 163:
                    Trace.Assert(originalScript == "game.global_vars[704] == 3 and game.quests[77].state == qs_accepted");
                    return GetGlobalVar(704) == 3 && GetQuestState(77) == QuestState.Accepted;
                case 171:
                    Trace.Assert(originalScript == "is_daytime()");
                    return Utilities.is_daytime();
                case 172:
                    Trace.Assert(originalScript == "not is_daytime()");
                    return !Utilities.is_daytime();
                case 191:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and game.global_flags[946] == 0 and game.global_flags[863] == 0 and not npc_get(npc,1)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && !GetGlobalFlag(946) && !GetGlobalFlag(863) && !ScriptDaemon.npc_get(npc, 1);
                case 192:
                    Trace.Assert(originalScript == "(game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) and game.global_flags[973] == 0 and not npc_get(npc,2)");
                    return (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) && !GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 2);
                case 193:
                    Trace.Assert(originalScript == "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,3)");
                    return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 3);
                case 194:
                    Trace.Assert(originalScript == "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,4)");
                    return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 4);
                case 195:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and npc_get(npc,8) and not npc_get(npc,10)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && ScriptDaemon.npc_get(npc, 8) && !ScriptDaemon.npc_get(npc, 10);
                case 196:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and npc_get(npc,9) and not npc_get(npc,11)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && ScriptDaemon.npc_get(npc, 9) && !ScriptDaemon.npc_get(npc, 11);
                case 202:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 15");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 15;
                case 291:
                    Trace.Assert(originalScript == "not npc_get(npc,12)");
                    return !ScriptDaemon.npc_get(npc, 12);
                case 292:
                    Trace.Assert(originalScript == "not npc_get(npc,13)");
                    return !ScriptDaemon.npc_get(npc, 13);
                case 293:
                    Trace.Assert(originalScript == "not npc_get(npc,14)");
                    return !ScriptDaemon.npc_get(npc, 14);
                case 294:
                    Trace.Assert(originalScript == "not npc_get(npc,15)");
                    return !ScriptDaemon.npc_get(npc, 15);
                case 302:
                    Trace.Assert(originalScript == "not npc_get(npc,16)");
                    return !ScriptDaemon.npc_get(npc, 16);
                case 303:
                    Trace.Assert(originalScript == "not npc_get(npc,17)");
                    return !ScriptDaemon.npc_get(npc, 17);
                case 304:
                    Trace.Assert(originalScript == "not npc_get(npc,18)");
                    return !ScriptDaemon.npc_get(npc, 18);
                case 692:
                case 702:
                case 712:
                case 722:
                case 732:
                case 752:
                case 762:
                case 772:
                case 792:
                case 1132:
                    Trace.Assert(originalScript == "not npc_get(npc,21)");
                    return !ScriptDaemon.npc_get(npc, 21);
                case 693:
                case 703:
                case 713:
                case 723:
                case 733:
                case 753:
                case 763:
                case 773:
                case 793:
                case 1133:
                    Trace.Assert(originalScript == "not npc_get(npc,22)");
                    return !ScriptDaemon.npc_get(npc, 22);
                case 694:
                case 704:
                case 714:
                case 724:
                case 734:
                case 754:
                case 764:
                case 774:
                case 794:
                case 1134:
                    Trace.Assert(originalScript == "not npc_get(npc,23)");
                    return !ScriptDaemon.npc_get(npc, 23);
                case 695:
                case 705:
                case 715:
                case 725:
                case 735:
                case 755:
                case 765:
                case 775:
                case 795:
                case 1135:
                    Trace.Assert(originalScript == "game.global_flags[530] == 0 and not npc_get(npc,24)");
                    return !GetGlobalFlag(530) && !ScriptDaemon.npc_get(npc, 24);
                case 696:
                case 706:
                case 716:
                case 726:
                case 736:
                case 756:
                case 766:
                case 776:
                case 796:
                case 1136:
                    Trace.Assert(originalScript == "not npc_get(npc,25)");
                    return !ScriptDaemon.npc_get(npc, 25);
                case 697:
                case 707:
                case 717:
                case 727:
                case 737:
                case 757:
                case 767:
                case 777:
                case 797:
                case 1137:
                    Trace.Assert(originalScript == "not npc_get(npc,26)");
                    return !ScriptDaemon.npc_get(npc, 26);
                case 698:
                case 708:
                case 718:
                case 728:
                case 738:
                case 758:
                case 768:
                case 778:
                case 798:
                case 1138:
                    Trace.Assert(originalScript == "game.global_vars[550] == 0 and not npc_get(npc,27)");
                    return GetGlobalVar(550) == 0 && !ScriptDaemon.npc_get(npc, 27);
                case 699:
                case 709:
                case 719:
                case 729:
                case 739:
                case 759:
                case 769:
                case 779:
                case 799:
                case 1139:
                    Trace.Assert(originalScript == "not npc_get(npc,28)");
                    return !ScriptDaemon.npc_get(npc, 28);
                case 801:
                case 811:
                case 1061:
                    Trace.Assert(originalScript == "game.global_vars[542] == 2 and game.global_vars[543] >= 3");
                    return GetGlobalVar(542) == 2 && GetGlobalVar(543) >= 3;
                case 802:
                case 1062:
                    Trace.Assert(originalScript == "(game.global_vars[542] == 1 and game.global_vars[544] >=3) or (game.global_vars[542] == 3 and game.global_vars[545] >=3)");
                    return (GetGlobalVar(542) == 1 && GetGlobalVar(544) >= 3) || (GetGlobalVar(542) == 3 && GetGlobalVar(545) >= 3);
                case 803:
                case 814:
                case 1063:
                case 1073:
                    Trace.Assert(originalScript == "game.global_vars[543] <= 2 and game.global_vars[544] <= 2 and game.global_vars[545] <= 2");
                    return GetGlobalVar(543) <= 2 && GetGlobalVar(544) <= 2 && GetGlobalVar(545) <= 2;
                case 812:
                case 1071:
                    Trace.Assert(originalScript == "game.global_vars[542] == 3 and game.global_vars[545] >= 3");
                    return GetGlobalVar(542) == 3 && GetGlobalVar(545) >= 3;
                case 813:
                case 1072:
                    Trace.Assert(originalScript == "game.global_vars[542] == 1 and game.global_vars[544] >= 3");
                    return GetGlobalVar(542) == 1 && GetGlobalVar(544) >= 3;
                case 821:
                    Trace.Assert(originalScript == "game.global_flags[543] == 1 and game.global_vars[555] == 2 and game.global_vars[552] >= 1 and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(543) && GetGlobalVar(555) == 2 && GetGlobalVar(552) >= 1 && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 822:
                    Trace.Assert(originalScript == "game.global_flags[543] == 1 and game.global_vars[555] == 2 and game.global_vars[552] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(543) && GetGlobalVar(555) == 2 && GetGlobalVar(552) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 823:
                    Trace.Assert(originalScript == "game.global_flags[543] == 1 and game.global_vars[555] == 2 and game.global_vars[552] == 0 and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(543) && GetGlobalVar(555) == 2 && GetGlobalVar(552) == 0 && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 824:
                    Trace.Assert(originalScript == "game.global_flags[543] == 1 and game.global_vars[555] == 2 and game.global_vars[552] == 0 and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(543) && GetGlobalVar(555) == 2 && GetGlobalVar(552) == 0 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 825:
                    Trace.Assert(originalScript == "game.global_flags[543] == 1 and game.global_vars[555] == 1 and game.global_vars[552] >= 1 and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(543) && GetGlobalVar(555) == 1 && GetGlobalVar(552) >= 1 && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 826:
                    Trace.Assert(originalScript == "game.global_flags[543] == 1 and game.global_vars[555] == 1 and game.global_vars[552] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(543) && GetGlobalVar(555) == 1 && GetGlobalVar(552) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 827:
                    Trace.Assert(originalScript == "game.global_flags[543] == 0 and game.global_vars[555] == 2 and game.global_vars[552] >= 1 and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return !GetGlobalFlag(543) && GetGlobalVar(555) == 2 && GetGlobalVar(552) >= 1 && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 828:
                    Trace.Assert(originalScript == "game.global_flags[543] == 0 and game.global_vars[555] == 2 and game.global_vars[552] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return !GetGlobalFlag(543) && GetGlobalVar(555) == 2 && GetGlobalVar(552) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 829:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 830:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 831:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 832:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 833:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] == 0 and anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) == 0 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 834:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] == 0 and anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) == 0 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 835:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] == 0 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) == 0 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 836:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 2 and game.global_vars[554] == 0 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) == 0 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 837:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 1 and game.global_vars[554] >= 1 and anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 1 && GetGlobalVar(554) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 838:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 1 and game.global_vars[554] >= 1 and anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 1 && GetGlobalVar(554) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 839:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 1 and game.global_vars[554] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 1 && GetGlobalVar(554) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 840:
                    Trace.Assert(originalScript == "game.global_flags[541] == 1 and game.global_vars[557] == 1 and game.global_vars[554] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(541) && GetGlobalVar(557) == 1 && GetGlobalVar(554) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 841:
                    Trace.Assert(originalScript == "game.global_flags[541] == 0 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return !GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 842:
                    Trace.Assert(originalScript == "game.global_flags[541] == 0 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return !GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 843:
                    Trace.Assert(originalScript == "game.global_flags[541] == 0 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return !GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 844:
                    Trace.Assert(originalScript == "game.global_flags[541] == 0 and game.global_vars[557] == 2 and game.global_vars[554] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8766 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return !GetGlobalFlag(541) && GetGlobalVar(557) == 2 && GetGlobalVar(554) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 845:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 846:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 847:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 848:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 849:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] == 0 and anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) == 0 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 850:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] == 0 and anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) == 0 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 851:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] == 0 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) == 0 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 852:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 2 and game.global_vars[553] == 0 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) == 0 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 853:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 1 and game.global_vars[553] >= 1 and anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 1 && GetGlobalVar(553) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 854:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 1 and game.global_vars[553] >= 1 and anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 1 && GetGlobalVar(553) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 855:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 1 and game.global_vars[553] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 1 && GetGlobalVar(553) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 856:
                    Trace.Assert(originalScript == "game.global_flags[542] == 1 and game.global_vars[556] == 1 and game.global_vars[553] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return GetGlobalFlag(542) && GetGlobalVar(556) == 1 && GetGlobalVar(553) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 857:
                    Trace.Assert(originalScript == "game.global_flags[542] == 0 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return !GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 858:
                    Trace.Assert(originalScript == "game.global_flags[542] == 0 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return !GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 859:
                    Trace.Assert(originalScript == "game.global_flags[542] == 0 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and (anyone( pc.group_list(), \"has_follower\", 8791 ) or anyone( pc.group_list(), \"has_follower\", 8792 ) or anyone( pc.group_list(), \"has_follower\", 8793 ) or anyone( pc.group_list(), \"has_follower\", 8794 ) or anyone( pc.group_list(), \"has_follower\", 8795 ) or anyone( pc.group_list(), \"has_follower\", 8796 ) or anyone( pc.group_list(), \"has_follower\", 8797 ) or anyone( pc.group_list(), \"has_follower\", 8798 ))");
                    return !GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) || pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798)));
                case 860:
                    Trace.Assert(originalScript == "game.global_flags[542] == 0 and game.global_vars[556] == 2 and game.global_vars[553] >= 1 and not anyone( pc.group_list(), \"has_follower\", 8767 ) and not anyone( pc.group_list(), \"has_follower\", 8791 ) and not anyone( pc.group_list(), \"has_follower\", 8792 ) and not anyone( pc.group_list(), \"has_follower\", 8793 ) and not anyone( pc.group_list(), \"has_follower\", 8794 ) and not anyone( pc.group_list(), \"has_follower\", 8795 ) and not anyone( pc.group_list(), \"has_follower\", 8796 ) and not anyone( pc.group_list(), \"has_follower\", 8797 ) and not anyone( pc.group_list(), \"has_follower\", 8798 )");
                    return !GetGlobalFlag(542) && GetGlobalVar(556) == 2 && GetGlobalVar(553) >= 1 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8791)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8792)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8793)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8794)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8795)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8796)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8797)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8798));
                case 911:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_barbarian) >= 1");
                    return pc.GetStat(Stat.level_barbarian) >= 1;
                case 912:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_bard) >= 1");
                    return pc.GetStat(Stat.level_bard) >= 1;
                case 913:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_cleric) >= 1");
                    return pc.GetStat(Stat.level_cleric) >= 1;
                case 914:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_druid) >= 1");
                    return pc.GetStat(Stat.level_druid) >= 1;
                case 915:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_fighter) >= 1");
                    return pc.GetStat(Stat.level_fighter) >= 1;
                case 916:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_monk) >= 1");
                    return pc.GetStat(Stat.level_monk) >= 1;
                case 917:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) >= 1");
                    return pc.GetStat(Stat.level_paladin) >= 1;
                case 918:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_ranger) >= 1");
                    return pc.GetStat(Stat.level_ranger) >= 1;
                case 919:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_rogue) >= 1");
                    return pc.GetStat(Stat.level_rogue) >= 1;
                case 920:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_sorcerer) >= 1");
                    return pc.GetStat(Stat.level_sorcerer) >= 1;
                case 921:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_wizard) >= 1");
                    return pc.GetStat(Stat.level_wizard) >= 1;
                case 1101:
                case 1111:
                case 1161:
                case 1171:
                case 1201:
                case 1241:
                case 1261:
                    Trace.Assert(originalScript == "game.global_vars[550] == 2");
                    return GetGlobalVar(550) == 2;
                case 1102:
                case 1112:
                case 1162:
                case 1172:
                case 1202:
                case 1242:
                case 1262:
                    Trace.Assert(originalScript == "game.global_vars[550] == 0");
                    return GetGlobalVar(550) == 0;
                case 1141:
                case 1251:
                    Trace.Assert(originalScript == "game.global_flags[539] == 0");
                    return !GetGlobalFlag(539);
                case 1142:
                case 1252:
                    Trace.Assert(originalScript == "game.global_flags[539] == 1");
                    return GetGlobalFlag(539);
                case 1151:
                case 1271:
                    Trace.Assert(originalScript == "game.global_flags[540] == 0");
                    return !GetGlobalFlag(540);
                case 1152:
                case 1272:
                    Trace.Assert(originalScript == "game.global_flags[540] == 1");
                    return GetGlobalFlag(540);
                case 1191:
                    Trace.Assert(originalScript == "game.global_flags[809] == 0");
                    return !GetGlobalFlag(809);
                case 1192:
                    Trace.Assert(originalScript == "game.global_flags[809] == 1");
                    return GetGlobalFlag(809);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 22:
                case 53:
                case 62:
                case 72:
                    Trace.Assert(originalScript == "game.global_flags[980] = 1");
                    SetGlobalFlag(980, true);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.global_vars[979] = 2");
                    SetGlobalVar(979, 2);
                    break;
                case 74:
                case 94:
                case 104:
                case 114:
                case 424:
                    Trace.Assert(originalScript == "game.global_vars[979] = 1");
                    SetGlobalVar(979, 1);
                    break;
                case 76:
                case 122:
                    Trace.Assert(originalScript == "npc_set(npc,19)");
                    ScriptDaemon.npc_set(npc, 19);
                    break;
                case 77:
                case 123:
                    Trace.Assert(originalScript == "game.global_vars[535] = 1; game.quests[110].state = qs_completed");
                    SetGlobalVar(535, 1);
                    SetQuestState(110, QuestState.Completed);
                    ;
                    break;
                case 82:
                case 83:
                case 128:
                case 129:
                    Trace.Assert(originalScript == "npc_set(npc,29)");
                    ScriptDaemon.npc_set(npc, 29);
                    break;
                case 91:
                case 112:
                case 422:
                case 1052:
                case 1282:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 92:
                case 102:
                case 423:
                case 1053:
                case 1283:
                    Trace.Assert(originalScript == "npc_set(npc,6)");
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 93:
                case 103:
                case 113:
                case 1054:
                case 1284:
                    Trace.Assert(originalScript == "npc_set(npc,20)");
                    ScriptDaemon.npc_set(npc, 20);
                    break;
                case 100:
                    Trace.Assert(originalScript == "game.global_vars[963] = 1");
                    SetGlobalVar(963, 1);
                    break;
                case 101:
                case 111:
                case 421:
                case 1051:
                case 1281:
                    Trace.Assert(originalScript == "npc_set(npc,7)");
                    ScriptDaemon.npc_set(npc, 7);
                    break;
                case 133:
                    Trace.Assert(originalScript == "npc_set(npc,30)");
                    ScriptDaemon.npc_set(npc, 30);
                    break;
                case 140:
                    Trace.Assert(originalScript == "game.global_vars[969] = 1");
                    SetGlobalVar(969, 1);
                    break;
                case 142:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 150:
                    Trace.Assert(originalScript == "game.global_vars[969] = 2");
                    SetGlobalVar(969, 2);
                    break;
                case 151:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5172,471,489)");
                    FadeAndTeleport(0, 0, 0, 5172, 471, 489);
                    break;
                case 170:
                case 180:
                    Trace.Assert(originalScript == "game.global_vars[948] = 1");
                    SetGlobalVar(948, 1);
                    break;
                case 171:
                case 181:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5170,476,483)");
                    FadeAndTeleport(0, 0, 0, 5170, 476, 483);
                    break;
                case 172:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5135,480,477)");
                    FadeAndTeleport(0, 0, 0, 5135, 480, 477);
                    break;
                case 191:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 192:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 193:
                    Trace.Assert(originalScript == "npc_set(npc,3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 194:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 195:
                    Trace.Assert(originalScript == "npc_set(npc,10)");
                    ScriptDaemon.npc_set(npc, 10);
                    break;
                case 196:
                    Trace.Assert(originalScript == "npc_set(npc,11)");
                    ScriptDaemon.npc_set(npc, 11);
                    break;
                case 202:
                    Trace.Assert(originalScript == "npc_set(npc,8)");
                    ScriptDaemon.npc_set(npc, 8);
                    break;
                case 203:
                    Trace.Assert(originalScript == "npc_set(npc,9)");
                    ScriptDaemon.npc_set(npc, 9);
                    break;
                case 250:
                    Trace.Assert(originalScript == "game.global_flags[940] = 1");
                    SetGlobalFlag(940, true);
                    break;
                case 270:
                    Trace.Assert(originalScript == "game.global_vars[948] = 2; record_time_stamp('abiram_off_to_arrest')");
                    SetGlobalVar(948, 2);
                    ScriptDaemon.record_time_stamp("abiram_off_to_arrest");
                    ;
                    break;
                case 271:
                    Trace.Assert(originalScript == "switch_to_wilfrick( npc, pc, 980)");
                    switch_to_wilfrick(npc, pc, 980);
                    break;
                case 280:
                    Trace.Assert(originalScript == "game.global_vars[944] = 3");
                    SetGlobalVar(944, 3);
                    break;
                case 281:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 291:
                    Trace.Assert(originalScript == "npc_set(npc,12)");
                    ScriptDaemon.npc_set(npc, 12);
                    break;
                case 292:
                    Trace.Assert(originalScript == "npc_set(npc,13)");
                    ScriptDaemon.npc_set(npc, 13);
                    break;
                case 293:
                    Trace.Assert(originalScript == "npc_set(npc,14)");
                    ScriptDaemon.npc_set(npc, 14);
                    break;
                case 294:
                    Trace.Assert(originalScript == "npc_set(npc,15)");
                    ScriptDaemon.npc_set(npc, 15);
                    break;
                case 302:
                    Trace.Assert(originalScript == "npc_set(npc,16)");
                    ScriptDaemon.npc_set(npc, 16);
                    break;
                case 303:
                    Trace.Assert(originalScript == "npc_set(npc,17)");
                    ScriptDaemon.npc_set(npc, 17);
                    break;
                case 304:
                    Trace.Assert(originalScript == "npc_set(npc,18)");
                    ScriptDaemon.npc_set(npc, 18);
                    break;
                case 470:
                    Trace.Assert(originalScript == "game.global_vars[535] = 2");
                    SetGlobalVar(535, 2);
                    break;
                case 580:
                    Trace.Assert(originalScript == "game.quests[109].state = qs_mentioned");
                    SetQuestState(109, QuestState.Mentioned);
                    break;
                case 671:
                    Trace.Assert(originalScript == "game.quests[109].state = qs_accepted; game.global_vars[535] = 3");
                    SetQuestState(109, QuestState.Accepted);
                    SetGlobalVar(535, 3);
                    ;
                    break;
                case 690:
                    Trace.Assert(originalScript == "npc.item_transfer_to(pc,4408); game.global_flags[532] = 1");
                    npc.TransferItemByNameTo(pc, 4408);
                    SetGlobalFlag(532, true);
                    ;
                    break;
                case 692:
                case 702:
                case 712:
                case 722:
                case 732:
                case 752:
                case 762:
                case 772:
                case 792:
                case 1132:
                    Trace.Assert(originalScript == "npc_set(npc,21)");
                    ScriptDaemon.npc_set(npc, 21);
                    break;
                case 693:
                case 703:
                case 713:
                case 723:
                case 733:
                case 753:
                case 763:
                case 773:
                case 793:
                case 1133:
                    Trace.Assert(originalScript == "npc_set(npc,22)");
                    ScriptDaemon.npc_set(npc, 22);
                    break;
                case 694:
                case 704:
                case 714:
                case 724:
                case 734:
                case 754:
                case 764:
                case 774:
                case 794:
                case 1134:
                    Trace.Assert(originalScript == "npc_set(npc,23)");
                    ScriptDaemon.npc_set(npc, 23);
                    break;
                case 695:
                case 705:
                case 715:
                case 725:
                case 735:
                case 755:
                case 765:
                case 775:
                case 795:
                case 1135:
                    Trace.Assert(originalScript == "npc_set(npc,24)");
                    ScriptDaemon.npc_set(npc, 24);
                    break;
                case 696:
                case 706:
                case 716:
                case 726:
                case 736:
                case 756:
                case 766:
                case 776:
                case 796:
                case 1136:
                    Trace.Assert(originalScript == "npc_set(npc,25)");
                    ScriptDaemon.npc_set(npc, 25);
                    break;
                case 697:
                case 707:
                case 717:
                case 727:
                case 737:
                case 757:
                case 767:
                case 777:
                case 797:
                case 1137:
                    Trace.Assert(originalScript == "npc_set(npc,26)");
                    ScriptDaemon.npc_set(npc, 26);
                    break;
                case 698:
                case 708:
                case 718:
                case 728:
                case 738:
                case 758:
                case 768:
                case 778:
                case 798:
                case 1138:
                    Trace.Assert(originalScript == "npc_set(npc,27)");
                    ScriptDaemon.npc_set(npc, 27);
                    break;
                case 699:
                case 709:
                case 719:
                case 729:
                case 739:
                case 759:
                case 769:
                case 779:
                case 799:
                case 1139:
                    Trace.Assert(originalScript == "npc_set(npc,28)");
                    ScriptDaemon.npc_set(npc, 28);
                    break;
                case 891:
                    Trace.Assert(originalScript == "pc.money_adj(110000); game.quests[109].state = qs_completed; rep_routine(npc,pc)");
                    pc.AdjustMoney(110000);
                    SetQuestState(109, QuestState.Completed);
                    rep_routine(npc, pc);
                    ;
                    break;
                case 1100:
                    Trace.Assert(originalScript == "remove_rakham(npc,pc); remove_panathaes(npc,pc)");
                    remove_rakham(npc, pc);
                    remove_panathaes(npc, pc);
                    ;
                    break;
                case 1110:
                    Trace.Assert(originalScript == "remove_boroquin(npc,pc); remove_panathaes(npc,pc)");
                    remove_boroquin(npc, pc);
                    remove_panathaes(npc, pc);
                    ;
                    break;
                case 1140:
                case 1150:
                case 1200:
                    Trace.Assert(originalScript == "remove_panathaes(npc,pc)");
                    remove_panathaes(npc, pc);
                    break;
                case 1141:
                case 1251:
                    Trace.Assert(originalScript == "game.global_vars[549] = 3");
                    SetGlobalVar(549, 3);
                    break;
                case 1040:
                    Trace.Assert(originalScript == "pc.money_adj(10000)");
                    pc.AdjustMoney(10000);
                    break;
                case 1151:
                case 1271:
                    Trace.Assert(originalScript == "game.global_vars[549] = 1");
                    SetGlobalVar(549, 1);
                    break;
                case 1221:
                    Trace.Assert(originalScript == "game.party[0].reputation_add( 67 )");
                    PartyLeader.AddReputation(67);
                    break;
                case 1240:
                    Trace.Assert(originalScript == "remove_rakham(npc,pc)");
                    remove_rakham(npc, pc);
                    break;
                case 1260:
                    Trace.Assert(originalScript == "remove_boroquin(npc,pc)");
                    remove_boroquin(npc, pc);
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
                case 202:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 15);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
