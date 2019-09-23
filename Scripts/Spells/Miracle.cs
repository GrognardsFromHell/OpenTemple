
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
    [SpellScript(313)]
    public class Miracle : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Miracle OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            foreach (var target_item in spell.Targets)
            {
                if (!target_item.Object.IsFriendly(spell.caster))
                {
                    spell.RemoveTarget(target_item.Object);
                }

            }

        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Miracle OnSpellEffect");
            spell.duration = 0;
            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.IsFriendly(spell.caster))
                {
                    if (target_item.Object.GetStat(Stat.hp_current) <= -10)
                    {
                        target_item.Object.AddCondition("sp-Raise Dead", spell.spellId, spell.duration, 0);
                    }
                    else
                    {
                        target_item.Object.AddCondition("sp-Heal", spell.spellId, spell.duration, 0);
                        AttachParticles("sp-Cure Critical Wounds", target_item.Object);
                    }

                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Miracle OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Miracle OnEndSpellCast");
        }

    }
}
