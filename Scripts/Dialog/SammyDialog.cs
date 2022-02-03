
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

namespace Scripts.Dialog
{
    [DialogScript(101)]
    public class SammyDialog : Sammy, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                    originalScript = "pc.money_get() < 50000";
                    return pc.GetMoney() < 50000;
                case 43:
                case 44:
                case 221:
                case 222:
                    originalScript = "pc.money_get() >= 50000";
                    return pc.GetMoney() >= 50000;
                case 45:
                case 46:
                case 223:
                case 224:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 6 and pc.money_get() >= 20000";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6 && pc.GetMoney() >= 20000;
                case 101:
                case 104:
                    originalScript = "(game.quests[32].state == qs_mentioned) and (game.quests[63].state != qs_mentioned)";
                    return (GetQuestState(32) == QuestState.Mentioned) && (GetQuestState(63) != QuestState.Mentioned);
                case 102:
                case 105:
                    originalScript = "game.quests[32].state == qs_accepted";
                    return GetQuestState(32) == QuestState.Accepted;
                case 103:
                case 106:
                    originalScript = "game.quests[32].state == qs_completed";
                    return GetQuestState(32) == QuestState.Completed;
                case 107:
                case 108:
                    originalScript = "(game.quests[32].state == qs_mentioned) and (game.quests[63].state == qs_mentioned)";
                    return (GetQuestState(32) == QuestState.Mentioned) && (GetQuestState(63) == QuestState.Mentioned);
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
                    originalScript = "game.quests[32].state = qs_mentioned";
                    SetQuestState(32, QuestState.Mentioned);
                    break;
                case 43:
                case 44:
                case 221:
                case 222:
                    originalScript = "pc.money_adj(-50000)";
                    pc.AdjustMoney(-50000);
                    break;
                case 45:
                case 46:
                case 223:
                case 224:
                    originalScript = "pc.money_adj(-20000)";
                    pc.AdjustMoney(-20000);
                    break;
                case 60:
                    originalScript = "game.quests[32].state = qs_accepted";
                    SetQuestState(32, QuestState.Accepted);
                    break;
                case 70:
                    originalScript = "game.quests[63].state = qs_mentioned";
                    SetQuestState(63, QuestState.Mentioned);
                    break;
                case 180:
                case 190:
                    originalScript = "game.quests[63].state = qs_accepted";
                    SetQuestState(63, QuestState.Accepted);
                    break;
                case 191:
                case 192:
                    originalScript = "pc.money_adj(5000)";
                    pc.AdjustMoney(5000);
                    break;
                case 200:
                    originalScript = "game.quests[32].state = qs_botched";
                    SetQuestState(32, QuestState.Botched);
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
                case 45:
                case 46:
                case 223:
                case 224:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
