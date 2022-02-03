
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
    [DialogScript(11)]
    public class FarmerWifeDialog : FarmerWife, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 4:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 5:
                    originalScript = "npc.has_met( pc ) and game.quests[6].state <= qs_mentioned";
                    return npc.HasMet(pc) && GetQuestState(6) <= QuestState.Mentioned;
                case 6:
                    originalScript = "npc.has_met( pc ) and game.quests[6].state >= qs_accepted";
                    return npc.HasMet(pc) && GetQuestState(6) >= QuestState.Accepted;
                case 13:
                case 18:
                    originalScript = "game.global_flags[67] == 0";
                    return !GetGlobalFlag(67);
                case 101:
                case 102:
                    originalScript = "game.quests[6].state == qs_mentioned";
                    return GetQuestState(6) == QuestState.Mentioned;
                case 103:
                case 104:
                    originalScript = "game.global_flags[2] == 0";
                    return !GetGlobalFlag(2);
                case 105:
                case 106:
                    originalScript = "game.quests[6].state == qs_unknown and game.global_flags[2] == 1";
                    return GetQuestState(6) == QuestState.Unknown && GetGlobalFlag(2);
                case 122:
                case 123:
                    originalScript = "game.global_flags[14] == 1";
                    return GetGlobalFlag(14);
                case 501:
                case 510:
                    originalScript = "game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    originalScript = "game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 20:
                    originalScript = "game.global_flags[2] = 1";
                    SetGlobalFlag(2, true);
                    break;
                case 40:
                case 84:
                    originalScript = "game.quests[6].state = qs_mentioned";
                    SetQuestState(6, QuestState.Mentioned);
                    break;
                case 41:
                case 42:
                case 43:
                case 85:
                case 86:
                case 101:
                case 102:
                    originalScript = "game.quests[6].state = qs_accepted";
                    SetQuestState(6, QuestState.Accepted);
                    break;
                case 110:
                    originalScript = "npc.reaction_adj( pc,+10)";
                    npc.AdjustReaction(pc, +10);
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
