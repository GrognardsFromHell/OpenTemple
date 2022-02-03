
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
    [DialogScript(355)]
    public class CorpusDialog : Corpus, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8716 ) and not anyone( pc.group_list(), \"has_follower\", 8717 )";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8716)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717));
                case 3:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8716 ) and not anyone( pc.group_list(), \"has_follower\", 8717 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8716)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717));
                case 4:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8717 ) and not anyone( pc.group_list(), \"has_follower\", 8716 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8716));
                case 5:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8716 ) and anyone( pc.group_list(), \"has_follower\", 8717 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8716)) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717));
                case 52:
                case 62:
                case 72:
                case 92:
                case 102:
                case 112:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 53:
                case 63:
                case 73:
                case 93:
                case 103:
                case 113:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 12 and pc.follower_atmax() == 0";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12 && !pc.HasMaxFollowers();
                case 54:
                case 64:
                case 74:
                case 94:
                case 104:
                case 114:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                case 141:
                case 151:
                    originalScript = "game.quests[80].state <= qs_mentioned";
                    return GetQuestState(80) <= QuestState.Mentioned;
                case 142:
                case 152:
                    originalScript = "game.quests[80].state == qs_accepted";
                    return GetQuestState(80) == QuestState.Accepted;
                case 161:
                    originalScript = "game.quests[79].state <= qs_mentioned";
                    return GetQuestState(79) <= QuestState.Mentioned;
                case 162:
                    originalScript = "game.quests[79].state == qs_accepted";
                    return GetQuestState(79) == QuestState.Accepted;
                case 262:
                    originalScript = "not get_1(npc)";
                    return !Scripts.get_1(npc);
                case 263:
                    originalScript = "not get_2(npc)";
                    return !Scripts.get_2(npc);
                case 264:
                    originalScript = "not get_3(npc)";
                    return !Scripts.get_3(npc);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 210:
                    originalScript = "game.global_vars[144] = 1";
                    SetGlobalVar(144, 1);
                    break;
                case 131:
                case 181:
                case 191:
                case 201:
                case 231:
                case 241:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 140:
                case 150:
                case 160:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 141:
                case 151:
                    originalScript = "game.quests[80].state = qs_accepted";
                    SetQuestState(80, QuestState.Accepted);
                    break;
                case 161:
                    originalScript = "game.quests[79].state = qs_accepted";
                    SetQuestState(79, QuestState.Accepted);
                    break;
                case 170:
                    originalScript = "game.global_vars[962] = 1";
                    SetGlobalVar(962, 1);
                    break;
                case 262:
                    originalScript = "npc_1(npc)";
                    Scripts.npc_1(npc);
                    break;
                case 263:
                    originalScript = "npc_2(npc)";
                    Scripts.npc_2(npc);
                    break;
                case 264:
                    originalScript = "npc_3(npc)";
                    Scripts.npc_3(npc);
                    break;
                case 2000:
                case 3000:
                    originalScript = "pc.follower_remove( npc ); game.quests[81].state = qs_botched";
                    pc.RemoveFollower(npc);
                    SetQuestState(81, QuestState.Botched);
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
