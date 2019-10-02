
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
    [DialogScript(353)]
    public class GladstoneDialog : Gladstone, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 8717 ) and not anyone( pc.group_list(), \"has_follower\", 8718 )");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8718));
                case 3:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8717 ) and not anyone( pc.group_list(), \"has_follower\", 8718 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8718));
                case 4:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8718 ) and not anyone( pc.group_list(), \"has_follower\", 8717 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8718)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717));
                case 5:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8717 ) and anyone( pc.group_list(), \"has_follower\", 8718 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717)) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8718));
                case 52:
                case 62:
                case 72:
                case 92:
                case 102:
                case 112:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
                case 53:
                case 63:
                case 73:
                case 93:
                case 103:
                case 113:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 12 and pc.follower_atmax() == 0");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12 && !pc.HasMaxFollowers();
                case 54:
                case 64:
                case 74:
                case 94:
                case 104:
                case 114:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 141:
                case 151:
                    Trace.Assert(originalScript == "game.quests[81].state <= qs_mentioned");
                    return GetQuestState(81) <= QuestState.Mentioned;
                case 142:
                case 152:
                    Trace.Assert(originalScript == "game.quests[81].state == qs_accepted");
                    return GetQuestState(81) == QuestState.Accepted;
                case 161:
                    Trace.Assert(originalScript == "game.quests[80].state <= qs_mentioned");
                    return GetQuestState(80) <= QuestState.Mentioned;
                case 162:
                    Trace.Assert(originalScript == "game.quests[80].state == qs_accepted");
                    return GetQuestState(80) == QuestState.Accepted;
                case 262:
                    Trace.Assert(originalScript == "not get_1(npc)");
                    return !Scripts.get_1(npc);
                case 263:
                    Trace.Assert(originalScript == "not get_2(npc)");
                    return !Scripts.get_2(npc);
                case 264:
                    Trace.Assert(originalScript == "not get_3(npc)");
                    return !Scripts.get_3(npc);
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
                case 210:
                    Trace.Assert(originalScript == "game.global_vars[142] = 1");
                    SetGlobalVar(142, 1);
                    break;
                case 131:
                case 181:
                case 191:
                case 201:
                case 231:
                case 241:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 140:
                case 150:
                case 160:
                    Trace.Assert(originalScript == "pc.follower_add( npc )");
                    pc.AddFollower(npc);
                    break;
                case 141:
                case 151:
                    Trace.Assert(originalScript == "game.quests[81].state = qs_accepted");
                    SetQuestState(81, QuestState.Accepted);
                    break;
                case 161:
                    Trace.Assert(originalScript == "game.quests[80].state = qs_accepted");
                    SetQuestState(80, QuestState.Accepted);
                    break;
                case 170:
                    Trace.Assert(originalScript == "game.global_vars[962] = 1");
                    SetGlobalVar(962, 1);
                    break;
                case 262:
                    Trace.Assert(originalScript == "npc_1(npc)");
                    Scripts.npc_1(npc);
                    break;
                case 263:
                    Trace.Assert(originalScript == "npc_2(npc)");
                    Scripts.npc_2(npc);
                    break;
                case 264:
                    Trace.Assert(originalScript == "npc_3(npc)");
                    Scripts.npc_3(npc);
                    break;
                case 2000:
                case 3000:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); game.quests[79].state = qs_botched");
                    pc.RemoveFollower(npc);
                    SetQuestState(79, QuestState.Botched);
                    ;
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
                case 53:
                case 63:
                case 73:
                case 93:
                case 103:
                case 113:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
