
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
    [SpellScript(769)]
    public class PowerSight : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Power Sight OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-divination-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Power Sight OnSpellEffect");
            spell.duration = 0;
            var target_item = spell.Targets[0];
            var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
            // game.global_vars[555] = obj_hit_dice
            var fin = obj_hit_dice + 100;
            if (fin > 120)
            {
                fin = 121;
            }

            target_item.Object.FloatMesFileLine("mes/dice_rolls.mes", 11, TextFloaterColor.LightBlue);
            target_item.Object.FloatMesFileLine("mes/dice_rolls.mes", fin, TextFloaterColor.LightBlue);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Power Sight OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Power Sight OnEndSpellCast");
        }

    }
}
