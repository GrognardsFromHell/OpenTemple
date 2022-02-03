
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

[SpellScript(113)]
public class DetectLaw : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Detect Law OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-divination-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Detect Law OnSpellEffect");
        spell.duration = 100 * spell.casterLevel;

        var target = spell.Targets[0];

        target.Object.AddCondition("sp-Detect Law", spell.spellId, spell.duration, 0);
        target.ParticleSystem = AttachParticles("sp-Detect Alignment", target.Object);

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Detect Law OnBeginRound");
        foreach (var obj in ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, spell.spellRange, -45, 90))
        {
            if ((obj.GetAlignment().IsLawful()))
            {
                AttachParticles("sp-Detect Alignment Law", obj);
            }

        }

    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Detect Law OnEndSpellCast");
    }


}