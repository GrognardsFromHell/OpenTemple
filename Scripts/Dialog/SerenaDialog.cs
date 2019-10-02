
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
    [DialogScript(214)]
    public class SerenaDialog : Serena, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[59].state <= qs_mentioned");
                    return npc.HasMet(pc) && GetQuestState(59) <= QuestState.Mentioned;
                case 8:
                case 9:
                case 133:
                case 134:
                    Trace.Assert(originalScript == "game.quests[59].state == qs_mentioned");
                    return GetQuestState(59) == QuestState.Mentioned;
                case 10:
                case 11:
                    Trace.Assert(originalScript == "game.quests[59].state == qs_accepted and game.global_flags[315] == 0");
                    return GetQuestState(59) == QuestState.Accepted && !GetGlobalFlag(315);
                case 12:
                case 13:
                    Trace.Assert(originalScript == "game.quests[59].state == qs_completed and pc.follower_atmax() == 0");
                    return GetQuestState(59) == QuestState.Completed && !pc.HasMaxFollowers();
                case 14:
                case 15:
                case 16:
                case 17:
                    Trace.Assert(originalScript == "game.quests[59].state == qs_accepted and game.global_flags[315] == 1");
                    return GetQuestState(59) == QuestState.Accepted && GetGlobalFlag(315);
                case 18:
                case 19:
                    Trace.Assert(originalScript == "game.quests[59].state <= qs_mentioned and game.global_flags[315] == 1");
                    return GetQuestState(59) <= QuestState.Mentioned && GetGlobalFlag(315);
                case 20:
                case 21:
                    Trace.Assert(originalScript == "game.quests[59].state == qs_completed");
                    return GetQuestState(59) == QuestState.Completed;
                case 22:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 59:
                case 60:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8058 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8058));
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.global_flags[319] == 0");
                    return !GetGlobalFlag(319);
                case 103:
                case 104:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) <= 10 and pc.stat_level_get( stat_gender ) == gender_male");
                    return pc.GetStat(Stat.charisma) <= 10 && pc.GetGender() == Gender.Male;
                case 106:
                case 107:
                    Trace.Assert(originalScript == "game.global_flags[319] == 1");
                    return GetGlobalFlag(319);
                case 131:
                case 132:
                    Trace.Assert(originalScript == "game.quests[59].state == qs_unknown");
                    return GetQuestState(59) == QuestState.Unknown;
                case 151:
                case 152:
                case 171:
                case 172:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
                case 153:
                case 154:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 1023:
                case 1024:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male and anyone( pc.group_list(), \"has_follower\", 8015 )");
                    return pc.GetGender() == Gender.Male && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8015));
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    Trace.Assert(originalScript == "game.global_vars[119] = 1");
                    SetGlobalVar(119, 1);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.quests[59].state = qs_mentioned");
                    SetQuestState(59, QuestState.Mentioned);
                    break;
                case 59:
                case 60:
                    Trace.Assert(originalScript == "buttin(npc, pc, 190)");
                    buttin(npc, pc, 190);
                    break;
                case 120:
                    Trace.Assert(originalScript == "game.quests[59].state = qs_accepted");
                    SetQuestState(59, QuestState.Accepted);
                    break;
                case 151:
                case 152:
                    Trace.Assert(originalScript == "pc.follower_add( npc ); together_again(npc,pc)");
                    pc.AddFollower(npc);
                    together_again(npc, pc);
                    ;
                    break;
                case 160:
                case 170:
                case 210:
                    Trace.Assert(originalScript == "game.quests[59].state = qs_completed");
                    SetQuestState(59, QuestState.Completed);
                    break;
                case 190:
                case 200:
                    Trace.Assert(originalScript == "game.global_flags[931] = 1");
                    SetGlobalFlag(931, true);
                    break;
                case 191:
                case 192:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "pc.follower_remove( npc )");
                    pc.RemoveFollower(npc);
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.global_flags[319] = 1");
                    SetGlobalFlag(319, true);
                    break;
                case 251:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 280)");
                    switch_to_tarah(npc, pc, 280);
                    break;
                case 261:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 300)");
                    switch_to_tarah(npc, pc, 300);
                    break;
                case 1000:
                    Trace.Assert(originalScript == "game.global_flags[930] = 1");
                    SetGlobalFlag(930, true);
                    break;
                case 1011:
                    Trace.Assert(originalScript == "buttin2(npc, pc, 1020)");
                    buttin2(npc, pc, 1020);
                    break;
                case 1023:
                case 1024:
                    Trace.Assert(originalScript == "buttin3(npc, pc, 450)");
                    buttin3(npc, pc, 450);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
