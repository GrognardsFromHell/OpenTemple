
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
    [DialogScript(87)]
    public class PeasantLaborersDialog : PeasantLaborers, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                    originalScript = "not npc.has_met( pc ) and (is_daytime() == 1)";
                    return !npc.HasMet(pc) && (Utilities.is_daytime());
                case 4:
                    originalScript = "not npc.has_met( pc ) and (is_daytime() != 1)";
                    return !npc.HasMet(pc) && !Utilities.is_daytime();
                case 5:
                    originalScript = "not npc.has_met( pc ) and game.quests[15].state == qs_accepted";
                    return !npc.HasMet(pc) && GetQuestState(15) == QuestState.Accepted;
                case 6:
                    originalScript = "game.global_flags[508] == 1 and game.quests[96].state == qs_unknown";
                    return GetGlobalFlag(508) && GetQuestState(96) == QuestState.Unknown;
                case 7:
                    originalScript = "game.quests[96].state == qs_mentioned";
                    return GetQuestState(96) == QuestState.Mentioned;
                case 41:
                case 61:
                case 71:
                case 81:
                case 91:
                    originalScript = "not npc_get(npc,1)";
                    return !ScriptDaemon.npc_get(npc, 1);
                case 42:
                case 51:
                case 72:
                case 82:
                case 92:
                    originalScript = "not npc_get(npc,2)";
                    return !ScriptDaemon.npc_get(npc, 2);
                case 43:
                case 52:
                case 62:
                    originalScript = "not npc_get(npc,3) and game.global_flags[282] == 0 and not anyone( pc.group_list(), \"has_follower\", 8054 ) and game.global_flags[284] == 1";
                    return !ScriptDaemon.npc_get(npc, 3) && !GetGlobalFlag(282) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && GetGlobalFlag(284);
                case 44:
                case 53:
                case 63:
                    originalScript = "not npc_get(npc,4) and game.global_flags[282] == 1 and not anyone( pc.group_list(), \"has_follower\", 8071 ) and game.global_flags[284] == 0";
                    return !ScriptDaemon.npc_get(npc, 4) && GetGlobalFlag(282) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071)) && !GetGlobalFlag(284);
                case 45:
                case 54:
                case 64:
                    originalScript = "not npc_get(npc,5) and not anyone( pc.group_list(), \"has_follower\", 8054 ) and game.global_flags[282] == 0 and not anyone( pc.group_list(), \"has_follower\", 8071 ) and game.global_flags[284] == 0";
                    return !ScriptDaemon.npc_get(npc, 5) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8054)) && !GetGlobalFlag(282) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8071)) && !GetGlobalFlag(284);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
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
                    originalScript = "game.quests[96].state = qs_accepted";
                    SetQuestState(96, QuestState.Accepted);
                    break;
                case 40:
                    originalScript = "game.quests[96].state = qs_mentioned";
                    SetQuestState(96, QuestState.Mentioned);
                    break;
                case 41:
                case 61:
                case 71:
                case 81:
                case 91:
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 42:
                case 51:
                case 72:
                case 82:
                case 92:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 43:
                case 52:
                case 62:
                    originalScript = "npc_set(npc,3)";
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 44:
                case 53:
                case 63:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 45:
                case 54:
                case 64:
                    originalScript = "npc_set(npc,5)";
                    ScriptDaemon.npc_set(npc, 5);
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
