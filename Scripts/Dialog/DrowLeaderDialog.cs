
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
    [DialogScript(548)]
    public class DrowLeaderDialog : DrowLeader, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 4;
                case 5:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "party_transfer_to( npc, 11098 )";
                    Utilities.party_transfer_to(npc, 11098);
                    break;
                case 6:
                case 11:
                case 12:
                case 13:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 10:
                    originalScript = "destroy_map(npc,pc)";
                    destroy_map(npc, pc);
                    break;
                case 105:
                case 113:
                case 125:
                case 131:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 140:
                    originalScript = "game.quests[xx].state = qs_mentioned";
                    throw new NotSupportedException("SetQuestState(xx, QuestState.Mentioned);");
                case 141:
                    originalScript = "npc.item_transfer_to(pc,3000); npc.item_transfer_to(pc,9999); game.quests[xx].state = qs_accepted";
                    npc.TransferItemByNameTo(pc, 3000);
                    npc.TransferItemByNameTo(pc, 9999);
                    throw new NotSupportedException("SetQuestState(xx, QuestState.Accepted);");
                case 142:
                    originalScript = "game.quests[xx].state = qs_botched";
                    throw new NotSupportedException("SetQuestState(xx, QuestState.Botched);");
                case 151:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
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
                case 4:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 4);
                    return true;
                case 5:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
