
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
    [DialogScript(152)]
    public class KellaDialog : Kella, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                case 51:
                case 52:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 23:
                case 24:
                case 53:
                case 54:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 141:
                case 142:
                    Trace.Assert(originalScript == "anyone(pc.group_list(), \"has_follower\", 8034 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8034));
                case 201:
                case 202:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_halfelf");
                    return pc.GetRace() == RaceId.halfelf;
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
                case 30:
                case 60:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 41:
                case 71:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 93:
                case 94:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 190:
                    Trace.Assert(originalScript == "game.map_flags( 5080, 1, 1 ); game.map_flags( 5080, 2, 1 ); game.map_flags( 5080, 3, 1 ); game.map_flags( 5080, 4, 1 )");
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 200:
                    Trace.Assert(originalScript == "game.map_flags( 5080, 5, 1 ); game.map_flags( 5080, 6, 1 )");
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 210:
                    Trace.Assert(originalScript == "game.map_flags( 5080, 7, 1 ); game.map_flags( 5080, 8, 1 ); game.map_flags( 5080, 9, 1 ); game.map_flags( 5080, 10, 1 )");
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 231:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 280)");
                    switch_to_tarah(npc, pc, 280);
                    break;
                case 241:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 300)");
                    switch_to_tarah(npc, pc, 300);
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