
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
    [DialogScript(341)]
    public class KenanDialog : Kenan, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    Trace.Assert(originalScript == "(game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == NEUTRAL_GOOD)");
                    return (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.NEUTRAL_GOOD);
                case 12:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 13:
                    Trace.Assert(originalScript == "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
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
                    Trace.Assert(originalScript == "game.global_flags[870] = 1");
                    SetGlobalFlag(870, true);
                    break;
                case 51:
                case 61:
                case 71:
                case 81:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(14); game.quests[78].state = qs_botched; game.global_vars[993] = 3");
                    // FIXME: worldmap_travel_by_dialog;
                    SetQuestState(78, QuestState.Botched);
                    SetGlobalVar(993, 3);
                    ;
                    break;
                case 52:
                case 62:
                case 72:
                case 82:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 1)");
                    switch_to_tarah(npc, pc, 1);
                    break;
                case 53:
                case 63:
                case 73:
                case 83:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 91:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 130)");
                    switch_to_tarah(npc, pc, 130);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
