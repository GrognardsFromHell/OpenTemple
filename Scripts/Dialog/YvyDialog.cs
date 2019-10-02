
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
    [DialogScript(281)]
    public class YvyDialog : Yvy, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 11:
                case 12:
                case 25:
                case 26:
                    Trace.Assert(originalScript == "game.quests[15].state == qs_accepted");
                    return GetQuestState(15) == QuestState.Accepted;
                case 22:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 91:
                    Trace.Assert(originalScript == "pc.money_get() >= 500");
                    return pc.GetMoney() >= 500;
                case 92:
                    Trace.Assert(originalScript == "pc.money_get() >= 1500");
                    return pc.GetMoney() >= 1500;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 100:
                    Trace.Assert(originalScript == "pc.money_adj(-500)");
                    pc.AdjustMoney(-500);
                    break;
                case 110:
                    Trace.Assert(originalScript == "pc.money_adj(-1500)");
                    pc.AdjustMoney(-1500);
                    break;
                case 111:
                    Trace.Assert(originalScript == "create_item_in_inventory(6330,pc)");
                    Utilities.create_item_in_inventory(6330, pc);
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
