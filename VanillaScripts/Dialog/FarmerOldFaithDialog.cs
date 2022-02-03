
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
    [DialogScript(10)]
    public class FarmerOldFaithDialog : FarmerOldFaith, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 11:
                case 12:
                case 51:
                case 52:
                    originalScript = "game.quests[6].state <= qs_accepted";
                    return GetQuestState(6) <= QuestState.Accepted;
                case 13:
                case 14:
                case 53:
                case 54:
                    originalScript = "game.quests[6].state == qs_completed";
                    return GetQuestState(6) == QuestState.Completed;
                case 21:
                    originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == LAWFUL_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 22:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_GOOD or CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 24:
                case 28:
                    originalScript = "game.quests[6].state == qs_accepted";
                    return GetQuestState(6) == QuestState.Accepted;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
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
