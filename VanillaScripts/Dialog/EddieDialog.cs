
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

[DialogScript(96)]
public class EddieDialog : Eddie, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "not npc.has_met(pc)";
                return !npc.HasMet(pc);
            case 4:
            case 5:
                originalScript = "npc.has_met(pc) and (game.quests[21].state <= qs_mentioned or pc.stat_level_get( stat_gender ) == gender_male)";
                return npc.HasMet(pc) && (GetQuestState(21) <= QuestState.Mentioned || pc.GetGender() == Gender.Male);
            case 6:
            case 7:
                originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] == 1 and pc.stat_level_get( stat_gender ) == gender_female";
                return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 1 && pc.GetGender() == Gender.Female;
            case 8:
            case 9:
                originalScript = "game.quests[21].state == qs_accepted and (game.global_vars[9] == 2 or game.global_vars[9] == 3) and pc.stat_level_get( stat_gender ) == gender_female";
                return GetQuestState(21) == QuestState.Accepted && (GetGlobalVar(9) == 2 || GetGlobalVar(9) == 3) && pc.GetGender() == Gender.Female;
            case 10:
            case 11:
                originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] == 4 and pc.stat_level_get( stat_gender ) == gender_female";
                return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 4 && pc.GetGender() == Gender.Female;
            case 12:
            case 13:
                originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] == 5 and pc.stat_level_get( stat_gender ) == gender_female";
                return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 5 && pc.GetGender() == Gender.Female;
            case 14:
            case 15:
                originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] >= 6 and pc.stat_level_get( stat_gender ) == gender_female";
                return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) >= 6 && pc.GetGender() == Gender.Female;
            case 16:
                originalScript = "npc.has_met(pc)";
                return npc.HasMet(pc);
            case 17:
            case 18:
                originalScript = "game.quests[21].state == qs_completed and game.global_flags[68] == 1";
                return GetQuestState(21) == QuestState.Completed && GetGlobalFlag(68);
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
                originalScript = "game.global_vars[9] = 2";
                SetGlobalVar(9, 2);
                break;
            case 90:
                originalScript = "npc.reaction_adj( pc,+10)";
                npc.AdjustReaction(pc, +10);
                break;
            case 100:
                originalScript = "npc.reaction_adj( pc,+20)";
                npc.AdjustReaction(pc, +20);
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