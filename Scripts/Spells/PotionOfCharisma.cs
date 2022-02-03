
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

[SpellScript(705)]
public class PotionOfCharisma : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Potion of charisma OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Potion of charisma OnSpellEffect");
        spell.duration = 1800;
        var target = spell.Targets[0];
        var dice = Dice.D4;
        dice = dice.WithModifier(1);
        var bonus = dice.Roll();
        target.Object.AddCondition("sp-Potion of charisma", spell.spellId, bonus, spell.duration);
    }
    // target.partsys_id = game.particles( 'sp-Potion of charisma', spell.caster )
    // spell.target_list.remove_target( target.obj )
    // spell.spell_end( spell.id )

    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Potion of charisma OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Potion of charisma OnEndSpellCast");
    }

}