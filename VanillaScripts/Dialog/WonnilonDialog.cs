
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

namespace VanillaScripts.Dialog
{
    [DialogScript(129)]
    public class WonnilonDialog : Wonnilon, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                    originalScript = "not (anyone( pc.group_list(), \"has_wielded\", 3007 ))";
                    return !(pc.GetPartyMembers().Any(o => o.HasEquippedByName(3007)));
                case 7:
                    originalScript = "anyone( pc.group_list(), \"has_wielded\", 3007 )";
                    return pc.GetPartyMembers().Any(o => o.HasEquippedByName(3007));
                case 8:
                case 9:
                case 13:
                case 14:
                case 73:
                case 74:
                case 112:
                    originalScript = "pc.stat_level_get(stat_race) != race_gnome";
                    return pc.GetRace() != RaceId.svirfneblin;
                case 11:
                case 12:
                case 75:
                case 76:
                case 113:
                    originalScript = "pc.stat_level_get(stat_race) == race_gnome";
                    return pc.GetRace() == RaceId.svirfneblin;
                case 62:
                case 63:
                case 64:
                case 65:
                case 163:
                case 164:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 10402 ) and anyone( pc.group_list(), \"has_item\", 2204 ) and anyone( pc.group_list(), \"has_item\", 3012 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(10402)) && pc.GetPartyMembers().Any(o => o.HasItemByName(2204)) && pc.GetPartyMembers().Any(o => o.HasItemByName(3012));
                case 181:
                case 182:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 183:
                case 184:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                case 201:
                    originalScript = "npc.has_met( pc ) and (npc.leader_get() == OBJ_HANDLE_NULL)";
                    return npc.HasMet(pc) && (npc.GetLeader() == null);
                case 202:
                case 203:
                    originalScript = "game.quests[55].state == qs_mentioned";
                    return GetQuestState(55) == QuestState.Mentioned;
                case 204:
                case 205:
                    originalScript = "game.quests[55].state == qs_accepted";
                    return GetQuestState(55) == QuestState.Accepted;
                case 206:
                case 207:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 10402 ) and anyone( pc.group_list(), \"has_item\", 2204 ) and anyone( pc.group_list(), \"has_item\", 3012 ) and ( game.quests[55].state == qs_accepted or game.quests[55].state == qs_mentioned )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(10402)) && pc.GetPartyMembers().Any(o => o.HasItemByName(2204)) && pc.GetPartyMembers().Any(o => o.HasItemByName(3012)) && (GetQuestState(55) == QuestState.Accepted || GetQuestState(55) == QuestState.Mentioned);
                case 208:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 10402 ) and anyone( pc.group_list(), \"has_item\", 2204 ) and anyone( pc.group_list(), \"has_item\", 3012 )  and ( game.quests[55].state == qs_unknown )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(10402)) && pc.GetPartyMembers().Any(o => o.HasItemByName(2204)) && pc.GetPartyMembers().Any(o => o.HasItemByName(3012)) && (GetQuestState(55) == QuestState.Unknown);
                case 209:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 10402 ) and anyone( pc.group_list(), \"has_item\", 2204 ) and anyone( pc.group_list(), \"has_item\", 3012 ) and ( game.quests[55].state == qs_unknown )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(10402)) && pc.GetPartyMembers().Any(o => o.HasItemByName(2204)) && pc.GetPartyMembers().Any(o => o.HasItemByName(3012)) && (GetQuestState(55) == QuestState.Unknown);
                case 210:
                case 211:
                    originalScript = "(npc.leader_get() != OBJ_HANDLE_NULL)";
                    return (npc.GetLeader() != null);
                case 214:
                case 215:
                    originalScript = "( anyone( pc.group_list(), \"has_item\", 2205 ) and anyone( pc.group_list(), \"has_item\", 4005 )  ) and ( game.quests[56].state == qs_accepted or game.quests[56].state == qs_mentioned )";
                    return (pc.GetPartyMembers().Any(o => o.HasItemByName(2205)) && pc.GetPartyMembers().Any(o => o.HasItemByName(4005))) && (GetQuestState(56) == QuestState.Accepted || GetQuestState(56) == QuestState.Mentioned);
                case 216:
                case 217:
                    originalScript = "( game.global_flags[128] == 0 ) and ( game.quests[55].state == qs_unknown or game.quests[55].state == qs_completed ) and (npc.leader_get() == OBJ_HANDLE_NULL)";
                    return (!GetGlobalFlag(128)) && (GetQuestState(55) == QuestState.Unknown || GetQuestState(55) == QuestState.Completed) && (npc.GetLeader() == null);
                case 218:
                case 219:
                    originalScript = "( game.global_flags[128] == 1 ) and ( game.quests[55].state == qs_unknown or game.quests[55].state == qs_completed ) and (npc.leader_get() == OBJ_HANDLE_NULL)";
                    return (GetGlobalFlag(128)) && (GetQuestState(55) == QuestState.Unknown || GetQuestState(55) == QuestState.Completed) && (npc.GetLeader() == null);
                case 220:
                case 221:
                    originalScript = "( game.quests[55].state == qs_accepted or game.quests[55].state == qs_mentioned )";
                    return (GetQuestState(55) == QuestState.Accepted || GetQuestState(55) == QuestState.Mentioned);
                case 222:
                case 223:
                    originalScript = "game.quests[56].state == qs_mentioned";
                    return GetQuestState(56) == QuestState.Mentioned;
                case 224:
                case 225:
                    originalScript = "game.quests[55].state == qs_unknown and game.global_flags[128] == 1";
                    return GetQuestState(55) == QuestState.Unknown && GetGlobalFlag(128);
                case 371:
                case 372:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 376:
                    originalScript = "pc.stat_level_get(stat_race) != race_gnome and npc.leader_get() == OBJ_HANDLE_NULL";
                    return pc.GetRace() != RaceId.svirfneblin && npc.GetLeader() == null;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 21:
                case 22:
                case 111:
                case 112:
                case 113:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 4:
                case 5:
                    originalScript = "game.global_flags[129] = 1";
                    SetGlobalFlag(129, true);
                    break;
                case 8:
                case 9:
                case 63:
                case 65:
                case 73:
                case 74:
                case 75:
                case 76:
                case 165:
                case 166:
                case 261:
                case 262:
                case 291:
                case 292:
                case 376:
                case 383:
                case 384:
                case 403:
                case 404:
                    originalScript = "game.global_flags[128] = 1";
                    SetGlobalFlag(128, true);
                    break;
                case 24:
                case 25:
                case 123:
                case 124:
                case 151:
                case 152:
                    originalScript = "game.global_flags[128] = 1; go_hideout(npc,pc)";
                    SetGlobalFlag(128, true);
                    go_hideout(npc, pc);
                    ;
                    break;
                case 50:
                    originalScript = "game.story_state = 5";
                    StoryState = 5;
                    break;
                case 60:
                    originalScript = "game.quests[55].state = qs_mentioned";
                    SetQuestState(55, QuestState.Mentioned);
                    break;
                case 62:
                case 64:
                case 281:
                case 282:
                case 293:
                case 294:
                    originalScript = "party_transfer_to( npc, 10402 ); party_transfer_to( npc, 2204 ); party_transfer_to( npc, 3012 )";
                    Utilities.party_transfer_to(npc, 10402);
                    Utilities.party_transfer_to(npc, 2204);
                    Utilities.party_transfer_to(npc, 3012);
                    ;
                    break;
                case 70:
                case 170:
                    originalScript = "game.map_flags( 5066, 0, 1 )";
                    // FIXME: map_flags;
                    break;
                case 71:
                case 72:
                case 251:
                case 252:
                    originalScript = "game.quests[55].state = qs_accepted";
                    SetQuestState(55, QuestState.Accepted);
                    break;
                case 81:
                case 91:
                case 92:
                case 114:
                case 115:
                case 131:
                case 132:
                case 141:
                case 171:
                case 191:
                    originalScript = "go_hideout(npc,pc)";
                    go_hideout(npc, pc);
                    break;
                case 100:
                    originalScript = "game.quests[55].state = qs_completed";
                    SetQuestState(55, QuestState.Completed);
                    break;
                case 163:
                case 164:
                    originalScript = "party_transfer_to( npc, 10402 ); party_transfer_to( npc, 2204 ); party_transfer_to( npc, 3012 ); game.global_flags[128] = 0";
                    Utilities.party_transfer_to(npc, 10402);
                    Utilities.party_transfer_to(npc, 2204);
                    Utilities.party_transfer_to(npc, 3012);
                    SetGlobalFlag(128, false);
                    ;
                    break;
                case 181:
                case 182:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 202:
                case 203:
                    originalScript = "game.global_flags[128] = 0; game.quests[55].state = qs_accepted";
                    SetGlobalFlag(128, false);
                    SetQuestState(55, QuestState.Accepted);
                    ;
                    break;
                case 206:
                case 207:
                case 208:
                case 209:
                    originalScript = "game.global_flags[128] = 0";
                    SetGlobalFlag(128, false);
                    break;
                case 300:
                    originalScript = "game.quests[56].state = qs_mentioned";
                    SetQuestState(56, QuestState.Mentioned);
                    break;
                case 301:
                case 302:
                case 351:
                case 352:
                    originalScript = "game.quests[56].state = qs_accepted";
                    SetQuestState(56, QuestState.Accepted);
                    break;
                case 331:
                    originalScript = "pc.follower_remove( npc ); go_hideout(npc,pc)";
                    pc.RemoveFollower(npc);
                    go_hideout(npc, pc);
                    ;
                    break;
                case 360:
                    originalScript = "game.quests[56].state = qs_completed";
                    SetQuestState(56, QuestState.Completed);
                    break;
                case 361:
                case 362:
                    originalScript = "party_transfer_to( npc, 2205 )";
                    Utilities.party_transfer_to(npc, 2205);
                    break;
                case 401:
                case 402:
                    originalScript = "party_transfer_to( npc, 2205 ); party_transfer_to( npc, 4005 )";
                    Utilities.party_transfer_to(npc, 2205);
                    Utilities.party_transfer_to(npc, 4005);
                    ;
                    break;
                case 431:
                    originalScript = "pc.follower_remove( npc ); disappear(npc,pc)";
                    pc.RemoveFollower(npc);
                    disappear(npc, pc);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
