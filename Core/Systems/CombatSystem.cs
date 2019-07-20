using System;
using System.Collections.Generic;
using System.Globalization;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public class CombatSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10AA8418)]
        private bool _active;

        [TempleDllLocation(0x10062eb0)]
        public void Dispose()
        {
            CombatEnd();
        }

        [TempleDllLocation(0x10062ed0)]
        public void Reset()
        {
            CombatEnd(true);
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        public void AdvanceTime(TimePoint time)
        {
            // TODO
        }

        [TempleDllLocation(0x100628d0)]
        public bool IsCombatActive()
        {
            return _active;
        }

        [TempleDllLocation(0x100634e0)]
        public void AdvanceTurn(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100630f0)]
        public void CritterLeaveCombat(GameObjectBody obj)
        {
            if (!GameSystems.Party.IsPlayerControlled(obj))
            {
                var critterFlags = obj.GetCritterFlags();
                if (critterFlags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                {
                    obj.SetCritterFlags(critterFlags & ~CritterFlag.COMBAT_MODE_ACTIVE);
                }

                return;
            }

            if (!IsCombatActive())
                return;

            if (GameSystems.Party.GetConsciousLeader() != obj)
            {
                return;
            }

            if (GameSystems.Critter.IsDeadNullDestroyed(obj))
            {
                if (GameSystems.Party.GetLivingPartyMemberCount() >= 1)
                {
                    return;
                }
            }

            if (!CombatEnd())
            {
                return;
            }

            GameUiBridge.OnExitCombat();

            GameSystems.SoundGame.StopCombatMusic(obj);

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var critterFlags = partyMember.GetCritterFlags();
                if (critterFlags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                {
                    partyMember.SetCritterFlags(critterFlags & ~CritterFlag.COMBAT_MODE_ACTIVE);
                }
            }
        }

        [TempleDllLocation(0x10062a30)]
        private bool CombatEnd(bool resetting = false)
        {
            if (!IsCombatActive())
            {
                return true;
            }

            GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Combat_End);
            _active = false;
            GameSystems.Anim.SetAllGoalsClearedCallback(null);
            if (!GameSystems.Anim.InterruptAllForTbCombat())
            {
                Logger.Debug("CombatEnd: Anim goal interrupt FAILED!");
            }

            GameSystems.D20.Actions.ActionSequencesResetOnCombatEnd();
            if (!resetting)
            {
                GameSystems.D20.Initiative.OnExitCombat();
            }

            GameSystems.D20.EndTurnBasedCombat();

            if (!resetting)
            {
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    AutoReloadCrossbow(partyMember);
                }

                GameSystems.D20.Combat.GiveXPAwards();
            }

            return true;
        }

        [TempleDllLocation(0x100B70A0)]
        private void AutoReloadCrossbow(GameObjectBody critter)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100570c0)]
        public bool HasLineOfAttack(GameObjectBody obj, GameObjectBody target)
        {
            using var objIt = new RaycastPacket();
            objIt.origin = obj.GetLocationFull();
            LocAndOffsets tgtLoc = target.GetLocationFull();
            objIt.targetLoc = tgtLoc;
            objIt.flags = RaycastFlag.StopAfterFirstBlockerFound | RaycastFlag.ExcludeItemObjects |
                          RaycastFlag.HasTargetObj | RaycastFlag.HasSourceObj | RaycastFlag.HasRadius;
            objIt.radius = 0.1f;
            bool blockerFound = false;
            if (objIt.Raycast() > 0)
            {
                foreach (var resultItem in objIt)
                {
                    var resultObj = resultItem.obj;
                    if (resultObj == null)
                    {
                        if (resultItem.flags.HasFlag(RaycastResultFlag.BlockerSubtile))
                        {
                            blockerFound = true;
                        }

                        continue;
                    }

                    if (resultObj.type == ObjectType.portal)
                    {
                        if (!resultObj.IsPortalOpen())
                        {
                            blockerFound = true;
                        }

                        continue;
                    }

                    if (resultObj.IsCritter())
                    {
                        if (GameSystems.Critter.IsDeadOrUnconscious(resultObj)
                            || GameSystems.D20.D20Query(resultObj, D20DispatcherKey.QUE_Prone) == 1)
                        {
                            continue;
                        }

                        // TODO: flag for Cover
                    }
                }
            }

            return !blockerFound;
        }

        [TempleDllLocation(0x1004e730)]
        public void DispatchBeginRound(GameObjectBody obj, int numRounds)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher != null)
            {
                var dispIo = new DispIoD20Signal();
                dispIo.data1 = numRounds;
                dispatcher.Process(DispatcherType.BeginRound, D20DispatcherKey.NONE, dispIo);
                GameSystems.Spell.ObjOnSpellBeginRound(obj);
            }
        }

        [TempleDllLocation(0x10062720)]
        public bool IsCombatModeActive(GameObjectBody obj)
        {
            return obj.GetCritterFlags().HasFlag(CritterFlag.COMBAT_MODE_ACTIVE);
        }

        [TempleDllLocation(0x100624c0)]
        public GameObjectBody GetMainHandWeapon(GameObjectBody obj)
        {
            return GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
        }

        [TempleDllLocation(0x10062df0)]
        public bool IsGameConfigAutoAttack()
        {
            if (IsCombatActive())
                return false;

            return Globals.Config.AutoAttack;
        }

        [TempleDllLocation(0x10063010)]
        public void ThrowItem(GameObjectBody critter, GameObjectBody item, locXY targetLocation)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100629b0)]
        private bool IsCloseEnoughForCombat(GameObjectBody handle)
        {
            if (handle.type == ObjectType.pc)
            {
                return true;
            }

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (handle.DistanceToObjInFeet(partyMember) < MathF.Sqrt(1800.0f))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x10062740)]
        private void CritterEnterCombat(GameObjectBody critter)
        {
        }

        [TempleDllLocation(0x100631e0)]
        public void EnterCombat(GameObjectBody handle)
        {
            if (GameSystems.D20.D20Query(handle, D20DispatcherKey.QUE_EnterCombat) != 0)
            {
                if (IsCloseEnoughForCombat(handle))
                {
                    if (GameSystems.Party.IsPlayerControlled(handle))
                    {
                        foreach (var partyMember in GameSystems.Party.PartyMembers)
                        {
                            if (!partyMember.GetCritterFlags().HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                            {
                                CritterEnterCombat(partyMember);
                            }
                        }
                    }
                    else if (!handle.GetCritterFlags().HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                    {
                        var partyLeader = GameSystems.Party.GetLeader();
                        GameSystems.Item.WieldBestAll(handle, partyLeader);
                        CritterEnterCombat(handle);
                        GameSystems.SoundGame.StartCombatMusic(handle);
                    }
                }
            }
        }

        [TempleDllLocation(0x10062fd0)]
        public void ProjectileCleanup2(GameObjectBody projectile, GameObjectBody actor)
        {
            ThrownItemCleanup(projectile, actor, null);
        }

        [TempleDllLocation(0x10062560)]
        private void ThrownItemCleanup(GameObjectBody projectile, GameObjectBody actor,
            GameObjectBody target, bool recursed = false)
        {
            var projectileFlags = projectile.ProjectileFlags;
            if (projectileFlags.HasFlag(ProjectileFlag.UNK_40))
            {
                var thrownWeapon = projectile.GetObject(obj_f.projectile_parent_weapon);
                GameSystems.MapObject.Move(thrownWeapon, projectile.GetLocationFull());

                GameSystems.MapObject.ClearFlags(thrownWeapon, ObjectFlag.OFF);
                GameSystems.Object.Destroy(projectile);
            }
            else if (projectileFlags.HasFlag(ProjectileFlag.UNK_1000))
            {
                if (!recursed || projectileFlags.HasFlag(ProjectileFlag.UNK_2000))
                {
                    actor.SetCritterFlags2(actor.GetCritterFlags2() & ~CritterFlag2.USING_BOOMERANG);
                    GameSystems.Object.Destroy(projectile);
                    GameSystems.AI.sub_10057790(actor, target);
                }
                else
                {
                    projectile.ProjectileFlags |= ProjectileFlag.UNK_2000;
                    var returnTo = actor.GetLocationFull();
                    if (!GameSystems.Anim.ReturnProjectile(projectile, returnTo, target))
                    {
                        ThrownItemCleanup(projectile, actor, target, true);
                    }
                }
            }
            else
            {
                GameSystems.Object.Destroy(projectile);
            }
        }

        [TempleDllLocation(0x1007eb30)]
        public bool AffiliationSame(GameObjectBody critterA, GameObjectBody critterB)
        {
            return GameSystems.Party.IsInParty(critterA) == GameSystems.Party.IsInParty(critterB);
        }

        public bool IsUnarmed(GameObjectBody critter)
        {
            if (GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponPrimary) != null)
                return false;
            if (GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponSecondary) != null)
                return false;
            return true;
        }

        public bool DisarmCheck(GameObjectBody attacker, GameObjectBody defender)
        {
            	GameObjectBody attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
	            if (attackerWeapon == null)
                {
                    attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                }

                GameObjectBody defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponPrimary);
	            if (defenderWeapon == null)
                {
                    defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponSecondary);
                }

                int attackerRoll = Dice.D20.Roll();
                int attackerSize = attacker.GetStat(Stat.size);
	            BonusList atkBonlist;
	            DispIoAttackBonus dispIoAtkBonus = DispIoAttackBonus.Default;
	            if (GameSystems.Feat.HasFeatCountByClass(attacker, FeatId.IMPROVED_DISARM) != 0)
	            {
		            var featName = GameSystems.Feat.GetFeatName(FeatId.IMPROVED_DISARM);
                    dispIoAtkBonus.bonlist.AddBonus(4, 0, 114, featName); // Feat Improved Disarm
	            }

                dispIoAtkBonus.bonlist.AddBonus((attackerSize - 5) * 4, 0, 316);

                if (attackerWeapon != null)
	            {
		            int attackerWieldType = GameSystems.Item.GetWieldType(attacker, attackerWeapon);
		            if (attackerWieldType == 0)
			            dispIoAtkBonus.bonlist.AddBonus( -4, 0, 340); // Light Weapon
		            else if (attackerWieldType == 2)
			            dispIoAtkBonus.bonlist.AddBonus( 4, 0, 341); // Two Handed Weapon
                    var weaponType = attackerWeapon.GetWeaponType();
		            if (weaponType == WeaponType.spike_chain || weaponType == WeaponType.nunchaku || weaponType == WeaponType.light_flail || weaponType == WeaponType.heavy_flail || weaponType == WeaponType.dire_flail || weaponType == WeaponType.ranseur || weaponType == WeaponType.halfling_nunchaku)
			            dispIoAtkBonus.bonlist.AddBonus( 2, 0, 343); // Weapon Special Bonus
	            } else
	            {
		            if (GameSystems.Feat.HasFeatCountByClass(attacker, FeatId.IMPROVED_UNARMED_STRIKE) == 0)
			            dispIoAtkBonus.bonlist.AddBonus( -4, 0, 342); // Disarming While Unarmed
		            else
			            dispIoAtkBonus.bonlist.AddBonus( -4, 0, 340); // Light Weapon
	            }

	            dispIoAtkBonus.attackPacket.weaponUsed = attackerWeapon;
	            GameSystems.Stat.DispatchAttackBonus(attacker, defender, ref dispIoAtkBonus, DispatcherType.BucklerAcPenalty, 0); // buckler penalty
	            GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoAtkBonus, DispatcherType.ToHitBonus2, 0); // to hit bonus2
	            int atkToHitBonus = GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoAtkBonus, DispatcherType.ToHitBonusFromDefenderCondition, 0);
                int attackerResult = attackerRoll + dispIoAtkBonus.bonlist.OverallBonus;

	            int defenderRoll = Dice.D20.Roll();
	            int defenderSize = defender.GetStat(Stat.size);
	            BonusList defBonlist;
	            DispIoAttackBonus dispIoDefBonus = DispIoAttackBonus.Default;
	            dispIoDefBonus.bonlist.AddBonus((defenderSize - 5) * 4, 0, 316);
	            if (defenderWeapon != null)
	            {
		            int wieldType = GameSystems.Item.GetWieldType(defender, defenderWeapon);
		            if (wieldType == 0)
			            dispIoDefBonus.bonlist.AddBonus(-4, 0, 340); // Light Off-hand Weapon
		            else if (wieldType == 2)
			            dispIoDefBonus.bonlist.AddBonus(4, 0, 341); // Two Handed Weapon
                    var weaponType = defenderWeapon.GetWeaponType();
		            if (weaponType == WeaponType.spike_chain || weaponType == WeaponType.nunchaku || weaponType == WeaponType.light_flail || weaponType == WeaponType.heavy_flail || weaponType == WeaponType.dire_flail || weaponType == WeaponType.ranseur || weaponType == WeaponType.halfling_nunchaku)
			            dispIoAtkBonus.bonlist.AddBonus( 2, 0, 343); // Weapon Special Bonus
	            }
	            
	            dispIoDefBonus.attackPacket.weaponUsed = attackerWeapon;
	            GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.BucklerAcPenalty, 0); // buckler penalty
	            GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.ToHitBonus2, 0); // to hit bonus2
	            int defToHitBonus = GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoDefBonus, DispatcherType.ToHitBonusFromDefenderCondition, 0);
	            int defenderResult = defenderRoll + dispIoAtkBonus.bonlist.OverallBonus;

	            bool attackerSucceeded = attackerResult > defenderResult;
                var mesLineResult = attackerSucceeded ? 143 : 144;
	            var rollHistId = GameSystems.RollHistory.RollHistoryAddType6OpposedCheck(attacker, defender, attackerRoll, defenderRoll, dispIoAtkBonus.bonlist, dispIoDefBonus.bonlist, 5109, mesLineResult, 1);
                GameSystems.RollHistory.CreateRollHistoryString(rollHistId);
	            
	            return attackerSucceeded;
        }

        public bool SunderCheck(GameObjectBody attacker, GameObjectBody defender)
        {

	        GameObjectBody attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
	        if (attackerWeapon == null)
		        attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
	        GameObjectBody defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponPrimary);
	        if (defenderWeapon == null)
		        defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponSecondary);
	        int attackerRoll = Dice.D20.Roll();
	        int attackerSize = attacker.GetStat(Stat.size);
	        BonusList atkBonlist;
	        DispIoAttackBonus dispIoAtkBonus = DispIoAttackBonus.Default;
	        if (GameSystems.Feat.HasFeatCountByClass(attacker, FeatId.IMPROVED_SUNDER) != 0)
	        {
		        string featName = GameSystems.Feat.GetFeatName(FeatId.IMPROVED_SUNDER);
		        dispIoAtkBonus.bonlist.AddBonus(4, 0, 114, featName); // Feat Improved Sunder
	        }
	        dispIoAtkBonus.bonlist.AddBonus( (attackerSize - 5) * 4, 0, 316);
	        if (attackerWeapon != null)
	        {
		        int attackerWieldType = GameSystems.Item.GetWieldType(attacker, attackerWeapon);
		        if (attackerWieldType == 0)
			        dispIoAtkBonus.bonlist.AddBonus( -4, 0, 340); // Light Weapon
		        else if (attackerWieldType == 2)
			        dispIoAtkBonus.bonlist.AddBonus( 4, 0, 341); // Two Handed Weapon
	        }
	        else
	        {
		        dispIoAtkBonus.bonlist.AddBonus( -4, 0, 342); // Disarming While Unarmed
	        }



	        dispIoAtkBonus.attackPacket.weaponUsed = attackerWeapon;
	        
	        GameSystems.Stat.DispatchAttackBonus(attacker, defender, ref dispIoAtkBonus, DispatcherType.BucklerAcPenalty, 0); // buckler penalty
	        GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoAtkBonus, DispatcherType.ToHitBonus2, 0); // to hit bonus2
	        int atkToHitBonus = GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoAtkBonus, DispatcherType.ToHitBonusFromDefenderCondition, 0);
	        int attackerResult = attackerRoll + dispIoAtkBonus.bonlist.OverallBonus;

	        int defenderRoll = Dice.D20.Roll();
	        int defenderSize = defender.GetStat(Stat.size);
	        DispIoAttackBonus dispIoDefBonus = DispIoAttackBonus.Default;
	        dispIoDefBonus.bonlist.AddBonus( (defenderSize - 5) * 4, 0, 316);
	        if (defenderWeapon != null)
	        {
		        int wieldType = GameSystems.Item.GetWieldType(defender, defenderWeapon);
		        if (wieldType == 0)
			        dispIoDefBonus.bonlist.AddBonus( -4, 0, 340); // Light Off-hand Weapon
		        else if (wieldType == 2)
			        dispIoDefBonus.bonlist.AddBonus( 4, 0, 341); // Two Handed Weapon
	        }

	        dispIoDefBonus.attackPacket.weaponUsed = attackerWeapon;
	        GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.BucklerAcPenalty, 0); // buckler penalty
	        GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.ToHitBonus2, 0); // to hit bonus2
	        int defToHitBonus = GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoDefBonus, DispatcherType.ToHitBonusFromDefenderCondition, 0);
	        int defenderResult = defenderRoll + dispIoDefBonus.bonlist.OverallBonus;

	        bool attackerSucceeded = attackerResult > defenderResult;
	        var resultMesLine = attackerSucceeded ? 143 : 144;
	        int rollHistId = GameSystems.RollHistory.RollHistoryAddType6OpposedCheck(attacker, defender, attackerRoll, defenderRoll, dispIoAtkBonus.bonlist, dispIoDefBonus.bonlist, 5109, resultMesLine, 1);
	        GameSystems.RollHistory.CreateRollHistoryString(rollHistId);

	        return attackerSucceeded;
        }

        [TempleDllLocation(0x100b6230)]
        public bool TripCheck(GameObjectBody attacker, GameObjectBody target)
        {
            	if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Untripable) != 0)	{
				GameSystems.RollHistory.CreateFromFreeText( GameSystems.D20.Combat.GetCombatMesLine(171));
				return false;
			}

			void AbilityScoreCheckModDispatch(GameObjectBody obj, GameObjectBody opponent, Stat statUsed, ref BonusList bonlist, int flags) {
				var dispatcher = obj.GetDispatcher();
				if (dispatcher == null)
					return;
				var dispIo = DispIoObjBonus.Default;
				dispIo.bonlist = bonlist;
				dispIo.flags = flags;
				dispIo.obj = opponent;
				dispatcher.Process(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH + (int) statUsed, dispIo);
				bonlist = dispIo.bonlist;
			};


			var attackerRoll = Dice.D20.Roll();
			var attackerBon = BonusList.Default;
			var attackerStrMod = attacker.GetStat(Stat.str_mod);
			attackerBon.AddBonus(attackerStrMod, 0, 103);
			AbilityScoreCheckModDispatch(attacker, target, Stat.strength, ref attackerBon, 1);
			var attackerSize = attacker.GetStat(Stat.size);
			if (attackerSize != 5){
				attackerBon.AddBonus(4 * (attackerSize - 5), 0, 316);
			}

			var attackerResult = attackerRoll + attackerBon.OverallBonus;

			var defenderRoll = Dice.D20.Roll();
			BonusList defenderBon = BonusList.Default;
			var defenderStr = target.GetStat(Stat.strength);
			var defenderDex = target.GetStat(Stat.dexterity);
			Stat defenderStat = Stat.strength;
			if (defenderDex > defenderStr){
				defenderStat = Stat.dexterity;
				var defenderMod = D20StatSystem.GetModifierForAbilityScore(defenderDex);
				defenderBon.AddBonus(defenderMod, 0, 104);
			} else
			{
				var defenderMod = D20StatSystem.GetModifierForAbilityScore(defenderStr);
				defenderBon.AddBonus(defenderMod, 0, 103);
			}
			AbilityScoreCheckModDispatch(target, attacker, defenderStat, ref defenderBon, 3);
			var defenderSize = target.GetStat(Stat.size);
			if (defenderSize != 5) {
				defenderBon.AddBonus(4 * (defenderSize - 5), 0, 316);
			}
			var defenderResult = defenderRoll + defenderBon.OverallBonus;



			var succeeded = attackerResult > defenderResult;
			var resultMesLine = succeeded ? 143 : 144;
			var rollId = GameSystems.RollHistory.RollHistoryAddType6OpposedCheck(attacker, target, attackerRoll, defenderRoll, attackerBon, defenderBon, 5062, resultMesLine, 1 );
			GameSystems.RollHistory.CreateRollHistoryString(rollId);

			return succeeded;
        }
    }
}