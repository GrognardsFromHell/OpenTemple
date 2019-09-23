
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
    [SpellScript(195)]
    public class GiantVermin : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Giant Vermin OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Giant Vermin OnSpellEffect");
            spell.duration = 10 * spell.casterLevel; // fixed - should be 1min/level
                                                     // get the proto_id for this monster (from radial menu)
                                                     // Solves Radial menu problem for Wands/NPCs
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (spell_arg != 14089 && spell_arg != 14047 && spell_arg != 14417)
            {
                spell_arg = RandomRange(1, 3);
                if (spell_arg == 1)
                {
                    spell_arg = 14089;
                }
                else if (spell_arg == 2)
                {
                    spell_arg = 14047;
                }
                else if (spell_arg == 3)
                {
                    spell_arg = 14417;
                }

            }

            // create monster, monster should be added to target_list
            spell.SummonMonsters(true, spell_arg);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Giant Vermin OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Giant Vermin OnEndSpellCast");
        }

    }
}
