
using System;
using System.Collections.Generic;
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

namespace Scripts.Spells
{
    [SpellScript(756)]
    public class SporeNoMore : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Spore No More OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Spore No More  OnSpellEffect");
            AttachParticles("sp-Fireball-conjure", spell.caster);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Spore No More OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
        {
            Logger.Info("Spore No More OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Fireball-proj', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Melfs Acid Arrow Projectile", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
        {
            Logger.Info("Spore No More OnEndProjectile");
            var remove_list = new List<GameObject>();
            spell.duration = 0;
            var dam = Dice.D6;
            dam = dam.WithCount(3);
            var npc = spell.caster;
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && npc.GetInt(obj_f.npc_pad_i_5) >= 1000)
            {
                var xyx = npc.GetInt(obj_f.npc_pad_i_5) - 1000;
                var loc = GameSystems.Party.GetPartyGroupMemberN(xyx).GetLocation();
                EndProjectileParticles(projectile);
            }
            else
            {
                // game.particles( 'sp-Fireball-Hit', loc )
                EndProjectileParticles(projectile);
            }

            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.IsMonsterCategory(MonsterCategory.plant))
                {
                    if ((target_item.Object.DistanceTo(spell.Targets[0].Object) <= 10))
                    {
                        dam = dam.WithCount(3);
                        target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.NegativeEnergy, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        AttachParticles("sp-Blight", target_item.Object);
                    }
                    else
                    {
                        dam = dam.WithCount(2);
                        target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.NegativeEnergy, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        AttachParticles("sp-Blight", target_item.Object);
                    }

                }
                else
                {
                    dam = dam.WithCount(1);
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31012);
                    AttachParticles("Fizzle", target_item.Object);
                    target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Acid, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Spore No More OnEndSpellCast");
        }

    }
}
