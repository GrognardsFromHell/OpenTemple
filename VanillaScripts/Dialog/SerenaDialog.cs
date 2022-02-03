
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
    [DialogScript(214)]
    public class SerenaDialog : Serena, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 6:
                case 7:
                    originalScript = "npc.has_met( pc ) and game.quests[59].state <= qs_mentioned";
                    return npc.HasMet(pc) && GetQuestState(59) <= QuestState.Mentioned;
                case 8:
                case 9:
                case 133:
                case 134:
                    originalScript = "game.quests[59].state == qs_mentioned";
                    return GetQuestState(59) == QuestState.Mentioned;
                case 10:
                case 11:
                    originalScript = "game.quests[59].state == qs_accepted and game.global_flags[315] == 0";
                    return GetQuestState(59) == QuestState.Accepted && !GetGlobalFlag(315);
                case 12:
                case 13:
                    originalScript = "game.quests[59].state == qs_completed and pc.follower_atmax() == 0";
                    return GetQuestState(59) == QuestState.Completed && !pc.HasMaxFollowers();
                case 14:
                case 15:
                case 16:
                case 17:
                    originalScript = "game.quests[59].state == qs_accepted and game.global_flags[315] == 1";
                    return GetQuestState(59) == QuestState.Accepted && GetGlobalFlag(315);
                case 18:
                case 19:
                    originalScript = "game.quests[59].state <= qs_mentioned and game.global_flags[315] == 1";
                    return GetQuestState(59) <= QuestState.Mentioned && GetGlobalFlag(315);
                case 20:
                case 21:
                    originalScript = "game.quests[59].state == qs_completed";
                    return GetQuestState(59) == QuestState.Completed;
                case 22:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 59:
                case 60:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8058 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8058));
                case 101:
                case 102:
                    originalScript = "game.global_flags[319] == 0";
                    return !GetGlobalFlag(319);
                case 103:
                case 104:
                    originalScript = "pc.stat_level_get(stat_charisma) <= 10 and pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetStat(Stat.charisma) <= 10 && pc.GetGender() == Gender.Male;
                case 106:
                case 107:
                    originalScript = "game.global_flags[319] == 1";
                    return GetGlobalFlag(319);
                case 131:
                case 132:
                    originalScript = "game.quests[59].state == qs_unknown";
                    return GetQuestState(59) == QuestState.Unknown;
                case 151:
                case 152:
                case 171:
                case 172:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 153:
                case 154:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 50:
                    originalScript = "game.quests[59].state = qs_mentioned";
                    SetQuestState(59, QuestState.Mentioned);
                    break;
                case 59:
                case 60:
                    originalScript = "buttin(npc, pc, 190)";
                    buttin(npc, pc, 190);
                    break;
                case 120:
                    originalScript = "game.quests[59].state = qs_accepted";
                    SetQuestState(59, QuestState.Accepted);
                    break;
                case 151:
                case 152:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 160:
                case 170:
                case 210:
                    originalScript = "game.quests[59].state = qs_completed";
                    SetQuestState(59, QuestState.Completed);
                    break;
                case 171:
                case 172:
                    originalScript = "npc.loots = follower_loot_nothing";
                    npc.SetLootSharingType(LootSharingType.nothing);
                    break;
                case 191:
                case 192:
                case 201:
                case 202:
                    originalScript = "pc.follower_remove( npc ); run_off(npc,pc)";
                    pc.RemoveFollower(npc);
                    npc.RunOff();
                    break;
                case 240:
                    originalScript = "game.global_flags[319] = 1";
                    SetGlobalFlag(319, true);
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
