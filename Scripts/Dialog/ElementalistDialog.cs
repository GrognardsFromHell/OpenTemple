
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
    [DialogScript(482)]
    public class ElementalistDialog : Elementalist, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 11:
                case 21:
                case 31:
                    originalScript = "not npc_get(npc,2)";
                    return !ScriptDaemon.npc_get(npc, 2);
                case 3:
                case 12:
                case 22:
                case 32:
                    originalScript = "not npc_get(npc,3)";
                    return !ScriptDaemon.npc_get(npc, 3);
                case 4:
                case 13:
                case 23:
                case 33:
                    originalScript = "not npc_get(npc,4)";
                    return !ScriptDaemon.npc_get(npc, 4);
                case 5:
                case 14:
                case 24:
                case 34:
                    originalScript = "npc_get(npc,2) and npc_get(npc,3) and npc_get(npc,4)";
                    return ScriptDaemon.npc_get(npc, 2) && ScriptDaemon.npc_get(npc, 3) && ScriptDaemon.npc_get(npc, 4);
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
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 6:
                case 15:
                case 25:
                case 35:
                    originalScript = "switch_to_ariakas( npc, pc, 310)";
                    switch_to_ariakas(npc, pc, 310);
                    break;
                case 10:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 20:
                    originalScript = "npc_set(npc,3)";
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 30:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 40:
                    originalScript = "npc_set(npc,5)";
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 41:
                    originalScript = "switch_to_ariakas( npc, pc, 170); shake(npc,pc)";
                    switch_to_ariakas(npc, pc, 170);
                    shake(npc, pc);
                    ;
                    break;
                case 51:
                    originalScript = "switch_to_ariakas( npc, pc, 180)";
                    switch_to_ariakas(npc, pc, 180);
                    break;
                case 61:
                    originalScript = "dump_old_man(npc,pc); switch_to_ariakas( npc, pc, 190)";
                    dump_old_man(npc, pc);
                    switch_to_ariakas(npc, pc, 190);
                    ;
                    break;
                case 71:
                    originalScript = "switch_to_ariakas( npc, pc, 320); shake(npc,pc)";
                    switch_to_ariakas(npc, pc, 320);
                    shake(npc, pc);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
