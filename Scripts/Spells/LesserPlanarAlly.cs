
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(270)]
    public class LesserPlanarAlly : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            var selection = PlanarAllies.choose_allies(spell.caster, 3, 6, 1);
            spell.duration = 10 * spell.casterLevel;
            SpawnParticles("sp-Summon Monster III", spell.aoeCenter);
            spell.SummonMonsters(true, selection[0]);
            // There seems to be no provision for experience costs, so
            // this is the best we can do.
            spell.caster.AwardExperience(-100);
            spell.EndSpell();
        }

    }
}
