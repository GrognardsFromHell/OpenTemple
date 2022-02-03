
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
    [SpellScript(767)]
    public class BenignTransposition : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Benign Transposition OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Benign Transposition OnSpellEffect");
            var target_1 = spell.Targets[0];
            var target_2 = spell.Targets[1];
            var loc_1 = target_1.Object.GetLocation();
            var loc_2 = target_2.Object.GetLocation();
            var size_1 = GameSystems.Stat.DispatchGetSizeCategory(target_1.Object);
            var size_2 = GameSystems.Stat.DispatchGetSizeCategory(target_2.Object);
            if ((target_1.Object.HasCondition(SpellEffects.SpellDimensionalAnchor) || target_2.Object.HasCondition(SpellEffects.SpellDimensionalAnchor)))
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 30011);
                AttachParticles("Fizzle", spell.caster);
            }
            else if ((size_1 > SizeCategory.Large || size_2 > SizeCategory.Large))
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 31005);
                AttachParticles("Fizzle", spell.caster);
            }
            else if (target_1.Object.IsFriendly(spell.caster) && target_2.Object.IsFriendly(spell.caster))
            {
                target_1.Object.Move(loc_2);
                AttachParticles("sp-Dimension Door", target_1.Object);
                target_2.Object.Move(loc_1);
                AttachParticles("sp-Dimension Door", target_2.Object);
            }
            else
            {
                AttachParticles("Fizzle", spell.caster);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Benign Transposition OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Benign Transposition OnEndSpellCast");
        }

    }
}
