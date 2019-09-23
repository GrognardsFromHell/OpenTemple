
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
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(0)]
    public class AcidFog : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Acid Fog OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Acid Fog OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;
            // spawn one spell_object object
            var spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
            // add to d20initiative
            var caster_init_value = spell.caster.GetInitiative();
            spell_obj.InitD20Status();
            spell_obj.SetInitiative(caster_init_value);
            // put sp-Solid Fog condition on obj
            var spell_obj_partsys_id = AttachParticles("sp-Solid Fog", spell_obj);
            spell_obj.AddCondition("sp-Solid Fog", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
            // spell_obj.condition_add_arg_x( 3, spell_obj_partsys_id )
            // objectevent_id = spell_obj.condition_get_arg_x( 2 )
            // add monster to target list
            spell.ClearTargets();
            spell.AddTarget(spell_obj);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Acid Fog OnBeginRound");
            var damage_dice = Dice.Parse("2d6");
            foreach (var obj in ObjList.ListVicinity(spell.Targets[0].Object.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((spell.Targets[0].Object.DistanceTo(obj) <= 20))
                {
                    obj.DealSpellDamage(spell.caster, DamageType.Acid, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }

            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Acid Fog OnEndSpellCast");
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Acid Fog OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Acid Fog OnSpellStruck");
        }

    }
}
