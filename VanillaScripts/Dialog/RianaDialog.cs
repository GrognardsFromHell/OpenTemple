
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
    [DialogScript(100)]
    public class RianaDialog : Riana, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 23:
                    originalScript = "game.global_flags[77] == 0";
                    return !GetGlobalFlag(77);
                case 3:
                case 4:
                    originalScript = "game.global_flags[77] == 1";
                    return GetGlobalFlag(77);
                case 5:
                case 6:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8056 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8056));
                case 21:
                case 22:
                    originalScript = "game.quests[33].state == qs_completed and game.global_flags[84] == 1";
                    return GetQuestState(33) == QuestState.Completed && GetGlobalFlag(84);
                case 24:
                case 25:
                    originalScript = "game.quests[33].state == qs_completed and game.global_flags[84] == 0 and game.global_flags[77] == 1";
                    return GetQuestState(33) == QuestState.Completed && !GetGlobalFlag(84) && GetGlobalFlag(77);
                case 27:
                case 28:
                    originalScript = "game.quests[33].state != qs_completed";
                    return GetQuestState(33) != QuestState.Completed;
                case 33:
                case 34:
                case 43:
                    originalScript = "game.quests[33].state == qs_accepted and game.global_flags[77] == 1";
                    return GetQuestState(33) == QuestState.Accepted && GetGlobalFlag(77);
                case 41:
                case 42:
                    originalScript = "game.quests[33].state <= qs_mentioned";
                    return GetQuestState(33) <= QuestState.Mentioned;
                case 46:
                case 47:
                case 71:
                case 72:
                case 142:
                case 143:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 131:
                case 132:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 133:
                case 134:
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
                case 5:
                case 6:
                    originalScript = "game.global_flags[319] = 1";
                    SetGlobalFlag(319, true);
                    break;
                case 21:
                case 22:
                    originalScript = "game.global_flags[84] = 0";
                    SetGlobalFlag(84, false);
                    break;
                case 46:
                case 47:
                    originalScript = "npc.reaction_adj( pc,-10)";
                    npc.AdjustReaction(pc, -10);
                    break;
                case 50:
                    originalScript = "game.quests[33].state = qs_mentioned";
                    SetQuestState(33, QuestState.Mentioned);
                    break;
                case 51:
                case 52:
                    originalScript = "game.quests[33].state = qs_accepted";
                    SetQuestState(33, QuestState.Accepted);
                    break;
                case 61:
                case 62:
                case 103:
                case 104:
                    originalScript = "game.global_flags[84] = 1";
                    SetGlobalFlag(84, true);
                    break;
                case 81:
                    originalScript = "npc.attack( pc ); get_rep( npc, pc )";
                    npc.Attack(pc);
                    get_rep(npc, pc);
                    ;
                    break;
                case 100:
                    originalScript = "disappear( npc ); game.global_flags[203] = 1";
                    disappear(npc);
                    SetGlobalFlag(203, true);
                    ;
                    break;
                case 131:
                case 132:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 181:
                case 182:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
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
