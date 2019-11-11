
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
    [DialogScript(481)]
    public class RobinOfLocksleyDialog : RobinOfLocksley, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 101:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetGender() == Gender.Male;
                case 102:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 101:
                case 102:
                    originalScript = "buff_npc(npc, pc)";
                    // TODO buff_npc(npc, pc);
                    break;
                case 200:
                case 300:
                    originalScript = "game.global_vars[923] = 20";
                    SetGlobalVar(923, 20);
                    break;
                case 201:
                case 301:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
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
