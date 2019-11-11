
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
    [DialogScript(63)]
    public class ReclusiveFarmerDialog : ReclusiveFarmer, IDialogScript
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
                    originalScript = "game.global_flags[26] == 1 and npc.has_met( pc ) and game.quests[12].state == qs_unknown";
                    return GetGlobalFlag(26) && npc.HasMet(pc) && GetQuestState(12) == QuestState.Unknown;
                case 6:
                case 7:
                case 18:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 8:
                case 9:
                    originalScript = "game.quests[12].state == qs_mentioned and pc.stat_level_get( stat_gender ) == gender_male";
                    return GetQuestState(12) == QuestState.Mentioned && pc.GetGender() == Gender.Male;
                case 10:
                case 11:
                    originalScript = "game.quests[12].state == qs_completed";
                    return GetQuestState(12) == QuestState.Completed;
                case 12:
                case 13:
                    originalScript = "game.quests[12].state == qs_accepted and game.global_flags[42] == 0";
                    return GetQuestState(12) == QuestState.Accepted && !GetGlobalFlag(42);
                case 14:
                case 15:
                    originalScript = "game.quests[12].state == qs_accepted and game.global_flags[42] == 1 and game.global_flags[26] == 0";
                    return GetQuestState(12) == QuestState.Accepted && GetGlobalFlag(42) && !GetGlobalFlag(26);
                case 16:
                case 17:
                    originalScript = "game.quests[12].state == qs_accepted and game.global_flags[42] == 1 and game.global_flags[26] == 1";
                    return GetQuestState(12) == QuestState.Accepted && GetGlobalFlag(42) && GetGlobalFlag(26);
                case 21:
                case 22:
                    originalScript = "game.global_flags[26] == 1 and game.quests[12].state == qs_unknown";
                    return GetGlobalFlag(26) && GetQuestState(12) == QuestState.Unknown;
                case 53:
                case 54:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 2 and game.quests[12].state == qs_unknown";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 2 && GetQuestState(12) == QuestState.Unknown;
                case 63:
                case 64:
                case 138:
                case 139:
                case 153:
                case 154:
                case 172:
                case 173:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetGender() == Gender.Male;
                case 65:
                case 66:
                case 142:
                case 143:
                case 151:
                case 152:
                case 174:
                case 175:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 136:
                case 137:
                    originalScript = "pc.stat_level_get( stat_deity ) != 16";
                    return pc.GetStat(Stat.deity) != 16;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 8:
                case 9:
                case 63:
                case 64:
                    originalScript = "game.quests[12].state = qs_accepted";
                    SetQuestState(12, QuestState.Accepted);
                    break;
                case 60:
                case 135:
                    originalScript = "game.quests[12].state = qs_mentioned";
                    SetQuestState(12, QuestState.Mentioned);
                    break;
                case 80:
                    originalScript = "make_like( npc, pc )";
                    make_like(npc, pc);
                    break;
                case 115:
                    originalScript = "game.global_flags[43] = 1";
                    SetGlobalFlag(43, true);
                    break;
                case 138:
                case 139:
                case 153:
                case 154:
                case 172:
                case 173:
                    originalScript = "game.global_flags[43] = 1; game.quests[12].state = qs_accepted";
                    SetGlobalFlag(43, true);
                    SetQuestState(12, QuestState.Accepted);
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
                case 53:
                case 54:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 2);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
