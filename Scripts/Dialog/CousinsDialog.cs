
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
    [DialogScript(378)]
    public class CousinsDialog : Cousins, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 61:
                    Trace.Assert(originalScript == "pc.reputation_has( 24 ) == 0");
                    return !pc.HasReputation(24);
                case 62:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 10 and pc.reputation_has( 24 ) == 0");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 10 && !pc.HasReputation(24);
                case 63:
                    Trace.Assert(originalScript == "pc.reputation_has( 24 ) == 1 and game.global_flags[815] == 1");
                    return pc.HasReputation(24) && GetGlobalFlag(815);
                case 64:
                    Trace.Assert(originalScript == "pc.reputation_has( 24 ) == 1 and game.global_flags[815] == 0");
                    return pc.HasReputation(24) && !GetGlobalFlag(815);
                case 161:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 162:
                    Trace.Assert(originalScript == "pc.money_get() <= 9900");
                    return pc.GetMoney() <= 9900;
                case 201:
                    Trace.Assert(originalScript == "game.global_vars[945] == 22");
                    return GetGlobalVar(945) == 22;
                case 202:
                    Trace.Assert(originalScript == "game.global_vars[945] == 23");
                    return GetGlobalVar(945) == 23;
                case 203:
                    Trace.Assert(originalScript == "game.global_vars[945] == 24");
                    return GetGlobalVar(945) == 24;
                case 271:
                    Trace.Assert(originalScript == "game.global_vars[945] == 25");
                    return GetGlobalVar(945) == 25;
                case 272:
                    Trace.Assert(originalScript == "game.global_vars[945] == 26");
                    return GetGlobalVar(945) == 26;
                case 273:
                    Trace.Assert(originalScript == "game.global_vars[945] == 27");
                    return GetGlobalVar(945) == 27;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 81:
                case 190:
                    Trace.Assert(originalScript == "game.global_vars[930] = 1; wait_a_day(npc,pc)");
                    SetGlobalVar(930, 1);
                    wait_a_day(npc, pc);
                    ;
                    break;
                case 110:
                    Trace.Assert(originalScript == "game.global_flags[982] = 1;");
                    SetGlobalFlag(982, true);
                    ;
                    break;
                case 111:
                    Trace.Assert(originalScript == "game.global_flags[983] = 1");
                    SetGlobalFlag(983, true);
                    break;
                case 161:
                    Trace.Assert(originalScript == "pc.money_adj(-10000)");
                    pc.AdjustMoney(-10000);
                    break;
                case 170:
                    Trace.Assert(originalScript == "game.global_flags[982] = 1");
                    SetGlobalFlag(982, true);
                    break;
                case 201:
                    Trace.Assert(originalScript == "game.global_vars[945] = 25");
                    SetGlobalVar(945, 25);
                    break;
                case 202:
                    Trace.Assert(originalScript == "game.global_vars[945] = 26");
                    SetGlobalVar(945, 26);
                    break;
                case 203:
                    Trace.Assert(originalScript == "game.global_vars[945] = 27");
                    SetGlobalVar(945, 27);
                    break;
                case 241:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(6); game.global_vars[959] = 3; run_off(npc,pc)");
                    // FIXME: worldmap_travel_by_dialog;
                    SetGlobalVar(959, 3);
                    run_off(npc, pc);
                    ;
                    break;
                case 250:
                    Trace.Assert(originalScript == "game.global_vars[959] = 6; run_off(npc,pc)");
                    SetGlobalVar(959, 6);
                    run_off(npc, pc);
                    ;
                    break;
                case 271:
                    Trace.Assert(originalScript == "run_off_3(npc,pc); spawn_attackers_for_snitch( npc, pc )");
                    run_off_3(npc, pc);
                    spawn_attackers_for_snitch(npc, pc);
                    ;
                    break;
                case 272:
                    Trace.Assert(originalScript == "run_off_3(npc,pc); spawn_attackers_for_narc( npc, pc )");
                    run_off_3(npc, pc);
                    spawn_attackers_for_narc(npc, pc);
                    ;
                    break;
                case 273:
                    Trace.Assert(originalScript == "run_off_3(npc,pc); spawn_attackers_for_whistleblower( npc, pc )");
                    run_off_3(npc, pc);
                    spawn_attackers_for_whistleblower(npc, pc);
                    ;
                    break;
                case 10000:
                    Trace.Assert(originalScript == "strip_forest(npc,pc)");
                    strip_forest(npc, pc);
                    break;
                case 20000:
                    Trace.Assert(originalScript == "strip_swamp(npc,pc)");
                    strip_swamp(npc, pc);
                    break;
                case 30000:
                    Trace.Assert(originalScript == "strip_river(npc,pc)");
                    strip_river(npc, pc);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 62:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
