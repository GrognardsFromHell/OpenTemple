
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
    [DialogScript(161)]
    public class FemalePrisoner1Dialog : FemalePrisoner1, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 92:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 21:
                case 32:
                case 101:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetGender() == Gender.Male;
                case 22:
                case 34:
                case 102:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 33:
                    originalScript = "( game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD ) and ( pc.stat_level_get( stat_gender ) == gender_male )";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD) && (pc.GetGender() == Gender.Male);
                case 35:
                    originalScript = "( game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD ) and ( pc.stat_level_get( stat_gender ) == gender_female )";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD) && (pc.GetGender() == Gender.Female);
                case 41:
                case 51:
                case 64:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 42:
                case 52:
                case 65:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 43:
                case 53:
                case 66:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 61:
                    originalScript = "pc.money_get() >= 100000";
                    return pc.GetMoney() >= 100000;
                case 62:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 63:
                    originalScript = "pc.money_get() >= 1000";
                    return pc.GetMoney() >= 1000;
                case 81:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 82:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
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
                    originalScript = "game.quests[70].state = qs_mentioned; game.quests[71].state = qs_mentioned";
                    SetQuestState(70, QuestState.Mentioned);
                    SetQuestState(71, QuestState.Mentioned);
                    ;
                    break;
                case 2:
                    originalScript = "game.quests[70].state = qs_accepted; game.quests[71].state = qs_accepted";
                    SetQuestState(70, QuestState.Accepted);
                    SetQuestState(71, QuestState.Accepted);
                    ;
                    break;
                case 21:
                case 22:
                case 32:
                case 33:
                case 34:
                case 35:
                    originalScript = "game.quests[70].state = qs_completed; game.quests[71].state = qs_completed";
                    SetQuestState(70, QuestState.Completed);
                    SetQuestState(71, QuestState.Completed);
                    ;
                    break;
                case 40:
                case 50:
                    originalScript = "game.global_flags[131] = 1; free_rep( npc, pc )";
                    SetGlobalFlag(131, true);
                    free_rep(npc, pc);
                    ;
                    break;
                case 41:
                case 42:
                case 43:
                case 51:
                case 52:
                case 53:
                case 64:
                case 65:
                case 66:
                case 101:
                case 102:
                case 121:
                case 131:
                case 141:
                    originalScript = "run_off( npc, pc ); game.global_flags[993] = 1";
                    run_off(npc, pc);
                    SetGlobalFlag(993, true);
                    ;
                    break;
                case 61:
                    originalScript = "pc.money_adj(-100000); game.global_vars[998] = 1";
                    pc.AdjustMoney(-100000);
                    SetGlobalVar(998, 1);
                    ;
                    break;
                case 62:
                    originalScript = "pc.money_adj(-10000); game.global_vars[998] = 2";
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(998, 2);
                    ;
                    break;
                case 63:
                    originalScript = "pc.money_adj(-1000); game.global_vars[998] = 3";
                    pc.AdjustMoney(-1000);
                    SetGlobalVar(998, 3);
                    ;
                    break;
                case 72:
                    originalScript = "npc.attack(pc); game.quests[70].state = qs_botched; game.quests[71].state = qs_botched";
                    npc.Attack(pc);
                    SetQuestState(70, QuestState.Botched);
                    SetQuestState(71, QuestState.Botched);
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
