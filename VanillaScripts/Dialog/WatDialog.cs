
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
    [DialogScript(113)]
    public class WatDialog : Wat, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    originalScript = "game.party_pc_size() > 1";
                    return GameSystems.Party.PlayerCharactersSize > 1;
                case 13:
                case 14:
                    originalScript = "game.party_pc_size() <= 1";
                    return GameSystems.Party.PlayerCharactersSize <= 1;
                case 22:
                case 24:
                case 232:
                case 234:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 6;
                case 61:
                case 62:
                    originalScript = "game.global_flags[96] == 1";
                    return GetGlobalFlag(96);
                case 73:
                case 74:
                    originalScript = "game.quests[39].state == qs_unknown and game.global_flags[91] == 1 and game.global_flags[92] == 1 and pc.skill_level_get(npc, skill_bluff) >= 8";
                    return GetQuestState(39) == QuestState.Unknown && GetGlobalFlag(91) && GetGlobalFlag(92) && pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 75:
                case 76:
                    originalScript = "game.quests[39].state == qs_accepted and pc.skill_level_get(npc, skill_bluff) >= 8";
                    return GetQuestState(39) == QuestState.Accepted && pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 77:
                case 78:
                    originalScript = "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 8 and game.global_flags[88] == 0";
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 8 && !GetGlobalFlag(88);
                case 79:
                case 80:
                    originalScript = "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 8 and game.global_flags[88] == 1";
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 8 && GetGlobalFlag(88);
                case 81:
                case 82:
                    originalScript = "game.quests[38].state == qs_accepted and game.global_flags[95] == 0";
                    return GetQuestState(38) == QuestState.Accepted && !GetGlobalFlag(95);
                case 83:
                case 84:
                    originalScript = "game.global_flags[95] == 1";
                    return GetGlobalFlag(95);
                case 91:
                case 93:
                    originalScript = "pc.money_get() >= 5";
                    return pc.GetMoney() >= 5;
                case 92:
                case 94:
                    originalScript = "pc.money_get() < 5";
                    return pc.GetMoney() < 5;
                case 101:
                case 102:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 181:
                case 183:
                    originalScript = "game.quests[38].state == qs_completed";
                    return GetQuestState(38) == QuestState.Completed;
                case 182:
                case 184:
                    originalScript = "game.global_flags[96] == 1 and game.global_flags[92] == 0";
                    return GetGlobalFlag(96) && !GetGlobalFlag(92);
                case 185:
                case 186:
                    originalScript = "game.quests[38].state == qs_accepted";
                    return GetQuestState(38) == QuestState.Accepted;
                case 211:
                case 212:
                    originalScript = "game.global_flags[92] == 1";
                    return GetGlobalFlag(92);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
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
                case 93:
                    originalScript = "pc.money_adj(-5)";
                    pc.AdjustMoney(-5);
                    break;
                case 101:
                case 102:
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
                case 184:
                    originalScript = "game.global_flags[92] = 1";
                    SetGlobalFlag(92, true);
                    break;
                case 210:
                    originalScript = "create_item_in_inventory( 8004, pc )";
                    Utilities.create_item_in_inventory(8004, pc);
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
                case 24:
                case 232:
                case 234:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 6);
                    return true;
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 78:
                case 79:
                case 80:
                case 101:
                case 102:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
