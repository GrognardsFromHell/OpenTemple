
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
    [DialogScript(262)]
    public class BurneApprenticeDialog : BurneApprentice, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 13:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 6:
                case 7:
                    originalScript = "game.global_flags[358] == 1 and not npc.has_met( pc ) and game.global_flags[359] == 0";
                    return GetGlobalFlag(358) && !npc.HasMet(pc) && !GetGlobalFlag(359);
                case 8:
                case 9:
                    originalScript = "game.global_flags[358] == 1 and npc.has_met( pc ) and game.global_flags[359] == 0";
                    return GetGlobalFlag(358) && npc.HasMet(pc) && !GetGlobalFlag(359);
                case 10:
                case 11:
                    originalScript = "game.global_flags[359] == 1";
                    return GetGlobalFlag(359);
                case 12:
                    originalScript = "game.quests[97].state == qs_completed";
                    return GetQuestState(97) == QuestState.Completed;
                case 33:
                    originalScript = "game.quests[61].state == qs_accepted";
                    return GetQuestState(61) == QuestState.Accepted;
                case 34:
                case 35:
                    originalScript = "pc.reputation_has( 27 ) == 0 and game.global_vars[501] != 2";
                    return !pc.HasReputation(27) && GetGlobalVar(501) != 2;
                case 36:
                case 37:
                    originalScript = "pc.reputation_has( 27 ) == 0 and game.global_vars[501] == 2";
                    return !pc.HasReputation(27) && GetGlobalVar(501) == 2;
                case 47:
                case 48:
                case 55:
                case 56:
                case 73:
                case 74:
                case 85:
                case 86:
                case 105:
                case 106:
                case 605:
                case 606:
                case 623:
                case 624:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 2208 ) and anyone( pc.group_list(), \"has_item\", 4003 ) and anyone( pc.group_list(), \"has_item\", 4004 ) and anyone( pc.group_list(), \"has_item\", 3603 ) and anyone( pc.group_list(), \"has_item\", 2203 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(2208)) && pc.GetPartyMembers().Any(o => o.HasItemByName(4003)) && pc.GetPartyMembers().Any(o => o.HasItemByName(4004)) && pc.GetPartyMembers().Any(o => o.HasItemByName(3603)) && pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 151:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 12 and game.party[0].reputation_has( 15 )";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12 && PartyLeader.HasReputation(15);
                case 201:
                case 391:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 202:
                case 203:
                case 392:
                case 393:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 211:
                    originalScript = "pc.money_get() < 5000";
                    return pc.GetMoney() < 5000;
                case 212:
                    originalScript = "pc.money_get() >= 5000";
                    return pc.GetMoney() >= 5000;
                case 231:
                    originalScript = "game.global_vars[501] != 2";
                    return GetGlobalVar(501) != 2;
                case 232:
                    originalScript = "game.global_vars[501] == 2";
                    return GetGlobalVar(501) == 2;
                case 251:
                case 252:
                    originalScript = "npc.area != 1";
                    return npc.GetArea() != 1;
                case 253:
                case 254:
                    originalScript = "npc.area == 1";
                    return npc.GetArea() == 1;
                case 257:
                case 258:
                    originalScript = "game.global_flags[358] == 1 and game.global_flags[359] == 0";
                    return GetGlobalFlag(358) && !GetGlobalFlag(359);
                case 312:
                    originalScript = "pc.stat_level_get(stat_level_wizard) >= 1";
                    return pc.GetStat(Stat.level_wizard) >= 1;
                case 313:
                    originalScript = "pc.stat_level_get(stat_level_sorcerer) >= 1";
                    return pc.GetStat(Stat.level_sorcerer) >= 1;
                case 331:
                case 341:
                    originalScript = "game.global_vars[526] <= 7";
                    return GetGlobalVar(526) <= 7;
                case 332:
                case 342:
                    originalScript = "game.global_vars[526] >= 8";
                    return GetGlobalVar(526) >= 8;
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
                    originalScript = "game.global_vars[109] = 1";
                    SetGlobalVar(109, 1);
                    break;
                case 47:
                case 48:
                case 55:
                case 56:
                case 73:
                case 74:
                case 85:
                case 86:
                case 105:
                case 106:
                    originalScript = "party_transfer_to( npc, 2208 ); party_transfer_to( npc, 4003 ); party_transfer_to( npc, 4004 ); party_transfer_to( npc, 3603 ); party_transfer_to( npc, 2203 )";
                    Utilities.party_transfer_to(npc, 2208);
                    Utilities.party_transfer_to(npc, 4003);
                    Utilities.party_transfer_to(npc, 4004);
                    Utilities.party_transfer_to(npc, 3603);
                    Utilities.party_transfer_to(npc, 2203);
                    ;
                    break;
                case 90:
                case 640:
                    originalScript = "destroy_orb( npc, pc ); game.global_flags[359] = 1";
                    destroy_orb(npc, pc);
                    SetGlobalFlag(359, true);
                    ;
                    break;
                case 121:
                case 641:
                case 642:
                case 661:
                case 662:
                case 663:
                case 664:
                    originalScript = "play_effect( npc, pc )";
                    play_effect(npc, pc);
                    break;
                case 230:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 231:
                    originalScript = "pc.money_adj(-5000)";
                    pc.AdjustMoney(-5000);
                    break;
                case 241:
                    originalScript = "switch_to_tarah( npc, pc, 280)";
                    switch_to_tarah(npc, pc, 280);
                    break;
                case 280:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 291:
                case 292:
                    originalScript = "pc.follower_remove( npc ); pc.follower_add( npc )";
                    pc.RemoveFollower(npc);
                    pc.AddFollower(npc);
                    ;
                    break;
                case 321:
                    originalScript = "switch_to_tarah( npc, pc, 300)";
                    switch_to_tarah(npc, pc, 300);
                    break;
                case 331:
                case 341:
                    originalScript = "game.global_vars[526] = game.global_vars[526] + 1";
                    SetGlobalVar(526, GetGlobalVar(526) + 1);
                    break;
                case 340:
                    originalScript = "create_item_in_inventory(8006,pc)";
                    Utilities.create_item_in_inventory(8006, pc);
                    break;
                case 605:
                case 606:
                case 623:
                case 624:
                    originalScript = "pishella_destroy_skull_while_party_npc(pc,npc)";
                    pishella_destroy_skull_while_party_npc(pc, npc);
                    break;
                case 22000:
                    originalScript = "game.global_vars[911] = 32";
                    SetGlobalVar(911, 32);
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
                case 151:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
