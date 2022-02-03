
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

namespace Scripts.Spells;

[SpellScript(246)]
public class InflictCriticalWounds : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Inflict Critical Wounds OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Inflict Critical Wounds OnSpellEffect");
        var dice = Dice.Parse("4d8");
        dice = dice.WithModifier(Math.Min(20, spell.casterLevel));
        var target = spell.Targets[0];
        var npc = spell.caster; // added so NPC's will choose valid targets
        if (npc.type != ObjectType.pc && npc.GetLeader() == null)
        {
            if (!Utilities.critter_is_unconscious(target.Object) && !target.Object.D20Query(D20DispatcherKey.QUE_Prone))
            {
                npc = spell.caster;
            }
            else
            {
                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 10 && !Utilities.critter_is_unconscious(obj) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target.Object = obj;
                    }

                }

            }

        }

        // check if target is friendly (willing target)
        if (target.Object.IsFriendly(spell.caster))
        {
            // check if target is undead
            if (target.Object.IsMonsterCategory(MonsterCategory.undead))
            {
                target.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
            }
            else
            {
                // damage target
                if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    // saving throw succesful, damage target, 1/2 damage
                    target.Object.DealReducedSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                }
                else
                {
                    target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // saving throw unsuccesful, damage target, full damage
                    target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }

            }

        }
        else
        {
            var attack_result = spell.caster.PerformTouchAttack(target.Object);
            if ((attack_result & D20CAF.HIT) != D20CAF.NONE)
            {
                // check if target is undead
                if (target.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    // check saving throw, heal target
                    if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // target.obj.float_mesfile_line( 'mes\\spell.mes', 30001 )
                        // saving throw succesful, heal target, 1/2 heal
                        target.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        // target.obj.float_mesfile_line( 'mes\\spell.mes', 30002 )
                        // saving throw unsuccesful, heal target, full heal
                        target.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    // check saving throw, damage target
                    if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        // saving throw succesful, damage target, 1/2 damage
                        target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId, attack_result, 0);
                    }
                    else
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        // saving throw unsuccesful, damage target, full damage
                        target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, attack_result, 0);
                    }

                }

            }

        }

        AttachParticles("sp-Inflict Critical Wounds", target.Object);
        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Inflict Critical Wounds OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Inflict Critical Wounds OnEndSpellCast");
    }

}