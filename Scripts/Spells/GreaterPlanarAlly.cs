
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

[SpellScript(206)]
public class GreaterPlanarAlly : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        var selection = PlanarAllies.choose_allies(spell.caster, 6, 18, 3);
        spell.duration = 10 * spell.casterLevel;
        SpawnParticles("sp-Summon Monster V", spell.aoeCenter);
        foreach (var mon in selection)
        {
            spell.SummonMonsters(true, mon);
        }

        // There seems to be no provision for experience costs, so
        // this is the best we can do.
        spell.caster.AwardExperience(-500);
        spell.EndSpell();
    }

}