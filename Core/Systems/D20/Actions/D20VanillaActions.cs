using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.AI;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Actions
{
    public static class D20ActionVanillaCallbacks
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10095860)]
        public static ActionErrorCode UnspecifiedMoveAddToSeq(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            return GameSystems.D20.Actions.MoveSequenceParse(action, sequence, tbStatus, 0.0f, 0.0f, true);
        }


        [TempleDllLocation(0x1008c910)]
        public static ActionErrorCode StandardAttackLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            if (GameSystems.D20.D20QueryWithObject(
                    action.d20APerformer,
                    D20DispatcherKey.QUE_IsActionInvalid_CheckAction,
                    action) != 0)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            float reach;
            if (action.d20Caf.HasFlag(D20CAF.RANGED))
            {
                reach = 100.0f;
            }
            else
            {
                reach = action.d20APerformer.GetReach(action.d20ActType);
            }

            var distance = action.d20ATarget.DistanceToLocInFeet(location) -
                           action.d20APerformer.GetRadius() / locXY.INCH_PER_FEET;
            if (distance > reach)
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            if (distance < -10.0f)
            {
                return ActionErrorCode.AEC_TARGET_TOO_CLOSE;
            }

            using var objIterator = new RaycastPacket();
            objIterator.flags |= RaycastFlag.HasTargetObj | RaycastFlag.StopAfterFirstBlockerFound |
                                 RaycastFlag.ExcludeItemObjects | RaycastFlag.HasSourceObj;
            objIterator.origin = location;
            objIterator.targetLoc = action.d20ATarget.GetLocationFull();
            objIterator.sourceObj = action.d20APerformer;
            objIterator.target = action.d20ATarget;

            if (!objIterator.TestLineOfSight(true, out var foundCover))
            {
                return ActionErrorCode.AEC_TARGET_BLOCKED;
            }

            if (foundCover)
            {
                action.d20Caf |= D20CAF.COVER;
            }

            return ActionErrorCode.AEC_OK;
        }

        public static CursorType? AttackSequenceGetCursor(D20Action action)
        {
            // TODO: Why not use CAF RANGED?
            var weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
            if (weapon != null && GameSystems.Item.IsRangedWeapon(weapon))
            {
                return CursorType.Arrow;
            }
            else
            {
                return CursorType.Sword;
            }
        }

        [TempleDllLocation(0x10094860)]
        public static void AttackSequenceRender(IGameViewport viewport, D20Action action, SequenceRenderFlag flags)
        {
            var weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
            if (weapon != null && GameSystems.Item.IsRangedWeapon(weapon))
            {
                // NOTE: Vanilla accidentally assigned the weapon flags to the render flags here, most likely a bug!
                ThrowSequenceRender(viewport, action, flags);
            }
            else
            {
                PickerFuncTooltipToHitChance(viewport, action, flags);
            }
        }

        [TempleDllLocation(0x1008ee60)]
        public static ActionErrorCode RangedAttackActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            var itemSlot = EquipSlot.WeaponPrimary;
            if (GameSystems.D20.D20QueryWithObject(
                    action.d20APerformer,
                    D20DispatcherKey.QUE_IsActionInvalid_CheckAction,
                    action) != 0)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                itemSlot = EquipSlot.WeaponSecondary;
            }

            if (!GameSystems.Item.AmmoMatchesItemAtSlot(action.d20APerformer, itemSlot))
            {
                return ActionErrorCode.AEC_OUT_OF_AMMO;
            }

            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            action.d20ATarget.GetInt64(obj_f.location);
            var distance = action.d20APerformer.DistanceToObjInFeet(action.d20ATarget);
            if (distance > 100.0f)
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            if (distance < 0.0f)
            {
                return ActionErrorCode.AEC_TARGET_TOO_CLOSE;
            }

            using var objIterator = new RaycastPacket();
            objIterator.origin = action.d20APerformer.GetLocationFull();
            objIterator.targetLoc = action.d20ATarget.GetLocationFull();
            objIterator.flags |= RaycastFlag.HasTargetObj | RaycastFlag.StopAfterFirstBlockerFound |
                                 RaycastFlag.ExcludeItemObjects | RaycastFlag.HasSourceObj;
            objIterator.sourceObj = action.d20APerformer;
            objIterator.target = action.d20ATarget;

            if (!objIterator.TestLineOfSight(true, out var foundCover))
            {
                return ActionErrorCode.AEC_TARGET_BLOCKED;
            }

            if (foundCover)
            {
                action.d20Caf |= D20CAF.COVER;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008f380)]
        public static ActionErrorCode RangedAttackPerform(D20Action action)
        {
            action.d20Caf |= D20CAF.RANGED;

            var isCrit = false;
            var secondary = false;
            var weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
            if (weapon != null && GameSystems.Item.IsThrowingWeapon(weapon))
            {
                action.d20Caf |= D20CAF.THROWN;
            }

            GameSystems.D20.Combat.ToHitProcessing(action);
            if (action.d20Caf.HasFlag(D20CAF.CRITICAL))
            {
                isCrit = true;
            }

            var animIdx = GameSystems.Random.GetInt(0, 2);
            if (action.data1 == 6 || action.data1 == 8)
            {
                secondary = true;
            }

            bool success;
            if (action.d20Caf.HasFlag(D20CAF.THROWN))
            {
                success = GameSystems.Anim.PushThrowWeapon(action.d20APerformer, action.d20ATarget, -1, secondary);
            }
            else
            {
                success = GameSystems.Anim.PushAttack(action.d20APerformer, action.d20ATarget, -1, animIdx, isCrit,
                    secondary);
            }

            if (success)
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008eb90)]
        public static bool RangedAttackActionFrame(D20Action action)
        {
            var itemSlot = 203;
            var target = action.d20ATarget;
            GameObjectBody thrownItem = null;
            var targetLoc = action.d20ATarget.GetLocationFull();
            var sourceLoc = action.d20APerformer.GetLocationFull();
            if (!action.d20Caf.HasFlag(D20CAF.HIT))
            {
                var hitOffset = new Vector2(
                    GameSystems.Random.GetInt(-30, 30),
                    GameSystems.Random.GetInt(-30, 30)
                );
                target = null;
                targetLoc = LocAndOffsets.FromInches(targetLoc.ToInches2D() + hitOffset);
            }

            GameObjectBody weapon = null;
            if (!action.d20Caf.HasFlag(D20CAF.TOUCH_ATTACK))
            {
                weapon = GameSystems.D20.GetAttackWeapon(action.d20APerformer, action.data1, action.d20Caf);
                if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
                {
                    itemSlot = 204;
                }
            }

            var projectileProtoNum = GameSystems.Item.GetWeaponProjectileProto(weapon);

            var projectile = GameSystems.D20.Combat.CreateProjectileAndThrow(
                sourceLoc,
                projectileProtoNum,
                0,
                0,
                targetLoc,
                action.d20APerformer,
                target);
            projectile.OffsetZ = 60.0f;

            if (action.d20Caf.HasFlag(D20CAF.THROWN) && weapon != null)
            {
                thrownItem = GameSystems.Item.SplitObjectFromStack(weapon, sourceLoc);
            }

            if (GameSystems.D20.Actions.ProjectileAppend(action, projectile, thrownItem))
            {
                action.d20APerformer.DispatchProjectileCreated(projectile, action.d20Caf);
                action.d20Caf |= D20CAF.NEED_PROJECTILE_HIT;
            }

            if (action.d20Caf.HasFlag(D20CAF.THROWN) && weapon != null)
            {
                if (thrownItem == weapon)
                {
                    GameSystems.Item.ItemDrop(weapon);
                }

                GameSystems.MapObject.SetFlags(thrownItem, ObjectFlag.OFF);
                if (weapon.GetWeaponAmmoType() == WeaponAmmoType.shuriken
                    || GameSystems.Feat.HasFeat(action.d20APerformer, FeatId.QUICK_DRAW))
                {
                    // Auto-refill thrown item ammo for shurikens or if user has quick draw feat, but don't move
                    // from either hand
                    var refillItem = GameSystems.Item.FindMatchingStackableItem(action.d20APerformer, weapon);
                    if (refillItem != null && refillItem != GameSystems.Item.GetItemAtInvIdx(action.d20APerformer, 204))
                    {
                        GameSystems.Item.ItemPlaceInIndex(refillItem, itemSlot);
                    }
                }
            }

            return true;
        }


        [TempleDllLocation(0x100982e0)]
        public static bool RangedAttackProjectileHit(D20Action action, GameObjectBody projectile,
            GameObjectBody thrownItem)
        {
            var attacker = action.d20APerformer;
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId1);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId2);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId0);

            if (action.d20Caf.HasFlag(D20CAF.THROWN) && thrownItem != null)
            {
                var itemSlot = 203;
                GameSystems.MapObject.ClearFlags(thrownItem, ObjectFlag.OFF);

                if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
                {
                    itemSlot = 204;
                }

                var previousThrown = thrownItem;
                thrownItem = GameSystems.Item.GiveItemAndFindMatchingStack(thrownItem, attacker);
                if (thrownItem == previousThrown)
                {
                    // No change means the item wasn't stacked ?!
                    GameSystems.Item.ItemPlaceInIndex(thrownItem, itemSlot);
                }
            }

            var defender = action.d20ATarget;
            GameSystems.D20.Combat.DealAttackDamage(attacker, defender, action.data1, action.d20Caf, action.d20ActType);
            if (action.d20Caf.HasFlag(D20CAF.THROWN) && thrownItem != null)
            {
                var thrownBefore = thrownItem;
                thrownItem = GameSystems.Item.SplitObjectFromStack(thrownItem, attacker.GetLocationFull());
                if (thrownBefore == thrownItem)
                {
                    GameSystems.Item.ItemDrop(thrownItem);
                }
            }

            attacker.DispatchProjectileDestroyed(projectile, action.d20Caf);
            var caf = action.d20Caf;
            if (caf.HasFlag(D20CAF.DEFLECT_ARROWS)
                && (!caf.HasFlag(D20CAF.THROWN) || !GameSystems.Feat.HasFeat(defender, FeatId.SNATCH_ARROWS)))
            {
                D20CombatMessage reasonLine;
                if (GameSystems.Feat.HasFeat(defender, FeatId.SNATCH_ARROWS))
                {
                    reasonLine = D20CombatMessage.grabs_missile;
                }
                else
                {
                    reasonLine = D20CombatMessage.deflect_attack;
                }

                var snatcherName = GameSystems.MapObject.GetDisplayNameForParty(defender);

                var reasonText = GameSystems.D20.Combat.GetCombatMesLine(reasonLine);
                var descr = snatcherName + " " + reasonText + "\n";

                GameSystems.RollHistory.CreateFromFreeText(descr);
            }

            if (!caf.HasFlag(D20CAF.THROWN))
            {
                // Places the snatched arrow in the target's inventory
                if (caf.HasFlag(D20CAF.DEFLECT_ARROWS) && GameSystems.Feat.HasFeat(defender, FeatId.SNATCH_ARROWS))
                {
                    var ammo = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
                    var targetLoc = defender.GetLocationFull();
                    var copiedAmmo = GameSystems.MapObject.CloneObject(ammo, targetLoc);
                    copiedAmmo.SetInt32(obj_f.ammo_quantity, 1);
                    GameSystems.Item.SetItemParent(copiedAmmo, defender);
                }

                GameSystems.Item.RangedWeaponDeductAmmo(attacker);

                if (GameSystems.Item.MainWeaponUsesAmmo(attacker))
                {
                    if (!GameSystems.Item.AmmoMatchesItemAtSlot(attacker, EquipSlot.WeaponPrimary))
                    {
                        GameSystems.D20.Combat.FloatCombatLine(attacker, D20CombatMessage.out_of_ammo);
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0, attacker, null);
                    }
                }

                return true;
            }

            if (thrownItem == null)
            {
                return true;
            }

            GameSystems.MapObject.ClearFlags(thrownItem, ObjectFlag.OFF);

            // Snatch arrows allows the target to catch a thrown weapon and throw it back immediately (as an AoO essentially)
            // but only if they have at least one free hand.
            if (action.d20Caf.HasFlag(D20CAF.DEFLECT_ARROWS) &&
                GameSystems.Feat.HasFeat(defender, FeatId.SNATCH_ARROWS))
            {
                if (GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponPrimary) == null)
                {
                    ThrowThrownItemBack(defender, attacker, thrownItem, 203, default);
                }
                else if (GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponSecondary) == null)
                {
                    ThrowThrownItemBack(defender, attacker, thrownItem, 204, D20CAF.SECONDARY_WEAPON);
                }

                return true;
            }

            if (action.d20Caf.HasFlag(D20CAF.HIT))
            {
                // TODO: Shouldn't there actually be a chance to break the item here???
                if (GameSystems.Item.SetItemParent(thrownItem, defender))
                {
                    return true;
                }
            }

            // This'll only happen if the defender's inventory is full and the arrow can't be placed in it
            if (GameSystems.Item.GetParent(thrownItem) != null)
            {
                GameSystems.Item.ItemDrop(thrownItem);
            }

            GameSystems.AI.ForceSpreadOut(thrownItem, defender.GetLocationFull());
            return true;
        }

        private static void ThrowThrownItemBack(GameObjectBody attacker, GameObjectBody defender,
            GameObjectBody thrownItem,
            int useWeaponSlot,
            D20CAF attackFlags)
        {
            thrownItem = GameSystems.Item.SplitObjectFromStack(thrownItem, attacker.GetLocationFull());
            if (GameSystems.Item.SetItemParent(thrownItem, attacker))
            {
                GameSystems.Item.ItemPlaceInIndex(thrownItem, useWeaponSlot);
            }

            var descr = GameSystems.MapObject.GetDisplayNameForParty(attacker);
            var actionDescr = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.missile_counterattack);
            GameSystems.RollHistory.CreateFromFreeText(descr + " " + actionDescr + "\n");

            GameSystems.D20.Actions.RangedCounterAttack(attacker, defender, attackFlags);
            GameSystems.D20.Actions.sequencePerform();
        }


        [TempleDllLocation(0x100948b0)]
        public static ActionErrorCode ReloadPerform(D20Action action)
        {
            GameSystems.Item.ReloadEquippedWeapon(action.d20APerformer);
            GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.reloaded);
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(2, action.d20APerformer, null);
            return 0;
        }


        [TempleDllLocation(0x100903b0)]
        public static ActionErrorCode ReloadActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            actionCost.hourglassCost = 0;
            actionCost.chargeAfterPicker = 0;
            actionCost.moveDistCost = 0;
            if (!action.d20Caf.HasFlag(D20CAF.FREE_ACTION))
            {
                if (GameSystems.Combat.IsCombatActive())
                {
                    var weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
                    if (weapon != null)
                    {
                        tbStatus.baseAttackNumCode = 0;
                        tbStatus.attackModeCode = 0;
                        tbStatus.numBonusAttacks = 0;
                        tbStatus.surplusMoveDistance = 0;

                        if (!GameSystems.Item.IsCrossbow(weapon))
                        {
                            return 0;
                        }

                        if (!GameSystems.Feat.HasFeat(action.d20APerformer, FeatId.RAPID_RELOAD))
                        {
                            actionCost.hourglassCost = ActionCostType.Move;
                        }
                    }
                }
            }

            return 0;
        }


        [TempleDllLocation(0x10095880)]
        public static ActionErrorCode ActionSequencesAddMovement(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            return GameSystems.D20.Actions.MoveSequenceParse(action, sequence, tbStatus, 0.0f, 0.0f, false);
        }


        [TempleDllLocation(0x1008d220)]
        public static ActionErrorCode FivefootstepTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            var flags = tbStatus.tbsFlags;
            if (flags.HasFlag(TurnBasedStatusFlags.Moved5FootStep) || flags.HasFlag(TurnBasedStatusFlags.Moved))
            {
                return ActionErrorCode.AEC_ALREADY_MOVED;
            }
            else
            {
                tbStatus.surplusMoveDistance = 5.0f;
                return ActionErrorCode.AEC_OK;
            }
        }


        [TempleDllLocation(0x1008d0f0)]
        public static ActionErrorCode RunPerform(D20Action action)
        {
            if (GameSystems.Anim.PushRunToTile(action.d20APerformer, action.destLoc, action.path))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return 0;
        }


        [TempleDllLocation(0x1008d1a0)]
        public static ActionErrorCode FivefootstepActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            var flags = tbStatus.tbsFlags;
            if (flags.HasFlag(TurnBasedStatusFlags.Moved5FootStep) || flags.HasFlag(TurnBasedStatusFlags.Moved))
            {
                return ActionErrorCode.AEC_ALREADY_MOVED;
            }

            actionCost.hourglassCost = 0;
            actionCost.chargeAfterPicker = 0;
            actionCost.moveDistCost = 0;
            if (!action.d20Caf.HasFlag(D20CAF.FREE_ACTION))
            {
                if (GameSystems.Combat.IsCombatActive())
                {
                    actionCost.moveDistCost = action.distTraversed;
                    if (action.distTraversed > 0.0f)
                    {
                        actionCost.moveDistCost = 5.0f;
                    }
                }
            }

            if (tbStatus.surplusMoveDistance < actionCost.moveDistCost)
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved5FootStep | TurnBasedStatusFlags.Moved;
            return ActionErrorCode.AEC_OK;
        }

        public static CursorType? MovementGetCursor(D20Action action)
        {
            if (action.d20Caf.HasFlag(D20CAF.TRUNCATED))
            {
                return CursorType.FeetRed;
            }
            return CursorType.FeetGreen;
        }

        [TempleDllLocation(0x1008d090)]
        public static void MovementSequenceRender(IGameViewport viewport, D20Action action, SequenceRenderFlag flags)
        {
            var path = action.path;
            if (path != null && path.flags.HasFlag(PathFlags.PF_COMPLETE))
            {
                var lastMoveAction = (flags & SequenceRenderFlag.FinalMovement) != 0;
                GameSystems.PathXRender.RenderPathPreview(viewport, path, lastMoveAction);
            }

            if (path != null)
            {
                GameSystems.PathXRender.PathpreviewGetFromToDist(path);
            }
        }


        [TempleDllLocation(0x1008cf60)]
        public static ActionErrorCode MoveTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (tbStatus.hourglassState < HourglassState.MOVE)
            {
                return ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
            }

            if (tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.Moved5FootStep))
            {
                return ActionErrorCode.AEC_ALREADY_MOVED;
            }

            var movementSpeed = action.d20APerformer.Dispatch41GetMoveSpeed(out _);
            if (tbStatus.hourglassState != HourglassState.INVALID)
            {
                tbStatus.hourglassState = GameSystems.D20.Actions.GetHourglassTransition(tbStatus.hourglassState, ActionCostType.Move);
            }

            tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            tbStatus.surplusMoveDistance += movementSpeed;
            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008cfc0)]
        public static ActionErrorCode MoveActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Critter_Is_Grappling) )
            {
                return ActionErrorCode.AEC_NOT_ENOUGH_TIME2;
            }

            if (GameSystems.Combat.IsCombatActive())
            {
                if (action.d20Caf.HasFlag(D20CAF.ALTERNATE))
                {
                    return ActionErrorCode.AEC_TARGET_BLOCKED;
                }

                if (tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.Moved5FootStep))
                {
                    return ActionErrorCode.AEC_ALREADY_MOVED;
                }
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008d010)]
        public static ActionErrorCode MovePerform(D20Action action)
        {
            if (GameSystems.Anim.PushRunToTile(action.d20APerformer, action.destLoc, action.path))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return 0;
        }


        [TempleDllLocation(0x1008d240)]
        public static ActionErrorCode DoubleMoveTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (tbStatus.hourglassState < HourglassState.FULL)
            {
                return ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
            }

            if (tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.Moved5FootStep))
            {
                return ActionErrorCode.AEC_ALREADY_MOVED;
            }

            var movementSpeed = action.d20APerformer.Dispatch41GetMoveSpeed(out _);
            if (tbStatus.hourglassState != HourglassState.INVALID)
            {
                tbStatus.hourglassState = GameSystems.D20.Actions.GetHourglassTransition(tbStatus.hourglassState, ActionCostType.FullRound);
            }

            tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            tbStatus.surplusMoveDistance += movementSpeed + movementSpeed;
            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008d2a0)]
        public static ActionErrorCode DoubleMoveActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.Moved5FootStep))
            {
                return ActionErrorCode.AEC_ALREADY_MOVED;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008d2b0)]
        public static ActionErrorCode DoubleMovePerform(D20Action action)
        {
            if (GameSystems.Anim.PushRunToTile(action.d20APerformer, action.destLoc, action.path))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
                Logger.Info("move action {0}", action.d20APerformer);
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c220)]
        public static ActionErrorCode DoubleMoveActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            actionCost.hourglassCost = 0;
            actionCost.chargeAfterPicker = 0;
            actionCost.moveDistCost = 0;
            if (!action.d20Caf.HasFlag(D20CAF.FREE_ACTION))
            {
                if (GameSystems.Combat.IsCombatActive())
                {
                    actionCost.hourglassCost = ActionCostType.FullRound;
                    actionCost.moveDistCost = action.distTraversed;
                    if (tbStatus.hourglassState >= HourglassState.FULL)
                    {
                        var movementSpeed = action.d20APerformer.Dispatch41GetMoveSpeed(out _);
                        tbStatus.surplusMoveDistance = movementSpeed + movementSpeed;
                        tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                    }

                    tbStatus.numAttacks = 0;
                    tbStatus.baseAttackNumCode = 0;
                    tbStatus.attackModeCode = 0;
                    tbStatus.numBonusAttacks = 0;
                }
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008d330)]
        public static ActionErrorCode RunTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (tbStatus.hourglassState < HourglassState.FULL)
            {
                return ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
            }

            if (tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.Moved5FootStep))
            {
                return ActionErrorCode.AEC_ALREADY_MOVED;
            }

            if (tbStatus.hourglassState != HourglassState.INVALID)
            {
                tbStatus.hourglassState = GameSystems.D20.Actions.GetHourglassTransition(tbStatus.hourglassState, ActionCostType.FullRound);
            }

            tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            tbStatus.surplusMoveDistance += action.d20APerformer.DispatchGetRunSpeed(out _);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008d390)]
        public static ActionErrorCode RunActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (action.d20APerformer.HasCondition("sp-Gaseous Form"))
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            var path = action.path;
            if (path == null)
            {
                return ActionErrorCode.AEC_OK;
            }

            if (path.nodeCount != 1)
            {
                return ActionErrorCode.AEC_NEED_A_STRAIGHT_LINE;
            }

            var pathLength = path.GetPathResultLength();
            var runSpeed = action.d20APerformer.DispatchGetRunSpeed(out _);

            if (pathLength > runSpeed)
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008c2e0)]
        public static ActionErrorCode ActionCostRun(D20Action action, TurnBasedStatus tbStatus, ActionCostPacket acp)
        {
            acp.hourglassCost = 0;
            acp.chargeAfterPicker = 0;
            acp.moveDistCost = 0;

            if (!action.d20Caf.HasFlag(D20CAF.FREE_ACTION))
            {
                if (GameSystems.Combat.IsCombatActive())
                {
                    acp.hourglassCost = tbStatus.hourglassState != HourglassState.PARTIAL ? ActionCostType.FullRound : ActionCostType.PartialCharge;

                    tbStatus.surplusMoveDistance = 0;
                    tbStatus.numAttacks = 0;
                    tbStatus.baseAttackNumCode = 0;
                    tbStatus.attackModeCode = 0;
                    tbStatus.numBonusAttacks = 0;
                    tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                }
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008d420)]
        public static ActionErrorCode CastSpellTargetCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (action.d20APerformer == action.d20ATarget)
            {
                return ActionErrorCode.AEC_OK;
            }

            if (action.d20ATarget == null)
            {
                Logger.Info("d20_action.c / _CheckTarget_CastSpell(): WARNING! action.target not set!");
                return ActionErrorCode.AEC_OK;
            }

            if (GameSystems.MapObject.IsUntargetable(action.d20ATarget)
                || action.d20ATarget.HasFlag(ObjectFlag.INVULNERABLE)
                || !GameSystems.D20.Actions.CurrentSequence.ignoreLos
                && (action.d20APerformer.DistanceToObjInFeet(action.d20ATarget) > 125.0f
                    || !GameSystems.Combat.HasLineOfAttack(action.d20APerformer, action.d20ATarget))
                || action.d20APerformer.IsNPC()
                && GameSystems.AI.NpcAiListFindAlly(action.d20APerformer, action.d20ATarget))
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008dea0)]
        public static bool CastSpellActionFrame(D20Action action)
        {
            var projectileProto = 3000;
            var spellEnum = action.d20SpellData.SpellEnum;

            if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spEntry))
            {
                Logger.Info("Failed to retrieve spell entry {0}!", spellEnum);
                return false;
            }

            if (!spEntry.projectileFlag)
            {
                return false;
            }

            if ((spEntry.spellRangeType == SpellRangeType.SRT_Personal ||
                 spEntry.spellRangeType == SpellRangeType.SRT_Touch)
                && !action.d20Caf.HasFlag(D20CAF.RANGED))
            {
                return false;
            }

            if (!GameSystems.Spell.TryGetActiveSpell(action.spellId, out var spellPkt))
            {
                Logger.Info("Failed to retrieve active spell {0}!", action.spellId);
                return false;
            }

            var origin = action.d20APerformer.GetLocationFull();
            var missX = 0;
            var missY = 0;
            if (!action.d20Caf.HasFlag(D20CAF.HIT))
            {
                // Randomly offset the spell hit for misses
                missX = GameSystems.Random.GetInt(-30, 30);
                missY = GameSystems.Random.GetInt(-30, 30);
            }

            var destLoc = action.destLoc;
            var targettingMode = spEntry.modeTargetSemiBitmask.GetBaseMode();

            if (spellPkt.spellEnum == WellKnownSpells.MagicMissile)
            {
                projectileProto = 3004;
            }
            else if (spellPkt.spellEnum == WellKnownSpells.ProduceFlame)
            {
                targettingMode = UiPickerType.Single;
                if (GameSystems.D20.Actions.CurrentSequence != null)
                {
                    var tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
                    tbStatus.tbsFlags &= ~TurnBasedStatusFlags.TouchAttack;
                    action.d20Caf &= ~D20CAF.FREE_ACTION;
                }
            }
            else
            {
                projectileProto = 3000;
            }

            var newTargets = new List<GameObjectBody>();
            if (targettingMode == UiPickerType.Multi)
            {
                spellPkt.projectiles = new GameObjectBody[spellPkt.Targets.Length];

                for (var i = 0; i < spellPkt.projectiles.Length; i++)
                {
                    var target = spellPkt.Targets[i].Object;
                    var projectile = GameSystems.D20.Combat.CreateProjectileAndThrow(origin, projectileProto, missX,
                        missY, destLoc, action.d20APerformer, target);
                    projectile.OffsetZ = 60.0f;
                    projectile.SetInt32(obj_f.projectile_flags_combat, 0);
                    spellPkt.projectiles[i] = projectile;

                    if (!target.IsCritter())
                    {
                        GameSystems.Script.Spells.SpellTriggerProjectile(action.spellId, SpellEvent.BeginProjectile,
                            projectile, i);
                        GameSystems.MapObject.SetFlags(projectile, ObjectFlag.DONTDRAW);
                        if (GameSystems.D20.Actions.ProjectileAppend(action, projectile, null))
                        {
                            action.d20Caf |= D20CAF.NEED_PROJECTILE_HIT;
                        }
                    }
                    else
                    {
                        action.d20ATarget = target;
                        if (GameSystems.D20.D20QueryWithObject(
                                target,
                                D20DispatcherKey.QUE_CanBeAffected_ActionFrame,
                                action,
                                1) != 0)
                        {
                            GameSystems.Script.Spells.SpellTriggerProjectile(action.spellId, SpellEvent.BeginProjectile,
                                projectile, i);
                            GameSystems.MapObject.SetFlags(projectile, ObjectFlag.DONTDRAW);
                            if (GameSystems.D20.Actions.ProjectileAppend(action, projectile, null))
                            {
                                action.d20Caf |= D20CAF.NEED_PROJECTILE_HIT;
                            }

                            newTargets.Add(target);
                        }
                    }
                }
            }
            else if (targettingMode == UiPickerType.Single || targettingMode == UiPickerType.Area)
            {
                GameObjectBody projectile;
                if (targettingMode == UiPickerType.Single || spellPkt.animFlags.HasFlag(SpellAnimationFlag.SAF_UNK8))
                {
                    projectile = GameSystems.D20.Combat.CreateProjectileAndThrow(
                        origin,
                        projectileProto,
                        missX,
                        missY,
                        destLoc,
                        action.d20APerformer,
                        action.d20ATarget);
                }
                else
                {
                    projectile = GameSystems.D20.Combat.CreateProjectileAndThrow(
                        origin,
                        projectileProto,
                        missX,
                        missY,
                        destLoc,
                        action.d20APerformer,
                        null);
                }

                if (projectile == null)
                {
                    return false;
                }

                projectile.OffsetZ = 60.0f;
                projectile.SetInt32(obj_f.projectile_flags_combat, 0);
                spellPkt.projectiles = new[] {projectile};

                if (GameSystems.D20.Actions.ProjectileAppend(action, projectile, null))
                {
                    action.d20Caf |= D20CAF.NEED_PROJECTILE_HIT;
                }

                if (spellPkt.Targets.Length > 0)
                {
                    foreach (var target in spellPkt.Targets)
                    {
                        if (!target.Object.IsCritter())
                        {
                            GameSystems.Script.Spells.SpellTriggerProjectile(action.spellId, SpellEvent.BeginProjectile,
                                projectile, 0);
                            GameSystems.MapObject.SetFlags(projectile, ObjectFlag.DONTDRAW);
                        }
                        else
                        {
                            action.d20ATarget = target.Object;
                            if (GameSystems.D20.D20QueryWithObject(
                                    target.Object,
                                    D20DispatcherKey.QUE_CanBeAffected_ActionFrame,
                                    action,
                                    1) != 0)
                            {
                                GameSystems.Script.Spells.SpellTriggerProjectile(action.spellId,
                                    SpellEvent.BeginProjectile, projectile, 0);
                                GameSystems.MapObject.SetFlags(projectile, ObjectFlag.DONTDRAW);
                                newTargets.Add(target.Object);
                            }
                        }
                    }
                }
                else
                {
                    action.d20ATarget = null;
                    GameSystems.Script.Spells.SpellTriggerProjectile(action.spellId, SpellEvent.BeginProjectile,
                        projectile, 0);
                    GameSystems.MapObject.SetFlags(projectile, ObjectFlag.DONTDRAW);
                }
            }

            spellPkt.SetTargets(newTargets);

            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            return spellPkt.Targets.Length > 0;
        }

        public static CursorType? CastSpellGetCursor(D20Action action)
        {
            var targetInvalid = GameUiBridge.IsPickerTargetInvalid();
            return targetInvalid ? CursorType.UseSpellInvalid : CursorType.UseSpell;
        }

        [TempleDllLocation(0x10092020)]
        public static void CastSpellSequenceRender(IGameViewport viewport, D20Action action, SequenceRenderFlag flags)
        {
        }

        [TempleDllLocation(0x1008cdf0)]
        public static ActionErrorCode LocationCheckWithinReach(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            var inReach =
                GameSystems.D20.Combat.TargetWithinReachOfLoc(action.d20APerformer, action.d20ATarget, location);
            return inReach ? ActionErrorCode.AEC_OK : ActionErrorCode.AEC_TARGET_TOO_FAR;
        }


        [TempleDllLocation(0x100902f0)]
        public static ActionErrorCode HealPerform(D20Action action)
        {
            if (GameSystems.Anim.PushUseSkillOn(action.d20APerformer, action.d20ATarget, SkillId.heal))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090340)]
        public static bool HealActionFrame(D20Action action)
        {
            const int dc = 15;
            if (GameSystems.Skill.SkillRoll(action.d20APerformer, SkillId.heal, dc, out _, SkillCheckFlags.UnderDuress))
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.heal_success);
                GameSystems.D20.D20SendSignal(action.d20ATarget, D20DispatcherKey.SIG_HealSkill);
            }
            else
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.heal_failure);
            }

            return false;
        }

        [TempleDllLocation(0x10091fa0)]
        public static void HealSequenceRender(IGameViewport viewport, D20Action action, SequenceRenderFlag flags)
        {
        }

        [TempleDllLocation(0x1008c4c0)]
        public static ActionErrorCode TargetCheckActionValid(D20Action action, TurnBasedStatus tbStatus)
        {
            return GameSystems.D20.D20QueryWithObject(
                       action.d20APerformer,
                       D20DispatcherKey.QUE_IsActionInvalid_CheckAction,
                       action) != 0
                ? ActionErrorCode.AEC_INVALID_ACTION
                : ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10096760)]
        public static ActionErrorCode TouchAttackAddToSeq(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            // Holding a spell charge on hand is what makes this more complex...
            if (!GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_HoldingCharge) )
            {
                return GameSystems.D20.Actions.AddToSeqWithTarget(action, sequence, tbStatus);
            }

            action.d20ActType = D20ActionType.TOUCH_ATTACK;
            action.spellId =
                (int) GameSystems.D20.D20QueryReturnData(action.d20APerformer, D20DispatcherKey.QUE_HoldingCharge);

            var reach = action.d20APerformer.GetReach(action.d20ActType);
            if (action.d20APerformer.DistanceToObjInFeet(action.d20ATarget) > reach)
            {
                if (!GameSystems.Spell.TryGetActiveSpell(action.spellId, out var spellPkt))
                {
                    return GameSystems.D20.Actions.AddToSeqWithTarget(action, sequence, tbStatus);
                }

                if (spellPkt.spellEnum != WellKnownSpells.ProduceFlame)
                {
                    return GameSystems.D20.Actions.AddToSeqWithTarget(action, sequence, tbStatus);
                }
            }

            if (tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.TouchAttack))
            {
                action.d20Caf |= D20CAF.FREE_ACTION;
            }

            sequence.d20ActArray.Add(action);
            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008c7c0)]
        public static ActionErrorCode Check_StandardAttack(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            if (GameSystems.D20.D20QueryWithObject(
                    action.d20APerformer,
                    D20DispatcherKey.QUE_IsActionInvalid_CheckAction,
                    action) != 0)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            float reach;
            if (action.d20Caf.HasFlag(D20CAF.RANGED))
            {
                reach = 100.0f;
            }
            else
            {
                reach = action.d20APerformer.GetReach(action.d20ActType);
            }

            var distance = action.d20APerformer.DistanceToObjInFeet(action.d20ATarget, false);

            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_HoldingCharge) )
            {
                if (!GameSystems.Spell.TryGetActiveSpell(action.spellId, out var spellPkt))
                {
                    Logger.Info("Unable to retrieve spell_packet for TOUCH_ATTACK, spell_id={0}", action.spellId);
                }
                else
                {
                    if (spellPkt.spellEnum == WellKnownSpells.ProduceFlame)
                    {
                        if (reach >= distance)
                        {
                            return distance < 0 ? ActionErrorCode.AEC_TARGET_TOO_CLOSE : ActionErrorCode.AEC_OK;
                        }

                        reach = 120.0f;
                        action.d20Caf |= D20CAF.RANGED;
                    }
                    else
                    {
                        reach = spellPkt.spellRange;
                    }
                }
            }

            if (distance > reach)
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            return distance < 0 ? ActionErrorCode.AEC_TARGET_TOO_CLOSE : ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090610)]
        public static ActionErrorCode TouchAttackPerform(D20Action action)
        {
            action.d20Caf |= D20CAF.TOUCH_ATTACK;
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_HoldingCharge) )
            {
                if (!GameSystems.Spell.TryGetActiveSpell(action.spellId, out var pktNew))
                {
                    Logger.Info("Unable to retrieve spell_packet for TOUCH_ATTACK for active spell id {0}",
                        action.spellId);

                    if (action.d20Caf.HasFlag(D20CAF.RANGED))
                    {
                        return ActionErrorCode.AEC_OK;
                    }
                }
                else
                {
                    if (action.d20Caf.HasFlag(D20CAF.RANGED))
                    {
                        pktNew.SetTargets(new[] {action.d20ATarget});
                        GameSystems.Spell.UpdateSpellPacket(pktNew);
                        GameSystems.Script.Spells.UpdateSpell(pktNew.spellId);

                        action.d20ActType = D20ActionType.CAST_SPELL;
                        action.data1 = 0;
                        action.d20SpellData.SetSpellData(pktNew.spellEnum, pktNew.spellClass,
                            pktNew.spellKnownSlotLevel, -1, default);

                        return CastSpellActionFrame(action)
                            ? ActionErrorCode.AEC_OK
                            : ActionErrorCode.AEC_INVALID_ACTION;
                    }
                }
            }
            else if (action.d20Caf.HasFlag(D20CAF.RANGED))
            {
                return ActionErrorCode.AEC_OK;
            }

            GameSystems.D20.Combat.ToHitProcessing(action);
            if (GameSystems.Anim.PushAttemptAttack(action.d20APerformer, action.d20ATarget))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x100907a0)]
        public static ActionErrorCode TotalDefensePerform(D20Action action)
        {
            action.d20APerformer.AddCondition(BuiltInConditions.TotalDefense);
            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090900)]
        public static ActionErrorCode FallToProneActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Prone) )
            {
                return ActionErrorCode.AEC_CANT_WHILE_PRONE;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090930)]
        public static ActionErrorCode FallToPronePerform(D20Action action)
        {
            action.d20APerformer.AddCondition(BuiltInConditions.Prone);
            if (GameSystems.Anim.PushAnimate(action.d20APerformer, NormalAnimType.Falldown))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return 0;
        }

        [TempleDllLocation(0x10090980)]
        public static ActionErrorCode StandUpActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Prone) )
            {
                return ActionErrorCode.AEC_OK;
            }

            return ActionErrorCode.AEC_INVALID_ACTION;
        }


        [TempleDllLocation(0x100909b0)]
        public static ActionErrorCode StandUpPerform(D20Action action)
        {
            if (GameSystems.Anim.PushGetUp(action.d20APerformer))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_Standing_Up);
            return 0;
        }


        [TempleDllLocation(0x1008d410)]
        public static ActionErrorCode Dispatch36D20ActnCheck_(D20Action action, TurnBasedStatus tbStatus)
        {
            return (ActionErrorCode) GameSystems.D20.Actions.DispatchD20ActionCheck(action, tbStatus,
                DispatcherType.D20ActionCheck);
        }

        [TempleDllLocation(0x10090a10)]
        public static ActionErrorCode PerformActionClericPaladin(D20Action action)
        {
            if (action.d20ATarget != null && action.d20ATarget.IsCritter() && GameSystems.D20.D20QueryWithObject(
                    action.d20ATarget,
                    D20DispatcherKey.QUE_CanBeAffected_PerformAction,
                    action,
                    1) == 0)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            if (GameSystems.Anim.PushAnimate(action.d20APerformer, NormalAnimType.AbjurationCasting))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090b60)]
        public static bool Dispatch38OnActionFrame(D20Action action)
        {
            return GameSystems.D20.Actions.DispatchD20ActionCheck(action, null, DispatcherType.D20ActionOnActionFrame)
                   != 0;
        }


        [TempleDllLocation(0x10090ab0)]
        public static ActionErrorCode PickupObjectTargetCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (!GameSystems.Party.IsPlayerControlled(action.d20APerformer) ||
                GameSystems.Critter.GetCategory(action.d20APerformer) == MonsterCategory.animal)
            {
                // TODO Slightly weird category check to be honest...
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x100999b0)]
        public static ActionErrorCode PickupObjectPerform(D20Action action)
        {
            GameSystems.Anim.PickUpItemWithSound(action.d20APerformer, action.d20ATarget);
            GameSystems.D20.Actions.ActionPerform(); // TODO: THIS seems weird
            return 0;
        }


        [TempleDllLocation(0x10090b70)]
        public static ActionErrorCode CoupDeGraceTargetCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (GameSystems.D20.D20QueryWithObject(
                    action.d20APerformer,
                    D20DispatcherKey.QUE_IsActionInvalid_CheckAction,
                    action) != 0)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            if (!GameSystems.D20.D20Query(action.d20ATarget, D20DispatcherKey.QUE_CoupDeGrace) )
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090bd0)]
        public static ActionErrorCode CoupDeGracePerform(D20Action action)
        {
            if (action.d20ATarget != null && action.d20ATarget.IsCritter() && GameSystems.D20.D20QueryWithObject(
                    action.d20ATarget,
                    D20DispatcherKey.QUE_CanBeAffected_PerformAction,
                    action,
                    1) == 0)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            action.d20Caf |= D20CAF.CRITICAL | D20CAF.HIT;
            if (GameSystems.Anim.PushAttemptAttack(action.d20APerformer, action.d20ATarget))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090c80)]
        public static bool CoupDeGraceActionFrame(D20Action action)
        {
            if (GameSystems.D20.Combat.DealAttackDamage(action.d20APerformer, action.d20ATarget, action.data1,
                    action.d20Caf,
                    action.d20ActType) > 0
                && !GameSystems.Critter.IsDeadNullDestroyed(action.d20ATarget))
            {
                var targetName = GameSystems.MapObject.GetDisplayNameForParty(action.d20ATarget);
                var message = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.coup_de_grace_kill);
                GameSystems.RollHistory.CreateFromFreeText($"{targetName} {message}\n");
                GameSystems.D20.Combat.Kill(action.d20ATarget, action.d20APerformer);
            }

            return false;
        }


        [TempleDllLocation(0x1008edf0)]
        public static void PickerFuncTooltipToHitChance(IGameViewport viewport, D20Action action, SequenceRenderFlag flags)
        {
            AddAttackHitChanceTooltip(action);
        }

        [TempleDllLocation(0x10091fb0)]
        public static CursorType? UseItemGetCursor(D20Action action)
        {
            if (action.d20SpellData.HasItem)
            {
                var invIndex = action.d20SpellData.itemSpellData;
                var item = GameSystems.Item.GetItemAtInvIdx(action.d20APerformer, invIndex);
                if (item != null)
                {
                    var isPotion = item.type == ObjectType.food;
                    return isPotion ? CursorType.UsePotion : CursorType.UseSpell;
                }
            }

            return null;
        }

        [TempleDllLocation(0x10090b50)]
        public static ActionErrorCode Dispatch37OnActionPerformSimple(D20Action action)
        {
            return (ActionErrorCode) GameSystems.D20.Actions.DispatchD20ActionCheck(action, null,
                DispatcherType.D20ActionPerform);
        }


        [TempleDllLocation(0x10097ba0)]
        public static ActionErrorCode ActionSequencesAddSmite(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            sequence.d20ActArray.Add(action);
            return D20ActionCallbacks.UnspecifiedAttackAddToSeq(action, sequence, tbStatus);
        }

        [TempleDllLocation(0x10090d60)]
        public static ActionErrorCode BreakFreePerform(D20Action action)
        {
            if (GameSystems.Spell.TryGetActiveSpell(action.data1, out var spellPkt))
            {
                GameSystems.D20.D20SendSignal(action.d20APerformer, D20DispatcherKey.SIG_BreakFree, spellPkt.spellId);
            }

            return 0;
        }


        [TempleDllLocation(0x10090e90)]
        public static ActionErrorCode ItemCreationLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            if (GameSystems.Combat.IsCombatActive())
            {
                return ActionErrorCode.AEC_NOT_IN_COMBAT;
            }

            var sleepStatus = GameUiBridge.GetSleepStatus();
            if (sleepStatus != SleepStatus.Safe && sleepStatus != SleepStatus.PassTimeOnly)
            {
                return ActionErrorCode.AEC_AREA_NOT_SAFE;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090ec0)]
        public static ActionErrorCode ItemCreationPerform(D20Action action)
        {
            GameUiBridge.CreateItem(action.d20APerformer, action.data1);
            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10090ee0)]
        public static ActionErrorCode UseMagicDeviceDecipherWrittenSpellPerform(D20Action action)
        {
            GameSystems.Skill.PickInventoryScroll(action.d20APerformer);
            return 0;
        }


        [TempleDllLocation(0x10094770)]
        public static ActionErrorCode SpellCallLightningLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            if (GameSystems.D20.D20Query(action.d20APerformer, D20DispatcherKey.QUE_Critter_Can_Call_Lightning) )
            {
                return ActionErrorCode.AEC_OK;
            }

            GameSystems.D20.Actions.CurrentSequence?.ResetSpell();

            Logger.Info("cannot cast this spell because you have to wait 10 minutes between each lightning!");
            return ActionErrorCode.AEC_CANNOT_CAST_SPELLS;
        }


        [TempleDllLocation(0x1008e660)]
        public static ActionErrorCode SpellCallLightningPerform(D20Action action)
        {
            var spellId = (int) GameSystems.D20.D20QueryReturnData(action.d20APerformer,
                D20DispatcherKey.QUE_Critter_Can_Call_Lightning);
            GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt);

            Dice damageDice;
            if (GameSystems.Map.IsCurrentMapOutdoors())
            {
                if (spellPkt.spellEnum == WellKnownSpells.CallLightningStorm)
                {
                    damageDice = new Dice(5, 10);
                }
                else
                {
                    damageDice = new Dice(3, 10);
                }
            }
            else
            {
                if (spellPkt.spellEnum == WellKnownSpells.CallLightningStorm)
                {
                    damageDice = new Dice(5, 6);
                }
                else
                {
                    damageDice = new Dice(3, 6);
                }
            }

            GameSystems.Vfx.CallLightning(GameSystems.D20.Actions.actSeqSpellLoc);
            var targetPos = GameSystems.D20.Actions.actSeqSpellLoc.ToInches3D();
            GameSystems.ParticleSys.CreateAt("sp-Call Lightning", targetPos);

            foreach (var target in GameSystems.D20.Actions.actSeqTargets)
            {
                if (target.IsCritter())
                {
                    action.d20ATarget = target;
                    if (GameSystems.D20.D20QueryWithObject(
                            target,
                            D20DispatcherKey.QUE_CanBeAffected_PerformAction,
                            action,
                            1) == 0)
                    {
                        continue;
                    }
                }

                if (!D20ModSpells.CheckSpellResistance(action.d20ATarget, spellPkt))
                {
                    GameSystems.D20.Combat.ReflexSaveAndDamage(
                        target,
                        action.d20APerformer,
                        spellPkt.dc,
                        D20SavingThrowReduction.Half,
                        0,
                        damageDice,
                        DamageType.Electricity,
                        D20AttackPower.UNSPECIFIED,
                        D20ActionType.CAST_SPELL,
                        spellPkt.spellId);

                    GameSystems.ParticleSys.CreateAtObj("sp-Call Lightning-hit", target);
                }
            }

            GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Call_Lightning, spellPkt.spellId);
            return 0;
        }


        [TempleDllLocation(0x10091d60)]
        public static void AooMovementSequenceRender(IGameViewport viewport, D20Action action, SequenceRenderFlag flags)
        {
            var performerLoc = action.d20APerformer.GetLocationFull();
            GameSystems.D20.Actions.AddAttackOfOpportunityIndicator(action.destLoc);
            GameSystems.PathXRender.DrawAttackOfOpportunityIndicator(viewport, performerLoc, action.destLoc);
        }

        [TempleDllLocation(0x10090f00)]
        public static ActionErrorCode OpenInventoryPerform(D20Action action)
        {
            GameUiBridge.OpenInventory(action.d20APerformer);
            return 0;
        }


        [TempleDllLocation(0x10090f20)]
        public static ActionErrorCode DisableDevicePerform(D20Action action)
        {
            GameSystems.Anim.PushUseSkillOn(action.d20APerformer, action.d20ATarget, SkillId.disable_device);
            return 0;
        }


        [TempleDllLocation(0x10090f50)]
        public static ActionErrorCode SearchPerform(D20Action action)
        {
            if (GameSystems.Skill.TryUseSearchSkill(action.d20APerformer, out var found))
            {
                var text = GameSystems.Skill.GetSkillUiMessage(1301);
                GameSystems.TextFloater.FloatLine(found, TextFloaterCategory.Generic, TextFloaterColor.White, text);
            }
            else
            {
                var text = GameSystems.Skill.GetSkillUiMessage(1300);
                GameSystems.TextFloater.FloatLine(action.d20APerformer, TextFloaterCategory.Generic,
                    TextFloaterColor.White, text);
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10091de0)]
        public static ActionErrorCode TalkActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            return GameSystems.Combat.IsCombatActive()
                ? ActionErrorCode.AEC_OUT_OF_COMBAT_ONLY
                : ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10091df0)]
        public static ActionErrorCode TalkPerform(D20Action action)
        {
            GameSystems.Anim.PushTalk(action.d20APerformer, action.d20ATarget);
            return 0;
        }

        [TempleDllLocation(0x10091090)]
        public static ActionErrorCode OpenLockPerform(D20Action action)
        {
            GameSystems.Anim.PushUseSkillOn(action.d20APerformer, action.d20ATarget, SkillId.open_lock);
            return 0;
        }


        [TempleDllLocation(0x100910c0)]
        public static ActionErrorCode SleightOfHandPerform(D20Action action)
        {
            GameSystems.Anim.PushUseSkillOn(action.d20APerformer, action.d20ATarget, SkillId.pick_pocket);
            return 0;
        }


        [TempleDllLocation(0x10091280)]
        public static ActionErrorCode OpenContainerLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            if (action.d20APerformer.type != ObjectType.pc)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            if (!GameSystems.D20.Combat.TargetWithinReachOfLoc(
                action.d20APerformer,
                action.d20ATarget,
                location))
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100912e0)]
        public static ActionErrorCode OpenContainerPerform(D20Action action)
        {
            if (action.d20ATarget.type == ObjectType.container)
            {
                var rotation = action.d20APerformer.RotationTo(action.d20ATarget);
                GameSystems.Anim.PushRotate(action.d20APerformer, rotation);

                if (action.d20ATarget.GetContainerFlags().HasFlag(ContainerFlag.HAS_BEEN_OPENED))
                {
                    // If the container was opened previously, we now open instantly
                    GameUiBridge.OpenContainer(action.d20APerformer, action.d20ATarget);
                    return ActionErrorCode.AEC_OK;
                }

                // Otherwise we need to animate
                if (GameSystems.Anim.PushAnimate(action.d20APerformer, NormalAnimType.Picklock))
                {
                    action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                    action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
                    return ActionErrorCode.AEC_OK;
                }
            }
            else if (action.d20ATarget.IsCritter())
            {
                GameUiBridge.OpenContainer(action.d20APerformer, action.d20ATarget);
                return ActionErrorCode.AEC_OK;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x100913d0)]
        public static bool OpenContainerActionFrame(D20Action action)
        {
            if (action.d20ATarget.type == ObjectType.container)
            {
                var keyId = action.d20ATarget.GetInt32(obj_f.container_key_id);
                if (GameSystems.Party.IsInParty(action.d20APerformer) &&
                    GameSystems.Item.HasKey(action.d20APerformer, keyId))
                {
                    GameUiBridge.OpenContainer(action.d20APerformer, action.d20ATarget);
                    return true;
                }

                var scriptEvent = action.d20ATarget.NeedsToBeUnlocked()
                    ? ObjScriptEvent.UnlockAttempt
                    : ObjScriptEvent.Use;

                if (GameSystems.Script.ExecuteObjectScript(
                        action.d20APerformer,
                        action.d20ATarget,
                        action.d20APerformer,
                        scriptEvent,
                        0) != 0)
                {
                    GameUiBridge.OpenContainer(action.d20APerformer, action.d20ATarget);
                    return true;
                }

                return false;
            }
            else
            {
                if (action.d20ATarget.IsCritter())
                {
                    GameUiBridge.OpenContainer(action.d20APerformer, action.d20ATarget);
                }
            }

            return false;
        }

        [TempleDllLocation(0x10092040)]
        public static CursorType? OpenContainerGetCursor(D20Action action)
        {
            if (action.d20ATarget != null)
            {
                if (action.d20ATarget.NeedsToBeUnlocked())
                {
                    var cannotOpen =
                        GameSystems.AI.DryRunAttemptOpenContainer(action.d20APerformer, action.d20ATarget) !=
                        LockStatus.PLS_OPEN;
                    return cannotOpen ? CursorType.Locked : CursorType.HaveKey;
                }
                else
                {
                    return CursorType.UseTeleportIcon;
                }
            }
            else
            {
                return CursorType.ArrowInvalid;
            }
        }

        [TempleDllLocation(0x100947c0)]
        public static ActionErrorCode ThrowAddToSeq(D20Action action, ActionSequence sequence, TurnBasedStatus tbStatus)
        {
            action.d20Caf |= D20CAF.THROWN | D20CAF.RANGED;

            var tbStatusCopy = tbStatus.Copy();
            var throwAction = action.Copy();

            var result = GameSystems.D20.Actions.TurnBasedStatusUpdate(action, tbStatusCopy);
            if (result == ActionErrorCode.AEC_OK)
            {
                throwAction.data1 = tbStatusCopy.attackModeCode;
                sequence.d20ActArray.Add(throwAction);
            }

            return result;
        }

        private static void AddAttackHitChanceTooltip(D20Action action)
        {
            var attackCode = action.data1;
            if (attackCode != 0)
            {
                // This is the attack text ("Primary Attack" and so on)
                var attackMesId = Math.Min(132 + attackCode, 142);

                var attackName = GameSystems.D20.Combat.GetCombatMesLine(attackMesId);
                var hitChance = GameSystems.D20.Combat.GetToHitChance(action);
                var text = $"{attackName} ({hitChance}%)";
                GameSystems.D20.Actions.AddCurrentSequenceTooltip(text);
            }
        }

        [TempleDllLocation(0x1008f460)]
        public static void ThrowSequenceRender(IGameViewport viewport, D20Action action, SequenceRenderFlag flags)
        {
            AddAttackHitChanceTooltip(action);

            // These are used for cover providers
            var throwIndicatorFill = new PackedLinearColorA(0x40FF0000);
            var throwIndicatorOutline = new PackedLinearColorA(0xFFFF0000);

            if (action.d20Caf.HasFlag(D20CAF.COVER))
            {
                using var rayPkt = new RaycastPacket();
                rayPkt.flags |= RaycastFlag.HasTargetObj | RaycastFlag.StopAfterFirstBlockerFound |
                                RaycastFlag.ExcludeItemObjects | RaycastFlag.HasSourceObj;
                rayPkt.origin = action.d20APerformer.GetLocationFull();
                rayPkt.targetLoc = action.d20ATarget.GetLocationFull();
                rayPkt.sourceObj = action.d20APerformer;
                rayPkt.target = action.d20ATarget;
                rayPkt.Raycast();

                foreach (var resultItem in rayPkt)
                {
                    if (resultItem.obj != null)
                    {
                        if (!resultItem.obj.IsCritter()
                            || !GameSystems.Critter.IsDeadNullDestroyed(resultItem.obj)
                            && !GameSystems.Critter.IsProne(resultItem.obj)
                            && !GameSystems.Critter.IsDeadOrUnconscious(resultItem.obj))
                        {
                            if (resultItem.obj.type != ObjectType.portal)
                            {
                                // TODO: These should be batched...
                                var radius = resultItem.obj.GetRadius();
                                var location = resultItem.obj.GetLocationFull();
                                GameSystems.PathXRender.DrawCircle3d(viewport, location, 1.0f, throwIndicatorFill,
                                    throwIndicatorOutline, radius, false);
                            }
                        }
                    }
                }
            }
        }


        [TempleDllLocation(0x10094840)]
        public static ActionErrorCode ThrowGrenadeAddToSeq(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            action.d20Caf |= D20CAF.THROWN_GRENADE | D20CAF.TOUCH_ATTACK;
            return ThrowAddToSeq(action, sequence, tbStatus);
        }


        [TempleDllLocation(0x1008f690)]
        public static ActionErrorCode ThrowGrenadeLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            if (GameSystems.D20.D20QueryWithObject(
                    action.d20APerformer,
                    D20DispatcherKey.QUE_IsActionInvalid_CheckAction,
                    action) != 0)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            var weaponSlot = EquipSlot.WeaponPrimary;
            if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                weaponSlot = EquipSlot.WeaponSecondary;
            }

            if (!GameSystems.Item.AmmoMatchesItemAtSlot(action.d20APerformer, weaponSlot))
            {
                return ActionErrorCode.AEC_OUT_OF_AMMO;
            }

            if (action.d20ATarget == null)
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            var distance = action.d20APerformer.DistanceToObjInFeet(action.d20ATarget, false);
            if (distance > 100.0f)
            {
                return ActionErrorCode.AEC_TARGET_TOO_FAR;
            }

            if (distance < 0.0f)
            {
                return ActionErrorCode.AEC_TARGET_TOO_CLOSE;
            }

            // TODO: This check is copied from StandardAttack
            using var objIterator = new RaycastPacket();
            objIterator.flags |= RaycastFlag.HasTargetObj | RaycastFlag.StopAfterFirstBlockerFound |
                                 RaycastFlag.ExcludeItemObjects | RaycastFlag.HasSourceObj;
            objIterator.origin =
                action.d20ATarget
                    .GetLocationFull(); // TODO: Standard attack uses the passed in location for this check... Isn't that better?
            objIterator.targetLoc = action.d20ATarget.GetLocationFull();
            objIterator.sourceObj = action.d20APerformer;
            objIterator.target = action.d20ATarget;

            if (!objIterator.TestLineOfSight(true, out var foundCover))
            {
                return ActionErrorCode.AEC_TARGET_BLOCKED;
            }

            if (foundCover)
            {
                action.d20Caf |= D20CAF.COVER;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x1008fb50)]
        public static ActionErrorCode ThrowGrenadePerform(D20Action action)
        {
            var criticalHit = false;

            action.d20Caf |= D20CAF.RANGED;
            GameSystems.D20.Combat.ToHitProcessing(action);
            if (action.d20Caf.HasFlag(D20CAF.CRITICAL))
            {
                criticalHit = true;
            }

            var animIdx = GameSystems.Random.GetInt(0, 2);
            var secondaryAnim = (action.data1 == 6 || action.data1 == 8);

            if (GameSystems.Anim.PushAttack(action.d20APerformer, action.d20ATarget, -1, animIdx, criticalHit,
                secondaryAnim))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return 0;
        }

        [TempleDllLocation(0x10090000)]
        public static bool ThrowGrenadeActionFrame(D20Action action)
        {
            var missOffset = Vector2.Zero;
            var weapon = GameSystems.D20.GetAttackWeapon(action.d20APerformer, action.data1, action.d20Caf);
            var target = action.d20ATarget;
            if (!action.d20Caf.HasFlag(D20CAF.HIT))
            {
                var range = weapon.GetInt32(obj_f.weapon_range);
                var distance = action.d20APerformer.DistanceToObjInFeet(action.d20ATarget);
                var rangeIncrements = (int) (distance / range);

                var deviationAngle = Angles.ToRadians(GameSystems.Random.GetInt(0, 360));
                var actualDistance =
                    (float) (locXY.INCH_PER_FEET * (rangeIncrements + GameSystems.Random.GetInt(1, 6)));
                target = null;

                missOffset = new Vector2(
                    MathF.Cos(deviationAngle) * actualDistance,
                    MathF.Sin(deviationAngle) * actualDistance
                );
            }

            var itemSlot = 203;
            if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                itemSlot = 204;
            }

            var sourceLoc = action.d20APerformer.GetLocationFull();
            var targetLoc = LocAndOffsets.FromInches(action.destLoc.ToInches2D() + missOffset);

            var projectileProto = GameSystems.Item.GetWeaponProjectileProto(weapon);
            var projectile = GameSystems.D20.Combat.CreateProjectileAndThrow(sourceLoc, projectileProto, 0, 0,
                targetLoc, action.d20APerformer, target);
            projectile.OffsetZ = 60.0f;

            if (GameSystems.D20.Actions.ProjectileAppend(action, projectile, weapon))
            {
                action.d20APerformer.DispatchProjectileCreated(projectile, action.d20Caf);
                action.d20Caf |= D20CAF.NEED_PROJECTILE_HIT;
            }

            if (action.d20Caf.HasFlag(D20CAF.THROWN_GRENADE) && weapon != null)
            {
                GameSystems.Item.ItemDrop(weapon);
                GameSystems.MapObject.SetFlags(weapon, ObjectFlag.OFF);

                // Try to replenish the thrown item
                if (GameSystems.Feat.HasFeat(action.d20APerformer, FeatId.QUICK_DRAW))
                {
                    var stackableItem = GameSystems.Item.FindMatchingStackableItem(action.d20APerformer, weapon);
                    if (stackableItem != null)
                    {
                        var offHandItem = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponSecondary);
                        if (stackableItem != offHandItem)
                        {
                            // Only move the item when it's not already in the off-hand
                            GameSystems.Item.ItemPlaceInIndex(stackableItem, itemSlot);
                            return true;
                        }
                    }

                    if (!action.d20Caf.HasFlag(D20CAF.THROWN))
                    {
                        GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.out_of_ammo);
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0, projectile, null);
                    }
                }
            }

            return true;
        }


        [TempleDllLocation(0x1008fbe0)]
        public static bool ThrowGrenadeProjectileHit(D20Action action, GameObjectBody target, GameObjectBody ammoItem)
        {
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId1);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId2);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId0);
            var isHit = false;
            if (action.d20Caf.HasFlag(D20CAF.HIT))
            {
                isHit = true;

                // The following convoluted logic will place the used ammo item (the grenade) into the
                // attacker's hands before performing the attack damage calculations, so that the
                // calculated damage takes the grenade into account. (Blergh)

                GameSystems.MapObject.ClearFlags(ammoItem, ObjectFlag.OFF);

                var invIdx = 203;
                if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
                {
                    invIdx = 204;
                }

                var item = GameSystems.Item.GetItemAtInvIdx(action.d20APerformer, invIdx);
                if (GameSystems.Item.SetItemParent(ammoItem, action.d20APerformer))
                {
                    GameSystems.Item.ItemPlaceInIndex(ammoItem, invIdx);
                }

                GameSystems.D20.Combat.DealAttackDamage(action.d20APerformer, action.d20ATarget, action.data1,
                    action.d20Caf,
                    action.d20ActType);
                GameSystems.Item.ItemDrop(ammoItem);
                if (item != null)
                {
                    GameSystems.Item.ItemPlaceInIndex(item, invIdx);
                }

                action.d20APerformer.DispatchProjectileDestroyed(target, action.d20Caf);
                if (ammoItem == null)
                {
                    return true;
                }
            }

            var damageType = (DamageType) ammoItem.GetInt32(obj_f.weapon_attacktype);

            var protoId = ammoItem.ProtoId;

            if (target != null)
            {
                using var listResult = ObjList.ListVicinity(target, ObjectListFilter.OLC_CRITTERS);

                foreach (var critterCloseBy in listResult)
                {
                    if (target.DistanceToObjInFeet(critterCloseBy) > 5.0f)
                    {
                        // I would have expected this distance to be based on the grenade being used,
                        // but apparently it's fixed at 5 feet.
                        continue;
                    }

                    switch (protoId)
                    {
                        // Jaer's spheres of fire
                        case 4197:
                            if (!isHit || critterCloseBy != action.d20ATarget)
                            {
                                GameSystems.D20.Combat.DoUnclassifiedDamage(critterCloseBy, action.d20APerformer,
                                    Dice.D6, damageType, 0,
                                    action.d20ActType);
                            }

                            break;
                        // Holy water
                        case 4224:
                            if ((!isHit || critterCloseBy != action.d20ATarget)
                                && (GameSystems.Critter.IsCategory(critterCloseBy, MonsterCategory.undead)
                                    || GameSystems.Critter.IsCategory(critterCloseBy, MonsterCategory.outsider)
                                    // TODO: I don't like this way of testing alignment. Should be moved out
                                    && (GameSystems.Stat.StatLevelGet(critterCloseBy, Stat.alignment) & 8) != 0))
                            {
                                var damageDice = new Dice(0, 0, 1);
                                GameSystems.D20.Combat.DoUnclassifiedDamage(critterCloseBy, action.d20APerformer,
                                    damageDice, damageType, 0,
                                    action.d20ActType);
                            }

                            break;
                        // Kelno's vial of unholy water
                        case 4225:
                        // Unholy water
                        case 4226:
                            if ((!isHit || critterCloseBy != action.d20ATarget)
                                && GameSystems.Critter.IsCategory(critterCloseBy, MonsterCategory.outsider)
                                // TODO: I don't like this way of testing alignment. Should be moved out
                                && (GameSystems.Stat.StatLevelGet(critterCloseBy, Stat.alignment) & 4) != 0)
                            {
                                var damageDice = new Dice(0, 0, 1);
                                GameSystems.D20.Combat.DoUnclassifiedDamage(critterCloseBy, action.d20APerformer,
                                    damageDice, damageType, 0,
                                    action.d20ActType);
                            }

                            break;
                    }
                }
            }

            var particlesPos = target.GetLocationFull().ToInches3D();
            switch (protoId)
            {
                // Jaer's spheres of fire
                case 4197:
                    GameSystems.ParticleSys.CreateAt("sp-Spheres of Fire", particlesPos);
                    break;
                // Holy water
                case 4224:
                    GameSystems.ParticleSys.CreateAt("sp-holy water", particlesPos);
                    break;
                // Kelno's vial of unholy water
                case 4225:
                // Unholy water
                case 4226:
                    GameSystems.ParticleSys.CreateAt("sp-unholy water", particlesPos);
                    break;
            }

            return true;
        }


        [TempleDllLocation(0x10091e20)]
        public static ActionErrorCode FeintPerform(D20Action action)
        {
            if (GameSystems.Anim.PushAttackOther(action.d20APerformer, action.d20ATarget))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }

            return 0;
        }


        [TempleDllLocation(0x10091ed0)]
        public static bool FeintActionFrame(D20Action action)
        {
            if (GameSystems.D20.Combat.TryFeint(action.d20APerformer, action.d20ATarget))
            {
                action.d20APerformer.AddCondition(BuiltInConditions.Feinting, action.d20ATarget);
                GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, D20CombatMessage.feint_successful);
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(8, action.d20APerformer, action.d20ATarget);
            }
            else
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, D20CombatMessage.failure);
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(9, action.d20APerformer, action.d20ATarget);
            }

            return false;
        }


        [TempleDllLocation(0x10091e70)]
        public static ActionErrorCode FeintActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            actionCost.hourglassCost = 0;
            actionCost.chargeAfterPicker = 0;
            actionCost.moveDistCost = 0;
            if (!action.d20Caf.HasFlag(D20CAF.FREE_ACTION))
            {
                if (GameSystems.Combat.IsCombatActive())
                {
                    actionCost.hourglassCost =
                        GameSystems.Feat.HasFeat(action.d20APerformer, FeatId.IMPROVED_FEINT) ? ActionCostType.Move : ActionCostType.FullRound;
                    tbStatus.numAttacks = 0;
                    tbStatus.baseAttackNumCode = 0;
                    tbStatus.attackModeCode = 0;
                    tbStatus.numBonusAttacks = 0;
                    tbStatus.surplusMoveDistance = 0;
                }
            }

            return 0;
        }


        [TempleDllLocation(0x10091a40)]
        public static ActionErrorCode ReadySpellPerform(D20Action action)
        {
            GameSystems.D20.Actions.AddReadyAction(action.d20APerformer, ReadyVsTypeEnum.RV_Spell);
            GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.action_readied);
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(45, action.d20APerformer, null);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10091a90)]
        public static ActionErrorCode ReadyCounterspellPerform(D20Action action)
        {
            GameSystems.D20.Actions.AddReadyAction(action.d20APerformer, ReadyVsTypeEnum.RV_Counterspell);
            GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.action_readied);
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(45, action.d20APerformer, null);
            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10091ae0)]
        public static ActionErrorCode ReadyEnterPerform(D20Action action)
        {
            GameSystems.D20.Actions.AddReadyAction(action.d20APerformer, ReadyVsTypeEnum.RV_Approach);
            GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.action_readied);
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(45, action.d20APerformer, null);
            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10091b30)]
        public static ActionErrorCode ReadyExitPerform(D20Action action)
        {
            GameSystems.D20.Actions.AddReadyAction(action.d20APerformer, ReadyVsTypeEnum.RV_Withdrawal);
            GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.action_readied);
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(45, action.d20APerformer, null);
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x10091b80)]
        public static ActionErrorCode CopyScrollActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if (GameSystems.Combat.IsCombatActive())
            {
                return ActionErrorCode.AEC_OUT_OF_COMBAT_ONLY;
            }

            var spellEnum = action.d20SpellData.SpellEnum;
            if (GameSystems.D20.D20Query(
                    action.d20APerformer,
                    D20DispatcherKey.QUE_Failed_Copy_Scroll,
                    spellEnum) )
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x100920b0)]
        public static ActionErrorCode ReadiedInterruptPerform(D20Action action)
        {
            var currentSequence = GameSystems.D20.Actions.CurrentSequence;
            var currentAction = currentSequence.d20aCurIdx;

            if (currentAction + 1 < currentSequence.d20ActArrayNum
                && currentSequence.d20ActArray[currentAction + 1].d20ActType != D20ActionType.READIED_INTERRUPT)
            {
                currentSequence.d20ActArray.RemoveRange(currentAction + 1,
                    currentSequence.d20ActArray.Count - currentAction - 1);
            }

            return 0;
        }


        [TempleDllLocation(0x10090db0)]
        public static ActionErrorCode FleeCombatActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            if ((tbStatus.tbsFlags & (TurnBasedStatusFlags.Moved5FootStep | TurnBasedStatusFlags.Moved |
                                      TurnBasedStatusFlags.UNK_1)) != TurnBasedStatusFlags.NONE)
            {
                return ActionErrorCode.AEC_ALREADY_MOVED;
            }

            if (!GameSystems.Map.HasFleeInfo())
            {
                return ActionErrorCode.AEC_INVALID_ACTION;
            }

            return ActionErrorCode.AEC_OK;
        }


        [TempleDllLocation(0x10098810)]
        public static ActionErrorCode FleeCombatPerform(D20Action action)
        {
            // Combat might have ended already I guess...
            if (!GameSystems.Combat.IsCombatActive())
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.success);
                GameSystems.Map.SetFleeing(true);
                GameSystems.Combat.AdvanceTurn(action.d20APerformer);
                return ActionErrorCode.AEC_OK;
            }

            var otherCombatantsPresent = false;
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant.IsNPC() && !GameSystems.Party.IsInParty(combatant))
                {
                    otherCombatantsPresent = true;
                    break;
                }
            }

            if (!otherCombatantsPresent)
            {
                GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.success);
                GameSystems.Map.SetFleeing(true);
            }
            else
            {
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    Logger.Info("Triggering attack of opportunities against '{0}' for fleeing.", partyMember);
                    GameSystems.D20.Actions.TriggerAoOsByAdjacentEnemies(partyMember);
                }

                if (GameSystems.Critter.IsDeadNullDestroyed(action.d20APerformer))
                {
                    // They ded now...
                    GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.failure);
                    GameSystems.Map.SetFleeing(false);
                }
                else
                {
                    // Find the highest challenge rating NPC that's in the fight (and not part of our party)
                    var highestChallengeRating = 0;
                    foreach (var combatant in GameSystems.D20.Initiative)
                    {
                        if (combatant.IsNPC() && !GameSystems.Party.IsInParty(combatant))
                        {
                            var classLevels = GameSystems.Stat.StatLevelGet(combatant, Stat.level);
                            var cr = combatant.GetInt32(obj_f.npc_challenge_rating) + classLevels;
                            if (cr > highestChallengeRating)
                            {
                                highestChallengeRating = cr;
                            }
                        }
                    }

                    var bonlist = BonusList.Default;
                    var bonValue = GameSystems.Critter.GetHitDiceNum(action.d20APerformer);
                    bonlist.AddBonus(bonValue, 0, 137);

                    var survivalSkillBonus = action.d20APerformer.dispatch1ESkillLevel(SkillId.wilderness_lore, null, SkillCheckFlags.UnderDuress);
                    bonlist.AddBonus(survivalSkillBonus, 0, 101);

                    var dc = 15 + highestChallengeRating;

                    var actionText = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.flee_combat);
                    if (GameSystems.Spell.DispelRoll(action.d20APerformer, bonlist, 0, dc, actionText, out _) > 0)
                    {
                        GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.success);
                        GameSystems.Map.SetFleeing(true);
                    }
                    else
                    {
                        GameSystems.D20.Combat.FloatCombatLine(action.d20APerformer, D20CombatMessage.failure);
                        GameSystems.Map.SetFleeing(false);
                    }
                }
            }

            if (GameSystems.Combat.IsCombatActive())
            {
                var currentSequence = GameSystems.D20.Actions.CurrentSequence;
                if (currentSequence != null)
                {
                    currentSequence.tbStatus.tbsFlags |=
                        TurnBasedStatusFlags.Moved | TurnBasedStatusFlags.Moved5FootStep;
                }
            }
            else
            {
                GameSystems.Combat.AdvanceTurn(action.d20APerformer);
            }

            return ActionErrorCode.AEC_OK;
        }
    }
}