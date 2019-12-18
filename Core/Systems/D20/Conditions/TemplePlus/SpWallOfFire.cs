using System;
using System.Collections.Generic;
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
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class SpWallOfFire
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        // args: (0-7)
        // 0 - spell_id
        // 1 - duration
        // 2 - event ID
        // 3 - angle (milli radians)
        // 4 - length (milli feet)
        public static void WallOfFireOnAdd(in DispatcherCallbackArgs evt)
        {
            var spell_id = evt.GetConditionArg1();
            var wall_angle_mrad = evt.GetConditionArg4();
            var wall_length_mft = evt.GetConditionArg(4);
            var wall_angle_rad = wall_angle_mrad / 1000f;
            var wall_length_ft = wall_length_mft / 1000f;
            var evt_id = GameSystems.ObjectEvent.AddEvent(
                evt.objHndCaller,
                ObjectEvent.OBJ_EVENT_WALL_ENTERED_HANDLER_ID,
                ObjectEvent.OBJ_EVENT_WALL_EXITED_HANDLER_ID,
                ObjectListFilter.OLC_CRITTERS,
                wall_length_ft * locXY.INCH_PER_FEET,
                wall_angle_rad,
                2 * MathF.PI /* 360° */
            );
            evt.SetConditionArg3(evt_id); // store the event ID
            // Update the spell packet
            var spell_packet = GameSystems.Spell.GetActiveSpell(spell_id);
            var spell_obj = evt.objHndCaller;
            // x,y = location_to_axis(spell_obj.location)
            // print "spell_obj loc x,y: " + str(x) + " " + str(y)
            // print "spell_obj loc off_x: " + str(spell_obj.off_x)
            // print "spell_obj loc off_y: " + str(spell_obj.off_y)
            var spell_partsys_id = AttachParticles("sp-Wall of Fire3", spell_obj);
            spell_packet.AddSpellObject(spell_obj, spell_partsys_id); // store the spell obj and the particle sys
            var origin_loc = evt.objHndCaller.GetLocationFull();
            var N_sections = (int) (MathF.Round(wall_length_ft / 5f));
            // print "Wall length(ft): " + str(wall_length_ft) + "  sections: " + str(N_sections)
            for (var p = 1; p < N_sections; p++)
            {
                var spell_obj_loc = origin_loc.OffsetFeet(wall_angle_rad, p * 5f);
                spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell_obj_loc);
                spell_obj.Move(spell_obj_loc);
                spell_obj.TurnTowards(evt.objHndCaller);
                spell_obj.Rotation += 3.1415f;
                var (x, y) = spell_obj.GetLocation();
                // print "spell_obj loc x,y: " + str(x) + " " + str(y)
                // print "spell_obj loc off_x: " + str(spell_obj.off_x)
                // print "spell_obj loc off_y: " + str(spell_obj.off_y)
                spell_partsys_id = AttachParticles("sp-Wall of Fire3", spell_obj);
                spell_packet.AddSpellObject(spell_obj, spell_partsys_id); // store the spell obj and the particle sys
            }

            spell_packet.caster.AddCondition(SpellEffects.SpellConcentrating, spell_id);
        }

        public static void OnWallAoEEntered(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjEvent();
            // print "Wall of Fire entered event"
            var obj_evt_id = evt.GetConditionArg3();
            var spell_id = evt.GetConditionArg1();
            var spell_packet = GameSystems.Spell.GetActiveSpell(spell_id);
            var duration = evt.GetConditionArg2();
            var caster = spell_packet.caster;
            if (obj_evt_id != dispIo.evtId)
            {
                // print "Wall of Fire Entered: ID mismatch " + str(dispIo.evt_id) + ", stored was: " + str(obj_evt_id)
                return;
            }

            // print "Wall of Fire Entered, event ID: " + str(obj_evt_id)
            var tgt = dispIo.tgt;
            if (tgt == null || evt.objHndCaller == null || tgt == evt.objHndCaller)
            {
                return;
            }

            // print str(tgt) + " hit by wall of fire"
            GameSystems.Script.Spells.SpellTrigger(spell_packet.spellId, SpellEvent.AreaOfEffectHit);
            if (D20ModSpells.CheckSpellResistance(tgt, spell_packet))
            {
                return;
            }

            // apply sp-Wall of Fire hit condition (applies damage on beginning of round)
            var partsys_id = GameSystems.ParticleSys.CreateAtObj("sp-Wall of Fire-hit", tgt);
            if (spell_packet.AddTarget(tgt, partsys_id))
            {
                tgt.AddCondition("sp-Wall of Fire hit", spell_id, duration, obj_evt_id);
            }

            return;
        }

        public static void OnConcentrationBroken(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // print "Concentration broken"
            var spellId = evt.GetConditionArg1();
            var spell_packet = GameSystems.Spell.GetActiveSpell(spellId);
            if (spell_packet.spellEnum == 0)
            {
                return;
            }

            SpellEffects.Spell_remove_spell(in evt, 0, 0);
            // args.remove_spell_mod()
            return;
        }

        public static void OnCombatEnd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // print "Combat End"
            var spellId = evt.GetConditionArg1();
            var spell_packet = GameSystems.Spell.GetActiveSpell(spellId);
            if (spell_packet.spellEnum == 0)
            {
                return;
            }

            if (spell_packet.caster != null)
            {
                GameSystems.Spell.FloatSpellLine(spell_packet.caster, 20000, TextFloaterColor.White);
            }

            SpellEffects.Spell_remove_spell(in evt, 0, 0);
            SpellEffects.Spell_remove_mod(in evt, 0);
            return;
        }

        [AutoRegister] public static readonly ConditionSpec wallOfFire = ConditionSpec.Create("sp-Wall of Fire", 8)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, WallOfFireOnAdd)
            .AddHandler(DispatcherType.ObjectEvent, D20DispatcherKey.OnEnterAoE, OnWallAoEEntered)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Concentration_Broken, OnConcentrationBroken)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Combat_End, OnCombatEnd)
            .AddSpellCountdownStandardHook()
            .AddAoESpellEndStandardHook()
            .Build(); // oops, Wall of Fire doesn't have Dismiss (but it does have COncentration...)

        // sp-Wall of fire hit
        // does damage at the beginning of round

        public static void EndSpellMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var spell_id = evt.GetConditionArg1();
            if (dispIo.data1 == spell_id)
            {
                Logger.Info("{0}", "Ending mod for spell ID: " + spell_id.ToString());
                SpellEffects.Spell_remove_mod(in evt, 0); // does a .condition_remove() with some safety checks
            }

            return;
        }

        public static void WallOfFireBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // print "Wall of Fire begin round"
            var tgt = evt.objHndCaller;
            var spell_id = evt.GetConditionArg1();
            var spell_packet = GameSystems.Spell.GetActiveSpell(spell_id);
            var caster = spell_packet.caster;
            var damage_dice = Dice.Parse("2d4");
            var undead_dice = Dice.Parse("4d4");
            if (tgt.IsMonsterCategory(MonsterCategory.undead))
            {
                tgt.DealSpellDamage(caster, DamageType.Fire, undead_dice, D20AttackPower.UNSPECIFIED,
                    D20ActionType.CAST_SPELL, spell_id);
            }
            else
            {
                tgt.DealSpellDamage(caster, DamageType.Fire, damage_dice, D20AttackPower.UNSPECIFIED,
                    D20ActionType.CAST_SPELL, spell_id);
            }

            return;
        }

        public static void WallOfFireHitDamage(in DispatcherCallbackArgs evt)
        {
            // print "Wall of Fire hit damage"
            var tgt = evt.objHndCaller;
            var spell_id = evt.GetConditionArg1();
            var spell_packet = GameSystems.Spell.GetActiveSpell(spell_id);
            var caster = spell_packet.caster;
            var damage_dice = Dice.Parse("2d6");
            damage_dice = damage_dice.WithModifier(Math.Min(1 * spell_packet.casterLevel, 20));
            var undead_dice = Dice.Parse("4d6");
            undead_dice = undead_dice.WithModifier(Math.Min(2 * spell_packet.casterLevel, 40));
            if (tgt.IsMonsterCategory(MonsterCategory.undead))
            {
                tgt.DealSpellDamage(caster, DamageType.Fire, undead_dice, D20AttackPower.UNSPECIFIED,
                    D20ActionType.CAST_SPELL, spell_id);
            }
            else
            {
                tgt.DealSpellDamage(caster, DamageType.Fire, damage_dice, D20AttackPower.UNSPECIFIED,
                    D20ActionType.CAST_SPELL, spell_id);
            }

            return;
        }

        public static void OnWallAoEExit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjEvent();
            var evt_id = evt.GetConditionArg3();
            if (evt_id != dispIo.evtId)
            {
                return;
            }

            Logger.Info("{0}", "Removing sp-Wall of fire hit on " + evt.objHndCaller.ToString());
            var spell_id = evt.GetConditionArg1();
            var spell_packet = GameSystems.Spell.GetActiveSpell(spell_id);
            spell_packet.RemoveTarget(evt.objHndCaller);
            SpellEffects.Spell_remove_mod(in evt, 0);
            return;
        }

        [AutoRegister] public static readonly ConditionSpec wallOfFireHit = ConditionSpec.Create("sp-Wall of Fire hit", 8)
            .SetUnique()
            .AddHandler(DispatcherType.BeginRound, WallOfFireBeginRound)
            .AddHandler(DispatcherType.ConditionAdd, WallOfFireHitDamage)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Spell_End, EndSpellMod)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Killed, EndSpellMod)
            .AddHandler(DispatcherType.ObjectEvent, D20DispatcherKey.OnLeaveAoE, OnWallAoEExit)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Concentration_Broken, EndSpellMod)
            .AddSpellCountdownStandardHook()
            .Build();
    }
}