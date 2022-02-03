
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

[SpellScript(62)]
public class ClairaudienceClairvoyance : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Clairaudience/Clairvoyance OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-divination-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Clairaudience/Clairvoyance OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;
        // HTN - WIP! temporary until we have a func() that clears up a section of fog
        var partsys_id = AttachParticles("sp-Clairaudience-Clairvoyance", spell.caster);
        spell.caster.AddCondition("sp-Clairaudience Clairvoyance", spell.spellId, spell.duration, partsys_id);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Clairaudience/Clairvoyance OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Clairaudience/Clairvoyance OnEndSpellCast");
    }

}