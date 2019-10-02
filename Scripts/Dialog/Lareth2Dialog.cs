
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
    [DialogScript(308)]
    public class Lareth2Dialog : Lareth2, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 103:
                case 104:
                case 163:
                case 164:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 161:
                case 162:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 431:
                case 432:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 901:
                case 903:
                    Trace.Assert(originalScript == "pc.money_get() >= 50000");
                    return pc.GetMoney() >= 50000;
                case 1901:
                case 1903:
                    Trace.Assert(originalScript == "pc.money_get() >= 600000");
                    return pc.GetMoney() >= 600000;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 120:
                    Trace.Assert(originalScript == "game.global_flags[838] = 1");
                    SetGlobalFlag(838, true);
                    break;
                case 10:
                case 20:
                case 30:
                case 40:
                    Trace.Assert(originalScript == "game.particles( \"sp-Bestow Curse\", npc )");
                    AttachParticles("sp-Bestow Curse", npc);
                    break;
                case 103:
                case 104:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 163:
                case 164:
                    Trace.Assert(originalScript == "pc.follower_add(npc); game.global_flags[53] = 1; pc.reputation_add( 18 )");
                    pc.AddFollower(npc);
                    SetGlobalFlag(53, true);
                    pc.AddReputation(18);
                    ;
                    break;
                case 251:
                    Trace.Assert(originalScript == "game.global_flags[846] = 1; game.global_flags[37] = 1; run_off( npc, pc )");
                    SetGlobalFlag(846, true);
                    SetGlobalFlag(37, true);
                    run_off(npc, pc);
                    ;
                    break;
                case 300:
                    Trace.Assert(originalScript == "game.global_flags[839] = 1");
                    SetGlobalFlag(839, true);
                    break;
                case 301:
                case 302:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5065,485,494)");
                    FadeAndTeleport(0, 0, 0, 5065, 485, 494);
                    break;
                case 303:
                case 304:
                    Trace.Assert(originalScript == "game.global_flags[847] = 1; pc.follower_remove(npc); run_off(npc,pc)");
                    SetGlobalFlag(847, true);
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    ;
                    break;
                case 350:
                    Trace.Assert(originalScript == "game.global_flags[840] = 1; game.global_flags[833] = 1; remove_combat(npc,pc)");
                    SetGlobalFlag(840, true);
                    SetGlobalFlag(833, true);
                    remove_combat(npc, pc);
                    ;
                    break;
                case 400:
                    Trace.Assert(originalScript == "game.global_flags[840] = 1; game.global_flags[841] = 1; game.global_flags[833] = 1; pc.follower_remove(npc)");
                    SetGlobalFlag(840, true);
                    SetGlobalFlag(841, true);
                    SetGlobalFlag(833, true);
                    pc.RemoveFollower(npc);
                    ;
                    break;
                case 430:
                case 440:
                    Trace.Assert(originalScript == "pc.money_adj(50000)");
                    pc.AdjustMoney(50000);
                    break;
                case 605:
                case 606:
                case 855:
                case 856:
                case 1005:
                case 1006:
                case 1853:
                case 1854:
                case 2005:
                case 2006:
                    Trace.Assert(originalScript == "create_store(npc,pc)");
                    create_store(npc, pc);
                    break;
                case 800:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_critical_wounds, should_heal_hp_on, [ 850, 600, 900 ] )");
                    // FIXME: picker;
                    break;
                case 901:
                case 903:
                    Trace.Assert(originalScript == "pc.money_adj(-50000); npc.cast_spell( spell_cure_critical_wounds, picker_obj )");
                    pc.AdjustMoney(-50000);
                    npc.CastSpell(WellKnownSpells.CureCriticalWounds, PickedObject);
                    ;
                    break;
                case 1000:
                case 2000:
                    Trace.Assert(originalScript == "npc.spells_pending_to_memorized()");
                    npc.PendingSpellsToMemorized();
                    break;
                case 1800:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 1850, 600, 1900 ] )");
                    // FIXME: picker;
                    break;
                case 1901:
                case 1903:
                    Trace.Assert(originalScript == "pc.money_adj(-600000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-600000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
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
                case 431:
                case 432:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
