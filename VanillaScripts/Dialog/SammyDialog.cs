
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

namespace VanillaScripts.Dialog
{
    [DialogScript(101)]
    public class SammyDialog : Sammy, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                    originalScript = "pc.money_get() < 50000";
                    return pc.GetMoney() < 50000;
                case 43:
                case 44:
                    originalScript = "pc.money_get() >= 50000";
                    return pc.GetMoney() >= 50000;
                case 47:
                case 48:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 6 and pc.money_get() >= 20000";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6 && pc.GetMoney() >= 20000;
                case 49:
                case 50:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 6;
                case 71:
                case 72:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 101:
                case 104:
                    originalScript = "game.quests[32].state == qs_mentioned";
                    return GetQuestState(32) == QuestState.Mentioned;
                case 102:
                case 105:
                    originalScript = "game.quests[32].state == qs_accepted";
                    return GetQuestState(32) == QuestState.Accepted;
                case 103:
                case 106:
                    originalScript = "game.quests[32].state == qs_completed";
                    return GetQuestState(32) == QuestState.Completed;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    originalScript = "game.quests[32].state = qs_mentioned";
                    SetQuestState(32, QuestState.Mentioned);
                    break;
                case 43:
                case 44:
                    originalScript = "pc.money_adj(-50000)";
                    pc.AdjustMoney(-50000);
                    break;
                case 47:
                case 48:
                    originalScript = "pc.money_adj(-20000)";
                    pc.AdjustMoney(-20000);
                    break;
                case 60:
                    originalScript = "game.quests[32].state = qs_accepted";
                    SetQuestState(32, QuestState.Accepted);
                    break;
                case 71:
                case 72:
                    originalScript = "pc.money_adj(-10000)";
                    pc.AdjustMoney(-10000);
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
                case 47:
                case 48:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                case 49:
                case 50:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
