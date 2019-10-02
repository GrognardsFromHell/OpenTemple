
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

namespace Scripts.Dialog
{
    [DialogScript(114)]
    public class PearlDialog : Pearl, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 24:
                    Trace.Assert(originalScript == "game.quests[38].state == qs_unknown");
                    return GetQuestState(38) == QuestState.Unknown;
                case 25:
                case 26:
                    Trace.Assert(originalScript == "game.quests[38].state == qs_mentioned and pc.skill_level_get(npc, skill_diplomacy) >= 6");
                    return GetQuestState(38) == QuestState.Mentioned && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 36:
                    Trace.Assert(originalScript == "pc.money_get() >= 5");
                    return pc.GetMoney() >= 5;
                case 37:
                case 38:
                    Trace.Assert(originalScript == "pc.money_get() < 5");
                    return pc.GetMoney() < 5;
                case 43:
                case 44:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_gather_information) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 6;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "game.global_flags[95] == 0");
                    return !GetGlobalFlag(95);
                case 113:
                case 114:
                    Trace.Assert(originalScript == "game.global_flags[95] == 1");
                    return GetGlobalFlag(95);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 36:
                    Trace.Assert(originalScript == "pc.money_adj(-5)");
                    pc.AdjustMoney(-5);
                    break;
                case 60:
                    Trace.Assert(originalScript == "game.quests[38].state = qs_mentioned");
                    SetQuestState(38, QuestState.Mentioned);
                    break;
                case 90:
                    Trace.Assert(originalScript == "game.quests[38].state = qs_accepted");
                    SetQuestState(38, QuestState.Accepted);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.quests[38].state = qs_completed");
                    SetQuestState(38, QuestState.Completed);
                    break;
                case 170:
                    Trace.Assert(originalScript == "create_item_in_inventory( 8004, pc )");
                    Utilities.create_item_in_inventory(8004, pc);
                    break;
                default:
                    Trace.Assert(originalScript == null);
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
}
