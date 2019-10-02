
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
    [DialogScript(162)]
    public class FemalePrisoner2Dialog : FemalePrisoner2, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 102:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 41:
                case 61:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 42:
                case 62:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 43:
                case 63:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 91:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 92:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
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
                    Trace.Assert(originalScript == "game.quests[70].state = qs_mentioned; game.quests[71].state = qs_mentioned");
                    SetQuestState(70, QuestState.Mentioned);
                    SetQuestState(71, QuestState.Mentioned);
                    ;
                    break;
                case 2:
                    Trace.Assert(originalScript == "game.quests[70].state = qs_accepted; game.quests[71].state = qs_accepted");
                    SetQuestState(70, QuestState.Accepted);
                    SetQuestState(71, QuestState.Accepted);
                    ;
                    break;
                case 21:
                case 32:
                case 33:
                    Trace.Assert(originalScript == "game.quests[70].state = qs_completed; game.quests[71].state = qs_completed");
                    SetQuestState(70, QuestState.Completed);
                    SetQuestState(71, QuestState.Completed);
                    ;
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.global_flags[131] = 1; free_rep( npc, pc )");
                    SetGlobalFlag(131, true);
                    free_rep(npc, pc);
                    ;
                    break;
                case 41:
                case 42:
                case 43:
                    Trace.Assert(originalScript == "run_off( npc, pc ); game.global_flags[994] = 1;");
                    run_off(npc, pc);
                    SetGlobalFlag(994, true);
                    ;
                    break;
                case 61:
                case 62:
                case 63:
                case 81:
                    Trace.Assert(originalScript == "run_off( npc, pc ); game.global_flags[994] = 1");
                    run_off(npc, pc);
                    SetGlobalFlag(994, true);
                    ;
                    break;
                case 72:
                    Trace.Assert(originalScript == "npc.attack(pc); game.quests[70].state = qs_botched; game.quests[71].state = qs_botched");
                    npc.Attack(pc);
                    SetQuestState(70, QuestState.Botched);
                    SetQuestState(71, QuestState.Botched);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
