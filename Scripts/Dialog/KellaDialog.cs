
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

namespace Scripts.Dialog
{
    [DialogScript(152)]
    public class KellaDialog : Kella, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                case 51:
                case 52:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 23:
                case 24:
                case 53:
                case 54:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 141:
                case 142:
                    originalScript = "anyone(pc.group_list(), \"has_follower\", 8034 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8034));
                case 201:
                case 202:
                    originalScript = "pc.stat_level_get(stat_race) == race_halfelf";
                    return pc.GetRace() == RaceId.halfelf;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 30:
                case 60:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 41:
                case 71:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
                    break;
                case 93:
                case 94:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 190:
                    originalScript = "game.map_flags( 5080, 1, 1 ); game.map_flags( 5080, 2, 1 ); game.map_flags( 5080, 3, 1 ); game.map_flags( 5080, 4, 1 )";
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 200:
                    originalScript = "game.map_flags( 5080, 5, 1 ); game.map_flags( 5080, 6, 1 )";
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 210:
                    originalScript = "game.map_flags( 5080, 7, 1 ); game.map_flags( 5080, 8, 1 ); game.map_flags( 5080, 9, 1 ); game.map_flags( 5080, 10, 1 )";
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 231:
                    originalScript = "switch_to_tarah( npc, pc, 280)";
                    switch_to_tarah(npc, pc, 280);
                    break;
                case 241:
                    originalScript = "switch_to_tarah( npc, pc, 300)";
                    switch_to_tarah(npc, pc, 300);
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
