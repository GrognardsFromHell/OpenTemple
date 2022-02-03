
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

[SpellScript(348)]
public class PlanarAlly : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        spell.duration = 10 * spell.casterLevel;
        var selection = PlanarAllies.choose_allies(spell.caster, 4, 12, 2);
        SpawnParticles("sp-Summon Monster IV", spell.aoeCenter);
        foreach (var n in selection)
        {
            spell.SummonMonsters(true, n);
        }

        spell.caster.AwardExperience(-250);
        spell.EndSpell();
    }

}