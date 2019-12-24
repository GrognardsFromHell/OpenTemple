
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
    [DialogScript(374)]
    public class NybbleDialog : Nybble, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not npc.has_met(pc) and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)";
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 3:
                    originalScript = "not npc.has_met(pc) and (game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 4:
                    originalScript = "not npc.has_met(pc) and (game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 5:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 21:
                    originalScript = "game.global_vars[999] >= 15 and game.quests[69].state != qs_completed";
                    return GetGlobalVar(999) >= 15 && GetQuestState(69) != QuestState.Completed;
                case 22:
                    originalScript = "game.global_vars[999] >= 1 and game.global_vars[999] <= 14";
                    return GetGlobalVar(999) >= 1 && GetGlobalVar(999) <= 14;
                case 23:
                    originalScript = "game.global_vars[999] == 0 and game.quests[69].state == qs_mentioned";
                    return GetGlobalVar(999) == 0 && GetQuestState(69) == QuestState.Mentioned;
                case 31:
                    originalScript = "game.quests[69].state == qs_unknown";
                    return GetQuestState(69) == QuestState.Unknown;
                case 32:
                    originalScript = "game.quests[69].state >= qs_mentioned";
                    return GetQuestState(69) >= QuestState.Mentioned;
                case 41:
                case 71:
                case 87:
                    originalScript = "not get_1(npc)";
                    return !Scripts.get_1(npc);
                case 42:
                case 58:
                case 88:
                    originalScript = "not get_2(npc)";
                    return !Scripts.get_2(npc);
                case 43:
                case 54:
                case 72:
                    originalScript = "not get_3(npc)";
                    return !Scripts.get_3(npc);
                case 51:
                case 55:
                case 73:
                case 76:
                case 81:
                case 84:
                case 91:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 52:
                case 56:
                case 74:
                case 77:
                case 82:
                case 85:
                case 92:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 53:
                case 57:
                case 75:
                case 78:
                case 83:
                case 86:
                case 93:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 61:
                    originalScript = "game.quests[69].state == qs_mentioned and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)";
                    return GetQuestState(69) == QuestState.Mentioned && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 62:
                    originalScript = "game.quests[69].state == qs_mentioned and (game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return GetQuestState(69) == QuestState.Mentioned && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 63:
                    originalScript = "game.quests[69].state == qs_mentioned and (game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return GetQuestState(69) == QuestState.Mentioned && (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 64:
                    originalScript = "game.quests[69].state == qs_accepted and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)";
                    return GetQuestState(69) == QuestState.Accepted && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 65:
                    originalScript = "game.quests[69].state == qs_accepted and (game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return GetQuestState(69) == QuestState.Accepted && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 66:
                    originalScript = "game.quests[69].state == qs_accepted and (game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return GetQuestState(69) == QuestState.Accepted && (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                    originalScript = "game.quests[69].state = qs_accepted";
                    SetQuestState(69, QuestState.Accepted);
                    break;
                case 31:
                    originalScript = "game.quests[69].state = qs_mentioned";
                    SetQuestState(69, QuestState.Mentioned);
                    break;
                case 41:
                case 71:
                case 87:
                    originalScript = "npc_1(npc)";
                    Scripts.npc_1(npc);
                    break;
                case 42:
                case 58:
                case 88:
                    originalScript = "npc_2(npc)";
                    Scripts.npc_2(npc);
                    break;
                case 43:
                case 54:
                case 72:
                    originalScript = "npc_3(npc)";
                    Scripts.npc_3(npc);
                    break;
                case 61:
                case 62:
                case 63:
                    originalScript = "npc.item_transfer_to(pc,4407); game.quests[69].state = qs_accepted";
                    npc.TransferItemByNameTo(pc, 4407);
                    SetQuestState(69, QuestState.Accepted);
                    ;
                    break;
                case 64:
                case 65:
                case 66:
                    originalScript = "npc.item_transfer_to(pc,4407)";
                    npc.TransferItemByNameTo(pc, 4407);
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
