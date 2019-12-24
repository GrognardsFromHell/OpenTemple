
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
    [DialogScript(20)]
    public class TailorDialog : Tailor, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 6:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 3:
                    originalScript = "npc.has_met( pc ) and game.quests[1].state != qs_completed";
                    return npc.HasMet(pc) && GetQuestState(1) != QuestState.Completed;
                case 4:
                    originalScript = "npc.has_met( pc ) and game.quests[1].state == qs_completed";
                    return npc.HasMet(pc) && GetQuestState(1) == QuestState.Completed;
                case 5:
                case 7:
                    originalScript = "game.global_vars[1] == 1";
                    return GetGlobalVar(1) == 1;
                case 22:
                    originalScript = "pc.stat_level_get(stat_race) == race_dwarf or pc.stat_level_get(stat_race) == race_halfling or pc.stat_level_get(stat_race) == race_gnome";
                    return pc.GetRace() == RaceId.derro || pc.GetRace() == RaceId.tallfellow || pc.GetRace() == RaceId.svirfneblin;
                case 101:
                case 106:
                    originalScript = "game.quests[1].state == qs_unknown";
                    return GetQuestState(1) == QuestState.Unknown;
                case 102:
                case 107:
                    originalScript = "game.quests[1].state == qs_mentioned";
                    return GetQuestState(1) == QuestState.Mentioned;
                case 105:
                case 108:
                case 113:
                case 116:
                    originalScript = "game.story_state >= 2 and game.areas[3] == 0";
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 114:
                case 117:
                    originalScript = "game.global_vars[1] == 3";
                    return GetGlobalVar(1) == 3;
                case 115:
                case 118:
                    originalScript = "game.global_vars[1] == 2";
                    return GetGlobalVar(1) == 2;
                case 121:
                case 131:
                    originalScript = "game.quests[1].state != qs_completed";
                    return GetQuestState(1) != QuestState.Completed;
                case 122:
                case 132:
                    originalScript = "game.quests[1].state == qs_completed";
                    return GetQuestState(1) == QuestState.Completed;
                case 152:
                case 154:
                case 182:
                case 184:
                    originalScript = "pc.money_get() >= 2000";
                    return pc.GetMoney() >= 2000;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 7:
                    originalScript = "game.global_vars[1] = 2";
                    SetGlobalVar(1, 2);
                    break;
                case 40:
                    originalScript = "game.quests[1].state = qs_mentioned";
                    SetQuestState(1, QuestState.Mentioned);
                    break;
                case 51:
                case 53:
                    originalScript = "game.quests[1].state = qs_accepted";
                    SetQuestState(1, QuestState.Accepted);
                    break;
                case 130:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 133:
                case 134:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 152:
                case 154:
                case 182:
                case 184:
                    originalScript = "game.global_vars[1] = 3; pc.money_adj(-2000)";
                    SetGlobalVar(1, 3);
                    pc.AdjustMoney(-2000);
                    ;
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
}
