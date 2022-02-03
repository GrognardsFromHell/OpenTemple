
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
    [DialogScript(142)]
    public class SenshockDialog : Senshock, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                    originalScript = "game.global_flags[145] == 1 and game.global_flags[146] == 0 and pc.skill_level_get(npc, skill_bluff) >= 12";
                    return GetGlobalFlag(145) && !GetGlobalFlag(146) && pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 111:
                case 112:
                    originalScript = "game.global_flags[146] == 0";
                    return !GetGlobalFlag(146);
                case 113:
                case 114:
                    originalScript = "game.global_flags[146] == 1";
                    return GetGlobalFlag(146);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 161:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 101:
                    originalScript = "senshock_kills_hedrack(npc,pc)";
                    senshock_kills_hedrack(npc, pc);
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
                case 21:
                case 22:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
