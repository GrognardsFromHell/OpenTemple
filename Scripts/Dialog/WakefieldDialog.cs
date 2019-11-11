
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
    [DialogScript(484)]
    public class WakefieldDialog : Wakefield, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 12:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 14:
                case 164:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 2203 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 31:
                case 161:
                    originalScript = "not pc.follower_atmax() and not anyone( pc.group_list(), \"has_follower\", 8731 )";
                    return !pc.HasMaxFollowers() && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8731));
                case 32:
                case 162:
                    originalScript = "pc.follower_atmax() and not anyone( pc.group_list(), \"has_follower\", 8731 )";
                    return pc.HasMaxFollowers() && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8731));
                case 101:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 102:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 103:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 104:
                    originalScript = "pc.stat_level_get( stat_deity ) == 9 and not anyone( pc.group_list(), \"has_follower\", 8731 )";
                    return pc.GetStat(Stat.deity) == 9 && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8731));
                case 105:
                    originalScript = "pc.stat_level_get( stat_deity ) == 9";
                    return pc.GetStat(Stat.deity) == 9;
                case 111:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD) and anyone( pc.group_list(), \"has_item\", 2203 )";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD) && pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 112:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and anyone( pc.group_list(), \"has_item\", 2203 )";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 113:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL) and anyone( pc.group_list(), \"has_item\", 2203 )";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL) && pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 114:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD) and not anyone( pc.group_list(), \"has_item\", 2203 )";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD) && !pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 115:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and not anyone( pc.group_list(), \"has_item\", 2203 )";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 116:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL) and not anyone( pc.group_list(), \"has_item\", 2203 )";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL) && !pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 151:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 152:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 153:
                    originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "run_off(npc, pc); game.quests[103].state = qs_completed; game.quests[97].state = qs_botched";
                    run_off(npc, pc);
                    SetQuestState(103, QuestState.Completed);
                    SetQuestState(97, QuestState.Botched);
                    ;
                    break;
                case 11:
                case 12:
                case 31:
                case 32:
                case 161:
                case 162:
                    originalScript = "game.global_flags[525] = 1";
                    SetGlobalFlag(525, true);
                    break;
                case 13:
                case 22:
                case 33:
                case 151:
                case 152:
                case 153:
                case 163:
                    originalScript = "hextor_buff_2(npc,pc)";
                    hextor_buff_2(npc, pc);
                    break;
                case 21:
                    originalScript = "party_transfer_to( npc, 2203 ); flag_no_transfer(npc,pc)";
                    Utilities.party_transfer_to(npc, 2203);
                    flag_no_transfer(npc, pc);
                    ;
                    break;
                case 100:
                    originalScript = "game.global_vars[501] = 6";
                    SetGlobalVar(501, 6);
                    break;
                case 121:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 171:
                    originalScript = "game.quests[103].state = qs_accepted; pc.follower_add(npc); td_attack(npc,pc)";
                    SetQuestState(103, QuestState.Accepted);
                    pc.AddFollower(npc);
                    td_attack(npc, pc);
                    ;
                    break;
                case 181:
                    originalScript = "game.quests[103].state = qs_accepted; td_attack(npc,pc)";
                    SetQuestState(103, QuestState.Accepted);
                    td_attack(npc, pc);
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
