
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
    [SpellScript(739)]
    public class FrozenBreath : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Frozen Breath OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Frozen Breath OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var dam = Dice.D6;
            dam = dam.WithCount(Math.Min(15, spell.casterLevel));
            AttachParticles("sp-Cone of Cold", spell.caster);
            var npc = spell.caster;
            spell.dc = spell.dc + 5;
            // range = 25 + 5 * int(spell.caster_level/2)
            var range = 60;
            var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, range, -30, 60);
            foreach (var obj in target_list)
            {
                if (obj == spell.caster)
                {
                    continue;
                }
                if (obj.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Cold, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    // saving throw successful
                    obj.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    // saving throw unsuccessful
                    obj.FloatMesFileLine("mes/spell.mes", 30002);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Frozen Breath OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Frozen Breath OnEndSpellCast");
        }

    }
}
