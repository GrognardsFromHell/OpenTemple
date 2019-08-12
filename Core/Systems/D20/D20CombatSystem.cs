using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
{
    public enum D20CombatMessage
    {
        poison_level = 0,
        hp = 1,
        critical_miss = 2,
        scarred = 3,
        blinded = 4,
        crippled_arm = 5,
        crippled_leg = 6,
        dodge = 11,
        critical_hit = 12,
        stunned = 16,
        unconscious = 17,
        Nonlethal = 25,
        nonlethal = 25,
        stabilizes = 26,
        dying = 27,
        exertion = 28,
        DEAD = 30,
        hit = 31,
        heal_success = 34,
        heal_failure = 35,
        cleave = 36,
        great_cleave = 37,
        invisible = 41,
        reloaded = 42,
        attack_of_opportunity = 43,
        out_of_ammo = 44,
        miss_concealment = 45,
        acid_damage = 46,
        sleeping = 47,
        flatfooted = 48,
        surprised = 49,
        Points_of_Damage_Absorbed = 50,
        afraid = 52,
        spell_disrupted = 54,
        miscast_armor = 57,
        miscast_defensive_casting = 58,
        arcane_spell_failure_due_to_armor = 59,
        touch_attack_hit = 68,
        holding_charge = 70,
        charmed = 73,
        blind = 76,
        _stunned = 89,
        success = 102,
        failure = 103,
        friendly_fire = 107,
        tumble_successful = 129,
        tumble_unsuccessful = 130,
        attempt_succeeds = 143,
        attempt_fails = 144,
        gains = 145,
        experience_point = 146,
        award_reduced_due_to_uneven_class_levels = 147,
        gains_a_level = 148,
        feint_successful = 152,
        attached = 154,
        action_readied = 157,
        deflect_attack = 159,
        missile_counterattack = 160,
        grabs_missile = 161,
        uses_Manyshot = 162,
        activated = 165,
        deactivated = 166,
        coup_de_grace_kill = 174,
        duration = 175,
        key_Assign_ = 179,
        keyCurrentlyUnassigned = 182,
        keyCurrentlyAssignedTo = 183,
        key_AssignHotkey = 185,
        fortitude = 500,
        reflex = 501,
        will = 502,
        full_attack = 5001,
        scribe_scroll = 5067,
        craft_wand = 5068,
        craft_rod = 5069,
        craft_wondrous_item = 5070,
        craft_magic_arms_and_armor = 5071,
        identify_potion = 5072,
        use_magic_device_decipher_script = 5073,
        track = 5074,
        set_weapon_charge = 5075,
        wild_shape = 5076,
        ready_vs_spell = 5090,
        ready_vs_approach = 5092,
        flee_combat = 5102,
        animal_companion = 6000,
        dismiss_AC = 6001,
        not_during_combat_AC = 6002,
        are_you_sure_you_want_to_dismiss = 6003,
        are_you_sure_you_want_to_dismiss_AC = 6003,
        ok = 6009,
        cancel = 6010,
        name_your_animal_companion = 6012,
    }

    public class D20CombatSystem
    {
        public const int MesTumbleSuccessful = 129;
        public const int MesTumbleUnsuccessful = 130;

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

        public string GetCombatMesLine(D20CombatMessage message) => GetCombatMesLine((int) message);

        public void FloatCombatLine(GameObjectBody obj, D20CombatMessage message, string prefix = null, string suffix = null)
            => FloatCombatLine(obj, (int) message, prefix, suffix);

        [TempleDllLocation(0x100b4b60)]
        public void FloatCombatLine(GameObjectBody obj, int line, string prefix = null, string suffix = null)
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

            var text = GetCombatMesLine(line);
            if (prefix != null)
            {
                text = prefix + text;
            }
            if (suffix != null)
            {
                text += suffix;
            }

            GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, floatColor, text);
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
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_RerollSavingThrow))
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

        [TempleDllLocation(0x100b83c0)]
        public bool SavingThrowSpell(GameObjectBody objHnd, GameObjectBody caster, int DC, SavingThrowType saveType,
            D20SavingThrowFlag D20STDFlags, int spellId)
        {
            D20SavingThrowFlag flags;
            SpellEntry spellEntry;
            SpellPacketBody spPkt;

            var result_1 = false;
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out spPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spPkt.spellEnum, out spellEntry);
                flags = D20STDFlags | D20SavingThrowFlag.SPELL_LIKE_EFFECT;
                switch (spellEntry.spellSchoolEnum)
                {
                    case 1:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_ABJURATION;
                        break;
                    case 2:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_CONJURATION;
                        break;
                    case 3:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_DIVINATION;
                        break;
                    case 4:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT;
                        break;
                    case 5:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_EVOCATION;
                        break;
                    case 6:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_ILLUSION;
                        break;
                    case 7:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_NECROMANCY;
                        break;
                    case 8:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_TRANSMUTATION;
                        break;
                    default:
                        break;
                }

                // TODO: Make this nicer. Transfers spell descriptors like ACID, et al over to the saving throw flags
                // TODO: This overflows the available bits (32-bit are not enough)
                for (var i = 0; i < 21; i++)
                {
                    var spellDescriptor = (SpellDescriptor) (1 << i);
                    var savingThrowFlag = (D20SavingThrowFlag) (1 << (i + 13));

                    if (spellEntry.HasDescriptor(spellDescriptor))
                    {
                        flags |= savingThrowFlag;
                    }
                }

                result_1 = SavingThrow(objHnd, caster, DC, saveType, flags);
                spPkt.savingThrowResult = result_1;
                GameSystems.Spell.UpdateSpellPacket(spPkt);
                GameSystems.Script.Spells.UpdateSpell(spPkt.spellId);
            }

            return result_1;
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
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Dead))
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
                foreach (var enemy in enemies)
                {
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
            animPathSpec.flags = AnimPathDataFlags.UNK40 | AnimPathDataFlags.UNK10;

            if (animPathSpec.srcLoc.EstimateDistance(destLoc) * 2.5f > isZero)
            {
                var animPathSearchResult = GameSystems.PathX.AnimPathSearch(ref animPathSpec);
                if (animPathSearchResult != 0)
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

        [TempleDllLocation(0x100b4d00)]
        public GameObjectBody CreateProjectileAndThrow(LocAndOffsets sourceLoc, int protoId, int missX, int missY,
            LocAndOffsets targetLoc, GameObjectBody attacker, GameObjectBody target)
        {
            if (sourceLoc.location == targetLoc.location)
            {
                return null;
            }

            var projectile = GameSystems.MapObject.CreateObject((ushort) protoId, sourceLoc.location);
            if (projectile == null)
            {
                return null;
            }

            projectile.SetInt32(obj_f.projectile_flags_combat_damage, 0);
            projectile.SetInt32(obj_f.projectile_flags_combat, 0);
            projectile.SetObject(obj_f.projectile_parent_weapon, null);
            projectile.SetObject(obj_f.projectile_parent_weapon, null);

            GameSystems.Anim.PushThrowProjectile(attacker, projectile, missX, missY, target, targetLoc, 1);
            return projectile;
        }

        [TempleDllLocation(0x100b4970)]
        public int GetToHitChance(D20Action action)
        {
            var attacker = action.d20ATarget;
            var attackType = action.data1;

            DispIoAttackBonus dispIo = DispIoAttackBonus.Default;
            dispIo.attackPacket.victim = attacker;
            dispIo.attackPacket.flags = action.d20Caf;
            dispIo.attackPacket.attacker = action.d20APerformer;
            dispIo.attackPacket.dispKey = attackType;
            dispIo.attackPacket.d20ActnType = action.d20ActType;
            GameObjectBody weapon;
            if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponSecondary);
            }
            else
            {
                weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
            }

            if (weapon != null && weapon.type == ObjectType.weapon)
            {
                dispIo.attackPacket.weaponUsed = weapon;
            }

            dispIo.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(action.d20APerformer);

            GameSystems.Stat.Dispatch16GetToHitBonus(action.d20APerformer, dispIo);
            var attackBonus = attacker.DispatchGetToHitModifiersFromDefender(dispIo);

            // Reuse the attack packet to query the AC
            var acDispIo = new DispIoAttackBonus();
            acDispIo.attackPacket = dispIo.attackPacket;
            acDispIo.bonlist = BonusList.Default;

            GameSystems.Stat.GetAC(attacker, acDispIo);
            var ac = action.d20APerformer.DispatchGetAcAdjustedByAttacker(acDispIo);

            var result = 5 * (attackBonus - ac + 20);
            return Math.Clamp(result, 5, 95);
        }

        [TempleDllLocation(0x100b9500)]
        public bool ReflexSaveAndDamage(GameObjectBody victim, GameObjectBody attacker, int dc,
            D20SavingThrowReduction reduction, D20SavingThrowFlag savingThrowFlags,
            Dice attackDice, DamageType attackType, D20AttackPower attackPower, D20ActionType actionType, int spellId)
        {
            DispIoReflexThrow savingThrowIo = DispIoReflexThrow.Default;

            savingThrowIo.reduction = reduction;
            savingThrowIo.attackPower = attackPower;
            savingThrowIo.damageMesLine = 105;
            savingThrowIo.attackType = attackType;
            savingThrowIo.flags = savingThrowFlags;
            savingThrowIo.throwResult =
                GameSystems.D20.Combat.SavingThrow(victim, attacker, dc, SavingThrowType.Reflex, savingThrowFlags);

            D20CAF flags = default;
            if (savingThrowIo.throwResult)
            {
                flags = D20CAF.SAVE_SUCCESSFUL;
                switch (reduction)
                {
                    case D20SavingThrowReduction.None:
                        savingThrowIo.effectiveReduction = 0;
                        break;
                    case D20SavingThrowReduction.Quarter:
                        savingThrowIo.effectiveReduction = 25;
                        break;
                    case D20SavingThrowReduction.Half:
                        savingThrowIo.effectiveReduction = 50;
                        break;
                    default:
                        savingThrowIo.effectiveReduction = 100;
                        break;
                }
            }

            victim.GetDispatcher()?.Process(DispatcherType.ReflexThrow, D20DispatcherKey.NONE, savingThrowIo);

            if (savingThrowIo.effectiveReduction == 100)
            {
                if (actionType == D20ActionType.CAST_SPELL)
                    DealSpellDamage(
                        victim,
                        attacker,
                        attackDice,
                        savingThrowIo.attackType,
                        savingThrowIo.attackPower,
                        100,
                        103,
                        actionType,
                        spellId,
                        flags);
                else
                    DoDamage(
                        victim,
                        attacker,
                        attackDice,
                        savingThrowIo.attackType,
                        savingThrowIo.attackPower,
                        100,
                        103,
                        actionType);
            }
            else if (actionType == D20ActionType.CAST_SPELL)
            {
                DealSpellDamage(
                    victim,
                    attacker,
                    attackDice,
                    savingThrowIo.attackType,
                    savingThrowIo.attackPower,
                    savingThrowIo.effectiveReduction,
                    savingThrowIo.damageMesLine,
                    actionType,
                    spellId,
                    flags);
            }
            else
            {
                DoDamage(
                    victim,
                    attacker,
                    attackDice,
                    savingThrowIo.attackType,
                    savingThrowIo.attackPower,
                    savingThrowIo.effectiveReduction,
                    savingThrowIo.damageMesLine,
                    actionType);
            }

            return savingThrowIo.throwResult;
        }

        [TempleDllLocation(0x100b9080)]
        public void SpellDamageFull(GameObjectBody target, GameObjectBody attacker, Dice dicePacked, DamageType damType,
            D20AttackPower attackPower, D20ActionType actionType, int spellId, D20CAF d20caf)
        {
            DealSpellDamage(target, attacker, dicePacked, damType, attackPower, 100, 103, actionType, spellId, d20caf);
        }

        [TempleDllLocation(0x100b7f80)]
        void DealSpellDamage(GameObjectBody tgt, GameObjectBody attacker, Dice dice, DamageType type,
            D20AttackPower attackPower, int reduction, int damageDescId, D20ActionType actionType, int spellId,
            D20CAF flags)
        {
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spPkt))
            {
                return;
            }

            if (attacker != null && attacker != tgt && GameSystems.Critter.NpcAllegianceShared(tgt, attacker))
            {
                FloatCombatLine(tgt, D20CombatMessage.friendly_fire);
            }

            GameSystems.AI.ProvokeHostility(attacker, tgt, 1, 0);

            if (GameSystems.Critter.IsDeadNullDestroyed(tgt))
            {
                return;
            }

            DispIoDamage evtObjDam = new DispIoDamage();
            evtObjDam.attackPacket.d20ActnType = actionType;
            evtObjDam.attackPacket.attacker = attacker;
            evtObjDam.attackPacket.victim = tgt;
            evtObjDam.attackPacket.dispKey = 1;
            evtObjDam.attackPacket.flags = flags | D20CAF.HIT;

            if (attacker != null && attacker.IsCritter())
            {
                GameObjectBody weapon;
                if (flags.HasFlag(D20CAF.SECONDARY_WEAPON))
                {
                    weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                }
                else
                {
                    weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                }

                if (weapon != null && weapon.type == ObjectType.weapon)
                {
                    evtObjDam.attackPacket.weaponUsed = weapon;
                }

                evtObjDam.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
            }
            else
            {
                evtObjDam.attackPacket.weaponUsed = null;
                evtObjDam.attackPacket.ammoItem = null;
            }

            if (reduction != 100)
            {
                evtObjDam.damage.AddModFactor(reduction * 0.01f, type, damageDescId);
            }

            evtObjDam.damage.AddDamageDice(dice, type, 103);
            evtObjDam.damage.AddAttackPower(attackPower);
            var mmData = spPkt.metaMagicData;
            if (mmData.metaMagicEmpowerSpellCount > 0)
                evtObjDam.damage.flags |= 2; // empowered
            if (mmData.metaMagicFlags.HasFlag(MetaMagicFlags.MetaMagic_Maximize))
                evtObjDam.damage.flags |= 1; // maximized

            attacker.DispatchSpellDamage(evtObjDam.damage, tgt, spPkt);

            _lastDamageFromAttack = false; // is weapon damage (used in logbook for record holding)

            DamageCritter(attacker, tgt, evtObjDam);
        }

        [TempleDllLocation(0x100b94c0)]
        public void DoUnclassifiedDamage(GameObjectBody target, GameObjectBody attacker, Dice dmgDice,
            DamageType damageType,
            D20AttackPower attackPowerType, D20ActionType actionType)
        {
            DoDamage(target, attacker, dmgDice, damageType, attackPowerType, 100, 103, actionType);
        }

        [TempleDllLocation(0x100b8d70)]
        public void DoDamage(GameObjectBody target, GameObjectBody attacker, Dice dmgDice, DamageType damageType,
            D20AttackPower attackPowerType, int reduction, int damageDescMesKey, D20ActionType actionType)
        {
            Trace.Assert(target != null);

            var wasDeadOrUnconscious = GameSystems.Critter.IsDeadOrUnconscious(target);

            if (attacker != null && target != attacker && GameSystems.Combat.AffiliationSame(target, attacker))
            {
                FloatCombatLine(target, D20CombatMessage.friendly_fire);
            }

            if (attacker != null && attacker.IsCritter())
            {
                GameSystems.AI.ProvokeHostility(attacker, target, 1, 0);
            }

            if (!GameSystems.Critter.IsDeadNullDestroyed(target))
            {
                var dispIo = new DispIoDamage();
                dispIo.attackPacket.d20ActnType = actionType;
                dispIo.attackPacket.attacker = attacker;
                dispIo.attackPacket.victim = target;
                dispIo.attackPacket.dispKey = 1;
                dispIo.attackPacket.flags = D20CAF.HIT;
                if (attacker != null)
                {
                    if (IsTrapped(attacker))
                    {
                        dispIo.attackPacket.flags |= D20CAF.TRAP;
                    }

                    if (dispIo.attackPacket.flags.HasFlag(D20CAF.SECONDARY_WEAPON))
                    {
                        dispIo.attackPacket.weaponUsed =
                            GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                    }
                    else
                    {
                        dispIo.attackPacket.weaponUsed = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                    }

                    if (attacker.IsCritter())
                    {
                        dispIo.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
                    }
                    else
                    {
                        dispIo.attackPacket.ammoItem = null;
                    }
                }

                if (dispIo.attackPacket.weaponUsed != null && dispIo.attackPacket.weaponUsed.type != ObjectType.weapon)
                {
                    dispIo.attackPacket.weaponUsed = null;
                }

                if (reduction != 100)
                {
                    var dmgFactor = reduction * 0.01f;
                    dispIo.damage.AddModFactor(dmgFactor, damageType, damageDescMesKey);
                }

                dispIo.damage.AddDamageDice(dmgDice, damageType, damageDescMesKey);
                dispIo.damage.AddAttackPower(attackPowerType);
                _lastDamageFromAttack = true;
                DamageCritter(attacker, target, dispIo);

                if (!wasDeadOrUnconscious && GameSystems.Critter.IsDeadOrUnconscious(target))
                {
                    if (attacker == null || attacker == target || GameSystems.Party.IsInParty(attacker))
                    {
                        if (!GameSystems.Party.IsInParty(target) &&
                            !target.GetCritterFlags().HasFlag(CritterFlag.EXPERIENCE_AWARDED))
                        {
                            GameSystems.D20.Combat.AwardExperienceForKill(attacker, target);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100b6950)]
        public bool IsTrapped(GameObjectBody obj)
        {
            return (obj.type == ObjectType.portal || obj.type == ObjectType.container)
                   && GameSystems.Trap.WillTrigger(obj);
        }

        [TempleDllLocation(0x100b64c0)]
        public bool TryFeint(GameObjectBody attacker, GameObjectBody defender)
        {
            var attackerRoll = Dice.D20.Roll();
            var defenderRoll = Dice.D20.Roll();

            var attackerBonus = BonusList.Default;
            var defenderBonus = BonusList.Default;
            attacker.dispatch1ESkillLevel(SkillId.bluff, ref attackerBonus, defender, 0);
            defender.dispatch1ESkillLevel(SkillId.sense_motive, ref defenderBonus, attacker, 0);

            if (GameSystems.Stat.StatLevelGet(defender, Stat.intelligence) <= 2)
            {
                // A rock is hard to convince that you're attack it
                attackerBonus.AddBonus(-8, 0, 290);
            }
            else if (!GameSystems.Critter.IsCategory(defender, MonsterCategory.humanoid))
            {
                // An ooze might mistake your spasmic movement for a mating ritual
                attackerBonus.AddBonus(-4, 0, 291);
            }

            var defenderBab = defender.DispatchToHitBonusBase();
            defenderBonus.AddBonus(defenderBab, 0, 118);
            var success = attackerRoll + attackerBonus.OverallBonus > defenderRoll + defenderBonus.OverallBonus;
            var mesLineResult = success ? D20CombatMessage.attempt_succeeds : D20CombatMessage.attempt_fails;
            var histId = GameSystems.RollHistory.RollHistoryAddType6OpposedCheck(
                attacker,
                defender,
                attackerRoll,
                defenderRoll,
                attackerBonus,
                defenderBonus,
                153,
                mesLineResult,
                0);
            GameSystems.RollHistory.CreateRollHistoryString /*0x100dfff0*/(histId);
            return success;
        }

        [TempleDllLocation(0x100b9200)]
        public bool IsFlankedBy(GameObjectBody victim, GameObjectBody attacker)
        {
            if (victim == null)
            {
                return false;
            }

            if (!GameSystems.Combat.CanMeleeTarget(attacker, victim))
            {
                return false;
            }

            if (GameSystems.D20.D20QueryWithObject(victim, D20DispatcherKey.QUE_CanBeFlanked, attacker) == 0)
            {
                return false;
            }

            var attackerPos = attacker.GetLocationFull().ToInches2D();
            var victimPos = victim.GetLocationFull().ToInches2D();
            var victimToAttackerDir = Vector2.Normalize(attackerPos - victimPos);

            var enemies = GameSystems.Combat.GetEnemiesCanMelee(victim);
            var cos120deg = MathF.Cos(2.0943952f); // 120°

            foreach (var enemy in enemies)
            {
                if (enemy != attacker)
                {
                    var enemyPos = enemy.GetLocationFull().ToInches2D();
                    var victimToEnemyDir = Vector2.Normalize(enemyPos - victimPos);

                    // This works out to be a 120° wide angle behind the victim
                    if (Vector2.Dot(victimToAttackerDir, victimToEnemyDir) < cos120deg)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x100b9160)]
        public bool HasThreateningCrittersAtLoc(GameObjectBody obj, LocAndOffsets loc)
        {
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != obj && !GameSystems.Combat.AffiliationSame(obj, combatant))
                {
                    if (CanMeleeTargetAtLocation(combatant, obj, loc))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}