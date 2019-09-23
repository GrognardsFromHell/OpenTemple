
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
    [SpellScript(486)]
    public class Sunbeam : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sunbeam OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Sunbeam OnSpellEffect");
            if (spell.casterLevel >= 18)
            {
                var bonus = 6;
            }
            else if (spell.casterLevel >= 15)
            {
                var bonus = 5;
            }
            else if (spell.casterLevel >= 12)
            {
                var bonus = 4;
            }
            else if (spell.casterLevel >= 9)
            {
                var bonus = 3;
            }
            else if (spell.casterLevel >= 6)
            {
                var bonus = 2;
            }
            else
            {
                var bonus = 1;
            }

            spell.duration = 1 * bonus;
            var target = spell.Targets[0];
            target.Object.AddCondition("sp-Produce Flame", spell.spellId, spell.duration, 2);
            target.ParticleSystem = AttachParticles("sp-Produce Flame", target.Object);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Sunbeam OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Sunbeam OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Produce Flame-proj', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Searing Light", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Sunbeam OnEndProjectile");
            var targg486 = spell.Targets[index_of_target].Object;
            var dam = Dice.D6;
            var dam2 = Dice.Parse("4d6");
            dam = dam.WithCount(Math.Min(20, spell.casterLevel));
            var return_val = spell.caster.PerformTouchAttack(targg486);
            AttachParticles("sp-Searing Light-Hit", targg486);
            if ((return_val & D20CAF.HIT) != D20CAF.NONE)
            {
                if (targg486.IsMonsterCategory(MonsterCategory.undead) || targg486.IsMonsterCategory(MonsterCategory.plant) || targg486.IsMonsterCategory(MonsterCategory.ooze))
                {
                    if (targg486.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.PositiveEnergy, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                    {
                        // saving throw successful
                        targg486.FloatMesFileLine("mes/spell.mes", 30001);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        targg486.FloatMesFileLine("mes/spell.mes", 20019);
                        targg486.AddCondition("sp-Blindness", spell.spellId, 30000, 0);
                    }

                }
                else
                {
                    if (targg486.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam2, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                    {
                        // saving throw successful
                        targg486.FloatMesFileLine("mes/spell.mes", 30001);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        targg486.FloatMesFileLine("mes/spell.mes", 20019);
                        targg486.AddCondition("sp-Blindness", spell.spellId, 30000, 0);
                    }

                }

            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sunbeam OnEndSpellCast");
        }

    }
}
