
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
                spell.Targets.Length = 1;
                spell.Targets[0].Object = spell.caster;
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
