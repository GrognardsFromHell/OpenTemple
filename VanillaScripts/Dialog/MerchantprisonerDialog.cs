
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

namespace VanillaScripts.Dialog
{
    [DialogScript(235)]
    public class MerchantprisonerDialog : Merchantprisoner, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 11:
                case 25:
                case 26:
                case 31:
                    Trace.Assert(originalScript == "game.global_flags[352] == 0");
                    return !GetGlobalFlag(352);
                case 12:
                case 13:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male and game.global_flags[352] == 0");
                    return pc.GetGender() == Gender.Male && !GetGlobalFlag(352);
                case 15:
                case 23:
                case 24:
                case 32:
                    Trace.Assert(originalScript == "game.global_flags[352] == 1");
                    return GetGlobalFlag(352);
                case 16:
                case 17:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male and game.global_flags[352] == 1");
                    return pc.GetGender() == Gender.Male && GetGlobalFlag(352);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 11:
                case 12:
                case 13:
                case 25:
                case 26:
                case 31:
                case 43:
                case 44:
                    Trace.Assert(originalScript == "run_off(npc, pc)");
                    run_off(npc, pc);
                    break;
                case 41:
                case 42:
                    Trace.Assert(originalScript == "pc.money_adj(+103000); run_off(npc, pc)");
                    pc.AdjustMoney(+103000);
                    run_off(npc, pc);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
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
