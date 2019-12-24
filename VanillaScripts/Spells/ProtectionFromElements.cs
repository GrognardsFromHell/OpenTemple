
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

namespace VanillaScripts.Spells
{
    [SpellScript(369)]
    public class ProtectionFromElements : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnSpellEffect");
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);

            SpellDescriptor element_type; // DECL_PULL_UP
            string partsys_type; // DECL_PULL_UP
            if (spell_arg == 1)
            {
                element_type = SpellDescriptor.ACID;

                partsys_type = "sp-Protection From Elements-acid";

            }
            else if (spell_arg == 2)
            {
                element_type = SpellDescriptor.COLD;

                partsys_type = "sp-Protection From Elements-cold";

            }
            else if (spell_arg == 3)
            {
                element_type = SpellDescriptor.ELECTRICITY;

                partsys_type = "sp-Protection From Elements-electricity";

            }
            else if (spell_arg == 4)
            {
                element_type = SpellDescriptor.FIRE;

                partsys_type = "sp-Protection From Elements-fire";

            }
            else if (spell_arg == 5)
            {
                element_type = SpellDescriptor.SONIC;

                partsys_type = "sp-Protection From Elements-sonic";

            }
            else
            {
                Logger.Error("Protection from elements called with invalid spell arg {0}", spell_arg);
                return;
            }

            spell.duration = 100 * spell.casterLevel;

            var target_item = spell.Targets[0];

            target_item.Object.AddCondition("sp-Protection From Elements", spell.spellId, element_type, spell.duration);
            target_item.ParticleSystem = AttachParticles(partsys_type, target_item.Object);

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnEndSpellCast");
        }


    }
}
