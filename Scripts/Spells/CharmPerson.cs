
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
            var target_item_obj = target_item.Object;
            if (Co8Settings.CharmSpellDCModifier)
            {
                if (GameSystems.Combat.IsCombatActive())
                {
                    spell.dc = spell.dc - 5; // to reflect a bonus to the saving throw for casting charm in combat
                }

            }

            if ((!((GameSystems.Party.PartyMembers).Contains(spell.caster))) && (target_item.Object.type != ObjectType.pc) && ((GameSystems.Party.PartyMembers).Contains(target_item.Object)))
            {
                // NPC enemy is trying to charm an NPC from your party - this is bad because it effectively kills the NPC (is dismissed from party and becomes hostile, thus becoming unrecruitable unless you use dominate person/monster)
                target_item_obj = Utilities.party_closest(spell.caster, mode_select: 0); // select nearest conscious PC instead, who isn't already charmed
                if (target_item_obj == null)
                {
                    target_item_obj = target_item.Object;
                }

            }

            if (!target_item_obj.IsFriendly(spell.caster))
            {
                if ((target_item_obj.IsMonsterCategory(MonsterCategory.humanoid)) && (GameSystems.Stat.DispatchGetSizeCategory(target_item_obj) < SizeCategory.Large))
                {
                    if (!target_item_obj.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw unsuccessful
                        target_item_obj.FloatMesFileLine("mes/spell.mes", 30002);
                        spell.caster.AddAIFollower(target_item_obj);
                        target_item_obj.AddCondition("sp-Charm Person", spell.spellId, spell.duration, GameSystems.Critter.GetHitDiceNum(target_item.Object));
                        target_item.ParticleSystem = AttachParticles("sp-Charm Person", target_item_obj);
                        // add target to initiative, just in case
                        target_item_obj.AddToInitiative();
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                    }
                    else
                    {
                        // saving throw successful
                        target_item_obj.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item_obj);
                        spell.RemoveTarget(target_item_obj);
                    }

                }
                else
                {
                    // not a humanoid
                    target_item_obj.FloatMesFileLine("mes/spell.mes", 30000);
                    target_item_obj.FloatMesFileLine("mes/spell.mes", 31004);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item_obj);
                }

            }
            else
            {
                // can't target friendlies
                AttachParticles("Fizzle", target_item_obj);
                spell.RemoveTarget(target_item_obj);
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
