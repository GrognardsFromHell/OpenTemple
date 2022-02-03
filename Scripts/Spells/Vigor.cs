
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

[SpellScript(795)]
public class Vigor : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Vigor OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Vigor OnSpellEffect");
        spell.duration = 10 + Math.Min(15, spell.casterLevel);
        var target = spell.Targets[0];
        // Use any spell effect with a duration that you will not be using while under
        // the effects of vigor
        target.Object.AddCondition("sp-Barkskin", spell.spellId, spell.duration, 0);
        target.ParticleSystem = AttachParticles("sp-Cure Minor Wounds", target.Object);
        var dice = Dice.Parse("1d1");
        dice = dice.WithModifier(1);
        target.Object.Heal(null, dice);
        target.Object.HealSubdual(null, dice);
        var heal_count = 1;
        var heal_tick_time = 999;
        if (!GameSystems.Combat.IsCombatActive())
        {
            heal_tick_time = 6000;
        }

        while (heal_count < spell.duration)
        {
            StartTimer((heal_count * heal_tick_time), () => heal_tick_vigor(target.Object, dice));
            heal_count += 1;
        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    // end while

    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Vigor OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Vigor OnEndSpellCast");
    }
    public static void heal_tick_vigor(GameObject target, Dice dice)
    {
        target.Heal(null, dice);
        target.HealSubdual(null, dice);
    }

}