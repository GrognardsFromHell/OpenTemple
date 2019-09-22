
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(149)]
    public class EndureElements : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Endure Elements OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Endure Elements OnSpellEffect");
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);

            SpellDescriptor element_type; // DECL_PULL_UP
            string partsys_type; // DECL_PULL_UP
            if (spell_arg == 1)
            {
                element_type = SpellDescriptor.ACID;

                partsys_type = "sp-Endure Elements-acid";

            }
            else if (spell_arg == 2)
            {
                element_type = SpellDescriptor.COLD;

                partsys_type = "sp-Endure Elements-cold";

            }
            else if (spell_arg == 3)
            {
                element_type = SpellDescriptor.ELECTRICITY;

                partsys_type = "sp-Endure Elements-water";

            }
            else if (spell_arg == 4)
            {
                element_type = SpellDescriptor.FIRE;

                partsys_type = "sp-Endure Elements-fire";

            }
            else if (spell_arg == 5)
            {
                element_type = SpellDescriptor.SONIC;

                partsys_type = "sp-Endure Elements-Sonic";

            }
            else
            {
                Logger.Error("Endure Elements triggered with unknown spell arg: {0}", spell_arg);
                return;
            }

            spell.duration = 14400;

            var target_item = spell.Targets[0];

            target_item.Object.AddCondition("sp-Endure Elements", spell.spellId, element_type, spell.duration);
            target_item.ParticleSystem = AttachParticles(partsys_type, target_item.Object);

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Endure Elements OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Endure Elements OnEndSpellCast");
        }


    }
}
