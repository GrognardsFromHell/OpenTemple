
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells;

[SpellScript(173)]
public class FireShield : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnSpellEffect");
        var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);

        SpellDescriptor element_type; // DECL_PULL_UP
        string partsys_type; // DECL_PULL_UP
        if (spell_arg == 1)
        {
            element_type = SpellDescriptor.COLD;

            partsys_type = "sp-Fire Shield-Cold";

        }
        else if (spell_arg == 2)
        {
            element_type = SpellDescriptor.FIRE;

            partsys_type = "sp-Fire Shield-Warm";

        }
        else
        {
            Logger.Error("Fire shield triggered with unknown spell arg: {0}", spell_arg);
            return;
        }

        spell.duration = 1 * spell.casterLevel;

        var target_item = spell.Targets[0];

        target_item.Object.AddCondition("sp-Fire Shield", spell.spellId, spell.duration, element_type);
        target_item.ParticleSystem = AttachParticles(partsys_type, target_item.Object);

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fire Shield OnEndSpellCast");
    }


}