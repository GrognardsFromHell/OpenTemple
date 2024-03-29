
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

[DialogScript(114)]
public class PearlDialog : Pearl, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 23:
            case 24:
                originalScript = "game.quests[38].state == qs_unknown";
                return GetQuestState(38) == QuestState.Unknown;
            case 25:
            case 26:
                originalScript = "game.quests[38].state == qs_mentioned and pc.skill_level_get(npc, skill_diplomacy) >= 6";
                return GetQuestState(38) == QuestState.Mentioned && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
            case 36:
                originalScript = "pc.money_get() >= 5";
                return pc.GetMoney() >= 5;
            case 37:
            case 38:
                originalScript = "pc.money_get() < 5";
                return pc.GetMoney() < 5;
            case 43:
            case 44:
                originalScript = "pc.skill_level_get(npc, skill_gather_information) >= 6";
                return pc.GetSkillLevel(npc, SkillId.gather_information) >= 6;
            case 65:
            case 66:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 6";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
            case 111:
            case 112:
                originalScript = "game.global_flags[95] == 0";
                return !GetGlobalFlag(95);
            case 113:
            case 114:
                originalScript = "game.global_flags[95] == 1";
                return GetGlobalFlag(95);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 36:
                originalScript = "pc.money_adj(-5)";
                pc.AdjustMoney(-5);
                break;
            case 60:
                originalScript = "game.quests[38].state = qs_mentioned";
                SetQuestState(38, QuestState.Mentioned);
                break;
            case 90:
                originalScript = "game.quests[38].state = qs_accepted";
                SetQuestState(38, QuestState.Accepted);
                break;
            case 160:
                originalScript = "game.quests[38].state = qs_completed";
                SetQuestState(38, QuestState.Completed);
                break;
            case 170:
                originalScript = "create_item_in_inventory( 8004, pc )";
                Utilities.create_item_in_inventory(8004, pc);
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
            case 25:
            case 26:
            case 65:
            case 66:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                return true;
            case 43:
            case 44:
                skillChecks = new DialogSkillChecks(SkillId.gather_information, 6);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}