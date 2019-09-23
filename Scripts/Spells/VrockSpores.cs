
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
    [SpellScript(602)]
    public class VrockSpores : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Vrock Spores OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Vrock Spores OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            spell.duration = 10;
            var damage_dice = Dice.D8;
            AttachParticles("Mon-Vrock-Spores", spell.caster);
            foreach (var target_item in spell.Targets)
            {
                if (!target_item.Object.IsMonsterSubtype(MonsterSubtype.demon))
                {
                    // damage 1-8 (no save)
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.Poison, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    // add SPORE condition
                    target_item.Object.AddCondition("sp-Vrock Spores", spell.spellId, spell.duration, 0);
                    target_item.ParticleSystem = AttachParticles("Mon-Vrock-Spores-Hit", target_item.Object);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Vrock Spores OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Vrock Spores OnEndSpellCast");
        }

    }
}
