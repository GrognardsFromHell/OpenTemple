
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
    [SpellScript(559)]
    public class Quench : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Quench OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Quench OnSpellEffect");
            // duration is 1d4 hours if implementing 2nd part of quench
            spell.duration = 0;
            var target_item = spell.Targets[0];
            var damage_dice = Dice.D6;
            damage_dice = damage_dice.WithCount(Math.Min(spell.casterLevel, 15));
            target_item.ParticleSystem = AttachParticles("sp-Quench", target_item.Object);
            // only damages fire elementals (and flaming sphere Darmagon)
            if (Co8.is_spell_flag_set(target_item.Object, Co8SpellFlag.FlamingSphere))
            {
                target_item.Object.Destroy();
            }
            else
            {
                if ((target_item.Object.IsMonsterCategory(MonsterCategory.elemental)) && (target_item.Object.IsMonsterSubtype(MonsterSubtype.fire)))
                {
                    // no save, full damage
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.Unspecified, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    spell.RemoveTarget(target_item.Object);
                }
                else
                {
                    // allowing targeting of creatures? 2nd part of quench
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31013);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Quench OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Quench OnEndSpellCast");
        }

    }
}
