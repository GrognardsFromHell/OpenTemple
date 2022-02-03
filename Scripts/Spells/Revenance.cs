
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

[SpellScript(774)]
public class Revenance : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("{0}", ("Revenance OnBeginSpellCast"));
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Revenance OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;
        var target_item = spell.Targets[0];
        if (target_item.Object.GetStat(Stat.hp_current) <= -10)
        {
            target_item.Object.Resurrect(ResurrectionType.CuthbertResurrection, 0);
            var damage_dice = (target_item.Object.GetStat(Stat.hp_current) / 2);
            var dam = Dice.Parse("1d1");
            dam = dam.WithCount(Math.Min(500, damage_dice));
            target_item.Object.DealSpellDamage(spell.caster, DamageType.BloodLoss, dam, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            target_item.Object.SetScriptId(ObjScriptEvent.ExitCombat, 453);
        }
        else
        {
            AttachParticles("Fizzle", target_item.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("{0}", ("Revenance OnBeginRound"));
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("{0}", ("Revenance OnEndSpellCast"));
    }

}