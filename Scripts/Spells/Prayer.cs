
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
    [SpellScript(359)]
    public class Prayer : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Prayer OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Prayer OnSpellEffect");
            // remove_list = []
            spell.duration = 1 * spell.casterLevel;
            SpawnParticles("sp-Prayer Burst", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.IsFriendly(spell.caster))
                {
                    target_item.Object.AddCondition("sp-Prayer", spell.spellId, spell.duration, D20_MODS_SPELLS_F_PRAYER_POSITIVE);
                    target_item.ParticleSystem = AttachParticles("sp-Prayer Burst Favor", target_item.Object);
                }
                else
                {
                    target_item.Object.AddCondition("sp-Prayer", spell.spellId, spell.duration, D20_MODS_SPELLS_F_PRAYER_NEGATIVE);
                    target_item.ParticleSystem = AttachParticles("sp-Prayer Burst DisFavor", target_item.Object);
                }

            }

            // spell.target_list.remove_list( remove_list )
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Prayer OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Prayer OnEndSpellCast");
        }

    }
}
