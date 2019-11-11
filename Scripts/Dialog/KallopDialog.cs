
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
    [DialogScript(592)]
    public class KallopDialog : Kallop, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 11:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 0";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
                case 3:
                case 12:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 0";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
                case 4:
                case 13:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 0";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
                case 5:
                case 14:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 0";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    originalScript = "game.global_vars[802] = game.global_vars[802] + 1; increment_rep(npc,pc)";
                    SetGlobalVar(802, GetGlobalVar(802) + 1);
                    increment_rep(npc, pc);
                    ;
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                    originalScript = "switch_to_boonthag( npc, pc, 1 )";
                    switch_to_boonthag(npc, pc, 1);
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
                case 2:
                case 11:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 0);
                    return true;
                case 3:
                case 12:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 0);
                    return true;
                case 4:
                case 13:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 0);
                    return true;
                case 5:
                case 14:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 0);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
