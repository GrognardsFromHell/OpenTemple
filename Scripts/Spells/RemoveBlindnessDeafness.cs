
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

namespace Scripts.Spells
{
    [SpellScript(390)]
    public class RemoveBlindnessDeafness : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Remove Blindness/Deafness OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Remove Blindness/Deafness OnSpellEffect");
            spell.duration = 0;
            var target = spell.Targets[0];
            // Solves Radial menu problem for Wands/NPCs
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (spell_arg != 1 && spell_arg != 2)
            {
                spell_arg = 2;
            }

            AttachParticles("sp-Remove Blindness Deafness", target.Object);
            if (spell_arg == 1)
            {
                // apply remove blindness
                target.Object.AddCondition("sp-Remove Blindness", spell.spellId, spell.duration, 0);
            }
            else
            {
                // apply deafness
                target.Object.AddCondition("sp-Remove Deafness", spell.spellId, spell.duration, 0);
            }

            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Remove Blindness/Deafness OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Remove Blindness/Deafness OnEndSpellCast");
        }

    }
}
