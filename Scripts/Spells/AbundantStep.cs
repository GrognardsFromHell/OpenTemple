
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
    [SpellScript(743)]
    public class AbundantStep : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            // print "Abundant Step OnBeginSpellCast"
            // print "spell.target_list=", spell.target_list
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            // print "Abundant Step OnSpellEffect"
            var target = spell.caster;
            var item = target.ItemWornAt(EquipSlot.Lockpicks);
            if ((item != null && item.GetNameId() == 12420))
            {
                var monkLevel = target.GetStat(Stat.level_monk);
                Logger.Info("wearing belt, monk level= {0}", monkLevel);
                if ((monkLevel >= 12))
                {
                    if (!target.HasCondition(SpellEffects.SpellDimensionalAnchor))
                    {
                        AttachParticles("sp-Dimension Door", spell.caster);
                        target.FadeTo(0, 10, 40);
                        StartTimer(750, () => fade_back_in(target, spell.aoeCenter, spell), true);
                    }
                    else
                    {
                        target.FloatMesFileLine("mes/spell.mes", 30011);
                        AttachParticles("Fizzle", target);
                        spell.RemoveTarget(target);
                        spell.EndSpell();
                    }

                }
                else
                {
                    target.FloatMesFileLine("mes/spell.mes", 30020);
                }

            }

            spell.RemoveTarget(target);
            spell.EndSpell();
            return;
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Abundant Step OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Abundant Step OnEndSpellCast");
        }
        public static void fade_back_in(GameObjectBody target, LocAndOffsets loc, SpellPacketBody spell)
        {
            target.Move(loc);
            AttachParticles("sp-Dimension Door", target);
            target.FadeTo(255, 10, 5);
            spell.RemoveTarget(target);
            spell.EndSpell();
        }

    }
}
