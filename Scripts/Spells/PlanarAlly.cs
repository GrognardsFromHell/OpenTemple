
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
}
