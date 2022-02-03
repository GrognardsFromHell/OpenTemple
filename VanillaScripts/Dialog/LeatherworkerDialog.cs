
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

namespace VanillaScripts.Dialog;

[DialogScript(53)]
public class LeatherworkerDialog : Leatherworker, IDialogScript
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
            case 106:
            case 107:
                originalScript = "game.global_flags[34] == 1 and game.quests[11].state == qs_accepted and game.global_flags[16] == 0";
                return GetGlobalFlag(34) && GetQuestState(11) == QuestState.Accepted && !GetGlobalFlag(16);
            case 6:
                originalScript = "npc.has_met( pc ) and game.quests[11].state <= qs_accepted";
                return npc.HasMet(pc) && GetQuestState(11) <= QuestState.Accepted;
            case 7:
                originalScript = "npc.has_met( pc ) and game.quests[11].state == qs_completed";
                return npc.HasMet(pc) && GetQuestState(11) == QuestState.Completed;
            case 8:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 11:
            case 12:
                originalScript = "game.quests[11].state == qs_accepted";
                return GetQuestState(11) == QuestState.Accepted;
            case 103:
            case 104:
                originalScript = "game.global_flags[34] == 0 and game.global_flags[16] == 0 and game.quests[11].state == qs_accepted";
                return !GetGlobalFlag(34) && !GetGlobalFlag(16) && GetQuestState(11) == QuestState.Accepted;
            case 124:
            case 125:
                originalScript = "game.story_state >= 2 and game.areas[3] == 0";
                return StoryState >= 2 && !IsAreaKnown(3);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 30:
                originalScript = "game.global_flags[33] = 1";
                SetGlobalFlag(33, true);
                break;
            case 130:
                originalScript = "game.global_flags[16] = 1; npc.reaction_adj( pc,+30)";
                SetGlobalFlag(16, true);
                npc.AdjustReaction(pc, +30);
                ;
                break;
            case 140:
                originalScript = "game.areas[3] = 1; game.story_state = 3";
                MakeAreaKnown(3);
                StoryState = 3;
                ;
                break;
            case 141:
            case 142:
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