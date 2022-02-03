
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
    [SpellScript(232)]
    public class HolySword : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Holy Sword OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Holy Sword OnSpellEffect");
            // game.particles("sp-Raise Dead", spell.caster)
            spell.duration = spell.casterLevel;
            var y = spell.caster.FindItemByProto(4999);
            if (spell.spellKnownSlotLevel > 3 && y == null)
            {
                AttachParticles("sp-Shillelagh", spell.caster);
                Utilities.create_item_in_inventory(4999, spell.caster);
                spell.ClearTargets();
                spell.AddTarget(spell.caster);
                spell.caster.AddCondition("sp-Magic Circle Outward", spell.spellId, spell.duration, 2);
            }
            else
            {
                if (spell.spellKnownSlotLevel < 4)
                {
                    spell.caster.FloatMesFileLine("mes/spell.mes", 16008);
                }

                if (y != null)
                {
                    spell.caster.FloatMesFileLine("mes/spell.mes", 16007);
                }

                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Holy Sword OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Holy Sword OnEndSpellCast");
            var x = spell.caster.FindItemByProto(4999);
            if (x != null)
            {
                x.Destroy();
            }

        }

    }
}
