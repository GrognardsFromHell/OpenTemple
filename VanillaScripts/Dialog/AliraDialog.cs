
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
    [DialogScript(115)]
    public class AliraDialog : Alira, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetGender() == Gender.Male;
                case 4:
                case 5:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 11:
                case 12:
                case 31:
                case 32:
                    originalScript = "game.quests[38].state == qs_accepted";
                    return GetQuestState(38) == QuestState.Accepted;
                case 41:
                case 42:
                    originalScript = "game.global_flags[92] == 0";
                    return !GetGlobalFlag(92);
                case 43:
                case 44:
                    originalScript = "game.global_flags[92] == 1";
                    return GetGlobalFlag(92);
                case 51:
                case 52:
                case 71:
                case 82:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6;
                case 61:
                case 62:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 6;
                case 63:
                case 64:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                case 181:
                case 182:
                    originalScript = "game.global_flags[96] == 1";
                    return GetGlobalFlag(96);
                case 183:
                case 184:
                    originalScript = "game.quests[38].state == qs_completed";
                    return GetQuestState(38) == QuestState.Completed;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 91:
                case 121:
                    originalScript = "game.global_flags[95] = 1";
                    SetGlobalFlag(95, true);
                    break;
                case 161:
                    originalScript = "game.global_flags[95] = 1; game.global_flags[96] = 1; game.areas[4] = 1; game.story_state = 4";
                    SetGlobalFlag(95, true);
                    SetGlobalFlag(96, true);
                    MakeAreaKnown(4);
                    StoryState = 4;
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
                case 51:
                case 52:
                case 71:
                case 82:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                    return true;
                case 61:
                case 62:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 6);
                    return true;
                case 63:
                case 64:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
