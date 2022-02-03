
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

namespace Scripts.Dialog;

[DialogScript(19)]
public class StonemasonDialog : Stonemason, IDialogScript
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
            case 6:
                originalScript = "game.global_flags[67] == 0 and not npc.has_met( pc )";
                return !GetGlobalFlag(67) && !npc.HasMet(pc);
            case 7:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 21:
            case 31:
                originalScript = "game.quests[5].state == qs_mentioned or game.quests[5] == qs_accepted";
                return GetQuestState(5) == QuestState.Mentioned || GetQuestState(5) == QuestState.Accepted;
            case 101:
            case 102:
                originalScript = "game.story_state >= 2 and game.areas[3] == 0";
                return StoryState >= 2 && !IsAreaKnown(3);
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
            case 41:
                originalScript = "npc.reaction_adj( pc,+10)";
                npc.AdjustReaction(pc, +10);
                break;
            case 50:
                originalScript = "npc.reaction_adj( pc,-20)";
                npc.AdjustReaction(pc, -20);
                break;
            case 110:
                originalScript = "game.areas[3] = 1; game.story_state = 3";
                MakeAreaKnown(3);
                StoryState = 3;
                ;
                break;
            case 111:
            case 112:
                originalScript = "game.worldmap_travel_by_dialog(3)";
                WorldMapTravelByDialog(3);
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