
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

namespace VanillaScripts.Dialog
{
    [DialogScript(175)]
    public class AshremDialog : Ashrem, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 61:
                case 62:
                    originalScript = "( game.quests[52].state >= qs_mentioned ) and ( not pc.follower_atmax() ) and ( anyone( pc.group_list(), \"has_follower\", 8039 ) ) and ( pc.stat_level_get(stat_level_paladin) == 0 )";
                    return (GetQuestState(52) >= QuestState.Mentioned) && (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 63:
                case 64:
                case 135:
                case 136:
                case 153:
                case 154:
                case 161:
                case 162:
                    originalScript = "pc.follower_atmax() and pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 65:
                case 66:
                    originalScript = "game.global_flags[107] == 1";
                    return GetGlobalFlag(107);
                case 67:
                case 68:
                    originalScript = "( game.quests[52].state >= qs_mentioned ) and ( not pc.follower_atmax() ) and ( not anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)";
                    return (GetQuestState(52) >= QuestState.Mentioned) && (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 69:
                case 70:
                    originalScript = "( game.quests[52].state == qs_unknown ) and ( not pc.follower_atmax() ) and ( anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)";
                    return (GetQuestState(52) == QuestState.Unknown) && (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 71:
                case 72:
                    originalScript = "( game.quests[52].state == qs_unknown ) and ( not pc.follower_atmax() ) and ( not anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)";
                    return (GetQuestState(52) == QuestState.Unknown) && (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 75:
                case 76:
                    originalScript = "(pc.stat_level_get(stat_level_paladin) == 1)";
                    return (pc.GetStat(Stat.level_paladin) == 1);
                case 81:
                case 82:
                    originalScript = "(game.party_alignment != LAWFUL_EVIL) and (game.party_alignment != NEUTRAL_EVIL) and (game.party_alignment != CHAOTIC_EVIL) and anyone( pc.group_list(), \"has_follower\", 8040 )";
                    return (PartyAlignment != Alignment.LAWFUL_EVIL) && (PartyAlignment != Alignment.NEUTRAL_EVIL) && (PartyAlignment != Alignment.CHAOTIC_EVIL) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8040));
                case 107:
                case 108:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 131:
                case 132:
                case 155:
                case 156:
                case 163:
                case 164:
                    originalScript = "( not pc.follower_atmax() ) and ( anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)";
                    return (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                case 133:
                case 134:
                case 157:
                case 158:
                case 165:
                case 166:
                    originalScript = "( not pc.follower_atmax() ) and ( not anyone( pc.group_list(), \"has_follower\", 8039 ) ) and (pc.stat_level_get(stat_level_paladin) == 0)";
                    return (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(8039))) && (pc.GetStat(Stat.level_paladin) == 0);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "talk_Taki(npc,pc,10,20)";
                    talk_Taki(npc, pc, 10, 20);
                    break;
                case 11:
                    originalScript = "talk_Taki(npc,pc,180,20)";
                    talk_Taki(npc, pc, 180, 20);
                    break;
                case 31:
                case 32:
                case 75:
                case 76:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 91:
                    originalScript = "talk_Taki(npc,pc,230,100)";
                    talk_Taki(npc, pc, 230, 100);
                    break;
                case 140:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 180:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 230:
                    originalScript = "game.global_flags[192] = 1";
                    SetGlobalFlag(192, true);
                    break;
                case 231:
                    originalScript = "talk_Alrrem(npc,pc,230,100)";
                    talk_Alrrem(npc, pc, 230, 100);
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
                case 107:
                case 108:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
