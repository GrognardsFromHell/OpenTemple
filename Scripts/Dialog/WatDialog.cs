
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
    [DialogScript(113)]
    public class WatDialog : Wat, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    originalScript = "game.party_pc_size() > 1";
                    return GameSystems.Party.PlayerCharactersSize > 1;
                case 12:
                    originalScript = "game.party_pc_size() <= 1";
                    return GameSystems.Party.PlayerCharactersSize <= 1;
                case 22:
                case 232:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 61:
                    originalScript = "game.global_flags[96] == 1";
                    return GetGlobalFlag(96);
                case 72:
                case 272:
                    originalScript = "game.quests[39].state == qs_unknown and game.global_flags[91] == 1 and game.global_flags[92] == 1 and pc.skill_level_get(npc, skill_bluff) >= 10";
                    return GetQuestState(39) == QuestState.Unknown && GetGlobalFlag(91) && GetGlobalFlag(92) && pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 73:
                case 273:
                    originalScript = "game.quests[39].state == qs_accepted and pc.skill_level_get(npc, skill_bluff) >= 10";
                    return GetQuestState(39) == QuestState.Accepted && pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 74:
                case 274:
                    originalScript = "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 10 and game.global_flags[88] == 0";
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 10 && !GetGlobalFlag(88);
                case 75:
                case 275:
                    originalScript = "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 10 and game.global_flags[88] == 1";
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 10 && GetGlobalFlag(88);
                case 76:
                case 276:
                    originalScript = "game.quests[38].state == qs_accepted and game.global_flags[95] == 0";
                    return GetQuestState(38) == QuestState.Accepted && !GetGlobalFlag(95);
                case 77:
                case 277:
                    originalScript = "game.global_flags[95] == 1";
                    return GetGlobalFlag(95);
                case 91:
                    originalScript = "pc.money_get() >= 5";
                    return pc.GetMoney() >= 5;
                case 92:
                    originalScript = "pc.money_get() < 5";
                    return pc.GetMoney() < 5;
                case 101:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 181:
                    originalScript = "game.quests[38].state == qs_completed";
                    return GetQuestState(38) == QuestState.Completed;
                case 182:
                    originalScript = "game.global_flags[96] == 1 and game.global_flags[92] == 0";
                    return GetGlobalFlag(96) && !GetGlobalFlag(92);
                case 183:
                    originalScript = "game.quests[38].state == qs_accepted";
                    return GetQuestState(38) == QuestState.Accepted;
                case 211:
                    originalScript = "game.global_flags[92] == 1";
                    return GetGlobalFlag(92);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 60:
                    originalScript = "game.areas[4] = 1; game.story_state = 4; game.global_flags[92] = 1";
                    MakeAreaKnown(4);
                    StoryState = 4;
                    SetGlobalFlag(92, true);
                    ;
                    break;
                case 91:
                    originalScript = "pc.money_adj(-5)";
                    pc.AdjustMoney(-5);
                    break;
                case 101:
                    originalScript = "game.quests[39].state = qs_mentioned";
                    SetQuestState(39, QuestState.Mentioned);
                    break;
                case 121:
                    originalScript = "game.quests[39].state = qs_completed; kill_dick(npc)";
                    SetQuestState(39, QuestState.Completed);
                    kill_dick(npc);
                    ;
                    break;
                case 182:
                    originalScript = "game.global_flags[92] = 1";
                    SetGlobalFlag(92, true);
                    break;
                case 210:
                    originalScript = "create_item_in_inventory( 8004, pc )";
                    Utilities.create_item_in_inventory(8004, pc);
                    break;
                case 310:
                    originalScript = "game.global_flags[499] = 1";
                    SetGlobalFlag(499, true);
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
                case 22:
                case 232:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                    return true;
                case 72:
                case 73:
                case 74:
                case 75:
                case 101:
                case 272:
                case 273:
                case 274:
                case 275:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
