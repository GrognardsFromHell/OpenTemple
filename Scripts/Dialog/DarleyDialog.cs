
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
    [DialogScript(171)]
    public class DarleyDialog : Darley, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 32:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 10 and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10 && pc.GetStat(Stat.level_paladin) == 0;
                case 33:
                case 34:
                case 57:
                case 58:
                case 59:
                case 131:
                case 132:
                case 141:
                case 142:
                case 181:
                case 182:
                case 183:
                case 184:
                case 191:
                case 192:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 35:
                case 36:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) > 0");
                    return pc.GetStat(Stat.level_paladin) > 0;
                case 41:
                case 51:
                case 52:
                case 211:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD) or pc.stat_level_get(stat_level_paladin) > 0");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD) || pc.GetStat(Stat.level_paladin) > 0;
                case 42:
                case 212:
                    Trace.Assert(originalScript == "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != NEUTRAL_GOOD) and (game.party_alignment != CHAOTIC_GOOD) and pc.stat_level_get(stat_level_paladin) == 0");
                    return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.NEUTRAL_GOOD) && (PartyAlignment != Alignment.CHAOTIC_GOOD) && pc.GetStat(Stat.level_paladin) == 0;
                case 53:
                case 55:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_human and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetRace() == RaceId.human && pc.GetStat(Stat.level_paladin) == 0;
                case 54:
                case 56:
                    Trace.Assert(originalScript == "(pc.stat_level_get(stat_race) == race_halfelf or pc.stat_level_get(stat_race) == race_halforc) and pc.stat_level_get(stat_level_paladin) == 0");
                    return (pc.GetRace() == RaceId.halfelf || pc.GetRace() == RaceId.half_orc) && pc.GetStat(Stat.level_paladin) == 0;
                case 73:
                case 74:
                case 81:
                case 82:
                case 83:
                case 84:
                case 85:
                    Trace.Assert(originalScript == "npc.leader_get() == OBJ_HANDLE_NULL");
                    return npc.GetLeader() == null;
                case 75:
                case 76:
                case 86:
                case 87:
                case 88:
                case 89:
                    Trace.Assert(originalScript == "npc.leader_get() != OBJ_HANDLE_NULL");
                    return npc.GetLeader() != null;
                case 91:
                case 92:
                    Trace.Assert(originalScript == "game.global_flags[180] == 1 and not pc.follower_atmax()");
                    return GetGlobalFlag(180) && !pc.HasMaxFollowers();
                case 93:
                case 94:
                    Trace.Assert(originalScript == "game.global_flags[180] == 0 and not pc.follower_atmax()");
                    return !GetGlobalFlag(180) && !pc.HasMaxFollowers();
                case 95:
                case 96:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
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
                    Trace.Assert(originalScript == "game.global_vars[138] = 1");
                    SetGlobalVar(138, 1);
                    break;
                case 31:
                case 32:
                case 35:
                case 36:
                    Trace.Assert(originalScript == "change_to_demon(npc,pc,40)");
                    change_to_demon(npc, pc, 40);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.global_flags[180] = 1");
                    SetGlobalFlag(180, true);
                    break;
                case 51:
                case 52:
                    Trace.Assert(originalScript == "pc.follower_remove(npc); npc.attack(pc)");
                    pc.RemoveFollower(npc);
                    npc.Attack(pc);
                    ;
                    break;
                case 100:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 160:
                case 193:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
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
                case 31:
                case 32:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
