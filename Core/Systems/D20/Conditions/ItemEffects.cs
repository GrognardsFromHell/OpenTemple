using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .AddQueryHandler(D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus, WeaponHasEnhancementBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Item_Remove_Enhancement, ItemRemoveEnhancement)
            .Build();


        [TempleDllLocation(0x102f0898)]
        public static readonly ConditionSpec WeaponDefendingBonus = ConditionSpec.Create("Weapon Defending Bonus", 4)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.ToHitBonus2, sub_10101A40)
            .AddHandler(DispatcherType.DealingDamage, sub_10101AD0)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .AddHandler(DispatcherType.GetAC, WeaponDefendingArmorBonus)
            .AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
            .AddHandler(DispatcherType.RadialMenuEntry, ActivateDefendingWeaponRadial)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 1)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .Build();


        [TempleDllLocation(0x102f0998)]
        public static readonly ConditionSpec WeaponFlaming = ConditionSpec.Create("Weapon Flaming", 3)
            .AddHandler(DispatcherType.DealingDamage, GenericElementalDamageBonus, DamageType.Fire)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Fire")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 6)
            .Build();


        [TempleDllLocation(0x102f0a18)]
        public static readonly ConditionSpec WeaponFrost = ConditionSpec.Create("Weapon Frost", 3)
            .AddHandler(DispatcherType.DealingDamage, GenericElementalDamageBonus, DamageType.Cold)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4100)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 5)
            .Build();


        [TempleDllLocation(0x102f0a98)]
        public static readonly ConditionSpec WeaponShock = ConditionSpec.Create("Weapon Shock", 3)
            .AddHandler(DispatcherType.DealingDamage, GenericElementalDamageBonus, DamageType.Electricity)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Shock")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 9)
            .Build();


        [TempleDllLocation(0x102f0c50)]
        public static readonly ConditionSpec WeaponFlamingBurst = ConditionSpec.Create("Weapon Flaming Burst", 3)
            .AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, DamageType.Fire)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Fire")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-FIRE-burst")
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 6)
            .Build();


        [TempleDllLocation(0x102f0ce8)]
        public static readonly ConditionSpec WeaponIcyBurst = ConditionSpec.Create("Weapon Icy Burst", 3)
            .AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, DamageType.Cold)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4096)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-COLD-burst")
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 5)
            .Build();


        [TempleDllLocation(0x102f0d80)]
        public static readonly ConditionSpec WeaponShockingBurst = ConditionSpec.Create("Weapon Shocking Burst", 3)
            .AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, DamageType.Electricity)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Shock")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-SHOCK-burst")
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 9)
            .Build();


        [TempleDllLocation(0x102f0e18)]
        public static readonly ConditionSpec WeaponHoly = ConditionSpec.Create("Weapon Holy", 3)
            .AddHandler(DispatcherType.DealingDamage, sub_100FFA50)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 12)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-HOLY-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 8)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 8)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 8)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 8)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 8)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 8)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 7)
            .Build();


        [TempleDllLocation(0x102f0f10)]
        public static readonly ConditionSpec WeaponUnholy = ConditionSpec.Create("Weapon Unholy", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponUnholyDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 20)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-UNHOLY-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 4)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 4)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 4)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 4)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 4)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 4)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 10)
            .Build();


        [TempleDllLocation(0x102f1008)]
        public static readonly ConditionSpec WeaponLawful = ConditionSpec.Create("Weapon Lawful", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponLawfulDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 68)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-LAW-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 2)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 2)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 2)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 2)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 2)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 2)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 8)
            .Build();


        [TempleDllLocation(0x102f1100)]
        public static readonly ConditionSpec WeaponChaotic = ConditionSpec.Create("Weapon Chaotic", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponChaoticDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 36)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-CHAOTIC-medium")
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd,
                273, 1)
            .AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 1)
            .AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 1)
            .AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 1)
            .AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 1)
            .AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 1)
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 4)
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
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .AddHandler(DispatcherType.ToHitBonus2, BaneWeaponToHitBonus)
            .AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Bane")
            .AddHandler(DispatcherType.ProjectileDestroyed, ProjectileDestroyedEndParticles, 1)
            .AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-BANE-medium")
            .AddHandler(DispatcherType.WeaponGlowType, GetWeaponGlowType, 3)
            .Build();


        [TempleDllLocation(0x102f11f8)]
        public static readonly ConditionSpec ToHitBonus = ConditionSpec.Create("To Hit Bonus", 3)
            .AddHandler(DispatcherType.ToHitBonus2, WeaponEnhancementToHitBonus)
            .Build();


        [TempleDllLocation(0x102f1228)]
        public static readonly ConditionSpec DamageBonus = ConditionSpec.Create("Damage Bonus", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponDamageBonus)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
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
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, ItemParticlesPack, 1)
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
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .Build();


        [TempleDllLocation(0x102f1ca0)]
        public static readonly ConditionSpec DaggerofVenom = ConditionSpec.Create("Dagger of Venom", 4)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit, DaggerOfVenomStatusInit)
            .AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
            .AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.CondNodeSetArgToZero)
            .AddHandler(DispatcherType.RadialMenuEntry, DaggerOfVenomRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 116, DaggerOfVenomActivate)
            .AddHandler(DispatcherType.DealingDamage2, DaggerOfVenomPoisonOnDamage)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
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
            .AddHandler(DispatcherType.GetAC, BootsOfSpeedAcBonus)
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
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
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
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
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
            .AddHandler(DispatcherType.MaxHP, FamiliarHpBonusGetMaxHp)
            .AddSkillLevelHandler(SkillId.listen, AddFamiliarSkillBonus, 2)
            .AddSkillLevelHandler(SkillId.spot, AddFamiliarSkillBonus, 2)
            .Build();


        [TempleDllLocation(0x102f1fc8)]
        public static readonly ConditionSpec ArmorShadow = ConditionSpec.Create("Armor Shadow", 3)
            .AddSkillLevelHandler(SkillId.hide, ArmorShadowSilentMovesSkillBonus)
            .Build();


        [TempleDllLocation(0x102f2340)]
        public static readonly ConditionSpec GoldenSkull = ConditionSpec.Create("Golden Skull", 9)
            .AddSignalHandler(D20DispatcherKey.SIG_Golden_Skull_Combine, GoldenSkullCombine)
            .AddHandler(DispatcherType.RadialMenuEntry, GoldenSkullRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 122 /* ACTIVATE_DEVICE_SPELL */,
                GoldenSkullActivateDeviceSpellPerform)
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
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 40)
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
            .AddQueryHandler(D20DispatcherKey.QUE_Has_Thieves_Tools, HasThievesToolsQuery)
            .Build();


        [TempleDllLocation(0x102f2588)]
        public static readonly ConditionSpec ThievesToolsMasterwork = ConditionSpec
            .Create("Thieves Tools Masterwork", 3)
            .AddQueryHandler(D20DispatcherKey.QUE_Has_Thieves_Tools, HasThievesToolsQuery)
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
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 4)
            .Build();


        [TempleDllLocation(0x102f2b6c)]
        public static readonly ConditionSpec WeaponSilver = ConditionSpec.Create("Weapon Silver", 3)
            .AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, (D20AttackPower) 6)
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
                var text = GameSystems.MapObject.GetDisplayName(item);
                var radMenuEntry = RadialMenuEntry.CreateAction(text, D20ActionType.ACTIVATE_DEVICE_STANDARD,
                    condArg1, "TAG_DAGGER");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Items);
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
        public static void ItemParticleSystemShow(in DispatcherCallbackArgs evt, int partSysArgIdx,
            int partSysNameHashArgIdx)
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
            var weapon = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponPrimary);
            if (weapon != null)
            {
                string helpText;
                if (weapon.GetWeaponType() == WeaponType.heavy_crossbow)
                {
                    helpText = "TAG_WEAPONS_CROSSBOW_HEAVY";
                }
                else
                {
                    helpText = "TAG_WEAPONS_CROSSBOW_LIGHT";
                }

                var radMenuEntry = RadialMenuEntry.CreateAction(5009, D20ActionType.RELOAD, 0, helpText);
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Offense);
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
            if (roundsNew >= 0)
            {
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
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var invIdx = condArg3;
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var charges = GameSystems.Item.GetItemSpellCharges(item);
            if (GameSystems.Item.IsIdentified(item) && charges > 0)
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item);
                var parentEntry = RadialMenuEntry.CreateParent(itemName);
                var parentIdx =
                    GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                        RadialMenuStandardNode.Items);
                for (var index = 0; index < condArg1; ++index)
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
                            GameSystems.Spell.CreateSpellPacketForStaff(spData.spellEnum, evt.objHndCaller,
                                out var spellPacketBody);

                            var spellData = new D20SpellData(spData.spellEnum, spellPacketBody.spellClass,
                                spellPacketBody.casterLevel, invIdx);
                            var spellEntry = RadialMenuEntry.CreateSpellAction(spellData, D20ActionType.USE_ITEM);
                            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref spellEntry, parentIdx);
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

        private const int WhiteGemMask = 0x01_00_00_00; // From air elemental gem
        private const int OrangeGemMask = 0x00_01_00_00; // From earth elemental gem
        private const int RedGemMask = 0x00_00_01_00; // From fire elemental gem
        private const int BlueGemMask = 0x00_00_00_01; // From water elemental gem

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x10102b70)]
        public static void GoldenSkullCombine(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var condArg3 = evt.GetConditionArg3();
            var goldenSkullItem = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var airGemProto = GameSystems.Proto.GetProtoById(WellKnownProtos.AirElementalPowerGem);
            var earthGemProto = GameSystems.Proto.GetProtoById(WellKnownProtos.EarthElementalPowerGem);
            var fireGemProto = GameSystems.Proto.GetProtoById(WellKnownProtos.FireElementalPowerGem);
            var waterGemProto = GameSystems.Proto.GetProtoById(WellKnownProtos.WaterElementalPowerGem);
            var combiningWith = (GameObjectBody) dispIo.obj;
            var combiningWithName = GameSystems.MapObject.GetDisplayName(combiningWith);

            int argIndex;
            int bitmask;
            if (combiningWithName == GameSystems.MapObject.GetDisplayName(airGemProto))
            {
                argIndex = 3;
                bitmask = WhiteGemMask;
            }
            else if (combiningWithName == GameSystems.MapObject.GetDisplayName(earthGemProto))
            {
                argIndex = 4;
                bitmask = OrangeGemMask;
            }
            else if (combiningWithName == GameSystems.MapObject.GetDisplayName(fireGemProto))
            {
                argIndex = 5;
                bitmask = RedGemMask;
            }
            else if (combiningWithName == GameSystems.MapObject.GetDisplayName(waterGemProto))
            {
                argIndex = 6;
                bitmask = BlueGemMask;
            }
            else
            {
                return;
            }

            if ((condArg1 & bitmask) == 0)
            {
                var gemBitField = condArg1 | bitmask;
                evt.SetConditionArg1(gemBitField);
                var v10 = GameSystems.D20.D20QueryWithObject(evt.objHndCaller, D20DispatcherKey.QUE_Elemental_Gem_State,
                    combiningWith);
                evt.SetConditionArg(argIndex, v10);

                var artId = GetGoldenSkullArtId(gemBitField);
                goldenSkullItem.SetInt32(obj_f.item_inv_aid, artId);
            }
        }

        private static int GetGoldenSkullArtId(int activeGems)
        {
            switch (activeGems)
            {
                case BlueGemMask:
                    return 332;
                case OrangeGemMask | BlueGemMask | RedGemMask:
                    return 333;
                case OrangeGemMask | BlueGemMask:
                    return 334;
                case OrangeGemMask | RedGemMask:
                    return 335;
                case WhiteGemMask | OrangeGemMask | BlueGemMask | RedGemMask:
                    return 336;
                case WhiteGemMask | OrangeGemMask | RedGemMask:
                    return 337;
                case WhiteGemMask | OrangeGemMask:
                    return 338;
                case OrangeGemMask:
                    return 339;
                case WhiteGemMask | BlueGemMask | RedGemMask:
                    return 340;
                case BlueGemMask | RedGemMask:
                    return 341;
                case WhiteGemMask | RedGemMask:
                    return 342;
                case RedGemMask:
                    return 343;
                case WhiteGemMask | OrangeGemMask | BlueGemMask:
                    return 344;
                case WhiteGemMask | BlueGemMask:
                    return 345;
                case WhiteGemMask:
                    return 346;
                default:
                    return 347;
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
            SpellStoreData spellData;

            var condArg3 = evt.GetConditionArg3();
            var condArg2 = evt.GetConditionArg2();
            var gem = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (!GameSystems.Item.IsIdentified(gem))
            {
                // Gem cannot be used when unidentified
                return;
            }

            var parentEntry = RadialMenuEntry.CreateParent(GameSystems.MapObject.GetDisplayName(gem));
            var parentIdx = GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                RadialMenuStandardNode.Items);
            var condArg1 = evt.GetConditionArg1();
            var gemTarget = ElementalGemTargets[condArg2];

            if (((condArg1 >> 24) & 0xFF) == 0 && GameSystems.Map.GetCurrentMapId() != gemTarget.MapId)
            {
                var radMenuEntry = RadialMenuEntry.CreateAction(gemTarget.RadialMenuMesLine,
                    D20ActionType.ACTIVATE_DEVICE_FREE, condArg2, "TAG_GOLDEN_SKULL");
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }

            if (((condArg1 >> 16) & 0xFF) == 0)
            {
                spellData = gem.GetSpell(obj_f.item_spell_idx, 0);
                var d20SpellData = new D20SpellData(spellData.spellEnum, spellData.classCode, spellData.spellLevel,
                    condArg3);
                var radMenuEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.ACTIVATE_DEVICE_SPELL);
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 1;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }

            if (((condArg1 >> 8) & 0xFF) == 0)
            {
                spellData = gem.GetSpell(obj_f.item_spell_idx, 1);
                var d20SpellData = new D20SpellData(spellData.spellEnum, spellData.classCode, spellData.spellLevel,
                    condArg3);
                var radMenuEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.ACTIVATE_DEVICE_SPELL);
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 2;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }

            if ((condArg1 & 0xFF) == 0)
            {
                spellData = gem.GetSpell(obj_f.item_spell_idx, 2);
                var metaMagic = new MetaMagicData();
                metaMagic.IsQuicken = true;
                var d20SpellData = new D20SpellData(spellData.spellEnum, spellData.classCode, spellData.spellLevel,
                    condArg3, metaMagic);
                var radMenuEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.ACTIVATE_DEVICE_SPELL);
                radMenuEntry.d20ActionData1 = (condArg3 << 16) | 3;
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ffe90)]
        [TemplePlusLocation("condition.cpp:478")]
        public static void WeaponDamageBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            var attacker = dispIo.attackPacket.attacker;
            if (attacker == null)
                return;
            var invIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(attacker, invIdx);

            var weapUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weapUsed == null)
                return;

            if (item == null)
                return;

            if (item == weapUsed
                || item == dispIo.attackPacket.ammoItem && GameSystems.Item.AmmoMatchesWeapon(weapUsed, item))
            {
                var damBonus = evt.GetConditionArg1();
                var weapName = GameSystems.MapObject.GetDisplayName(item);
                dispIo.damage.AddDamageBonus(damBonus, 12, 147, weapName);
            }
        }

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
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (GameSystems.Item.IsIdentified(item))
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item);
                var radMenuEntry = RadialMenuEntry.CreateAction(itemName, D20ActionType.ACTIVATE_DEVICE_STANDARD,
                    condArg1, "TAG_MAGIC_ITEM");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Items);
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
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoObjBonus();
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            dispIo.bonOut.AddBonus(1, 0, 242, itemName);
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x10102370)]
        public static void ArmorShadowSilentMovesSkillBonus(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoObjBonus();
            var v6 = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            dispIo.bonOut.AddBonus(5, 34, 112, v6);
        }


        [DispTypes(DispatcherType.MaxHP)]
        [TempleDllLocation(0x10102a00)]
        public static void FamiliarHpBonusGetMaxHp(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();

            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonus(condArg1, 0, 278, itemName);
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x10100250)]
        public static void ArmorEnhancementAcBonus(in DispatcherCallbackArgs evt)
        {
            var bonValue = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);

            var dispIo = evt.GetDispIoAttackBonus();
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            var caf = dispIo.attackPacket.flags;
            if ((caf & D20CAF.TOUCH_ATTACK) == 0)
            {
                dispIo.bonlist.AddBonus(bonValue, 12, 147, itemName);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ffc30)]
        public static void WeaponChaoticDamageBonus(in DispatcherCallbackArgs evt)
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
        public static void BootsOfSpeedAcBonus(in DispatcherCallbackArgs evt)
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
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            if (item.type == ObjectType.armor)
            {
                var bonValue = GameSystems.D20.GetArmorSkillCheckPenalty(item);
                if (!evt.objHndCaller.IsNPC() && !GameSystems.Feat.IsProficientWithArmor(evt.objHndCaller, item))
                {
                    dispIo.bonOut.AddBonus(bonValue, 0, 112, itemName);
                }
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10104da0)]
        [TemplePlusLocation("condition.cpp:464")]
        public static void BucklerToHitPenalty(in DispatcherCallbackArgs evt)
        {
            var invIdx = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoAttackBonus();

            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
                return;

            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weaponUsed == null)
                return;

            // TODO: Passing a damage packet to query if sth is wielded two handed seems off
            var dispIoDamage = new DispIoDamage();
            dispIoDamage.attackPacket = dispIo.attackPacket;
            dispIoDamage.damage = new DamagePacket();

            if (GameSystems.D20.D20QueryWithObject(evt.objHndCaller, D20DispatcherKey.QUE_WieldedTwoHanded,
                    dispIoDamage) != 0
                || dispIo.attackPacket.IsOffhandAttack())
            {
                dispIo.bonlist.AddBonus(-1, 0, 327);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100edfb0)]
        public static void AttackPowerTypeAdd(in DispatcherCallbackArgs evt, D20AttackPower attackPower)
        {
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoDamage();
            if (item == dispIo.attackPacket.weaponUsed || item == dispIo.attackPacket.ammoItem)
            {
                dispIo.damage.AddAttackPower(attackPower);
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
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo != null && item == dispIo.obj)
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
        public static void GoldenSkullActivateDeviceSpellPerform(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var v3 = (dispIo.action.data1 >> 16) & 0xFFFF;
            int v4 = dispIo.action.data1 & 0xFFFF;
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
                    var argIdx = 3 + v4 / 3;
                    int flag, mask;
                    switch (v4 % 3)
                    {
                        case 0:
                            flag = 0x70000;
                            mask = unchecked((int) 0xFF00FFFF);
                            break;
                        case 1:
                            flag = 0x700;
                            mask = unchecked((int) 0xFFFF00FF);
                            break;
                        case 2:
                            flag = 7;
                            mask = unchecked((int) 0xFFFFFF00);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var bitmask = evt.GetConditionArg(argIdx);
                    evt.SetConditionArg(argIdx, bitmask & mask | flag);
                }
            }
        }

        private static void AddGemSpellRadialEntry(in DispatcherCallbackArgs evt, GameObjectBody item,
            int spellIdx, int parentNodeIdx, bool quicken = false)
        {
            var itemInvIdx = evt.GetConditionArg3();

            var spellStoreData = item.GetSpell(obj_f.item_spell_idx, spellIdx);
            var mm = new MetaMagicData();
            mm.IsQuicken = quicken;
            var d20SpellData = new D20SpellData(spellStoreData.spellEnum, spellStoreData.classCode,
                spellStoreData.spellLevel, itemInvIdx, mm);

            var radMenuEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.ACTIVATE_DEVICE_SPELL);
            radMenuEntry.d20ActionData1 = (itemInvIdx << 16) | spellIdx;
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentNodeIdx);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10102ed0)]
        public static void GoldenSkullRadial(in DispatcherCallbackArgs evt)
        {
            var itemInvIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, itemInvIdx);
            var condArg1 = evt.GetConditionArg1();
            var condArg8 = evt.GetConditionArg(7);
            var condArg9 = evt.GetConditionArg(8);
            if (evt.objHndCaller.GetStat(Stat.level_cleric) != 0 && evt.objHndCaller.HasGoodAlignment()
                || evt.objHndCaller.GetStat(Stat.level_paladin) != 0 &&
                !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                return;
            }

            var whiteGemPresent = (condArg1 & WhiteGemMask) != 0;
            var orangeGemPresent = (condArg1 & OrangeGemMask) != 0;
            var redGemPresent = (condArg1 & RedGemMask) != 0;
            var blueGemPresent = (condArg1 & BlueGemMask) != 0;

            var parentEntry = RadialMenuEntry.CreateParent(GameSystems.MapObject.GetDisplayName(item));
            var parentIdx =
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                    RadialMenuStandardNode.Items);

            if (condArg8 == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 12, parentIdx);
            }

            if (condArg9 == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 13, parentIdx);
            }

            if (whiteGemPresent)
            {
                AddWhiteGemRadialMenu(in evt, item, parentIdx);
            }

            if (orangeGemPresent)
            {
                AddOrangeGemRadialMenu(in evt, item, parentIdx);
            }

            if (redGemPresent)
            {
                AddRedGemRadialMenu(in evt, item, parentIdx);
            }

            if (blueGemPresent)
            {
                AddBlueGemRadialMenu(in evt, item, parentIdx);
            }
        }

        private static void AddWhiteGemRadialMenu(in DispatcherCallbackArgs evt, GameObjectBody item, int parentIdxa)
        {
            var condArg4 = evt.GetConditionArg4();
            if (((condArg4 >> 16) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 0, parentIdxa);
            }

            if (((condArg4 >> 8) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 1, parentIdxa);
            }

            if ((condArg4 & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 2, parentIdxa, true);
            }
        }

        private static void AddOrangeGemRadialMenu(in DispatcherCallbackArgs evt, GameObjectBody item,
            in int parentIdxa)
        {
            var condArg5 = evt.GetConditionArg(4);
            if (((condArg5 >> 16) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 3, parentIdxa);
            }

            if (((condArg5 >> 8) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 4, parentIdxa);
            }

            if ((condArg5 & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 5, parentIdxa, true);
            }
        }

        private static void AddRedGemRadialMenu(in DispatcherCallbackArgs evt, GameObjectBody item, int parentIdxa)
        {
            var condArg6 = evt.GetConditionArg(5);
            if (((condArg6 >> 16) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 6, parentIdxa);
            }

            if (((condArg6 >> 8) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 7, parentIdxa);
            }

            if ((condArg6 & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 8, parentIdxa, true);
            }
        }

        private static void AddBlueGemRadialMenu(in DispatcherCallbackArgs evt, GameObjectBody item, int parentIdxa)
        {
            var condArg7 = evt.GetConditionArg(6);
            if (((condArg7 >> 16) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 9, parentIdxa);
            }

            if (((condArg7 >> 8) & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 10, parentIdxa);
            }

            if ((condArg7 & 0xFF) == 0)
            {
                AddGemSpellRadialEntry(in evt, item, 11, parentIdxa, true);
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
                var parentEntry = RadialMenuEntry.CreateParent(GameSystems.MapObject.GetDisplayName(item));
                var parentIdx =
                    GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                        RadialMenuStandardNode.Items);
                for (var i = 0; i < 3; i++)
                {
                    var spell = item.GetSpell(obj_f.item_spell_idx, i);
                    var d20SpellData = new D20SpellData(spell.spellEnum, spell.classCode, spell.spellLevel, condArg3);
                    var radMenuEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.USE_ITEM);
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
            if (!evt.objHndCaller.IsNPC() && !GameSystems.Feat.IsProficientWithArmor(evt.objHndCaller, v2))
            {
                var dispIo = evt.GetDispIoObjBonus();
                var itemName = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
                dispIo.bonOut.AddBonus(1, 0, 242, itemName);
            }
        }


        [DispTypes(DispatcherType.WeaponGlowType)]
        [TempleDllLocation(0x100edf40)]
        public static void GetWeaponGlowType(in DispatcherCallbackArgs evt, int weaponGlowType)
        {
            var dispIo = evt.GetDispIoD20Query();
            var queryForObj = (GameObjectBody) dispIo.obj;

            // This may be called directly on the item in which case the object is null
            if (evt.objHndCaller != null)
            {
                var condArg3 = evt.GetConditionArg3();
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                if (item != queryForObj)
                {
                    return;
                }
            }

            if (dispIo.return_val < weaponGlowType)
            {
                dispIo.return_val = weaponGlowType;
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x101003f0)]
        public static void ShieldEnhancementAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();

            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            if ((dispIo.attackPacket.flags & D20CAF.TOUCH_ATTACK) == 0)
            {
                dispIo.bonlist.AddBonus(condArg1, 33, 147, itemName);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x101044f0)]
        public static void HasThievesToolsQuery(in DispatcherCallbackArgs evt)
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
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var weaponUsed = dispIo.attackPacket.weaponUsed;
            if (item == weaponUsed || item == dispIo.attackPacket.ammoItem &&
                GameSystems.Item.AmmoMatchesWeapon(weaponUsed, item))
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(condArg1, 12, 147, itemName);
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x101001d0)]
        public static void ArmorBonusAcBonus(in DispatcherCallbackArgs evt)
        {
            var bonValue = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var caf = dispIo.attackPacket.flags;
            if ((caf & D20CAF.TOUCH_ATTACK) == 0)
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                dispIo.bonlist.AddBonus(bonValue, 28, 124, itemName);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x10100FC0)]
        public static void RingOfInvisSequence(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var seq = (ActionSequence) evt.GetDispIoD20Signal().obj;

            foreach (var action in seq.d20ActArray)
            {
                switch (action.d20ActType)
                {
                    default:
                        continue;
                    case D20ActionType.LAY_ON_HANDS_USE:
                        if (action.d20ATarget == action.d20APerformer)
                        {
                            // Apparently using lay on hands on yourself breaks the ring of invisibility
                            break;
                        }

                        return;
                    case D20ActionType.UNSPECIFIED_ATTACK:
                    case D20ActionType.STANDARD_ATTACK:
                    case D20ActionType.FULL_ATTACK:
                    case D20ActionType.STANDARD_RANGED_ATTACK:
                    case D20ActionType.CAST_SPELL:
                    case D20ActionType.CLEAVE:
                    case D20ActionType.ATTACK_OF_OPPORTUNITY:
                    case D20ActionType.WHIRLWIND_ATTACK:
                    case D20ActionType.TOUCH_ATTACK:
                    case D20ActionType.TOTAL_DEFENSE:
                    case D20ActionType.CHARGE:
                    case D20ActionType.TURN_UNDEAD:
                    case D20ActionType.DEATH_TOUCH:
                    case D20ActionType.BARDIC_MUSIC:
                    case D20ActionType.COUP_DE_GRACE:
                    case D20ActionType.STUNNING_FIST:
                    case D20ActionType.SMITE_EVIL:
                    case D20ActionType.TRIP:
                    case D20ActionType.USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL:
                    case D20ActionType.SPELL_CALL_LIGHTNING:
                    case D20ActionType.AOO_MOVEMENT:
                        // All of these actions break invisibility
                        break;
                }

                // Deactivate invisibility
                GameSystems.D20.D20SendSignal(evt.objHndCaller,
                    D20DispatcherKey.SIG_Magical_Item_Deactivate, condArg1);
                evt.SetConditionArg2(0);
                return;
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10102650)]
        public static void ActivateDeviceSpellRadial(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (condArg2 != 0)
            {
                var condArg1 = evt.GetConditionArg1();
                var spellData = item.GetSpell(obj_f.item_spell_idx, condArg1);
                var d20SpellData = new D20SpellData(spellData.spellEnum, spellData.classCode,
                    spellData.spellLevel, condArg3);

                var radMenuEntry = RadialMenuEntry.CreateSpellAction(d20SpellData, D20ActionType.ACTIVATE_DEVICE_SPELL);
                radMenuEntry.d20ActionData1 = condArg3;
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Items);
            }
        }


        [DispTypes(DispatcherType.ConditionAddFromD20StatusInit)]
        [TempleDllLocation(0x10100ec0)]
        public static void RingOfInvisibilityStatusD20StatusInit(in DispatcherCallbackArgs evt)
        {
            throw new NotImplementedException();
            // Storing conditions in the condition args in such a way is currently not supported (also... self-referential? wtf?)
            // evt.SetConditionArg1((int) evt.subDispNode.condNode);
            evt.SetConditionArg2(0);
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x10102550)]
        public static void WeaponDisruptionOnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var victim = dispIo.attackPacket.victim;
            var caf = dispIo.attackPacket.flags;
            if ((caf & D20CAF.RANGED) == 0
                && dispIo.attackPacket.GetWeaponUsed() == item
                && GameSystems.Critter.IsCategory(victim, MonsterCategory.undead)
                && !GameSystems.D20.Combat.SavingThrow(victim, evt.objHndCaller, 14, SavingThrowType.Will))
            {
                GameSystems.D20.Combat.Kill(victim, evt.objHndCaller);
                GameSystems.ParticleSys.CreateAtObj("sp-Destroy Undead", victim);
                GameSystems.RollHistory.CreateFromFreeText(GameSystems.D20.Combat.GetCombatMesLine(7000));
                GameSystems.RollHistory.CreateFromFreeText("\n");
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100efc80)]
        public static void ItemParticlesPack(in DispatcherCallbackArgs evt, int data)
        {
            throw new NotImplementedException();
            // ERMMMMMMM, doesnt this lose the handle?!?!?!
            // evt.SetConditionArg(data, 0xDEADBEEF);
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x10104290)]
        public static void FragarachToHitBonus(in DispatcherCallbackArgs evt)
        {
            if (!evt.objHndCaller.HasEvilAlignment())
            {
                var condArg3 = evt.GetConditionArg3();
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
                var dispIo = evt.GetDispIoAttackBonus();
                var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
                if (item == weaponUsed)
                {
                    dispIo.attackPacket.flags |= D20CAF.ALWAYS_HIT;
                    var weaponName = GameSystems.MapObject.GetDisplayName(weaponUsed, evt.objHndCaller);
                    dispIo.bonlist.AddBonus(4, 12, 112, weaponName);
                    dispIo.bonlist.zeroBonusSetMeslineNum(308);
                }
            }
        }

        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x10102280)]
        public static void BootsOfSpeedGetMoveSpeed(in DispatcherCallbackArgs evt)
        {
            if (evt.GetConditionArg4() == 0)
            {
                return; // Boots are not activated
            }

            var currentCap = 0;
            var dispIo = evt.GetDispIoMoveSpeed();

            for (var i = 0; i < dispIo.bonlist.bonCount; i++)
            {
                if (dispIo.bonlist.bonusEntries[i].bonType == 1)
                {
                    currentCap = dispIo.bonlist.bonusEntries[i].bonValue;
                    break;
                }
            }

            dispIo.bonlist.SetOverallCap(1, 2 * currentCap, 12, 174);
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x101015d0)]
        public static void ItemElementalResistanceDR(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var itemHandle = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var drType = (DamageType) evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoDamage();
            var itemName = GameSystems.MapObject.GetDisplayName(itemHandle, evt.objHndCaller);
            dispIo.damage.AddDR(condArg2, drType, 121, itemName);
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
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Item.IsIdentified(item))
            {
                if (condArg1 >= 1)
                {
                    var itemName = GameSystems.MapObject.GetDisplayName(item);
                    var radMenuEntry = RadialMenuEntry.CreateAction(itemName, D20ActionType.ACTIVATE_DEVICE_FREE,
                        condArg3, "TAG_MAGIC_ITEMS");
                    GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                        RadialMenuStandardNode.Items);
                }
            }
        }


        [DispTypes(DispatcherType.GetCriticalHitExtraDice)]
        [TempleDllLocation(0x10104fa0)]
        public static void RodOfSmiting_CritHit(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            var dispIo = evt.GetDispIoAttackBonus();
            var v5 = dispIo.attackPacket.GetWeaponUsed();
            if (condArg1 > 0 && v5 == item)
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item);
                dispIo.bonlist.AddBonus(2, 0, 112, itemName);
                evt.SetConditionArg1(condArg1 - 1);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x101023f0)]
        public static void BaneWeaponToHitBonus(in DispatcherCallbackArgs evt)
        {
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
            var resistanceType = (DamageType) args.GetConditionArg1();
            var invIndex = args.GetConditionArg3();
            var remainingAbsorption = args.GetConditionArg4();
            var dispIo = args.GetDispIoDamage();
            var damPkt = dispIo.damage;

            int damageOfType = damPkt.GetOverallDamageByType(resistanceType);
            if (damageOfType > 0 && remainingAbsorption > 0)
            {
                int absorbed;
                if (remainingAbsorption < damageOfType)
                {
                    absorbed = remainingAbsorption;
                    remainingAbsorption = 0;
                }
                else
                {
                    absorbed = damageOfType;
                    remainingAbsorption -= damageOfType;
                }

                Logger.Info("({0}) damage reduced", absorbed);
                args.SetConditionArg4(remainingAbsorption);
                var item = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, invIndex);
                var itemName = GameSystems.MapObject.GetDisplayName(item, args.objHndCaller);
                damPkt.AddDR(absorbed, resistanceType, 124, itemName);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x10100840)]
        [TemplePlusLocation("condition.cpp:455")]
        public static void UseableItemRadialEntry(in DispatcherCallbackArgs evt)
        {
            var invIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, invIdx);
            int useMagicDeviceSkillBase = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.use_magic_device);

            if (item.type != ObjectType.food && !GameSystems.Item.IsIdentified(item))
                return;

            var charges = item.GetInt32(obj_f.item_spell_charges_idx);
            if (charges == 0)
                return;

            var itemFlags = item.GetItemFlags();

            var spIdx = evt.GetConditionArg1();

            var spData = item.GetSpell(obj_f.item_spell_idx, spIdx);

            var handle = evt.objHndCaller;

            if (item.type == ObjectType.scroll || (itemFlags & ItemFlag.NEEDS_SPELL) != 0 &&
                (item.type == ObjectType.generic || item.type == ObjectType.weapon))
            {
                var isOk = false;

                if (useMagicDeviceSkillBase > 0 ||
                    GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spData.spellEnum))
                    isOk = true;

                // clerics with magic domain
                else if (GameSystems.Spell.IsArcaneSpellClass(spData.classCode))
                {
                    var clrLvl = handle.GetStat(Stat.level_cleric);
                    if (clrLvl > 0 && Math.Max(1, clrLvl / 2) >= (int) spData.spellLevel &&
                        GameSystems.Critter.HasDomain(handle, DomainId.Magic))
                        isOk = true;
                }

                if (!isOk)
                    return;
            }

            if (item.type == ObjectType.scroll &&
                !GameSystems.Spell.CheckAbilityScoreReqForSpell(evt.objHndCaller, spData.spellEnum) &&
                useMagicDeviceSkillBase == 0)
                return;

            var actType = D20ActionType.USE_ITEM;
            if (item.type == ObjectType.food)
            {
                if (GameSystems.Item.IsMagical(item))
                    actType = D20ActionType.USE_POTION;
                else
                    actType = D20ActionType.USE_ITEM;
            }
            else
            {
                actType = D20ActionType.USE_ITEM;
            }

            var itemName = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
            var radEntry = RadialMenuEntry.CreateAction(itemName, actType, invIdx, null);
            radEntry.d20SpellData.SetSpellData(spData.spellEnum, spData.classCode, spData.spellLevel, invIdx);

            var chargesRem = charges;
            if (item.type == ObjectType.scroll || item.type == ObjectType.food)
            {
                chargesRem = item.GetQuantity();
            }

            if (chargesRem > 1)
            {
                radEntry.text = $"{radEntry.text} ({chargesRem})";
            }

            RadialMenuStandardNode parentType;
            switch (item.type)
            {
                case ObjectType.scroll:
                    parentType = RadialMenuStandardNode.Scrolls;
                    break;
                case ObjectType.food:
                    parentType = RadialMenuStandardNode.Potions;
                    break;
                default:
                    parentType = evt.GetConditionArg2() != 3
                        ? RadialMenuStandardNode.Items
                        : RadialMenuStandardNode.Wands;
                    break;
            }

            radEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spData.spellEnum);
            if (!GameSystems.Spell.SpellHasMultiSelection(spData.spellEnum))
            {
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radEntry, parentType);
            }
            else
            {
                radEntry.type = RadialMenuEntryType.Parent;
                radEntry.d20ActionType = D20ActionType.NONE;
                var parentNodeIdx = GameSystems.D20.RadialMenu.AddToStandardNode(handle, ref radEntry, parentType);
                if (!GameSystems.Spell.TryGetMultiSelectOptions(spData.spellEnum, out var multiOptions))
                {
                    Logger.Error("Spell multiselect options not found!");
                }

                // populate options
                for (var i = 0; i < multiOptions.Count; i++)
                {
                    var op = multiOptions[i];

                    var spellData = new D20SpellData(spData.spellEnum, spData.classCode, spData.spellLevel, invIdx);
                    RadialMenuEntry radChild = RadialMenuEntry.CreateSpellAction(spellData, actType);
                    radChild.d20ActionData1 = invIdx;
                    radChild.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spData.spellEnum);
                    GameSystems.D20.RadialMenu.SetCallbackCopyEntryToSelected(ref radChild);

                    if (op.isProto)
                    {
                        var protoId = op.value;
                        radChild.minArg = protoId;

                        var protoHandle = GameSystems.Proto.GetProtoById((ushort) protoId);
                        radChild.text = GameSystems.MapObject.GetDisplayName(protoHandle);
                    }
                    else
                    {
                        radChild.text = GameSystems.Spell.GetSpellsRadialMenuOptions(op.value);
                        radChild.minArg = i + 1;
                    }

                    radChild.AddAsChild(handle, parentNodeIdx);
                }
            }

            // add to Copy Scroll
            if (item.type == ObjectType.scroll && evt.objHndCaller.GetStat(Stat.level_wizard) >= 1
                                               && GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller,
                                                   spData.spellEnum)
                                               && GameSystems.Spell.IsArcaneSpellClass(spData.classCode))
            {
                // check if spell is not known
                var spellClasses = new List<int>();
                var spellLevels = new List<int>();
                GameSystems.Spell.SpellKnownQueryGetData(evt.objHndCaller, spData.spellEnum, spellClasses, spellLevels);

                var alreadyKnows = false;

                for (var i = 0; i < spellClasses.Count; i++)
                {
                    if (spellClasses[i] == GameSystems.Spell.GetSpellClass(Stat.level_wizard))
                        return;
                }

                var spLvl = GameSystems.Spell.GetSpellLevelBySpellClass(spData.spellEnum,
                    GameSystems.Spell.GetSpellClass(Stat.level_wizard));

                if (spLvl >= 0)
                {
                    radEntry.text = GameSystems.MapObject.GetDisplayName(item, evt.objHndCaller);
                    radEntry.type = RadialMenuEntryType.Action;
                    radEntry.helpSystemHashkey = GameSystems.Spell.GetSpellHelpTopic(spData.spellEnum);
                    radEntry.d20ActionType = D20ActionType.COPY_SCROLL;
                    radEntry.d20ActionData1 = item.GetItemInventoryLocation();
                    GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radEntry,
                        RadialMenuStandardNode.CopyScroll);
                }
            }

            return;
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ff6f0)]
        public static void WeaponHasEnhancementBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            dispIo.data1 = evt.GetConditionArg1();
        }

        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x101014c0)]
        public static void BurstDamageTargetParticles(in DispatcherCallbackArgs evt, string partSysName)
        {
            var dispIo = evt.GetDispIoDamage();
            var victim = dispIo.attackPacket.victim;

            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
            if (dispIo.attackPacket.GetWeaponUsed() == v2 && partSysName != null)
            {
                var caf = dispIo.attackPacket.flags;
                if ((caf & D20CAF.CRITICAL) != 0)
                {
                    GameSystems.ParticleSys.CreateAtObj(partSysName, victim);
                }
            }
        }


        [DispTypes(DispatcherType.GetBonusAttacks)]
        [TempleDllLocation(0x10102190)]
        [TemplePlusLocation("spell_condition.cpp:258")]
        public static void BootsOfSpeedBonusAttack(in DispatcherCallbackArgs evt)
        {
            if (evt.GetConditionArg4() != 0)
            {
                var dispIo = evt.GetDispIoD20ActionTurnBased();
                dispIo.bonlist.AddBonus(1, 34, 174); // Haste
            }
        }
    }
}