using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public class TemplePlusMonsterAbilities
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public static readonly ConditionSpec Rend = ConditionSpec.Create("Rend", 8)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, (in DispatcherCallbackArgs evt) =>
            {
                DispIoDamage dispIo = evt.GetDispIoDamage();
                GameObjectBody weapon = dispIo.attackPacket.GetWeaponUsed();
                if (weapon != null)
                    return;

                var dmgPacket = dispIo.damage;
                var attackDescr = dmgPacket.dice[0].typeDescription; // e.g. Claw etc.
                var hasDeliveredDamage = evt.GetConditionArg1();
                var previousAttackDescr = evt.GetConditionStringArg(1);
                var previousTarget = evt.GetConditionObjArg(2);
                if (hasDeliveredDamage != 0 && attackDescr == previousAttackDescr &&
                    previousTarget == dispIo.attackPacket.victim)
                {
                    var dice = new Dice(2, 6, 9);
                    dispIo.damage.AddDamageDice(dice, DamageType.PiercingAndSlashing, 133);
                    //GameSystems.D20.Combat.AddDamageDice(&dispIo.damage, dice.ToPacked(), DamageType.PiercingAndSlashing, 133);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 203);
                    evt.SetConditionArg1(0);
                }
                else
                {
                    evt.SetConditionArg1(1);
                    evt.SetConditionStringArg(1, attackDescr);
                    evt.SetConditionObjArg(2, dispIo.attackPacket.victim);
                }
            })
            .AddHandler(DispatcherType.BeginRound, (in DispatcherCallbackArgs evt) =>
            {
                // TODO: I  don't think Rend actually uses 8 args
                for (int i = 0; i < 8; i++)
                {
                    evt.SetConditionArg(i, 0);
                }
            })
            .Build();

        public static readonly ConditionSpec CaptivatingSong = ConditionSpec.Create("Captivating Song", 8)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, (in DispatcherCallbackArgs evt) =>
            {
                int spellId = evt.GetConditionArg1();
                // int duration = conds.CondNodeGetArg(evt.subDispNode.condNode, 1);
                int duration = 2;
                if (spellId == 0)
                    return;
                var spellPktBody = GameSystems.Spell.GetActiveSpell(spellId);
                var singer = spellPktBody.caster;
                evt.SetConditionObjectIdArg(2, singer.id);
                evt.objHndCaller.AddCondition("Captivated", duration, singer);
            })
            .Build();

        private static void CountDownDuration(in DispatcherCallbackArgs evt, int argIdx)
        {
            var condArg = evt.GetConditionArg(argIdx);
            var dispIo = evt.GetDispIoD20Signal();
            int durationRemaining = condArg - dispIo.data1;
            if (durationRemaining >= 0)
            {
                evt.SetConditionArg(argIdx, durationRemaining);
            }
            else
            {
                evt.RemoveThisCondition();
            }
        }

        public static readonly ConditionSpec Captivated = ConditionSpec.Create("Captivated", 8)
            .SetUnique()
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_CannotCast, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, (in DispatcherCallbackArgs evt) => evt.RemoveThisCondition())
            .AddUniqueTooltip(205)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "Bardic-Fascinate-hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId,
                1, "Bardic-Fascinate-hit")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddHandler(DispatcherType.BeginRound, CountDownDuration, 0)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .AddHandler(DispatcherType.EffectTooltip, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIoEffectTooltip();
                int durationRemaining = evt.GetConditionArg1();
                var extraText = $"\n{durationRemaining} rounds remaining.";
                dispIo.bdb.AddEntry(BuffDebuffType.Debuff, 100, extraText,
                    90000); // will fetch 90000 from spell_ext.mes (Captivated!)
            })
            .Build();

        // Hezrou Stench
        public static readonly ConditionSpec HezrouStench = ConditionSpec
            .Create("Hezrou Stench", 4) // 0 - spellId; 1 - duration; 2 - eventId; 3 - partsysId;
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, (in DispatcherCallbackArgs evt) =>
            {
                var spellId = evt.GetConditionArg1();
                var spellPkt = GameSystems.Spell.GetActiveSpell(spellId);
                if (spellId == 0)
                {
                    return;
                }

                var spellEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);

                var evtId = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 0, 1, ObjectListFilter.OLC_CRITTERS,
                    locXY.INCH_PER_FEET * spellEntry.radiusTarget, 0.0f, MathF.PI * 2);

                evt.SetConditionArg3(evtId);
                GameSystems.Spell.UpdateSpellPacket(spellPkt);

                spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            })
            .AddSignalHandler(D20DispatcherKey.SIG_Spell_End, SpellEffects.AoESpellRemove, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, SpellEffects.AoESpellRemove, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Critter_Killed, SpellEffects.AoESpellRemove, 0)
            .AddHandler(DispatcherType.ObjectEvent,
                (in DispatcherCallbackArgs evt) => HezrouStenchObjectEvent(in evt, 0))
            .Build();

        private static void HezrouStenchObjectEvent(in DispatcherCallbackArgs evt, int condMode)
        {
            var dispIo = evt.GetDispIoObjEvent();
            var condEvtId = evt.GetConditionArg3();
            if (dispIo.evtId == condEvtId)
            {
                var spellId = evt.GetConditionArg1();
                var spellPkt = GameSystems.Spell.GetActiveSpell(spellId);
                if (spellPkt.spellId == 0)
                {
                    Logger.Warn("HezrouStenchObjEvent: Unable to fetch spell! ID {0}", spellId);
                    return;
                }

                /*
                AoE Entered;
                - add the target to the Spell's Target List
                - Do a saving throw
                */
                if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                {
                    /*
                        Hezrou Stench condition (the one applied to the SpellObject)
                    */
                    if (condMode == 0)
                    {
                        // if already has the condition - skip
                        if (dispIo.tgt.HasCondition(HezrouStenchHit))
                            return;
                        if (GameSystems.D20.D20Query(dispIo.tgt, D20DispatcherKey.QUE_Critter_Is_Immune_Poison))
                            return;
                        if (GameSystems.Critter.IsCategorySubtype(dispIo.tgt, MonsterSubtype.demon))
                            return;
                        if (GameSystems.Critter.IsCategory(dispIo.tgt, MonsterCategory.elemental))
                            return;

                        GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.SpellStruck);
                        GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.AreaOfEffectHit);

                        // Hezrou Stench does not provoke Spell Resistance
                        /*if (GameSystems.Spell.CheckSpellResistance(&spellPkt, dispIo.tgt) == 1)
                            return;*/

                        var partsysId = GameSystems.ParticleSys.CreateAtObj("sp-Stinking Cloud Hit", dispIo.tgt);
                        spellPkt.AddTarget(dispIo.tgt, partsysId, true);
                        // save succeeds - apply Sickened
                        if (GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, 24,
                            SavingThrowType.Fortitude, 0, spellPkt.spellId))
                        {
                            dispIo.tgt.AddCondition(HezrouStenchHit, spellPkt.spellId,
                                spellPkt.durationRemaining, dispIo.evtId, partsysId, 1);
                            GameSystems.Spell.FloatSpellLine(dispIo.tgt, 20026, TextFloaterColor.Red);
                        }
                        // save failed - apply nauseated
                        else
                        {
                            dispIo.tgt.AddCondition(HezrouStenchHit, spellPkt.spellId,
                                spellPkt.durationRemaining, dispIo.evtId, partsysId, 0);
                            GameSystems.D20.Combat.FloatCombatLine(dispIo.tgt, 150, forcedColor: TextFloaterColor.Red);
                        }
                    }
                    /*
                        "Hezrou Stench Hit" condition (the one applied to the critter from the above)
                    */
                    else if (condMode == 1)
                    {
                        if (evt.GetConditionArg(4) == 2) // "cured"
                        {
                            evt.SetConditionArg(4, 1); // re-establish sickness when stepping into Hezrou AoE
                        }
                    }
                }
                /*
                AoE exited;
                - If Nauseated (identified by arg3= 0), apply a 1d4 duration
                - If Sickened, changed to "cured" (arg3 = 2)
                */
                else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                {
                    if (condMode == 1)
                    {
                        if (evt.GetConditionArg(4) == 1) // sickened
                        {
                            evt.SetConditionArg(4, 2);
                            // GameSystems.RollHistory.CreateFromFreeText($"{GameSystems.MapObject.GetDisplayName(dispIo.tgt)} exited Stinking Cloud; Nauseated for {rollResult} more rounds.\n".c_str());
                        }
                        else if (evt.GetConditionArg(4) == 0) // nauseated
                        {
                            var rollResult = Dice.Roll(1, 4, 0); // new duration
                            evt.SetConditionArg2(rollResult);
                            GameSystems.RollHistory.CreateFromFreeText(
                                $"{GameSystems.MapObject.GetDisplayName(dispIo.tgt)} exited Hezrou Stench; Nauseated for {rollResult} more rounds.\n"
                            );
                        }
                    }
                }

                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellId);
            }
        }

        private static void SkillBonus(in DispatcherCallbackArgs evt, SkillId? skill, int bonusValue)
        {
            var spellId = evt.GetConditionArg1();
            var spellPkt = GameSystems.Spell.GetActiveSpell(spellId);

            int bonType = 0; // will stack if 0

            var usedSkill = evt.GetSkillIdFromDispatcherKey();
            if (!skill.HasValue || skill.Value == usedSkill)
            {
                var dispIo = evt.GetDispIoObjBonus();
                var spellName = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
                dispIo.bonOut.AddBonus(bonusValue, bonType, 113, spellName); // 113 is ~Spell~[TAG_SPELLS] in bonus.mes
            }
        }

        // Hezrou Stench Nausea / Sickness
        // Args: 0 - spellId; 1 - duration; 2 - eventId; 3 - partsysId; 4 - nausea/sickness (0 = nausea, 1 = sickness)
        public static readonly ConditionSpec HezrouStenchHit = ConditionSpec.Create("Hezrou Stench Hit", 5)
            .SetUnique()
            .AddHandler(DispatcherType.BeginRound, (in DispatcherCallbackArgs evt) =>
            {
                var effectType = evt.GetConditionArg(4);
                /*
                count down for nauseated only
                */
                if (effectType != 0)
                {
                    return;
                }

                var dispIo = evt.GetDispIoD20Signal();

                // new duration
                int durationRem = (int) evt.GetConditionArg2() - (int) dispIo.data1;
                evt.SetConditionArg2(durationRem);

                // if duration drops below 0, change to "Cured" status
                // (the assumption is that this will only happen for the 1d4 countdown, i.e. after you leave the hezrou area)
                if (durationRem < 0)
                {
                    evt.SetConditionArg(4, 2);
                }
            })
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                (in DispatcherCallbackArgs evt) => evt.RemoveThisCondition())
            .AddHandler(DispatcherType.ObjectEvent,
                (in DispatcherCallbackArgs evt) => HezrouStenchObjectEvent(in evt, 1))
            .AddHandler(DispatcherType.TurnBasedStatusInit, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIOTurnBasedStatus();
                if (evt.GetConditionArg(4) == 0)
                {
                    var tbStat = dispIo.tbStatus;
                    if (tbStat != null)
                    {
                        if (tbStat.hourglassState > HourglassState.MOVE)
                        {
                            tbStat.hourglassState = HourglassState.MOVE;
                        }
                    }
                }
            })
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIoD20Query();
                // if nauseated
                if (dispIo.return_val != 0 && evt.GetConditionArg(4) == 0)
                {
                    dispIo.return_val = 0;
                }
            })
            .AddHandler(DispatcherType.SkillLevel, SkillBonus, (SkillId?) null, -2)
            .AddHandler(DispatcherType.AbilityCheckModifier, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg(4) > 1)
                    return;

                var dispIo = evt.GetDispIoObjBonus();

                dispIo.bonOut.AddBonus(-2, 0, 345);
            })
            .AddHandler(DispatcherType.SaveThrowLevel, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg(4) > 1)
                    return;

                var dispIo = evt.GetDispIoSavingThrow();
                dispIo.bonlist.AddBonus(-2, 0, 345);
            })
            .AddHandler(DispatcherType.DealingDamage2, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg(4) > 1)
                    return;

                var dispIo = evt.GetDispIoDamage();
                dispIo.damage.bonuses.AddBonus(-2, 0, 345);
            })
            .AddHandler(DispatcherType.ToHitBonus2, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg(4) > 1)
                    return;

                var dispIo = evt.GetDispIoAttackBonus();
                dispIo.bonlist.AddBonus(-2, 0, 345);
            })
            .AddHandler(DispatcherType.EffectTooltip, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg(4) > 1)
                    return;

                var dispIo = evt.GetDispIoEffectTooltip();
                var spellId = evt.GetConditionArg1();
                var spellPkt = GameSystems.Spell.GetActiveSpell(spellId);

                /*
                    nauseated
                */
                if (evt.GetConditionArg(4) == 0)
                {
                    var remainingDuration = evt.GetConditionArg2();
                    if (remainingDuration < 5)
                    {
                        var suffix = $"\n {GameSystems.D20.Combat.GetCombatMesLine(175)}: {remainingDuration}";
                        dispIo.bdb.AddEntry(BuffDebuffType.Debuff, 141, suffix, spellPkt.spellEnum);
                        return;
                    }
                }

                dispIo.bdb.AddEntry(BuffDebuffType.Debuff, 141, null, spellPkt.spellEnum);
            })
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg(4) == 1)
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 206, forcedColor: TextFloaterColor.White);
                else if (evt.GetConditionArg(4) == 0)
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 207, forcedColor: TextFloaterColor.White);
                evt.SetConditionArg(4, 2);
            })
            .SupportHasConditionQuery()
            .RemovedBy(SpellEffects.SpellNeutralizePoison)
            .Build(); // make neutralie poison remove existing stench effect
    }
}