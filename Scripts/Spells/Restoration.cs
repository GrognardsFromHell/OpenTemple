
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

[SpellScript(401)]
public class Restoration : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Restoration OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Restoration OnSpellEffect");
        spell.duration = 0;
        var target_item = spell.Targets[0];
        // get the ability type (from radial menu, -1 to offset index and D20Strength == 0)
        var ability_type = (spell.GetMenuArg(RadialMenuParam.MinSetting) - 1);
        // Solves Radial menu problem for Wands/NPCs
        if (ability_type != 0 && ability_type != 1 && ability_type != 2 && ability_type != 3 && ability_type != 4 && ability_type != 5)
        {
            ability_type = RandomRange(1, 6);
            ability_type = ability_type - 1;
        }

        Logger.Info("{0}", "Restoration Ability type: " + ability_type.ToString());
        if (target_item.Object.IsFriendly(spell.caster))
        {
            target_item.Object.AddCondition("sp-Restoration", spell.spellId, spell.duration, ability_type);
            target_item.ParticleSystem = AttachParticles("sp-Restoration", target_item.Object);
        }
        else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            // saving throw unsuccesful
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
            target_item.Object.AddCondition("sp-Restoration", spell.spellId, spell.duration, ability_type);
            target_item.ParticleSystem = AttachParticles("sp-Restoration", target_item.Object);
        }
        else
        {
            // saving throw successful
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target_item.Object);
        }

        spell.RemoveTarget(target_item.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Restoration OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Restoration OnEndSpellCast");
    }

}