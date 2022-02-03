
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
    [DialogScript(341)]
    public class KenanDialog : Kenan, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    originalScript = "(game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == NEUTRAL_GOOD)";
                    return (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.NEUTRAL_GOOD);
                case 12:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 13:
                    originalScript = "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
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
                    originalScript = "game.global_flags[870] = 1";
                    SetGlobalFlag(870, true);
                    break;
                case 51:
                case 61:
                case 71:
                case 81:
                    originalScript = "game.worldmap_travel_by_dialog(14); game.quests[78].state = qs_botched; game.global_vars[993] = 3";
                    WorldMapTravelByDialog(14);
                    SetQuestState(78, QuestState.Botched);
                    SetGlobalVar(993, 3);
                    break;
                case 52:
                case 62:
                case 72:
                case 82:
                    originalScript = "switch_to_tarah( npc, pc, 1)";
                    switch_to_tarah(npc, pc, 1);
                    break;
                case 53:
                case 63:
                case 73:
                case 83:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 91:
                    originalScript = "switch_to_tarah( npc, pc, 130)";
                    switch_to_tarah(npc, pc, 130);
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
