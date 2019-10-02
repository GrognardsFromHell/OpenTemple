
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
    [DialogScript(24)]
    public class WidowDialog : Widow, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 6:
                    Trace.Assert(originalScript == "game.quests[6].state <= qs_accepted and npc.has_met( pc ) and game.global_flags[14] == 0");
                    return GetQuestState(6) <= QuestState.Accepted && npc.HasMet(pc) && !GetGlobalFlag(14);
                case 7:
                case 143:
                case 144:
                    Trace.Assert(originalScript == "game.global_flags[14] == 1");
                    return GetGlobalFlag(14);
                case 8:
                    Trace.Assert(originalScript == "game.quests[6].state == qs_completed");
                    return GetQuestState(6) == QuestState.Completed;
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.global_flags[67] == 0");
                    return !GetGlobalFlag(67);
                case 91:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 92:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 105:
                    Trace.Assert(originalScript == "game.global_flags[1] == 1");
                    return GetGlobalFlag(1);
                case 106:
                case 107:
                    Trace.Assert(originalScript == "game.quests[6].state == qs_accepted");
                    return GetQuestState(6) == QuestState.Accepted;
                case 108:
                case 109:
                    Trace.Assert(originalScript == "game.global_flags[14] == 1 and npc.has_met( pc )");
                    return GetGlobalFlag(14) && npc.HasMet(pc);
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.global_flags[14] == 0");
                    return !GetGlobalFlag(14);
                case 501:
                case 510:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    Trace.Assert(originalScript == "game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 160:
                    Trace.Assert(originalScript == "game.global_flags[9] = 1");
                    SetGlobalFlag(9, true);
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
