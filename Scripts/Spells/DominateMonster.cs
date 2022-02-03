
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

namespace Scripts.Spells;

[SpellScript(140)]
public class DominateMonster : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        var target = spell.Targets[0];
        spell.duration = 14400 * spell.casterLevel;
        if (target.Object.IsFriendly(spell.caster))
        {
            // can't dominate friendlies
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
        }
        else if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            // success
            target.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
        }
        else
        {
            // failure
            target.Object.FloatMesFileLine("mes/spell.mes", 30002);
            target.ParticleSystem = AttachParticles("sp-Dominate Person", target.Object);
            target.Object.AddCondition("sp-Dominate Person", spell.spellId, spell.duration, 0);
            target.Object.AddToInitiative();
            UiSystems.Combat.Initiative.UpdateIfNeeded();
        }

        spell.EndSpell();
    }

}