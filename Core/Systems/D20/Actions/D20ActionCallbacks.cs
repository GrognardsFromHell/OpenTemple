using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Ui.InGameSelect;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public static class D20ActionCallbacks
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public static ActionErrorCode ActionCheckDivineMight(D20Action action, TurnBasedStatus tbStatus)
        {
            DispIoD20ActionTurnBased dispIo = DispIoD20ActionTurnBased.Default;
            dispIo.returnVal = 0;
            dispIo.action = action;
            dispIo.tbStatus = tbStatus;
            var dispatcher = action.d20APerformer.GetDispatcher();
            var alignmentchoice = action.d20APerformer.GetInt32(obj_f.critter_alignment_choice);
            if (alignmentchoice == 2)
                action.data1 = 1;

            dispatcher?.Process(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_TURN_UNDEAD, dispIo);

            return dispIo.returnVal;
        }

        public static ActionErrorCode PerformDivineMight(D20Action action)
        {
            var dispIo = DispIoD20ActionTurnBased.Default;
            dispIo.returnVal = 0;
            dispIo.action = action;
            dispIo.tbStatus = null;
            // Deduct a turn undead charge for divine might
            GameSystems.D20.D20SignalPython(action.d20APerformer, "Deduct Turn Undead Charge");

            var chaMod = action.d20APerformer.GetStat(Stat.cha_mod);

            action.d20APerformer.AddCondition("Divine Might Bonus", chaMod, 0);

            return dispIo.returnVal;
        }

        public static ActionErrorCode PerformEmptyBody(D20Action action)
        {
            DispIoD20ActionTurnBased dispIo = DispIoD20ActionTurnBased.Default;
            dispIo.returnVal = 0;
            dispIo.action = action;
            dispIo.tbStatus = null;

            var dispatcher = action.d20APerformer.GetDispatcher();

            // TODO This translates to it being only stored in data1 I believe...
            int numRounds =
                (int) ((GameSystems.D20.D20QueryReturnData(action.d20APerformer,
                            D20DispatcherKey.QUE_Empty_Body_Num_Rounds, 1) & 0xffFFffFF00000000) >> 32);
            dispIo.action.data1 = numRounds; // number of rounds to deduct from max possible

            // reduces the number of remaining rounds by 1
            dispatcher?.Process(DispatcherType.D20ActionPerform, D20DispatcherKey.D20A_EMPTY_BODY, dispIo);

            action.d20APerformer.AddCondition("Ethereal", 0, 0, numRounds);

            return dispIo.returnVal;
        }

        [TempleDllLocation(0x1008c720)]
        public static ActionErrorCode FullAttackPerform(D20Action action)
        {
            // this function is largely irrelevant...

            var curSeq = GameSystems.D20.Actions.CurrentSequence;


            // if no subsequent actions
            if (curSeq.IsLastAction)
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, 5001); // Full Attack

                if (GameSystems.Combat.IsCombatActive() && !GameSystems.D20.Actions.actSeqPickerActive
                                                        && GameSystems.Party.IsPlayerControlled(curSeq.performer))
                {
                    if (curSeq.tbStatus.baseAttackNumCode + curSeq.tbStatus.numBonusAttacks >
                        curSeq.tbStatus.attackModeCode)
                    {
                        GameSystems.D20.Actions.seqPickerD20ActnType = D20ActionType.STANDARD_ATTACK;
                        GameSystems.D20.Actions.seqPickerD20ActnData1 = 0;
                        GameSystems.D20.Actions.seqPickerTargetingType = D20TargetClassification.SingleExcSelf;
                    }
                }
            }
            else
            {
                if (GameSystems.Combat.IsCombatActive() && !GameSystems.D20.Actions.actSeqPickerActive
                                                        && GameSystems.Party.IsPlayerControlled(curSeq.performer))
                {
                    /*	if (curSeq.tbStatus.baseAttackNumCode + curSeq.tbStatus.numBonusAttacks > curSeq.tbStatus.attackModeCode) {
                            GameSystems.D20.Actions.seqPickerD20ActnType = D20ActionType.STANDARD_ATTACK;
                            GameSystems.D20.Actions.seqPickerD20ActnData1 = 0;
                            GameSystems.D20.Actions.seqPickerTargetingType = D20TargetClassification.SingleExcSelf;
                        }*/
                }
            }

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode PerformPython(D20Action action)
        {
            DispIoD20ActionTurnBased dispIo = new DispIoD20ActionTurnBased(action);
            dispIo.DispatchPythonActionPerform((D20DispatcherKey) action.data1);

            return dispIo.returnVal;
        }

        public static ActionErrorCode PerformQuiveringPalm(D20Action action)
        {
            DispIoD20ActionTurnBased dispIo = new DispIoD20ActionTurnBased(action);
            dispIo.DispatchPerform(D20DispatcherKey.D20A_QUIVERING_PALM);

            if (dispIo.returnVal != ActionErrorCode.AEC_OK)
                return dispIo.returnVal;

            var playCritFlag = false;
            GameSystems.D20.Combat.ToHitProcessing(action);
            var caflags = action.d20Caf;
            if (caflags.HasFlag(D20CAF.CRITICAL)
                || GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Play_Critical_Hit_Anim,
                    (int) caflags, 0) != 0)
            {
                playCritFlag = true;
            }

            var attackAnimSubid = GameSystems.Random.GetInt(0, 2);


            if (GameSystems.Anim.PushAttack(action.d20APerformer, action.d20ATarget, -1, attackAnimSubid, playCritFlag,
                false))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10091040)]
        public static ActionErrorCode SneakPerform(D20Action action)
        {
            var performer = action.d20APerformer;

            var newSneakState = !GameSystems.Critter.IsMovingSilently(performer);
            GameSystems.Anim.Interrupt(performer, AnimGoalPriority.AGP_5);

            if (newSneakState && GameSystems.Combat.IsCombatActive())
            {
                // entering sneak while in combat

                var hasHideInPlainSight = GameSystems.D20.D20QueryPython(performer, "Can Hide In Plain Sight") != 0;

                BonusList sneakerBon = BonusList.Default;
                sneakerBon.AddBonus(-20, 0, 349); // Hiding in Combat
                var sneakerHide = performer.dispatch1ESkillLevel(SkillId.hide, ref sneakerBon, performer, 1);
                var hideRoll = Dice.Roll(1, 20);

                foreach (var combatant in GameSystems.D20.Initiative)
                {
                    if (combatant == performer)
                        continue;

                    if (GameSystems.Critter.IsFriendly(combatant, performer) ||
                        GameSystems.Critter.NpcAllegianceShared(combatant, performer))
                        continue;

                    if (GameSystems.AI.HasLineOfSight(combatant, performer) == 0)
                    {
                        // note: the function actually returns obstacles

                        if (!hasHideInPlainSight)
                            return ActionErrorCode.AEC_INVALID_ACTION;

                        var spotterBon = BonusList.Default;
                        var combatantSpot = combatant.dispatch1ESkillLevel(SkillId.spot, ref spotterBon,
                            combatant, 1);
                        var spotRoll = Dice.Roll(1, 20, 0);
                        if (combatantSpot + spotRoll > hideRoll + sneakerHide)
                        {
                            var rollHistId = GameSystems.RollHistory.RollHistoryAddType6OpposedCheck(performer,
                                combatant, hideRoll, spotRoll,
                                in sneakerBon, in spotterBon,
                                5123,
                                D20CombatMessage.failure,
                                1);
                            GameSystems.RollHistory.CreateRollHistoryString(rollHistId);
                            return ActionErrorCode.AEC_INVALID_ACTION;
                        }
                    }
                }
            }

            GameSystems.Critter.SetMovingSilently(performer, newSneakState);
            return ActionErrorCode.AEC_OK;
        }

        public static bool ActionFrameQuiveringPalm(D20Action action)
        {
            GameObjectBody performer = action.d20APerformer;

            StandardAttackActionFrame(action);
            if (!action.d20Caf.HasFlag(D20CAF.HIT))
            {
                return false;
            }

            var monkLvl = performer.GetStat(Stat.level_monk);
            var wisScore = performer.GetStat(Stat.wisdom);
            var dc = 10 + monkLvl / 2 + (wisScore - 10) / 2;
            if (!GameSystems.D20.Combat.SavingThrow(action.d20ATarget, performer, dc, SavingThrowType.Fortitude))
            {
                GameSystems.D20.Combat.KillWithDeathEffect(action.d20ATarget, performer);
                GameSystems.RollHistory.CreateFromFreeText(
                    $"{GameSystems.MapObject.GetDisplayName(action.d20ATarget)} killed by Quivering Palm!");
                var text = GameSystems.D20.Combat.GetCombatMesLine(215);
                GameUiBridge.ShowTextBubble(performer, text);
            }

            return false;
        }

        [TempleDllLocation(0x1008cf10)]
        public static bool StandardAttackActionFrame(D20Action action)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Prone) != 0)
            {
                //GameSystems.RollHistory.CreateFromFreeText($"{GameSystems.MapObject.GetDisplayName(action.d20APerformer)} aborted attack (prone).".c_str());
                return false;
            }

            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId1);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId2);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId0);
            GameSystems.D20.Combat.DealAttackDamage(action.d20APerformer, action.d20ATarget, action.data1,
                action.d20Caf, action.d20ActType);
            return true;
        }

        public static ActionErrorCode ActionCheckDisarm(D20Action action, TurnBasedStatus tbStatus)
        {
            GameObjectBody performer = action.d20APerformer;
            if (GameSystems.Critter.IsProne(performer) ||
                GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_Unconscious) != 0)
            {
                return ActionErrorCode.AEC_CANT_WHILE_PRONE;
            }

            GameObjectBody weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);

            if (weapon != null && weapon.WeaponFlags.HasFlag(WeaponFlag.RANGED_WEAPON)
            ) // ranged weapon - Need Melee Weapon error
            {
                return ActionErrorCode.AEC_NEED_MELEE_WEAPON;
            }

            if (action.d20ATarget != null)
            {
                GameObjectBody targetWeapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponPrimary);
                if (targetWeapon == null)
                {
                    targetWeapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponSecondary);
                    if (targetWeapon == null)
                        return ActionErrorCode.AEC_TARGET_INVALID;
                    if (targetWeapon.type != ObjectType.weapon)
                        return ActionErrorCode.AEC_TARGET_INVALID;
                }
            }
            else
                return ActionErrorCode.AEC_TARGET_INVALID;

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode ActionCheckEmptyBody(D20Action action, TurnBasedStatus tbStatus)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Is_Ethereal) != 0)
                return ActionErrorCode.AEC_INVALID_ACTION;
            int numRounds =
                (int) (GameSystems.D20.D20QueryReturnData(action.d20APerformer,
                           D20DispatcherKey.QUE_Empty_Body_Num_Rounds, 2, 0) >> 32);
            if (numRounds <= 0)
                return ActionErrorCode.AEC_OUT_OF_CHARGES;

            if (action.data1 > numRounds)
            {
                action.data1 = numRounds;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008d150)]
        public static ActionErrorCode FivefootstepActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (!GameSystems.Combat.IsCombatActive())
                return ActionErrorCode.AEC_OK;
            if ((action.d20Caf & D20CAF.ALTERNATE) != default)
                return ActionErrorCode.AEC_TARGET_BLOCKED;
            var moveSpeed = action.d20APerformer.Dispatch41GetMoveSpeed(out _);
            if (moveSpeed < 0.1)
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode ActionCheckPython(D20Action action, TurnBasedStatus tbStatus)
        {
            DispIoD20ActionTurnBased evtObj = new DispIoD20ActionTurnBased(action);

            evtObj.DispatchPythonActionCheck((D20DispatcherKey) action.data1);

            return evtObj.returnVal;
        }

        public static ActionErrorCode ActionCheckQuiveringPalm(D20Action action, TurnBasedStatus tbStatus)
        {
            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            if (!GameSystems.Combat.IsUnarmed(action.d20APerformer))
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Quivering_Palm_Can_Perform) == 0)
                return ActionErrorCode.AEC_OUT_OF_CHARGES;

            if (GameSystems.D20.D20Query(action.d20ATarget, D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits) != 0)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode SneakActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (GameSystems.Critter.IsMovingSilently(action.d20APerformer)) // will cause to stop sneaking
                return ActionErrorCode.AEC_OK;

            return
                ActionErrorCode
                    .AEC_OK; // used to be possible only outside of combat, but now you can attempt it in combat too
        }

        [TempleDllLocation(0x100907c0)]
        public static ActionErrorCode ChargePerform(D20Action action)
        {
            int crit = 0, isSecondary = 0;
            var performer = action.d20APerformer;
            performer.AddCondition("Charging", 0);
            action.d20Caf |= D20CAF.CHARGE;

            var d20aCopy = action.Copy();

            var weapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
            if (weapon == null)
            {
                weapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);
                if (weapon != null)
                {
                    action.d20Caf |= D20CAF.SECONDARY_WEAPON;
                    action.data1 = AttackPacket.ATTACK_CODE_OFFHAND + 1;
                }
                else if (GameSystems.D20.Actions.DispatchD20ActionCheck(d20aCopy, null,
                             DispatcherType.GetCritterNaturalAttacksNum) != 0)
                {
                    action.data1 = AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1;
                }
                else
                {
                    action.data1 = AttackPacket.ATTACK_CODE_PRIMARY + 1;
                }
            }

            return StandardAttackPerform(action);
        }

        [TempleDllLocation(0x10091be0)]
        public static ActionErrorCode CopyScrollPerform(D20Action action)
        {
            var performer = action.d20APerformer;

            var check = D20ActionVanillaCallbacks.CopyScrollActionCheck(action, null);
            if (check == ActionErrorCode.AEC_INVALID_ACTION)
            {
                GameSystems.Skill.ShowSkillMessage(performer, SkillMessageId.SpellcraftFailureRaiseRank);
                return ActionErrorCode.AEC_OK;
            }
            else if (check != ActionErrorCode.AEC_OK)
            {
                return ActionErrorCode.AEC_OK;
            }

            // get spell enum & level
            var spEnum = action.d20SpellData.SpellEnum;
            var spLvl = GameSystems.Spell.GetSpellLevelBySpellClass(spEnum,
                GameSystems.Spell.GetSpellClass(Stat.level_wizard));

            // check forbidden school
            var spEntry = GameSystems.Spell.GetSpellEntry(spEnum);
            if (GameSystems.Spell.IsForbiddenSchool(performer, spEntry.spellSchoolEnum))
            {
                GameSystems.Skill.ShowSkillMessage(performer, SkillMessageId.SpellcraftSchoolProhibited);
                return ActionErrorCode.AEC_OK;
            }

            var roll = GameSystems.Skill.SkillRoll(performer, SkillId.spellcraft, spLvl + 15, out _,
                1 << (spEntry.spellSchoolEnum + 4));
            if (!roll)
            {
                var spellcraftLvl = GameSystems.Skill.GetSkillRanks(performer, SkillId.spellcraft);
                performer.AddCondition("Failed_Copy_Scroll", spEnum, spellcraftLvl);
                GameSystems.Skill.ShowSkillMessage(performer, SkillMessageId.SpellcraftFailure);
                return ActionErrorCode.AEC_OK;
            }

            GameSystems.Spell.SpellKnownAdd(performer, spEnum, GameSystems.Spell.GetSpellClass(Stat.level_wizard),
                spLvl, 1, 0);
            var scrollObj = GameSystems.Item.GetItemAtInvIdx(performer, action.data1);
            if (scrollObj != null)
            {
                var qty = scrollObj.GetQuantity();
                if (qty > 1)
                {
                    scrollObj.SetQuantity(qty - 1);
                }
                else
                {
                    GameSystems.Object.Destroy(scrollObj);
                }
            }

            GameSystems.Skill.ShowSkillMessage(performer, SkillMessageId.SpellcraftSuccess);

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode PerformDisarm(D20Action action)
        {
            if (GameSystems.Anim.PushAttemptAttack(action.d20APerformer, action.d20ATarget))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100908b0)]
        public static bool ChargeActionFrame(D20Action action)
        {
            StandardAttackActionFrame(action);
            return false;
        }

        public static bool ActionFrameDisarm(D20Action action)
        {
            GameObjectBody performer = action.d20APerformer;
            bool failedOnce = false;
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Can_Perform_Disarm) == 0)
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, 195); //fail!
                failedOnce = true;
            }

            else if (GameSystems.Combat.DisarmCheck(action.d20APerformer, action.d20ATarget))
            {
                GameObjectBody targetWeapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponPrimary);
                GameObjectBody attackerWeapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
                GameObjectBody attackerOffhand = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);
                GameObjectBody attackerShield = GameSystems.Item.ItemWornAt(performer, EquipSlot.Shield);
                if (targetWeapon == null)
                    targetWeapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponSecondary);
                if (attackerWeapon == null && attackerOffhand == null)
                {
                    if ((GameSystems.Item.GetWieldType(action.d20APerformer, targetWeapon) != 2
                         || attackerShield == null
                         || attackerShield.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                        && action.d20APerformer.GetStat(Stat.level_monk) == 0)
                    {
                        GameSystems.Item.SetParentAdvanced(targetWeapon, action.d20APerformer, 203, 0);
                    }
                    else
                    {
                        GameSystems.Item.ItemDrop(targetWeapon);
                        GameSystems.Item.SetItemParent(targetWeapon, action.d20APerformer, ItemInsertFlag.Use_Bags);
                    }

                    GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, 202);
                }
                else if (targetWeapon != null)
                {
                    GameSystems.Item.ItemDrop(targetWeapon);
                    GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, 198);
                }

                // action.d20ATarget.AddCondition("Disarmed", targetWeapon, targetWeapon,0,0,0,0,0,0);
                throw new NotImplementedException(); // TODO: no way of passing objhndl to condition yet
                return false;
            }

            // counter attempt
            if (GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.IMPROVED_DISARM) == 0)
            {
                D20Action d20aCopy = action.Copy();
                d20aCopy.d20APerformer = action.d20ATarget;
                d20aCopy.d20ATarget = action.d20APerformer;
                if (D20ActionDefs.GetActionDef(D20ActionType.DISARM).actionCheckFunc(d20aCopy, null) ==
                    ActionErrorCode.AEC_OK)
                {
                    if (GameSystems.Anim.PushAttemptAttack(d20aCopy.d20APerformer, d20aCopy.d20ATarget))
                        d20aCopy.animID = GameSystems.Anim.GetActionAnimId(d20aCopy.d20APerformer);
                    if (GameSystems.Combat.DisarmCheck(action.d20ATarget, action.d20APerformer))
                    {
                        var weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
                        if (weapon == null)
                            weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponSecondary);
                        if (weapon != null)
                        {
                            GameSystems.Item.ItemDrop(weapon);
                        }

                        GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, 200); // Counter Disarmed!
                        // action.d20APerformer.AddCondition("Disarmed", weapon, weapon, 0,0,0,0,0,0);
                        throw new NotImplementedException(); // TODO: no way of passing objhndl to condition yet
                        return false;
                    }
                    else if (!failedOnce)
                    {
                        GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, 195);
                    }
                }
                else if (!failedOnce)
                {
                    GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, 195);
                }
            }
            else if (!failedOnce)
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, 195);
            }

            return false;
        }

        public static bool ActionFramePython(D20Action action)
        {
            DispIoD20ActionTurnBased dispIo = new DispIoD20ActionTurnBased(action);
            dispIo.DispatchPythonActionFrame((D20DispatcherKey) action.data1);

            return true;
        }

#pragma region Retrieve Disarmed Weapon
        public static ActionErrorCode LocationCheckDisarmedWeaponRetrieve(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets loc)
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                return ActionErrorCode.AEC_OK;
            }

            if (action.d20ATarget != null)
            {
                return GameSystems.D20.Combat.TargetWithinReachOfLoc(action.d20APerformer, action.d20ATarget, loc)
                    ? ActionErrorCode.AEC_OK
                    : ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            GameObjectBody weapon =
                GameSystems.D20.D20QueryReturnObject(action.d20APerformer, D20DispatcherKey.QUE_Disarmed);
            if (weapon != null)
            {
                return GameSystems.D20.Combat.TargetWithinReachOfLoc(action.d20APerformer, weapon, loc)
                    ? ActionErrorCode.AEC_OK
                    : ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            return ActionErrorCode.AEC_TARGET_INVALID;
        }

        public static ActionErrorCode LocationCheckPython(D20Action action, TurnBasedStatus tbStatus, LocAndOffsets loc)
        {
            return ActionErrorCode.AEC_OK; // TODO
        }

        public static ActionErrorCode ActionCheckDisarmedWeaponRetrieve(D20Action action, TurnBasedStatus tbStatus)
        {
            int dummy = 1;
            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode PerformDisarmedWeaponRetrieve(D20Action action)
        {
            GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_Disarmed_Weapon_Retrieve, action);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10090dd0)]
        public static ActionErrorCode DismissSpellsPerform(D20Action action)
        {
            var spellId = action.data1;
            GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_Dismiss_Spells, spellId, 0);
            var spPkt = GameSystems.Spell.GetActiveSpell(spellId);
            if (spPkt.spellEnum == 0)
                return ActionErrorCode.AEC_OK;

            if (spPkt.caster != null)
                GameSystems.D20.D20SendSignal(spPkt.caster, D20DispatcherKey.SIG_Dismiss_Spells, spellId, 0);
            if (spPkt.aoeObj != null)
            {
                GameSystems.D20.D20SendSignal(spPkt.aoeObj, D20DispatcherKey.SIG_Dismiss_Spells, spellId, 0);
            }

            for (var i = 0u; i < spPkt.targetCount; i++)
            {
                var tgtHndl = spPkt.targetListHandles[i];
                if (tgtHndl != null)
                {
                    GameSystems.D20.D20SendSignal(tgtHndl, D20DispatcherKey.SIG_Dismiss_Spells, spellId, 0);
                }
            }

            // in case the dismiss handlers didn't take care of this themselves: (e.g. Grease effect)
            if (spPkt.aoeObj != null)
            {
                GameSystems.D20.D20SendSignal(spPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellId, 0);
            }

            return ActionErrorCode.AEC_OK;
        }


#pragma endregion

        public static ActionErrorCode ActionCheckSunder(D20Action action, TurnBasedStatus tbStatus)
        {
            GameObjectBody weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);

            if (weapon == null || weapon.WeaponFlags.HasFlag(WeaponFlag.RANGED_WEAPON)
            ) // ranged weapon - Need Melee Weapon error
            {
                return ActionErrorCode.AEC_NEED_MELEE_WEAPON;
            }

            if (!GameSystems.Weapon.IsSlashingOrBludgeoning(weapon))
            {
                return ActionErrorCode.AEC_WRONG_WEAPON_TYPE;
            }

            if (action.d20ATarget != null)
            {
                GameObjectBody targetWeapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponPrimary);
                if (targetWeapon == null)
                {
                    targetWeapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponSecondary);
                    if (targetWeapon != null)
                        return ActionErrorCode.AEC_OK;
                    targetWeapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.Shield); // shield
                    if (targetWeapon == null)
                        return ActionErrorCode.AEC_TARGET_INVALID;
                }
            }
            else
                return ActionErrorCode.AEC_TARGET_INVALID;

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100911b0)]
        public static ActionErrorCode TripActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            var weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
            if (weapon != null && weapon.WeaponFlags.HasFlag(WeaponFlag.RANGED_WEAPON))
            {
                return ActionErrorCode.AEC_NEED_MELEE_WEAPON;
            }

            // doing trip on a full attack
            //if (tbStatus.tbsFlags & TurnBasedStatusFlags.FullAttack &&action.d20ATarget == null)
            //	return ActionErrorCode.AEC_OK;


            if (action.d20ATarget == null)
                return ActionErrorCode.AEC_TARGET_INVALID;

            if (GameSystems.D20.D20Query(action.d20ATarget, D20DispatcherKey.QUE_Prone) != 0)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008bfd0)]
        public static ActionErrorCode ActionCostSpell(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.hourglassCost = 0;
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0.0f;
            var flags = action.d20Caf;
            if ((flags.HasFlag(D20CAF.FREE_ACTION)) || !GameSystems.Combat.IsCombatActive())
            {
                return ActionErrorCode.AEC_OK;
            }

            var spEnum = action.d20SpellData.SpellEnum;
            var spellClass = action.d20SpellData.spellClassCode;
            var mmData = action.d20SpellData.metaMagicData;

            //Modify metamagic information for quicken if necessary
            mmData = GameSystems.D20.Actions.globD20Action.d20APerformer.DispatchMetaMagicModify(mmData);

            var spEntry = GameSystems.Spell.GetSpellEntry(spEnum);

            // Metamagicked spontaneous casters always cost full round to cast
            if (mmData.HasModifiers && !GameSystems.Spell.IsDomainSpell(spellClass)
                                    && D20ClassSystem.IsNaturalCastingClass(
                                        GameSystems.Spell.GetCastingClass(spellClass)))
            {
                acp.hourglassCost = 4;
                return ActionErrorCode.AEC_OK;
            }

            // Quicken Spell handling
            var tbsFlags = tbStatus.tbsFlags;
            if (mmData.IsQuicken)
            {
                if (!tbsFlags.HasFlag(TurnBasedStatusFlags.FreeActionSpellPerformed))
                {
                    tbStatus.tbsFlags |= TurnBasedStatusFlags.FreeActionSpellPerformed;
                    acp.hourglassCost = 0;
                    return ActionErrorCode.AEC_OK;
                }
            }

            tbStatus.surplusMoveDistance = 0;
            tbStatus.numAttacks = 0;
            tbStatus.baseAttackNumCode = 0;
            tbStatus.numBonusAttacks = 0;
            tbStatus.attackModeCode = 0;

            switch (spEntry.castingTimeType)
            {
                case 0: // standard
                    acp.hourglassCost = 2;
                    return ActionErrorCode.AEC_OK;
                case 1: // full round
                    acp.hourglassCost = 4;
                    return ActionErrorCode.AEC_OK;
                case 2
                    : // there was a check for combat here but it's done at the start of the function anyway, and it didn't do anything anyway except print "spells with casttime_out_of_combat need an 'action cost' > 'full_round'!"
                    return ActionErrorCode.AEC_OK;
                case 3:
                    return ActionErrorCode.AEC_OUT_OF_COMBAT_ONLY;
                case 4:
                    if (tbsFlags.HasFlag(TurnBasedStatusFlags.FreeActionSpellPerformed))
                    {
                        // if already performed free action spell
                        acp.hourglassCost = 2;
                        return ActionErrorCode.AEC_OK;
                    }
                    else
                    {
                        tbStatus.tbsFlags |= TurnBasedStatusFlags.FreeActionSpellPerformed;
                        acp.hourglassCost = 0;
                        return ActionErrorCode.AEC_OK;
                    }

                default:
                    return ActionErrorCode.AEC_OK;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c290)]
        public static ActionErrorCode ActionCostFullRound(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            acp.hourglassCost = 4;
            if (((action.d20Caf & D20CAF.FREE_ACTION) != default) || !GameSystems.Combat.IsCombatActive())
                acp.hourglassCost = 0;

            return ActionErrorCode.AEC_OK;
        }

        public static bool ActionFrameSunder(D20Action action)
        {
            if (GameSystems.Combat.SunderCheck(action.d20APerformer, action.d20ATarget))
            {
                GameObjectBody weapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponPrimary);
                if (weapon == null)
                    weapon = GameSystems.Item.ItemWornAt(action.d20ATarget, EquipSlot.WeaponSecondary);
                if (weapon != null)
                    GameSystems.Item.ItemDrop(weapon);
                //GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, 198);
            }


            return false;
        }

        [TempleDllLocation(0x10090730)]
        public static bool TouchAttackActionFrame(D20Action action)
        {
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId1);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId2);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId0);

            if ((action.d20Caf & D20CAF.RANGED) != default)
                return false;


            var curSeq = GameSystems.D20.Actions.CurrentSequence;
            if ((action.d20Caf & D20CAF.HIT) == default) // touch attack charges should remain until discharged
            {
                curSeq.tbStatus.tbsFlags &= ~TurnBasedStatusFlags.TouchAttack;
                action.d20Caf &= ~D20CAF.FREE_ACTION;
                return false;
            }


            GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_TouchAttack, action);

            if (curSeq != null)
            {
                curSeq.tbStatus.tbsFlags &= ~TurnBasedStatusFlags.TouchAttack;
                action.d20Caf &= ~D20CAF.FREE_ACTION;
            }

            return false;
        }

        [TempleDllLocation(0x10095b00)]
        public static bool TripActionFrame(D20Action action)
        {
            if (action.d20ATarget == null)
                return false;

            var tgt = action.d20ATarget;
            var performer = action.d20APerformer;

            if ((action.d20Caf & D20CAF.HIT) == default)
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, 29); //miss
                return false;
            }

            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId1);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId2);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId0);

            if (GameSystems.Combat.TripCheck(action.d20APerformer, tgt))
            {
                action.d20ATarget.AddCondition("Prone");
                GameSystems.Anim.PushAnimate(tgt, NormalAnimType.Falldown);
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(44, performer, tgt);
                GameSystems.D20.Combat.FloatCombatLine(tgt, 104);

                if (GameSystems.Feat.HasFeatCountByClass(performer, FeatId.IMPROVED_TRIP) != 0)
                {
                    InsertD20Action(performer, D20ActionType.STANDARD_ATTACK, action.data1, action.d20ATarget,
                        action.destLoc, 0);
                    var curSeq = GameSystems.D20.Actions.CurrentSequence;
                    curSeq.d20ActArray[curSeq.d20aCurIdx + 1].d20Caf |= D20CAF.FREE_ACTION;
                    return false;
                }
            }
            else // counter attempt
            {
                if (GameSystems.Combat.TripCheck(tgt, performer))
                {
                    performer.AddCondition("Prone");
                    GameSystems.Anim.PushAnimate(performer, NormalAnimType.Falldown);
                    GameSystems.D20.Combat.FloatCombatLine(performer, 104);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(44, tgt, performer);
                    return false;
                }

                GameSystems.D20.Combat.FloatCombatLine(performer, 103);
            }

            return false;
        }

        /// <summary>
        /// Insert an action after the current action.
        /// </summary>
        [TempleDllLocation(0x10094b40)]
        private static void InsertD20Action(GameObjectBody performer, D20ActionType type, int data1, GameObjectBody tgt,
            LocAndOffsets loc, int radialMenuArg)
        {
            var currentAction = GameSystems.D20.Actions.CurrentSequence;

            var action = new D20Action(type, performer);
            action.data1 = data1;
            action.d20ATarget = tgt;
            action.destLoc = loc;
            action.radialMenuActualArg = radialMenuArg;

            currentAction.d20ActArray.Insert(currentAction.d20aCurIdx + 1, action);
        }

        [TempleDllLocation(0x1008e510)]
        public static bool CastSpellProjectileHit(D20Action action, GameObjectBody projectile, GameObjectBody obj2)
        {
            var projectileIdx = -1;
            var spellEnum = action.d20SpellData.SpellEnum;

            var spEntry = GameSystems.Spell.GetSpellEntry(spellEnum);
            ;
            if (spEntry.projectileFlag == 0)
                return false;

            SpellPacketBody pkt = GameSystems.Spell.GetActiveSpell(action.spellId);
            if (pkt.spellEnum == 0)
            {
                Logger.Debug("ProjectileHitSpell: Unable to retrieve spell packet!");
                return false;
            }

            GameSystems.Script.Spells.SpellSoundPlay(pkt, SpellEvent.SpellStruck);
            if (pkt.projectiles.Length > 1)
                action.d20Caf |= D20CAF.NEED_PROJECTILE_HIT;

            // get the projectileIdx
            for (var i = 0; i < pkt.projectiles.Length; i++)
            {
                if (pkt.projectiles[i] == projectile)
                {
                    projectileIdx = i;
                    break;
                }
            }

            if (projectileIdx < 0)
            {
                Logger.Error("ProjectileHitSpell: Projectile not found!");
                return false;
            }

            GameSystems.Script.Spells.SpellTriggerProjectile(action.spellId, SpellEvent.EndProjectile, projectile,
                projectileIdx);

            pkt = GameSystems.Spell
                .GetActiveSpell(action.spellId); // update spell if altered by the above (probably not needed anymore)

            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_HoldingCharge) != 0
                && ((action.d20Caf & D20CAF.RANGED) != default))
            {
                pkt.targetListHandles[0] = pkt.caster;
            }

            GameSystems.Spell.UpdateSpellPacket(pkt);
            GameSystems.Script.Spells.UpdateSpell(pkt.spellId);

            return true;
        }

        public static bool ProjectileHitPython(D20Action action, GameObjectBody projectile, GameObjectBody obj2)
        {
            return GameSystems.Script.Actions.PyProjectileHit((D20DispatcherKey) action.data1, action, projectile,
                obj2);
        }

        public static ActionErrorCode PerformAidAnotherWakeUp(D20Action action)
        {
            if (GameSystems.Anim.PushAttemptAttack(action.d20APerformer, action.d20ATarget))
            {
                GameSystems.Anim.PushUseSkillOn(action.d20APerformer, action.d20ATarget, SkillId.heal);
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;

                GameUiBridge.ShowTextBubble(action.d20APerformer, "Wake up!");
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100902b0)]
        public static ActionErrorCode AttackOfOpportunityPerform(D20Action action)
        {
            if (action.d20APerformer == null)
                return ActionErrorCode.AEC_INVALID_ACTION;

            var performer = action.d20APerformer;

            if (action.d20ATarget == null)
                return ActionErrorCode.AEC_TARGET_INVALID;

            var tgt = action.d20ATarget;

            GameSystems.D20.Combat.FloatCombatLine(performer, 43); // attack of opportunity
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(1, performer, tgt);

            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Trip_AOO) != 0 &&
                GameSystems.D20.D20Query(action.d20ATarget, D20DispatcherKey.QUE_Prone) == 0)
            {
                return TripPerform(action);
            }

            // else do standard attack
            return StandardAttackPerform(action);
        }

        [TempleDllLocation(0x10091220)]
        public static ActionErrorCode TripPerform(D20Action action)
        {
            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            action.d20Caf |= D20CAF.TOUCH_ATTACK;
            GameSystems.D20.Combat.ToHitProcessing(action);
            if (GameSystems.Anim.PushAttemptAttack(action.d20APerformer, action.d20ATarget))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode PerformCastItemSpell(D20Action action)
        {
            var spellEnum = action.d20SpellData.SpellEnum;
            var invIdx = action.d20SpellData.itemSpellData;
            var spellClass = action.d20SpellData.spellClassCode;
            var spellLvl = action.d20SpellData.spellSlotLevel;
            if (invIdx == D20ActionSystem.INV_IDX_INVALID)
                return ActionErrorCode.AEC_OK;

            var item = GameSystems.Item.GetItemAtInvIdx(action.d20APerformer, invIdx);
            if (item == null)
                return ActionErrorCode.AEC_OK;
            var itemFlags = (ItemFlag) item.GetInt32(obj_f.item_flags);

            if (action.d20ActType == D20ActionType.ACTIVATE_DEVICE_SPELL || item == null)
                return ActionErrorCode.AEC_OK;

            var caster = action.d20APerformer;

            var useMagicDeviceBase = GameSystems.Skill.GetSkillRanks(action.d20APerformer, SkillId.use_magic_device);
            int resultDeltaFromDc;
            if (!GameSystems.Item.IsIdentified(item) && item.type != ObjectType.food)
            {
                // blind use of magic item
                if (item.type == ObjectType.scroll || useMagicDeviceBase == 0)
                {
                    return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
                }

                var umdRoll = GameSystems.Skill.SkillRoll(action.d20APerformer, SkillId.use_magic_device, 25,
                    out resultDeltaFromDc, 1);
                if (!umdRoll)
                {
                    GameSystems.Skill.ShowSkillMessage(action.d20APerformer,
                        SkillMessageId.UseMagicDeviceActivateBlindlyFailed); // casting mishap
                    if (resultDeltaFromDc >= 0)
                    {
                        GameSystems.Skill.ShowSkillMessage(action.d20APerformer, SkillMessageId.UseMagicDeviceMishap);
                        GameSystems.Item.ItemSpellChargeConsume(item);
                        return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
                    }
                }
            }


            // check if item requires knowing the spell
            if (item.type == ObjectType.scroll || itemFlags.HasFlag(ItemFlag.NEEDS_SPELL) &&
                (item.type == ObjectType.generic || item.type == ObjectType.weapon))
            {
                var isOk = false;

                if (GameSystems.Spell.HashMatchingClassForSpell(action.d20APerformer, spellEnum))
                {
                    isOk = true;
                }
                else if (GameSystems.Spell.IsArcaneSpellClass(spellClass))
                {
                    var clrLvl = caster.GetStat(Stat.level_cleric);
                    if (clrLvl > 0 && GameSystems.Critter.HasDomain(caster, DomainId.Magic)
                                   && spellLvl <= Math.Max(1, clrLvl / 2))
                        isOk = true;
                }

                if (!isOk)
                {
                    // do Use Magic Device roll
                    if (useMagicDeviceBase == 0)
                        return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;

                    if (item.type == ObjectType.scroll)
                    {
                        var umdRoll = GameSystems.Skill.SkillRoll(action.d20APerformer, SkillId.use_magic_device, 20,
                            out resultDeltaFromDc, 1);
                        if (!umdRoll)
                        {
                            GameSystems.Skill.ShowSkillMessage(action.d20APerformer,
                                SkillMessageId.UseMagicDeviceUseScrollFailed); // Use Scroll Failed
                            GameSystems.D20.Actions.CurrentSequence?.spellPktBody.Reset();
                            return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
                        }
                    }
                    else
                    {
                        var umdRoll = GameSystems.Skill.SkillRoll(action.d20APerformer, SkillId.use_magic_device, 20,
                            out resultDeltaFromDc, 1);
                        if (!umdRoll)
                        {
                            GameSystems.Skill.ShowSkillMessage(action.d20APerformer,
                                SkillMessageId.UseMagicDeviceUseWandFailed); // Use Wand Failed
                            return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
                        }
                    }
                }
            }


            if (item.type == ObjectType.scroll)
            {
                if (!GameSystems.Spell.CheckAbilityScoreReqForSpell(action.d20APerformer, spellEnum))
                {
                    if (useMagicDeviceBase == 0)
                    {
                        GameSystems.D20.Actions.CurrentSequence?.spellPktBody.Reset();
                        return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
                    }

                    var umdRoll = GameSystems.Skill.SkillRoll(action.d20APerformer, SkillId.use_magic_device, 20,
                        out resultDeltaFromDc, 1);
                    if (!umdRoll)
                    {
                        GameSystems.Skill.ShowSkillMessage(action.d20APerformer,
                            SkillMessageId.UseMagicDeviceEmulateAbilityScoreFailed); // Emulate Ability Score Failed
                        return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
                    }
                }
            }

            WeaponFlag weapFlags = default;
            if (item.type == ObjectType.weapon)
            {
                weapFlags = item.WeaponFlags;
            }

            var chargesUsedUp = 1;
            if (weapFlags.HasFlag(WeaponFlag.MAGIC_STAFF) && spellEnum == 379)
            {
                // raise dead
                chargesUsedUp = 5;
            }

            GameSystems.Item.ItemSpellChargeConsume(item, chargesUsedUp);

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10094350)]
        public static ActionErrorCode CastSpellPerform(D20Action action)
        {
            var curSeq = GameSystems.D20.Actions.CurrentSequence;
            ref var spellPkt = ref curSeq.spellPktBody;

            //Get the metamagic data
            action.d20SpellData.metaMagicData =
                GameSystems.D20.Actions.globD20Action.d20APerformer.DispatchMetaMagicModify(action.d20SpellData
                    .metaMagicData);

            // Make sure the spell packet has the correct meta magic data (it will not if metamagic data has been modified)
            spellPkt.metaMagicData = action.d20SpellData.metaMagicData;

            // Now the deduct charge signal should be sent since the spell can no longer be aborted (but it can fail)
            GameSystems.D20.D20SignalPython(action.d20APerformer, "Sudden Metamagic Deduct Charge");

            var spellEnum = action.d20SpellData.SpellEnum;
            var spellClass = action.d20SpellData.spellClassCode;
            var spellLvl = action.d20SpellData.spellSlotLevel;
            var invIdx = action.d20SpellData.itemSpellData;
            var mmData = action.d20SpellData.metaMagicData;
            SpellStoreData spellData = new SpellStoreData(spellEnum, spellLvl, spellClass, mmData);

            GameObjectBody item = null;

            // if it's an item spell
            if (invIdx != D20ActionSystem.INV_IDX_INVALID)
            {
                spellPkt.invIdx = invIdx;
                spellPkt.spellEnumOriginal = spellEnum;
                item = GameSystems.Item.GetItemAtInvIdx(spellPkt.caster, invIdx);
            }

            var spellEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);

            // spell interruption
            void spellInterruptApply(int spellSchool, GameObjectBody caster, int invIdx)
            {
                caster.AddCondition("Spell Interrupted", 0, 0, 0);
                GameObjectBody item = null;
                if (invIdx != D20ActionSystem.INV_IDX_INVALID)
                {
                    item = GameSystems.Item.GetItemAtInvIdx(caster, invIdx);
                }

                GameSystems.Anim.PushSpellInterrupt(caster, item, AnimGoalType.attempt_spell_w_cast_anim, spellSchool);
            }

            if (SpellIsInterruptedCheck(action, invIdx, in spellData))
            {
                if (invIdx == D20ActionSystem.INV_IDX_INVALID)
                {
                    spellPkt.Debit();
                }

                spellInterruptApply(spellEntry.spellSchoolEnum, spellPkt.caster, invIdx);
                if (curSeq != null)
                    curSeq.spellPktBody.Reset();
                return ActionErrorCode.AEC_OK;
            }

            var result = PerformCastItemSpell(action);

            if (result != ActionErrorCode.AEC_OK)
            {
                if (curSeq != null)
                    curSeq.spellPktBody.Reset();
                return result;
            }


            // acquire D20Action target from the spell packet if none is present
            if (action.d20ATarget == null && spellEntry.projectileFlag == 0)
            {
                if (spellPkt.targetCount > 0)
                {
                    action.d20ATarget = spellPkt.targetListHandles[0];
                }
            }

            // charge GP spell component
            if (spellPkt.invIdx == D20ActionSystem.INV_IDX_INVALID && spellEntry.costGp > 0)
            {
                if (GameSystems.Party.IsInParty(spellPkt.caster))
                {
                    GameSystems.Party.RemovePartyMoney(0, spellEntry.costGp, 0, 0);
                }
            }


            if (spellPkt.targetCount > 0)
            {
                var filterResult = action.FilterSpellTargets(spellPkt);

                if (!filterResult
                    && !spellEntry.IsBaseModeTarget(UiPickerType.Area)
                    && !spellEntry.IsBaseModeTarget(UiPickerType.Cone)
                    && !spellEntry.IsBaseModeTarget(UiPickerType.Location))
                {
                    spellPkt.Debit();
                    spellInterruptApply(spellEntry.spellSchoolEnum, spellPkt.caster,
                        invIdx); // note: perhaps the current sequence changes due to the applied interrupt
                    GameSystems.Spell.FloatSpellLine(spellPkt.caster, 30000, TextFloaterColor.Red); // Spell has fizzled
                    if (!GameSystems.Party.IsInParty(curSeq.spellPktBody.caster))
                    {
                        var leader = GameSystems.Party.GetConsciousLeader();
                        var targetRot = curSeq.spellPktBody.caster.RotationTo(leader);
                        GameSystems.Anim.PushRotate(curSeq.spellPktBody.caster, targetRot);
                    }

                    curSeq.spellPktBody.Reset();
                    return ActionErrorCode.AEC_OK;
                }
            }

            var spellId = GameSystems.Spell.GetNewSpellId();
            GameSystems.Spell.RegisterSpell(spellPkt, spellId);

            if (GameSystems.Anim.PushSpellCast(spellPkt, item))
            {
                spellPkt.Debit();
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
            }

            // provoke hostility
            for (var i = 0u; i < curSeq.spellPktBody.targetCount; i++)
            {
                var tgt = curSeq.spellPktBody.targetListHandles[i];
                if (tgt == null)
                    continue;
                var tgtObj = tgt;
                if (!tgtObj.IsCritter())
                    continue;
                if (GameSystems.Spell.IsSpellHarmful(curSeq.spellPktBody.spellEnum, curSeq.spellPktBody.caster, tgt))
                {
                    GameSystems.AI.ProvokeHostility(curSeq.spellPktBody.caster, tgt, 1, 0);
                }
            }

            action.spellId = curSeq.d20Action.spellId = curSeq.spellPktBody.spellId;
            GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_Spell_Cast, spellId, 0);

            for (var i = 0u; i < curSeq.spellPktBody.targetCount; i++)
            {
                var tgt = curSeq.spellPktBody.targetListHandles[i];
                if (tgt != null)
                {
                    GameSystems.D20.D20SendSignal(tgt, D20DispatcherKey.SIG_Spell_Cast, spellId, 0);
                }
            }

            GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_Remove_Concentration, 0, 0);

            if (curSeq != null)
                curSeq.spellPktBody.Reset();

            return ActionErrorCode.AEC_OK;
        }

        private static bool SpellIsInterruptedCheck(D20Action d20a, int invIdx, in SpellStoreData spellData)
        {
            if (GameSystems.Spell.IsSpellLike(spellData.spellEnum)
                //|| (invIdx != D20ActionSystem.INV_IDX_INVALID) // removed to support miscasting when Casting Defensively with Scrolls
                || GameSystems.D20.D20Query(d20a.d20APerformer,
                    D20DispatcherKey.QUE_Critter_Is_Spell_An_Ability, spellData.spellEnum) != 0)
                return false;

            if (invIdx != D20ActionSystem.INV_IDX_INVALID)
            {
                if (d20a.d20ActType != D20ActionType.USE_ITEM)
                    return false;
                var item = GameSystems.Item.GetItemAtInvIdx(d20a.d20APerformer, invIdx);
                if (item == null)
                    return false;
                if (item.type != ObjectType.scroll)
                {
                    if (item.type == ObjectType.generic)
                    {
                        var itemFlags = item.GetItemFlags();
                        if (!itemFlags.HasFlag(ItemFlag.NEEDS_SPELL))
                        {
                            return false;
                        }
                    }
                    else
                        return false;
                }
            }

            if (d20a.d20Caf.HasFlag(D20CAF.COUNTERSPELLED))
                return true;
            return GameSystems.D20.D20QueryWithObject(d20a.d20APerformer,
                       D20DispatcherKey.QUE_SpellInterrupted, d20a.d20SpellData) != 0;
        }

        public static bool ActionFrameAidAnotherWakeUp(D20Action action)
        {
            // GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, 204); // woken up // not necessary - already gets applied with the removal of the sleep condition I think
            GameSystems.D20.D20SendSignal(action.d20ATarget, D20DispatcherKey.SIG_AID_ANOTHER_WAKE_UP, action);


            return true;
        }

        public static bool AttackOfOpportunityActionFrame(D20Action action)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Trip_AOO) != 0 &&
                GameSystems.D20.D20Query(action.d20ATarget, D20DispatcherKey.QUE_Prone) == 0)
            {
                return TripActionFrame(action);
            }

            return StandardAttackActionFrame(action);
        }

        public static ActionErrorCode ActionCheckAidAnotherWakeUp(D20Action action, TurnBasedStatus tbStatus)
        {
            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10093cb0)]
        public static ActionErrorCode CastSpellActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            var spEnum = action.d20SpellData.SpellEnum;
            var spellEnumOrg = action.d20SpellData.spellEnumOrg;
            var spellClass = action.d20SpellData.spellClassCode;
            var spellLvl = action.d20SpellData.spellSlotLevel;
            var invIdx = action.d20SpellData.itemSpellData;
            if (!GameSystems.Spell.IsValidSpell(spEnum))
                return ActionErrorCode.AEC_INVALID_ACTION;

            var spEntry = GameSystems.Spell.GetSpellEntry(spEnum);

            void actSeqSpellResetter()
            {
                GameSystems.D20.Actions.CurrentSequence?.spellPktBody.Reset();
            }

            // check casting time
            if (spEntry.castingTimeType == 2 && GameSystems.Combat.IsCombatActive())
            {
                actSeqSpellResetter();
                return ActionErrorCode.AEC_OUT_OF_COMBAT_ONLY;
            }

            // if not an item spell
            if (invIdx == D20ActionSystem.INV_IDX_INVALID)
            {
                tbStatus.surplusMoveDistance = 0;

                // check CannotCast
                if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_CannotCast) != 0)
                {
                    actSeqSpellResetter();
                    return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
                }

                if (!GameSystems.Spell.spellCanCast(action.d20APerformer, spellEnumOrg, spellClass, spellLvl))
                    return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;


                // check Spell/Sorc slots
                if (!GameSystems.Spell.IsDomainSpell(spellClass))
                {
                    var classCode = GameSystems.Spell.GetCastingClass(spellClass);
                    if (D20ClassSystem.IsNaturalCastingClass(classCode))
                        while (true)
                        {
                            var spellsPerDay =
                                GameSystems.Spell.GetNumSpellsPerDay(action.d20APerformer, classCode, spellLvl);
                            var spellsCastNum = GameSystems.Spell.NumSpellsInLevel(action.d20APerformer,
                                obj_f.critter_spells_cast_idx, spellClass, spellLvl);

                            if (spellsCastNum < spellsPerDay)
                            {
                                break;
                            }

                            action.d20SpellData.spellSlotLevel += 1;
                            if (++spellLvl >= SpellSystem.NUM_SPELL_LEVELS)
                            {
                                actSeqSpellResetter();
                                return ActionErrorCode.AEC_CANNOT_CAST_OUT_OF_AVAILABLE_SPELLS;
                            }
                        }
                }

                // check GP requirement
                if (spEntry.costGp > 0u && GameSystems.Party.IsInParty(action.d20APerformer)
                                        && GameSystems.Party.GetPartyMoney() >= 0
                                        && (uint) GameSystems.Party.GetPartyMoney() < spEntry.costGp * 100)
                {
                    // making sure that costGp is interpreted as unsigned in case of some crazy overflow scenario
                    actSeqSpellResetter();
                    return ActionErrorCode.AEC_CANNOT_CAST_NOT_ENOUGH_GP;
                }
            }

            if (GameSystems.D20.D20QueryWithObject(action.d20APerformer,
                    D20DispatcherKey.QUE_IsActionInvalid_CheckAction, action) != 0)
            {
                actSeqSpellResetter();
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            return GameSystems.D20.Actions.TargetCheck(action)
                ? ActionErrorCode.AEC_OK
                : ActionErrorCode.AEC_TARGET_INVALID;
        }

        [TempleDllLocation(0x100959b0)]
        public static ActionErrorCode ChargeAddToSeq(D20Action action, ActionSequence actSeq, TurnBasedStatus tbStatus)
        {
            var tgt = action.d20ATarget;
            if (tgt == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            var performer = action.d20APerformer;
            if ((tbStatus.tbsFlags & (TurnBasedStatusFlags.Moved | TurnBasedStatusFlags.UNK_1)) != default)
                return ActionErrorCode.AEC_ALREADY_MOVED;

            D20Action d20aCopy = action.Copy();
            TurnBasedStatus tbStatCopy = tbStatus.Copy();

            var weapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
            if (weapon == null)
            {
                weapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);
                if (weapon != null)
                {
                    d20aCopy.d20Caf |= D20CAF.SECONDARY_WEAPON;
                    tbStatCopy.attackModeCode = AttackPacket.ATTACK_CODE_OFFHAND;
                    d20aCopy.data1 = AttackPacket.ATTACK_CODE_OFFHAND + 1;
                }
                else if (GameSystems.D20.Actions.DispatchD20ActionCheck(d20aCopy, tbStatCopy,
                             DispatcherType.GetCritterNaturalAttacksNum) != 0)
                {
                    tbStatCopy.attackModeCode = AttackPacket.ATTACK_CODE_NATURAL_ATTACK;
                    d20aCopy.data1 = AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1;
                }
                else
                {
                    tbStatCopy.attackModeCode = AttackPacket.ATTACK_CODE_PRIMARY;
                    d20aCopy.data1 = AttackPacket.ATTACK_CODE_PRIMARY + 1;
                }
            }

            return ActionErrorCode.AEC_OK; //TODO complete the rest
        }

        public static ActionErrorCode AddToSeqPython(D20Action action, ActionSequence actSeq, TurnBasedStatus tbStatus)
        {
            return GameSystems.Script.Actions.PyAddToSeq((D20DispatcherKey) action.data1, action, actSeq, tbStatus);
        }

        [TempleDllLocation(0x1008bfa0)]
        public static ActionErrorCode ActionSequencesAddSimple(D20Action action, ActionSequence actSeq, TurnBasedStatus tbStatus)
        {
            actSeq.d20ActArray.Add(action);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100958a0)]
        public static ActionErrorCode ActionSequencesAddWithSpell(D20Action action, ActionSequence seq, TurnBasedStatus tbStatus)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Prone) != 0)
            {
                D20Action d20aGetup = action.Copy();
                d20aGetup.d20ActType = D20ActionType.STAND_UP;
                seq.d20ActArray.Add(d20aGetup);
            }

            var spellEnum = action.d20SpellData.SpellEnum;

            if (GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spellEntry)
                && spellEntry.spellRangeType == SpellRangeType.SRT_Touch
                && spellEntry.modeTargetSemiBitmask == UiPickerType.Single
                && !seq.ignoreLos
            )
            {
                return GameSystems.D20.Actions.AddToSeqWithTarget(action, seq, tbStatus);
            }

            seq.d20ActArray.Add(action.Copy());
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100955e0)]
        public static ActionErrorCode StandardAttackAddToSeq(D20Action action, ActionSequence actSeq,
            TurnBasedStatus tbStatus)
        {
            var tgt = action.d20ATarget;

            if (tgt == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            var performer = action.d20APerformer;

            var d20aCopy = action.Copy();
            var tbStatCopy = tbStatus.Copy();

            var weapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);

            // ranged weapon
            if (GameSystems.Weapon.IsRangedWeapon(weapon.GetWeaponType()))
            {
                ActionCostPacket acp = new ActionCostPacket();
                d20aCopy.d20Caf |= D20CAF.RANGED;
                if (GameSystems.Item.IsNormalCrossbow(weapon))
                {
                    D20ActionVanillaCallbacks.ReloadActionCost(action, tbStatCopy, acp);
                    if (acp.hourglassCost != 0)
                    {
                        action.d20ActType = D20ActionType.STANDARD_RANGED_ATTACK;
                        PerformReloadAndThenAction(d20aCopy, actSeq);
                        return ActionErrorCode.AEC_OK;
                    }
                }

                if (GameSystems.D20.Actions.TurnBasedStatusUpdate(d20aCopy, tbStatCopy) == ActionErrorCode.AEC_OK)
                {
                    if (GameSystems.Item.IsThrowingWeapon(weapon))
                    {
                        d20aCopy.d20ActType = D20ActionType.THROW;
                        d20aCopy.d20Caf |= D20CAF.THROWN;
                    }
                    else
                    {
                        d20aCopy.d20ActType = D20ActionType.STANDARD_RANGED_ATTACK;
                    }
                }

                AttackAppend(actSeq, d20aCopy, tbStatCopy, tbStatCopy.attackModeCode);
                return ActionErrorCode.AEC_OK;
            }


            var reach = performer.GetReach(action.d20ActType);
            if (performer.DistanceToObjInFeet(tgt) > reach)
            {
                d20aCopy = action.Copy();
                d20aCopy.d20ActType = D20ActionType.UNSPECIFIED_MOVE;
                GameSystems.D20.Actions.MoveSequenceParse(d20aCopy, actSeq, tbStatus, 0.0f, reach, true);
            }

            if (GameSystems.D20.Actions.TurnBasedStatusUpdate(d20aCopy, tbStatCopy) != ActionErrorCode.AEC_OK)
            {
                // bug??
                return ActionErrorCode.AEC_OK;
            }

            AttackAppend(actSeq, d20aCopy, tbStatCopy, tbStatCopy.attackModeCode);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008E8A0)]
        private static void PerformReloadAndThenAction(D20Action action, ActionSequence actSeq)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Prone) != 0)
            {
                var standUpAction = action.Copy();
                standUpAction.d20ActType = D20ActionType.STAND_UP;
                actSeq.d20ActArray.Add(standUpAction);
            }

            if (GameSystems.Item.IsWieldingUnloadedCrossbow(action.d20APerformer))
            {
                var reloadAction = action.Copy();
                reloadAction.d20ActType = D20ActionType.RELOAD;
                actSeq.d20ActArray.Add(reloadAction);
            }

            actSeq.d20ActArray.Add(action);
            action.data1 = 1;
        }

        [TempleDllLocation(0x100968b0)]
        public static ActionErrorCode UnspecifiedAttackAddToSeq(D20Action d20a, ActionSequence actSeq, TurnBasedStatus tbStat) {
	        GameObjectBody tgt = d20a.d20ATarget;
	        GameObjectBody performer = d20a.d20APerformer;
            var d20aCopy = d20a.Copy();
	        int d20aNumInitial = actSeq.d20ActArrayNum;

	        // denote performance of Default Action (is checked in ShouldAutoEndTurn)
	        GameSystems.D20.Actions.performingDefaultAction = true;

	        if (tgt == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            // if holding a spell charge do a touch attack
	        if (GameSystems.D20.D20Query(d20a.d20APerformer, D20DispatcherKey.QUE_HoldingCharge) != 0)
            {
                return D20ActionVanillaCallbacks.TouchAttackAddToSeq(d20a, actSeq, tbStat);
            }

            var weapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
	        if (weapon == null)
            {
                // vanilla didn't have this, I think it may have screwed the AI in some cases
                // (the dagger-switching phenomenon in the moathouse)
                weapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);
            }

            var tbStatCopy = tbStat.Copy();

	        // ranged attack handling
	        if (weapon != null && GameSystems.Item.IsRangedWeapon(weapon)) {
		        d20aCopy.d20Caf |= D20CAF.RANGED;
		        if (GameSystems.Item.IsNormalCrossbow(weapon))	{

                    ActionCostPacket acp = new ActionCostPacket();
			        D20ActionVanillaCallbacks.ReloadActionCost(d20a, tbStatCopy, acp);
			        if (acp.hourglassCost != 0)
			        {
				        d20aCopy.d20ActType = D20ActionType.STANDARD_RANGED_ATTACK;
				        PerformReloadAndThenAction(d20aCopy, actSeq);
                        return ActionErrorCode.AEC_OK;
                    }
		        }

		        GameSystems.D20.Actions.FullAttackCostCalculate(d20aCopy, tbStatCopy, out _, out _, out var numRangedAttacks, out _);
		        d20aCopy.d20ActType = D20ActionType.FULL_ATTACK;
		        if (numRangedAttacks > 1 && GameSystems.D20.Actions.TurnBasedStatusUpdate(d20aCopy, tbStatCopy) == ActionErrorCode.AEC_OK)
		        {
			        actSeq.d20ActArray.Add(d20aCopy);

                    var nextAttack = d20aCopy.Copy();
                    nextAttack.d20ActType = GameSystems.Item.IsThrowingWeapon(weapon) ? D20ActionType.THROW : D20ActionType.STANDARD_RANGED_ATTACK;
			        return UnspecifiedAttackAddToSeqRangedMulti(actSeq, nextAttack, tbStatCopy);
		        }

		        d20aCopy = d20a.Copy();
		        d20aCopy.d20ActType = GameSystems.Item.IsThrowingWeapon(weapon) ? D20ActionType.THROW : D20ActionType.STANDARD_RANGED_ATTACK;
		        var result = GameSystems.D20.Actions.TurnBasedStatusUpdate(d20aCopy, tbStatCopy);
		        if (result == ActionErrorCode.AEC_OK)
		        {
			        int attackCode = tbStatCopy.attackModeCode;
			        AttackAppend(actSeq, d20aCopy, tbStat, attackCode);
		        }
		        return result;
	        }

	        // if out of reach, add move sequence
	        var reach = d20a.d20APerformer.GetReach(d20a.d20ActType);
            d20aCopy.destLoc = tgt.GetLocationFull();
	        if (performer.DistanceToObjInFeet(tgt) > reach){
		        d20aCopy.d20ActType = D20ActionType.UNSPECIFIED_MOVE;
		        var moveResult = GameSystems.D20.Actions.MoveSequenceParse(d20aCopy, actSeq, tbStat, 0.0f, reach, true);
		        if (moveResult != ActionErrorCode.AEC_OK)
			        return moveResult;
		        tbStatCopy = tbStat.Copy();
	        }

	        // run the check function for all the newly added actions (if there are any)
	        for (int i = d20aNumInitial; i < actSeq.d20ActArrayNum; i++){
		        var checkResult = GameSystems.D20.Actions.seqCheckAction(actSeq.d20ActArray[i], tbStatCopy);
		        if (checkResult != ActionErrorCode.AEC_OK)
                {
                    return checkResult;
                }
            }

	        // add full attack if possible
	        d20aCopy = d20a.Copy();
            GameSystems.D20.Actions.FullAttackCostCalculate(d20aCopy, tbStatCopy, out _, out _, out var numAttacks, out _);
	        if (numAttacks > 1)	{
		        d20aCopy.d20ActType = D20ActionType.FULL_ATTACK;
		        if (GameSystems.D20.Actions.TurnBasedStatusUpdate(d20aCopy, tbStatCopy) == ActionErrorCode.AEC_OK)
                {
                    actSeq.d20ActArray.Add(d20aCopy.Copy());
			        d20aCopy.d20ActType = D20ActionType.STANDARD_ATTACK;
			        return UnspecifiedAttackAddToSeqMeleeMulti(actSeq, tbStatCopy, d20aCopy);
		        }
	        }

	        // add single attack otherwise (if possible)
	        d20aCopy = d20a.Copy();
	        d20aCopy.d20ActType = D20ActionType.STANDARD_ATTACK;

            var tbResult = GameSystems.D20.Actions.TurnBasedStatusUpdate(d20aCopy, tbStatCopy);
	        if (tbResult == ActionErrorCode.AEC_OK)
	        {
		        int attackCode = tbStatCopy.attackModeCode;
		        AttackAppend(actSeq, d20aCopy, tbStat, attackCode);
	        }
	        return tbResult;

	        // todo: add support for charge attack if shift key is pressed
        }

        [TempleDllLocation(0x1008e970)]
        private static ActionErrorCode UnspecifiedAttackAddToSeqRangedMulti(ActionSequence actSeq, D20Action d20a, TurnBasedStatus tbStat)
        {
	        int baseAttackNumCode = tbStat.baseAttackNumCode;
	        int numBonusAttacks = tbStat.numBonusAttacks;
	        int attackModeCode = tbStat.attackModeCode;
	        d20a.d20Caf |= D20CAF.RANGED;

	        int bonusAttackNumCode = attackModeCode + numBonusAttacks;
	        int attackCode = attackModeCode + 1;
	        var weapon = GameSystems.Item.ItemWornAt(d20a.d20APerformer, EquipSlot.WeaponPrimary);
	        if (weapon != null)
            {
                var ammoType = weapon.GetWeaponAmmoType();
		        if (ammoType.IsThrown()) // thrown weapons   TODO: should this include daggers??
		        {
			        d20a.d20Caf |= D20CAF.THROWN;
			        if (ammoType != WeaponAmmoType.shuriken && GameSystems.Feat.HasFeatCount(d20a.d20APerformer, FeatId.QUICK_DRAW) == 0)
			        {
				        baseAttackNumCode = attackModeCode + 1;
				        bonusAttackNumCode = attackModeCode;
			        }
		        }
	        }

	        if (GameSystems.D20.D20Query(d20a.d20APerformer, D20DispatcherKey.QUE_Prone) != 0)
            {
                var standUpAction = d20a.Copy();
                standUpAction.d20ActType = D20ActionType.STAND_UP;
                actSeq.d20ActArray.Add(standUpAction);
            }

	        for (int i = bonusAttackNumCode - attackModeCode; i > 0; i--)
	        {
		        AttackAppend(actSeq, d20a, tbStat, attackCode);
	        }

	        for (int i = baseAttackNumCode - attackModeCode; i > 0; i--)
	        {
		        AttackAppend(actSeq, d20a, tbStat, attackCode);
		        attackCode++;
	        }
	        return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c340)]
        private static ActionErrorCode UnspecifiedAttackAddToSeqMeleeMulti(ActionSequence actSeq, TurnBasedStatus tbStat, D20Action d20a)
        {
	        int  attackModeCode = tbStat.attackModeCode;
	        int  baseAttackNumCode = tbStat.baseAttackNumCode;
	        int  attackCode = attackModeCode + 1;

	        for (int i = tbStat.numBonusAttacks; i > 0; i--){
		        AttackAppend(actSeq, d20a, tbStat, attackCode);
	        }

	        for (int i = baseAttackNumCode - attackModeCode; i > 0; i--){

		        AttackAppend(actSeq, d20a, tbStat, attackCode);
		        attackCode++;
	        }
	        return ActionErrorCode.AEC_OK;
        }


        public static ActionErrorCode AddToSeqWithTarget(D20Action action, ActionSequence actSeq,
            TurnBasedStatus tbStatus)
        {
            return GameSystems.D20.Actions.AddToSeqWithTarget(action, actSeq, tbStatus);
        }

        [TempleDllLocation(0x100904e0)]
        public static ActionErrorCode WhirlwindAttackAddToSeq(D20Action action, ActionSequence actSeq,
            TurnBasedStatus tbStatus)
        {
            var performer = action.d20APerformer;
            if (GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_Prone) != 0)
            {
                var standUp = action.Copy();
                standUp.d20ActType = D20ActionType.STAND_UP;
                actSeq.d20ActArray.Add(standUp);
            }

            actSeq.d20ActArray.Add(action.Copy());

            var reach = performer.GetReach();
            var perfSizeFeet = performer.GetRadius() / locXY.INCH_PER_FEET;

            var searchRadius = 1.0f + perfSizeFeet + reach;
            foreach (var enemy in GameSystems.D20.Combat.EnumerateEnemiesInRange(performer, searchRadius))
            {
                if (performer.DistanceToObjInFeet(enemy) <= reach)
                {
                    var stdAttack = action.Copy();
                    stdAttack.d20ActType = D20ActionType.STANDARD_ATTACK;
                    stdAttack.data1 = AttackPacket.ATTACK_CODE_PRIMARY + 1;
                    stdAttack.d20ATarget = enemy;
                    stdAttack.d20Caf |= D20CAF.FREE_ACTION;
                    actSeq.d20ActArray.Add(stdAttack);
                }
            }

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode ActionSequencesAddTrip(D20Action action, ActionSequence actSeq,
            TurnBasedStatus tbStatus)
        {
            var tgt = action.d20ATarget;
            if (tgt == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            var actNum = actSeq.d20ActArrayNum;

            var tbStatCopy = tbStatus.Copy();
            var reach = action.d20APerformer.GetReach(action.d20ActType);
            if (action.d20APerformer.DistanceToObjInFeet(action.d20ATarget) > reach)
            {
                var d20aCopy = action.Copy();
                d20aCopy.d20ActType = D20ActionType.UNSPECIFIED_MOVE;
                d20aCopy.destLoc = tgt.GetLocationFull();
                var result =
                    GameSystems.D20.Actions.MoveSequenceParse(d20aCopy, actSeq, tbStatus, 0.0f, reach, true);
                if (result == ActionErrorCode.AEC_OK)
                {
                    var tbStatusCopy = tbStatus.Copy();
                    actSeq.d20ActArray.Add(d20aCopy);
                    if (actNum < actSeq.d20ActArrayNum)
                    {
                        for (; actNum < actSeq.d20ActArrayNum; actNum++)
                        {
                            var actionToCheck = actSeq.d20ActArray[actNum];
                            result = GameSystems.D20.Actions.TurnBasedStatusUpdate(actionToCheck, tbStatusCopy);
                            if (result != ActionErrorCode.AEC_OK)
                            {
                                tbStatusCopy.errCode = result;
                                return result;
                            }

                            var actionCheckFunc = D20ActionDefs.GetActionDef(actionToCheck.d20ActType).actionCheckFunc;
                            if (actionCheckFunc != null)
                            {
                                result = actionCheckFunc(actionToCheck, tbStatusCopy);
                                if (result != ActionErrorCode.AEC_OK)
                                    return result;
                            }
                        }

                        if (actNum >= actSeq.d20ActArrayNum)
                            return ActionErrorCode.AEC_OK;
                        tbStatusCopy.errCode = result;
                        if (result != ActionErrorCode.AEC_OK)
                            return result;
                    }

                    return ActionErrorCode.AEC_OK;
                }

                return result;
            }


            if ((tbStatus.tbsFlags & TurnBasedStatusFlags.FullAttack) != default)
            {
                action.data1 = tbStatCopy.attackModeCode + 1;
                action.d20Caf |= D20CAF.FULL_ATTACK;
            }

            actSeq.d20ActArray.Add(action);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c4f0)]
        public static ActionErrorCode StandardAttackTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            var hgState = tbStatus.hourglassState;

            if (tbStatus.attackModeCode < tbStatus.baseAttackNumCode || hgState < HourglassState.STD)
            {
                return ActionErrorCode.AEC_NOT_ENOUGH_TIME1; // Not enough time error
            }

            var tgt = action.d20ATarget;
            if (GameSystems.D20.Actions.performingDefaultAction
                && tgt != null
                && GameSystems.Critter.IsDeadOrUnconscious(tgt)
                && (action.d20ActType == D20ActionType.STANDARD_ATTACK ||
                    action.d20ActType == D20ActionType.STANDARD_RANGED_ATTACK))
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            if (hgState != HourglassState.INVALID)
            {
                tbStatus.hourglassState = GameSystems.D20.Actions.GetHourglassTransition(hgState, 2);
            }

            var primaryWeapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
            if (primaryWeapon != null || GameSystems.D20.Actions.DispatchD20ActionCheck(action, tbStatus, DispatcherType.GetCritterNaturalAttacksNum)  <= 0)
            {
                tbStatus.attackModeCode = 0;
            }
            else
            {
                tbStatus.attackModeCode = AttackPacket.ATTACK_CODE_NATURAL_ATTACK;
            }
            tbStatus.baseAttackNumCode = tbStatus.attackModeCode + 1;
            tbStatus.numBonusAttacks = 0;
            tbStatus.numAttacks = 0;
            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode TurnBasedStatusCheckPython(D20Action action, TurnBasedStatus tbStatus)
        {
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c6a0)]
        public static ActionErrorCode FullAttackActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            acp.hourglassCost = 4;
            if (action.d20Caf.HasFlag(D20CAF.FREE_ACTION) || !GameSystems.Combat.IsCombatActive())
                acp.hourglassCost = 0;
            if (tbStatus.attackModeCode >= tbStatus.baseAttackNumCode &&
                tbStatus.hourglassState >= HourglassState.FULL &&
                tbStatus.numBonusAttacks == 0)
            {
                GameSystems.D20.Actions.FullAttackCostCalculate(action, tbStatus,
                    out tbStatus.baseAttackNumCode,
                    out tbStatus.numBonusAttacks,
                    out tbStatus.numAttacks,
                    out tbStatus.attackModeCode);
                tbStatus.surplusMoveDistance = 0;
                tbStatus.tbsFlags = tbStatus.tbsFlags | TurnBasedStatusFlags.FullAttack;
            }

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode ActionCostPartialCharge(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            acp.hourglassCost = 3;
            if (((action.d20Caf & D20CAF.FREE_ACTION) != default) || !GameSystems.Combat.IsCombatActive())
                acp.hourglassCost = 0;

            return ActionErrorCode.AEC_OK;
        }

        public static ActionErrorCode ActionCostPython(D20Action action, TurnBasedStatus tbStatus, ActionCostPacket acp)
        {
            return GameSystems.Script.Actions.GetPyActionCost(action, tbStatus, acp);
        }

        [TempleDllLocation(0x100910f0)]
        public static ActionErrorCode StandardAttackActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_HoldingCharge) != 0
                && tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.TouchAttack)
                && !action.d20Caf.HasFlag(D20CAF.FREE_ACTION))
            {
                acp.hourglassCost = 0;
                return ActionErrorCode.AEC_OK;
            }

            acp.hourglassCost = 0;
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;

            if (!((action.d20Caf & D20CAF.FREE_ACTION) != default) && GameSystems.Combat.IsCombatActive())
            {
                acp.chargeAfterPicker = 1;

                var retainSurplusMoveDist = false;

                if (action.d20ActType == D20ActionType.STANDARD_RANGED_ATTACK &&
                    GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.SHOT_ON_THE_RUN) != 0)
                {
                    retainSurplusMoveDist = true;
                }

                if (action.d20ActType == D20ActionType.STANDARD_ATTACK &&
                    GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.SPRING_ATTACK) != 0)
                {
                    retainSurplusMoveDist = true;
                }

                if (!retainSurplusMoveDist)
                {
                    tbStatus.surplusMoveDistance = 0;
                }
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c130)]
        public static ActionErrorCode MoveActionCost(D20Action d20, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.hourglassCost = 0;
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            if (!d20.d20Caf.HasFlag(D20CAF.FREE_ACTION) && GameSystems.Combat.IsCombatActive())
            {
                acp.moveDistCost = d20.distTraversed;
                tbStatus.numAttacks = 0;
                tbStatus.baseAttackNumCode = 0;
                tbStatus.attackModeCode = 0;
                tbStatus.numBonusAttacks = 0;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c180)]
        public static ActionErrorCode ActionCostMoveAction(D20Action d20, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.hourglassCost = 0;
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            if (!d20.d20Caf.HasFlag(D20CAF.FREE_ACTION) && GameSystems.Combat.IsCombatActive())
            {
                acp.hourglassCost = 1;
                tbStatus.surplusMoveDistance = 0;
                tbStatus.numAttacks = 0;
                tbStatus.baseAttackNumCode = 0;
                tbStatus.attackModeCode = 0;
                tbStatus.numBonusAttacks = 0;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10091f80)]
        public static ActionErrorCode ActionCostNone(D20Action action, TurnBasedStatus tbStatus, ActionCostPacket acp)
        {
            acp.hourglassCost = 0;
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c1d0)]
        public static ActionErrorCode ActionCostStandardAction(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.hourglassCost = 2;
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10090450)]
        public static ActionErrorCode WhirlwindAttackActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket acp)
        {
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;
            acp.hourglassCost = 4;
            if (action.d20Caf.HasFlag(D20CAF.FREE_ACTION) || !GameSystems.Combat.IsCombatActive())
            {
                acp.hourglassCost = 0;
            }

            if (tbStatus.attackModeCode >= tbStatus.baseAttackNumCode &&
                tbStatus.hourglassState >= HourglassState.FULL &&
                tbStatus.numBonusAttacks == 0)
            {
                var performer = action.d20APerformer;
                var reach = performer.GetReach();
                var perfSizeFeet = performer.GetRadius() / locXY.INCH_PER_FEET;

                var numEnemies = 0;
                var searchRadius = 1.0f + perfSizeFeet + reach;
                foreach (var enemy in GameSystems.D20.Combat.EnumerateEnemiesInRange(performer, searchRadius))
                {
                    if (performer.DistanceToObjInFeet(enemy) <= reach)
                    {
                        numEnemies++;
                    }
                }

                tbStatus.numBonusAttacks = 0;
                tbStatus.baseAttackNumCode = 0;
                tbStatus.numAttacks = numEnemies;
                tbStatus.attackModeCode = AttackPacket.ATTACK_CODE_PRIMARY;
                tbStatus.surplusMoveDistance = 0;
            }

            return ActionErrorCode.AEC_OK;
        }

        public static void AttackAppend(ActionSequence actSeq, D20Action d20a, TurnBasedStatus tbStat, int attackCode)
        {
            var newAction = d20a.Copy();
            newAction.data1 = attackCode;
            if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2 || attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4 ||
                attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 6)
            {
                if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2)
                {
                    newAction.d20Caf |= D20CAF.SECONDARY_WEAPON;
                }
                else if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4)
                {
                    if (GameSystems.Feat.HasFeatCount(d20a.d20APerformer, FeatId.IMPROVED_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(d20a.d20APerformer,
                            FeatId.IMPROVED_TWO_WEAPON_FIGHTING_RANGER) != 0)
                    {
                        newAction.d20Caf |= D20CAF.SECONDARY_WEAPON;
                    }
                }
                else
                {
                    if (GameSystems.Feat.HasFeatCount(d20a.d20APerformer, FeatId.GREATER_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(d20a.d20APerformer,
                            FeatId.GREATER_TWO_WEAPON_FIGHTING_RANGER) != 0)
                    {
                        newAction.d20Caf |= D20CAF.SECONDARY_WEAPON;
                    }
                }
            }

            if (tbStat.tbsFlags.HasFlag(TurnBasedStatusFlags.FullAttack))
            {
                newAction.d20Caf |= D20CAF.FULL_ATTACK;
            }

            actSeq.d20ActArray.Add(newAction);
        }

        [TempleDllLocation(0x1008ce30)]
        public static ActionErrorCode StandardAttackPerform(D20Action action)
        {
            var hitAnimIdx = GameSystems.Random.GetInt(0, 2);

            var playCritFlag = false;
            var useSecondaryAnim = false;
            if (GameSystems.D20.Actions.UsingSecondaryWeapon(action))
            {
                action.d20Caf |= D20CAF.SECONDARY_WEAPON;
                useSecondaryAnim = true;
            }
            else if (action.data1 >= AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1)
            {
                useSecondaryAnim = GameSystems.Random.GetBool();
                hitAnimIdx = (action.data1 - (AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1)) % 3;
            }

            GameSystems.D20.Combat.ToHitProcessing(action);

            var caflags = action.d20Caf;
            if (caflags.HasFlag(D20CAF.CRITICAL)
                || GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Play_Critical_Hit_Anim, (int)caflags) != 0) {
                playCritFlag = true;
            }

            if (GameSystems.Anim.PushAttack(action.d20APerformer, action.d20ATarget, -1, hitAnimIdx, playCritFlag, useSecondaryAnim))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100948f0)]
        public static ActionErrorCode UseItemPerform(D20Action action) {
            GameSystems.D20.Actions.DispatchD20ActionCheck(action, null, DispatcherType.D20ActionPerform);
            return CastSpellPerform(action);
        }

        [TempleDllLocation(0x10090d40)]
        public static ActionErrorCode StopConcentrationPerform(D20Action action){
            GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_Remove_Concentration, action.d20APerformer);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10095450)]
        public static ActionErrorCode ActionSequencesAddWithTarget(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            var tgt = action.d20ATarget;
            if (tgt == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            var actNum = sequence.d20ActArrayNum;

            var reach = action.d20APerformer.GetReach(action.d20ActType);
            if (action.d20APerformer.DistanceToObjInFeet(action.d20ATarget) > reach)
            {
                var d20aCopy = action.Copy();
                d20aCopy.d20ActType = D20ActionType.UNSPECIFIED_MOVE;
                d20aCopy.destLoc = tgt.GetLocationFull();
                var result =
                    GameSystems.D20.Actions.MoveSequenceParse(d20aCopy, sequence, tbStatus, 0.0f, reach, true);
                if (result == ActionErrorCode.AEC_OK)
                {
                    var tbStatusCopy = tbStatus.Copy();
                    sequence.d20ActArray.Add(d20aCopy);
                    if (actNum < sequence.d20ActArrayNum)
                    {
                        for (; actNum < sequence.d20ActArrayNum; actNum++)
                        {
                            var actionToCheck = sequence.d20ActArray[actNum];
                            result = GameSystems.D20.Actions.TurnBasedStatusUpdate(actionToCheck, tbStatusCopy);
                            if (result != ActionErrorCode.AEC_OK)
                            {
                                tbStatusCopy.errCode = result;
                                return result;
                            }

                            var actionCheckFunc = D20ActionDefs.GetActionDef(actionToCheck.d20ActType).actionCheckFunc;
                            if (actionCheckFunc != null)
                            {
                                result = actionCheckFunc(actionToCheck, tbStatusCopy);
                                if (result != ActionErrorCode.AEC_OK)
                                    return result;
                            }
                        }

                        if (actNum >= sequence.d20ActArrayNum)
                            return ActionErrorCode.AEC_OK;
                        tbStatusCopy.errCode = result;
                        if (result != ActionErrorCode.AEC_OK)
                            return result;
                    }

                    return ActionErrorCode.AEC_OK;
                }

                return result;
            }

            sequence.d20ActArray.Add(action);
            return ActionErrorCode.AEC_OK;
        }

    }
}