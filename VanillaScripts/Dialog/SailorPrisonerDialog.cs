
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
    [DialogScript(130)]
    public class SailorPrisonerDialog : SailorPrisoner, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 51:
                case 52:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 53:
                case 54:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 93:
                case 94:
                    originalScript = "game.global_flags[88] == 1";
                    return GetGlobalFlag(88);
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
                case 3:
                case 20:
                    originalScript = "get_rep( npc, pc )";
                    get_rep(npc, pc);
                    break;
                case 60:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 100:
                case 110:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 111:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
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
