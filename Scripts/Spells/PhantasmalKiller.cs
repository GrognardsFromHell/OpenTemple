
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

[SpellScript(345)]
public class PhantasmalKiller : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Phantasmal Killer OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-illusion-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Phantasmal Killer OnSpellEffect");
        var dice = Dice.Parse("3d6");
        var target = spell.Targets[0];
        AttachParticles("sp-Phantasmal Killer", target.Object);
        // will save to negate, otherwise fort save to avoid death
        if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            // saving throw successful
            target.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target.Object);
        }
        else if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR, spell.caster, spell.spellId))
        {
            // fort save success, damage 3d6
            target.Object.FloatMesFileLine("mes/spell.mes", 30001);
            // saving throw unsuccesful, damage target, full damage
            target.Object.DealSpellDamage(spell.caster, DamageType.Magic, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
        }
        else
        {
            // fort save failure, kill
            target.Object.FloatMesFileLine("mes/spell.mes", 30002);
            // kill target
            // So you'll get awarded XP for the kill
            if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target.Object)))
            {
                target.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
            }

            target.Object.Kill();
        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Phantasmal Killer OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Phantasmal Killer OnEndSpellCast");
    }

}