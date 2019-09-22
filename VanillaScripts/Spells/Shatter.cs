
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(425)]
    public class Shatter : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Shatter OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Shatter OnSpellEffect");
            spell.duration = 0;

            var target_item = spell.Targets[0];

            var damage_dice = Dice.D6;

            damage_dice = damage_dice.WithCount(Math.Min(spell.casterLevel, 10));
            target_item.ParticleSystem = AttachParticles("sp-Shatter", target_item.Object);

            if (target_item.Object.IsMonsterSubtype(MonsterSubtype.earth))
            {
                if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.Sonic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Sonic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                }

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31011);
                AttachParticles("Fizzle", target_item.Object);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Shatter OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Shatter OnEndSpellCast");
        }


    }
}
