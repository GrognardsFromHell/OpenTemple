
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

[DialogScript(13)]
public class MillerServantDialog : MillerServant, IDialogScript
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
                originalScript = "game.quests[10].state == qs_mentioned";
                return GetQuestState(10) == QuestState.Mentioned;
            case 6:
                originalScript = "game.quests[10].state == qs_accepted";
                return GetQuestState(10) == QuestState.Accepted;
            case 7:
                originalScript = "game.quests[10].state == qs_completed and game.global_flags[302] == 1";
                return GetQuestState(10) == QuestState.Completed && GetGlobalFlag(302);
            case 8:
                originalScript = "game.quests[10].state == qs_completed and game.global_flags[302] == 0";
                return GetQuestState(10) == QuestState.Completed && !GetGlobalFlag(302);
            case 9:
                originalScript = "npc.has_met(pc)";
                return npc.HasMet(pc);
            case 38:
            case 39:
                originalScript = "game.global_flags[24] == 1";
                return GetGlobalFlag(24);
            case 41:
            case 44:
            case 62:
                originalScript = "game.quests[8].state == qs_completed";
                return GetQuestState(8) == QuestState.Completed;
            case 42:
            case 45:
                originalScript = "game.quests[8].state != qs_completed";
                return GetQuestState(8) != QuestState.Completed;
            case 71:
            case 74:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 2 and game.global_flags[15] == 0";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 2 && !GetGlobalFlag(15);
            case 72:
            case 75:
                originalScript = "game.global_flags[15] == 1";
                return GetGlobalFlag(15);
            case 73:
            case 76:
                originalScript = "game.global_flags[15] == 0";
                return !GetGlobalFlag(15);
            case 502:
                originalScript = "game.quests[10].state == qs_mentioned or game.quests[10].state == qs_accepted";
                return GetQuestState(10) == QuestState.Mentioned || GetQuestState(10) == QuestState.Accepted;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 40:
                originalScript = "game.quests[10].state = qs_mentioned";
                SetQuestState(10, QuestState.Mentioned);
                break;
            case 60:
                originalScript = "game.quests[10].state = qs_accepted";
                SetQuestState(10, QuestState.Accepted);
                break;
            case 71:
            case 74:
                originalScript = "beggar_soon(npc, pc)";
                beggar_soon(npc, pc);
                break;
            case 80:
                originalScript = "complete_quest(npc,pc)";
                complete_quest(npc, pc);
                break;
            case 202:
                originalScript = "npc.obj_set_int( obj_f_npc_pad_i_5, 1)";
                npc.SetInt(obj_f.npc_pad_i_5, 1);
                break;
            case 502:
                originalScript = "game.quests[10].state = qs_botched";
                SetQuestState(10, QuestState.Botched);
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
            case 71:
            case 74:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 2);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}