
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
    [DialogScript(102)]
    public class GrudDialog : Grud, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 101:
                case 102:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 5803 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5803));
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    Trace.Assert(originalScript == "game.quests[35].state = qs_mentioned");
                    SetQuestState(35, QuestState.Mentioned);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.quests[35].state = qs_accepted; game.areas[6] = 1");
                    SetQuestState(35, QuestState.Accepted);
                    MakeAreaKnown(6);
                    ;
                    break;
                case 101:
                case 102:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 5803 )");
                    Utilities.party_transfer_to(npc, 5803);
                    break;
                case 110:
                    Trace.Assert(originalScript == "game.quests[35].state = qs_completed");
                    SetQuestState(35, QuestState.Completed);
                    break;
                case 130:
                    Trace.Assert(originalScript == "pc.reputation_add( 25 )");
                    pc.AddReputation(25);
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
