
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
    [SpellScript(56)]
    public class CharmPerson : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Charm Person OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Charm Person OnSpellEffect");
            spell.duration = 600 * spell.casterLevel;

            var target_item = spell.Targets[0];

            if (!target_item.Object.IsFriendly(spell.caster))
            {
                if ((target_item.Object.IsMonsterCategory(MonsterCategory.humanoid)) && (GameSystems.Stat.DispatchGetSizeCategory(target_item.Object) < SizeCategory.Large))
                {
                    if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        spell.caster.AddAIFollower(target_item.Object);
                        target_item.Object.AddCondition("sp-Charm Person", spell.spellId, spell.duration, GameSystems.Critter.GetHitDiceNum(target_item.Object));
                        target_item.ParticleSystem = AttachParticles("sp-Charm Person", target_item.Object);

                        target_item.Object.AddToInitiative();
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                        spell.RemoveTarget(target_item.Object);
                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31004);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }
            else
            {
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Charm Person OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Charm Person OnEndSpellCast");
        }


    }
}
