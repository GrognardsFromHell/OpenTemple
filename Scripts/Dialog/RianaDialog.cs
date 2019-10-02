
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
    [DialogScript(100)]
    public class RianaDialog : Riana, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 23:
                    Trace.Assert(originalScript == "game.global_flags[77] == 0");
                    return !GetGlobalFlag(77);
                case 3:
                case 4:
                    Trace.Assert(originalScript == "game.global_flags[77] == 1");
                    return GetGlobalFlag(77);
                case 5:
                case 6:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8056 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8056));
                case 21:
                case 22:
                    Trace.Assert(originalScript == "game.quests[33].state == qs_completed and game.global_flags[84] == 1");
                    return GetQuestState(33) == QuestState.Completed && GetGlobalFlag(84);
                case 24:
                case 25:
                    Trace.Assert(originalScript == "game.quests[33].state == qs_completed and game.global_flags[84] == 0 and game.global_flags[77] == 1");
                    return GetQuestState(33) == QuestState.Completed && !GetGlobalFlag(84) && GetGlobalFlag(77);
                case 27:
                case 28:
                    Trace.Assert(originalScript == "game.quests[33].state != qs_completed");
                    return GetQuestState(33) != QuestState.Completed;
                case 33:
                case 34:
                case 43:
                    Trace.Assert(originalScript == "game.quests[33].state == qs_accepted and game.global_flags[77] == 1");
                    return GetQuestState(33) == QuestState.Accepted && GetGlobalFlag(77);
                case 41:
                case 42:
                    Trace.Assert(originalScript == "game.quests[33].state <= qs_mentioned");
                    return GetQuestState(33) <= QuestState.Mentioned;
                case 46:
                case 47:
                case 71:
                case 72:
                case 142:
                case 143:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 131:
                case 132:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
                case 133:
                case 134:
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
                    Trace.Assert(originalScript == "game.global_vars[120] = 1");
                    SetGlobalVar(120, 1);
                    break;
                case 5:
                case 6:
                    Trace.Assert(originalScript == "game.global_flags[319] = 1");
                    SetGlobalFlag(319, true);
                    break;
                case 21:
                case 22:
                    Trace.Assert(originalScript == "game.global_flags[84] = 0");
                    SetGlobalFlag(84, false);
                    break;
                case 46:
                case 47:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-10)");
                    npc.AdjustReaction(pc, -10);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.quests[33].state = qs_mentioned");
                    SetQuestState(33, QuestState.Mentioned);
                    break;
                case 51:
                case 52:
                    Trace.Assert(originalScript == "game.quests[33].state = qs_accepted");
                    SetQuestState(33, QuestState.Accepted);
                    break;
                case 61:
                case 62:
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.global_flags[84] = 1");
                    SetGlobalFlag(84, true);
                    break;
                case 81:
                    Trace.Assert(originalScript == "npc.attack( pc ); get_rep( npc, pc )");
                    npc.Attack(pc);
                    get_rep(npc, pc);
                    ;
                    break;
                case 100:
                    Trace.Assert(originalScript == "disappear( npc ); game.global_flags[203] = 1");
                    disappear(npc);
                    SetGlobalFlag(203, true);
                    ;
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.global_flags[930] = 0");
                    SetGlobalFlag(930, false);
                    break;
                case 131:
                case 132:
                    Trace.Assert(originalScript == "pc.follower_add( npc ); together_again(npc,pc)");
                    pc.AddFollower(npc);
                    together_again(npc, pc);
                    ;
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.global_flags[930] = 1");
                    SetGlobalFlag(930, true);
                    break;
                case 181:
                case 182:
                    Trace.Assert(originalScript == "pc.follower_remove( npc )");
                    pc.RemoveFollower(npc);
                    break;
                case 201:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 280)");
                    switch_to_tarah(npc, pc, 280);
                    break;
                case 211:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 300)");
                    switch_to_tarah(npc, pc, 300);
                    break;
                case 1000:
                    Trace.Assert(originalScript == "game.global_flags[931] = 1");
                    SetGlobalFlag(931, true);
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
