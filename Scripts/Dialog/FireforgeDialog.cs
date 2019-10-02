
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
    [DialogScript(487)]
    public class FireforgeDialog : Fireforge, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                    Trace.Assert(originalScript == "not npc_get(npc,2)");
                    return !ScriptDaemon.npc_get(npc, 2);
                case 21:
                case 41:
                    Trace.Assert(originalScript == "not npc_get(npc,5)");
                    return !ScriptDaemon.npc_get(npc, 5);
                case 22:
                case 31:
                    Trace.Assert(originalScript == "not npc_get(npc,4)");
                    return !ScriptDaemon.npc_get(npc, 4);
                case 101:
                    Trace.Assert(originalScript == "not npc_get(npc,1)");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 102:
                    Trace.Assert(originalScript == "game.global_flags[505] == 1 and not anyone( pc.group_list(), \"has_item\", 12640 ) and game.quests[98].state == qs_unknown");
                    return GetGlobalFlag(505) && !pc.GetPartyMembers().Any(o => o.HasItemByName(12640)) && GetQuestState(98) == QuestState.Unknown;
                case 103:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 12640 ) and game.global_flags[966] == 1");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(12640)) && GetGlobalFlag(966);
                case 104:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 12640 ) and game.global_flags[966] == 0");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(12640)) && !GetGlobalFlag(966);
                case 105:
                    Trace.Assert(originalScript == "game.quests[98].state == qs_mentioned and game.quests[96].state == qs_unknown");
                    return GetQuestState(98) == QuestState.Mentioned && GetQuestState(96) == QuestState.Unknown;
                case 106:
                    Trace.Assert(originalScript == "game.quests[98].state == qs_mentioned and (game.quests[96].state == qs_mentioned or game.quests[96].state == qs_accepted)");
                    return GetQuestState(98) == QuestState.Mentioned && (GetQuestState(96) == QuestState.Mentioned || GetQuestState(96) == QuestState.Accepted);
                case 107:
                    Trace.Assert(originalScript == "game.quests[98].state == qs_accepted and game.global_flags[506] == 0");
                    return GetQuestState(98) == QuestState.Accepted && !GetGlobalFlag(506);
                case 511:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 512:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 21:
                case 41:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 22:
                case 31:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 101:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 103:
                case 104:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 12640 )");
                    Utilities.party_transfer_to(npc, 12640);
                    break;
                case 106:
                case 122:
                case 522:
                    Trace.Assert(originalScript == "game.quests[98].state = qs_accepted");
                    SetQuestState(98, QuestState.Accepted);
                    break;
                case 500:
                    Trace.Assert(originalScript == "game.quests[98].state = qs_mentioned");
                    SetQuestState(98, QuestState.Mentioned);
                    break;
                case 520:
                    Trace.Assert(originalScript == "game.global_flags[508] = 1");
                    SetGlobalFlag(508, true);
                    break;
                case 531:
                    Trace.Assert(originalScript == "game.areas[16] = 1");
                    MakeAreaKnown(16);
                    break;
                case 600:
                case 610:
                    Trace.Assert(originalScript == "game.quests[98].state = qs_completed");
                    SetQuestState(98, QuestState.Completed);
                    break;
                case 601:
                    Trace.Assert(originalScript == "game.global_vars[507] = 1; run_off( npc, pc )");
                    SetGlobalVar(507, 1);
                    // TODO: Original script had run_off here, Removed
                    break;
                case 611:
                    Trace.Assert(originalScript == "game.global_vars[507] = 2; run_off( npc, pc )");
                    SetGlobalVar(507, 2);
                    // TODO: Original script had run_off here, Removed
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
