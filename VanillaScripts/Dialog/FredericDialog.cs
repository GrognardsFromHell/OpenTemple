
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
    [DialogScript(215)]
    public class FredericDialog : Frederic, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 5:
                case 62:
                case 63:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 43:
                case 44:
                case 91:
                case 92:
                    originalScript = "game.global_flags[97] == 0";
                    return !GetGlobalFlag(97);
                case 45:
                case 93:
                    originalScript = "game.global_flags[97] == 1";
                    return GetGlobalFlag(97);
                case 52:
                case 53:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL and game.quests[41].state == qs_accepted";
                    return npc.GetLeader() == null && GetQuestState(41) == QuestState.Accepted;
                case 54:
                case 55:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL and game.quests[41].state == qs_accepted";
                    return npc.GetLeader() != null && GetQuestState(41) == QuestState.Accepted;
                case 103:
                case 104:
                case 105:
                case 106:
                    originalScript = "game.quests[36].state <= qs_accepted";
                    return GetQuestState(36) <= QuestState.Accepted;
                case 107:
                case 108:
                    originalScript = "game.quests[36].state == qs_completed";
                    return GetQuestState(36) == QuestState.Completed;
                case 109:
                case 110:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL and npc.area != 3";
                    return npc.GetLeader() != null && npc.GetArea() != 3;
                case 111:
                case 112:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL and npc.area == 3";
                    return npc.GetLeader() != null && npc.GetArea() == 3;
                case 201:
                case 202:
                    originalScript = "game.quests[36].state == qs_completed and game.global_flags[90] == 0";
                    return GetQuestState(36) == QuestState.Completed && !GetGlobalFlag(90);
                case 203:
                case 204:
                    originalScript = "game.quests[36].state == qs_completed and game.global_flags[90] == 1";
                    return GetQuestState(36) == QuestState.Completed && GetGlobalFlag(90);
                case 205:
                case 206:
                    originalScript = "game.quests[36].state <= qs_mentioned and pc.stat_level_get( stat_gender ) == gender_female";
                    return GetQuestState(36) <= QuestState.Mentioned && pc.GetGender() == Gender.Female;
                case 207:
                case 208:
                    originalScript = "game.quests[36].state == qs_accepted";
                    return GetQuestState(36) == QuestState.Accepted;
                case 231:
                case 232:
                case 241:
                case 242:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 233:
                case 234:
                case 243:
                case 244:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_flags[323] = 1";
                    SetGlobalFlag(323, true);
                    break;
                case 40:
                    originalScript = "game.quests[36].state = qs_mentioned";
                    SetQuestState(36, QuestState.Mentioned);
                    break;
                case 43:
                case 44:
                case 45:
                case 91:
                case 92:
                case 93:
                    originalScript = "game.quests[36].state = qs_accepted";
                    SetQuestState(36, QuestState.Accepted);
                    break;
                case 141:
                case 142:
                case 143:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 181:
                    originalScript = "buttin(npc,pc,200)";
                    buttin(npc, pc, 200);
                    break;
                case 182:
                    originalScript = "100";
                    break;
                case 183:
                    originalScript = "0";
                    break;
                case 196:
                case 197:
                case 291:
                    originalScript = "buttin(npc,pc,210)";
                    buttin(npc, pc, 210);
                    break;
                case 201:
                case 202:
                    originalScript = "game.global_flags[90] = 1";
                    SetGlobalFlag(90, true);
                    break;
                case 231:
                case 232:
                case 241:
                case 242:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 310:
                    originalScript = "game.quests[36].state = qs_completed; game.global_flags[90] = 1";
                    SetQuestState(36, QuestState.Completed);
                    SetGlobalFlag(90, true);
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
