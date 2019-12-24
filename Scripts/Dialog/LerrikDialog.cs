
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

namespace Scripts.Dialog
{
    [DialogScript(382)]
    public class LerrikDialog : Lerrik, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not npc.has_met(pc) and is_daytime() == 1";
                    return !npc.HasMet(pc) && Utilities.is_daytime();
                case 3:
                    originalScript = "not npc.has_met(pc) and not is_daytime() == 1";
                    return !npc.HasMet(pc) && !(Utilities.is_daytime());
                case 4:
                    originalScript = "npc.has_met(pc) and is_daytime() == 1";
                    return npc.HasMet(pc) && Utilities.is_daytime();
                case 5:
                    originalScript = "npc.has_met(pc) and not is_daytime() == 1 and game.quests[77].state == qs_unknown";
                    return npc.HasMet(pc) && !(Utilities.is_daytime()) && GetQuestState(77) == QuestState.Unknown;
                case 6:
                    originalScript = "not is_daytime() == 1 and (game.quests[77].state == qs_mentioned or game.quests[77].state == qs_accepted) and pc.skill_level_get(npc,skill_sense_motive) >= 10";
                    return !(Utilities.is_daytime()) && (GetQuestState(77) == QuestState.Mentioned || GetQuestState(77) == QuestState.Accepted) && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 7:
                    originalScript = "not is_daytime() == 1 and game.quests[77].state == qs_completed and pc.skill_level_get(npc,skill_sense_motive) >= 10 and not npc_get(npc,2)";
                    return !(Utilities.is_daytime()) && GetQuestState(77) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10 && !ScriptDaemon.npc_get(npc, 2);
                case 21:
                    originalScript = "game.quests[74].state != qs_unknown and game.quests[74].state != qs_completed and game.quests[69].state == qs_unknown and not npc_get(npc,1)";
                    return GetQuestState(74) != QuestState.Unknown && GetQuestState(74) != QuestState.Completed && GetQuestState(69) == QuestState.Unknown && !ScriptDaemon.npc_get(npc, 1);
                case 22:
                    originalScript = "game.quests[69].state != qs_unknown and game.quests[69].state != qs_completed and game.quests[74].state == qs_unknown and not npc_get(npc,1)";
                    return GetQuestState(69) != QuestState.Unknown && GetQuestState(69) != QuestState.Completed && GetQuestState(74) == QuestState.Unknown && !ScriptDaemon.npc_get(npc, 1);
                case 23:
                    originalScript = "game.quests[74].state != qs_unknown and game.quests[74].state != qs_completed and game.quests[69].state != qs_unknown and game.quests[69].state != qs_completed and not npc_get(npc,1)";
                    return GetQuestState(74) != QuestState.Unknown && GetQuestState(74) != QuestState.Completed && GetQuestState(69) != QuestState.Unknown && GetQuestState(69) != QuestState.Completed && !ScriptDaemon.npc_get(npc, 1);
                case 24:
                    originalScript = "game.global_vars[978] == 1 and (pc.skill_level_get(npc,skill_spot) >= 11 or party_spot_check() >= 11) and game.global_vars[966] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 25:
                    originalScript = "game.global_vars[978] == 3 and game.global_vars[966] == 1";
                    return GetGlobalVar(978) == 3 && GetGlobalVar(966) == 1;
                case 26:
                    originalScript = "game.global_vars[978] == 4 and game.global_vars[966] == 1";
                    return GetGlobalVar(978) == 4 && GetGlobalVar(966) == 1;
                case 27:
                    originalScript = "game.global_vars[978] == 5 and game.global_vars[966] == 1";
                    return GetGlobalVar(978) == 5 && GetGlobalVar(966) == 1;
                case 31:
                case 41:
                    originalScript = "pc.stat_level_get(stat_race) == race_gnome";
                    return pc.GetRace() == RaceId.svirfneblin;
                case 71:
                case 171:
                    originalScript = "game.global_flags[986] == 1";
                    return GetGlobalFlag(986);
                case 72:
                case 181:
                    originalScript = "game.global_flags[981] == 1";
                    return GetGlobalFlag(981);
                case 131:
                    originalScript = "game.global_vars[999] >= 1 and game.quests[69].state != completed";
                    return GetGlobalVar(999) >= 1 && GetQuestState(69) != QuestState.Completed;
                case 221:
                    originalScript = "npc_get(npc,2)";
                    return ScriptDaemon.npc_get(npc, 2);
                case 222:
                    originalScript = "npc_get(npc,3)";
                    return ScriptDaemon.npc_get(npc, 3);
                case 231:
                case 341:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 9";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
                case 232:
                case 342:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) <= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 8;
                case 251:
                case 253:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8714 )";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8714));
                case 252:
                case 254:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8714 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8714));
                case 261:
                case 281:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 262:
                case 282:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) <= 7";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 7;
                case 271:
                case 291:
                    originalScript = "game.leader.reputation_has(36) == 1";
                    return SelectedPartyLeader.HasReputation(36);
                case 272:
                case 292:
                    originalScript = "game.leader.reputation_has(36) == 0";
                    return !SelectedPartyLeader.HasReputation(36);
                case 361:
                    originalScript = "is_daytime() == 1 and not npc_get(npc,4)";
                    return Utilities.is_daytime() && !ScriptDaemon.npc_get(npc, 4);
                case 362:
                    originalScript = "is_daytime() == 1 and npc_get(npc,4)";
                    return Utilities.is_daytime() && ScriptDaemon.npc_get(npc, 4);
                case 363:
                    originalScript = "not is_daytime() == 1";
                    return !(Utilities.is_daytime());
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                    originalScript = "game.quests[77].state = qs_botched";
                    SetQuestState(77, QuestState.Botched);
                    break;
                case 7:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 10:
                    originalScript = "game.global_flags[968] = 1";
                    SetGlobalFlag(968, true);
                    break;
                case 21:
                case 22:
                case 23:
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 24:
                    originalScript = "game.global_vars[966] = 1";
                    SetGlobalVar(966, 1);
                    break;
                case 25:
                case 26:
                case 27:
                    originalScript = "game.global_vars[978] = 6; game.global_vars[966] = 2";
                    SetGlobalVar(978, 6);
                    SetGlobalVar(966, 2);
                    ;
                    break;
                case 51:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 71:
                case 171:
                    originalScript = "game.global_vars[978] = 1; npc_set(npc,2)";
                    SetGlobalVar(978, 1);
                    ScriptDaemon.npc_set(npc, 2);
                    ;
                    break;
                case 72:
                case 181:
                    originalScript = "game.global_vars[978] = 1; npc_set(npc,3)";
                    SetGlobalVar(978, 1);
                    ScriptDaemon.npc_set(npc, 3);
                    ;
                    break;
                case 240:
                    originalScript = "game.quests[68].state = qs_mentioned";
                    SetQuestState(68, QuestState.Mentioned);
                    break;
                case 361:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
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
                case 6:
                case 7:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                case 231:
                case 341:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                    return true;
                case 261:
                case 281:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
