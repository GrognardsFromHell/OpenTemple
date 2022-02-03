
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
    [DialogScript(85)]
    public class WeaverSonInLawDialog : WeaverSonInLaw, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 81:
                case 82:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 83:
                case 84:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 4:
                    originalScript = "argue(npc,pc,20)";
                    argue(npc, pc, 20);
                    break;
                case 21:
                    originalScript = "argue(npc,pc,40)";
                    argue(npc, pc, 40);
                    break;
                case 31:
                    originalScript = "argue(npc,pc,50)";
                    argue(npc, pc, 50);
                    break;
                case 41:
                case 42:
                    originalScript = "argue(npc,pc,60)";
                    argue(npc, pc, 60);
                    break;
                case 71:
                case 72:
                    originalScript = "argue(npc,pc,70)";
                    argue(npc, pc, 70);
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
