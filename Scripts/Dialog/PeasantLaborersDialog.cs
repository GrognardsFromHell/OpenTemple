
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
    [DialogScript(87)]
    public class PeasantLaborersDialog : PeasantLaborers, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met( pc ) and (is_daytime() == 1)");
                    return !npc.HasMet(pc) && (Utilities.is_daytime());
                case 4:
                    Trace.Assert(originalScript == "not npc.has_met( pc ) and (is_daytime() != 1)");
                    return !npc.HasMet(pc) && !Utilities.is_daytime();
                case 5:
                    Trace.Assert(originalScript == "not npc.has_met( pc ) and game.quests[15].state == qs_accepted");
                    return !npc.HasMet(pc) && GetQuestState(15) == QuestState.Accepted;
                case 6:
                    Trace.Assert(originalScript == "game.global_flags[508] == 1 and game.quests[96].state == qs_unknown");
                    return GetGlobalFlag(508) && GetQuestState(96) == QuestState.Unknown;
                case 7:
                    Trace.Assert(originalScript == "game.quests[96].state == qs_mentioned");
                    return GetQuestState(96) == QuestState.Mentioned;
                case 41:
                case 61:
                case 71:
                case 81:
                case 91:
                    Trace.Assert(originalScript == "not npc_get(npc,1)");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 42:
                case 51:
                case 72:
                case 82:
                case 92:
                    Trace.Assert(originalScript == "not npc_get(npc,2)");
                    return !ScriptDaemon.npc_get(npc, 2);
                case 43:
                case 52:
                case 62:
                    Trace.Assert(originalScript == "not npc_get(npc,3) and game.global_flags[282] == 0 and not anyone( pc.group_list(), \"has_follower\", 8054 ) and game.global_flags[284] == 1");
                    return !ScriptDaemon.npc_get(npc, 3) && !GetGlobalFlag(282) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && GetGlobalFlag(284);
                case 44:
                case 53:
                case 63:
                    Trace.Assert(originalScript == "not npc_get(npc,4) and game.global_flags[282] == 1 and not anyone( pc.group_list(), \"has_follower\", 8071 ) and game.global_flags[284] == 0");
                    return !ScriptDaemon.npc_get(npc, 4) && GetGlobalFlag(282) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071)) && !GetGlobalFlag(284);
                case 45:
                case 54:
                case 64:
                    Trace.Assert(originalScript == "not npc_get(npc,5) and not anyone( pc.group_list(), \"has_follower\", 8054 ) and game.global_flags[282] == 0 and not anyone( pc.group_list(), \"has_follower\", 8071 ) and game.global_flags[284] == 0");
                    return !ScriptDaemon.npc_get(npc, 5) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && !GetGlobalFlag(282) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071)) && !GetGlobalFlag(284);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 7:
                case 46:
                case 55:
                case 65:
                case 73:
                case 83:
                case 93:
                    Trace.Assert(originalScript == "game.quests[96].state = qs_accepted");
                    SetQuestState(96, QuestState.Accepted);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.quests[96].state = qs_mentioned");
                    SetQuestState(96, QuestState.Mentioned);
                    break;
                case 41:
                case 61:
                case 71:
                case 81:
                case 91:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 42:
                case 51:
                case 72:
                case 82:
                case 92:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 43:
                case 52:
                case 62:
                    Trace.Assert(originalScript == "npc_set(npc,3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 44:
                case 53:
                case 63:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 45:
                case 54:
                case 64:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
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
