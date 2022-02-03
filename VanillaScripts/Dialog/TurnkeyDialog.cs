
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

[DialogScript(153)]
public class TurnkeyDialog : Turnkey, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 21:
            case 22:
                originalScript = "game.story_state >= 5";
                return StoryState >= 5;
            case 31:
            case 32:
                originalScript = "game.quests[43].state >= qs_accepted";
                return GetQuestState(43) >= QuestState.Accepted;
            case 65:
            case 66:
                originalScript = "pc.skill_level_get(npc, skill_gather_information) >= 8 or pc.stat_level_get(stat_level_bard) >= 1";
                return pc.GetSkillLevel(npc, SkillId.gather_information) >= 8 || pc.GetStat(Stat.level_bard) >= 1;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
            case 23:
            case 24:
            case 41:
            case 42:
            case 61:
            case 62:
            case 101:
            case 102:
            case 114:
            case 115:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 51:
            case 52:
            case 63:
            case 64:
            case 103:
            case 104:
                originalScript = "game.global_flags[208] = 1";
                SetGlobalFlag(208, true);
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
            case 65:
            case 66:
                skillChecks = new DialogSkillChecks(SkillId.gather_information, 8);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}