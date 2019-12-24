
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
    [DialogScript(212)]
    public class BurnebadgerDialog : Burnebadger, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 5:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 3:
                case 4:
                case 6:
                case 7:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 31:
                case 35:
                    originalScript = "find_npc_near(npc,8054) == OBJ_HANDLE_NULL and (npc.map < 5014 or npc.map > 5019)";
                    return Utilities.find_npc_near(npc, 8054) == null && (npc.GetMap() < 5014 || npc.GetMap() > 5019);
                case 32:
                case 36:
                    originalScript = "find_npc_near(npc,8054) == OBJ_HANDLE_NULL and npc.map >= 5014 and npc.map <= 5019";
                    return Utilities.find_npc_near(npc, 8054) == null && npc.GetMap() >= 5014 && npc.GetMap() <= 5019;
                case 33:
                case 37:
                    originalScript = "find_npc_near(npc,8054) != OBJ_HANDLE_NULL";
                    return Utilities.find_npc_near(npc, 8054) != null;
                case 71:
                    originalScript = "npc.map < 5006 or npc.map >5008";
                    return npc.GetMap() < 5006 || npc.GetMap() > 5008;
                case 72:
                    originalScript = "npc.map < 5011 or npc.map > 5013";
                    return npc.GetMap() < 5011 || npc.GetMap() > 5013;
                case 73:
                    originalScript = "npc.map < 5014 or npc.map > 5019";
                    return npc.GetMap() < 5014 || npc.GetMap() > 5019;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
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
