
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

namespace VanillaScripts.Dialog
{
    [DialogScript(102)]
    public class GrudDialog : Grud, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 101:
                case 102:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5803 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5803));
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    originalScript = "game.quests[35].state = qs_mentioned";
                    SetQuestState(35, QuestState.Mentioned);
                    break;
                case 50:
                    originalScript = "game.quests[35].state = qs_accepted; game.areas[6] = 1";
                    SetQuestState(35, QuestState.Accepted);
                    MakeAreaKnown(6);
                    ;
                    break;
                case 101:
                case 102:
                    originalScript = "party_transfer_to( npc, 5803 )";
                    Utilities.party_transfer_to(npc, 5803);
                    break;
                case 110:
                    originalScript = "game.quests[35].state = qs_completed";
                    SetQuestState(35, QuestState.Completed);
                    break;
                case 130:
                    originalScript = "pc.reputation_add( 19 )";
                    pc.AddReputation(19);
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
