using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.D20
{
    public class D20StatSystem : IGameSystem
    {
        /// <summary>
        /// Use this value as the hit points of a critter to indicate to the stats system that it's HP
        /// will need to be initialized on first access.
        /// </summary>
        public const int UninitializedHitPoints = -65535;

        private static FeatId GetFeatFromStat(Stat stat) => (FeatId) ((int) stat - 1000);

        public D20StatSystem()
        {
            statMes = Tig.FS.ReadMesFile("mes/stat.mes");
            statMesExt = Tig.FS.ReadMesFile("mes/stat_ext.mes");

            statRules = Tig.FS.ReadMesFile("rules/stat.mes");
            statRulesExt = Tig.FS.ReadMesFile("rules/stat_ext.mes");

            statEnum = Tig.FS.ReadMesFile("rules/stat_enum.mes");

            for (var i = 0; i < (int) Stat._count; i++)
            {
                string line;
                if (statMes.TryGetValue(i, out line))
                {
                    statMesStrings[i] = line;
                }

                if (statMesExt.TryGetValue(i, out line))
                {
                    statMesStrings[i] = line;
                }

                if (statEnum.TryGetValue(i, out line))
                {
                    statEnumStrings[i] = line;
                }

                if (statRules.TryGetValue(i, out line))
                {
                    statRulesStrings[i] = line;
                }

                if (statRulesExt.TryGetValue(i, out line))
                {
                    statRulesStrings[i] = line;
                }

                if (statMes.TryGetValue(i + 1000, out line))
                {
                    statShortNameStrings[i] = line;
                }

                // overrun from the extension file
                if (statMesExt.TryGetValue(i + 1000, out line))
                {
                    statShortNameStrings[i] = line;
                }
            }

            foreach (var it in D20ClassSystem.VanillaClasses)
            {
                cannotPickClassStr[(int) it] = statMes[(int) (13100 + it - Stat.level_barbarian)];
            }
        }

        [TempleDllLocation(0x10074950)]
        public string GetStatName(Stat stat)
        {
            if (GetType(stat) == StatType.Feat)
            {
                return GameSystems.Feat.GetFeatName(GetFeatFromStat(stat));
            }

            return statMesStrings[(int) stat];
        }

        [TempleDllLocation(0x10074980)]
        public string GetStatShortName(Stat stat)
        {
            if (GetType(stat) == StatType.Feat)
            {
                return GameSystems.Feat.GetFeatName(GetFeatFromStat(stat));
            }

            return statShortNameStrings[(int) stat];
        }

        [TempleDllLocation(0x10073a10)]
        public string GetStatEnumString(Stat stat)
        {
            return statEnumStrings[(int) stat];
        }

        // not really needed - just used for the python layer (replaced in constants.py) CHAR* GetStatRulesString(Stat stat) {
        [TempleDllLocation(0x10073a20)]
        public string GetStatRulesString(Stat stat)
        {
            return statRulesStrings[(int) stat];
        }

        [TempleDllLocation(0x10073ae0)]
        public string GetClassShortDesc(Stat stat)
        {
            var key = 13000 + stat - Stat.level_barbarian;
            if (stat <= Stat.level_wizard)
                return statMes[key];
            else
                return statMesExt[key];
        }

        [TempleDllLocation(0x10073c20)]
        public string GetAlignmentName(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.NEUTRAL:
                    return statMes[6000];
                case Alignment.LAWFUL:
                    return statMes[6001];
                case Alignment.CHAOTIC:
                    return statMes[6002];
                case Alignment.GOOD:
                    return statMes[6004];
                case Alignment.EVIL:
                    return statMes[6008];
                case Alignment.LAWFUL_GOOD:
                    return statMes[6005];
                case Alignment.CHAOTIC_GOOD:
                    return statMes[6006];
                case Alignment.LAWFUL_EVIL:
                    return statMes[6009];
                case Alignment.CHAOTIC_EVIL:
                    return statMes[6010];
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }

        [TempleDllLocation(0x10073b40)]
        public string GetAlignmentShortDesc(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.NEUTRAL:
                    return statMes[16000];
                case Alignment.LAWFUL:
                    return statMes[16001];
                case Alignment.CHAOTIC:
                    return statMes[16002];
                case Alignment.GOOD:
                    return statMes[16004];
                case Alignment.EVIL:
                    return statMes[16008];
                case Alignment.LAWFUL_GOOD:
                    return statMes[16005];
                case Alignment.CHAOTIC_GOOD:
                    return statMes[16006];
                case Alignment.LAWFUL_EVIL:
                    return statMes[16009];
                case Alignment.CHAOTIC_EVIL:
                    return statMes[16010];
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }

        public const int VANILLA_STAT_COUNT = 288;
        public const int VANILLA_NUM_RACES = 7;

        public string GetRaceName(RaceId race)
        {
            if (statMesExt.TryGetValue(2000 + (int) race, out var line))
            {
                return line;
            }

            if (statMes.TryGetValue(2000 + (int) race, out line))
            {
                return line;
            }

            return null;
        }

        public string GetRaceEnumName(RaceId race)
        {
            if (statEnum.TryGetValue(2000 + (int) race, out var line))
            {
                return line;
            }

            return null;
        }

        [TempleDllLocation(0x10073b70)]
        public bool GetRaceByEnumName(string name, out RaceId race)
        {
            // TODO: TP extensions
            // foreach (var it in mRaceSpecs){
            //     if (it.second.conditionName == raceName){
            //         return it.first;
            //   }
            // }

            // Try the primary races
            for (int i = 0; i < VANILLA_NUM_RACES; i++)
            {
                var raceName = GetRaceEnumName((RaceId) i);
                if (raceName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    race = (RaceId) i;
                    return true;
                }
            }

            // Try the subraces
            int[] subraceEnumIds =
            {
                33, 65, 97, 129,
                34, 66, 98, 130, 162,
                35, 67, 38, 70
            };
            // TODO: I do not believe this is actually correct. Test if it's actually used
            foreach (var subraceId in subraceEnumIds)
            {
                var raceName = GetRaceEnumName((RaceId) subraceId);
                if (raceName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new NotImplementedException();
                    race = (RaceId) subraceId;
                    return true;
                }
            }

            race = default;
            return false;
        }

        [TempleDllLocation(0x10073ab0)]
        public string GetRaceShortDesc(RaceId race)
        {
            if ((int) race <= VANILLA_NUM_RACES)
                return statMes[12000 + (int) race];
            else
                return statMesExt[12000 + (int) race];
        }

        public string GetMonsterSubcategoryName(int monsterSubcat)
        {
            return statMes[8000 + monsterSubcat];
        }

        public string GetMonsterCategoryName(MonsterCategory monsterCat)
        {
            return statMes[7000 + (int) monsterCat];
        }

        public string GetGenderName(int genderId) => GetGenderName((Gender) genderId);

        public string GetGenderName(Gender genderId)
        {
            if (genderId == Gender.Female)
            {
                return statMes[4000];
            }
            else if (genderId == Gender.Male)
            {
                return statMes[4001];
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public string GetCannotPickClassHelp(Stat stat)
        {
            if (stat <= Stat.level_wizard)
            {
                return statMes[13100 + stat - Stat.level_barbarian];
            }

            return statMesExt[20000 + (int) stat];
        }

        public int GetValue(GameObjectBody obj, Stat stat, int statArg = -1)
        {
            switch (GetType(stat))
            {
                case StatType.Abilities:
                    return obj.Dispatch10AbilityScoreLevelGet(stat, null);
                case StatType.Level:
                    return GetLevelStat(obj, stat);
                case StatType.Money:
                    return StatLevelGet(obj, stat);
                case StatType.SpellCasting:
                    return GetSpellCastingStat(obj, stat, statArg);
                case StatType.Psi:
                    return GetPsiStat(obj, stat, statArg);
                case StatType.HitPoints:
                // todo!
                case StatType.Combat:
                // todo!
                case StatType.AbilityMods:
                // todo
                case StatType.Speed:
                //todo
                case StatType.Race:
                //todo
                case StatType.Load:
                // todo
                case StatType.SavingThrows:
                // todo

                default:
                    return StatLevelGet(obj, stat);
            }

            return 0;
        }

        [TempleDllLocation(0x1004dc30)]
        public static int GetModifierForAbilityScore(int abilityScore) => abilityScore / 2 - 5;

        [TempleDllLocation(0x10074CF0)]
        public int ObjStatBaseGet(GameObjectBody obj, Stat stat)
        {
            var statType = GetType(stat);

            switch (statType)
            {
                case StatType.Abilities:
                    return obj.GetInt32(obj_f.critter_abilities_idx, (int) stat);
                case StatType.Level:
                    return GetValue(obj, stat);
                case StatType.HitPoints:
                    return Obj_Get_HP_Max_or_Current_or_SubdualDamage(obj, stat);
                case StatType.Combat:
                    return GetType3StatBase(obj, stat);
                case StatType.Money:
                    return GetMoney(obj, stat);
                case StatType.AbilityMods:
                    return GetModifierForAbilityScore(ObjStatBaseGet(obj, (Stat) (stat - 255)));
                case StatType.Speed:
                    return GetBaseSpeed(obj, stat);
                case StatType.Load:
                    return StatGetBaseLoad(obj, stat);
                case StatType.Feat:
                    return GameSystems.Feat.HasFeatCountByClass(obj, GetFeatFromStat(stat));
                case StatType.Psi:
                    return GetPsiStatBase(obj, stat);
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x10074AD0)]
        private int GetBaseSpeed(GameObjectBody obj, Stat stat)
        {
            var race = StatLevelGet(obj, Stat.race);
            int speedInFeet;
            switch (race)
            {
                case 0:
                case 2:
                case 4:
                case 5:
                    speedInFeet = 30;
                    break;
                case 1:
                case 3:
                case 6:
                    speedInFeet = 20;
                    break;
                default:
                    speedInFeet = (int) stat; // TODO: This seems wrong...
                    break;
            }

            if (stat == Stat.run_speed)
            {
                speedInFeet *= 4;
            }

            return speedInFeet;
        }

        [TempleDllLocation(0x10074350)]
        private int GetMoney(GameObjectBody obj, Stat stat)
        {
            if (obj.type == ObjectType.pc)
            {
                // For player characters, this stat will be shared party money
                switch (stat)
                {
                    case Stat.money:
                        return GameSystems.Party.GetPartyMoney();
                    case Stat.money_pp:
                        GameSystems.Party.GetPartyMoneyCoins(out var platinCoins, out _, out _, out _);
                        return platinCoins;
                    case Stat.money_gp:
                        GameSystems.Party.GetPartyMoneyCoins(out _, out var goldCoins, out _, out _);
                        return goldCoins;
                    case Stat.money_sp:
                        GameSystems.Party.GetPartyMoneyCoins(out _, out _, out var silverCoins, out _);
                        return silverCoins;
                    case Stat.money_cp:
                        GameSystems.Party.GetPartyMoneyCoins(out _, out _, out _, out var copperCoins);
                        return copperCoins;
                    default:
                        return 0;
                }
            }
            else
            {
                switch (stat)
                {
                    case Stat.money:
                        var platinCoins = obj.GetInt32(obj_f.critter_money_idx, 3);
                        var goldCoins = obj.GetInt32(obj_f.critter_money_idx, 2);
                        var silverCoins = obj.GetInt32(obj_f.critter_money_idx, 1);
                        var copperCoins = obj.GetInt32(obj_f.critter_money_idx, 0);
                        return GameSystems.Party.GetCoinWorth(platinCoins, goldCoins, silverCoins, copperCoins);
                    case Stat.money_pp:
                        return obj.GetInt32(obj_f.critter_money_idx, 0);
                    case Stat.money_gp:
                        return obj.GetInt32(obj_f.critter_money_idx, 1);
                    case Stat.money_sp:
                        return obj.GetInt32(obj_f.critter_money_idx, 2);
                    case Stat.money_cp:
                        return obj.GetInt32(obj_f.critter_money_idx, 3);
                    default:
                        return 0;
                }
            }
        }

        private static readonly int[] CarryCapacityForStrength =
        {
            0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 115, 130, 150, 175, 200, 230,
            260, 300, 350, 400, 460, 520, 600, 700, 800, 920, 1040, 1200, 1400
        };

        private static int GetCarryCapacityFromStrength(int strength)
        {
            int capacity;
            if (strength >= 30)
            {
                // TODO: Double check this, might be wrong for strength == 30
                capacity = CarryCapacityForStrength[20 + strength % 10];
                if (strength > 30)
                {
                    for (var times = (strength - 31) / 10 + 1; times > 0; times--)
                    {
                        capacity *= 4;
                    }
                }
            }
            else
            {
                capacity = CarryCapacityForStrength[strength];
            }

            return capacity;
        }

        private EncumbranceType GetLoadFromWeightAndCapacity(int carriedWeight, int carryingCapacity)
        {
            if (carriedWeight > carryingCapacity / 3)
            {
                if (carriedWeight > 2 * (carryingCapacity / 3))
                {
                    return EncumbranceType.HeavyLoad;
                }

                return EncumbranceType.MediumLoad;
            }
            else
            {
                return EncumbranceType.LightLoad;
            }
        }

        [TempleDllLocation(0x10075050)]
        private int StatGetBaseLoad(GameObjectBody obj, Stat stat)
        {
            if (stat == Stat.load)
            {
                var strength = ObjStatBaseGet(obj, Stat.strength);
                var capacity = GetCarryCapacityFromStrength(strength);
                var totalWeight = GameSystems.Item.GetTotalCarriedWeight(obj);
                return (int) GetLoadFromWeightAndCapacity(totalWeight, capacity);
            }
            else
            {
                return 0;
            }
        }

        [TempleDllLocation(0x100745F0)]
        private int StatGetLoad(GameObjectBody obj, Stat stat)
        {
            if (stat == Stat.load)
            {
                var strength = StatLevelGet(obj, Stat.strength);
                var capacity = GetCarryCapacityFromStrength(strength);

                var size = StatLevelGet(obj, Stat.size);
                capacity = AdjustCarryingCapacityForSize(capacity, (SizeCategory) size);

                var totalWeight = GameSystems.Item.GetTotalCarriedWeight(obj);
                return (int) GetLoadFromWeightAndCapacity(totalWeight, capacity);
            }
            else
            {
                return 0;
            }
        }

        // See: http://www.d20srd.org/srd/carryingCapacity.htm
        private static int AdjustCarryingCapacityForSize(int capacity, SizeCategory size)
        {
            if (size > SizeCategory.Medium)
            {
                for (var i = (int) size - 5; i > 0; i--)
                {
                    capacity *= 2;
                }
            }
            else if (size == SizeCategory.Small)
            {
                // Small creature
                capacity = capacity * 3 / 4;
            }
            else if (size < SizeCategory.Small)
            {
                var i = 4 - size;
                do
                {
                    --i;
                    capacity /= 2;
                } while (i > 0);
            }

            return capacity;
        }

        [TempleDllLocation(0x10074010)]
        private int Obj_Get_HP_Max_or_Current_or_SubdualDamage(GameObjectBody obj, Stat stat)
        {
            if (obj.IsCritter() && obj.GetInt32(obj_f.hp_pts) == UninitializedHitPoints)
            {
                GameSystems.Critter.GenerateHp(obj);
            }

            switch (stat)
            {
                case Stat.hp_max:
                    return obj.GetInt32(obj_f.hp_pts);
                case Stat.hp_current:
                    return obj.GetInt32(obj_f.hp_pts) - obj.GetInt32(obj_f.hp_damage);
                case Stat.subdual_damage:
                    return obj.GetInt32(obj_f.critter_subdual_damage);
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x10074800)]
        public int StatLevelGet(GameObjectBody obj, Stat stat)
        {
            switch (GetType(stat))
            {
                case StatType.SpellCasting:
                    return GetValue(obj, stat);
                case StatType.SavingThrows:
                    switch (stat)
                    {
                        case Stat.save_reflexes:
                            return Dispatch13SavingThrow(obj, SavingThrowType.Reflex, null);
                        case Stat.save_fortitude:
                            return Dispatch13SavingThrow(obj, SavingThrowType.Fortitude, null);
                        case Stat.save_willpower:
                            return Dispatch13SavingThrow(obj, SavingThrowType.Will, null);
                        default:
                            return 0;
                    }

                case StatType.Abilities:
                    return GetValue(obj, stat);
                case StatType.Combat:
                    if (stat == Stat.race || stat == Stat.subrace)
                    {
                        return GetType3StatBase(obj, stat);
                    }

                    return GetCombatValue(obj, stat);
                case StatType.HitPoints:
                    return Get_HP_Base_or_Current__or_Subdual_Damage(obj, stat);
                case StatType.Speed:
                    return GetSpeed(obj, stat);
                case StatType.AbilityMods:
                    return GetAbilityMods(obj, stat);
                case StatType.Load:
                    return StatGetLoad(obj, stat);
                case StatType.Money:
                    return ObjStatBaseGet(obj, stat);
                case StatType.Level:
                    return GetValue(obj, stat);
                case StatType.Psi:
                    return GetPsiStat(obj, stat);
                default:
                    return ObjStatBaseGet(obj, stat);
            }
        }

        [TempleDllLocation(0x10074580)]
        private int GetAbilityMods(GameObjectBody obj, Stat stat)
        {
            // Get actual modifier value
            var statValue = GameSystems.Stat.ObjStatBaseGet(obj, stat - 255);
            var modifier = GetModifierForAbilityScore(statValue);

            // Adjust the dexterity modifier based on medium/heavy load
            if (stat == Stat.dex_mod)
            {
                var loadCategory = GameSystems.Stat.StatLevelGet(obj, Stat.load);
                if (loadCategory == 2)
                {
                    if (modifier > 3)
                    {
                        modifier = 3;
                    }
                }
                else if (loadCategory == 3)
                {
                    if (modifier > 1)
                    {
                        modifier = 1;
                    }
                }
            }

            return modifier;
        }

        [TempleDllLocation(0x100742A0)]
        private int GetSpeed(GameObjectBody obj, Stat stat)
        {
            var baseSpeed = ObjStatBaseGet(obj, stat);
            if (stat == Stat.run_speed)
            {
                if (GameSystems.Feat.HasFeat(obj, FeatId.RUN))
                {
                    baseSpeed = 5 * (baseSpeed / 4);
                }
            }

            // Adjust the speed for heavy or medium load
            var loadCategory = StatLevelGet(obj, Stat.load);
            if (loadCategory == 3)
            {
                if (stat == Stat.run_speed)
                {
                    return 9 * (baseSpeed / 4) / 4;
                }

                return 3 * baseSpeed / 4;
            }
            else if (loadCategory == 2)
            {
                return 3 * baseSpeed / 4;
            }

            return baseSpeed;
        }

        [TempleDllLocation(0x10073f30)]
        private int Get_HP_Base_or_Current__or_Subdual_Damage(GameObjectBody obj, Stat stat)
        {
            switch (stat)
            {
                case Stat.hp_max:
                    if (!obj.IsCritter())
                        return ObjStatBaseGet(obj, Stat.hp_max);
                    else
                        return Dispatch26hGetMaxHP(obj, null);
                case Stat.hp_current:
                    if (!obj.IsCritter())
                        return ObjStatBaseGet(obj, Stat.hp_current);
                    else
                        return Dispatch25CurrentHP(obj, null);
                case Stat.subdual_damage:
                    return ObjStatBaseGet(obj, Stat.subdual_damage);
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x100740C0)]
        private int GetCombatValue(GameObjectBody obj, Stat stat)
        {
            switch (stat)
            {
                case Stat.race:
                case Stat.gender:
                case Stat.height:
                case Stat.weight:
                case Stat.size:
                case Stat.alignment:
                case Stat.deity:
                    return GetType3StatBase(obj, stat);
                case Stat.ac:
                    return GetAC(obj, null);
                case Stat.attack_bonus:
                case Stat.melee_attack_bonus:
                    return Dispatch16GetToHitBonus(obj, null);
                case Stat.ranged_attack_bonus:
                    var dispIo = DispIoAttackBonus.Default;
                    dispIo.attackPacket.flags |= D20CAF.RANGED;
                    return Dispatch16GetToHitBonus(obj, dispIo);
                case Stat.damage_bonus:
                    return StatLevelGet(obj, Stat.str_mod);
                case Stat.experience:
                    return obj.GetInt32(obj_f.critter_experience);
                case Stat.domain_1:
                    return obj.GetInt32(obj_f.critter_domain_1);
                case Stat.domain_2:
                    return obj.GetInt32(obj_f.critter_domain_2);
                default:
                    return 0;
            }
        }

        public int GetLevelStat(GameObjectBody obj, Stat stat)
        {
            var lvlArr = obj.GetInt32Array(obj_f.critter_level_idx);
            var numItems = lvlArr.Count;
            var result = 0;
            // get the overall level (not accounting for negative levels yet!)
            if (stat == Stat.level)
            {
                for (var i = 0; i < numItems; i++)
                {
                    var lvl = lvlArr[i];
                    if (GetType((Stat) lvl) == StatType.Level)
                        result++;
                }
            }
            else
            {
                for (var i = 0; i < numItems; i++)
                {
                    if (lvlArr[i] == (int) stat)
                        result++;
                }
            }

            return result;
        }

        public int GetSpellCastingStat(GameObjectBody handle, Stat stat, int statArg)
        {
            int ret = 0;

            if (stat == Stat.caster_level & statArg != -1)
            {
                Stat statCheck = (Stat) statArg;
                ret = GameSystems.Critter.GetCasterLevelForClass(handle, statCheck);
            }
            else if (stat >= Stat.caster_level_barbarian && stat <= Stat.caster_level_wizard)
            {
                // Convert stat from Stat.caster_level_X to Stat.level_X since the function  GetCasterLevelForClass talkes the later
                Stat statCheck = (Stat) ((int) stat - (int) Stat.caster_level_barbarian + (int) Stat.level_barbarian);
                ret = GameSystems.Critter.GetCasterLevelForClass(handle, statCheck);
            }
            else if (stat == Stat.spell_list_level && statArg != -1)
            {
                ret = StatLevelGet(handle, (Stat) statArg) +
                      GameSystems.Critter.GetSpellListLevelExtension(handle, (Stat) statArg);
            }

            return ret;
        }

        public int GetPsiStat(GameObjectBody handle, Stat stat, int statArg = -1)
        {
            if (stat == Stat.psi_points_max)
            {
                return GameSystems.D20.D20QueryPython(handle, "Max Psi");
            }

            if (stat == Stat.psi_points_cur)
            {
                return GameSystems.D20.D20QueryPython(handle, "Current Psi");
            }

            return 0;
        }

        public int GetPsiStatBase(GameObjectBody handle, Stat stat, int statArg = -1)
        {
            if (stat == Stat.psi_points_max)
            {
                return GameSystems.D20.D20QueryPython(handle, "Base Max Psi");
            }

            if (stat == Stat.psi_points_cur)
            {
                return GameSystems.D20.D20QueryPython(handle, "Current Psi");
            }

            return 0;
        }

        [TempleDllLocation(0x10074B30)]
        public int GetType3StatBase(GameObjectBody obj, Stat stat)
        {
            switch (stat)
            {
                case Stat.weight:
                    return obj.GetInt32(obj_f.critter_weight);
                case Stat.height:
                    return obj.GetInt32(obj_f.critter_height);
                case Stat.deity:
                    return obj.GetInt32(obj_f.critter_deity);
                case Stat.race:
                    return obj.GetInt32(obj_f.critter_race) & 0x1F;
                case Stat.subrace:
                    return obj.GetInt32(obj_f.critter_race) >> 5; // vanilla didn't bitshift
                case Stat.gender:
                    return obj.GetInt32(obj_f.critter_gender);
                case Stat.size:
                    return (int) DispatchGetSizeCategory(obj);
                case Stat.alignment:
                    return obj.GetInt32(obj_f.critter_alignment);
                case Stat.experience:
                    return obj.GetInt32(obj_f.critter_experience);
                case Stat.attack_bonus:
                    return GameSystems.Critter.GetBaseAttackBonus(obj);
                case Stat.melee_attack_bonus:
                    return GameSystems.Critter.GetBaseAttackBonus(obj) +
                           GetModifierForAbilityScore(StatLevelGet(obj, Stat.strength));
                case Stat.ranged_attack_bonus:
                    return GameSystems.Critter.GetBaseAttackBonus(obj) +
                           GetModifierForAbilityScore(StatLevelGet(obj, Stat.dexterity));
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x1004d690)]
        public SizeCategory DispatchGetSizeCategory(GameObjectBody obj)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }


            var dispIo = DispIoD20Query.Default;
            dispIo.return_val = obj.GetInt32(obj_f.size);

            dispatcher.Process(DispatcherType.GetSizeCategory, D20DispatcherKey.NONE, dispIo);

            return (SizeCategory) dispIo.return_val;
        }

        [TempleDllLocation(0x1004a8f0)]
        public bool AlignmentsUnopposed(Alignment a, Alignment b, bool strictCheck = false)
        {
            if (Globals.Config.laxRules && Globals.Config.disableAlignmentRestrictions && !strictCheck)
                return true;

            switch (a ^ b)
            {
                // XOR operation
                // example:
                // LAWFUL_GOOD ^ LAWFUL_EVIL      . GOOD | EVIL . no
                // LAWFUL_GOOD ^ LAWFUL_NEUTRAL   . GOOD        . ok
                case Alignment.LAWFUL:
                case Alignment.CHAOTIC:
                case Alignment.GOOD:
                case Alignment.EVIL:

                    return true;
                default:
                    return a == b;
            }
        }

        private Dictionary<int, string> statRules;
        private Dictionary<int, string> statRulesExt;
        private Dictionary<int, string> statMes;
        private Dictionary<int, string> statMesExt;
        private Dictionary<int, string> statEnum;

        [TempleDllLocation(0x10AAE8C8)]
        private string[] statMesStrings = new string[(int) Stat._count];

        [TempleDllLocation(0x10AAE418)]
        private string[] statEnumStrings = new string[(int) Stat._count];

        [TempleDllLocation(0x118CE080)]
        private string[] statRulesStrings = new string[(int) Stat._count];

        private string[] statShortNameStrings = new string[(int) Stat._count];
        private Dictionary<int, string> cannotPickClassStr = new Dictionary<int, string>();

        private static StatType GetType(Stat stat)
        {
            if ((int) stat >= 1000 & (int) stat <= 1999)
            {
                return StatType.Feat;
            }
            else if ((int) stat >= 2000 && (int) stat <= 2999)
            {
                return StatType.Race;
            }

            switch (stat)
            {
                case Stat.strength:
                case Stat.dexterity:
                case Stat.constitution:
                case Stat.intelligence:
                case Stat.wisdom:
                case Stat.charisma:
                    return StatType.Abilities;

                case Stat.str_mod:
                case Stat.dex_mod:
                case Stat.con_mod:
                case Stat.int_mod:
                case Stat.wis_mod:
                case Stat.cha_mod:
                    return
                        StatType
                            .AbilityMods; // largely does (StatLevelGet-10)/ 2 with special casing for dex regarding load

                case Stat.level:
                case Stat.level_barbarian:
                case Stat.level_bard:
                case Stat.level_cleric:
                case Stat.level_druid:
                case Stat.level_fighter:
                case Stat.level_monk:
                case Stat.level_paladin:
                case Stat.level_ranger:
                case Stat.level_rogue:
                case Stat.level_sorcerer:
                case Stat.level_wizard:
                case Stat.level_arcane_archer:
                case Stat.level_arcane_trickster:
                case Stat.level_archmage:
                case Stat.level_assassin:
                case Stat.level_blackguard:
                case Stat.level_dragon_disciple:
                case Stat.level_duelist:
                case Stat.level_dwarven_defender:
                case Stat.level_eldritch_knight:
                case Stat.level_hierophant:
                case Stat.level_horizon_walker:
                case Stat.level_loremaster:
                case Stat.level_mystic_theurge:
                case Stat.level_shadowdancer:
                case Stat.level_thaumaturgist:

                case Stat.level_warlock:
                case Stat.level_favored_soul:
                case Stat.level_red_avenger:
                case Stat.level_iaijutsu_master:
                case Stat.level_sacred_fist:
                case Stat.level_stormlord:
                case Stat.level_elemental_savant:
                case Stat.level_blood_magus:
                case Stat.level_beastmaster:
                case Stat.level_cryokineticist:
                case Stat.level_frost_mage:
                case Stat.level_artificer:
                case Stat.level_abjurant_champion:
                case Stat.level_scout:
                case Stat.level_warmage:
                case Stat.level_beguilers:

                case Stat.level_psion:
                case Stat.level_psychic_warrior:
                case Stat.level_soulknife:
                case Stat.level_wilder:
                case Stat.level_cerebmancer:
                case Stat.level_elocator:
                case Stat.level_metamind:
                case Stat.level_psion_uncarnate:
                case Stat.level_psionic_fist:
                case Stat.level_pyrokineticist:
                case Stat.level_slayer:
                case Stat.level_thrallherd:
                case Stat.level_war_mind:

                case Stat.level_crusader:
                case Stat.level_swordsage:
                case Stat.level_warblade:
                case Stat.level_bloodclaw_master:
                case Stat.level_bloodstorm_blade:
                case Stat.level_deepstone_sentinel:
                case Stat.level_eternal_blade:
                case Stat.level_jade_phoenix_mage:
                case Stat.level_master_of_nine:
                case Stat.level_ruby_knight_vindicator:
                case Stat.level_shadow_sun_ninja:

                    return StatType.Level;

                case Stat.money:
                case Stat.money_pp:
                case Stat.money_gp:
                case Stat.money_ep:
                case Stat.money_sp:
                case Stat.money_cp:
                    return StatType.Money;

                case Stat.save_reflexes:
                case Stat.save_fortitude:
                case Stat.save_willpower:
                    return StatType.SavingThrows; // does a dispatch for saving throw

                case Stat.subdual_damage:
                case Stat.hp_max:
                case Stat.hp_current:
                    return StatType.HitPoints;

                case Stat.race:
                case Stat.gender:
                case Stat.height:
                case Stat.weight:
                case Stat.size:
                case Stat.experience:
                case Stat.alignment:
                case Stat.deity:
                case Stat.domain_1:
                case Stat.domain_2:
                case Stat.ac:
                case Stat.attack_bonus:
                case Stat.damage_bonus:
                case Stat.subrace:
                case Stat.melee_attack_bonus:
                case Stat.ranged_attack_bonus:
                    return StatType.Combat;

                case Stat.movement_speed:
                case Stat.run_speed:
                    return StatType.Speed; // regards load

                case Stat.load:
                    return StatType.Load;

                case Stat.caster_level:
                case Stat.caster_level_barbarian:
                case Stat.caster_level_bard:
                case Stat.caster_level_cleric:
                case Stat.caster_level_druid:
                case Stat.caster_level_fighter:
                case Stat.caster_level_monk:
                case Stat.caster_level_paladin:
                case Stat.caster_level_ranger:
                case Stat.caster_level_rogue:
                case Stat.caster_level_sorcerer:
                case Stat.caster_level_wizard:
                case Stat.spell_list_level:
                    return StatType.SpellCasting;

                case Stat.psi_points_max:
                case Stat.psi_points_cur:
                    return StatType.Psi;

                default:
                    return StatType.Other;
            }

            // the following are unimplemented

            //case Stat.age:
            //case Stat.category:
            //case Stat.alignment_choice:
            //
            //
            //case Stat.initiative_bonus:
            //
            //case Stat.carried_weight:

            //case Stat.favored_enemies:
            //case Stat.known_spells:
            //case Stat.memorized_spells:
            //case Stat.spells_per_day:
            //case Stat.school_specialization:
            //case Stat.school_prohibited:
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x1004E870)]
        public static int Dispatch13SavingThrow(GameObjectBody obj, SavingThrowType saveType,
            DispIoSavingThrow dispIo)
        {
            var dispatcherKey = GetDispatcherKeyForSavingThrow(saveType);
            return DispatchSavingThrow(obj, dispIo, DispatcherType.SaveThrowLevel, dispatcherKey);
        }

        [TempleDllLocation(0x1004e8a0)]
        public static int Dispatch14SavingThrowResistance(GameObjectBody obj, SavingThrowType saveType,
            DispIoSavingThrow dispIo)
        {
            var dispatcherKey = GetDispatcherKeyForSavingThrow(saveType);
            return DispatchSavingThrow(obj, dispIo, DispatcherType.SaveThrowSpellResistanceBonus, dispatcherKey);
        }

        [TempleDllLocation(0x1004e8d0)]
        public static int Dispatch40SavingThrow(GameObjectBody critter, SavingThrowType saveType, DispIoSavingThrow dispIo)
        {
            var dispatcherKey = GetDispatcherKeyForSavingThrow(saveType);
            return DispatchSavingThrow(critter, dispIo, DispatcherType.CountersongSaveThrow, dispatcherKey);
        }

        private static D20DispatcherKey GetDispatcherKeyForSavingThrow(SavingThrowType saveType)
        {
            D20DispatcherKey dispatcherKey;
            switch (saveType)
            {
                case SavingThrowType.Fortitude:
                    dispatcherKey = D20DispatcherKey.SAVE_FORTITUDE;
                    break;
                case SavingThrowType.Reflex:
                    dispatcherKey = D20DispatcherKey.SAVE_REFLEX;
                    break;
                case SavingThrowType.Will:
                    dispatcherKey = D20DispatcherKey.SAVE_WILL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(saveType), saveType, null);
            }

            return dispatcherKey;
        }


        [TempleDllLocation(0x1004eb30)]
        private int Dispatch26hGetMaxHP(GameObjectBody obj, DispIoBonusList bonusList)
        {
            return obj.DispatchForCritter(bonusList, DispatcherType.MaxHP, 0);
        }

        [TempleDllLocation(0x1004eb10)]
        private int Dispatch25CurrentHP(GameObjectBody obj, DispIoBonusList bonusList)
        {
            return obj.DispatchForCritter(bonusList, DispatcherType.CurrentHP, 0);
        }

        // TODO This does not belong here
        [TempleDllLocation(0x1004e900)]
        public int GetAC(GameObjectBody attacker, DispIoAttackBonus attackBonus)
        {
            return DispatchAttackBonus(attacker, null, ref attackBonus, DispatcherType.GetAC, 0);
        }

        // TODO This does not belong here
        [TempleDllLocation(0x1004e970)]
        public int Dispatch16GetToHitBonus(GameObjectBody attacker, DispIoAttackBonus attackBonus)
        {
            return DispatchAttackBonus(attacker, null, ref attackBonus, DispatcherType.ToHitBonus2, 0);
        }

        [TempleDllLocation(0x1004dec0)]
        public int DispatchAttackBonus(GameObjectBody attacker, GameObjectBody victim, ref DispIoAttackBonus dispIo,
            DispatcherType dispType, D20DispatcherKey key)
        {
            var dispatcher = attacker.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            if (dispIo == null)
            {
                dispIo = DispIoAttackBonus.Default;
                dispIo.attackPacket.attacker = attacker;
                dispIo.attackPacket.victim = victim;
                dispIo.attackPacket.d20ActnType = (D20ActionType) dispType; //  that's the original code, jones...
                dispIo.attackPacket.ammoItem = null;
                dispIo.attackPacket.weaponUsed = null;
            }

            dispatcher.Process(dispType, key, dispIo);

            return dispIo.bonlist.OverallBonus;
        }

        [TempleDllLocation(0x1004DDF0)]
        private static int DispatchSavingThrow(GameObjectBody obj,
            DispIoSavingThrow dispIo,
            DispatcherType dispatcherType,
            D20DispatcherKey saveType)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            if (!obj.IsCritter())
            {
                return 0;
            }

            if (dispIo == null)
            {
                dispIo = DispIoSavingThrow.Default;
            }

            dispatcher.Process(dispatcherType, saveType, dispIo);

            return dispIo.bonlist.OverallBonus;
        }

        /// <summary>
        /// This is called during initialization of proto objects, but previously checked that it was
        /// not called for protos (which was actually true during creation). Probably a pointless exercise.
        /// </summary>
        [TempleDllLocation(0x100739d0)]
        public static void SetDefaultPlayerHP(GameObjectBody obj)
        {
            if (!obj.IsProto() && obj.IsPC())
            {
                GameSystems.Critter.GenerateHp(obj);
            }
        }

        [TempleDllLocation(0x10074e10)]
        public int SetBasicStat(GameObjectBody obj, Stat stat, int value)
        {
            // Handle read-only stats
            if (stat >= Stat.str_mod && stat <= Stat.carried_weight
                || stat == Stat.level || stat == Stat.hp_current || stat == Stat.money
                || stat == Stat.load || stat > Stat.subrace && stat <= Stat.ranged_attack_bonus)
            {
                return ObjStatBaseGet(obj, stat);
            }

            switch (GetType(stat))
            {
                case StatType.Abilities:
                    obj.SetInt32(obj_f.critter_abilities_idx, (int) stat, value);
                    break;
                case StatType.HitPoints:
                    if (stat == Stat.hp_max)
                    {
                        obj.SetInt32(obj_f.hp_pts, value);
                    }

                    break;
                case StatType.Combat:
                    switch (stat)
                    {
                        case Stat.experience:
                            obj.SetInt32(obj_f.critter_experience, value);
                            break;
                        case Stat.race:
                        case Stat.subrace:
                            // TODO: This seems wrong somehow
                            obj.SetInt32(obj_f.critter_race, value);
                            break;
                        case Stat.gender:
                            obj.SetInt32(obj_f.critter_gender, value);
                            break;
                        case Stat.alignment:
                            obj.SetInt32(obj_f.critter_alignment, value);
                            break;
                        case Stat.size:
                            obj.SetInt32(obj_f.size, value);
                            break;
                        case Stat.deity:
                            obj.SetInt32(obj_f.critter_deity, value);
                            break;
                    }

                    break;
                case StatType.Money:
                    SetNpcMoney(obj, stat, value);
                    break;
            }

            return ObjStatBaseGet(obj, stat);
        }

        private void SetNpcMoney(GameObjectBody obj, Stat stat, int value)
        {
            if (obj.IsNPC())
            {
                switch (stat)
                {
                    case Stat.money_pp:
                        obj.SetInt32(obj_f.critter_money_idx, 0, value);
                        break;
                    case Stat.money_gp:
                        obj.SetInt32(obj_f.critter_money_idx, 1, value);
                        break;
                    case Stat.money_sp:
                        obj.SetInt32(obj_f.critter_money_idx, 2, value);
                        break;
                    case Stat.money_cp:
                        obj.SetInt32(obj_f.critter_money_idx, 3, value);
                        break;
                }
            }
        }

        [TempleDllLocation(0x1001DC20)]
        public int GetCurrentHP(GameObjectBody obj)
        {
            return StatLevelGet(obj, Stat.hp_current);
        }

        // TODO Does not belong here
        [TempleDllLocation(0x100EBB20)]
        public int GetCarryingCapacityByLoad(int strength, EncumbranceType currentLoad)
        {
            var result = 0;
            if (strength <= 0)
            {
                switch (currentLoad)
                {
                    case EncumbranceType.LightLoad:
                        return 0;
                    case EncumbranceType.MediumLoad:
                        return 1;
                    case EncumbranceType.HeavyLoad:
                        return 2;
                }
            }

            if (strength >= 29)
            {
                result = 4 * GetCarryingCapacityByLoad(strength - 10, currentLoad);
            }
            else
            {
                switch (currentLoad)
                {
                    case EncumbranceType.LightLoad:
                        result = CarryingCapacityLightLoad[strength];
                        break;
                    case EncumbranceType.MediumLoad:
                        result = CarryingCapacityMediumLoad[strength];
                        break;
                    case EncumbranceType.HeavyLoad:
                        result = CarryingCapacityHeavyLoad[strength];
                        break;
                }
            }

            return result;
        }

        [TempleDllLocation(0x102E4904)]
        private static readonly int[] CarryingCapacityLightLoad =
        {
            0, 3, 6, 10, 13, 16, 20, 23, 26, 30, 33, 38, 43, 50, 58, 66, 76, 86,
            100, 116, 133, 153, 173, 200, 233, 266, 306, 346, 400, 466
        };

        [TempleDllLocation(0x102E497C)]
        private static readonly int[] CarryingCapacityMediumLoad =
        {
            0, 6, 13, 20, 26, 33, 40, 46, 53, 60, 66, 76, 86, 100, 116, 133, 153,
            173, 200, 233, 266, 306, 346, 400, 466, 533, 613, 693, 800, 933
        };

        [TempleDllLocation(0x102E49F4)]
        private static readonly int[] CarryingCapacityHeavyLoad =
        {
            0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 115, 130, 150, 175, 200,
            230, 260, 300, 350, 400, 460, 520, 600, 700, 800, 920, 1040, 1200, 1400
        };

    }

    public enum EncumbranceType
    {
        LightLoad = 1,
        MediumLoad = 2,
        HeavyLoad = 3
    }

    public static class D20StatExtensions
    {
        [TempleDllLocation(0x1004e810)]
        public static int GetBaseStat(this GameObjectBody obj, Stat stat)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher != null)
            {
                return obj.DispatchForCritter(
                    null, DispatcherType.StatBaseGet, (D20DispatcherKey) (1 + (int) stat)
                );
            }

            return GameSystems.Stat.ObjStatBaseGet(obj, stat);
        }

        public static int GetStat(this GameObjectBody obj, Stat stat) => GameSystems.Stat.StatLevelGet(obj, stat);
    }
}