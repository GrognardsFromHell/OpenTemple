using System;
using System.Collections.Generic;
using System.Globalization;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20CombatSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10BCA848)]
        private readonly Dictionary<int, string> _messages;

        [TempleDllLocation(0x102CF708)]
        private float _experienceMultiplier;

        [TempleDllLocation(0x10BCA8B8)]
        [TempleDllLocation(0x100B6690)]
        public bool SavingThrowsAlwaysFail { get; set; }

        [TempleDllLocation(0x10BCA8B4)]
        [TempleDllLocation(0x100b6670)]
        public bool SavingThrowsAlwaysSucceed { get; set; }

        // Used to record how many critters or other challenges have been defeated for a given CR
        // until XP awards can be given out
        [TempleDllLocation(0x10BCA850)]
        private readonly Dictionary<int, int> _challengeRatingsDefeated = new Dictionary<int, int>();

        // Indicates whether last damage was from direct attack (true) or spell (false)
        [TempleDllLocation(0x10BCA8AC)]
        private bool _lastDamageFromAttack;

        [TempleDllLocation(0x100b4770)]
        public D20CombatSystem()
        {
            _messages = Tig.FS.ReadMesFile("mes/combat.mes");

            var vars = Tig.FS.ReadMesFile("rules/combat_vars.mes");
            _experienceMultiplier = float.Parse(vars[0], CultureInfo.InvariantCulture);
        }

        [TempleDllLocation(0x100b4b30)]
        public string GetCombatMesLine(int line)
        {
            return _messages[line];
        }

        [TempleDllLocation(0x100b4b60)]
        public void FloatCombatLine(GameObjectBody obj, int line)
        {
            TextFloaterColor floatColor;

            var objType = obj.type;
            if (objType == ObjectType.pc)
            {
                floatColor = TextFloaterColor.White;
            }
            else if (objType != ObjectType.npc)
            {
                floatColor = TextFloaterColor.Red;
            }
            else
            {
                floatColor = TextFloaterColor.Yellow;
                var npcLeader = GameSystems.Critter.GetLeader(obj);
                if (!GameSystems.Party.IsInParty(npcLeader))
                {
                    floatColor = TextFloaterColor.Red;
                }
            }

            GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic,
                floatColor, GetCombatMesLine(line));
        }

        [TempleDllLocation(0x100b4f20)]
        public bool SavingThrow(GameObjectBody critter, GameObjectBody opponent, int dc,
            SavingThrowType saveType, D20SavingThrowFlag flags = D20SavingThrowFlag.NONE)
        {
            var dispIo = DispIoSavingThrow.Default;
            dispIo.flags = flags;
            dispIo.obj = opponent;

            // Apply static saving throw bonuses for NPCs
            if (critter.IsNPC())
            {
                int npcSaveBonus;
                switch (saveType)
                {
                    case SavingThrowType.Fortitude:
                        npcSaveBonus = critter.GetInt32(obj_f.npc_save_fortitude_bonus);
                        break;
                    case SavingThrowType.Reflex:
                        npcSaveBonus = critter.GetInt32(obj_f.npc_save_reflexes_bonus);
                        break;
                    case SavingThrowType.Will:
                        npcSaveBonus = critter.GetInt32(obj_f.npc_save_willpower_bonus);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(saveType));
                }

                dispIo.bonlist.AddBonus(npcSaveBonus, 0, 139);
            }

            D20StatSystem.Dispatch13SavingThrow(critter, saveType, dispIo);

            var spellResistanceMod = 0;
            dispIo.obj = critter;
            if (opponent != null && !dispIo.flags.HasFlag(D20SavingThrowFlag.CHARM))
            {
                spellResistanceMod = D20StatSystem.Dispatch14SavingThrowResistance(opponent, saveType, dispIo);
            }

            var saveThrowRoll = Dice.D20.Roll();
            if (SavingThrowsAlwaysFail)
            {
                saveThrowRoll = 1;
            }
            else if (SavingThrowsAlwaysSucceed)
            {
                saveThrowRoll = 20;
            }

            dispIo.rollResult = saveThrowRoll;
            if (saveThrowRoll + spellResistanceMod < dc || saveThrowRoll == 1)
            {
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_RerollSavingThrow) != 0)
                {
                    GameSystems.RollHistory.RollHistoryType3Add(critter, dc, saveType, flags, Dice.D20, saveThrowRoll,
                        in dispIo.bonlist);
                    saveThrowRoll = Dice.D20.Roll();
                    flags |= D20SavingThrowFlag.REROLL;
                }
            }

            // Bard's countersong can override the saving throw result
            var v13 = GameSystems.RollHistory.RollHistoryType3Add(critter, dc, saveType, flags, Dice.D20, saveThrowRoll,
                in dispIo.bonlist);
            GameSystems.RollHistory.CreateRollHistoryString(v13);

            if (saveThrowRoll == 1)
            {
                return false;
            }
            else if (saveThrowRoll == 20)
            {
                return true;
            }

            var countersongResult = D20StatSystem.Dispatch40SavingThrow(critter, saveType, dispIo);
            return countersongResult + saveThrowRoll >= dc;
        }

        [TempleDllLocation(0x100b9460)]
        public void KillWithDeathEffect(GameObjectBody critter, GameObjectBody killer)
        {
            if (!critter.AddCondition("Killed By Death Effect", 0, 0))
            {
                Logger.Warn("Failed to add killed by death effect condition.");
            }

            Kill(critter, killer);
        }

        [TempleDllLocation(0x100b8a00)]
        public void Kill(GameObjectBody critter, GameObjectBody killer)
        {
            if (DoOnDeathScripts(critter, killer))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(13, critter, killer);
                FloatCombatLine(critter, 30);
                GameSystems.D20.D20SendSignal(critter, D20DispatcherKey.SIG_Killed, killer);

                critter.AddCondition(BuiltInConditions.Dead);

                if (killer == null)
                {
                    killer = critter.GetObject(obj_f.last_hit_by);
                }

                AwardExperienceForKill(killer, critter);
            }
        }

        [TempleDllLocation(0x100b88e0)]
        private void AwardExperienceForKill(GameObjectBody killer, GameObjectBody killed)
        {
            GameUiBridge.RecordKill(killer, killed);
            if (GameSystems.Party.IsInParty(killer))
            {
                if (!killed.IsPC() && !killed.HasCondition(BuiltInConditions.Summoned))
                {
                    // Prevent a critter from awarding experience multiple times
                    var critterFlags = killed.GetCritterFlags();
                    if (!critterFlags.HasFlag(CritterFlag.EXPERIENCE_AWARDED))
                    {
                        var level = killed.GetStat(Stat.level);
                        var cr = killed.GetInt32(obj_f.npc_challenge_rating);
                        AwardExperienceForChallengeRating(level + cr);
                        killed.SetCritterFlags(critterFlags | CritterFlag.EXPERIENCE_AWARDED);
                    }
                }
            }
        }

        [TempleDllLocation(0x100b8880)]
        public void AwardExperienceForChallengeRating(int challengeRating)
        {
            if (challengeRating > 20)
            {
                challengeRating = 20;
            }
            else if (challengeRating < -2)
            {
                return;
            }

            if (!_challengeRatingsDefeated.TryGetValue(challengeRating, out var count))
            {
                count = 0;
            }

            _challengeRatingsDefeated[challengeRating] = count + 1;

            // Immediately give XP awards if we're not in combat, otherwise they're queued
            if (!GameSystems.Combat.IsCombatActive())
            {
                GiveXPAwards();
            }
        }

        [TempleDllLocation(0x100b88c0)]
        public void GiveXPAwards()
        {
            Stub.TODO();
        }

        private bool DoOnDeathScripts(GameObjectBody obj, GameObjectBody killer)
        {
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Dead) != 0)
            {
                return false;
            }

            var listener = GameSystems.Dialog.GetListeningPartyMember(obj);
            {
                GameSystems.Dialog.GetDyingVoiceLine(obj, listener, out var text, out var soundId);
                GameSystems.Dialog.PlayCritterVoiceLine(obj, listener, text, soundId);
            }

            // Give any NPC follower a chance to complain or lament or rejoyce!
            if (obj.IsPC())
            {
                foreach (var npcFollower in GameSystems.Party.NPCFollowers)
                {
                    if (GameSystems.Critter.GetLeader(npcFollower) == obj)
                    {
                        GameSystems.Dialog.GetLeaderDyingVoiceLine(npcFollower, listener, out var text,
                            out var soundId);
                        GameSystems.Dialog.PlayCritterVoiceLine(npcFollower, listener, text, soundId);
                    }
                }
            }

            // This way of setting death is actually unreliable given certain feats...
            var maxHp = obj.GetStat(Stat.hp_max);
            GameSystems.MapObject.ChangeTotalDamage(obj, maxHp + 10);
            EncodedAnimId animId;
            switch (GameSystems.Random.GetInt(0, 2))
            {
                default:
                    animId = new EncodedAnimId(NormalAnimType.Death);
                    break;
                case 1:
                    animId = new EncodedAnimId(NormalAnimType.Death2);
                    break;
                case 2:
                    animId = new EncodedAnimId(NormalAnimType.Death3);
                    break;
            }

            GameSystems.Critter.HandleDeath(obj, killer, animId);
            return true;
        }


        [TempleDllLocation(0x100B7950)]
        public int DealAttackDamage(GameObjectBody attacker, GameObjectBody target, int d20Data, D20CAF flags,
            D20ActionType actionType)
        {
            GameSystems.AI.ProvokeHostility(attacker, target, 1, 0);

            if (GameSystems.Critter.IsDeadNullDestroyed(target))
            {
                return -1;
            }

            DispIoDamage evtObjDam = new DispIoDamage();
            evtObjDam.attackPacket.d20ActnType = actionType;
            evtObjDam.attackPacket.attacker = attacker;
            evtObjDam.attackPacket.victim = target;
            evtObjDam.attackPacket.dispKey = d20Data;
            evtObjDam.attackPacket.flags = flags;

            ref var weaponUsed = ref evtObjDam.attackPacket.weaponUsed;
            if (flags.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                weaponUsed = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
            }
            else
            {
                weaponUsed = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
            }

            if (weaponUsed != null && weaponUsed.type != ObjectType.weapon)
            {
                weaponUsed = null;
            }

            evtObjDam.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);

            if (flags.HasFlag(D20CAF.CONCEALMENT_MISS))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(11, attacker, target);
                FloatCombatLine(attacker, 45); // Miss (Concealment)!
                var soundId = GameSystems.SoundMap.CombatFindWeaponSound(weaponUsed, attacker, target, 6);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);

                GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Attack_Made, evtObjDam);
                return -1;
            }

            if (!flags.HasFlag(D20CAF.HIT))
            {
                FloatCombatLine(attacker, 29);
                GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Attack_Made, evtObjDam);

                var soundId = GameSystems.SoundMap.CombatFindWeaponSound(weaponUsed, attacker, target, 6);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);

                if (flags.HasFlag(D20CAF.DEFLECT_ARROWS))
                {
                    FloatCombatLine(target, 5052);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(12, attacker, target);
                }

                // dodge animation
                if (!GameSystems.Critter.IsDeadOrUnconscious(target) && !GameSystems.Critter.IsProne(target))
                {
                    GameSystems.Anim.PushDodge(attacker, target);
                }

                return -1;
            }

            if (target != null
                && attacker != null
                && GameSystems.Critter.NpcAllegianceShared(target, attacker)
                && GameSystems.Combat.AffiliationSame(target, attacker))
            {
                FloatCombatLine(target, 107); // Friendly Fire
            }

            var isUnconsciousAlready = GameSystems.Critter.IsDeadOrUnconscious(target);

            var attackerDispatcher = attacker?.GetDispatcher();
            attackerDispatcher?.Process(DispatcherType.DealingDamage, D20DispatcherKey.NONE, evtObjDam);

            if (evtObjDam.attackPacket.flags.HasFlag(D20CAF.CRITICAL))
            {
                // get extra Hit Dice and apply them
                DispIoAttackBonus evtObjCritDice = DispIoAttackBonus.Default;
                evtObjCritDice.attackPacket.victim = target;
                evtObjCritDice.attackPacket.d20ActnType = evtObjDam.attackPacket.d20ActnType;
                evtObjCritDice.attackPacket.attacker = attacker;
                evtObjCritDice.attackPacket.dispKey = d20Data;
                evtObjCritDice.attackPacket.flags = evtObjDam.attackPacket.flags;
                if (evtObjDam.attackPacket.flags.HasFlag(D20CAF.SECONDARY_WEAPON))
                {
                    evtObjCritDice.attackPacket.weaponUsed =
                        GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                }
                else
                {
                    evtObjCritDice.attackPacket.weaponUsed =
                        GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                }

                if (evtObjCritDice.attackPacket.weaponUsed != null &&
                    evtObjCritDice.attackPacket.weaponUsed.type != ObjectType.weapon)
                {
                    evtObjCritDice.attackPacket.weaponUsed = null;
                }

                evtObjCritDice.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
                var extraHitDice = GameSystems.Stat.DispatchAttackBonus(attacker, null, ref evtObjCritDice,
                    DispatcherType.GetCriticalHitExtraDice, D20DispatcherKey.NONE);
                evtObjDam.damage.AddCritMultiplier(1 + extraHitDice, 102);

                FloatCombatLine(attacker, 12);

                // play sound
                var soundId = GameSystems.SoundMap.GetCritterSoundEffect(target, CritterSoundEffect.Attack);
                GameSystems.SoundGame.PositionalSound(soundId, target);
                soundId = GameSystems.SoundMap.CombatFindWeaponSound(evtObjCritDice.attackPacket.weaponUsed, attacker,
                    target,
                    7);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);

                // increase crit hits in logbook
                GameUiBridge.IncreaseCritHits(attacker);
            }
            else
            {
                var soundId =
                    GameSystems.SoundMap.CombatFindWeaponSound(evtObjDam.attackPacket.weaponUsed, attacker, target, 5);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);
            }

            _lastDamageFromAttack = true; // physical damage Flag used for logbook recording
            DamageCritter(attacker, target, evtObjDam);

            // play damage effect particles
            foreach (var damageDice in evtObjDam.damage.dice)
            {
                GameSystems.Anim.PlayDamageEffect(target, damageDice.type, damageDice.rolledDamage);
            }

            GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Attack_Made, evtObjDam);

            // signal events
            if (!isUnconsciousAlready && GameSystems.Critter.IsDeadOrUnconscious(target))
            {
                GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Dropped_Enemy, evtObjDam);
            }

            return evtObjDam.damage.GetOverallDamageByType(DamageType.Unspecified);
        }

        [TempleDllLocation(0x100b6b30)]
        private void DamageCritter(GameObjectBody attacker, GameObjectBody target, DispIoDamage damage)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100b86c0)]
        public bool TargetWithinReachOfLoc(GameObjectBody attacker, GameObjectBody target, LocAndOffsets loc)
        {
            var reach = attacker.GetReach();
            var radiusFt = attacker.GetRadius() / locXY.INCH_PER_FEET; // Conversion to feet
            var distance = target.DistanceToLocInFeet(loc);
            if (distance < 0.0f)
            {
                distance = 0.0f;
            }

            return distance - radiusFt <= reach;
        }

        [TempleDllLocation(0x100B7160)]
        public void ToHitProcessing(D20Action action)
        {
            throw new NotImplementedException();
        }


        [TempleDllLocation(0x100b8600)]
        public bool CanMeleeTargetAtLocation(GameObjectBody attacker, GameObjectBody defender, LocAndOffsets loc)
        {
            if (GameSystems.Critter.IsDeadOrUnconscious(attacker))
            {
                return false;
            }

            var weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
            if (CanAttackTargetAtLocRegardItem(attacker, weapon, defender, loc))
            {
                return true;
            }

            weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
            if (CanAttackTargetAtLocRegardItem(attacker, weapon, defender, loc))
            {
                return true;
            }

            if (!attacker.HasNaturalAttacks())
            {
                return false;
            }

            var reachFt = attacker.GetReach();
            var distFt = attacker.DistanceToLocInFeet(loc);
            var targetRadiusFt = defender.GetRadius() / locXY.INCH_PER_FEET;

            return reachFt >= distFt - targetRadiusFt;
        }

        [TempleDllLocation(0x100b6a50)]
        private bool CanAttackTargetAtLocRegardItem(GameObjectBody obj, GameObjectBody weapon, GameObjectBody targetObj,
            LocAndOffsets loc)
        {
            if (weapon != null)
            {
                if (weapon.type != ObjectType.weapon)
                {
                    return false;
                }

                if (weapon.WeaponFlags.HasFlag(WeaponFlag.RANGED_WEAPON))
                {
                    return false;
                }
            }
            else
            {
                // TODO: It seems more sensible to test for natural attacks here...
                var critterFlags = obj.GetCritterFlags();
                if (!critterFlags.HasFlag(CritterFlag.MONSTER) &&
                    !GameSystems.Feat.HasFeat(obj, FeatId.IMPROVED_UNARMED_STRIKE))
                {
                    return false;
                }
            }

            var reachFt = obj.GetReach();
            var distFt = obj.DistanceToLocInFeet(loc);
            var targetRadiusFt = targetObj.GetRadius() / locXY.INCH_PER_FEET;

            return reachFt >= distFt - targetRadiusFt;
        }

        [TempleDllLocation(0x100b5270)]
        public bool IsWithinThreeFeet(GameObjectBody obj, GameObjectBody target, LocAndOffsets loc)
        {
            var radiusFt = target.GetRadius() / locXY.INCH_PER_FEET;
            var distFt = obj.DistanceToLocInFeet(loc) - radiusFt;
            return distFt < 3.0f;
        }

        // The size of increments in which we search for interrupts along the path
        private const float InterruptSearchIncrement = 4.0f;

        [TempleDllLocation(0x100b8b40)]
        public bool FindAttacksOfOpportunity(GameObjectBody mover, Path path, float aooFreeDistFeet,
            out List<AttackOfOpportunity> attacks)
        {
            // aooFreeDistFeet specifies the minimum distance traveled before an AoO is registered (e.g. for Withdrawal it will receive 5 feet)
            attacks = null;

            var aooDistFeet = aooFreeDistFeet;

            var pathLength = path.GetPathResultLength();
            if (aooDistFeet > pathLength)
            {
                return false;
            }

            var enemies = GetHostileCombatantList(mover);

            while (aooDistFeet < pathLength - InterruptSearchIncrement / 2)
            {
                // this is the first possible distance where an Aoo might occur
                GameSystems.PathX.TruncatePathToDistance(path, out var truncatedLoc, aooDistFeet);

                // obj is moving away from truncatedLoc
                // if an enemy can hit you from when you're in the current truncatedLoc
                // it means you incur an AOO

                // loop over enemies to catch interceptions
                foreach (var enemy in enemies) {

                    // Check if this enemy already has an attack of opportunity in the list
                    var hasInterrupted = false;
                    if (attacks != null)
                    {
                        foreach (var attack in attacks)
                        {
                            if (enemy == attack.Interrupter)
                            {
                                hasInterrupted = true;
                                break;
                            }
                        }
                    }

                    if (!hasInterrupted)
                    {
                        if (GameSystems.D20.D20QueryWithObject(enemy, D20DispatcherKey.QUE_AOOPossible, mover) != 0)
                        {
                            if (GameSystems.D20.Combat.CanMeleeTargetAtLocation(enemy, mover, truncatedLoc))
                            {
                                if (attacks == null)
                                {
                                    attacks = new List<AttackOfOpportunity>();
                                }

                                attacks.Add(new AttackOfOpportunity(enemy, aooDistFeet, truncatedLoc));
                            }
                        }
                    }
                }

                // advanced the truncatedLoc by 4 feet along the path
                aooDistFeet += InterruptSearchIncrement;
            }

            return attacks != null;
        }

        public List<GameObjectBody> GetHostileCombatantList(GameObjectBody critter)
        {
            var result = new List<GameObjectBody>();
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (critter != combatant && !GameSystems.Critter.IsFriendly(critter, combatant))
                {
                    result.Add(combatant);
                }
            }

            return result;
        }

        public IEnumerable<GameObjectBody> EnumerateEnemiesInRange(GameObjectBody critter, float rangeFeet)
        {
            var perfLoc = critter.GetLocationFull();

            using var enemies =
                ObjList.ListRadius(perfLoc, rangeFeet * locXY.INCH_PER_FEET, ObjectListFilter.OLC_CRITTERS);
            foreach (var enemy in enemies)
            {
                if (GameSystems.Critter.IsDeadNullDestroyed(enemy))
                {
                    continue;
                }

                if (enemy != critter
                    && !GameSystems.Critter.NpcAllegianceShared(enemy, critter)
                    && !GameSystems.Critter.IsFriendly(critter, enemy))
                {
                    yield return enemy;
                }
            }
        }

        [TempleDllLocation(0x100b4830)]
        public float EstimateDistance(GameObjectBody performer, locXY destLoc, int isZero, double d)
        {
            var animPathSpec = new AnimPathData();
            animPathSpec.srcLoc = performer.GetLocation();
            animPathSpec.destLoc = destLoc;
            animPathSpec.size = 200;
            animPathSpec.handle = performer;
            sbyte[] deltas = new sbyte[200];
            animPathSpec.deltas = deltas;
            animPathSpec.flags = AnimPathDataFlags.UNK40|AnimPathDataFlags.UNK10;

            if ( animPathSpec.srcLoc.EstimateDistance(destLoc) * 2.5f > isZero )
            {
                var animPathSearchResult = GameSystems.PathX.AnimPathSearch(ref animPathSpec);
                if ( animPathSearchResult != 0 )
                {
                    var distance = animPathSearchResult * 2.5f;
                    var radiusFt = performer.GetRadius() / locXY.INCH_PER_FEET;
                    return distance - radiusFt;
                }
                else
                {
                    return -1.0f;
                }
            }
            else
            {
                return 0.0f;
            }

        }
    }
}