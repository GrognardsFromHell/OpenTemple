using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Utils;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    [AutoRegister]
    public static class DomainConditions
    {
        [TempleDllLocation(0x102b1620)]
        public static readonly ConditionSpec AnimalDomain = ConditionSpec.Create("Animal Domain", 1)
            .SetUnique()
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
            .AddHandler(DispatcherType.RadialMenuEntry, AnimalDomainRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 122, AnimalDomainPerformAction)
            .Build();


        [TempleDllLocation(0x102b0ec0)]
        public static readonly ConditionSpec DeathDomain = ConditionSpec.Create("Death Domain", 1)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.RadialMenuEntry, DeathTouchRadial)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_COUNTERSPELL,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_COUNTERSPELL,
                DeathDomainD20ACheck)
            .Build();


        [TempleDllLocation(0x102b0d48)]
        public static readonly ConditionSpec TurnUndead = ConditionSpec.Create("Turn Undead", 2)
            .SetUniqueWithKeyArg1()
            .AddHandler(DispatcherType.ConditionAdd, TurnUndeadInitNumPerDay)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, TurnUndeadInitNumPerDay)
            .AddHandler(DispatcherType.RadialMenuEntry, TurnUndeadRadialMenuEntry)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_SPELL, TurnUndead_Check)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_SPELL, TurnUndeadPerform)
            .Build();


        [TempleDllLocation(0x102b0e78)]
        public static readonly ConditionSpec ChaosDomain = ConditionSpec.Create("Chaos Domain", 0)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify,
                SpellDescriptor.CHAOTIC, 1)
            .Build();


        [TempleDllLocation(0x102b12b8)]
        public static readonly ConditionSpec ProtectionDomain = ConditionSpec.Create("Protection Domain", 1)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.RadialMenuEntry, ProtectiveWardRadial)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_ENTER,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_ENTER, GrantWardCondition)
            .Build();


        [TempleDllLocation(0x102b10f0)]
        public static readonly ConditionSpec GoodDomain = ConditionSpec.Create("Good Domain", 0)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify,
                SpellDescriptor.GOOD, 1)
            .Build();


        [TempleDllLocation(0x102b10a8)]
        public static readonly ConditionSpec EvilDomain = ConditionSpec.Create("Evil Domain", 0)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify,
                SpellDescriptor.EVIL, 1)
            .Build();


        [TempleDllLocation(0x102b1180)]
        public static readonly ConditionSpec LawDomain = ConditionSpec.Create("Law Domain", 0)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify,
                SpellDescriptor.LAWFUL, 1)
            .Build();


        [TempleDllLocation(0x102b15a0)]
        public static readonly ConditionSpec TravelDomain = ConditionSpec.Create("Travel Domain", 3)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, TravelDomainResetUses)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, TravelDomainResetUses)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, LuckDomainFreedomOfMovement)
            .AddHandler(DispatcherType.BeginRound, TravelClearParticles, 1)
            .Build();


        [TempleDllLocation(0x102b0de0)]
        public static readonly ConditionSpec GreaterTurning = ConditionSpec.Create("Greater Turning", 2)
            .PreventsWithSameArg1(TurnUndead)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 1)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, TurnUndeadInitNumPerDay)
            .AddHandler(DispatcherType.RadialMenuEntry, TurnUndeadRadialMenuEntry)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_SPELL, TurnUndead_Check)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_SPELL, TurnUndeadPerform)
            .Build();


        [TempleDllLocation(0x102b1210)]
        public static readonly ConditionSpec LuckDomain = ConditionSpec.Create("Luck Domain", 4)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.RadialMenuEntry, GoodFortune_RadialMenuEntry_Callback)
            .AddQueryHandler(D20DispatcherKey.QUE_RerollAttack, Luck_RerollQuery)
            .AddQueryHandler(D20DispatcherKey.QUE_RerollSavingThrow, Luck_RerollQuery)
            .AddQueryHandler(D20DispatcherKey.QUE_RerollCritical, Luck_RerollQuery)
            .Build();


        [TempleDllLocation(0x102b1438)]
        public static readonly ConditionSpec StrengthDomain = ConditionSpec.Create("Strength Domain", 1)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.RadialMenuEntry, FeatOfStrengthRadial)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_EXIT,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_EXIT,
                StrengthDomainFeatOfStrengthActivate)
            .Build();


        [TempleDllLocation(0x102b0f58)]
        public static readonly ConditionSpec DestructionDomain = ConditionSpec.Create("Destruction Domain", 1)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.NewDay, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddHandler(DispatcherType.RadialMenuEntry, DestructionDomainRadialMenu)
            .AddHandler(DispatcherType.DestructionDomain, (D20DispatcherKey) 322, DestructionDomainAddSmite)
            .Build();


        [TempleDllLocation(0x102b1138)]
        public static readonly ConditionSpec HealingDomain = ConditionSpec.Create("Healing Domain", 0)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, HealingDomainCasterLvlBonus, SubschoolOfMagic.Healing, 1)
            .Build();


        [TempleDllLocation(0x102b0fd8)]
        public static readonly ConditionSpec DestructionDomainSmite = ConditionSpec
            .Create("Destruction Domain Smite", 1)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, DestructionDomainAddToHitBonus)
            .AddHandler(DispatcherType.DealingDamage, DestructionDomainAddDamage)
            .AddHandler(DispatcherType.DealingDamage, CommonConditionCallbacks.conditionRemoveCallback)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-smite")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-smite")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
            .Build();


        [TempleDllLocation(0x102b1350)]
        public static readonly ConditionSpec ProtectionDomainWard = ConditionSpec.Create("Protection Domain Ward", 1)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.SaveThrowLevel, ProtectionDomainWard_SavingThrowCallback)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0,
                "sp-Protective Ward")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Protective Ward")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
            .AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0,
                "sp-Protective Ward-END")
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 79)
            .Build();


        [TempleDllLocation(0x102b14d0)]
        public static readonly ConditionSpec StrengthDomainFeat = ConditionSpec.Create("Strength Domain Feat", 1)
            .SetUnique()
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, FeatOfStrengthStatBonus)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0,
                "sp-Feat of Strength")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Feat of Strength")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
            .AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0,
                "sp-Feat of Strength-END")
            .AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 80)
            .Build();


        [TempleDllLocation(0x102b0bc0)]
        public static readonly ConditionSpec Turned = ConditionSpec.Create("Turned", 4)
            .SetUnique()
            .RemovedBy(Cowering)
            .RemovedBy(Commanded)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 10)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.conditionRemoveCallback)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Afraid, TurnedUndeadIsAfraid)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .SetQueryResult(D20DispatcherKey.QUE_Turned, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 85, 0)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 3,
                "sp-Turn Undead-Hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 3, "sp-Turn Undead-Hit")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 3)
            .AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 3,
                "sp-Turn Undead-END")
            .AddHandler(DispatcherType.ConditionRemove2, CoweringUndeadRemoved)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .Build();


        [TempleDllLocation(0x102b0a28)]
        public static readonly ConditionSpec Cowering = ConditionSpec.Create("Cowering", 2)
            .SetUnique()
            .RemovedBy(Turned)
            .RemovedBy(Commanded)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 10)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
            .AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.conditionRemoveCallback)
            .SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
            .SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
            .AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 187)
            .AddHandler(DispatcherType.GetAC, CoweringArmorMalus, -2, 187)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 86, 0)
            .SetQueryResult(D20DispatcherKey.QUE_Rebuked, true)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "sp-Rebuke Undead-Hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "sp-Rebuke Undead-Hit")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1,
                "sp-Rebuke Undead-END")
            .AddHandler(DispatcherType.ConditionRemove2, CoweringUndeadRemoved)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .Build();


        [TempleDllLocation(0x102b0940)]
        public static readonly ConditionSpec Commanded = ConditionSpec.Create("Commanded", 1)
            .SetUnique()
            .RemovedBy(Cowering)
            .RemovedBy(Turned)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 87, 0)
            .SetQueryResult(D20DispatcherKey.QUE_Commanded, true)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0,
                "sp-Commnad Undead-Hit")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Commnad Undead-Hit")
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
            .AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0,
                "sp-Commnad Undead-END")
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .Build();

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x1004bb40)]
        public static void ProtectionDomainWard_SavingThrowCallback(in DispatcherCallbackArgs evt)
        {
            var clericLevel = evt.objHndCaller.GetStat(Stat.level_cleric);
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(clericLevel, 0, 182);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x1004bd30)]
        public static void LuckDomainFreedomOfMovement(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((evt.GetConditionArg2()) != 0)
            {
                dispIo.return_val = 1;
            }
            else
            {
                var condArg1 = evt.GetConditionArg1();
                if (condArg1 > 0)
                {
                    GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
                    evt.SetConditionArg1(condArg1 - 1);
                    evt.SetConditionArg2(1);
                    var partSys = GameSystems.ParticleSys.CreateAtObj("sp-Luck Domain Reroll", evt.objHndCaller);
                    evt.SetConditionPartSysArg(2, (PartSys) partSys);
                    dispIo.return_val = 1;
                }
            }
        }


        [DispTypes(DispatcherType.BaseCasterLevelMod)]
        [TempleDllLocation(0x1004b430)]
        public static void HealingDomainCasterLvlBonus(in DispatcherCallbackArgs evt, SubschoolOfMagic data1, int data2)
        {
            var dispIo = evt.GetDispIoD20Query();
            var spellPacket = (SpellPacketBody) dispIo.obj;
            if (spellPacket.spellEnum != 0)
            {
                if (GameSystems.Spell.GetSpellSubSchool(spellPacket.spellEnum) == data1)
                {
                    dispIo.return_val += data2;
                }
            }
        }

        [TempleDllLocation(0x100f02c0)]
        private static bool DestructionDomainRadialCallback(GameObjectBody obj, ref RadialMenuEntry radEntry)
        {
            obj.DispatchDestructionDomainSignal(radEntry.dispKey);
            GameSystems.D20.RadialMenu.ClearActiveRadialMenu();
            return false;
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x1004b690)]
        public static void DestructionDomainRadialMenu(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = RadialMenuEntry.CreateCustom(5021, "TAG_DESTRUCTION_D", DestructionDomainRadialCallback);
            radMenuEntry.dispKey = D20DispatcherKey.SIG_Destruction_Domain_Smite;
            var node = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, node);
        }


        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x1004bbf0)]
        public static void StrengthDomainFeatOfStrengthActivate(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var condArg1 = evt.GetConditionArg1();
            evt.SetConditionArg1(condArg1 - 1);
            GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
            evt.objHndCaller.AddCondition(StrengthDomainFeat);
            dispIo.returnVal = 0;
        }


        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x1004bae0)]
        public static void GrantWardCondition(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var condArg1 = evt.GetConditionArg1();
            evt.SetConditionArg1(condArg1 - 1);
            GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
            dispIo.action.d20ATarget.AddCondition(ProtectionDomainWard);
            dispIo.returnVal = 0;
        }


        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x1004b4b0)]
        public static void DeathDomainD20ACheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var condArg1 = evt.GetConditionArg1();
            evt.SetConditionArg1(condArg1 - 1);
            if ((dispIo.action.d20Caf & D20CAF.HIT) != 0)
            {
                if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Immune_Death_Touch))
                {
                    var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(7001);
                    GameSystems.RollHistory.CreateFromFreeText(meslineValue);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x28, evt.objHndCaller, null);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 156);
                }
                else
                {
                    var clericLevel = evt.objHndCaller.GetStat(Stat.level_cleric);
                    var attemptedDamage = Dice.Roll(clericLevel, 6);
                    var currentHp = dispIo.action.d20ATarget.GetStat(Stat.hp_current);
                    var victim = dispIo.action.d20ATarget;
                    string particlesId;
                    if (attemptedDamage <= currentHp)
                    {
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(42, evt.objHndCaller, victim);
                        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 71);
                        particlesId = "fizzle";
                    }
                    else
                    {
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(41, evt.objHndCaller, victim);
                        GameSystems.D20.Combat.KillWithDeathEffect(dispIo.action.d20ATarget, evt.objHndCaller);
                        GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
                        particlesId = "sp-Slay Living";
                    }

                    GameSystems.ParticleSys.CreateAtObj(particlesId, dispIo.action.d20ATarget);
                    dispIo.returnVal = ActionErrorCode.AEC_OK;
                }
            }
            else
            {
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 29);
                dispIo.returnVal = ActionErrorCode.AEC_OK;
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x1004b9c0)]
        public static void Luck_RerollQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var condArg1 = evt.GetConditionArg1();
            if (condArg1 > 0)
            {
                int condArgIdx;
                switch (evt.dispKey)
                {
                    case D20DispatcherKey.QUE_RerollAttack:
                        condArgIdx = 1;
                        break;
                    case D20DispatcherKey.QUE_RerollSavingThrow:
                        condArgIdx = 2;
                        break;
                    case D20DispatcherKey.QUE_RerollCritical:
                        condArgIdx = 3;
                        break;
                    default:
                        return;
                }

                if (evt.GetConditionArg(condArgIdx) != 0)
                {
                    dispIo.return_val = 1;
                    evt.SetConditionArg1(condArg1 - 1);
                    GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
                    GameSystems.ParticleSys.CreateAtObj("sp-Luck Domain Reroll", evt.objHndCaller);
                }
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x1004bcf0)]
        public static void TravelClearParticles(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetConditionArg2() != 0)
            {
                GameSystems.ParticleSys.Remove(evt.GetConditionPartSysArg(2));
                evt.SetConditionPartSysArg(2, null);
            }

            evt.SetConditionArg2(0);
        }


        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x1004ade0)]
        [TemplePlusLocation("condition.cpp:511")]
        public static void TurnUndead_Check(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();

            var action = dispIo.action;
            var turnType = evt.GetConditionArg1();

            if (turnType == action.data1)
            {
                var charges = evt.GetConditionArg2();

                // Check if the turn undead ability has been disabled in python
                var result = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Turn Undead Disabled");
                if (result > 0)
                {
                    dispIo.returnVal = dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                }
                else
                {
                    if (charges > 0)
                    {
                        dispIo.returnVal = 0;
                    }
                    else
                    {
                        dispIo.returnVal = dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                    }
                }
            }
        }

        [DispTypes(DispatcherType.BaseCasterLevelMod)]
        [TempleDllLocation(0x1004b3f0)]
        public static void Alignment_Domain_SpellCasterLevel_Modify(in DispatcherCallbackArgs evt,
            SpellDescriptor data1, int data2)
        {
            var dispIo = evt.GetDispIoD20Query();
            var spellPacket = (SpellPacketBody) dispIo.obj;
            var spellEnum = spellPacket.spellEnum;
            if (spellEnum != 0)
            {
                if ((GameSystems.Spell.GetSpellDescriptor(spellEnum) & data1) != 0)
                {
                    dispIo.return_val += data2;
                }
            }
        }

        [TempleDllLocation(0x102B17E4)]
        private const int turnUndeadRange = 24;

        private enum TurnUndeadType
        {
            TurnUndead = 0,
            RebukeUndead,
            RebukeFireTurnWater = 2,
            RebukeWaterTurnFire = 3,
            RebukeAirTurnEarth = 4,
            RebukeEarthTurnAir = 5,
            RebukePlants = 6,
            GreaterTurning = 7
        }

        [TempleDllLocation(0x102b17a4)]
        private static readonly Dictionary<TurnUndeadType, Predicate<GameObjectBody>> TurnUndeadTargetChecks =
            new Dictionary<TurnUndeadType, Predicate<GameObjectBody>>
            {
                {TurnUndeadType.TurnUndead, obj => GameSystems.Critter.IsUndead(obj)},
                {TurnUndeadType.RebukeFireTurnWater, obj => GameSystems.Critter.IsWaterSubtype(obj)},
                {TurnUndeadType.RebukeWaterTurnFire, obj => GameSystems.Critter.IsFireSubtype(obj)},
                {TurnUndeadType.RebukeAirTurnEarth, obj => GameSystems.Critter.IsEarthSubtype(obj)},
                {TurnUndeadType.RebukeEarthTurnAir, obj => GameSystems.Critter.IsAirSubtype(obj)},
                {TurnUndeadType.GreaterTurning, obj => GameSystems.Critter.IsUndead(obj)},
            };

        [TempleDllLocation(0x102B17C4)]
        private static readonly Dictionary<TurnUndeadType, Predicate<GameObjectBody>> RebukeUndeadTargetChecks =
            new Dictionary<TurnUndeadType, Predicate<GameObjectBody>>
            {
                {TurnUndeadType.RebukeUndead, obj => GameSystems.Critter.IsUndead(obj)},
                {TurnUndeadType.RebukeFireTurnWater, obj => GameSystems.Critter.IsFireSubtype(obj)},
                {TurnUndeadType.RebukeWaterTurnFire, obj => GameSystems.Critter.IsWaterSubtype(obj)},
                {TurnUndeadType.RebukeAirTurnEarth, obj => GameSystems.Critter.IsAirSubtype(obj)},
                {TurnUndeadType.RebukeEarthTurnAir, obj => GameSystems.Critter.IsEarthSubtype(obj)},
                {TurnUndeadType.RebukePlants, obj => GameSystems.Critter.IsPlant(obj)}
            };

        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x1004aeb0)]
        [TemplePlusLocation("condition.cpp:509")]
        public static void TurnUndeadPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var action = dispIo.action;
            var turnUndeadType = (TurnUndeadType) dispIo.action.data1;
            var conditionTurnUndeadType = (TurnUndeadType) evt.GetConditionArg1();

            GameSystems.SoundGame.PositionalSound(9302, 1, action.d20APerformer);
            if (action.d20ATarget == null || !action.d20ATarget.IsCritter()
                                          || GameSystems.D20.D20QueryWithObject(
                                              action.d20ATarget,
                                              D20DispatcherKey.QUE_CanBeAffected_PerformAction,
                                              action,
                                              defaultResult: 1) != 0)
            {
                var turningLvl = evt.objHndCaller.GetStat(Stat.level_cleric);
                var palLvlAdj = evt.objHndCaller.GetStat(Stat.level_paladin) - 2;
                if (palLvlAdj > 0 &&
                    (turnUndeadType == TurnUndeadType.TurnUndead || turnUndeadType == TurnUndeadType.GreaterTurning))
                {
                    turningLvl += palLvlAdj;
                }

                if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.IMPROVED_TURNING))
                {
                    ++turningLvl;
                }

                if (conditionTurnUndeadType == turnUndeadType)
                {
                    GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
                    string partsysName;
                    if (evt.objHndCaller.GetInt32(obj_f.critter_alignment_choice) == 1)
                    {
                        partsysName = "sp-Turn Undead";
                    }
                    else
                    {
                        partsysName = "sp-Rebuke Undead";
                    }

                    GameSystems.ParticleSys.CreateAtObj(partsysName, evt.objHndCaller);
                    var condArg2 = evt.GetConditionArg2();
                    evt.SetConditionArg2(condArg2 - 1);
                    var loc = evt.objHndCaller.GetLocation();
                    var xyRect = new TileRect();
                    xyRect.x1 = loc.locx - turnUndeadRange;
                    xyRect.y1 = loc.locy - turnUndeadRange;
                    xyRect.x2 = loc.locx + turnUndeadRange;
                    xyRect.y2 = loc.locy + turnUndeadRange;

                    var affected = new List<GameObjectBody>();

                    using var objlist = ObjList.ListRect(in xyRect, ObjectListFilter.OLC_CRITTERS);
                    foreach (var critter in objlist)
                    {
                        if (critter != evt.objHndCaller && !GameSystems.Critter.IsDeadNullDestroyed(critter))
                        {
                            if (IsTurnUndeadTarget(turnUndeadType, critter)
                                || RebukeUndeadTargetChecks.TryGetValue(turnUndeadType, out var rebukeTargetCheck) &&
                                rebukeTargetCheck(critter))
                            {
                                affected.Add(critter);
                            }
                        }
                    }

                    // Sort by distance to actor
                    var critterLoc = evt.objHndCaller.GetLocationFull();
                    affected.Sort((a, b) => Comparer<float>.Default.Compare(
                        a.DistanceToLocInFeet(critterLoc, false),
                        b.DistanceToLocInFeet(critterLoc, false)
                    ));

                    var turnModifier = evt.objHndCaller.GetStat(Stat.cha_mod);
                    if (GameSystems.D20.D20Query(evt.objHndCaller,
                        D20DispatcherKey.QUE_Critter_Is_On_Consecrate_Ground))
                    {
                        turnModifier += (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller,
                            D20DispatcherKey.QUE_Critter_Is_On_Consecrate_Ground);
                    }

                    if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_On_Desecrate_Ground))
                    {
                        turnModifier -= (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller,
                            D20DispatcherKey.QUE_Critter_Is_On_Desecrate_Ground);
                    }

                    var turnRoll = turningLvl + (Dice.Roll(1, 20, turnModifier) - 10) / 3;
                    var hitdieTot = Dice.Roll(2, 6, turningLvl + turnModifier);

                    foreach (var target in affected)
                    {
                        var npcHd = target.GetInt32(obj_f.npc_hitdice_idx, 0);
                        if (npcHd <= turnRoll && npcHd <= hitdieTot)
                        {
                            if (IsRebukeUndeadTarget(turnUndeadType, target))
                            {
                                if (turnUndeadType != TurnUndeadType.GreaterTurning)
                                {
                                    if (!GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Commanded))
                                    {
                                        if (2 * npcHd <= turningLvl &&
                                            GameSystems.Critter.AddFollower(target, evt.objHndCaller, true, true))
                                        {
                                            hitdieTot -= npcHd;
                                            target.AddCondition(Commanded);
                                            GameSystems.RollHistory
                                                .CreateRollHistoryLineFromMesfile(0x34, target, null);
                                        }
                                        else if (!GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Rebuked))
                                        {
                                            hitdieTot -= npcHd;
                                            target.AddCondition(Cowering);
                                            GameSystems.RollHistory
                                                .CreateRollHistoryLineFromMesfile(0x36, target, null);
                                        }
                                    }

                                    continue;
                                }

                                hitdieTot -= npcHd;
                            }
                            else if (turnUndeadType == TurnUndeadType.GreaterTurning)
                            {
                                hitdieTot -= npcHd;
                            }
                            else
                            {
                                if (2 * npcHd > turningLvl)
                                {
                                    if (!GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Turned))
                                    {
                                        hitdieTot -= npcHd;
                                        target.AddCondition(Turned, 10, evt.objHndCaller);
                                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(53, target, null);
                                    }

                                    continue;
                                }

                                hitdieTot -= npcHd;
                            }

                            GameSystems.D20.Combat.KillWithDestroyEffect(target, evt.objHndCaller);
                            GameSystems.ParticleSys.CreateAtObj("sp-Destroy Undead", target);
                        }
                    }
                }
            }

            // Send the signal if this was the turn type used
            if (turnUndeadType == conditionTurnUndeadType)
            {
                GameSystems.D20.D20SignalPython(evt.objHndCaller, "Turn Undead Perform", (int) turnUndeadType);
            }
        }

        private static bool IsTurnUndeadTarget(TurnUndeadType turnUndeadType, GameObjectBody critter)
        {
            return TurnUndeadTargetChecks.TryGetValue(turnUndeadType, out var check) && check(critter);
        }

        private static bool IsRebukeUndeadTarget(TurnUndeadType turnUndeadType, GameObjectBody critter)
        {
            return RebukeUndeadTargetChecks.TryGetValue(turnUndeadType, out var check) && check(critter);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x1004bde0)]
        public static void AnimalDomainRadial(in DispatcherCallbackArgs evt)
        {
            if (evt.GetConditionArg1() == 0)
            {
                var spellData = new D20SpellData(WellKnownSpells.CharmPersonOrAnimal, 23, 1);
                var entry = RadialMenuEntry.CreateSpellAction(spellData, D20ActionType.ACTIVATE_DEVICE_SPELL);
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry, RadialMenuStandardNode.Class);
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100ed330)]
        public static void CoweringArmorMalus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data1, 0, data2);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x1004ac90)]
        public static void TurnedUndeadIsAfraid(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoD20Query();
            dispIo.data2 = condArg2;
            dispIo.return_val = 1;
            dispIo.data1 = condArg3;
        }


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x1004be80)]
        public static void AnimalDomainPerformAction(in DispatcherCallbackArgs evt)
        {
            if (evt.GetDispIoD20ActionTurnBased().action.data1 == 23)
            {
                evt.SetConditionArg1(1);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x1004b620)]
        public static void DeathTouchRadial(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = RadialMenuEntry.CreateAction(5020, D20ActionType.DEATH_TOUCH, 0, "TAG_DEATH_D");
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Class);
        }


        [DispTypes(DispatcherType.ConditionRemove2)]
        [TempleDllLocation(0x1004beb0)]
        public static void CoweringUndeadRemoved(in DispatcherCallbackArgs evt)
        {
            GameSystems.AI.StopFleeing(evt.objHndCaller);
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x1004b780)]
        public static void DestructionDomainAddDamage(in DispatcherCallbackArgs evt)
        {
            var clericLevel = evt.objHndCaller.GetStat(Stat.level_cleric);
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddDamageBonus(clericLevel, 0, 181);
            var victim = dispIo.attackPacket.victim;
            GameSystems.ParticleSys.CreateAtObj("sp-Smite-Hit", victim);
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x1004b750)]
        public static void DestructionDomainAddToHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(4, 0, 181);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x1004bb80)]
        public static void FeatOfStrengthRadial(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = RadialMenuEntry.CreateAction(5023, D20ActionType.FEAT_OF_STRENGTH, 0, "TAG_STRENGTH_D");
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Class);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x1004ba70)]
        public static void ProtectiveWardRadial(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = RadialMenuEntry.CreateAction(5022, D20ActionType.PROTECTIVE_WARD, 0, "TAG_PROTECTION_D");
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Class);
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x1004bc50)]
        public static void FeatOfStrengthStatBonus(in DispatcherCallbackArgs evt)
        {
            var clericLevel = evt.objHndCaller.GetStat(Stat.level_cleric);
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonus(clericLevel, 0, 183);
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x1004ace0)]
        public static void TurnUndeadInitNumPerDay(in DispatcherCallbackArgs evt)
        {
            var chaMod = evt.objHndCaller.GetStat(Stat.cha_mod);
            var extraTurningFeatTaken = GameSystems.Feat.HasFeatCount(evt.objHndCaller, FeatId.EXTRA_TURNING);
            var extraTurningAttempts = 0;
            if (extraTurningFeatTaken > 0)
            {
                extraTurningAttempts = 4 * extraTurningFeatTaken;
            }

            evt.SetConditionArg2(extraTurningAttempts + chaMod + 3);
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x1004bc90)]
        public static void TravelDomainResetUses(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg2()) != 0)
            {
                var condArg3 = evt.GetConditionArg3();
                GameSystems.ParticleSys.Remove(condArg3);
            }

            var clericLevel = evt.objHndCaller.GetStat(Stat.level_cleric);
            evt.SetConditionArg1(clericLevel);
            evt.SetConditionArg2(0);
        }


        [DispTypes(DispatcherType.DestructionDomain)]
        [TempleDllLocation(0x1004b700)]
        public static void DestructionDomainAddSmite(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (condArg1 > 0)
            {
                evt.SetConditionArg1(condArg1 - 1);
                evt.objHndCaller.AddCondition(DestructionDomainSmite);
                GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x1004ad40)]
        public static void TurnUndeadRadialMenuEntry(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                var type = (TurnUndeadType) evt.GetConditionArg1();
                var radMenuEntry = RadialMenuEntry.CreateAction(5028 + (int) type, D20ActionType.TURN_UNDEAD,
                    (int) type, "TAG_TURN");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Class);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x1004b7e0)]
        public static void GoodFortune_RadialMenuEntry_Callback(in DispatcherCallbackArgs evt)
        {
            var parentEntry = RadialMenuEntry.CreateParent(5024);
            var containerNode =
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                    RadialMenuStandardNode.Class);

            var rerollAttack = evt.CreateToggleForArg(1);
            rerollAttack.text = GameSystems.D20.Combat.GetCombatMesLine(5025);
            rerollAttack.helpSystemHashkey = "TAG_LUCK_D";
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref rerollAttack, containerNode);

            var rerollSave = evt.CreateToggleForArg(2);
            rerollSave.helpSystemHashkey = "TAG_LUCK_D";
            rerollSave.text = GameSystems.D20.Combat.GetCombatMesLine(5026);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref rerollSave, containerNode);

            var rerollCritical = evt.CreateToggleForArg(3);
            rerollCritical.text = GameSystems.D20.Combat.GetCombatMesLine(5027);
            rerollCritical.helpSystemHashkey = "TAG_LUCK_D";
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref rerollCritical, containerNode);
        }
    }
}