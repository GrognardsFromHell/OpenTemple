using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Script.Hooks;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.InGameSelect;

namespace OpenTemple.Core.Systems.AI
{
    public static class DefaultTactics
    {
        private static readonly Dictionary<string, AiTacticDef> Tactics = new Dictionary<string, AiTacticDef>
        {
            {"default", TacticDefault.Instance},
            {"defaultCast", TacticDefaultCast.Instance},
            {"sniper", TacticSniper.Instance},
            {"flank", TacticFlank.Instance},
            {"ready vs spell", TacticReadyVsSpell.Instance},
            {"partial charge", TacticPartialCharge.Instance},
            {"ready vs approach", TacticReadyVsApproach.Instance},
            {"target low ac", TacticTargetLowAc.Instance},
            {"target high ac", TacticTargetHighAc.Instance},
            {"target damaged", TacticTargetDamaged.Instance},
            {"target ranged", TacticTargetRanged.Instance},
            {"target closest", TacticTargetClosest.Instance},
            {"target prone", TacticTargetProne.Instance},
            {"rage", TacticRage.Instance},
            {"power attack", TacticPowerAttack.Instance},
            {"expertise", TacticExpertise.Instance},
            {"trip", TacticTrip.Instance},
            {"go ranged", TacticGoRanged.Instance},
            {"reload", TacticReload.Instance},
            {"target self", TacticTargetSelf.Instance},
            {"target friend high ac", TacticTargetFriendHighAc.Instance},
            {"target friend low ac", TacticTargetFriendLowAc.Instance},
            {"target friend hurt", TacticTargetFriendHurt.Instance},
            {"target friend nospell", TacticTargetFriendNospell.Instance},
            {"cast single", TacticCastSingle.Instance},
            {"cast fireball", TacticCastFireball.Instance},
            {"cast area", TacticCastArea.Instance},
            {"cast party", TacticCastParty.Instance},
            {"approach", TacticApproach.Instance},
            {"clear target", TacticClearTarget.Instance},
            {"attack threatened", TacticAttackThreatened.Instance},
            {"attack", TacticAttack.Instance},
            {"target threatened", TacticTargetThreatened.Instance},
            {"target nospell", TacticTargetNospell.Instance},
            {"five foot step", TacticFiveFootStep.Instance},
            {"coup de grace", TacticCoupDeGrace.Instance},
            {"use potion", TacticUsePotion.Instance},
            {"charge", TacticCharge.Instance},
            {"go melee", TacticGoMelee.Instance},
            {"target nearby buffed", TacticTargetNearbyBuffed.Instance},
            {"target bad will", TacticTargetBadWill.Instance},
            {"target bad fort", TacticTargetBadFort.Instance},
            {"target bad reflex", TacticTargetBadReflex.Instance},
            {"cast single alone", TacticCastSingleAlone.Instance}
        };

        public static AiTacticDef GetByName(string name) => Tactics[name];

        public static bool TryGetByName(string name, out AiTacticDef tactic) => Tactics.TryGetValue(name, out tactic);
    }

    internal class TacticDefault : AiTacticDef
    {
        public static readonly TacticDefault Instance = new TacticDefault();

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public string name => "default";

        [TempleDllLocation(0x100e3270)]
        public bool aiFunc(AiTactic aiTac)
        {
            if (GameSystems.D20.Actions.curSeqGetTurnBasedStatus().hourglassState >= HourglassState.STD)
            {
                if (aiTac.target != null)
                {
                    GameSystems.D20.Actions.GlobD20ActnInit();
                    GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.UNSPECIFIED_ATTACK, 0);
                    GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
                    GameSystems.D20.Actions.ActionAddToSeq();
                    var error = GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation();
                    if (error != ActionErrorCode.AEC_OK)
                    {
                        if (!aiTac.performer.HasRangedWeaponEquipped())
                        {
                            Logger.Info("AI Action Perform: Reseting sequence");
                            GameSystems.D20.Actions.CurSeqReset(aiTac.performer);
                            GameSystems.D20.Actions.GlobD20ActnInit();
                            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(0, 0);
                            GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
                            GameSystems.D20.Actions.ActionAddToSeq();
                            error = GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation();
                        }
                    }

                    return error == ActionErrorCode.AEC_OK;
                }

                return false;
            }

            return false;
        }
    }

    internal class TacticDefaultCast : AiTacticDef
    {
        public static readonly TacticDefaultCast Instance = new TacticDefaultCast();

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public string name => "defaultCast";

        [TempleDllLocation(0x100e4310)]
        public bool aiFunc(AiTactic aiTac)
        {
            var v1 = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;
            if (aiTac.target != null)
            {
                GameSystems.AI.AiFiveFootStepAttempt(aiTac);
                GameSystems.D20.Actions.GlobD20ActnInit();
                GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CAST_SPELL, 0);
                GameSystems.D20.Actions.ActSeqCurSetSpellPacket(aiTac.spellPktBody, false);
                GameSystems.D20.Actions.GlobD20ActnSetSpellData(aiTac.d20SpellData);
                if (aiTac.target != null)
                {
                    var locAndOffOut = aiTac.target.GetLocationFull();
                    GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, locAndOffOut);
                }

                GameSystems.D20.Actions.ActionAddToSeq();
                var error = GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation();
                if (error != ActionErrorCode.AEC_OK)
                {
                    var errorStr = GameSystems.D20.Actions.ActionErrorString(error);
                    var spellName = GameSystems.Spell.GetSpellName(aiTac.spellPktBody.spellEnum);
                    Logger.Info(
                        "d20_strategy.c / _DefaultCast_Perform(): caster=( {0} ) cannot cast spell=( {1} ) because reason=( {2} )",
                        aiTac.performer,
                        spellName,
                        errorStr);
                    GameSystems.D20.Actions.ActionSequenceRevertPath(v1);
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    internal class TacticSniper : AiTacticDef
    {
        public static readonly TacticSniper Instance = new TacticSniper();

        public string name => "sniper";

        [TempleDllLocation(0x100e58d0)]
        public bool aiFunc(AiTactic aiTac)
        {
            if (!aiTac.performer.HasRangedWeaponEquipped())
            {
                return false;
            }

            if (!GameSystems.Item.AmmoMatchesItemAtSlot(aiTac.performer, EquipSlot.WeaponPrimary))
            {
                TacticGoMelee.Instance.aiFunc(aiTac);
                return false;
            }

            if (!GameSystems.AI.AiFiveFootStepAttempt(aiTac))
            {
                TacticGoMelee.Instance.aiFunc(aiTac);
            }

            return TacticDefault.Instance.aiFunc(aiTac);
        }
    }

    internal class TacticFlank : AiTacticDef
    {
        public static readonly TacticFlank Instance = new TacticFlank();

        public string name => "flank";

        [TempleDllLocation(0x100e5950)]
        public bool aiFunc(AiTactic aiTac)
        {
            // TODO: Port TemplePlus replacement

            var d20ActNum = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;
            if (aiTac.target == null)
            {
                return false;
            }

            if (aiTac.performer.HasRangedWeaponEquipped())
            {
                TacticGoMelee.Instance.aiFunc(aiTac);
            }

            if (aiTac.target == null)
            {
                return false;
            }

            if (GameSystems.D20.D20QueryWithObject(aiTac.target, D20DispatcherKey.QUE_CanBeFlanked, aiTac.performer) ==
                0)
            {
                return false;
            }

            if (GameSystems.D20.Combat.IsFlankedBy(aiTac.target, aiTac.performer))
            {
                return false;
            }

            if (GameSystems.D20.Actions.isSimultPerformer(aiTac.performer))
            {
                if (GameSystems.D20.Actions.simulsAbort(aiTac.performer))
                {
                    GameSystems.D20.Actions.CurSeqReset(aiTac.performer);
                    return true;
                }
            }

            var enemies = GameSystems.Combat.GetEnemiesCanMelee(aiTac.target);

            var targetPos = aiTac.target.GetLocationFull().ToInches2D();

            var distanceFromTarget = aiTac.performer.GetRadius() + aiTac.target.GetRadius() + 8.0f;

            foreach (var enemy in enemies)
            {
                if (enemy == aiTac.performer)
                {
                    continue;
                }

                var enemyPos = enemy.GetLocationFull().ToInches2D();
                var chargeDir = Vector2.Normalize(enemyPos - targetPos);
                var locXy = LocAndOffsets.FromInches(targetPos - chargeDir * distanceFromTarget);

                GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(0, 0);
                GameSystems.D20.Actions.GlobD20ActnSetTarget(null, locXy);
                if (GameSystems.D20.Actions.ActionAddToSeq() == ActionErrorCode.AEC_OK)
                {
                    if (GameSystems.D20.Actions.GetPathTargetLocFromCurD20Action(out var locOut))
                    {
                        if (GameSystems.D20.Combat.TargetWithinReachOfLoc(aiTac.performer, aiTac.target, locOut))
                        {
                            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() ==
                                ActionErrorCode.AEC_OK)
                            {
                                return true;
                            }
                        }
                    }
                }

                GameSystems.D20.Actions.ActionSequenceRevertPath(d20ActNum);
            }

            return false;
        }
    }

    internal class TacticReadyVsSpell : AiTacticDef
    {
        public static readonly TacticReadyVsSpell Instance = new TacticReadyVsSpell();

        public string name => "ready vs spell";

        [TempleDllLocation(0x100e3320)]
        public bool aiFunc(AiTactic aiTac)
        {
            var strategyState = aiTac.performer.GetInt32(obj_f.strategy_state, aiTac.tacticIdx);
            if (strategyState == 2)
            {
                aiTac.performer.SetInt32(obj_f.strategy_state, aiTac.tacticIdx, 0);
                return false;
            }

            if (strategyState == 1)
            {
                var interruptee = GameSystems.D20.Actions.GetInterruptee();
                if (interruptee != null && aiTac.performer != interruptee)
                {
                    aiTac.performer.SetInt32(obj_f.strategy_state, aiTac.tacticIdx, 0);
                    aiTac.target = interruptee;
                    return false;
                }

                aiTac.performer.SetInt32(obj_f.strategy_state, aiTac.tacticIdx, 2);
                return false;
            }

            if (GameSystems.D20.Actions.isSimultPerformer(aiTac.performer) &&
                GameSystems.D20.Actions.simulsAbort(aiTac.performer))
            {
                GameSystems.D20.Actions.CurSeqReset(aiTac.performer);
                return true;
            }

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.READY_COUNTERSPELL, 0);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                return false;
            }

            aiTac.performer.SetInt32(obj_f.strategy_state, aiTac.tacticIdx, 1);
            return true;
        }
    }

    internal class TacticPartialCharge : AiTacticDef
    {
        public static readonly TacticPartialCharge Instance = new TacticPartialCharge();

        public string name => "partial charge";

        [TempleDllLocation(0x100e3450)]
        public bool aiFunc(AiTactic aiTac)
        {
            if (aiTac.performer.HasRangedWeaponEquipped()
                || GameSystems.D20.Actions.curSeqGetTurnBasedStatus().hourglassState != HourglassState.PARTIAL
                || aiTac.target == null)
            {
                return false;
            }

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CHARGE, 0);
            GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
            GameSystems.D20.Actions.ActionAddToSeq();
            return GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() == ActionErrorCode.AEC_OK;
        }
    }

    internal class TacticReadyVsApproach : AiTacticDef
    {
        public static readonly TacticReadyVsApproach Instance = new TacticReadyVsApproach();

        public string name => "ready vs approach";

        [TempleDllLocation(0x100e34d0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var strategyState = aiTac.performer.GetInt32(obj_f.strategy_state, aiTac.tacticIdx);
            if (strategyState == 2)
            {
                aiTac.performer.SetInt32(obj_f.strategy_state, aiTac.tacticIdx, 0);
                return false;
            }

            var enemies = GameSystems.D20.Actions.GetEnemiesWithinReach(aiTac.performer);
            if (enemies.Count > 0)
            {
                aiTac.target = enemies[0];
                return false;
            }

            if (strategyState == 1)
            {
                var interruptee = GameSystems.D20.Actions.GetInterruptee();

                if (interruptee != null)
                {
                    if (interruptee != aiTac.performer)
                    {
                        aiTac.performer.SetInt32(obj_f.strategy_state, aiTac.tacticIdx, 0);
                        aiTac.target = interruptee;
                        return false;
                    }
                }
            }

            if (GameSystems.D20.Actions.isSimultPerformer(aiTac.performer) &&
                GameSystems.D20.Actions.simulsAbort(aiTac.performer))
            {
                GameSystems.D20.Actions.CurSeqReset(aiTac.performer);
                return true;
            }

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.READY_ENTER, 0);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                return false;
            }

            aiTac.performer.SetInt32(obj_f.strategy_state, aiTac.tacticIdx, 1);
            return true;
        }
    }

    internal class TacticTargetLowAc : AiTacticDef
    {
        public static readonly TacticTargetLowAc Instance = new TacticTargetLowAc();

        public string name => "target low ac";

        [TempleDllLocation(0x100e3630)]
        public bool aiFunc(AiTactic a1)
        {
            var currentLowestAc = 10000;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != a1.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        !GameSystems.Critter.IsFriendly(a1.performer, combatant))
                    {
                        var ac = GameSystems.Stat.GetAC(combatant, DispIoAttackBonus.Default);
                        if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible)                             && !GameSystems.D20.D20Query(a1.performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible) )
                        {
                            // Makes it less likely to be picked
                            ac += 5;
                        }

                        if (ac < currentLowestAc)
                        {
                            currentLowestAc = ac;
                            a1.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticTargetHighAc : AiTacticDef
    {
        public static readonly TacticTargetHighAc Instance = new TacticTargetHighAc();

        public string name => "target high ac";

        [TempleDllLocation(0x100e3700)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentHighestAc = 0;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != aiTac.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        !GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                    {
                        var ac = GameSystems.Stat.GetAC(combatant, DispIoAttackBonus.Default);
                        if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible)                             && !GameSystems.D20.D20Query(aiTac.performer,
                                D20DispatcherKey.QUE_Critter_Can_See_Invisible) )
                        {
                            // Makes it less likely to be picked
                            ac -= 5;
                        }

                        if (ac > currentHighestAc)
                        {
                            currentHighestAc = ac;
                            aiTac.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticTargetDamaged : AiTacticDef
    {
        public static readonly TacticTargetDamaged Instance = new TacticTargetDamaged();

        public string name => "target damaged";

        [TempleDllLocation(0x100e37d0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var highest = 1.1f;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != aiTac.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        !GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                    {
                        var hpCur = (float) combatant.GetStat(Stat.hp_current);
                        var hpPct = hpCur / combatant.GetStat(Stat.hp_max);

                        if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                            !GameSystems.D20.D20Query(aiTac.performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                        {
                            // Makes it less likely to be chosen
                            hpPct += 5.0f;
                        }

                        if (hpPct < highest)
                        {
                            highest = hpPct;
                            aiTac.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticTargetRanged : AiTacticDef
    {
        public static readonly TacticTargetRanged Instance = new TacticTargetRanged();

        public string name => "target ranged";

        [TempleDllLocation(0x100e38e0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentClosest = 10000.0f;

            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != aiTac.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        !GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                    {
                        if (!combatant.HasRangedWeaponEquipped())
                        {
                            continue;
                        }

                        var dist = aiTac.performer.DistanceToObjInFeet(combatant);

                        if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                            !GameSystems.D20.D20Query(aiTac.performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                        {
                            // Makes it less likely to be chosen
                            dist = (dist + 5.0f) * 2.5f;
                        }

                        if (dist < currentClosest)
                        {
                            currentClosest = dist;
                            aiTac.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticTargetClosest : AiTacticDef
    {
        public static readonly TacticTargetClosest Instance = new TacticTargetClosest();

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public string name => "target closest";

        [TempleDllLocation(0x100e3a00)]
        public bool aiFunc(AiTactic aiTac)
        {
            GameObject performer = aiTac.performer;
            var dist = 1000000000.0f;
            var reach = performer.GetReach();
            bool hasGoodTarget = false;

            Logger.Debug("{0} targeting closest...", performer);

            foreach (var combatant in GameSystems.D20.Initiative)
            {
                var ignoreTarget = performer.ShouldIgnoreTarget(combatant);

                if (!GameSystems.Critter.IsFriendly(combatant, performer)
                    && !GameSystems.Critter.IsDeadOrUnconscious(combatant)
                    && !ignoreTarget)
                {
                    var distToCombatant = performer.DistanceToObjInFeet(combatant);
                    //Logger.Debug("Checking line of attack for target: {0}", GameSystems.MapObject.GetDisplayName(combatant));
                    bool hasLineOfAttack = GameSystems.Combat.HasLineOfAttack(performer, combatant);
                    if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible)                         && !GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible) )
                    {
                        // makes invisibile chars less likely to be attacked; also takes into accout stuff like Hide From Animals (albeit in a shitty manner)
                        distToCombatant = (distToCombatant + 5.0f) * 2.5f;
                    }

                    bool isGoodTarget = distToCombatant <= reach && hasLineOfAttack;

                    if (isGoodTarget)
                    {
                        if (distToCombatant < dist) // best
                        {
                            aiTac.target = combatant;
                            dist = distToCombatant;
                            hasGoodTarget = true;
                        }
                        else if (!hasGoodTarget
                        ) // is a good target within reach, not necessarily the closest so far, but other good targets haven't been found yet
                        {
                            aiTac.target = combatant;
                            dist = distToCombatant;
                            hasGoodTarget = true;
                        }
                    }
                    else if (distToCombatant < dist && !hasGoodTarget)
                    {
                        aiTac.target = combatant;
                        dist = distToCombatant;
                    }
                }
            }

            Logger.Info("{0} targeted.", aiTac.target);
            return false;
        }
    }

    internal class TacticTargetProne : AiTacticDef
    {
        public static readonly TacticTargetProne Instance = new TacticTargetProne();

        public string name => "target prone";

        [TempleDllLocation(0x100e3a60)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentClosest = 10000.0f;

            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != aiTac.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        !GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                    {
                        if (!GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Prone) )
                        {
                            continue;
                        }

                        var dist = aiTac.performer.DistanceToObjInFeet(combatant);

                        if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                            !GameSystems.D20.D20Query(aiTac.performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                        {
                            // Makes it less likely to be chosen
                            dist = (dist + 5.0f) * 2.5f;
                        }

                        if (dist < currentClosest)
                        {
                            currentClosest = dist;
                            aiTac.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticRage : AiTacticDef
    {
        public static readonly TacticRage Instance = new TacticRage();

        public string name => "rage";

        [TempleDllLocation(0x100e3b60)]
        public bool aiFunc(AiTactic aiTac)
        {
            var initialActNum =
                GameSystems.D20.Actions.CurrentSequence.d20ActArrayNum; // used for resetting in case of failure
            if (!GameSystems.Critter.CanBarbarianRage(aiTac.performer))
                return false;

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.BARBARIAN_RAGE, 1);
            var addToSeqError = GameSystems.D20.Actions.ActionAddToSeq();
            if (addToSeqError != ActionErrorCode.AEC_OK)
            {
                var locCheckError = GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation();
                if (locCheckError != ActionErrorCode.AEC_OK)
                {
                    GameSystems.D20.Actions.ActionSequenceRevertPath(initialActNum);
                    return false;
                }
            }

            return true;
        }
    }

    internal class TacticPowerAttack : AiTacticDef
    {
        public static readonly TacticPowerAttack Instance = new TacticPowerAttack();

        public string name => "power attack";

        [TempleDllLocation(0x100e3bd0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var baseAttackBonus = aiTac.performer.DispatchToHitBonusBase();
            var toHitBonus = GameSystems.Stat.Dispatch16GetToHitBonus(aiTac.performer, DispIoAttackBonus.Default);
            var againstAc = 15;
            var weapon = GameSystems.Item.ItemWornAt(aiTac.performer, EquipSlot.WeaponPrimary);
            if (weapon != null && GameSystems.Item.GetWieldType(aiTac.performer, weapon) != 0)
            {
                if (aiTac.target != null)
                {
                    againstAc = GameSystems.Stat.GetAC(aiTac.target, DispIoAttackBonus.Default);
                }

                var convertedBonus = toHitBonus - againstAc + 12;
                if (convertedBonus < 0)
                {
                    convertedBonus = 0;
                }

                if (convertedBonus > baseAttackBonus)
                {
                    convertedBonus = baseAttackBonus;
                }

                GameSystems.D20.D20SendSignal(aiTac.performer, D20DispatcherKey.SIG_SetPowerAttack, convertedBonus);
            }

            return false;
        }
    }

    internal class TacticExpertise : AiTacticDef
    {
        public static readonly TacticExpertise Instance = new TacticExpertise();

        public string name => "expertise";

        [TempleDllLocation(0x100e3c80)]
        public bool aiFunc(AiTactic a1)
        {
            var baseAttackBonus = a1.performer.DispatchToHitBonusBase();
            if (baseAttackBonus > 5)
            {
                baseAttackBonus = 5;
            }

            GameSystems.D20.D20SendSignal(a1.performer, D20DispatcherKey.SIG_SetExpertise, baseAttackBonus);
            return false;
        }
    }

    internal class TacticTrip : AiTacticDef
    {
        public static readonly TacticTrip Instance = new TacticTrip();

        public string name => "trip";

        [TempleDllLocation(0x100e3cc0)]
        public bool aiFunc(AiTactic aiTactic)
        {
            var currentActionNum = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;
            if (!aiTactic.performer.HasRangedWeaponEquipped())
            {
                if (aiTactic.target != null &&
!GameSystems.D20.D20Query(aiTactic.target, D20DispatcherKey.QUE_Prone))
                {
                    if (GameSystems.D20.Actions.isSimultPerformer(aiTactic.performer))
                    {
                        if (GameSystems.D20.Actions.simulsAbort(aiTactic.performer))
                        {
                            GameSystems.D20.Actions.CurSeqReset(aiTactic.performer);
                            return true;
                        }
                    }

                    GameSystems.D20.Actions.GlobD20ActnInit();
                    GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.TRIP, 0);
                    GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTactic.target, null);
                    GameSystems.D20.Actions.ActionAddToSeq();
                    if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() == ActionErrorCode.AEC_OK)
                    {
                        return true;
                    }

                    GameSystems.D20.Actions.ActionSequenceRevertPath(currentActionNum);
                }
            }

            return false;
        }
    }

    internal class TacticGoRanged : AiTacticDef
    {
        public static readonly TacticGoRanged Instance = new TacticGoRanged();

        public string name => "go ranged";

        [TempleDllLocation(0x100e3da0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
            if (tbStatus.hourglassState < HourglassState.MOVE)
            {
                return false;
            }

            // Search the inventory for a ranged weapon
            foreach (var item in aiTac.performer.EnumerateChildren())
            {
                if (GameSystems.Item.IsRangedWeapon(item))
                {
                    GameSystems.Item.UnequipItemInSlot(aiTac.performer, EquipSlot.WeaponPrimary);
                    GameSystems.Item.UnequipItemInSlot(aiTac.performer, EquipSlot.WeaponSecondary);
                    GameSystems.Item.ItemPlaceInIndex(item, 203);
                    // Deduct cost for changing weapon
                    tbStatus.hourglassState =
                        GameSystems.D20.Actions.GetHourglassTransition(tbStatus.hourglassState, ActionCostType.Move);
                    return true;
                }
            }

            return false;
        }
    }

    internal class TacticReload : AiTacticDef
    {
        public static readonly TacticReload Instance = new TacticReload();

        public string name => "reload";

        [TempleDllLocation(0x100e3ea0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentActNum = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;
            if (!GameSystems.Item.IsWieldingUnloadedCrossbow(aiTac.performer))
            {
                return false;
            }

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.RELOAD, 0);
            GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                GameSystems.D20.Actions.ActionSequenceRevertPath(currentActNum);
                return false;
            }

            return true;
        }
    }

    internal class TacticTargetSelf : AiTacticDef
    {
        public static readonly TacticTargetSelf Instance = new TacticTargetSelf();

        public string name => "target self";

        [TempleDllLocation(0x100e3f10)]
        public bool aiFunc(AiTactic aiTac)
        {
            aiTac.target = aiTac.performer;
            return false;
        }
    }

    internal class TacticTargetFriendHighAc : AiTacticDef
    {
        public static readonly TacticTargetFriendHighAc Instance = new TacticTargetFriendHighAc();

        public string name => "target friend high ac";

        [TempleDllLocation(0x100e3f30)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentHighestAc = 0;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != aiTac.performer)
                {
                    // TODO: Vanilla actually checked for NOT isFriendly... BUG?
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                    {
                        var ac = GameSystems.Stat.GetAC(combatant, DispIoAttackBonus.Default);
                        if (ac > currentHighestAc)
                        {
                            currentHighestAc = ac;
                            aiTac.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticTargetFriendLowAc : AiTacticDef
    {
        public static readonly TacticTargetFriendLowAc Instance = new TacticTargetFriendLowAc();

        public string name => "target friend low ac";

        [TempleDllLocation(0x100e3fd0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentLowestAc = 10000;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != aiTac.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                    {
                        var ac = GameSystems.Stat.GetAC(combatant, DispIoAttackBonus.Default);
                        if (ac < currentLowestAc)
                        {
                            currentLowestAc = ac;
                            aiTac.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticTargetFriendHurt : AiTacticDef
    {
        public static readonly TacticTargetFriendHurt Instance = new TacticTargetFriendHurt();

        public string name => "target friend hurt";

        [TempleDllLocation(0x100e4070)]
        public bool aiFunc(AiTactic aiTac)
        {
            var performer = aiTac.performer;
            var lowest = 70;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (!GameSystems.Critter.IsFriendly(performer, combatant))
                    continue;

                var hpPct = GameSystems.Critter.GetHpPercent(combatant);
                if (hpPct < lowest)
                {
                    lowest = hpPct;
                    aiTac.target = combatant;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Targets the closest combattant who is:
    /// - Not the NPC themselves
    /// - Friendly with the NPC
    /// - Not dead or unconscious
    /// - Is currently not under the influence of the spell defined in the tactic
    /// </summary>
    internal class TacticTargetFriendNospell : AiTacticDef
    {
        public static readonly TacticTargetFriendNospell Instance = new TacticTargetFriendNospell();

        public string name => "target friend nospell";

        [TempleDllLocation(0x100e4120)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentClosest = 10000.0f;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != aiTac.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant) &&
                        GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                    {
                        if (GameSystems.D20.D20Query(
                                combatant,
                                D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                                aiTac.spellPktBody.spellEnum) )
                        {
                            continue;
                        }

                        var dist = aiTac.performer.DistanceToObjInFeet(combatant);
                        if (dist < currentClosest)
                        {
                            currentClosest = dist;
                            aiTac.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticCastSingle : AiTacticDef
    {
        public static readonly TacticCastSingle Instance = new TacticCastSingle();

        public string name => "cast single";

        [TempleDllLocation(0x100e41e0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var actNumOrg = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;
            if (aiTac.target == null)
                return false;
            if (GameSystems.D20.D20Query(aiTac.target,
                    D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    aiTac.spellPktBody.spellEnum) )
            {
                return false;
            }

            var performer = aiTac.performer;
            var enemies = GameSystems.Combat.GetEnemiesCanMelee(performer);
            var castDefensively = enemies.Count > 0 ? 1 : 0;

            GameSystems.D20.D20SendSignal(performer, D20DispatcherKey.SIG_SetCastDefensively, castDefensively);

            GameSystems.Spell.GetSpellTargets(performer, aiTac.target, aiTac.spellPktBody,
                aiTac.spellPktBody.spellEnum);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CAST_SPELL, 0);
            GameSystems.D20.Actions.ActSeqCurSetSpellPacket(aiTac.spellPktBody, false);
            GameSystems.D20.Actions.GlobD20ActnSetSpellData(aiTac.d20SpellData);
            if (aiTac.target != null)
            {
                var locAndOff = aiTac.target.GetLocationFull();
                GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, locAndOff);
            }

            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                GameSystems.D20.Actions.ActionSequenceRevertPath(actNumOrg);
                return false;
            }

            return true;
        }
    }

    internal class TacticCastFireball : AiTacticDef
    {
        public static readonly TacticCastFireball Instance = new TacticCastFireball();

        public string name => "cast fireball";

        [TempleDllLocation(0x100e5c50)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentActionNum = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;

            var enemies = GameSystems.Combat.GetEnemiesCanMelee(aiTac.performer);
            var castDefensively = enemies.Count > 0 ? 1 : 0;

            GameSystems.D20.D20SendSignal(aiTac.performer, D20DispatcherKey.SIG_SetCastDefensively, castDefensively);

            if (TryCastFireball(aiTac))
            {
                return true;
            }

            GameSystems.D20.Actions.ActionSequenceRevertPath(currentActionNum);
            return false;
        }

        private bool TryCastFireball(AiTactic aiTac)
        {
            if (!TryFindFireballTarget(aiTac, out var loc))
            {
                return false;
            }

            if (!GameSystems.Spell.TryGetSpellEntry(aiTac.d20SpellData.spellEnumOrg, out var spEntry))
            {
                return false;
            }

            var pickArgs = new PickerArgs();
            GameSystems.Spell.PickerArgsFromSpellEntry(spEntry, pickArgs, aiTac.performer,
                aiTac.spellPktBody.casterLevel);
            pickArgs.SetAreaTargets(loc);
            GameSystems.Spell.ConfigSpellTargetting(pickArgs, aiTac.spellPktBody);

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CAST_SPELL, 0);
            GameSystems.D20.Actions.ActSeqCurSetSpellPacket(aiTac.spellPktBody, false);
            GameSystems.D20.Actions.GlobD20ActnSetSpellData(aiTac.d20SpellData);
            GameSystems.D20.Actions.GlobD20ActnSetTarget(null, loc);
            GameSystems.D20.Actions.ActionAddToSeq();
            return GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() == ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100e56c0)]
        private bool TryFindFireballTarget(AiTactic aiTac, out LocAndOffsets loc)
        {
            var fireballScoreBest = 0.0f;
            var fireballTargetBest = Vector2.Zero;
            Vector2 fireballTarget;

            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                {
                    continue;
                }

                var combatantPos = combatant.GetLocationFull().ToInches2D();

                if (AdjustCastLocationToClosestEnemy(combatantPos, aiTac.performer, out fireballTarget))
                {
                    var score = CalculateFireballScore(
                        aiTac.performer,
                        fireballTarget,
                        aiTac.spellPktBody,
                        aiTac.spellPktBody.spellEnum);
                    if (score > fireballScoreBest)
                    {
                        fireballScoreBest = score;
                        fireballTargetBest = fireballTarget;
                    }
                }
            }

            // This is a cross-check against just casting at the averaged enemy location...
            if (GetAverageEnemyLocation(aiTac.performer, out fireballTarget))
            {
                var score = CalculateFireballScore(aiTac.performer, fireballTarget, aiTac.spellPktBody,
                    aiTac.spellPktBody.spellEnum);
                if (score > fireballScoreBest)
                {
                    fireballScoreBest = score;
                    fireballTargetBest = fireballTarget;
                }
            }

            // Not quite sure what this check is for, to be honest...
            if (AdjustCastLocationToClosestEnemy(fireballTarget, aiTac.performer, out fireballTarget))
            {
                var score = CalculateFireballScore(aiTac.performer, fireballTarget, aiTac.spellPktBody,
                    aiTac.spellPktBody.spellEnum);
                if (score > fireballScoreBest)
                {
                    fireballScoreBest = score;
                    fireballTargetBest = fireballTarget;
                }
            }

            if (fireballScoreBest <= 0.0f)
            {
                loc = LocAndOffsets.Zero;
                return false;
            }
            else
            {
                loc = LocAndOffsets.FromInches(fireballTargetBest);
                return true;
            }
        }

        /// <summary>
        /// Higher is better.
        /// </summary>
        [TempleDllLocation(0x100e2fa0)]
        private float CalculateFireballScore(GameObject caster, Vector2 castAt, SpellPacketBody spellPkt,
            int spellEnum)
        {
            var fireballScore = 0.0f;
            var loc = LocAndOffsets.FromInches(castAt);
            if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spEntry))
            {
                return 0.0f;
            }

            var pickerArgs = new PickerArgs();
            GameSystems.Spell.PickerArgsFromSpellEntry(spEntry, pickerArgs, caster, spellPkt.casterLevel);
            pickerArgs.SetAreaTargets(loc);

            foreach (var target in pickerArgs.result.objList)
            {
                if (!GameSystems.Critter.IsFriendly(caster, target))
                {
                    if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_AI_Fireball_OK) )
                    {
                        fireballScore += 0.2f;
                    }
                    else
                    {
                        fireballScore += 1.0f;
                    }
                }
                else
                {
                    if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_AI_Fireball_OK) )
                    {
                        fireballScore -= 1.1f;
                    }
                    else
                    {
                        return -1.0f;
                    }
                }
            }

            return fireballScore;
        }

        [TempleDllLocation(0x100e2d70)]
        private bool AdjustCastLocationToClosestEnemy(Vector2 castAt, GameObject performer,
            out Vector2 actualCastTarget)
        {
            var locXy = LocAndOffsets.FromInches(castAt);
            GameObject target;
            float targetDist;
            if (GetClosestEnemy(performer, locXy, out target, out targetDist, true) && targetDist <= 240.0f
                || GetClosestEnemy(performer, locXy, out target, out targetDist, false) && targetDist <= 240.0f)
            {
                var targetPos = target.GetLocationFull().ToInches2D();
                var dir = Vector2.Normalize(castAt - targetPos);
                actualCastTarget = targetPos + dir * (60.0f + 240.0f);
                return true;
            }
            else
            {
                actualCastTarget = castAt;
                return true;
            }
        }

        [TempleDllLocation(0x100e2b80)]
        private bool GetClosestEnemy(GameObject obj, LocAndOffsets loc, out GameObject target,
            out float targetDistance, bool onlyValidFireballTargets)
        {
            targetDistance = 10000.0f;
            target = null;

            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (GameSystems.Critter.IsDeadOrUnconscious(combatant))
                {
                    continue;
                }

                if (GameSystems.Critter.IsFriendly(obj, combatant))
                {
                    // Skip tagets friendly with the caster
                    continue;
                }

                if (GameSystems.AI.IsCharmedBy(combatant, obj)
                    && GameSystems.AI.TargetIsPcPartyNotDead(combatant))
                {
                    // Skip targets charmed by the caster
                    continue;
                }

                if (onlyValidFireballTargets)
                {
                    if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_AI_Fireball_OK) )
                    {
                        continue;
                    }
                }

                var dist = combatant.DistanceToLocInFeet(loc);
                if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                    !GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                {
                    dist = (dist + 5.0f) * 2.5f;
                }

                if (dist < targetDistance)
                {
                    targetDistance = dist;
                    target = combatant;
                }
            }

            return target != null;
        }

        [TempleDllLocation(0x100e2ec0)]
        private bool GetAverageEnemyLocation(GameObject caster, out Vector2 averagedCenter)
        {
            var enemyCenter = Vector2.Zero;
            var enemyCount = 0;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (!GameSystems.Critter.IsFriendly(caster, combatant))
                {
                    enemyCenter += combatant.GetLocationFull().ToInches2D();
                    ++enemyCount;
                }
            }

            if (enemyCount > 0)
            {
                averagedCenter = enemyCenter / enemyCount;
                return true;
            }
            else
            {
                averagedCenter = Vector2.Zero;
                return false;
            }
        }
    }

    internal class TacticCastArea : AiTacticDef
    {
        public static readonly TacticCastArea Instance = new TacticCastArea();

        public string name => "cast area";

        [TempleDllLocation(0x100e4510)]
        public bool aiFunc(AiTactic aiTac)
        {
            var actNumOrg = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;
            if (aiTac.target == null)
                return false;
            if (GameSystems.D20.D20Query(aiTac.target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    aiTac.spellPktBody.spellEnum) )
            {
                return false;
            }

            var performer = aiTac.performer;
            var enemies = GameSystems.Combat.GetEnemiesCanMelee(performer);
            var castDefensively = 0;
            if (enemies.Count > 0)
            {
                castDefensively = 1;
            }

            GameSystems.D20.D20SendSignal(performer, D20DispatcherKey.SIG_SetCastDefensively, castDefensively);

            if (!GameSystems.Spell.TryGetSpellEntry(aiTac.d20SpellData.spellEnumOrg, out var spEntry))
            {
                return false;
            }

            PickerArgs pickArgs = new PickerArgs();
            var locAndOff = aiTac.target.GetLocationFull();
            GameSystems.Spell.PickerArgsFromSpellEntry(spEntry, pickArgs, performer, aiTac.spellPktBody.casterLevel);

            pickArgs.SetAreaTargets(locAndOff);
            GameSystems.Spell.ConfigSpellTargetting(pickArgs, aiTac.spellPktBody);

            GameSystems.Spell.GetSpellTargets(performer, aiTac.target, aiTac.spellPktBody,
                aiTac.spellPktBody.spellEnum);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CAST_SPELL, 0);
            GameSystems.D20.Actions.ActSeqCurSetSpellPacket(aiTac.spellPktBody, false);
            GameSystems.D20.Actions.GlobD20ActnSetSpellData(aiTac.d20SpellData);

            GameSystems.D20.Actions.GlobD20ActnSetTarget(null, locAndOff);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                GameSystems.D20.Actions.ActionSequenceRevertPath(actNumOrg);
                return false;
            }

            return true;
        }
    }

    internal class TacticCastParty : AiTacticDef
    {
        public static readonly TacticCastParty Instance = new TacticCastParty();

        public string name => "cast party";

        [TempleDllLocation(0x100e43f0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var initialActNum = GameSystems.D20.Actions.CurrentSequence.d20ActArrayNum;
            if (aiTac.target == null)
            {
                return false;
            }

            int castDefensively = 0;
            var enemiesCanMelee = GameSystems.Combat.GetEnemiesCanMelee(aiTac.performer);
            if (enemiesCanMelee.Count > 0)
            {
                castDefensively = 1;
            }

            GameSystems.D20.D20SendSignal(aiTac.performer, D20DispatcherKey.SIG_SetCastDefensively, castDefensively);
            LocAndOffsets targetLoc = aiTac.target.GetLocationFull();

            aiTac.spellPktBody.SetTargets(GameSystems.Party.PartyMembers.ToArray());

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CAST_SPELL, 0);
            GameSystems.D20.Actions.ActSeqCurSetSpellPacket(aiTac.spellPktBody,
                true); // ignore LOS changed to 1, was originally 0
            GameSystems.D20.Actions.GlobD20ActnSetSpellData(aiTac.d20SpellData);
            // originally fetched a concious party member, seems like a bug so I changed it to the target
            GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, targetLoc);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                GameSystems.D20.Actions.ActionSequenceRevertPath(initialActNum);
                return false;
            }

            return true;
        }
    }

    internal class TacticApproach : AiTacticDef
    {
        public static readonly TacticApproach Instance = new TacticApproach();

        public string name => "approach";

        [TempleDllLocation(0x100e48d0)]
        public bool aiFunc(AiTactic aiTac)
        {
            int initialActNum = GameSystems.D20.Actions.CurrentSequence.d20ActArrayNum;
            if (aiTac.target == null)
                return false;

            if (GameSystems.Combat.IsWithinReach(aiTac.performer, aiTac.target))
            {
                return false;
            }

            // check if 5' step is a good choice
            var isWorth = GameSystems.AI.Is5FootStepWorth(aiTac);

            if (isWorth)
            {
                GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.FIVEFOOTSTEP, 0);
                GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
                if (GameSystems.D20.Actions.ActionAddToSeq() == ActionErrorCode.AEC_OK
                    && GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() == ActionErrorCode.AEC_OK)
                {
                    return true;
                }

                GameSystems.D20.Actions.ActionSequenceRevertPath(initialActNum);
            }

            GameSystems.D20.Actions.CurSeqReset(aiTac.performer);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.UNSPECIFIED_MOVE, 0);
            GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                GameSystems.D20.Actions.ActionSequenceRevertPath(initialActNum);
                return false;
            }

            return true;
        }
    }

    internal class TacticClearTarget : AiTacticDef
    {
        public static readonly TacticClearTarget Instance = new TacticClearTarget();

        public string name => "clear target";

        [TempleDllLocation(0x100e4670)]
        public bool aiFunc(AiTactic a1)
        {
            a1.target = null;
            return false;
        }
    }

    internal class TacticAttackThreatened : AiTacticDef
    {
        public static readonly TacticAttackThreatened Instance = new TacticAttackThreatened();

        public string name => "attack threatened";

        [TempleDllLocation(0x100e4680)]
        public bool aiFunc(AiTactic aiTac)
        {
            if (aiTac.target == null || !GameSystems.Combat.CanMeleeTarget(aiTac.performer, aiTac.target))
            {
                return false;
            }

            return TacticDefault.Instance.aiFunc(aiTac);
        }
    }

    internal class TacticAttack : AiTacticDef
    {
        public static readonly TacticAttack Instance = new TacticAttack();

        public string name => "attack";

        [TempleDllLocation(0x100e46c0)]
        public bool aiFunc(AiTactic aiTac)
        {
            return TacticDefault.Instance.aiFunc(aiTac);
        }
    }

    internal class TacticTargetThreatened : AiTacticDef
    {
        public static readonly TacticTargetThreatened Instance = new TacticTargetThreatened();

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public string name => "target threatened";

        [TempleDllLocation(0x100e46d0)]
        public bool aiFunc(AiTactic aiTac)
        {
            GameObject performer = aiTac.performer;
            bool foundTarget = false;
            float closestDist = 1000000000.0f;
            var performerLoc = aiTac.performer.GetLocationFull();

            Logger.Info("{0} targeting threatened...", aiTac.performer);

            using var objlist = ObjList.ListVicinity(performerLoc.location, ObjectListFilter.OLC_CRITTERS);

            GameObject ignoredTarget = null;
            float ignoredDist = 1000000000.0f;
            foreach (var dude in objlist)
            {
                var ignoreTarget = performer.ShouldIgnoreTarget(dude);

                if (!GameSystems.Critter.IsFriendly(dude, performer)
                    && !GameSystems.Critter.IsDeadNullDestroyed(dude)
                    && GameSystems.Combat.IsWithinReach(performer, dude)
                )
                {
                    var distToDude = performer.DistanceToObjInFeet(dude);

                    if (!ignoreTarget && distToDude < closestDist)
                    {
                        aiTac.target = dude;
                        closestDist = performer.DistanceToObjInFeet(dude);
                        foundTarget = true;
                    }
                    else if (ignoreTarget && distToDude < ignoredDist)
                    {
                        // Remember the closest ignored target as a fallback
                        ignoredTarget = dude;
                        ignoredDist = distToDude;
                    }
                }
            }

            if (!foundTarget)
            {
                // check if already moved - if so, use the threatened target (todo: regard spellcasting)
                var curSeq = GameSystems.D20.Actions.CurrentSequence;
                if ((curSeq.tbStatus.tbsFlags & (TurnBasedStatusFlags.Moved | TurnBasedStatusFlags.Moved5FootStep)) ==
                    (TurnBasedStatusFlags.Moved | TurnBasedStatusFlags.Moved5FootStep))
                {
                    if (ignoredTarget != null)
                    {
                        aiTac.target = ignoredTarget;
                        Logger.Info("{0} targeted because there was no other legit target and am out of moves.",
                            ignoredTarget);
                        return false;
                    }
                }

                aiTac.target = null;
                Logger.Info("no target found. ");
            }
            else
            {
                Logger.Info("{0} targeted.", aiTac.target);
            }

            return false;
        }
    }

    internal class TacticTargetNospell : AiTacticDef
    {
        public static readonly TacticTargetNospell Instance = new TacticTargetNospell();

        public string name => "target nospell";

        [TempleDllLocation(0x100e4780)]
        public bool aiFunc(AiTactic tactic)
        {
            var closestDistance = 1000.0f;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != tactic.performer)
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(combatant)
                        && !GameSystems.Critter.IsFriendly(tactic.performer, combatant)
                        && !GameSystems.D20.D20Query(
                            combatant,
                            D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                            tactic.spellPktBody.spellEnum) )
                    {
                        var dist = tactic.performer.DistanceToObjInFeet(combatant);
                        if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                            !GameSystems.D20.D20Query(tactic.performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                        {
                            dist = (dist + 5.0f) * 2.5f;
                        }

                        if (dist < closestDistance)
                        {
                            closestDistance = dist;
                            tactic.target = combatant;
                        }
                    }
                }
            }

            return false;
        }
    }

    internal class TacticFiveFootStep : AiTacticDef
    {
        public static readonly TacticFiveFootStep Instance = new TacticFiveFootStep();

        public string name => "five foot step";

        [TempleDllLocation(0x100e4890)]
        public bool aiFunc(AiTactic aiTac)
        {
            var result = GameSystems.Combat.GetEnemiesCanMelee(aiTac.performer).Count;
            if (result > 0)
            {
                return GameSystems.AI.AiFiveFootStepAttempt(aiTac);
            }

            return false;
        }
    }

    internal class TacticCoupDeGrace : AiTacticDef
    {
        public static readonly TacticCoupDeGrace Instance = new TacticCoupDeGrace();

        public string name => "coup de grace";

        [TempleDllLocation(0x100e5db0)]
        public bool aiFunc(AiTactic aiTac)
        {
            GameObject origTarget = aiTac.target;
            var actNum = GameSystems.D20.Actions.CurrentSequence.d20ActArrayNum;
            aiTac.target = null;
            GameSystems.AI.TargetClosestEnemy(aiTac, true);
            var performer = aiTac.performer;
            var perfLoc = performer.GetLocationFull();

            var isThreatened = GameSystems.D20.Combat.HasThreateningCrittersAtLoc(aiTac.performer, perfLoc);

            if (aiTac.target != null)
            {
                if (GameSystems.Critter.IsDeadOrUnconscious(aiTac.target))
                {
                    // if the CDG is due to unconscious target...
                    // if it's not in melee range, forget it
                    if (!GameSystems.Combat.CanMeleeTarget(aiTac.performer, aiTac.target))
                        return false;

                    // if the AI is threatened, forget it too (since it incurs an AOO)
                    if (isThreatened)
                        return false;
                }

                if (TryCoupDeGrace(aiTac))
                {
                    return true;
                }

                GameSystems.D20.Actions.ActionSequenceRevertPath(actNum);
                return false;
            }

            aiTac.target = origTarget;
            return false;
        }

        private bool TryCoupDeGrace(AiTactic aiTac)
        {
            if (!TacticApproach.Instance.aiFunc(aiTac))
            {
                return false;
            }

            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.COUP_DE_GRACE, 0);
            GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
            GameSystems.D20.Actions.ActionAddToSeq();
            return GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() == ActionErrorCode.AEC_OK;
        }
    }

    internal class TacticUsePotion : AiTacticDef
    {
        public static readonly TacticUsePotion Instance = new TacticUsePotion();

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public string name => "use potion";

        [TempleDllLocation(0x100e4a40)]
        public bool aiFunc(AiTactic aiTac)
        {
            var performer = aiTac.performer;
            var hpCur = performer.GetStat(Stat.hp_current);
            var hpMax = performer.GetStat(Stat.hp_max);
            var hpPct = hpCur * 1.0 / hpMax;

            // check if critter can get whacked by AoO
            if (GameSystems.Combat.GetEnemiesCanMelee(performer).Count > 0)
            {
                Logger.Info("Skipping Use Potion action because threatened by critters.");
                return false;
            }

            foreach (var itemHandle in performer.EnumerateChildren())
            {
                var itemObj = itemHandle;
                if (itemObj.type != ObjectType.food)
                    continue;

                if (itemObj.GetSpellArray(obj_f.item_spell_idx).Count == 0)
                    continue;

                var spData = itemObj.GetSpell(obj_f.item_spell_idx, 0);
                var spEnum = spData.spellEnum;

                if (!GameSystems.Spell.TryGetSpellEntry(spEnum, out var spellEntry))
                {
                    continue;
                }

                var spellAiTypeIsHealing =
                    spellEntry.HasAiType(AiSpellType.ai_action_heal_heavy)
                    || spellEntry.HasAiType(AiSpellType.ai_action_heal_light)
                    || spellEntry.HasAiType(AiSpellType.ai_action_heal_medium);

                if (spellAiTypeIsHealing)
                {
                    if (hpPct >= 0.5)
                        continue;
                    // prevent usage of light healing when already fairly healthy...
                    if (spellEntry.HasAiType(AiSpellType.ai_action_heal_light) && hpCur >= 25)
                        continue;
                }

                if (GameSystems.D20.Actions.DoUseItemAction(performer, performer, itemHandle))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal class TacticCharge : AiTacticDef
    {
        public static readonly TacticCharge Instance = new TacticCharge();

        public string name => "charge";

        [TempleDllLocation(0x100e4bd0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var currentActionNum = GameSystems.D20.Actions.CurrentSequence.d20ActArray.Count;
            if (aiTac.target == null)
            {
                return false;
            }

            GameSystems.D20.Actions.CurSeqReset(aiTac.performer);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CHARGE, 0);
            GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                GameSystems.D20.Actions.ActionSequenceRevertPath(currentActionNum);
                return false;
            }

            return true;
        }
    }

    internal class TacticGoMelee : AiTacticDef
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static readonly TacticGoMelee Instance = new TacticGoMelee();

        public string name => "go melee";

        [TempleDllLocation(0x100e55a0)]
        public bool aiFunc(AiTactic aiTac)
        {
            Logger.Info("Attempting Go Melee...");
            var performer = aiTac.performer;
            var tbStat = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
            var weapon = GameSystems.Critter.GetWornItem(performer, EquipSlot.WeaponPrimary);
            if (weapon == null)
            {
                return false;
            }

            if (!GameSystems.Item.IsRangedWeapon(weapon))
            {
                return false;
            }

            foreach (var item in performer.EnumerateChildren())
            {
                if (item.type == ObjectType.weapon && (item.WeaponFlags & WeaponFlag.RANGED_WEAPON) == default)
                {
                    GameSystems.Item.UnequipItemInSlot(performer, EquipSlot.WeaponPrimary);
                    GameSystems.Item.UnequipItemInSlot(performer, EquipSlot.WeaponSecondary);
                    GameSystems.Item.ItemPlaceInIndex(item, 203);
                    tbStat.hourglassState = GameSystems.D20.Actions.GetHourglassTransition(tbStat.hourglassState, ActionCostType.Move);
                    Logger.Info("Go Melee succeeded.");
                    return true;
                }
            }

            return false;
        }
    }

    internal class TacticTargetNearbyBuffed : AiTacticDef
    {
        public static readonly TacticTargetNearbyBuffed Instance = new TacticTargetNearbyBuffed();

        public string name => "target nearby buffed";

        [TempleDllLocation(0x100e4c40)]
        public bool aiFunc(AiTactic aiTactic)
        {
            var enemies = GameSystems.D20.Actions.GetEnemiesWithinReach(aiTactic.performer);
            foreach (var enemy in enemies)
            {
                var buffDebuffPacket = GameSystems.D20.BuffDebuff.GetBuffDebuff(enemy);
                var debuffCount = buffDebuffPacket.GetCountByType(BuffDebuffType.Debuff);
                var buffCount = buffDebuffPacket.GetCountByType(BuffDebuffType.Buff);
                if (buffCount > debuffCount)
                {
                    aiTactic.target = enemy;
                    return false;
                }
            }

            return false;
        }
    }

    internal abstract class TacticTargetBadSave : AiTacticDef
    {
        public abstract string name { get; }

        [TempleDllLocation(0x100e4f00)]
        [TempleDllLocation(0x100e4ce0)]
        public bool aiFunc(AiTactic aiTac)
        {
            var closestDist = 10000.0f;

            var performer = aiTac.performer;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant == performer || GameSystems.Critter.IsFriendly(performer, combatant))
                {
                    continue;
                }

                if (GameSystems.Critter.IsDeadOrUnconscious(combatant))
                {
                    continue;
                }

                if (GetBadSaveEstimate(combatant) > 0)
                {
                    var dist = aiTac.performer.DistanceToObjInFeet(combatant);
                    if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                        !GameSystems.D20.D20Query(aiTac.performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                    {
                        dist = (dist + 5.0f) * 2.5f;
                    }

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        aiTac.target = combatant;
                    }
                }
            }

            return false;
        }

        protected abstract int GetBadSaveEstimate(GameObject critter);
    }

    internal class TacticTargetBadWill : TacticTargetBadSave
    {
        public static readonly TacticTargetBadWill Instance = new TacticTargetBadWill();

        public override string name => "target bad will";

        [TempleDllLocation(0x100e3100)]
        protected override int GetBadSaveEstimate(GameObject critter)
        {
            // TODO: This is a bad estimate... It ignores new classes
            var estimate = 0;

            foreach (var classStat in D20ClassSystem.VanillaClasses)
            {
                var levelCount = critter.GetStat(classStat);
                if (levelCount > 0)
                {
                    if (classStat == Stat.level_barbarian ||
                        classStat > Stat.level_druid && classStat <= Stat.level_rogue)
                    {
                        estimate += levelCount;
                    }
                    else
                    {
                        estimate -= levelCount;
                    }
                }
            }

            return estimate;
        }
    }

    internal class TacticTargetBadFort : TacticTargetBadSave
    {
        public static readonly TacticTargetBadFort Instance = new TacticTargetBadFort();

        public override string name => "target bad fort";

        [TempleDllLocation(0x100e3170)]
        protected override int GetBadSaveEstimate(GameObject critter)
        {
            // TODO: This is a bad estimate... It ignores new classes
            var estimate = 0;
            foreach (var classStat in D20ClassSystem.VanillaClasses)
            {
                var levelCount = critter.GetStat(classStat);
                if (levelCount > 0)
                {
                    if (classStat == Stat.level_bard ||
                        classStat >= Stat.level_rogue && classStat <= Stat.level_wizard)
                    {
                        estimate += levelCount;
                    }
                    else
                    {
                        estimate -= levelCount;
                    }
                }
            }

            return estimate;
        }
    }

    internal class TacticTargetBadReflex : TacticTargetBadSave
    {
        public static readonly TacticTargetBadReflex Instance = new TacticTargetBadReflex();

        public override string name => "target bad reflex";

        [TempleDllLocation(0x100e31e0)]
        protected override int GetBadSaveEstimate(GameObject critter)
        {
            // TODO: This is a bad estimate... It ignores new classes
            var estimate = 0;
            foreach (var classStat in D20ClassSystem.VanillaClasses)
            {
                var levelCount = critter.GetStat(classStat);
                if (levelCount > 0)
                {
                    switch (classStat)
                    {
                        case Stat.level_barbarian:
                        case Stat.level_cleric:
                        case Stat.level_druid:
                        case Stat.level_fighter:
                        case Stat.level_monk:
                        case Stat.level_paladin:
                        case Stat.level_sorcerer:
                        case Stat.level_wizard:
                            estimate += levelCount;
                            break;
                        default:
                            estimate -= levelCount;
                            break;
                    }
                }
            }

            return estimate;
        }
    }

    internal class TacticCastSingleAlone : AiTacticDef
    {
        public static readonly TacticCastSingleAlone Instance = new TacticCastSingleAlone();

        public string name => "cast single alone";

        [TempleDllLocation(0x100e5010)]
        public bool aiFunc(AiTactic aiTac)
        {
            var performer = aiTac.performer;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != performer)
                {
                    if (GameSystems.Critter.IsFriendly(performer, combatant))
                    {
                        return false;
                    }
                }
            }

            return TacticCastSingle.Instance.aiFunc(aiTac);
        }
    }
}