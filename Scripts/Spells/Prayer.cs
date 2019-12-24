
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(359)]
    public class Prayer : BaseSpellScript
    {
        private const int D20_MODS_SPELLS_F_PRAYER_NEGATIVE = -4;

        private const int D20_MODS_SPELLS_F_PRAYER_POSITIVE = -3;

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
