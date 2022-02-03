
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

[SpellScript(125)]
public class DiscernLies : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Discern Lies OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-divination-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Discern Lies OnSpellEffect");
        // Dar's level check no longer needed thanks to Spellslinger's dll fix
        // if spell.caster_class == 13: #added to check for proper paladin slot level (darmagon)
        // if spell.spell_level < 3:
        // spell.caster.float_mesfile_line('mes\\spell.mes', 16008)
        // spell.spell_end(spell.id)
        // return
        spell.duration = 1 * spell.casterLevel;
        var target = spell.Targets[0];
        target.Object.AddCondition("sp-Discern Lies", spell.spellId, spell.duration, 0);
        target.ParticleSystem = AttachParticles("sp-Discern Lies", spell.caster);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Discern Lies OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Discern Lies OnEndSpellCast");
    }

}