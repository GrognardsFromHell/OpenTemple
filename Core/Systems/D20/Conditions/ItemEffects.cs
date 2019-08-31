using System;
using System.Collections.Generic;
using IronPython.Runtime.Types;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Particles.Instances;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Teleport;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static class ItemEffects
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x102f07a8)]
        public static readonly ConditionSpec WeaponMasterwork = ConditionSpec.Create("Weapon Masterwork", 3)
            .AddHandler(DispatcherType.ToHitBonus2, WeaponMasterworkToHitBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .Build();


        [TempleDllLocation(0x102f07f0)]
        public static readonly ConditionSpec WeaponEnhancementBonus = ConditionSpec
            .Create("Weapon Enhancement Bonus", 5)
            .AddHandler(DispatcherType.ToHitBonus2, WeaponEnhancementToHitBonus)
            .AddHandler(DispatcherType.DealingDamage, WeaponDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddQueryHandler(D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus, WeaponHasEnhancementBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Item_Remove_Enhancement, ItemRemoveEnhancement)
            .Build();


        [TempleDllLocation(0x102f0898)]
        public static readonly ConditionSpec WeaponDefendingBonus = ConditionSpec.Create("Weapon Defending Bonus", 4)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.ToHitBonus2, sub_10101A40)
            .AddHandler(DispatcherType.DealingDamage, sub_10101AD0)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddHandler(DispatcherType.GetAC, WeaponDefendingArmorBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .AddHandler(DispatcherType.RadialMenuEntry, ActivateDefendingWeaponRadial)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 1)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .Build();


        [TempleDllLocation(0x102f0998)]
        public static readonly ConditionSpec WeaponFlaming = ConditionSpec.Create("Weapon Flaming", 3)
            .AddHandler(DispatcherType.DealingDamage, GenericElementalDamageBonus, DamageType.Fire)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Fire")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 6)
            .Build();


        [TempleDllLocation(0x102f0a18)]
        public static readonly ConditionSpec WeaponFrost = ConditionSpec.Create("Weapon Frost", 3)
            .AddHandler(DispatcherType.DealingDamage, GenericElementalDamageBonus, DamageType.Cold)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4100)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 5)
            .Build();


        [TempleDllLocation(0x102f0a98)]
        public static readonly ConditionSpec WeaponShock = ConditionSpec.Create("Weapon Shock", 3)
            .AddHandler(DispatcherType.DealingDamage, GenericElementalDamageBonus, DamageType.Electricity)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Shock")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 9)
            .Build();


        [TempleDllLocation(0x102f0c50)]
        public static readonly ConditionSpec WeaponFlamingBurst = ConditionSpec.Create("Weapon Flaming Burst", 3)
            .AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, DamageType.Fire)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Fire")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-FIRE-burst")
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 6)
            .Build();


        [TempleDllLocation(0x102f0ce8)]
        public static readonly ConditionSpec WeaponIcyBurst = ConditionSpec.Create("Weapon Icy Burst", 3)
            .AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, DamageType.Cold)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4096)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-COLD-burst")
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 5)
            .Build();


        [TempleDllLocation(0x102f0d80)]
        public static readonly ConditionSpec WeaponShockingBurst = ConditionSpec.Create("Weapon Shocking Burst", 3)
            .AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, DamageType.Electricity)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Shock")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-SHOCK-burst")
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 9)
            .Build();


        [TempleDllLocation(0x102f0e18)]
        public static readonly ConditionSpec WeaponHoly = ConditionSpec.Create("Weapon Holy", 3)
            .AddHandler(DispatcherType.DealingDamage, sub_100FFA50)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 12)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-HOLY-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 8)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 8)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 8)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 8)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 8)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 8)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 7)
            .Build();


        [TempleDllLocation(0x102f0f10)]
        public static readonly ConditionSpec WeaponUnholy = ConditionSpec.Create("Weapon Unholy", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponUnholyDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 20)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-UNHOLY-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 4)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 4)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 4)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 4)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 4)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 4)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 10)
            .Build();


        [TempleDllLocation(0x102f1008)]
        public static readonly ConditionSpec WeaponLawful = ConditionSpec.Create("Weapon Lawful", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponLawfulDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 68)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-LAW-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 2)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 2)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 2)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 2)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 2)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 2)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 8)
            .Build();


        [TempleDllLocation(0x102f1100)]
        public static readonly ConditionSpec WeaponChaotic = ConditionSpec.Create("Weapon Chaotic", 3)
            .AddHandler(DispatcherType.DealingDamage, sub_100FFC30)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 36)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-CHAOTIC-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 1)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 1)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 1)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 1)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 1)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 1)
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 4)
            .Build();


        [TempleDllLocation(0x102f0b08)]
        public static readonly ConditionSpec WeaponKeen = ConditionSpec.Create("Weapon Keen", 5)
            .AddHandler(DispatcherType.GetCriticalHitRange, WeaponKeenCritHitRange)
            .AddQueryHandler(D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus, WeaponHasEnhancementBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Weapon_Get_Keen_Bonus, WeaponKeepBonus)
            .AddSignalHandler(D20DispatcherKey.SIG_Item_Remove_Enhancement, ItemRemoveEnhancement)
            .Build();


        [TempleDllLocation(0x102f0ba8)]
        public static readonly ConditionSpec WeaponBane = ConditionSpec.Create("Weapon Bane", 3)
            .AddHandler(DispatcherType.DealingDamage, BaneWeaponDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddHandler(DispatcherType.ToHitBonus2, BaneWeaponToHitBonus)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Bane")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-BANE-medium")
            .AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 3)
            .Build();


        [TempleDllLocation(0x102f11f8)]
        public static readonly ConditionSpec ToHitBonus = ConditionSpec.Create("To Hit Bonus", 3)
            .AddHandler(DispatcherType.ToHitBonus2, WeaponEnhancementToHitBonus)
            .Build();


        [TempleDllLocation(0x102f1228)]
        public static readonly ConditionSpec DamageBonus = ConditionSpec.Create("Damage Bonus", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .Build();


        [TempleDllLocation(0x102f1270)]
        public static readonly ConditionSpec ArmorBonus = ConditionSpec.Create("Armor Bonus", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_AC_Bonus, QueryAcBonus, 0)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus, ArmorOrShieldGetMaxDexBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_Speed, ArmorOrShieldGetMaxSpeed)
            .AddHandler(DispatcherType.GetAC, ArmorBonusAcBonus)
            .AddHandler(DispatcherType.GetAC, ArmorBonusAcBonusCapValue)
            .AddHandler(DispatcherType.GetMoveSpeed, ArmorBonusMovementSthg_Callback)
            .AddSkillLevelHandler(SkillId.balance, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.climb, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.escape_artist, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.hide, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.jump, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.move_silently, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.pick_pocket, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.tumble, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.open_lock, sub_10100650)
            .AddSkillLevelHandler(SkillId.wilderness_lore, sub_10100650)
            .Build();


        [TempleDllLocation(0x102f13d0)]
        public static readonly ConditionSpec ArmorMasterwork = ConditionSpec.Create("Armor Masterwork", 3)
            .AddSkillLevelHandler(SkillId.balance, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.climb, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.escape_artist, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.hide, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.jump, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.move_silently, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.pick_pocket, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.tumble, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.open_lock, sub_10100500)
            .AddSkillLevelHandler(SkillId.wilderness_lore, sub_10100500)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH, sub_10100500)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_DEXTERITY, sub_10100500)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .Build();


        [TempleDllLocation(0x102f14e0)]
        public static readonly ConditionSpec ArmorEnhancementBonus = ConditionSpec.Create("Armor Enhancement Bonus", 5)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_AC_Bonus, QueryAcBonus, 0)
            .AddHandler(DispatcherType.GetAC, ArmorEnhancementAcBonus)
            .AddSkillLevelHandler(SkillId.balance, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.climb, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.escape_artist, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.hide, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.jump, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.move_silently, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.pick_pocket, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.tumble, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.open_lock, sub_10100500)
            .AddSkillLevelHandler(SkillId.wilderness_lore, sub_10100500)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH, sub_10100500)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_DEXTERITY, sub_10100500)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .AddQueryHandler(D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus, WeaponHasEnhancementBonus)
            .AddSignalHandler(D20DispatcherKey.SIG_Item_Remove_Enhancement, ItemRemoveEnhancement)
            .Build();


        [TempleDllLocation(0x102f0968)]
        public static readonly ConditionSpec DeflectionBonus = ConditionSpec.Create("Deflection Bonus", 3)
            .AddHandler(DispatcherType.GetAC, DeflectionAcBonus)
            .Build();


        [TempleDllLocation(0x102f1650)]
        public static readonly ConditionSpec ShieldBonus = ConditionSpec.Create("Shield Bonus", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_AC_Bonus, QueryAcBonus, 0)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus, ArmorOrShieldGetMaxDexBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_Speed, ArmorOrShieldGetMaxSpeed)
            .AddHandler(DispatcherType.GetAC, ShieldAcBonus)
            .AddSkillLevelHandler(SkillId.balance, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.climb, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.escape_artist, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.hide, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.jump, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.move_silently, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.pick_pocket, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.tumble, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.open_lock, sub_10100650)
            .AddSkillLevelHandler(SkillId.wilderness_lore, sub_10100650)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH, sub_10100650)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_DEXTERITY, sub_10100650)
            .Build();


        [TempleDllLocation(0x102f17b0)]
        public static readonly ConditionSpec ShieldEnhancementBonus = ConditionSpec
            .Create("Shield Enhancement Bonus", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_AC_Bonus, QueryAcBonus, 0)
            .AddHandler(DispatcherType.GetAC, ShieldEnhancementAcBonus)
            .AddSkillLevelHandler(SkillId.balance, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.climb, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.escape_artist, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.hide, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.jump, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.move_silently, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.pick_pocket, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.tumble, MasterworkArmorSkillBonus)
            .AddSkillLevelHandler(SkillId.open_lock, sub_10100500)
            .AddSkillLevelHandler(SkillId.wilderness_lore, sub_10100500)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH, sub_10100500)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_DEXTERITY, sub_10100500)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .Build();


        [TempleDllLocation(0x102f18f8)]
        public static readonly ConditionSpec Crossbow = ConditionSpec.Create("Crossbow", 3)
            .AddHandler(DispatcherType.RadialMenuEntry, ReloadRadial)
            .Build();


        [TempleDllLocation(0x102f1928)]
        public static readonly ConditionSpec UseableItem = ConditionSpec.Create("UseableItem", 3)
            .AddHandler(DispatcherType.RadialMenuEntry, UseableItemRadialEntry)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.LVL_Stats_Finalize, UseableItemActionCheck)
            .Build();


        [TempleDllLocation(0x102f196c)]
        public static readonly ConditionSpec SkillCircumstanceBonus = ConditionSpec
            .Create("Skill Circumstance Bonus", 3)
            .AddHandler(DispatcherType.SkillLevel, SkillBonusCallback, 21)
            .Build();


        [TempleDllLocation(0x102f199c)]
        public static readonly ConditionSpec SkillCompetenceBonus = ConditionSpec.Create("Skill Competence Bonus", 3)
            .AddHandler(DispatcherType.SkillLevel, SkillBonusCallback, 34)
            .Build();


        [TempleDllLocation(0x102f19cc)]
        public static readonly ConditionSpec AttributeEnhancementBonus = ConditionSpec
            .Create("Attribute Enhancement Bonus", 3)
            .AddHandler(DispatcherType.AbilityScoreLevel, AttributeEnhancementBonus_callback)
            .Build();


        [TempleDllLocation(0x102f19fc)]
        public static readonly ConditionSpec SavingThrowResistanceBonus = ConditionSpec
            .Create("Saving Throw Resistance Bonus", 3)
            .AddHandler(DispatcherType.SaveThrowLevel, AddSavingThrowResistanceBonus)
            .Build();


        [TempleDllLocation(0x102f1a30)]
        public static readonly ConditionSpec ItemParticleSystem = ConditionSpec.Create("Item Particle System", 3)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, ItemParticleSystemShow, 1, 0)
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Show, ItemParticleSystemShow, 1, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Hide, CommonConditionCallbacks.EndParticlesFromArg, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, sub_100EFC80, 1)
            .Build();


        [TempleDllLocation(0x102f1ab0)]
        public static readonly ConditionSpec FrostBow = ConditionSpec.Create("Frost Bow", 3)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .Build();


        [TempleDllLocation(0x102f1c20)]
        public static readonly ConditionSpec RingofInvisibility = ConditionSpec.Create("Ring of Invisibility", 3)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, RingOfInvisibilityStatusD20StatusInit)
            .AddHandler(DispatcherType.ConditionRemove, RingOfInvisibilityRemove)
            .AddHandler(DispatcherType.RadialMenuEntry, RingOfInvisibilityRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 116, RingOfInvisPerform)
            .AddSignalHandler(D20DispatcherKey.SIG_Sequence, RingOfInvisSequence)
            .Build();


        [TempleDllLocation(0x102f1af4)]
        public static readonly ConditionSpec LuckPoisonSaveBonus = ConditionSpec.Create("Luck Poison Save Bonus", 3)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, AddLuckPoisonSaveBonus)
            .Build();


        [TempleDllLocation(0x102f1b24)]
        public static readonly ConditionSpec ProofAgainstPoison = ConditionSpec.Create("Proof Against Poison", 3)
            .Prevents(StatusEffects.Poisoned)
            .Build();


        [TempleDllLocation(0x102f1b54)]
        public static readonly ConditionSpec ProofAgainstDetectionLocation = ConditionSpec
            .Create("Proof Against Detection Location", 3)
            .Build();


        [TempleDllLocation(0x102f1b70)]
        public static readonly ConditionSpec ElementalResistance = ConditionSpec.Create("Elemental Resistance", 3)
            .AddHandler(DispatcherType.TakingDamage2, ItemElementalResistanceDR)
            .Build();


        [TempleDllLocation(0x102f1ba0)]
        public static readonly ConditionSpec StaffOfStriking = ConditionSpec.Create("Staff Of Striking", 3)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 0)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.RadialMenuEntry, StaffOfStrikingRadial)
            .AddHandler(DispatcherType.DealingDamage, sub_10101760)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .Build();


        [TempleDllLocation(0x102f1ca0)]
        public static readonly ConditionSpec DaggerofVenom = ConditionSpec.Create("Dagger of Venom", 4)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, DaggerOfVenomStatusInit)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.CondNodeSetArgToZero)
            .AddHandler(DispatcherType.RadialMenuEntry, DaggerOfVenomRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 116, DaggerOfVenomActivate)
            .AddHandler(DispatcherType.DealingDamage2, DaggerOfVenomPoisonOnDamage)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_10101A20)
            .Build();


        [TempleDllLocation(0x102f1d60)]
        public static readonly ConditionSpec ElementalResistanceperround = ConditionSpec
            .Create("Elemental Resistance per round", 4)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, ElementalResistancePerRoundRefresh)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.BeginRound, ElementalResistancePerRoundRefresh)
            .AddHandler(DispatcherType.TakingDamage2, ElementalResistancePerRoundTakingDamage)
            .Build();


        [TempleDllLocation(0x102f1dcc)]
        public static readonly ConditionSpec UseableMagicStaff = ConditionSpec.Create("UseableMagicStaff", 3)
            .AddHandler(DispatcherType.RadialMenuEntry, UseableMagicStaffRadial)
            .Build();


        [TempleDllLocation(0x102f1dfc)]
        public static readonly ConditionSpec Ringoffreedomofmovement = ConditionSpec
            .Create("Ring of freedom of movement", 3)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, true)
            .Build();


        [TempleDllLocation(0x102f1e30)]
        public static readonly ConditionSpec Keoghtomsointment = ConditionSpec.Create("Keoghtom's ointment", 3)
            .AddHandler(DispatcherType.RadialMenuEntry, KeoghtomsOintmentRadial)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .Build();


        [TempleDllLocation(0x102f1e78)]
        public static readonly ConditionSpec Bootsofspeed = ConditionSpec.Create("Boots of speed", 5)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, BootsOfSpeedNewday)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.RadialMenuEntry, BootsOfSpeedRadial)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 120, BootsOfSpeedD20Check)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 120, BootsOfSpeedPerform)
            .AddHandler(DispatcherType.BeginRound, BootsOfSpeedBeginRound)
            .AddHandler(DispatcherType.SaveThrowLevel, BootsOfSpeedSaveThrow, 1, 0)
            .AddHandler(DispatcherType.GetBonusAttacks, BootsOfSpeedBonusAttack)
            .AddHandler(DispatcherType.GetAC, sub_101021C0)
            .AddHandler(DispatcherType.ToHitBonus2, BootsOfSpeedToHitBonus2, 1)
            .AddHandler(DispatcherType.GetMoveSpeedBase, BootsOfSpeed_MovementBonus_Callback)
            .AddHandler(DispatcherType.GetMoveSpeed, BootsOfSpeedGetMoveSpeed)
            .AddHandler(DispatcherType.Tooltip, BootsOfSpeedTooltip, 67)
            .Build();


        [TempleDllLocation(0x102f1f98)]
        public static readonly ConditionSpec ArmorSilentMoves = ConditionSpec.Create("Armor Silent Moves", 3)
            .AddSkillLevelHandler(SkillId.move_silently, ArmorShadowSilentMovesSkillBonus)
            .Build();


        [TempleDllLocation(0x102f1ff8)]
        public static readonly ConditionSpec ArmorSpellResistance = ConditionSpec.Create("Armor Spell Resistance", 3)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.SpellResistanceDebug)
            .AddHandler(DispatcherType.SpellResistanceMod, CommonConditionCallbacks.SpellResistanceMod_Callback, 5048)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance,
                CommonConditionCallbacks.SpellResistanceQuery)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipSpellResistanceCallback, 5048)
            .Build();


        [TempleDllLocation(0x102f2068)]
        public static readonly ConditionSpec WeaponFlameTongue = ConditionSpec.Create("Weapon Flame Tongue", 3)
            .AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, DamageType.Fire)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.RadialMenuEntry, ActivateDeviceSpellRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 122, sub_101027C0)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 1)
            .Build();


        [TempleDllLocation(0x102f0b74)]
        public static readonly ConditionSpec WeaponDisruption = ConditionSpec.Create("Weapon Disruption", 3)
            .AddHandler(DispatcherType.DealingDamage2, WeaponDisruptionOnDamage)
            .Build();


        [TempleDllLocation(0x102f20fc)]
        public static readonly ConditionSpec WeaponMightyCleaving = ConditionSpec.Create("Weapon Mighty Cleaving", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Weapon_Is_Mighty_Cleaving, QueryMasterwork)
            .Build();


        [TempleDllLocation(0x102f2130)]
        public static readonly ConditionSpec RingofAnimalSummoning = ConditionSpec.Create("Ring of Animal Summoning", 3)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.RadialMenuEntry, ActivateDeviceSpellRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 122, sub_101027C0)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 1)
            .Build();


        [TempleDllLocation(0x102f21a0)]
        public static readonly ConditionSpec SwordofLifeStealing = ConditionSpec.Create("Sword of Life Stealing", 3)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.ToHitBonus2, WeaponEnhancementToHitBonus)
            .AddHandler(DispatcherType.DealingDamage, WeaponDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .AddHandler(DispatcherType.DealingDamage2, SwordOfLifestealingDealDamage)
            .Build();


        [TempleDllLocation(0x102f2238)]
        public static readonly ConditionSpec FamiliarSkillBonus = ConditionSpec.Create("Familiar Skill Bonus", 3)
            .AddHandler(DispatcherType.SkillLevel, FamiliarSkillBonus2)
            .AddSkillLevelHandler(SkillId.listen, AddFamiliarSkillBonus, 2)
            .AddSkillLevelHandler(SkillId.spot, AddFamiliarSkillBonus, 2)
            .Build();


        [TempleDllLocation(0x102f2290)]
        public static readonly ConditionSpec FamiliarSaveBonus = ConditionSpec.Create("Familiar Save Bonus", 3)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrow_FamiliarBonus_Callback)
            .AddSkillLevelHandler(SkillId.listen, AddFamiliarSkillBonus, 2)
            .AddSkillLevelHandler(SkillId.spot, AddFamiliarSkillBonus, 2)
            .Build();


        [TempleDllLocation(0x102f22e8)]
        public static readonly ConditionSpec FamiliarHpBonus = ConditionSpec.Create("Familiar Hp Bonus", 3)
            .AddHandler(DispatcherType.MaxHP, sub_10102A00)
            .AddSkillLevelHandler(SkillId.listen, AddFamiliarSkillBonus, 2)
            .AddSkillLevelHandler(SkillId.spot, AddFamiliarSkillBonus, 2)
            .Build();


        [TempleDllLocation(0x102f1fc8)]
        public static readonly ConditionSpec ArmorShadow = ConditionSpec.Create("Armor Shadow", 3)
            .AddSkillLevelHandler(SkillId.hide, ArmorShadowSilentMovesSkillBonus)
            .Build();


        [TempleDllLocation(0x102f2340)]
        public static readonly ConditionSpec GoldenSkull = ConditionSpec.Create("Golden Skull", 9)
            .AddSignalHandler(D20DispatcherKey.SIG_Golden_Skull_Combine, sub_10102B70)
            .AddHandler(DispatcherType.RadialMenuEntry, GoldenSkullRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 122, sub_10103A00)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, GoldenSkullNewDay)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .Build();


        [TempleDllLocation(0x102f23c0)]
        public static readonly ConditionSpec ElementalGem = ConditionSpec.Create("Elemental Gem", 3)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.RadialMenuEntry, ElementalGemRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 122 /*ACTIVATE_DEVICE_SPELL*/,
                ElementalGemPerform)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ElementalGemNewdayRest)
            .AddQueryHandler(D20DispatcherKey.QUE_Elemental_Gem_State, ElementalGemQueryState)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 120 /*ACTIVATE_DEVICE_FREE*/,
                ElementalGemActionCheck)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 120 /*ACTIVATE_DEVICE_FREE*/,
                ElementalGemPerformFree)
            .Build();


        [TempleDllLocation(0x102f2468)]
        public static readonly ConditionSpec Fragarach = ConditionSpec.Create("Fragarach", 3)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage, FragarachDealingDmg)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 40)
            .AddHandler(DispatcherType.ToHitBonus2, FragarachToHitBonus)
            .AddHandler(DispatcherType.TakingDamage2, FragarachAnswering)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .Build();


        [TempleDllLocation(0x102f2500)]
        public static readonly ConditionSpec NormalProjectileParticles = ConditionSpec
            .Create("Normal Projectile Particles", 3)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-arrow normal")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .Build();


        [TempleDllLocation(0x102f2558)]
        public static readonly ConditionSpec ThievesTools = ConditionSpec.Create("Thieves Tools", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Has_Thieves_Tools, sub_101044F0)
            .Build();


        [TempleDllLocation(0x102f2588)]
        public static readonly ConditionSpec ThievesToolsMasterwork = ConditionSpec
            .Create("Thieves Tools Masterwork", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Has_Thieves_Tools, sub_101044F0)
            .AddSkillLevelHandler(SkillId.disable_device, ThievesToolsMasterworkSkillLevel)
            .AddSkillLevelHandler(SkillId.open_lock, ThievesToolsMasterworkSkillLevel)
            .Build();


        [TempleDllLocation(0x102f25e0)]
        public static readonly ConditionSpec CompositeBow = ConditionSpec.Create("Composite Bow", 3)
            .AddHandler(DispatcherType.ToHitBonus2, sub_10104700)
            .AddHandler(DispatcherType.DealingDamage, CompositeBowDamageBonus)
            .Build();


        [TempleDllLocation(0x102f26a0)]
        public static readonly ConditionSpec JaersSpheresofFire = ConditionSpec.Create("Jaer's Spheres of Fire", 3)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "sp-Spheres of Fire-proj")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .Build();


        [TempleDllLocation(0x102f26e8)]
        public static readonly ConditionSpec HolyWater = ConditionSpec.Create("Holy Water", 3)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-arrow normal")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage, HolyWaterOnDamage, 1)
            .Build();


        [TempleDllLocation(0x102f2740)]
        public static readonly ConditionSpec UnholyWater = ConditionSpec.Create("Unholy Water", 3)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-arrow normal")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage, sub_10104660, 1)
            .Build();


        [TempleDllLocation(0x102f2624)]
        public static readonly ConditionSpec BardicInstrument = ConditionSpec.Create("Bardic Instrument", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_BardicInstrument,
                CommonConditionCallbacks.D20Query_Callback_GetSDDKey1, 0)
            .Build();


        [TempleDllLocation(0x102f2798)]
        public static readonly ConditionSpec CharismaCompetenceBonus = ConditionSpec
            .Create("Charisma Competence Bonus", 3)
            .AddSkillLevelHandler(SkillId.bluff, CharismaCompetenceBonusSkillLevel)
            .AddSkillLevelHandler(SkillId.diplomacy, CharismaCompetenceBonusSkillLevel)
            .AddSkillLevelHandler(SkillId.gather_information, CharismaCompetenceBonusSkillLevel)
            .AddSkillLevelHandler(SkillId.intimidate, CharismaCompetenceBonusSkillLevel)
            .AddSkillLevelHandler(SkillId.use_magic_device, CharismaCompetenceBonusSkillLevel)
            .Build();


        [TempleDllLocation(0x102f2818)]
        public static readonly ConditionSpec NecklaceofDetection = ConditionSpec.Create("Necklace of Detection", 3)
            .AddSkillLevelHandler(SkillId.search, NecklaceOfDetectionSkillLevel)
            .AddSkillLevelHandler(SkillId.wilderness_lore, NecklaceOfDetectionSkillLevel)
            .Build();


        [TempleDllLocation(0x102f2860)]
        public static readonly ConditionSpec AmuletofMightyFists = ConditionSpec.Create("Amulet of Mighty Fists", 3)
            .AddHandler(DispatcherType.ToHitBonus2, AmuletOfMightyFistsToHitBonus)
            .AddHandler(DispatcherType.DealingDamage, AmuletOfMightyFistsDamageBonus)
            .Build();


        [TempleDllLocation(0x102f28a4)]
        public static readonly ConditionSpec AmuletofNaturalArmor = ConditionSpec.Create("Amulet of Natural Armor", 3)
            .AddHandler(DispatcherType.GetAC, amuletOfNaturalArmorACBonus)
            .Build();


        [TempleDllLocation(0x102f28d8)]
        public static readonly ConditionSpec BracersofArchery = ConditionSpec.Create("Bracers of Archery", 3)
            .AddHandler(DispatcherType.ToHitBonus2, BracersOfArcheryToHitBonus)
            .AddHandler(DispatcherType.DealingDamage, BracersOfArcheryDamageBonus)
            .Build();


        [TempleDllLocation(0x102f2920)]
        public static readonly ConditionSpec UseableItemXTimesPerDay = ConditionSpec
            .Create("Useable Item X Times Per Day", 4)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CondNodeAnullArg3)
            .AddHandler(DispatcherType.RadialMenuEntry, UseableItemXTimesPerDayRadialMenu)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.D20ActionPerform, D20DispatcherKey.LVL_Stats_Finalize,
                UseableItemXTimesPerDayPerform)
            .Build();


        [TempleDllLocation(0x102f2990)]
        public static readonly ConditionSpec Buckler = ConditionSpec.Create("Buckler", 3)
            .AddHandler(DispatcherType.ToHitBonus2, BucklerToHitPenalty)
            .AddHandler(DispatcherType.GetAC, BucklerAcBonus)
            .AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 0)
            .AddHandler(DispatcherType.BucklerAcPenalty, BucklerAcPenalty)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_AC_Bonus, QueryAcBonus, 0)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus, ArmorOrShieldGetMaxDexBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_Speed, ArmorOrShieldGetMaxSpeed)
            .AddSkillLevelHandler(SkillId.balance, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.climb, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.escape_artist, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.hide, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.jump, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.move_silently, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.pick_pocket, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.tumble, SkillLevelArmorPenalty)
            .AddSkillLevelHandler(SkillId.open_lock, sub_10100650)
            .AddSkillLevelHandler(SkillId.wilderness_lore, sub_10100650)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH, sub_10100650)
            .AddHandler(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_DEXTERITY, sub_10100650)
            .Build();


        [TempleDllLocation(0x102f2b28)]
        public static readonly ConditionSpec RodofSmiting = ConditionSpec.Create("Rod of Smiting", 3)
            .AddHandler(DispatcherType.GetCriticalHitExtraDice, RodOfSmiting_CritHit)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
            .Build();


        [TempleDllLocation(0x102f2b6c)]
        public static readonly ConditionSpec WeaponSilver = ConditionSpec.Create("Weapon Silver", 3)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 6)
            .Build();


        [TempleDllLocation(0x102f2b9c)]
        public static readonly ConditionSpec RingofChange = ConditionSpec.Create("Ring of Change", 3)
            .SetQueryResult(D20DispatcherKey.QUE_Wearing_Ring_of_Change, true)
            .Build();


        [TempleDllLocation(0x102f2cfc)]
        public static readonly ConditionSpec FailedCopyScroll = ConditionSpec.Create("Failed_Copy_Scroll", 2)
            .AddQueryHandler(D20DispatcherKey.QUE_Failed_Copy_Scroll, FailedCopyScrollQuery)
            .Build();


        public static IReadOnlyList<ConditionSpec> Conditions { get; } = new List<ConditionSpec>
        {
            WeaponSilver,
            WeaponFlaming,
            AttributeEnhancementBonus,
            WeaponFlamingBurst,
            SwordofLifeStealing,
            Fragarach,
            AmuletofMightyFists,
            WeaponDisruption,
            ArmorSilentMoves,
            WeaponFlameTongue,
            RingofChange,
            DeflectionBonus,
            FrostBow,
            RodofSmiting,
            WeaponChaotic,
            StaffOfStriking,
            Bootsofspeed,
            WeaponFrost,
            ItemParticleSystem,
            WeaponShockingBurst,
            SavingThrowResistanceBonus,
            Keoghtomsointment,
            UseableMagicStaff,
            RingofAnimalSummoning,
            AmuletofNaturalArmor,
            Ringoffreedomofmovement,
            WeaponMightyCleaving,
            NormalProjectileParticles,
            ToHitBonus,
            ArmorSpellResistance,
            ShieldBonus,
            WeaponIcyBurst,
            FailedCopyScroll,
            CompositeBow,
            Crossbow,
            CharismaCompetenceBonus,
            ArmorShadow,
            FamiliarSkillBonus,
            ShieldEnhancementBonus,
            ArmorBonus,
            ProofAgainstPoison,
            SkillCompetenceBonus,
            BracersofArchery,
            WeaponShock,
            ArmorEnhancementBonus,
            ThievesToolsMasterwork,
            ElementalGem,
            WeaponDefendingBonus,
            FamiliarSaveBonus,
            WeaponUnholy,
            UnholyWater,
            RingofInvisibility,
            GoldenSkull,
            ThievesTools,
            UseableItem,
            WeaponEnhancementBonus,
            LuckPoisonSaveBonus,
            Buckler,
            WeaponLawful,
            WeaponBane,
            NecklaceofDetection,
            JaersSpheresofFire,
            FamiliarHpBonus,
            ElementalResistance,
            SkillCircumstanceBonus,
            UseableItemXTimesPerDay,
            DaggerofVenom,
            HolyWater,
            WeaponKeen,
            WeaponHoly,
            BardicInstrument,
            ArmorMasterwork,
            ProofAgainstDetectionLocation,
            DamageBonus,
            ElementalResistanceperround,
            WeaponMasterwork,
        };

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x10102000)]
        public static void BootsOfSpeedPerform(in DispatcherCallbackArgs evt)
        {
            int v4;

            var condArg3 = evt.GetConditionArg3();
            var condArg4 = evt.GetConditionArg4();
            if (evt.GetDispIoD20ActionTurnBased().action.data1 == condArg3)
            {
                if ((condArg4) != 0)
                {
                    v4 = 0;
                    evt.KillPartSysInArg(4);
                }
                else
                {
                    v4 = 1;
                    var v6 = GameSystems.ParticleSys.CreateAtObj("sp-Haste", evt.objHndCaller);
                    evt.SetConditionPartSysArg(4, (PartSys) v6);
                }

                evt.SetConditionArg4(v4);
            }
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x10102800)]
        public static void SwordOfLifestealingDealDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var caf = dispIo.attackPacket.flags;
            if ((caf & D20CAF.RANGED) == 0 && dispIo.attackPacket.GetWeaponUsed() == item)
            {
                if ((caf & D20CAF.CRITICAL) != 0)
                {
                    dispIo.attackPacket.victim.AddCondition(StatusEffects.TempNegativeLevel, 0, 16);
                    var hpGained = Dice.D6.Roll();
                    evt.objHndCaller.AddCondition(StatusEffects.TemporaryHitPoints, 0, 14400, hpGained);
                }
            }
        }


        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x10103ae0)]
        public static void GoldenSkullNewDay(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            evt.SetConditionArg(7, 0);
            evt.SetConditionArg(8, 0);

            for (var i = 0; i < 4; i++)
            {
                var mask = 1 << (24 - i * 8);
                if ((condArg1 & mask) != 0)
                {
                    var argIndex = 3 + i;
                    var v5 = evt.GetConditionArg(argIndex);
                    var v6 = (v5 << 8 >> 24) - 1;
                    if (v6 < 0)
                    {
                        v6 = 0;
                    }

                    var v7 = (v5 << 16 >> 24) - 1;
                    if (v7 < 0)
                    {
                        v7 = 0;
                    }

                    evt.SetConditionArg(argIndex, v6 | ((v7 | (v6 << 8)) << 8));
                }
            }
        }


        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x10101eb0)]
        [TemplePlusLocation("ability_fixes.cpp:71")]
        public static void BootsOfSpeedNewday(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            evt.SetConditionArg1(condArg2);
        }
/* Orphan comments:
TP Replaced @ ability_fixes.cpp:71
*/


        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x10100b60)]
        [TemplePlusLocation("condition.cpp:454")]
        public static void UseableItemActionCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();

            var invIdx = evt.GetConditionArg3();

            // check if this is the referenced item
            if (dispIo.action.data1 != invIdx)
            {
                return;
            }

            var itemHandle = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, invIdx);
            var useMagicDeviceSkillBase = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.use_magic_device);

            // ensure is identified
            if (itemHandle.type != ObjectType.food && !GameSystems.Item.IsIdentified(itemHandle))
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            // check item charges
            var charges = itemHandle.GetInt32(obj_f.item_spell_charges_idx);
            if (charges == 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                return;
            }

            // check if caster needs and has spell/class
            var itemFlags = itemHandle.GetItemFlags();

            var spIdx = evt.GetConditionArg1();
            var spData = itemHandle.GetSpell(obj_f.item_spell_idx, spIdx);

            var handle = evt.objHndCaller;

            if (itemHandle.type == ObjectType.scroll || (itemFlags & ItemFlag.NEEDS_SPELL) != 0 &&
                (itemHandle.type == ObjectType.generic || itemHandle.type == ObjectType.weapon))
            {
                var isOk = false;


                if (useMagicDeviceSkillBase > 0 ||
                    GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spData.spellEnum))
                {
                    isOk = true;
                }

                // clerics with magic domain
                else if (GameSystems.Spell.IsArcaneSpellClass(spData.classCode))
                {
                    var clrLvl = handle.GetStat(Stat.level_cleric);
                    if (clrLvl > 0 && Math.Max(1, clrLvl / 2) >= spData.spellLevel &&
                        GameSystems.Critter.HasDomain(handle, DomainId.Magic))
                        isOk = true;
                }

                if (!isOk)
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    return;
                }
            }

            if (itemHandle.type == ObjectType.scroll &&
                !GameSystems.Spell.CheckAbilityScoreReqForSpell(evt.objHndCaller, spData.spellEnum) &&
                useMagicDeviceSkillBase == 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            return;
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ff960)]
        public static void WeaponUnholyDamageBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var unholyItem = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            var objHnd = dispIo.attackPacket.victim;
            if (IsWeaponOrAmmo(ref dispIo.attackPacket, unholyItem))
            {
                if ((objHnd.GetStat(Stat.alignment) & 4) != 0)
                {
                    var dice = new Dice(2, 6);
                    var itemName = GameSystems.MapObject.GetDisplayName(unholyItem, evt.objHndCaller);
                    dispIo.damage.AddDamageDice(dice, DamageType.Unspecified, 0x79, itemName);
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x10100370)]
        public static void ShieldAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var caf = dispIo.attackPacket.flags;
            if ((caf & D20CAF.TOUCH_ATTACK) == 0)
            {
                var acBonus = evt.GetConditionArg1();
                var condArg3 = evt.GetConditionArg3();
                var shield = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                var shieldName = GameSystems.MapObject.GetDisplayName(shield, evt.objHndCaller);
                dispIo.bonlist.AddBonus(acBonus, 29, 125, shieldName);
            }
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x10100f20)]
        public static void RingOfInvisPerform(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            if (condArg1 == evt.GetDispIoD20ActionTurnBased().action.data1)
            {
                if ((condArg2) != 0)
                {
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Magical_Item_Deactivate,
                        condArg1);
                    evt.SetConditionArg2(0);
                }
                else
                {
                    evt.objHndCaller.AddCondition(StatusEffects.Invisible, condArg1, 0);
                    evt.SetConditionArg2(1);
                }
            }
        }


        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x10101a20)]
        public static void sub_10101A20(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg2(1);
        }


        [DispTypes(DispatcherType.ConditionRemove)]
        [TempleDllLocation(0x10100ef0)]
        public static void RingOfInvisibilityRemove(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Magical_Item_Deactivate, condArg1);
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x10102970)]
        public static void SavingThrow_FamiliarBonus_Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();

            var saveType = (SavingThrowType) (evt.dispKey - D20DispatcherKey.SAVE_FORTITUDE);
            if (saveType == (SavingThrowType) evt.GetConditionArg1())
            {
                var condArg3 = evt.GetConditionArg3();
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                var bonusValue = evt.GetConditionArg2();
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(bonusValue, 0, 278, itemName);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fff20)]
        public static void QueryAcBonus(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Query();
            var item = (GameObjectBody) dispIo.obj;

            var condArg3 = evt.GetConditionArg3();
            var equippedItem = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);

            if (item == equippedItem)
            {
                dispIo.return_val += evt.GetConditionArg(data);
            }
        }

        private readonly struct ElementalGemTarget
        {
            // Line from combat.mes
            public readonly int RadialMenuMesLine;
            public readonly int MapId;
            public readonly locXY Location;

            public ElementalGemTarget(int mesLine, int mapId, locXY location)
            {
                RadialMenuMesLine = mesLine;
                MapId = mapId;
                Location = location;
            }
        }

        [TempleDllLocation(0x10290080)]
        private static readonly ElementalGemTarget[] ElementalGemTargets =
        {
            // Air Node
            new ElementalGemTarget(5096, 5081, new locXY(477, 484)),
            // Earth Node
            new ElementalGemTarget(5097, 5082, new locXY(484, 473)),
            // Fire Node
            new ElementalGemTarget(5098, 5083, new locXY(505, 496)),
            // Water Node
            new ElementalGemTarget(5099, 5084, new locXY(453, 473)),
        };

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x101040d0)]
        public static void ElementalGemPerformFree(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            if (evt.GetDispIoD20ActionTurnBased().action.data1 == condArg2)
            {
                evt.SetConditionArg1(condArg1 | 0x1000000);

                var teleportArgs = FadeAndTeleportArgs.Default;
                var target = ElementalGemTargets[condArg2];
                teleportArgs.flags = FadeAndTeleportFlags.CenterOnPartyLeader;
                teleportArgs.somehandle = GameSystems.Party.GetLeader();
                teleportArgs.destLoc = target.Location;
                teleportArgs.destMap = target.MapId;
                GameSystems.Teleport.FadeAndTeleport(in teleportArgs);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ff670)]
        [TemplePlusLocation("general_condition_fixes.cpp:38")]
        public static void WeaponKeepBonus(in DispatcherCallbackArgs evt)
        {
            evt.GetDispIoD20Query().return_val = 2;
        }

        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x10104cc0)]
        public static void CondNodeAnullArg3(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg4(0);
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x10104080)]
        public static void ElementalGemActionCheck(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            if (evt.GetDispIoD20ActionTurnBased().action.data1 == condArg2)
            {
                // TODO: I think vanilla meant to limit use of the gem somehow related to whether you were on the map or not, but returning anything from this function is pointless
                // GameSystems.Map.GetCurrentMapId();
                // ElementalGemTargets[condArg2].MapId;
            }
        }

        [DispTypes(DispatcherType.ItemForceRemove)]
        [TempleDllLocation(0x10104410)]
        public static void ItemForceRemoveCallback_SetItemPadWielderArgs(in DispatcherCallbackArgs evt)
        {
            var argCount = 0;
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArrayLength = item.GetArrayLength(obj_f.item_pad_wielder_condition_array);
            for (var i = 0; i < condArrayLength; i++)
            {
                var nameHash = item.GetInt32(obj_f.item_pad_wielder_condition_array, i);
                var cond = GameSystems.D20.Conditions.GetByHash(nameHash);
                if (cond == evt.subDispNode.condNode.condStruct)
                {
                    for (var j = 0; j < cond.numArgs; ++j)
                    {
                        var arg = evt.GetConditionArg(j);
                        item.SetInt32(obj_f.item_pad_wielder_argument_array, argCount + j, arg);
                    }

                    return;
                }

                argCount += cond.numArgs;
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10104990)]
        public static void AmuletOfMightyFistsToHitBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoAttackBonus();
            if (dispIo.attackPacket.GetWeaponUsed() == null)
            {
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(condArg1, 0, 112, itemName);
            }
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x10102140)]
        public static void BootsOfSpeedSaveThrow(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            if ((evt.GetConditionArg4()) != 0)
            {
                var dispIo = evt.GetDispIoSavingThrow();
                if (evt.dispKey == D20DispatcherKey.SAVE_REFLEX)
                {
                    dispIo.bonlist.AddBonus(data1, 8, data2);
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x10104ee0)]
        public static void BucklerAcBonus(in DispatcherCallbackArgs args)
        {
            var condArg2 = args.GetConditionArg2();
            var condArg3 = args.GetConditionArg3();

            var dispIo = args.GetDispIoAttackBonus();
            if (condArg2 != 0)
            {
                // Bonus lost due to second hand being used in an attack this round
                dispIo.bonlist.zeroBonusSetMeslineNum(326);
            }
            else
            {
                var buckler = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, condArg3);
                var bucklerName = GameSystems.MapObject.GetDisplayName(buckler, args.objHndCaller);
                var caf = dispIo.attackPacket.flags;
                if ((caf & D20CAF.TOUCH_ATTACK) == 0)
                {
                    var acBonus = args.GetConditionArg1();
                    dispIo.bonlist.AddBonus(acBonus, 29, 125, bucklerName);
                }
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10104880)]
        public static void CharismaCompetenceBonusSkillLevel(in DispatcherCallbackArgs args)
        {
            var condArg1 = args.GetConditionArg1();
            var condArg3 = args.GetConditionArg3();
            var dispIo = args.GetDispIoObjBonus();
            if (condArg3 >= 100) // TODO weird check
            {
                var item = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, condArg3);
                var itemName = GameSystems.MapObject.GetDisplayName(item, args.objHndCaller);
                dispIo.bonlist.AddBonus(condArg1, 34, 112, itemName);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10101960)]
        public static void DaggerOfVenomRadial(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (GameSystems.Item.IsIdentified(item) && condArg2 != 0)
            {
                var radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_STANDARD;
                radMenuEntry.d20ActionData1 = condArg1;
                radMenuEntry.text = GameSystems.MapObject.GetDisplayName(item);
                radMenuEntry.helpSystemHashkey = "TAG_DAGGER";
                var v7 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v7);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x10104660)]
        public static void sub_10104660(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            var v2 = dispIo.attackPacket.GetWeaponUsed();
            if (v2 != null)
            {
                if (GameSystems.Critter.IsCategory(dispIo.attackPacket.victim, MonsterCategory.outsider)
                    && (dispIo.attackPacket.victim.GetStat(Stat.alignment) & 4) != 0)
                {
                    var v3 = GameSystems.MapObject.GetDisplayName(v2);
                    var v4 = new Dice(2, 4);
                    dispIo.damage.AddDamageDice(v4, DamageType.Acid, 0x79, v3);
                }
                else
                {
                    dispIo.damage = new DamagePacket();
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x10101840)]
        public static void DaggerOfVenomPoisonOnDamage(in DispatcherCallbackArgs args)
        {
            var condArg3 = args.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, condArg3);
            var condArg4 = args.GetConditionArg4();
            var condArg2 = args.GetConditionArg2();
            if ((condArg4) != 0)
            {
                var dispIo = args.GetDispIoDamage();
                if (item == dispIo.attackPacket.GetWeaponUsed())
                {
                    args.SetConditionArg4(0);
                    args.SetConditionArg2(condArg2 - 1);
                    dispIo.attackPacket.victim.AddCondition(StatusEffects.Poisoned, 20, 10, 14);
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x10104190)]
        public static void FragarachDealingDmg(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if ((evt.objHndCaller.GetBaseStat(Stat.alignment) & 8) == 0)
            {
                var dispIo = evt.GetDispIoDamage();
                var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
                if (item == weaponUsed)
                {
                    var damageBonus = 4;
                    var attackerAlignment = evt.objHndCaller.GetAlignment();
                    var victim = dispIo.attackPacket.victim;
                    // TODO: This condition seems to be the exact opposite of what it should be
                    if ((dispIo.attackPacket.flags & D20CAF.CRITICAL) == 0
                        && !victim.HasEvilAlignment()
                        && (attackerAlignment == Alignment.CHAOTIC || attackerAlignment == Alignment.GOOD))
                    {
                        damageBonus = 8;
                    }

                    var itemName = GameSystems.MapObject.GetDisplayName(weaponUsed, evt.objHndCaller);
                    dispIo.damage.AddDamageBonus(damageBonus, 12, 112, itemName);
                }
            }
        }


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x101027c0)]
        public static void sub_101027C0(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            if (evt.GetDispIoD20ActionTurnBased().action.data1 == condArg3)
            {
                evt.SetConditionArg2(0);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ff5f0)]
        public static void WeaponMasterworkToHitBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
            if (item == weaponUsed)
            {
                var weaponName = GameSystems.MapObject.GetDisplayName(weaponUsed, evt.objHndCaller);
                dispIo.bonlist.AddBonus(1, 12, 241, weaponName);
            }
        }


        [DispTypes(DispatcherType.BucklerAcPenalty)]
        [TempleDllLocation(0x10104e40)]
        [TemplePlusLocation("condition.cpp:467")]
        public static void BucklerAcPenalty(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var weapon = dispIo.attackPacket.GetWeaponUsed();
            GameSystems.MapObject.GetDisplayName(weapon, evt.objHndCaller);
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) == 0 && weapon != null)
            {
                var attackCode = dispIo.attackPacket.dispKey;
                if (GameSystems.Item.GetWieldType(evt.objHndCaller, weapon) == 2
                    || attackCode == 6
                    || attackCode == 8)
                {
                    evt.SetConditionArg2(1);
                }
            }
        }

        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x101011f0)]
        public static void AttributeEnhancementBonus_callback(in DispatcherCallbackArgs evt)
        {
            var condArg1 = (Stat) evt.GetConditionArg1();
            var bonusValue = evt.GetConditionArg2();
            var queriedStat = evt.GetAttributeFromDispatcherKey();
            if (queriedStat == condArg1)
            {
                var condArg3 = evt.GetConditionArg3();
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                var dispIo = evt.GetDispIoBonusList();
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(bonusValue, 12, 112, itemName);
            }
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x10101450)]
        public static void WeaponPlayParticleOnHit(in DispatcherCallbackArgs evt, string partSysName)
        {
            var dispIo = evt.GetDispIoDamage();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (dispIo.attackPacket.GetWeaponUsed() == item)
            {
                if (partSysName != null)
                {
                    // TODO: If the particle system is permanent, this leaks...
                    GameSystems.ParticleSys.CreateAtObj(partSysName, dispIo.attackPacket.victim);
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x10102ae0)]
        public static void FailedCopyScrollQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var spellEnum = dispIo.data1;
            var spellEnumAttempted = evt.GetConditionArg1();
            var skillRanksWhenAttempted = evt.GetConditionArg2();
            if (GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.spellcraft) > skillRanksWhenAttempted)
            {
                // Allow it when the player has improved their spellcraft skill
                evt.RemoveThisCondition();
                dispIo.return_val = 0;
                return;
            }

            if (spellEnum != spellEnumAttempted)
            {
                // This condition only applies to this
                dispIo.return_val = 0;
                return;
            }

            dispIo.return_val = 1;
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x10104a20)]
        public static void AmuletOfMightyFistsDamageBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            if ((dispIo.attackPacket.GetWeaponUsed() == null))
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.damage.AddDamageBonus(condArg1, 0, 112, itemName);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10104910)]
        public static void NecklaceOfDetectionSkillLevel(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoObjBonus();
            if (condArg3 >= 100)
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(5, 0, 112, itemName);
            }
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x101022f0)]
        public static void BootsOfSpeedTooltip(in DispatcherCallbackArgs evt, int combatMesLine)
        {
            if (evt.GetConditionArg4() != 0)
            {
                var dispIo = evt.GetDispIoTooltip();
                dispIo.Append(GameSystems.D20.Combat.GetCombatMesLine(combatMesLine));
            }
        }

        private static bool IsWeaponOrAmmo(ref AttackPacket attackPacket, GameObjectBody item)
        {
            var weaponUsed = attackPacket.GetWeaponUsed();
            if (weaponUsed == item)
            {
                return true;
            }

            var attacker = attackPacket.attacker;
            return GameSystems.Item.IsRangedWeapon(weaponUsed)
                   && GameSystems.Item.ItemWornAt(attacker, EquipSlot.Ammo) == item;
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ffa50)]
        public static void sub_100FFA50(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            var victim = dispIo.attackPacket.victim;
            if (IsWeaponOrAmmo(ref dispIo.attackPacket, item))
            {
                if ((victim.GetStat(Stat.alignment) & 8) != 0)
                {
                    var v9 = new Dice(2, 6);
                    var v10 = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                    dispIo.damage.AddDamageDice(v9, DamageType.Unspecified, 0x79, v10);
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fffd0)]
        public static void ArmorOrShieldGetMaxSpeed(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var obj = (GameObjectBody) dispIo.obj;
            var wearer = GameSystems.Item.GetParent(obj);
            if (wearer == null)
            {
                dispIo.return_val = 100;
                return;
            }

            if (GameSystems.Item.ItemWornAt(wearer, EquipSlot.Armor) != obj
                || GameSystems.D20.D20Query(wearer, D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium)
                || GameSystems.D20.D20Query(wearer, D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy)
                || GameSystems.D20.D20Query(wearer, D20DispatcherKey.QUE_Critter_Is_Encumbered_Overburdened))
            {
                dispIo.return_val = 100;
                return;
            }

            var armorType = obj.GetArmorFlags().GetArmorType();
            if (armorType.IsLightArmorOrLess() || armorType == ArmorFlag.TYPE_SHIELD)
            {
                dispIo.return_val = 100;
                return;
            }

            var baseSpeed = 30;
            if (wearer.IsCritter())
            {
                baseSpeed = (int) wearer.Dispatch40GetMoveSpeedBase(out _, out _);
            }

            dispIo.return_val = baseSpeed > 20 ? 20 : 15;
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fff90)]
        public static void ArmorOrShieldGetMaxDexBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var obj = (GameObjectBody) dispIo.obj;
            if (obj.type == ObjectType.armor)
            {
                dispIo.return_val = obj.GetInt32(obj_f.armor_max_dex_bonus);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10102a70)]
        public static void AddFamiliarSkillBonus(in DispatcherCallbackArgs evt, int bonusValue)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoObjBonus();
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            dispIo.bonlist.AddBonus(bonusValue, 0, 279, itemName);
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10101150)]
        [TemplePlusLocation("condition.cpp:431")]
        public static void SkillBonusCallback(in DispatcherCallbackArgs evt, int bonType)
        {
            /*
            used by conditions: Skill Circumstance Bonus, Skill Competence Bonus
            */
            var skillEnum = (SkillId) evt.GetConditionArg1();
            var bonValue = evt.GetConditionArg2();
            var usedSkillId = evt.GetSkillIdFromDispatcherKey();
            if (usedSkillId == skillEnum)
            {
                int invIdx = evt.GetConditionArg3();
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, invIdx);
                var dispIo = evt.GetDispIoObjBonus();
                var name = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonOut.AddBonus(bonValue, bonType, 112, name);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x101047c0)]
        public static void CompositeBowDamageBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
            if (item == weaponUsed)
            {
                var damageBonus = evt.objHndCaller.GetStat(Stat.str_mod);
                // It's capped by the bow's strength bonus
                if (damageBonus > condArg1)
                {
                    damageBonus = condArg1;
                }

                if (damageBonus > 0)
                {
                    var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                    dispIo.damage.AddDamageBonus(damageBonus, 0, 312, itemName);
                }
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10101660)]
        public static void StaffOfStrikingRadial(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (GameSystems.Item.IsIdentified(v3) && condArg1 >= 1)
            {
                if (evt.objHndCaller.DispatchToHitBonusBase() > 0)
                {
                    var radMenuEntry = evt.CreateSliderForArg(1, 0, 3);
                    radMenuEntry.d20ActionType = D20ActionType.NONE;
                    radMenuEntry.text = GameSystems.D20.Combat.GetCombatMesLine(5075);
                    radMenuEntry.helpSystemHashkey = "TAG_RADIAL_MENU_STAFF_OF_STRIKING";
                    var v6 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v6);
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ffb40)]
        public static void WeaponLawfulDamageBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            var victim = dispIo.attackPacket.victim;
            if (IsWeaponOrAmmo(ref dispIo.attackPacket, item))
            {
                if (victim.HasChaoticAlignment())
                {
                    var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                    dispIo.damage.AddDamageDice(new Dice(2, 6), DamageType.Unspecified, 0x79, itemName);
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x101002e0)]
        public static void DeflectionAcBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var bonValue = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoAttackBonus();
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            var caf = dispIo.attackPacket.flags;
            if ((caf & D20CAF.TOUCH_ATTACK) == 0)
            {
                dispIo.bonlist.AddBonus(bonValue, 11, 148, itemName);
            }
        }

        [DispTypes(DispatcherType.D20Signal, DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x100ed3e0)]
        public static void ItemParticleSystemShow(in DispatcherCallbackArgs evt, int partSysArgIdx, int partSysNameHashArgIdx)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (item != null)
            {
                evt.EndPartSysInArg(partSysArgIdx);

                var partSysNameHash = evt.GetConditionArg(partSysNameHashArgIdx);
                var partSys = (PartSys) GameSystems.ParticleSys.CreateAtObj(partSysNameHash, item);
                evt.SetConditionPartSysArg(partSysArgIdx, partSys);
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x10104ab0)]
        public static void amuletOfNaturalArmorACBonus(in DispatcherCallbackArgs evt)
        {
            var bonusValue = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            dispIo.bonlist.AddBonus(bonusValue, 9, 112, itemName);
        }

        [DispTypes(DispatcherType.ProjectileDestroyed)]
        [TempleDllLocation(0x101013d0)]
        public static void ProjectileDestroyedEndParticles(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var ammoItem = dispIo.attackPacket.ammoItem;
            if (ammoItem != null && dispIo.attackPacket.weaponUsed == item)
            {
                var projectilePartSysId = ammoItem.GetInt32(obj_f.projectile_part_sys_id);
                if (projectilePartSysId != null)
                {
                    GameSystems.ParticleSys.End(projectilePartSysId);
                    throw new NotImplementedException();
                }
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10100790)]
        public static void ReloadRadial(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = RadialMenuEntry.Create();
            radMenuEntry.d20ActionType = D20ActionType.RELOAD;
            radMenuEntry.d20ActionData1 = 0;
            var weapon = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponPrimary);
            if (weapon != null)
            {
                if (weapon.GetWeaponType() == WeaponType.heavy_crossbow)
                {
                    radMenuEntry.helpSystemHashkey = "TAG_WEAPONS_CROSSBOW_HEAVY";
                }
                else
                {
                    radMenuEntry.helpSystemHashkey = "TAG_WEAPONS_CROSSBOW_LIGHT";
                }
                radMenuEntry.text = GameSystems.D20.Combat.GetCombatMesLine(5009);
                var offenseNode = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Offense);
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, offenseNode);
            }
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x10104330)]
        [TemplePlusLocation("generalfixes.cpp:241")]
        public static void FragarachAnswering(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            // Fixes the Fragarach hang that is caused by attacking a fire creature (which deals damage to the caster
            // -> triggers the answering ability - > attempts an AoO -> but there is no one to AoO!)
            // checks if the current TB actor is the same as the "attachee" (critter taking damage)
            // if so, aborts the answering (you can have an AoO on your turn!)
            var curActor = GameSystems.D20.Initiative.CurrentActor;
            var tgtObj = dispIo.attackPacket.victim;
            if (tgtObj == null || !tgtObj.IsCritter() || curActor == evt.objHndCaller)
            {
                Logger.Info("Prevented Scather AoO bug! TB Actor is {0}, Attachee is {1},  target is {2}",
                    GameSystems.MapObject.GetDisplayName(curActor), evt.objHndCaller, tgtObj);
                // got a crash report once from the getDisplayName here when triggered by a trap apparently, so disabling it
                return;
            }

            /*
            disable AoO effect for other identical conditions (so you don't get the 2 AoO Hang)
            // TODO: Makes this work like Great Cleave for maximum munchkinism
            */
            var condArg1 = evt.GetConditionArg1();
            if (condArg1 != 1)
            {
                return;
            }

            if (evt.objHndCaller.GetDispatcher() is Dispatcher dispatcher)
            {
                foreach (var itemCond in dispatcher.itemConds)
                {
                    if (itemCond.condStruct == evt.subDispNode.condNode.condStruct
                        && itemCond != evt.subDispNode.condNode)
                    {
                        itemCond.args[0] = 0;
                    }
                }
            }

            if (!evt.objHndCaller.HasEvilAlignment() && evt.objHndCaller.HasChaoticAlignment())
            {
                if (evt.objHndCaller.GetAlignment() == Alignment.CHAOTIC)
                {
                    if (condArg1 <= 0)
                    {
                        return;
                    }

                    evt.SetConditionArg1(0);
                }

                var distance = dispIo.attackPacket.attacker.DistanceToInFeetClamped(evt.objHndCaller);
                if (distance <= evt.objHndCaller.GetReach())
                {
                    GameSystems.Anim.Interrupt(evt.objHndCaller, AnimGoalPriority.AGP_HIGHEST);
                    GameSystems.D20.Actions.DoAoo(evt.objHndCaller, dispIo.attackPacket.attacker);
                    GameSystems.D20.Actions.sequencePerform();
                }
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x10101b60)]
        public static void WeaponDefendingArmorBonus(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (condArg2 > 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(condArg2, 0, 112, itemName);
            }
        }


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x10103f40)]
        public static void ElementalGemPerform(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var data1 = evt.GetDispIoD20ActionTurnBased().action.data1;
            if (data1 >> 16 == condArg3)
            {
                switch (data1 & 0xFFFF)
                {
                    case 1:
                        condArg1 = unchecked((int) (condArg1 & 0xFF07FFFF | 0x70000));
                        break;
                    case 2:
                        condArg1 = unchecked((int) (condArg1 & 0xFFFF07FF | 0x700));
                        break;
                    case 3:
                        condArg1 = unchecked((int) (condArg1 & 0xFFFFFF07 | 7));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                evt.SetConditionArg1(condArg1);
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x101028e0)]
        public static void FamiliarSkillBonus2(in DispatcherCallbackArgs evt)
        {
            var skillId = (SkillId) evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoObjBonus();
            var usedSkillId = evt.GetSkillIdFromDispatcherKey();
            if (usedSkillId == skillId)
            {
                var bonusValue = evt.GetConditionArg2();
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonOut.AddBonus(bonusValue, 0, 278, itemName);
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x10101280)]
        public static void AddSavingThrowResistanceBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = (SavingThrowType) evt.GetConditionArg1();
            var bonusValue = evt.GetConditionArg2();
            var usedSavingThrow = evt.GetSavingThrowTypeFromDispatcherKey();
            if (usedSavingThrow == condArg1)
            {
                var condArg3 = evt.GetConditionArg3();
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                var dispIo = evt.GetDispIoSavingThrow();
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(bonusValue, 15, 112, itemName);
            }
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x10101540)]
        public static void AddLuckPoisonSaveBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoSavingThrow();
            if ((dispIo.flags & D20SavingThrowFlag.POISON) != 0)
            {
                var v4 = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(condArg1, 15, 112, v4);
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x101020a0)]
        [TemplePlusLocation("ability_fixes.cpp:72")]
        public static void BootsOfSpeedBeginRound(in DispatcherCallbackArgs evt)
        {
            var roundsRem = evt.GetConditionArg1();
            var isOn = evt.GetConditionArg4();
            if (isOn == 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoD20Signal();
            var roundsNew = roundsRem - dispIo.data1;
            if (roundsNew >= 0){
                evt.SetConditionArg1(roundsNew);
                return;
            }

            // this was a bug in vanilla! should just reset the isOn arg, as below
            // conds.ConditionRemove(args.objHndCaller, args.subDispNode->condNode);
            evt.SetConditionArg1(0);
            evt.SetConditionArg4(0);
            evt.EndPartSysInArg(4);
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x10101f90)]
        public static void BootsOfSpeedD20Check(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var condArg4 = evt.GetConditionArg4();
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            if (dispIo.action.data1 == condArg3 && condArg4 == 0)
            {
                var tbStatus = dispIo.tbStatus;
                if ((tbStatus.tbsFlags & TurnBasedStatusFlags.HasActedThisRound) != 0)
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                }
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x101024a0)]
        public static void BaneWeaponDamageBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoDamage();

            if (FavoredEnemies.IsOfType(dispIo.attackPacket.victim, condArg1))
            {
                var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
                var weaponName = GameSystems.MapObject.GetDisplayName(weaponUsed, evt.objHndCaller);
                dispIo.damage.AddDamageDice(Dice.Constant(2), DamageType.Unspecified, 125, weaponName);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10104bf0)]
        public static void BracersOfArcheryToHitBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weaponUsed != null)
            {
                var weaponType = weaponUsed.GetWeaponType();
                if (GameSystems.Weapon.IsBow(weaponType))
                {
                    var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                    if (GameSystems.Feat.IsProficientWithWeaponType(evt.objHndCaller, weaponType))
                    {
                        dispIo.bonlist.AddBonus(condArg1, 34, 112, itemName);
                    }
                    else
                    {
                        dispIo.bonlist.AddCap(37, 0, 112, itemName);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x101045b0)]
        public static void HolyWaterOnDamage(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            var v2 = dispIo.attackPacket.GetWeaponUsed();
            if (v2 != null)
            {
                if (GameSystems.Critter.IsCategory(dispIo.attackPacket.victim, MonsterCategory.undead)
                    || GameSystems.Critter.IsCategory(dispIo.attackPacket.victim, MonsterCategory.outsider)
                    && (dispIo.attackPacket.victim.GetStat(Stat.alignment) & 8) != 0)
                {
                    var v3 = GameSystems.MapObject.GetDisplayName(v2);
                    var v4 = new Dice(2, 4);
                    dispIo.damage.AddDamageDice(v4, DamageType.Acid, 0x79, v3);
                }
                else
                {
                    dispIo.damage = new DamagePacket();
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10104700)]
        public static void sub_10104700(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var v5 = dispIo.attackPacket.GetWeaponUsed();
            if (v3 == v5)
            {
                var v6 = evt.objHndCaller.GetStat(0);
                if (D20StatSystem.GetModifierForAbilityScore(v6) < condArg1)
                {
                    var v7 = GameSystems.MapObject.GetDisplayName(v5, evt.objHndCaller);
                    dispIo.bonlist.AddBonus(-2, 0, 313, v7);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10101a40)]
        public static void sub_10101A40(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            if (condArg2 > 0)
            {
                var condArg3 = evt.GetConditionArg3();
                var v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                var dispIo = evt.GetDispIoAttackBonus();
                var v5 = dispIo.attackPacket.GetWeaponUsed();
                if (v3 == v5)
                {
                    var v6 = GameSystems.MapObject.GetDisplayName(v5, evt.objHndCaller);
                    dispIo.bonlist.AddBonus(-condArg2, 0, 112, v6);
                }
            }
        }


        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x10103fd0)]
        public static void ElementalGemNewdayRest(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var v2 = (condArg1 << 8 >> 24) - 1;
            if (v2 < 0)
            {
                v2 = 0;
            }

            var v3 = (condArg1 << 16 >> 24) - 1;
            if (v3 < 0)
            {
                v3 = 0;
            }

            evt.SetConditionArg1(v2 | ((v3 | (v2 << 8)) << 8));
        }


        [DispTypes(DispatcherType.ProjectileCreated)]
        [TempleDllLocation(0x10101310)]
        public static void ProjectileCreatileParticles(in DispatcherCallbackArgs evt, string partSysName)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var ammoItem = dispIo.attackPacket.ammoItem;
            var weaponUsed = dispIo.attackPacket.weaponUsed;
            if (dispIo.attackPacket.ammoItem
                != null && (weaponUsed == item ||
                            condArg3 == 209 && GameSystems.Item.AmmoMatchesWeapon(weaponUsed, item))
                        && ammoItem.GetInt32(obj_f.projectile_part_sys_id) == 0)
            {
                if (partSysName != null)
                {
                    var partSys = GameSystems.ParticleSys.CreateAtObj(partSysName, ammoItem);
                    ammoItem.SetInt32(obj_f.projectile_part_sys_id, (int) partSys);
                    throw new NotImplementedException();
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x10101820)]
        public static void DaggerOfVenomStatusInit(in DispatcherCallbackArgs evt)
        {
            throw new NotImplementedException();
            // evt.SetConditionArg1((int) evt.subDispNode.condNode);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10100cf0)]
        public static void UseableMagicStaffRadial(in DispatcherCallbackArgs evt)
        {
            int a3;
            int classCode;
            int spellLvl;

            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var invIdx = condArg3;
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var charges = GameSystems.Item.GetItemSpellCharges(item);
            if (GameSystems.Item.IsIdentified(item) && charges > 0)
            {
                var radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.text = GameSystems.MapObject.GetDisplayName(item, item);
                var itemsNode = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                var parentIdx =
                    GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, itemsNode);
                int index;
                for (index = 0; index < condArg1; ++index)
                {
                    var spData = item.GetSpell(obj_f.item_spell_idx, index);
                    if (GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spData.spellEnum))
                    {
                        var chargesNeeded = 1;
                        if (spData.spellEnum == WellKnownSpells.RaiseDead)
                        {
                            chargesNeeded = 5;
                        }

                        if (charges >= chargesNeeded)
                        {
                            sub_10079DB0 /*0x10079db0*/(spData.spellEnum, evt.objHndCaller, (int) &a3);
                            radMenuEntry = RadialMenuEntry.Create();
                            radMenuEntry.d20SpellData.SetSpellData(spData.spellEnum, classCode, spellLvl, invIdx, 0);
                            radMenuEntry.d20ActionType = D20ActionType.USE_ITEM;
                            radMenuEntry.d20ActionData1 = 0;
                            radMenuEntry.text = GameSystems.Spell.GetSpellName(spData.spellEnum);
                            radMenuEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spData.spellEnum);
                            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
                        }
                    }
                }
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x10102760)]
        public static void QueryMasterwork(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo != null)
            {
                var condArg3 = evt.GetConditionArg3();
                var obj = (GameObjectBody) dispIo.obj;
                if (obj == GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3))
                {
                    dispIo.return_val = 1;
                }
            }
        }


        [DispTypes(DispatcherType.BeginRound, DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x10101c90)]
        public static void ElementalResistancePerRoundRefresh(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            evt.SetConditionArg4(condArg2);
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x10102240)]
        public static void BootsOfSpeed_MovementBonus_Callback(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg4()) != 0)
            {
                var dispIo = evt.GetDispIoMoveSpeed();
                dispIo.bonlist.AddBonus(30, 12, 174);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ff690)]
        public static void ItemRemoveEnhancement(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == evt.GetConditionArg(4))
            {
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x10102b70)]
        public static void sub_10102B70(in DispatcherCallbackArgs evt)
        {
            GameObjectBody v5;
            int v7;
            CHAR v8;
            SubDispNode* v13;

            var dispIo = evt.GetDispIoD20Signal();
            var condArg3 = evt.GetConditionArg3();
            var v25 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var v3 = GameSystems.Proto.GetProtoById(12019);
            var v4 = GameSystems.Proto.GetProtoById(12020);
            var ObjHnd = GameSystems.Proto.GetProtoById(12021);
            var v22 = GameSystems.Proto.GetProtoById(12022);
            HIDWORD(v5) = dispIo.data2;
            var arg1 = dispIo.data1;
            int arg2 = *(_QWORD*) &dispIo.data1 >> 32;
            LODWORD(v5) = *(_QWORD*) &dispIo.data1;
            var v6 = GameSystems.MapObject.GetDisplayName(*(_QWORD*) &dispIo.data1, v5);
            if (!strcmp(v6, GameSystems.MapObject.GetDisplayName(v3, v3)))
            {
                v7 = 3;
                v8 = 24;
            }
            else if (!strcmp(v6, GameSystems.MapObject.GetDisplayName(v4, v4)))
            {
                v7 = 4;
                v8 = 16;
            }
            else if (!strcmp(v6, GameSystems.MapObject.GetDisplayName(ObjHnd, ObjHnd)))
            {
                v7 = 5;
                v8 = 8;
            }
            else
            {
                if (strcmp(v6, GameSystems.MapObject.GetDisplayName(v22, v22)))
                {
                    return;
                }

                v7 = 6;
                v8 = 0;
            }

            if (!((1 << v8) & condArg1))
            {
                var v9 = condArg1 | (1 << v8);
                evt.SetConditionArg1(condArg1 | (1 << v8));
                int v10 = GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Elemental_Gem_State, arg1,
                    arg2);
                evt.SetConditionArg(v7, v10);
                if (v9 > 0x1000000)
                {
                    if (v9 > 16842752)
                    {
                        var v18 = v9 - 16842753;
                        if ((v18) == 0)
                        {
                            v13 = (SubDispNode*) 344;
                            goto LABEL_44;
                        }

                        var v19 = v18 - 255;
                        if ((v19) == 0)
                        {
                            v13 = (SubDispNode*) 337;
                            goto LABEL_44;
                        }

                        if (v19 == 1)
                        {
                            v13 = (SubDispNode*) 336;
                            goto LABEL_44;
                        }
                    }
                    else
                    {
                        if (v9 == 16842752)
                        {
                            v13 = (SubDispNode*) 338;
                            goto LABEL_44;
                        }

                        var v16 = v9 - 16777217;
                        if ((v16) == 0)
                        {
                            v13 = (SubDispNode*) 345;
                            goto LABEL_44;
                        }

                        var v17 = v16 - 255;
                        if ((v17) == 0)
                        {
                            v13 = (SubDispNode*) 342;
                            goto LABEL_44;
                        }

                        if (v17 == 1)
                        {
                            v13 = (SubDispNode*) 340;
                            goto LABEL_44;
                        }
                    }
                }
                else
                {
                    if (v9 == 0x1000000)
                    {
                        v13 = (SubDispNode*) 346;
                        goto LABEL_44;
                    }

                    if (v9 > 0x10000)
                    {
                        var v14 = v9 - 65537;
                        if ((v14) == 0)
                        {
                            v13 = (SubDispNode*) 334;
                            goto LABEL_44;
                        }

                        var v15 = v14 - 255;
                        if ((v15) == 0)
                        {
                            v13 = (SubDispNode*) 335;
                            goto LABEL_44;
                        }

                        if (v15 == 1)
                        {
                            v13 = (SubDispNode*) 333;
                            goto LABEL_44;
                        }
                    }
                    else
                    {
                        if (v9 == 0x10000)
                        {
                            v13 = (SubDispNode*) 339;
                            goto LABEL_44;
                        }

                        var v11 = v9 - 1;
                        if ((v11) == 0)
                        {
                            v13 = (SubDispNode*) 332;
                            goto LABEL_44;
                        }

                        var v12 = v11 - 255;
                        if ((v12) == 0)
                        {
                            v13 = (SubDispNode*) 343;
                            goto LABEL_44;
                        }

                        if (v12 == 1)
                        {
                            v13 = (SubDispNode*) 341;
                            LABEL_44:
                            v25.SetInt32(obj_f.item_inv_aid, (int) v13);
                            return;
                        }
                    }
                }

                v13 = evt.subDispNode;
                goto LABEL_44;
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10104ce0)]
        public static void UseableItemXTimesPerDayRadialMenu(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            if (evt.GetConditionArg4() < condArg2)
            {
                ItemEffects.UseableItemRadialEntry(in evt);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10103b90)]
        public static void ElementalGemRadial(in DispatcherCallbackArgs evt)
        {
            int meslineKey;
            SpellStoreData spellData;

            var condArg3 = evt.GetConditionArg3();
            var condArg2 = evt.GetConditionArg2();
            var gem = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (!GameSystems.Item.IsIdentified(gem))
            {
                // Gem cannot be used when unidentified
                return;
            }

            var radMenuEntry = RadialMenuEntry.Create();
            radMenuEntry.text = GameSystems.MapObject.GetDisplayName(gem, gem);
            var itemNode = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
            var parentIdx = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, itemNode);
            var condArg1 = evt.GetConditionArg1();
            var gemTarget = ElementalGemTargets[condArg2];

            if (((condArg1 >> 24) & 0xFF) == 0 && GameSystems.Map.GetCurrentMapId() != gemTarget.MapId)
            {
                var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(gemTarget.RadialMenuMesLine);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.text = meslineValue;
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_FREE;
                radMenuEntry.d20ActionData1 = condArg2;
                radMenuEntry.helpSystemHashkey = "TAG_GOLDEN_SKULL";
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }

            if (((condArg1 >> 16) & 0xFF) == 0)
            {
                spellData = gem.GetSpell(obj_f.item_spell_idx, 0);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellData.spellEnum, spellData.classCode, spellData.spellLevel,
                    condArg3);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 1;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellData.spellEnum);
                radMenuEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spellData.spellEnum);
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }

            if (((condArg1 >> 8) & 0xFF) == 0)
            {
                spellData = gem.GetSpell(obj_f.item_spell_idx, 1);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellData.spellEnum, spellData.classCode, spellData.spellLevel,
                    condArg3);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 2;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellData.spellEnum);
                radMenuEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spellData.spellEnum);
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }

            if ((condArg1 & 0xFF) == 0)
            {
                spellData = gem.GetSpell(obj_f.item_spell_idx, 2);
                radMenuEntry = RadialMenuEntry.Create();
                var metaMagic = new MetaMagicData();
                metaMagic.IsQuicken = true;
                radMenuEntry.d20SpellData.SetSpellData(spellData.spellEnum, spellData.classCode, spellData.spellLevel,
                    condArg3, metaMagic);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 3;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellData.spellEnum);
                radMenuEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spellData.spellEnum);
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ffe90)]
        [TemplePlusLocation("condition.cpp:478")]
        public static void WeaponDamageBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v3 = HIDWORD(item);
            int v4 = item;
            evt.subDispNode = (SubDispNode*) evt.GetConditionArg1();
            var dispIo = evt.GetDispIoDamage();
            if (v4 == LODWORD(dispIo.attackPacket.weaponUsed) && v3 == HIDWORD(dispIo.attackPacket.weaponUsed)
                || v4 == LODWORD(dispIo.attackPacket.ammoItem) && v3 == HIDWORD(dispIo.attackPacket.ammoItem))
            {
                var v6 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
                dispIo.damage.AddDamageBonus((int) evt.subDispNode, 12, 147, v6);
            }
        }
/* Orphan comments:
TP Replaced @ condition.cpp:478
*/


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x101018f0)]
        public static void DaggerOfVenomActivate(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg4 = evt.GetConditionArg4();
            if (condArg1 == evt.GetDispIoD20ActionTurnBased().action.data1)
            {
                if ((condArg4) != 0)
                {
                    evt.SetConditionArg4(0);
                }
                else
                {
                    evt.SetConditionArg4(1);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10102200)]
        public static void BootsOfSpeedToHitBonus2(in DispatcherCallbackArgs evt, int data)
        {
            if ((evt.GetConditionArg4()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                dispIo.bonlist.AddBonus(1, 0, 174);
            }
        }


        [DispTypes(DispatcherType.GetCriticalHitRange)]
        [TempleDllLocation(0x100ffd20)]
        public static void WeaponKeenCritHitRange(in DispatcherCallbackArgs evt)
        {
            var bonValue = 1;
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            if (IsWeaponOrAmmo(ref dispIo.attackPacket, item))
            {
                bonValue = item.GetInt32(obj_f.weapon_crit_range);
            }

            // TODO: This seems odd... Is this correct? Shouldn't this only be added when the item is actually used for the attack
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            dispIo.bonlist.AddBonus(bonValue, 0, 246, itemName);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10101be0)]
        public static void ActivateDefendingWeaponRadial(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var radMenuEntry = evt.CreateSliderForArg(1, 0, condArg1);
            radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_FREE;
            radMenuEntry.helpSystemHashkey = "TAG_RADIAL_MENU_ACTIVATE_DEFENDING_WEAPON";
            radMenuEntry.text = GameSystems.D20.Combat.GetCombatMesLine(5078);
            var v4 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v4);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x101010b0)]
        public static void RingOfInvisibilityRadial(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v4 = HIDWORD(v3);
            int v5 = v3;
            if (GameSystems.Item.IsIdentified(v3))
            {
                var radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_STANDARD;
                radMenuEntry.d20ActionData1 = condArg1;
                radMenuEntry.text = GameSystems.MapObject.GetDisplayName(v5, v5);
                radMenuEntry.helpSystemHashkey = "TAG_MAGIC_ITEMS" /*ELFHASH*/;
                var v6 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v6);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x10101ad0)]
        public static void sub_10101AD0(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v6 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg2 = evt.GetConditionArg2();
            if (condArg2 > 0)
            {
                var dispIo = evt.GetDispIoDamage();
                var v4 = dispIo.attackPacket.GetWeaponUsed();
                if (v6 == v4)
                {
                    var v5 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
                    dispIo.damage.AddDamageBonus(-condArg2, 0, 112, v5);
                }
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10100470)]
        public static void MasterworkArmorSkillBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v3 = HIDWORD(v2);
            int v4 = v2;
            var dispIo = evt.GetDispIoObjBonus();
            var v6 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
            dispIo.bonOut.AddBonus(1, 0, 242, v6);
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10102370)]
        public static void ArmorShadowSilentMovesSkillBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v3 = HIDWORD(v2);
            int v4 = v2;
            var dispIo = evt.GetDispIoObjBonus();
            var v6 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
            dispIo.bonOut.AddBonus(5, 34, 112, v6);
        }


        [DispTypes(DispatcherType.MaxHP)]
        [TempleDllLocation(0x10102a00)]
        public static void sub_10102A00(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
            (BonusList*) &evt.dispIO.data.AddBonus(condArg1, 0, 278, v4);
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x10100250)]
        public static void ArmorEnhancementAcBonus(in DispatcherCallbackArgs evt)
        {
            int bonValue;

            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            evt.subDispNode = (SubDispNode*) evt.GetConditionArg1();
            var dispIo = evt.GetDispIoAttackBonus();
            var v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
            var v5 = dispIo.attackPacket.flags;
            if (!(((v5 & D20CAF.TOUCH_ATTACK) != 0)))
            {
                dispIo.bonlist.AddBonus(bonValue, 12, 147, v4);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ffc30)]
        public static void sub_100FFC30(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            var objHnd = dispIo.attackPacket.victim;
            if (IsWeaponOrAmmo(ref dispIo.attackPacket, item))
            {
                if ((objHnd.GetStat(Stat.alignment) & 1) != 0)
                {
                    var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                    dispIo.damage.AddDamageDice(new Dice(2, 6), DamageType.Unspecified, 0x79, itemName);
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x101021c0)]
        public static void sub_101021C0(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg4()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                dispIo.bonlist.AddBonus(1, 8, 174);
            }
        }


        [DispTypes(DispatcherType.SkillLevel, DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x10100650)]
        public static void sub_10100650(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoObjBonus();
            var v7 = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            if (item.type == ObjectType.armor)
            {
                var bonValue = GameSystems.D20.GetArmorSkillCheckPenalty(item);
                if (!evt.objHndCaller.IsNPC() && !GameSystems.Feat.IsProficientWithArmor(evt.objHndCaller, item))
                {
                    dispIo.bonOut.AddBonus(bonValue, 0, 112, v7);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10104da0)]
        [TemplePlusLocation("condition.cpp:464")]
        public static void BucklerToHitPenalty(in DispatcherCallbackArgs evt)
        {
            int v6;

            var condArg3 = evt.GetConditionArg3();
            GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var v3 = dispIo;
            var v4 = dispIo.attackPacket.GetWeaponUsed();
            ulong v5 = HIDWORD(v4);
            GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
            if (((v3.attackPacket.flags & D20CAF.RANGED) == 0))
            {
                if (v5)
                {
                    if (GameSystems.Item.GetWieldType(evt.objHndCaller, HIDWORD(v5)) == 2
                        || (v6 = v3.attackPacket.dispKey, v6 == 6)
                        || v6 == 8)
                    {
                        v3.bonlist.AddBonus(-1, 0, 327);
                    }
                }
            }
        }
/* Orphan comments:
TP Replaced @ condition.cpp:464
*/


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100edfb0)]
        public static void AttackPowerTypeAdd(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v3 = v2;
            var v4 = data;
            int v5 = HIDWORD(v2);
            var dispIo = evt.GetDispIoDamage();
            if (v3 == LODWORD(dispIo.attackPacket.weaponUsed) && v5 == HIDWORD(dispIo.attackPacket.weaponUsed)
                || v3 == LODWORD(dispIo.attackPacket.ammoItem) && v5 == HIDWORD(dispIo.attackPacket.ammoItem))
            {
                AddAttackPowerType /*0x100e0520*/(&dispIo.damage, v4);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x101005b0)]
        public static void SkillLevelArmorPenalty(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoObjBonus();
            if (item.type == ObjectType.armor)
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                var penalty = GameSystems.D20.GetArmorSkillCheckPenalty(item);
                dispIo.bonOut.AddBonus(penalty, 0, 112, itemName);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x10104020)]
        public static void ElementalGemQueryState(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo != null && (int) v2 == dispIo.data1 && HIDWORD(v2) == dispIo.data2)
            {
                dispIo.return_val = condArg1;
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10104530)]
        public static void ThievesToolsMasterworkSkillLevel(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoObjBonus();
            if (condArg3 == 215)
            {
                dispIo.bonOut.AddBonus(2, 21, 315);
            }
        }


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x10103a00)]
        public static void sub_10103A00(in DispatcherCallbackArgs evt)
        {
            int v8;
            int v9;

            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var v3 = dispIo.action.data1 >> 16;
            int v4 = (ushort) dispIo.action.data1;
            if (v3 == condArg3)
            {
                if (v4 == 12)
                {
                    evt.SetConditionArg(7, 1);
                }
                else if (v4 == 13)
                {
                    evt.SetConditionArg(8, 1);
                }
                else
                {
                    var v5 = v4 / 3 + 3;
                    var v6 = v4 % 3;
                    if ((v6) != 0)
                    {
                        var v7 = v6 - 1;
                        if ((v7) != 0)
                        {
                            if (v7 == 1)
                            {
                                v8 = 7;
                                v9 = -256;
                            }
                            else
                            {
                                v8 = (int) evt.subDispNode;
                                v9 = (int) evt.subDispNode;
                            }
                        }
                        else
                        {
                            v8 = 1792;
                            v9 = -65281;
                        }
                    }
                    else
                    {
                        v8 = 458752;
                        v9 = -16711681;
                    }

                    var v10 = evt.GetConditionArg(v5);
                    evt.SetConditionArg(v5, v8 | v9 & v10);
                }
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10102ed0)]
        public static void GoldenSkullRadial(in DispatcherCallbackArgs evt)
        {
            SpellStoreData spellStoreData;

            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v30 = HIDWORD(v2);
            var v24 = 0;
            var v32 = 0;
            var v34 = 0;
            var v33 = 0;
            var condArg1 = evt.GetConditionArg1();
            var condArg8 = evt.GetConditionArg(7);
            var condArg9 = evt.GetConditionArg(8);
            if ((evt.objHndCaller.GetStat(Stat.level_cleric)) != 0 && (evt.objHndCaller.GetStat(Stat.alignment) & 4
                ) != 0 || (evt.objHndCaller.GetStat(Stat.level_paladin)) != 0 &&
                !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                return;
            }

            if ((condArg1 & 0x1000000) != 0)
            {
                v24 = 1;
            }

            if ((condArg1 & 0x10000) != 0)
            {
                v32 = 1;
            }

            if ((condArg1 & 0x100) != 0)
            {
                v34 = 1;
            }

            if ((condArg1 & 1) != 0)
            {
                v33 = 1;
            }

            var radMenuEntry = RadialMenuEntry.Create();
            radMenuEntry.text = GameSystems.MapObject.GetDisplayName((intv2), (intv2));
            var v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
            var parentIdxa = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v3);
            if ((condArg8) == 0)
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 12);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 0);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xC;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v4 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v4 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
            }

            if ((condArg9) == 0)
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 13);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 0);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xD;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v5 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v5 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
            }

            if ((v24) != 0)
            {
                var condArg4 = evt.GetConditionArg4();
                short v25 = condArg4;
                if (!(byte) (condArg4 >> 16))
                {
                    spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 0);
                    radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                        spellStoreData.spellLevel, condArg3, 0);
                    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                    radMenuEntry.d20ActionData1 = condArg3 << 16;
                    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                    var v7 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                    radMenuEntry.helpSystemHashkey = v7 /*ELFHASH*/;
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                    LOWORD(condArg4) = v25;
                }

                if (!BYTE1(condArg4))
                {
                    spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 1);
                    radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                        spellStoreData.spellLevel, condArg3, 0);
                    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 1;
                    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                    var v8 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                    radMenuEntry.helpSystemHashkey = v8 /*ELFHASH*/;
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                    LOBYTE(condArg4) = v25;
                }

                if (!(_BYTE) condArg4)
                {
                    spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 2);
                    radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                        spellStoreData.spellLevel, condArg3, 2);
                    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 2;
                    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                    var v9 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                    radMenuEntry.helpSystemHashkey = v9 /*ELFHASH*/;
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                }

                if ((v32) == 0)
                {
                    LABEL_35:
                    if ((v34) == 0)
                    {
                        goto LABEL_42;
                    }

                    goto LABEL_36;
                }

                LABEL_29:
                var condArg5 = evt.GetConditionArg(4);
                short v26 = condArg5;
                if (!(byte) (condArg5 >> 16))
                {
                    spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 3);
                    radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                        spellStoreData.spellLevel, condArg3, 0);
                    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 3;
                    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                    var v11 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                    radMenuEntry.helpSystemHashkey = v11 /*ELFHASH*/;
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                    LOWORD(condArg5) = v26;
                }

                if (!BYTE1(condArg5))
                {
                    spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 4);
                    radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                        spellStoreData.spellLevel, condArg3, 0);
                    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                    radMenuEntry.d20ActionData1 = (condArg3 << 8) | 4;
                    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                    var v12 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                    radMenuEntry.helpSystemHashkey = v12 /*ELFHASH*/;
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                    LOBYTE(condArg5) = v26;
                }

                if (!(_BYTE) condArg5)
                {
                    spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 5);
                    radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                        spellStoreData.spellLevel, condArg3, 2);
                    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 5;
                    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                    var v13 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                    radMenuEntry.helpSystemHashkey = v13 /*ELFHASH*/;
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                }

                goto LABEL_35;
            }

            if ((v32) != 0)
            {
                goto LABEL_29;
            }

            if ((v34) == 0)
            {
                if ((v33) == 0)
                {
                    return;
                }

                goto LABEL_43;
            }

            LABEL_36:
            var condArg6 = evt.GetConditionArg(5);
            short v27 = condArg6;
            if (!(byte) (condArg6 >> 16))
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 6);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 0);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 6;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v15 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v15 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                LOWORD(condArg6) = v27;
            }

            if (!BYTE1(condArg6))
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 7);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 0);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 7;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v16 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v16 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                LOBYTE(condArg6) = v27;
            }

            if (!(_BYTE) condArg6)
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 8);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 2);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 8;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v17 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v17 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
            }

            LABEL_42:
            if ((v33) == 0)
            {
                return;
            }

            LABEL_43:
            var condArg7 = evt.GetConditionArg(6);
            short v28 = condArg7;
            if (!(byte) (condArg7 >> 16))
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 9);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 0);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 9;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v19 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v19 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                LOWORD(condArg7) = v28;
            }

            if (!BYTE1(condArg7))
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 10);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 0);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xA;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v20 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v20 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
                LOBYTE(condArg7) = v28;
            }

            if (!(_BYTE) condArg7)
            {
                spellStoreData = (intv2).GetSpell(obj_f.item_spell_idx, 11);
                radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                    spellStoreData.spellLevel, condArg3, 2);
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xB;
                radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
                var v21 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                radMenuEntry.helpSystemHashkey = v21 /*ELFHASH*/;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ff7f0)]
        public static void BurstWeaponCritDice(in DispatcherCallbackArgs evt, DamageType damageType)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            if (IsWeaponOrAmmo(ref dispIo.attackPacket, item))
            {
                Dice extraDamageDice;
                if ((dispIo.attackPacket.flags & D20CAF.CRITICAL) != 0)
                {
                    var extraCritDispIo = DispIoAttackBonus.Default;
                    extraCritDispIo.attackPacket.victim = dispIo.attackPacket.victim;
                    extraCritDispIo.attackPacket.attacker = evt.objHndCaller;
                    extraCritDispIo.attackPacket.dispKey = dispIo.attackPacket.dispKey;
                    extraCritDispIo.attackPacket.flags = dispIo.attackPacket.flags;
                    extraCritDispIo.attackPacket.weaponUsed = dispIo.attackPacket.weaponUsed;
                    extraCritDispIo.attackPacket.ammoItem = dispIo.attackPacket.ammoItem;
                    var count = evt.objHndCaller.DispatchGetCritExtraDice(ref extraCritDispIo);
                    extraDamageDice = new Dice(count, 10);
                }
                else
                {
                    extraDamageDice = Dice.D6;
                }

                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.damage.AddDamageDice(extraDamageDice, damageType, 121, itemName);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10101d80)]
        public static void KeoghtomsOintmentRadial(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            GameSystems.Item.GetItemSpellCharges(item);
            if (GameSystems.Item.IsIdentified(item))
            {
                var radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.text = GameSystems.MapObject.GetDisplayName(item, item);
                var itemNode = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                var parentIdx =
                    GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, itemNode);
                for (var i = 0; i < 3; i++)
                {
                    SpellStoreData spell = item.GetSpell(obj_f.item_spell_idx, i);
                    radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20SpellData.SetSpellData(spell.spellEnum, spell.classCode,
                        spell.spellLevel, condArg3);
                    radMenuEntry.d20ActionType = D20ActionType.USE_ITEM;
                    radMenuEntry.d20ActionData1 = 0;
                    radMenuEntry.text = GameSystems.Spell.GetSpellName(spell.spellEnum);
                    radMenuEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spell.spellEnum);
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
                }
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x10100720)]
        public static void ArmorBonusAcBonusCapValue(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            // TODO: This is somewhat misleading because it'll take encumbrance max dex into account, while that should be it's own cap entry!
            var maxDexBonus = item.DispatchGetArmorMaxDexBonus();
            dispIo.bonlist.AddCap(3, maxDexBonus, 112, itemName);
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x10104b30)]
        public static void BracersOfArcheryDamageBonus(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            var itemHandle = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            var weapused = dispIo.attackPacket.GetWeaponUsed();
            if (weapused != null)
            {
                var weapType = weapused.GetWeaponType();
                if (GameSystems.Weapon.IsBow(weapType)
                    && GameSystems.Feat.IsProficientWithWeaponType(evt.objHndCaller, weapType))
                {
                    var itemName = GameSystems.MapObject.GetDisplayName(itemHandle, evt.objHndCaller);
                    var damageBonus = Dice.Constant(condArg2);
                    dispIo.damage.AddDamageDice(damageBonus, DamageType.Piercing, 0x79, itemName);
                }
            }
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x10104d30)]
        public static void UseableItemXTimesPerDayPerform(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var condArg4 = evt.GetConditionArg4();
            evt.GetConditionArg2();
            if (evt.GetDispIoD20ActionTurnBased().action.data1 == condArg3)
            {
                evt.SetConditionArg4(condArg4 + 1);
            }
        }


        [DispTypes(DispatcherType.SkillLevel, DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x10100500)]
        public static void sub_10100500(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (evt.objHndCaller.type != 14 && !GameSystems.Feat.IsProficientWithArmor(evt.objHndCaller, v2))
            {
                var dispIo = evt.GetDispIoObjBonus();
                var v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
                dispIo.bonOut.AddBonus(1, 0, 242, v4);
            }
        }


        [DispTypes(DispatcherType.WeaponGlowType)]
        [TempleDllLocation(0x100edf40)]
        public static void WeaponGlowCb(in DispatcherCallbackArgs evt, int data)
        {
            int condArg3;
            int argsa; /*INLINED:sdn=evt.subDispNode*/
            evt.subDispNode = (SubDispNode*) data;
            var dispIo = evt.GetDispIoD20Query();
            if ((evt.objHndCaller == null) || (condArg3 = evt.GetConditionArg3(),
                    GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3) == *(_QWORD*) &dispIo.data1))
            {
                if (dispIo.return_val < argsa)
                {
                    dispIo.return_val = argsa;
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x101003f0)]
        public static void ShieldEnhancementAcBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var displayName = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
            int v5 = evt.dispIO[4].ioType;
            if ((v5 & 0x100) == 0)
            {
                (BonusList*) &evt.dispIO[7].AddBonus(condArg1, 33, 147, displayName);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x101044f0)]
        public static void sub_101044F0(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo != null)
            {
                if (condArg3 == 215)
                {
                    dispIo.return_val = 1;
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ffdf0)]
        public static void WeaponEnhancementToHitBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            int v5 = dispIo.attackPacket.weaponUsed;
            int v6 = HIDWORD(dispIo.attackPacket.weaponUsed);
            if (v3 == v5 || v3 == dispIo.attackPacket.ammoItem && GameSystems.Item.AmmoMatchesWeapon(v5, v3))
            {
                var v7 = GameSystems.MapObject.GetDisplayName(v3, evt.objHndCaller);
                dispIo.bonlist.AddBonus(condArg1, 12, 147, v7);
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x101001d0)]
        public static void ArmorBonusAcBonus(in DispatcherCallbackArgs evt)
        {
            int bonValue;

            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            evt.subDispNode = (SubDispNode*) evt.GetConditionArg1();
            var dispIo = evt.GetDispIoAttackBonus();
            var v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
            var v5 = dispIo.attackPacket.flags;
            if (!(((v5 & D20CAF.TOUCH_ATTACK) != 0)))
            {
                dispIo.bonlist.AddBonus(bonValue, 28, 124, v4);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x10100fc0)]
        public static void RingOfInvisSequence(in DispatcherCallbackArgs evt)
        {
            int v6;
            int v7;
            int v8;
            D20Action v9;

            var condArg1 = evt.GetConditionArg1();
            var seq = (ActionSequence) evt.GetDispIoD20Signal().data1;
            var N = seq.d20ActArrayNum;
            var i = 0;
            if (N > 0)
            {
                var d20a = (D20Action) seq;
                while (2)
                {
                    switch (d20a.d20ActType)
                    {
                        default:
                            ++i;
                            ++d20a;
                            if (i < N)
                            {
                                continue;
                            }

                            break;
                        case 0x40:
                            v6 = i;
                            v7 = seq.d20ActArray[v6].d20ATarget;
                            v8 = seq.d20ActArray[v6].d20APerformer;
                            v9 = &seq.d20ActArray[v6];
                            if (v7 == v8 && HIDWORD(v9.d20ATarget) == HIDWORD(v9.d20APerformer))
                            {
                                goto LABEL_8;
                            }

                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 0xA:
                        case 0xC:
                        case 0xD:
                        case 0xE:
                        case 0xF:
                        case 0x10:
                        case 0x11:
                        case 0x14:
                        case 0x15:
                        case 0x18:
                        case 0x1A:
                        case 0x1D:
                        case 0x1E:
                        case 0x23:
                        case 0x27:
                        case 0x2A:
                        case 0x2B:
                            LABEL_8:
                            GameSystems.D20.D20SendSignal(evt.objHndCaller,
                                D20DispatcherKey.SIG_Magical_Item_Deactivate, condArg1, (ulong) condArg1 >> 32);
                            evt.SetConditionArg2(0);
                            break;
                    }

                    break;
                }
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10102650)]
        public static void ActivateDeviceSpellRadial(in DispatcherCallbackArgs evt)
        {
            evt.subDispNode = (SubDispNode*) evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            var v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (evt.subDispNode)
            {
                var condArg1 = evt.GetConditionArg1();
                var spellEnumOrg = v3.GetSpell(obj_f.item_spell_idx, condArg1);
                var radMenuEntry = RadialMenuEntry.Create();
                radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
                radMenuEntry.d20ActionData1 = condArg3;
                radMenuEntry.d20SpellData.SetSpellData(spellEnumOrg.spellEnum, spellEnumOrg.classCode,
                    spellEnumOrg.spellLevel, condArg3, 0);
                radMenuEntry.text = GameSystems.MapObject.GetDisplayName(v3, v3);
                var v5 = GameSystems.Spell.GetSpellHelpTopic(spellEnumOrg.spellEnum);
                radMenuEntry.helpSystemHashkey = v5 /*ELFHASH*/;
                var v6 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v6);
            }
        }


        [DispTypes(DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x10100ec0)]
        public static void RingOfInvisibilityStatusD20StatusInit(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1((int) evt.subDispNode.condNode);
            evt.SetConditionArg2(0);
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x10102550)]
        public static void WeaponDisruptionOnDamage(in DispatcherCallbackArgs evt)
        {
            int v6;

            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v3 = v2;
            LOBYTE(v2) = evt.dispIO[4].ioType;
            int v8 = HIDWORD(v2);
            if (!(v2 & 4)
                && (AttackPacket*) &evt.dispIO[1].GetWeaponUsed() == v3
                && GameSystems.Critter.IsCategory(*(_QWORD*) &evt.dispIO[2], MonsterCategory.undead)
                && !GameSystems.D20.Combat.SavingThrow(*(_QWORD*) &evt.dispIO[2], evt.objHndCaller, 14,
                    SavingThrowType.Will, 0))
            {
                GameSystems.D20.Combat.Kill(*(_QWORD*) &evt.dispIO[2], evt.objHndCaller);
                var v4 = (GameObjectBody) evt.dispIO[2];
                GameSystems.ParticleSys.CreateAtObj("sp-Destroy Undead", v4);
                var meslineKey = 7000;
                var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
                GameSystems.RollHistory.CreateFromFreeText((string) meslineValue);
                GameSystems.RollHistory.CreateFromFreeText("\n");
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100efc80)]
        public static void sub_100EFC80(in DispatcherCallbackArgs evt, int data)
        {
            evt.SetConditionArg(data, 0xDEADBEEF);
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10104290)]
        public static void FragarachToHitBonus(in DispatcherCallbackArgs evt)
        {
            if ((evt.objHndCaller.GetBaseStat(Stat.alignment) & 8) == 0)
            {
                var condArg3 = evt.GetConditionArg3();
                var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                int v3 = HIDWORD(v2);
                int v4 = v2;
                var dispIo = evt.GetDispIoAttackBonus();
                var v6 = dispIo.attackPacket.GetWeaponUsed();
                if (v4 == v6)
                {
                    dispIo.attackPacket.flags |= D20CAF.ALWAYS_HIT;
                    var v7 = GameSystems.MapObject.GetDisplayName(v6, evt.objHndCaller);
                    var v8 = &dispIo.bonlist;
                    v8.AddBonus(4, 12, 112, v7);
                    v8.zeroBonusSetMeslineNum(308);
                }
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x10102280)]
        public static void BootsOfSpeedGetMoveSpeed(in DispatcherCallbackArgs evt)
        {
            var v1 = 0;
            if ((evt.GetConditionArg4()) != 0)
            {
                BonusList* v2 = evt.GetDispIoMoveSpeed().bonlist;
                int v3 = v2.bonCount;
                var v4 = 0;
                if (v3 > 0)
                {
                    int* v5 = &v2.bonusEntries[0].bonType;
                    while (*v5 != 1)
                    {
                        ++v4;
                        v5 += 4;
                        if (v4 >= v3)
                        {
                            goto LABEL_8;
                        }
                    }

                    v1 = v2.bonusEntries[v4].bonValue;
                }

                LABEL_8:
                v2.SetOverallCap(1, 2 * v1, 12, 0xAE);
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x101015d0)]
        public static void ItemElementalResistanceDR(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var itemHandle = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            evt.subDispNode = (SubDispNode*) evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            evt.dispIO = (DispIOGeneral*) evt.GetDispIoDamage();
            var itemName = GameSystems.MapObject.GetDisplayName(itemHandle, evt.objHndCaller);
            (DamagePacket) & evt.dispIO[7].AddDR(condArg2, (D20DT) evt.subDispNode, 0x79, itemName);
        }


        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x10100130)]
        public static void ArmorBonusMovementSthg_Callback(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var v3 = GameSystems.D20.D20QueryItem(v2, D20DispatcherKey.QUE_Armor_Get_Max_Speed);
            if (v3 != 100 && evt.objHndCaller.GetStat(Stat.race) != 1)
            {
                var dispIo = evt.GetDispIoMoveSpeed();
                if (dispIo.bonlist.bonusEntries[0].bonValue >= 30)
                {
                    if (v3 < 30)
                    {
                        dispIo.factor = dispIo.factor * 0.66F;
                    }
                }
                else if (v3 < 20)
                {
                    dispIo.factor = dispIo.factor * 0.75F;
                }
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10101ee0)]
        public static void BootsOfSpeedRadial(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Item.IsIdentified(v2))
            {
                if (condArg1 >= 1)
                {
                    var radMenuEntry = RadialMenuEntry.Create();
                    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_FREE;
                    radMenuEntry.d20ActionData1 = condArg3;
                    radMenuEntry.text = GameSystems.MapObject.GetDisplayName(v2, v2);
                    radMenuEntry.helpSystemHashkey = "TAG_MAGIC_ITEMS" /*ELFHASH*/;
                    var v4 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v4);
                }
            }
        }


        [DispTypes(DispatcherType.GetCriticalHitExtraDice)]
        [TempleDllLocation(0x10104fa0)]
        public static void RodOfSmiting_CritHit(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var condArg1 = evt.GetConditionArg1();
            var v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var v5 = dispIo.attackPacket.GetWeaponUsed();
            if (condArg1 > 0 && v5 == v3)
            {
                int v6 = GameSystems.MapObject.GetDisplayName(v3);
                CHAR v7 = String.Format("{0}", v6);
                dispIo.bonlist.AddBonus(2, 0, 112, &v7);
                evt.SetConditionArg1(condArg1 - 1);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x101023f0)]
        public static void BaneWeaponToHitBonus(in DispatcherCallbackArgs evt)
        {
            int v8;

            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var v4 = dispIo;
            var v5 = dispIo.attackPacket.GetWeaponUsed();
            var v6 = GameSystems.MapObject.GetDisplayName(v5, evt.objHndCaller);
            if (FavoredEnemies.IsOfType(v4.attackPacket.victim, condArg1))
            {
                v4.bonlist.AddBonus(2, 0, 263, v6);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ff730)]
        public static void GenericElementalDamageBonus(in DispatcherCallbackArgs evt, DamageType damageType)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            if (IsWeaponOrAmmo(ref dispIo.attackPacket, item))
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.damage.AddDamageDice(Dice.D6, damageType, 0x79, itemName);
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x10101760)]
        public static void sub_10101760(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            evt.SetConditionArg1(condArg1 - condArg2);
            evt.SetConditionArg2(0);
            var dispIo = evt.GetDispIoDamage();
            var v6 = dispIo.attackPacket.GetWeaponUsed();
            if (v2 == v6)
            {
                var v7 = GameSystems.MapObject.GetDisplayName(v6, evt.objHndCaller);
                dispIo.damage.AddDamageBonus(3 * condArg2, 0, 112, v7);
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x10101cc0)]
        public static void ElementalResistancePerRoundTakingDamage(in DispatcherCallbackArgs args)
        {
            int v8;
            int v9;
            var condArg1 = args.GetConditionArg1();
            D20DT v3 = condArg1;
            D20DT damType = condArg1;
            args.subDispNode = (SubDispNode*) args.GetConditionArg3();
            var condArg4 = args.GetConditionArg4();
            var dispIo = args.GetDispIoDamage();
            DamagePacket damPkt = &dispIo.damage;
            int v7 = GetDamageTypeOverallDamage /*0x100e1210*/(&dispIo.damage, v3);
            if (v7 > 0 && (condArg4) != 0 && condArg4 >= 0)
            {
                if (condArg4 < v7)
                {
                    v8 = condArg4;
                    v9 = 0;
                }
                else
                {
                    v8 = v7;
                    v9 = condArg4 - v7;
                }

                Logger.Info("({0}) damage reduced", v8);
                args.SetConditionArg4(v9);
                var v10 = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, (int) args.subDispNode);
                var v11 = GameSystems.MapObject.GetDisplayName(v10, args.objHndCaller);
                damPkt.AddDR(v8, damType, 0x7C, v11);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10100840)]
        [TemplePlusLocation("condition.cpp:455")]
        public static void UseableItemRadialEntry(in DispatcherCallbackArgs evt)
        {
            int v8;
            int v12;
            int v16;

            var condArg3 = evt.GetConditionArg3();
            var invIdx = condArg3;
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            int v3 = HIDWORD(v2);
            int v4 = v2;
            int v14 = v2.type;
            var v15 = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.use_magic_device);
            if (v14 == ObjectType.food || GameSystems.Item.IsIdentified(v4))
            {
                var v5 = GameSystems.Item.GetItemSpellCharges(v4);
                if (v5 > 0 || v5 == -1)
                {
                    CHAR v18 = v4.GetItemFlags();
                    var condArg1 = evt.GetConditionArg1();
                    SpellStoreData spellStoreData = v4.GetSpell(obj_f.item_spell_idx, condArg1);
                    var v7 = v14;
                    if (v14 == ObjectType.scroll || v18 & ItemFlag.NEEDS_SPELL &&
                        (v14 == ObjectType.generic || v14 == ObjectType.weapon))
                    {
                        if (!GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spellStoreData.spellEnum) &&
                            (v15) == 0)
                        {
                            return;
                        }

                        v7 = v14;
                    }

                    if (v7 == ObjectType.scroll &&
                        !GameSystems.Spell.CheckAbilityScoreReqForSpell(evt.objHndCaller, spellStoreData.spellEnum,
                            -1) && (v15) == 0)
                    {
                        return;
                    }

                    var radMenuEntry = RadialMenuEntry.Create();
                    if (v14 != ObjectType.food || (v8 = GameSystems.Item.IsMagical(v4),
                            radMenuEntry.d20ActionType = D20ActionType.USE_POTION, (v8) == 0))
                    {
                        radMenuEntry.d20ActionType = D20ActionType.USE_ITEM;
                    }

                    radMenuEntry.d20ActionData1 = invIdx;
                    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                        spellStoreData.spellLevel, invIdx, 0);
                    radMenuEntry.text = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
                    if (v14 == 9)
                    {
                        v16 = 2;
                    }
                    else if (v14 == 8)
                    {
                        v16 = 1;
                    }
                    else
                    {
                        v16 = evt.GetConditionArg2() != 3 ? 0 : 3;
                    }

                    var v9 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
                    radMenuEntry.helpSystemHashkey = v9 /*ELFHASH*/;
                    if (v16 != 1)
                    {
                        if (v16 == 2)
                        {
                            var v11 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Scrolls);
                            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v11);
                        }
                        else
                        {
                            if (v16 != 3)
                            {
                                v12 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
                                LABEL_30:
                                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v12);
                                goto LABEL_31;
                            }

                            var v10 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Wands);
                            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v10);
                        }

                        LABEL_31:
                        if (v14 == 9
                            && evt.objHndCaller.GetStat(Stat.level_wizard) >= 1
                            && GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spellStoreData.spellEnum)
                            && SLOBYTE(spellStoreData.classCode) < 0
                            && IsArcaneClass /*0x10076170*/(spellStoreData.classCode & 0x7F)
                            && !GameSystems.Spell.SpellKnownQueryGetData(evt.objHndCaller, spellStoreData.spellEnum, 0,
                                0, 0))
                        {
                            radMenuEntry = RadialMenuEntry.Create();
                            radMenuEntry.text = GameSystems.MapObject.GetDisplayName(v4, v4);
                            radMenuEntry.d20ActionType = D20ActionType.COPY_SCROLL;
                            radMenuEntry.d20ActionData1 = v4.GetItemInventoryLocation();
                            radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                                spellStoreData.spellLevel, invIdx, 0);
                            var v13 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.CopyScroll);
                            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v13);
                        }

                        return;
                    }

                    v12 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Potions);
                    goto LABEL_30;
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ff6f0)]
        public static void WeaponHasEnhancementBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            *(_QWORD*) &dispIo.data1 = evt.GetConditionArg1();
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x101014c0)]
        public static void BurstDamageTargetParticles(in DispatcherCallbackArgs evt, string data)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if ((AttackPacket*) &evt.dispIO[1].GetWeaponUsed() == v2 && data != null)
            {
                int v3 = (string;
                data);
                if (evt.dispIO[4].ioType & D20CAF.CRITICAL)
                {
                    GameSystems.ParticleSys.CreateAtObj(v3, *(_QWORD*) &evt.dispIO[2]);
                }
            }
        }


        [DispTypes(DispatcherType.GetBonusAttacks)]
        [TempleDllLocation(0x10102190)]
        [TemplePlusLocation("spell_condition.cpp:258")]
        public static void BootsOfSpeedBonusAttack(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg4()) != 0)
            {
                var dispIo = evt.GetDispIoD20ActionTurnBased();
                ++dispIo.returnVal;
            }
        }

/* Orphan comments:
TP Replaced @ spell_condition.cpp:258
*/
    }
}