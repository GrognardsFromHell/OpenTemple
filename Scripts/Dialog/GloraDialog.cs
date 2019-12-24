
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
    [DialogScript(89)]
    public class GloraDialog : Glora, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 2003:
                    originalScript = "(pc.stat_level_get( stat_race ) == race_elf) and (pc.stat_level_get( stat_gender ) == gender_male) and (pc.stat_level_get( stat_level_wizard ) >= 1) and (not npc.has_met( pc ))";
                    return (pc.GetRace() == RaceId.aquatic_elf) && (pc.GetGender() == Gender.Male) && (pc.GetStat(Stat.level_wizard) >= 1) && (!npc.HasMet(pc));
                case 7:
                case 8:
                    originalScript = "game.global_flags[67] == 0";
                    return !GetGlobalFlag(67);
                case 141:
                    originalScript = "pc.money_get() >= 15000";
                    return pc.GetMoney() >= 15000;
                case 142:
                    originalScript = "pc.money_get() <= 14900";
                    return pc.GetMoney() <= 14900;
                case 501:
                case 510:
                    originalScript = "game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    originalScript = "game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 1002:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 2002:
                    originalScript = "game.quests[18].state !=qs_completed";
                    return GetQuestState(18) != QuestState.Completed;
                case 2041:
                    originalScript = "game.quests[18].state <= qs_mentioned";
                    return GetQuestState(18) <= QuestState.Mentioned;
                case 2042:
                    originalScript = "game.global_flags[51] == 1";
                    return GetGlobalFlag(51);
                case 2201:
                case 2202:
                    originalScript = "game.global_vars[695] == 2";
                    return GetGlobalVar(695) == 2;
                case 2203:
                    originalScript = "game.party_alignment != LAWFUL_GOOD and game.global_vars[695] == 2";
                    return PartyAlignment != Alignment.LAWFUL_GOOD && GetGlobalVar(695) == 2;
                case 2204:
                case 2205:
                    originalScript = "game.global_vars[695] == 1";
                    return GetGlobalVar(695) == 1;
                case 2206:
                case 2207:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 12891)";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(12891));
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 141:
                    originalScript = "pc.money_adj(-15000)";
                    pc.AdjustMoney(-15000);
                    break;
                case 2041:
                    originalScript = "game.quests[18].state = qs_accepted";
                    SetQuestState(18, QuestState.Accepted);
                    break;
                case 2042:
                    originalScript = "game.quests[18].state = qs_completed";
                    SetQuestState(18, QuestState.Completed);
                    break;
                case 2120:
                    originalScript = "game.global_vars[695] = 1";
                    SetGlobalVar(695, 1);
                    break;
                case 2203:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 2206:
                case 2207:
                    originalScript = "party_transfer_to( npc, 12891 )";
                    Utilities.party_transfer_to(npc, 12891);
                    break;
                case 2210:
                    originalScript = "game.global_vars[695] = 3";
                    SetGlobalVar(695, 3);
                    break;
                case 2231:
                case 2232:
                    originalScript = "create_item_in_inventory(12892,pc)";
                    Utilities.create_item_in_inventory(12892, pc);
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
