
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
    [DialogScript(134)]
    public class MerrolanDialog : Merrolan, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                    originalScript = "switch_dialog(npc,pc,30)";
                    switch_dialog(npc, pc, 30);
                    break;
                case 41:
                case 42:
                    originalScript = "switch_dialog(npc,pc,50)";
                    switch_dialog(npc, pc, 50);
                    break;
                case 51:
                case 52:
                    originalScript = "switch_dialog(npc,pc,60)";
                    switch_dialog(npc, pc, 60);
                    break;
                case 61:
                case 62:
                    originalScript = "switch_dialog(npc,pc,70)";
                    switch_dialog(npc, pc, 70);
                    break;
                case 81:
                case 82:
                    originalScript = "switch_dialog(npc,pc,90)";
                    switch_dialog(npc, pc, 90);
                    break;
                case 101:
                case 102:
                    originalScript = "switch_dialog(npc,pc,100)";
                    switch_dialog(npc, pc, 100);
                    break;
                case 111:
                case 112:
                    originalScript = "switch_dialog(npc,pc,110)";
                    switch_dialog(npc, pc, 110);
                    break;
                case 121:
                case 122:
                    originalScript = "switch_dialog(npc,pc,120)";
                    switch_dialog(npc, pc, 120);
                    break;
                case 131:
                case 132:
                    originalScript = "switch_dialog(npc,pc,130)";
                    switch_dialog(npc, pc, 130);
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
