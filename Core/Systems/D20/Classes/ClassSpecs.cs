using System.Collections.Generic;
using System.Collections.Immutable;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Feats;

namespace SpicyTemple.Core.Systems.D20.Classes
{
    public static class ClassSpecs
    {
        public static readonly IImmutableList<D20ClassSpec> Specs = ImmutableList.Create(
            CreateBarbarian(),
            CreateBard(),
            CreateCleric(),
            CreateDruid(),
            CreateFighter(),
            CreateMonk(),
            CreatePaladin(),
            CreateRanger(),
            CreateRogue(),
            CreateSorcerer(),
            CreateWizard(),
            CreateArcaneArcher(),
            CreateArcaneTrickster(),
            CreateArchmage(),
            CreateAssassin(),
            CreateBlackguard(),
            CreateDuelist(),
            CreateDwarvenDefender(),
            CreateEldritchKnight(),
            CreateMysticTheurge(),
            CreateFavoredSoul(),
            CreateScout(),
            CreateWarmage(),
            CreateBeguiler()
        );

        public static D20ClassSpec CreateBarbarian()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_barbarian,
                helpTopic = "TAG_BARBARIANS",
                conditionName = "Barbarian",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 12,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 4,
                hasArmoredArcaneCasterFeature = false,
                classSkills = new HashSet<SkillId>
                {
                    SkillId.intimidate,
                    SkillId.listen,
                    SkillId.wilderness_lore,
                    SkillId.alchemy,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.handle_animal,
                    SkillId.jump,
                    SkillId.ride,
                    SkillId.swim,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.BARBARIAN_RAGE, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                    {FeatId.FAST_MOVEMENT, 1},
                    {FeatId.UNCANNY_DODGE, 2},
                    {FeatId.IMPROVED_UNCANNY_DODGE, 5},
                    {FeatId.GREATER_RAGE, 11},
                    {FeatId.INDOMITABLE_WILL, 14},
                    {FeatId.TIRELESS_RAGE, 17},
                    {FeatId.MIGHTY_RAGE, 20},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateBard()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_bard,
                helpTopic = "TAG_BARDS",
                conditionName = "Bard",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 6,
                spellListType = SpellListType.Bardic,
                hasArmoredArcaneCasterFeature = true,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(2),
                    [2] = ImmutableList.Create(3, 0),
                    [3] = ImmutableList.Create(3, 1),
                    [4] = ImmutableList.Create(3, 2, 0),
                    [5] = ImmutableList.Create(3, 3, 1),
                    [6] = ImmutableList.Create(3, 3, 2),
                    [7] = ImmutableList.Create(3, 3, 2, 0),
                    [8] = ImmutableList.Create(3, 3, 3, 1),
                    [9] = ImmutableList.Create(3, 3, 3, 2),
                    [10] = ImmutableList.Create(3, 3, 3, 2, 0),
                    [11] = ImmutableList.Create(3, 3, 3, 3, 1),
                    [12] = ImmutableList.Create(3, 3, 3, 3, 2),
                    [13] = ImmutableList.Create(3, 3, 3, 3, 2, 0),
                    [14] = ImmutableList.Create(4, 3, 3, 3, 3, 1),
                    [15] = ImmutableList.Create(4, 4, 3, 3, 3, 2),
                    [16] = ImmutableList.Create(4, 4, 4, 3, 3, 2, 0),
                    [17] = ImmutableList.Create(4, 4, 4, 4, 3, 3, 1),
                    [18] = ImmutableList.Create(4, 4, 4, 4, 4, 3, 2),
                    [19] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 3),
                    [20] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.appraise,
                    SkillId.bluff,
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.gather_information,
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.pick_pocket,
                    SkillId.sense_motive,
                    SkillId.spellcraft,
                    SkillId.tumble,
                    SkillId.use_magic_device,
                    SkillId.perform,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.disguise,
                    SkillId.escape_artist,
                    SkillId.jump,
                    SkillId.knowledge_nature,
                    SkillId.knowledge_all,
                    SkillId.profession,
                    SkillId.swim,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.BARDIC_MUSIC, 1},
                    {FeatId.BARDIC_KNOWLEDGE, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_BARD, 1},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateCleric()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_cleric,
                helpTopic = "TAG_CLERICS",
                conditionName = "Cleric",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 8,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                spellListType = SpellListType.Clerical,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Divine,
                spellCastingConditionName = null,
                spellStat = Stat.wisdom,
                spellDcStat = Stat.wisdom,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(3, 1),
                    [2] = ImmutableList.Create(4, 2),
                    [3] = ImmutableList.Create(4, 2, 1),
                    [4] = ImmutableList.Create(5, 3, 2),
                    [5] = ImmutableList.Create(5, 3, 2, 1),
                    [6] = ImmutableList.Create(5, 3, 3, 2),
                    [7] = ImmutableList.Create(6, 4, 3, 2, 1),
                    [8] = ImmutableList.Create(6, 4, 3, 3, 2),
                    [9] = ImmutableList.Create(6, 4, 4, 3, 2, 1),
                    [10] = ImmutableList.Create(6, 4, 4, 3, 3, 2),
                    [11] = ImmutableList.Create(6, 5, 4, 4, 3, 2, 1),
                    [12] = ImmutableList.Create(6, 5, 4, 4, 3, 3, 2),
                    [13] = ImmutableList.Create(6, 5, 5, 4, 4, 3, 2, 1),
                    [14] = ImmutableList.Create(6, 5, 5, 4, 4, 3, 3, 2),
                    [15] = ImmutableList.Create(6, 5, 5, 5, 4, 4, 3, 2, 1),
                    [16] = ImmutableList.Create(6, 5, 5, 5, 4, 4, 3, 3, 2),
                    [17] = ImmutableList.Create(6, 5, 5, 5, 5, 4, 4, 3, 2, 1),
                    [18] = ImmutableList.Create(6, 5, 5, 5, 5, 4, 4, 3, 3, 2),
                    [19] = ImmutableList.Create(6, 5, 5, 5, 5, 5, 4, 4, 3, 3),
                    [20] = ImmutableList.Create(6, 5, 5, 5, 5, 5, 4, 4, 4, 4)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.heal,
                    SkillId.spellcraft,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.knowledge_arcana,
                    SkillId.knowledge_religion,
                    SkillId.profession,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.DOMAIN_POWER, 1},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateDruid()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_druid,
                helpTopic = "TAG_DRUIDS",
                conditionName = "Druid",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 8,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 4,
                spellListType = SpellListType.Druidic,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Divine,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(3, 1),
                    [2] = ImmutableList.Create(4, 2),
                    [3] = ImmutableList.Create(4, 2, 1),
                    [4] = ImmutableList.Create(5, 3, 2),
                    [5] = ImmutableList.Create(5, 3, 2, 1),
                    [6] = ImmutableList.Create(5, 3, 3, 2),
                    [7] = ImmutableList.Create(6, 4, 3, 2, 1),
                    [8] = ImmutableList.Create(6, 4, 3, 3, 2),
                    [9] = ImmutableList.Create(6, 4, 4, 3, 2, 1),
                    [10] = ImmutableList.Create(6, 4, 4, 3, 3, 2),
                    [11] = ImmutableList.Create(6, 5, 4, 4, 3, 2, 1),
                    [12] = ImmutableList.Create(6, 5, 4, 4, 3, 3, 2),
                    [13] = ImmutableList.Create(6, 5, 5, 4, 4, 3, 2, 1),
                    [14] = ImmutableList.Create(6, 5, 5, 4, 4, 3, 3, 2),
                    [15] = ImmutableList.Create(6, 5, 5, 5, 4, 4, 3, 2, 1),
                    [16] = ImmutableList.Create(6, 5, 5, 5, 4, 4, 3, 3, 2),
                    [17] = ImmutableList.Create(6, 5, 5, 5, 5, 4, 4, 3, 2, 1),
                    [18] = ImmutableList.Create(6, 5, 5, 5, 5, 4, 4, 3, 3, 2),
                    [19] = ImmutableList.Create(6, 5, 5, 5, 5, 5, 4, 4, 3, 3),
                    [20] = ImmutableList.Create(6, 5, 5, 5, 5, 5, 4, 4, 4, 4)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.heal,
                    SkillId.listen,
                    SkillId.spellcraft,
                    SkillId.spot,
                    SkillId.wilderness_lore,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.handle_animal,
                    SkillId.knowledge_nature,
                    SkillId.profession,
                    SkillId.ride,
                    SkillId.swim,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_SCIMITAR, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGSPEAR, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_DRUID, 1},
                    {FeatId.ANIMAL_COMPANION, 1},
                    {FeatId.NATURE_SENSE, 2},
                    {FeatId.TRACKLESS_STEP, 3},
                    {FeatId.RESIST_NATURES_LURE, 4},
                    {FeatId.WILD_SHAPE, 5},
                    {FeatId.VENOM_IMMUNITY, 9},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateFighter()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_fighter,
                helpTopic = "TAG_FIGHTERS",
                conditionName = "Fighter",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 10,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 2,
                hasArmoredArcaneCasterFeature = false,
                classSkills = new HashSet<SkillId>
                {
                    SkillId.intimidate,
                    SkillId.alchemy,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.handle_animal,
                    SkillId.jump,
                    SkillId.ride,
                    SkillId.swim,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateMonk()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_monk,
                helpTopic = "TAG_MONKS",
                conditionName = "Monk",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 8,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 4,
                hasArmoredArcaneCasterFeature = false,
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.sense_motive,
                    SkillId.spot,
                    SkillId.tumble,
                    SkillId.perform,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.escape_artist,
                    SkillId.jump,
                    SkillId.knowledge_arcana,
                    SkillId.knowledge_religion,
                    SkillId.profession,
                    SkillId.swim,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.IMPROVED_UNARMED_STRIKE, 1},
                    {FeatId.STUNNING_FIST, 1},
                    {FeatId.STUNNING_ATTACKS, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_MONK, 1},
                    {FeatId.FLURRY_OF_BLOWS, 1},
                    {FeatId.EVASION, 2},
                    {FeatId.FAST_MOVEMENT, 3},
                    {FeatId.STILL_MIND, 3},
                    {FeatId.KI_STRIKE, 4},
                    {FeatId.PURITY_OF_BODY, 5},
                    {FeatId.WHOLENESS_OF_BODY, 7},
                    {FeatId.IMPROVED_EVASION, 9},
                    {FeatId.MONK_DIAMOND_BODY, 11},
                    {FeatId.MONK_ABUNDANT_STEP, 12},
                    {FeatId.MONK_DIAMOND_SOUL, 13},
                    {FeatId.MONK_QUIVERING_PALM, 15},
                    {FeatId.MONK_EMPTY_BODY, 19},
                    {FeatId.MONK_PERFECT_SELF, 20},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreatePaladin()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_paladin,
                helpTopic = "TAG_PALADINS",
                conditionName = "Paladin",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 10,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 2,
                spellListType = SpellListType.Paladin,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Divine,
                spellCastingConditionName = "Paladin Spellcasting",
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(-1),
                    [2] = ImmutableList.Create(-1),
                    [3] = ImmutableList.Create(-1),
                    [4] = ImmutableList.Create(-1, 0),
                    [5] = ImmutableList.Create(-1, 0),
                    [6] = ImmutableList.Create(-1, 1),
                    [7] = ImmutableList.Create(-1, 1),
                    [8] = ImmutableList.Create(-1, 1, 0),
                    [9] = ImmutableList.Create(-1, 1, 0),
                    [10] = ImmutableList.Create(-1, 1, 1),
                    [11] = ImmutableList.Create(-1, 1, 1, 0),
                    [12] = ImmutableList.Create(-1, 1, 1, 1),
                    [13] = ImmutableList.Create(-1, 1, 1, 1),
                    [14] = ImmutableList.Create(-1, 2, 1, 1, 0),
                    [15] = ImmutableList.Create(-1, 2, 1, 1, 1),
                    [16] = ImmutableList.Create(-1, 2, 2, 1, 1),
                    [17] = ImmutableList.Create(-1, 2, 2, 2, 1),
                    [18] = ImmutableList.Create(-1, 3, 2, 2, 1),
                    [19] = ImmutableList.Create(-1, 3, 3, 3, 2),
                    [20] = ImmutableList.Create(-1, 3, 3, 3, 3)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.heal,
                    SkillId.sense_motive,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.handle_animal,
                    SkillId.knowledge_religion,
                    SkillId.profession,
                    SkillId.ride,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.SMITE_EVIL, 1},
                    {FeatId.DETECT_EVIL, 1},
                    {FeatId.CODE_OF_CONDUCT, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                    {FeatId.LAY_ON_HANDS, 2},
                    {FeatId.DIVINE_GRACE, 2},
                    {FeatId.AURA_OF_COURAGE, 3},
                    {FeatId.DIVINE_HEALTH, 3},
                    {FeatId.TURN_UNDEAD, 4},
                    {FeatId.SPECIAL_MOUNT, 5},
                    {FeatId.REMOVE_DISEASE, 6},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateRanger()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_ranger,
                helpTopic = "TAG_RANGERS",
                conditionName = "Ranger",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 8,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 6,
                spellListType = SpellListType.Ranger,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Divine,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(-1),
                    [2] = ImmutableList.Create(-1),
                    [3] = ImmutableList.Create(-1),
                    [4] = ImmutableList.Create(-1, 0),
                    [5] = ImmutableList.Create(-1, 0),
                    [6] = ImmutableList.Create(-1, 1),
                    [7] = ImmutableList.Create(-1, 1),
                    [8] = ImmutableList.Create(-1, 1, 0),
                    [9] = ImmutableList.Create(-1, 1, 0),
                    [10] = ImmutableList.Create(-1, 1, 1),
                    [11] = ImmutableList.Create(-1, 1, 1, 0),
                    [12] = ImmutableList.Create(-1, 1, 1, 1),
                    [13] = ImmutableList.Create(-1, 1, 1, 1),
                    [14] = ImmutableList.Create(-1, 2, 1, 1, 0),
                    [15] = ImmutableList.Create(-1, 2, 1, 1, 1),
                    [16] = ImmutableList.Create(-1, 2, 2, 1, 1),
                    [17] = ImmutableList.Create(-1, 2, 2, 2, 1),
                    [18] = ImmutableList.Create(-1, 3, 2, 2, 1),
                    [19] = ImmutableList.Create(-1, 3, 3, 3, 2),
                    [20] = ImmutableList.Create(-1, 3, 3, 3, 3)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.heal,
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.search,
                    SkillId.spot,
                    SkillId.wilderness_lore,
                    SkillId.alchemy,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.handle_animal,
                    SkillId.jump,
                    SkillId.knowledge_nature,
                    SkillId.profession,
                    SkillId.ride,
                    SkillId.swim,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.TRACK, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                    {FeatId.ANIMAL_COMPANION, 4},
                    {FeatId.EVASION, 9},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateRogue()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_rogue,
                helpTopic = "TAG_ROGUES",
                conditionName = "Rogue",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 8,
                hasArmoredArcaneCasterFeature = false,
                classSkills = new HashSet<SkillId>
                {
                    SkillId.appraise,
                    SkillId.bluff,
                    SkillId.diplomacy,
                    SkillId.disable_device,
                    SkillId.gather_information,
                    SkillId.hide,
                    SkillId.intimidate,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.open_lock,
                    SkillId.pick_pocket,
                    SkillId.search,
                    SkillId.sense_motive,
                    SkillId.spot,
                    SkillId.tumble,
                    SkillId.use_magic_device,
                    SkillId.perform,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.disguise,
                    SkillId.escape_artist,
                    SkillId.forgery,
                    SkillId.jump,
                    SkillId.profession,
                    SkillId.swim,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_ROGUE, 1},
                    {FeatId.SNEAK_ATTACK, 1},
                    {FeatId.TRAPS, 1},
                    {FeatId.EVASION, 2},
                    {FeatId.UNCANNY_DODGE, 4},
                    {FeatId.IMPROVED_UNCANNY_DODGE, 8},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateSorcerer()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_sorcerer,
                helpTopic = "TAG_SORCERERS",
                conditionName = "Sorcerer",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
                hitDice = 4,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                spellListType = SpellListType.Arcane,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(5, 3),
                    [2] = ImmutableList.Create(6, 4),
                    [3] = ImmutableList.Create(6, 5),
                    [4] = ImmutableList.Create(6, 6, 3),
                    [5] = ImmutableList.Create(6, 6, 4),
                    [6] = ImmutableList.Create(6, 6, 5, 3),
                    [7] = ImmutableList.Create(6, 6, 6, 4),
                    [8] = ImmutableList.Create(6, 6, 6, 5, 3),
                    [9] = ImmutableList.Create(6, 6, 6, 6, 4),
                    [10] = ImmutableList.Create(6, 6, 6, 6, 5, 3),
                    [11] = ImmutableList.Create(6, 6, 6, 6, 6, 4),
                    [12] = ImmutableList.Create(6, 6, 6, 6, 6, 5, 3),
                    [13] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 4),
                    [14] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 5, 3),
                    [15] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 4),
                    [16] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [17] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [18] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [19] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [20] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 6)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.bluff,
                    SkillId.concentration,
                    SkillId.spellcraft,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.knowledge_arcana,
                    SkillId.profession,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1}, {FeatId.CALL_FAMILIAR, 1},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateWizard()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_wizard,
                helpTopic = "TAG_WIZARDS",
                conditionName = "Wizard",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
                hitDice = 4,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                spellListType = SpellListType.Arcane,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellStat = Stat.intelligence,
                spellDcStat = Stat.intelligence,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(3, 1),
                    [2] = ImmutableList.Create(4, 2),
                    [3] = ImmutableList.Create(4, 2, 1),
                    [4] = ImmutableList.Create(4, 3, 2),
                    [5] = ImmutableList.Create(4, 3, 2, 1),
                    [6] = ImmutableList.Create(4, 3, 3, 2),
                    [7] = ImmutableList.Create(4, 4, 3, 2, 1),
                    [8] = ImmutableList.Create(4, 4, 3, 3, 2),
                    [9] = ImmutableList.Create(4, 4, 4, 3, 2, 1),
                    [10] = ImmutableList.Create(4, 4, 4, 3, 3, 2),
                    [11] = ImmutableList.Create(4, 4, 4, 4, 3, 2, 1),
                    [12] = ImmutableList.Create(4, 4, 4, 4, 3, 3, 2),
                    [13] = ImmutableList.Create(4, 4, 4, 4, 4, 3, 2, 1),
                    [14] = ImmutableList.Create(4, 4, 4, 4, 4, 3, 3, 2),
                    [15] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 3, 2, 1),
                    [16] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 3, 3, 2),
                    [17] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 3, 2, 1),
                    [18] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 3, 3, 2),
                    [19] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 4, 3, 3),
                    [20] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 4, 4, 4)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.spellcraft,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.knowledge_nature,
                    SkillId.knowledge_all,
                    SkillId.profession,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.SCRIBE_SCROLL, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_WIZARD, 1},
                    {FeatId.CALL_FAMILIAR, 1},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateArcaneArcher()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_arcane_archer,
                helpTopic = "TAG_ARCANE_ARCHERS",
                conditionName = "Arcane Archer",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 8,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 4,
                hasArmoredArcaneCasterFeature = false,
                classSkills = new HashSet<SkillId>
                {
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.spot,
                    SkillId.wilderness_lore,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.ride,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                    /* Omitted hash level 1 */
                    /* Omitted hash level 2 */
                    /* Omitted hash level 4 */
                    /* Omitted hash level 6 */
                    /* Omitted hash level 8 */
                    /* Omitted hash level 10 */
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateArcaneTrickster()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_arcane_trickster,
                helpTopic = "TAG_ARCANE_TRICKSTERS",
                conditionName = "Arcane Trickster",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
                hitDice = 4,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 4,
                spellListType = SpellListType.Extender,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = "Arcane Trickster Spellcasting",
                classSkills = new HashSet<SkillId>
                {
                    SkillId.appraise,
                    SkillId.bluff,
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.disable_device,
                    SkillId.gather_information,
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.open_lock,
                    SkillId.pick_pocket,
                    SkillId.search,
                    SkillId.sense_motive,
                    SkillId.spellcraft,
                    SkillId.spot,
                    SkillId.tumble,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.disguise,
                    SkillId.escape_artist,
                    SkillId.jump,
                    SkillId.knowledge_all,
                    SkillId.profession,
                    SkillId.swim,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int> {{FeatId.SNEAK_ATTACK, 2}}.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateArchmage()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_archmage,
                helpTopic = "TAG_ARCHMAGES",
                conditionName = "Archmage",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
                hitDice = 4,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                spellListType = SpellListType.Arcane,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = "Archmage Spellcasting",
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.search,
                    SkillId.spellcraft,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.knowledge_all,
                    SkillId.profession,
                }.ToImmutableHashSet(),
            };
        }

        public static D20ClassSpec CreateAssassin()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_assassin,
                helpTopic = "TAG_ASSASSINS",
                conditionName = "Assassin",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 4,
                spellListType = SpellListType.Special,
                hasArmoredArcaneCasterFeature = true,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(-1, 0),
                    [2] = ImmutableList.Create(-1, 1),
                    [3] = ImmutableList.Create(-1, 2, 0),
                    [4] = ImmutableList.Create(-1, 3, 1),
                    [5] = ImmutableList.Create(-1, 3, 2, 0),
                    [6] = ImmutableList.Create(-1, 3, 3, 1),
                    [7] = ImmutableList.Create(-1, 3, 3, 2, 0),
                    [8] = ImmutableList.Create(-1, 3, 3, 3, 1),
                    [9] = ImmutableList.Create(-1, 3, 3, 3, 2),
                    [10] = ImmutableList.Create(-1, 3, 3, 3, 3)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.bluff,
                    SkillId.diplomacy,
                    SkillId.disable_device,
                    SkillId.gather_information,
                    SkillId.hide,
                    SkillId.intimidate,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.open_lock,
                    SkillId.pick_pocket,
                    SkillId.search,
                    SkillId.sense_motive,
                    SkillId.spot,
                    SkillId.tumble,
                    SkillId.use_magic_device,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.disguise,
                    SkillId.escape_artist,
                    SkillId.forgery,
                    SkillId.jump,
                    SkillId.swim,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_ROGUE, 1},
                    {FeatId.SNEAK_ATTACK, 1},
                    /* Omitted hash level 1 */
                    {FeatId.UNCANNY_DODGE, 2},
                    /* Omitted hash level 2 */
                    {FeatId.IMPROVED_UNCANNY_DODGE, 5},
                    /* Omitted hash level 8 */
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateBlackguard()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_blackguard,
                helpTopic = "TAG_BLACKGUARDS",
                conditionName = "Blackguard",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 10,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 2,
                spellListType = SpellListType.Special,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Divine,
                spellCastingConditionName = "Blackguard Spellcasting",
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(-1, 0),
                    [2] = ImmutableList.Create(-1, 1),
                    [3] = ImmutableList.Create(-1, 1, 0),
                    [4] = ImmutableList.Create(-1, 1, 1),
                    [5] = ImmutableList.Create(-1, 1, 1, 0),
                    [6] = ImmutableList.Create(-1, 1, 1, 1),
                    [7] = ImmutableList.Create(-1, 2, 1, 1, 0),
                    [8] = ImmutableList.Create(-1, 2, 1, 1, 1),
                    [9] = ImmutableList.Create(-1, 2, 2, 1, 1),
                    [10] = ImmutableList.Create(-1, 2, 2, 2, 1)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.heal,
                    SkillId.hide,
                    SkillId.intimidate,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.handle_animal,
                    SkillId.knowledge_religion,
                    SkillId.profession,
                    SkillId.ride,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                    /* Omitted hash level 1 */
                    /* Omitted hash level 2 */
                    /* Omitted hash level 2 */
                    {FeatId.REBUKE_UNDEAD, 3},
                    /* Omitted hash level 3 */
                    {FeatId.SNEAK_ATTACK, 4},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateDuelist()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_duelist,
                helpTopic = "TAG_DUELISTS",
                conditionName = "Duelist",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 10,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 4,
                hasArmoredArcaneCasterFeature = false,
                classSkills = new HashSet<SkillId>
                {
                    SkillId.bluff,
                    SkillId.listen,
                    SkillId.sense_motive,
                    SkillId.spot,
                    SkillId.tumble,
                    SkillId.perform,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.escape_artist,
                    SkillId.jump,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1}, {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateDwarvenDefender()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_dwarven_defender,
                helpTopic = "TAG_DWARVEN_DEFENDERS",
                conditionName = "Dwarven Defender",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 12,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                hasArmoredArcaneCasterFeature = false,
                classSkills = new HashSet<SkillId>
                {
                    SkillId.listen,
                    SkillId.sense_motive,
                    SkillId.spot,
                    SkillId.alchemy,
                    SkillId.craft,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                    {FeatId.UNCANNY_DODGE, 2},
                    {FeatId.TRAPS, 4},
                    {FeatId.IMPROVED_UNCANNY_DODGE, 6},
                }.ToImmutableDictionary(),
            };
        }

        public static D20ClassSpec CreateEldritchKnight()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_eldritch_knight,
                helpTopic = "TAG_ELDRITCH_KNIGHTS",
                conditionName = "Eldritch Knight",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 2,
                spellListType = SpellListType.Extender,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Ability,
                spellCastingConditionName = "Eldritch Knight Spellcasting",
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.sense_motive,
                    SkillId.spellcraft,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.jump,
                    SkillId.knowledge_arcana,
                    SkillId.ride,
                    SkillId.swim,
                }.ToImmutableHashSet(),
            };
        }

        public static D20ClassSpec CreateMysticTheurge()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_mystic_theurge,
                helpTopic = "TAG_MYSTIC_THEURGES",
                conditionName = "Mystic Theurge",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
                hitDice = 4,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                spellListType = SpellListType.Extender,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Ability,
                spellCastingConditionName = "Mystic Theurge Spellcasting",
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.sense_motive,
                    SkillId.spellcraft,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.knowledge_arcana,
                    SkillId.knowledge_religion,
                    SkillId.profession,
                }.ToImmutableHashSet(),
            };
        }

        public static D20ClassSpec CreateFavoredSoul()
        {
            return new D20ClassSpec
            {
                classEnum = Stat.level_favored_soul,
                helpTopic = "TAG_FAVORED_SOULS",
                conditionName = "Favored Soul",
                flags = ClassDefinitionFlag.CDF_BaseClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 8,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                spellListType = SpellListType.Clerical,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Divine,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(5, 3),
                    [2] = ImmutableList.Create(6, 4),
                    [3] = ImmutableList.Create(6, 5),
                    [4] = ImmutableList.Create(6, 6, 3),
                    [5] = ImmutableList.Create(6, 6, 4),
                    [6] = ImmutableList.Create(6, 6, 5, 3),
                    [7] = ImmutableList.Create(6, 6, 6, 4),
                    [8] = ImmutableList.Create(6, 6, 6, 5, 3),
                    [9] = ImmutableList.Create(6, 6, 6, 6, 4),
                    [10] = ImmutableList.Create(6, 6, 6, 6, 5, 3),
                    [11] = ImmutableList.Create(6, 6, 6, 6, 6, 4),
                    [12] = ImmutableList.Create(6, 6, 6, 6, 6, 5, 3),
                    [13] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 4),
                    [14] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 5, 3),
                    [15] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 4),
                    [16] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [17] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [18] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [19] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [20] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 6)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.heal,
                    SkillId.sense_motive,
                    SkillId.spellcraft,
                    SkillId.alchemy,
                    SkillId.craft,
                    SkillId.knowledge_arcana,
                    SkillId.profession,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.DOMAIN_POWER, 1},
                    /* Omitted hash level 3 */
                    /* Omitted hash level 5 */
                    /* Omitted hash level 12 */
                    /* Omitted hash level 20 */
                }.ToImmutableDictionary(),
                deityClass = Stat.level_cleric
            };
        }

        public static D20ClassSpec CreateScout()
        {
            return new D20ClassSpec
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
                    /* Omitted hash level 1 */
                    {FeatId.UNCANNY_DODGE, 2},
                    /* Omitted hash level 2 */
                    /* Omitted hash level 3 */
                    {FeatId.EVASION, 5},
                    /* Omitted hash level 14 */
                    /* Omitted hash level 18 */
                    /* Omitted hash level 20 */
                }.ToImmutableDictionary(),
                deityClass = Stat.level_ranger
            };
        }

        public static D20ClassSpec CreateWarmage()
        {
            /*has advanced learning*/
            return new D20ClassSpec
            {
                classEnum = Stat.level_warmage,
                helpTopic = "TAG_WARMAGES",
                conditionName = "Warmage",
                flags = ClassDefinitionFlag.CDF_BaseClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 2,
                spellListType = SpellListType.Special,
                hasArmoredArcaneCasterFeature = true,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(5, 3),
                    [2] = ImmutableList.Create(6, 4),
                    [3] = ImmutableList.Create(6, 5),
                    [4] = ImmutableList.Create(6, 6, 3),
                    [5] = ImmutableList.Create(6, 6, 4),
                    [6] = ImmutableList.Create(6, 6, 5, 3),
                    [7] = ImmutableList.Create(6, 6, 6, 4),
                    [8] = ImmutableList.Create(6, 6, 6, 5, 3),
                    [9] = ImmutableList.Create(6, 6, 6, 6, 4),
                    [10] = ImmutableList.Create(6, 6, 6, 6, 5, 3),
                    [11] = ImmutableList.Create(6, 6, 6, 6, 6, 4),
                    [12] = ImmutableList.Create(6, 6, 6, 6, 6, 5, 3),
                    [13] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 4),
                    [14] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 5, 3),
                    [15] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 4),
                    [16] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [17] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [18] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [19] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [20] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 5)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration, SkillId.intimidate, SkillId.spellcraft, SkillId.alchemy,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    /* Omitted hash level 1 */
                    /* Omitted hash level 1 */
                    /* Omitted hash level 1 */
                    {FeatId.ARMOR_PROFICIENCY_MEDIUM, 8},
                }.ToImmutableDictionary(),
                deityClass = Stat.level_sorcerer
            };
        }

        public static D20ClassSpec CreateBeguiler()
        {
            /*has advanced learning*/
            return new D20ClassSpec
            {
                classEnum = Stat.level_beguilers,
                helpTopic = "TAG_BEGUILERS",
                conditionName = "Beguiler",
                flags = ClassDefinitionFlag.CDF_BaseClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 6,
                spellListType = SpellListType.Special,
                hasArmoredArcaneCasterFeature = true,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(5, 3),
                    [2] = ImmutableList.Create(6, 4),
                    [3] = ImmutableList.Create(6, 5),
                    [4] = ImmutableList.Create(6, 6, 3),
                    [5] = ImmutableList.Create(6, 6, 4),
                    [6] = ImmutableList.Create(6, 6, 5, 3),
                    [7] = ImmutableList.Create(6, 6, 6, 4),
                    [8] = ImmutableList.Create(6, 6, 6, 5, 3),
                    [9] = ImmutableList.Create(6, 6, 6, 6, 4),
                    [10] = ImmutableList.Create(6, 6, 6, 6, 5, 3),
                    [11] = ImmutableList.Create(6, 6, 6, 6, 6, 4),
                    [12] = ImmutableList.Create(6, 6, 6, 6, 6, 5, 3),
                    [13] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 4),
                    [14] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 5, 3),
                    [15] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 4),
                    [16] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [17] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [18] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 5, 3),
                    [19] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 4),
                    [20] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 5)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.appraise,
                    SkillId.bluff,
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.disable_device,
                    SkillId.gather_information,
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.open_lock,
                    SkillId.pick_pocket,
                    SkillId.search,
                    SkillId.sense_motive,
                    SkillId.spellcraft,
                    SkillId.spot,
                    SkillId.tumble,
                    SkillId.use_magic_device,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.climb,
                    SkillId.decipher_script,
                    SkillId.disguise,
                    SkillId.escape_artist,
                    SkillId.forgery,
                    SkillId.jump,
                    SkillId.swim,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.EXOTIC_WEAPON_PROFICIENCY_HAND_CROSSBOW, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORT_SWORD, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_RAPIER, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORTBOW, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.TRAPS, 1},
                    /* Omitted hash level 1 */
                    /* Omitted hash level 2 */
                    /* Omitted hash level 2 */
                    {FeatId.SILENT_SPELL, 5},
                    {FeatId.STILL_SPELL, 10},
                }.ToImmutableDictionary(),
                deityClass = Stat.level_rogue
            };
        }
    }
}