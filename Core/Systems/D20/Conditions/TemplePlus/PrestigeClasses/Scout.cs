using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObject;
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
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Ui.PartyCreation.Systems;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Scout
    {
        private static readonly Stat ClassId = Stat.level_scout;

        public const string Skirmish = "Skirmish";
        public static readonly FeatId SkirmishId = (FeatId) ElfHash.Hash(Skirmish);
        public const string BattleFortitude = "Battle Fortitude";
        public static readonly FeatId BattleFortitudeId = (FeatId) ElfHash.Hash(BattleFortitude);
        public const string FastMovement = "Fast Movement Scout";
        public static readonly FeatId FastMovementId = (FeatId) ElfHash.Hash(FastMovement);
        public const string HideInPlainSight = "Hide in Plain Sight Scout";
        public static readonly FeatId HideInPlainSightId = (FeatId) ElfHash.Hash(HideInPlainSight);
        public const string FreeMovement = "Free Movement";
        public static readonly FeatId FreeMovementId = (FeatId) ElfHash.Hash(FreeMovement);
        public const string Blindsight = "Blindsight";
        public static readonly FeatId BlindsightId = (FeatId) ElfHash.Hash(Blindsight);

        private static readonly ImmutableList<SelectableFeat> BonusFeats = new[]
        {
            FeatId.ACROBATIC, FeatId.AGILE, FeatId.ALERTNESS, FeatId.ATHLETIC, FeatId.BLIND_FIGHT,
            FeatId.COMBAT_EXPERTISE, FeatId.DODGE, FeatId.FAR_SHOT, FeatId.GREAT_FORTITUDE,
            FeatId.IMPROVED_INITIATIVE, FeatId.IRON_WILL, FeatId.LIGHTNING_REFLEXES, FeatId.MOBILITY,
            FeatId.POINT_BLANK_SHOT, FeatId.PRECISE_SHOT, FeatId.QUICK_DRAW, FeatId.RAPID_RELOAD,
            FeatId.SHOT_ON_THE_RUN, FeatId.SPRING_ATTACK, FeatId.TRACK,
            FeatId.SKILL_FOCUS_ALCHEMY, FeatId.SKILL_FOCUS_ANIMAL_EMPATHY, FeatId.SKILL_FOCUS_APPRAISE,
            FeatId.SKILL_FOCUS_BALANCE, FeatId.SKILL_FOCUS_BLUFF,
            FeatId.SKILL_FOCUS_CLIMB, FeatId.SKILL_FOCUS_CONCENTRATION, FeatId.SKILL_FOCUS_CRAFT,
            FeatId.SKILL_FOCUS_DECIPHER_SCRIPT, FeatId.SKILL_FOCUS_DIPLOMACY,
            FeatId.SKILL_FOCUS_DISABLE_DEVICE, FeatId.SKILL_FOCUS_DISGUISE, FeatId.SKILL_FOCUS_ESCAPE_ARTIST,
            FeatId.SKILL_FOCUS_FORGERY, FeatId.SKILL_FOCUS_GATHER_INFORMATION,
            FeatId.SKILL_FOCUS_HANDLE_ANIMAL, FeatId.SKILL_FOCUS_HEAL, FeatId.SKILL_FOCUS_HIDE,
            FeatId.SKILL_FOCUS_INNUENDO, FeatId.SKILL_FOCUS_INTIMIDATE, FeatId.SKILL_FOCUS_INTUIT_DIRECTION,
            FeatId.SKILL_FOCUS_JUMP, FeatId.SKILL_FOCUS_KNOWLEDGE, FeatId.SKILL_FOCUS_LISTEN,
            FeatId.SKILL_FOCUS_MOVE_SILENTLY, FeatId.SKILL_FOCUS_OPEN_LOCK,
            FeatId.SKILL_FOCUS_PERFORMANCE, FeatId.SKILL_FOCUS_SLIGHT_OF_HAND, FeatId.SKILL_FOCUS_PROFESSION,
            FeatId.SKILL_FOCUS_READ_LIPS, FeatId.SKILL_FOCUS_RIDE,
            FeatId.SKILL_FOCUS_SCRY, FeatId.SKILL_FOCUS_SEARCH, FeatId.SKILL_FOCUS_SENSE_MOTIVE,
            FeatId.SKILL_FOCUS_SPEAK_LANGUAGE, FeatId.SKILL_FOCUS_SPELLCRAFT, FeatId.SKILL_FOCUS_SPOT,
            FeatId.SKILL_FOCUS_SWIM, FeatId.SKILL_FOCUS_TUMBLE, FeatId.SKILL_FOCUS_USE_MAGIC_DEVICE,
            FeatId.SKILL_FOCUS_USE_ROPE, FeatId.SKILL_FOCUS_SURVIVAL
        }.Select(featId => new SelectableFeat(featId)).ToImmutableList();
        
        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("scout")
        {
            classEnum = Stat.level_scout,
            helpTopic = "TAG_SCOUTS",
            conditionName = "Scout",
            flags = ClassDefinitionFlag.CDF_BaseClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
            hitDice = 8,
            FortitudeSaveProgression = SavingThrowProgressionType.LOW,
            ReflexSaveProgression = SavingThrowProgressionType.HIGH,
            WillSaveProgression = SavingThrowProgressionType.LOW,
            skillPts = 8,
            hasArmoredArcaneCasterFeature = false,
            classSkills = new HashSet<SkillId>
            {
                SkillId.disable_device,
                SkillId.hide,
                SkillId.listen,
                SkillId.move_silently,
                SkillId.search,
                SkillId.sense_motive,
                SkillId.spot,
                SkillId.tumble,
                SkillId.wilderness_lore,
                SkillId.alchemy,
                SkillId.balance,
                SkillId.climb,
                SkillId.craft,
                SkillId.escape_artist,
                SkillId.jump,
                SkillId.knowledge_nature,
                SkillId.ride,
                SkillId.swim,
                SkillId.use_rope,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_THROWING_AXE, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_HANDAXE, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORT_SWORD, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORTBOW, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.TRAPS, 1},
                {SkirmishId, 1},
                {FeatId.UNCANNY_DODGE, 2},
                {BattleFortitudeId, 2},
                {FastMovementId, 3},
                {FeatId.EVASION, 5},
                {HideInPlainSightId, 14},
                {FreeMovementId, 18},
                {BlindsightId, 20},
            }.ToImmutableDictionary(),
            deityClass = Stat.level_ranger,
            IsSelectingFeatsOnLevelUp = critter =>
            {
                var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
                return newLvl % 4 == 0;
            },
            LevelupGetBonusFeats = critter => BonusFeats
        };

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .Build();

        // Scout Specific Feats

        #region Skirmish

        // Global variables for keeping track of the location of the scout at the beginning of the round
        // They do not need to be persistent except during a scout's turn
        private static LocAndOffsets startLocation;

        // Checks for a load greater than light or armor greater than light
        // (to enable skrimish, battle fortitude and fast movement)
        public static bool ScoutEncumberedCheck(GameObjectBody obj)
        {
            // Light armor or no armor
            if (!obj.IsWearingLightArmorOrLess())
            {
                return true;
            }

            // No heavy or medium load
            var HeavyLoad = obj.D20Query(D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy);
            if (HeavyLoad)
            {
                return true;
            }

            var MediumLoad = obj.D20Query(D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium);
            if (MediumLoad)
            {
                return true;
            }

            return false;
        }

        // Calculate the skrimish bonus for the scout
        public static int GetSkirmishACBonus(GameObjectBody obj)
        {
            var scoutLevel = (float) obj.GetStat(ClassId);
            var bonusValue = (int) ((scoutLevel + 1f) / 4f);
            return bonusValue;
        }

        // Damage Bonus:  1- 1d5, 5- 2d6, 9-3d6, 13- 4d6, 17 - 5d6
        public static Dice GetSkirmishDamageDice(GameObjectBody obj)
        {
            var scoutLevel = (float) obj.GetStat(ClassId);
            var bonusValue = (int) ((scoutLevel - 1) / 4f + 1);
            return Dice.D6.WithCount(bonusValue);
        }

        // Determine if skrimish is enabled based on if the scout has moved 10 feet
        public static bool SkirmishEnabled(in DispatcherCallbackArgs evt)
        {
            var distanceMoved = evt.GetConditionArg1();
            return distanceMoved >= 10;
        }

        public static void SkirmishTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (!SkirmishEnabled(in evt))
            {
                return;
            }

            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            // Set the tooltip
            dispIo.Append("Skirmish");
        }

        public static void SkirmishEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (!SkirmishEnabled(in evt))
            {
                return;
            }

            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            // Generate the tooltip
            var tipString = "(" + GetSkirmishDamageDice(evt.objHndCaller);
            var skirmishACBonus = GetSkirmishACBonus(evt.objHndCaller);
            if (skirmishACBonus > 0)
            {
                tipString = tipString + ",+" + skirmishACBonus.ToString() + "AC";
            }

            tipString = tipString + ")";
            dispIo.bdb.AddEntry(ElfHash.Hash("SCOUT_SKIRMISH"), tipString, -2);
        }

        public static void ScoutMovedDistance(in DispatcherCallbackArgs evt)
        {
            // Keep track of how far the scout as moved from their initial position (not total distance moved)
            // The distance needs to location at the beginning of the round needs to be adjusted by the radius (which is in inches)
            var moveDistance = (int) (evt.objHndCaller.DistanceToLocInFeet(startLocation) +
                                      (evt.objHndCaller.GetRadius() / 12f));
            evt.SetConditionArg1(moveDistance);
        }

        public static void SkirmishReset(in DispatcherCallbackArgs evt)
        {
            // Save the initial position for the scout and the distance moved for the round
            startLocation = evt.objHndCaller.GetLocationFull();

            // Zero out the total distance moved from the start position
            evt.SetConditionArg1(0);
        }

        public static void SkirmishAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            // not active, do nothing
            if (!SkirmishEnabled(in evt))
            {
                return;
            }

            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            var bonusValue = GetSkirmishACBonus(evt.objHndCaller);
            if ((bonusValue > 0))
            {
                dispIo.bonlist.AddBonus(bonusValue, 34, "Skirmish"); // Compitence Bonus
            }
        }

        public static void SkirmishDamageBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            // imprecise attacks cannot get skirmish
            var attackFlags = dispIo.attackPacket.flags;
            if ((attackFlags & D20CAF.NO_PRECISION_DAMAGE) != D20CAF.NONE)
            {
                return;
            }

            // Check if skrimish has been enabled
            var skirmishEnabled = SkirmishEnabled(in evt);

            var target = dispIo.attackPacket.victim;
            // Disable if too far away
            if (evt.objHndCaller.DistanceTo(target) > 30)
            {
                skirmishEnabled = false;
            }

            // Disable skirmish if the target can't be seen
            if (!GameSystems.Critter.CanSense(evt.objHndCaller, target))
            {
                skirmishEnabled = false;
            }

            // Check if sneak attack is turned on for criticals and it was a critical hit (this counts for skrimish too)
            var sneakAttackOnCritical = evt.objHndCaller.D20Query("Sneak Attack Critical");
            if (sneakAttackOnCritical)
            {
                if ((attackFlags & D20CAF.CRITICAL) != D20CAF.NONE)
                {
                    skirmishEnabled = true;
                }
            }

            if (!skirmishEnabled)
            {
                return;
            }

            // Check for immunity to skirmish (same as immunity to sneak attack)
            var NoSneakAttack = target.D20Query(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits);
            if (NoSneakAttack)
            {
                return;
            }

            var damage_dice = GetSkirmishDamageDice(evt.objHndCaller);
            dispIo.damage.AddDamageDice(damage_dice, DamageType.Unspecified, 127);
        }
        // Distance Moved, Spare, Spare, Spare

        [FeatCondition(Skirmish)]
        public static readonly ConditionSpec ScoutSkirmish = ConditionSpec.Create("Skirmish", 4)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, SkirmishDamageBonus)
            .AddHandler(DispatcherType.GetAC, SkirmishAcBonus)
            .AddHandler(DispatcherType.BeginRound, SkirmishReset)
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_Critter_Moved, ScoutMovedDistance)
            .AddHandler(DispatcherType.Tooltip, SkirmishTooltip)
            .AddHandler(DispatcherType.EffectTooltip, SkirmishEffectTooltip)
            .Build();

        #endregion

        #region Battle Fortitude

        public static int GetBattleFortitudeBonus(GameObjectBody obj)
        {
            var scoutLevel = obj.GetStat(ClassId);
            if (scoutLevel < 2)
            {
                return 0;
            }
            else if (scoutLevel < 11)
            {
                return 1;
            }
            else if (scoutLevel < 20)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        public static void BattleFortitudeInitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            var bonusValue = GetBattleFortitudeBonus(evt.objHndCaller);
            // Add the Value
            if (bonusValue > 0)
            {
                dispIo.bonlist.AddBonus(bonusValue, 34, "Battle Fortitude"); // Competence bonus to initiative
            }
        }

        public static void BattleFortitudeFortSaveBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            var bonusValue = GetBattleFortitudeBonus(evt.objHndCaller);
            if (bonusValue > 0)
            {
                dispIo.bonlist.AddBonus(bonusValue, 34, "Battle Fortitude"); // Competence bonus
            }
        }

        // Spare, Spare
        [FeatCondition(BattleFortitude)]
        public static readonly ConditionSpec scoutBattleFortitude = ConditionSpec.Create("Battle Fortitude", 2)
            .SetUnique()
            .AddHandler(DispatcherType.InitiativeMod, BattleFortitudeInitBonus)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, BattleFortitudeFortSaveBonus)
            .Build();

        #endregion

        #region Fast Movement Scout

        public static void FastMovementScoutBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            var scoutLevel = evt.objHndCaller.GetStat(ClassId);
            int bonusValue;
            if (scoutLevel < 3)
            {
                bonusValue = 0;
            }
            else if (scoutLevel < 11)
            {
                bonusValue = 10;
            }
            else
            {
                bonusValue = 20;
            }

            // Enhancement bonus to movement
            if (bonusValue > 0)
            {
                dispIo.bonlist.AddBonus(bonusValue, 12, "Fast Movement Scout"); // Enhancement bonus to movement
            }
        }

        // Spare, Spare
        [FeatCondition(FastMovement)]
        public static readonly ConditionSpec FastMovementCondition = ConditionSpec.Create("Fast Movement Scout", 2)
            .SetUnique()
            .AddHandler(DispatcherType.GetMoveSpeed, FastMovementScoutBonus)
            .Build();

        #endregion

        #region Hide in Plain Sight Scout

        public static void HideInPlainSightQueryScout(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            // Must be outdoors
            if (!GameSystems.Map.IsCurrentMapOutdoors())
            {
                return;
            }

            dispIo.return_val = 1;
        }

        // Spare, Spare
        [FeatCondition(HideInPlainSight)]
        public static readonly ConditionSpec HideInPlainSightCondition = ConditionSpec
            .Create("Hide in Plain Sight Scout", 2)
            .SetUnique()
            .AddQueryHandler("Can Hide In Plain Sight", HideInPlainSightQueryScout)
            .Build();

        #endregion

        #region Free Movement Scout

        public static void FreeMovementScout(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Scout must not be encumbered
            if (ScoutEncumberedCheck(evt.objHndCaller))
            {
                return;
            }

            dispIo.return_val = 1;
        }

        // Spare, Spare
        [FeatCondition(FreeMovement)]
        public static readonly ConditionSpec FreeMovementCondition = ConditionSpec.Create("Free Movement", 2)
            .SetUnique()
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, FreeMovementScout)
            .Build();

        #endregion

        #region Blindsight

        // Returns the range of the blindsight ability.  It is queried by the engine.
        public static void ScoutBlindsightRange(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 30;
        }

        // Spare, Spare
        [FeatCondition(Blindsight)]
        public static readonly ConditionSpec BlindsightCondition = ConditionSpec.Create("Blindsight", 2)
            .SetUnique()
            .AddQueryHandler("Blindsight Range", ScoutBlindsightRange)
            .Build();

        #endregion
    }
}