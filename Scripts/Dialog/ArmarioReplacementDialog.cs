
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

[DialogScript(36)]
public class ArmarioReplacementDialog : ArmarioReplacement, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "not npc.has_met( pc ) and game.global_vars[967] == 0";
                return !npc.HasMet(pc) && GetGlobalVar(967) == 0;
            case 4:
            case 5:
                originalScript = "npc.has_met( pc ) or game.global_vars[967] >= 1";
                return npc.HasMet(pc) || GetGlobalVar(967) >= 1;
            case 6:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 22:
            case 42:
                originalScript = "game.global_vars[967] == 0";
                return GetGlobalVar(967) == 0;
            case 23:
            case 43:
                originalScript = "game.quests[2].state != qs_completed";
                return GetQuestState(2) != QuestState.Completed;
            case 24:
            case 44:
                originalScript = "game.quests[2].state == qs_completed and game.global_vars[967] == 0";
                return GetQuestState(2) == QuestState.Completed && GetGlobalVar(967) == 0;
            case 25:
            case 45:
                originalScript = "game.global_vars[967] == 1";
                return GetGlobalVar(967) == 1;
            case 26:
            case 46:
                originalScript = "game.global_vars[967] == 2";
                return GetGlobalVar(967) == 2;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 320:
                originalScript = "game.global_vars[967] = 1";
                SetGlobalVar(967, 1);
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