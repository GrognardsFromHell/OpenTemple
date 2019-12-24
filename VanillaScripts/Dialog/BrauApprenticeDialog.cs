
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
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
    [DialogScript(64)]
    public class BrauApprenticeDialog : BrauApprentice, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "game.quests[14].state == qs_accepted and npc.has_met( pc ) and game.global_flags[29] == 0";
                    return GetQuestState(14) == QuestState.Accepted && npc.HasMet(pc) && !GetGlobalFlag(29);
                case 6:
                case 7:
                case 16:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 8:
                case 9:
                    originalScript = "game.quests[14].state == qs_completed and game.global_flags[30] == 0";
                    return GetQuestState(14) == QuestState.Completed && !GetGlobalFlag(30);
                case 10:
                case 11:
                    originalScript = "game.quests[14].state != qs_completed and game.global_flags[29] == 1 and game.global_flags[30] == 0";
                    return GetQuestState(14) != QuestState.Completed && GetGlobalFlag(29) && !GetGlobalFlag(30);
                case 12:
                case 13:
                    originalScript = "game.quests[14].state <= qs_accepted and game.global_flags[29] == 1 and game.global_flags[30] == 1";
                    return GetQuestState(14) <= QuestState.Accepted && GetGlobalFlag(29) && GetGlobalFlag(30);
                case 14:
                case 15:
                    originalScript = "game.quests[14].state == qs_botched and game.global_flags[29] == 1 and game.global_flags[30] == 1";
                    return GetQuestState(14) == QuestState.Botched && GetGlobalFlag(29) && GetGlobalFlag(30);
                case 24:
                case 25:
                    originalScript = "game.quests[14].state == qs_accepted";
                    return GetQuestState(14) == QuestState.Accepted;
                case 52:
                case 53:
                    originalScript = "game.quests[14].state == qs_unknown and pc.skill_level_get(npc,skill_gather_information) >= 4";
                    return GetQuestState(14) == QuestState.Unknown && pc.GetSkillLevel(npc, SkillId.gather_information) >= 4;
                case 71:
                case 72:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 3";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 3;
                case 122:
                case 123:
                    originalScript = "pc.money_get() >= 1";
                    return pc.GetMoney() >= 1;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 70:
                    originalScript = "game.global_flags[29] = 1";
                    SetGlobalFlag(29, true);
                    break;
                case 71:
                case 72:
                    originalScript = "game.global_flags[30] = 1";
                    SetGlobalFlag(30, true);
                    break;
                case 122:
                case 123:
                    originalScript = "pc.money_adj(-1)";
                    pc.AdjustMoney(-1);
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
                case 52:
                case 53:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 4);
                    return true;
                case 71:
                case 72:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 3);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
