
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

namespace VanillaScripts.Dialog
{
    [DialogScript(94)]
    public class LaszloDialog : Laszlo, IDialogScript
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
                    originalScript = "npc.has_met(pc) and (game.quests[21].state <= qs_mentioned or pc.stat_level_get( stat_gender ) == gender_male)";
                    return npc.HasMet(pc) && (GetQuestState(21) <= QuestState.Mentioned || pc.GetGender() == Gender.Male);
                case 6:
                case 7:
                    originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] == 1";
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 1;
                case 8:
                case 9:
                    originalScript = "game.quests[21].state == qs_accepted and (game.global_vars[9] == 2 or game.global_vars[9] == 3) and pc.stat_level_get( stat_gender ) == gender_female";
                    return GetQuestState(21) == QuestState.Accepted && (GetGlobalVar(9) == 2 || GetGlobalVar(9) == 3) && pc.GetGender() == Gender.Female;
                case 10:
                case 11:
                    originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] == 4 and pc.stat_level_get( stat_gender ) == gender_female";
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 4 && pc.GetGender() == Gender.Female;
                case 12:
                case 13:
                    originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] == 5 and pc.stat_level_get( stat_gender ) == gender_female";
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 5 && pc.GetGender() == Gender.Female;
                case 14:
                case 15:
                    originalScript = "game.quests[21].state == qs_accepted and game.global_vars[9] >= 6 and pc.stat_level_get( stat_gender ) == gender_female";
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) >= 6 && pc.GetGender() == Gender.Female;
                case 16:
                    originalScript = "game.quests[21].state >= qs_completed and game.global_flags[68] == 0";
                    return GetQuestState(21) >= QuestState.Completed && !GetGlobalFlag(68);
                case 17:
                case 18:
                    originalScript = "game.quests[21].state >= qs_completed and game.global_flags[68] == 1";
                    return GetQuestState(21) >= QuestState.Completed && GetGlobalFlag(68);
                case 19:
                    originalScript = "npc.has_met(pc) and game.quests[21].state <= qs_accepted";
                    return npc.HasMet(pc) && GetQuestState(21) <= QuestState.Accepted;
                case 23:
                    originalScript = "game.party_alignment != TRUE_NEUTRAL";
                    return PartyAlignment != Alignment.NEUTRAL;
                case 25:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 41:
                case 42:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 12 and pc.stat_level_get( stat_gender ) == gender_female and game.quests[21].state == qs_unknown";
                    return pc.GetStat(Stat.charisma) >= 12 && pc.GetGender() == Gender.Female && GetQuestState(21) == QuestState.Unknown;
                case 46:
                case 47:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 12 and pc.stat_level_get( stat_gender ) == gender_female and game.quests[21].state == qs_mentioned";
                    return pc.GetStat(Stat.charisma) >= 12 && pc.GetGender() == Gender.Female && GetQuestState(21) == QuestState.Mentioned;
                case 153:
                case 156:
                    originalScript = "game.story_state == 2";
                    return StoryState == 2;
                case 154:
                case 157:
                    originalScript = "game.story_state == 3";
                    return StoryState == 3;
                case 155:
                case 158:
                    originalScript = "game.story_state >= 4";
                    return StoryState >= 4;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 90:
                    originalScript = "game.quests[21].state = qs_mentioned";
                    SetQuestState(21, QuestState.Mentioned);
                    break;
                case 101:
                case 102:
                    originalScript = "game.quests[21].state = qs_accepted";
                    SetQuestState(21, QuestState.Accepted);
                    break;
                case 110:
                    originalScript = "game.global_vars[9] = 1";
                    SetGlobalVar(9, 1);
                    break;
                case 220:
                    originalScript = "game.global_flags[69] = 1; npc.reaction_adj( pc,+20)";
                    SetGlobalFlag(69, true);
                    npc.AdjustReaction(pc, +20);
                    ;
                    break;
                case 260:
                    originalScript = "game.quests[21].state = qs_completed";
                    SetQuestState(21, QuestState.Completed);
                    break;
                case 280:
                    originalScript = "game.quests[21].state = qs_botched; npc.reaction_adj( pc,-30)";
                    SetQuestState(21, QuestState.Botched);
                    npc.AdjustReaction(pc, -30);
                    ;
                    break;
                case 290:
                    originalScript = "game.quests[21].state = qs_completed; game.global_flags[68] = 1; npc.reaction_adj( pc,+50)";
                    SetQuestState(21, QuestState.Completed);
                    SetGlobalFlag(68, true);
                    npc.AdjustReaction(pc, +50);
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
