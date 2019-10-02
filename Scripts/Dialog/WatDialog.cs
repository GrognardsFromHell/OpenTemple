
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
    [DialogScript(113)]
    public class WatDialog : Wat, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    Trace.Assert(originalScript == "game.party_pc_size() > 1");
                    return GameSystems.Party.PlayerCharactersSize > 1;
                case 12:
                    Trace.Assert(originalScript == "game.party_pc_size() <= 1");
                    return GameSystems.Party.PlayerCharactersSize <= 1;
                case 22:
                case 232:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 61:
                    Trace.Assert(originalScript == "game.global_flags[96] == 1");
                    return GetGlobalFlag(96);
                case 72:
                case 272:
                    Trace.Assert(originalScript == "game.quests[39].state == qs_unknown and game.global_flags[91] == 1 and game.global_flags[92] == 1 and pc.skill_level_get(npc, skill_bluff) >= 10");
                    return GetQuestState(39) == QuestState.Unknown && GetGlobalFlag(91) && GetGlobalFlag(92) && pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 73:
                case 273:
                    Trace.Assert(originalScript == "game.quests[39].state == qs_accepted and pc.skill_level_get(npc, skill_bluff) >= 10");
                    return GetQuestState(39) == QuestState.Accepted && pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 74:
                case 274:
                    Trace.Assert(originalScript == "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 10 and game.global_flags[88] == 0");
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 10 && !GetGlobalFlag(88);
                case 75:
                case 275:
                    Trace.Assert(originalScript == "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 10 and game.global_flags[88] == 1");
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 10 && GetGlobalFlag(88);
                case 76:
                case 276:
                    Trace.Assert(originalScript == "game.quests[38].state == qs_accepted and game.global_flags[95] == 0");
                    return GetQuestState(38) == QuestState.Accepted && !GetGlobalFlag(95);
                case 77:
                case 277:
                    Trace.Assert(originalScript == "game.global_flags[95] == 1");
                    return GetGlobalFlag(95);
                case 91:
                    Trace.Assert(originalScript == "pc.money_get() >= 5");
                    return pc.GetMoney() >= 5;
                case 92:
                    Trace.Assert(originalScript == "pc.money_get() < 5");
                    return pc.GetMoney() < 5;
                case 101:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 181:
                    Trace.Assert(originalScript == "game.quests[38].state == qs_completed");
                    return GetQuestState(38) == QuestState.Completed;
                case 182:
                    Trace.Assert(originalScript == "game.global_flags[96] == 1 and game.global_flags[92] == 0");
                    return GetGlobalFlag(96) && !GetGlobalFlag(92);
                case 183:
                    Trace.Assert(originalScript == "game.quests[38].state == qs_accepted");
                    return GetQuestState(38) == QuestState.Accepted;
                case 211:
                    Trace.Assert(originalScript == "game.global_flags[92] == 1");
                    return GetGlobalFlag(92);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 60:
                    Trace.Assert(originalScript == "game.areas[4] = 1; game.story_state = 4; game.global_flags[92] = 1");
                    MakeAreaKnown(4);
                    StoryState = 4;
                    SetGlobalFlag(92, true);
                    ;
                    break;
                case 91:
                    Trace.Assert(originalScript == "pc.money_adj(-5)");
                    pc.AdjustMoney(-5);
                    break;
                case 101:
                    Trace.Assert(originalScript == "game.quests[39].state = qs_mentioned");
                    SetQuestState(39, QuestState.Mentioned);
                    break;
                case 121:
                    Trace.Assert(originalScript == "game.quests[39].state = qs_completed; kill_dick(npc)");
                    SetQuestState(39, QuestState.Completed);
                    kill_dick(npc);
                    ;
                    break;
                case 182:
                    Trace.Assert(originalScript == "game.global_flags[92] = 1");
                    SetGlobalFlag(92, true);
                    break;
                case 210:
                    Trace.Assert(originalScript == "create_item_in_inventory( 8004, pc )");
                    Utilities.create_item_in_inventory(8004, pc);
                    break;
                case 310:
                    Trace.Assert(originalScript == "game.global_flags[499] = 1");
                    SetGlobalFlag(499, true);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
