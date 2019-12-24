
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [SpellScript(757)]
    public class OozeYouLose : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Ooze You Lose OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Ooze You Lose OnSpellEffect");
            AttachParticles("sp-Fireball-conjure", spell.caster);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Ooze You Lose OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Spore No More OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-Melfs Acid Arrow Projectile", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Ooze You Lose OnEndProjectile");
            var remove_list = new List<GameObjectBody>();
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
                EndProjectileParticles(projectile);
            }

            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.IsMonsterCategory(MonsterCategory.ooze) || target_item.Object.GetNameId() == 14955)
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
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31015);
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
            Logger.Info("Ooze You Lose OnEndSpellCast");
        }

    }
}
