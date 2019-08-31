using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
namespace SpicyTemple.Core.Systems.D20.Conditions {

public static class ItemEffects {

private static readonly ILogger Logger = new ConsoleLogger();
[TempleDllLocation(0x102f07a8)]
  public static readonly ConditionSpec WeaponMasterwork = ConditionSpec.Create("Weapon Masterwork", 3)
.AddHandler(DispatcherType.ToHitBonus2, WeaponMasterworkToHitBonus)
.AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
                    .Build();


[TempleDllLocation(0x102f07f0)]
  public static readonly ConditionSpec WeaponEnhancementBonus = ConditionSpec.Create("Weapon Enhancement Bonus", 5)
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
.AddHandler(DispatcherType.GetAC, sub_10101B60)
.AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
.AddHandler(DispatcherType.RadialMenuEntry, ActivateDefendingWeaponRadial)
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 1)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
                    .Build();


[TempleDllLocation(0x102f0998)]
  public static readonly ConditionSpec WeaponFlaming = ConditionSpec.Create("Weapon Flaming", 3)
.AddHandler(DispatcherType.DealingDamage, sub_100FF730, 10)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Fire")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 6)
                    .Build();


[TempleDllLocation(0x102f0a18)]
  public static readonly ConditionSpec WeaponFrost = ConditionSpec.Create("Weapon Frost", 3)
.AddHandler(DispatcherType.DealingDamage, sub_100FF730, 8)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4100)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 5)
                    .Build();


[TempleDllLocation(0x102f0a98)]
  public static readonly ConditionSpec WeaponShock = ConditionSpec.Create("Weapon Shock", 3)
.AddHandler(DispatcherType.DealingDamage, sub_100FF730, 9)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Shock")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 9)
                    .Build();


[TempleDllLocation(0x102f0c50)]
  public static readonly ConditionSpec WeaponFlamingBurst = ConditionSpec.Create("Weapon Flaming Burst", 3)
.AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, 10)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Fire")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-FIRE-burst")
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 6)
                    .Build();


[TempleDllLocation(0x102f0ce8)]
  public static readonly ConditionSpec WeaponIcyBurst = ConditionSpec.Create("Weapon Icy Burst", 3)
.AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, 8)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4096)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-COLD-burst")
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 5)
                    .Build();


[TempleDllLocation(0x102f0d80)]
  public static readonly ConditionSpec WeaponShockingBurst = ConditionSpec.Create("Weapon Shocking Burst", 3)
.AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, 9)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Shock")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.DealingDamage2, BurstDamageTargetParticles, "hit-SHOCK-burst")
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 9)
                    .Build();


[TempleDllLocation(0x102f0e18)]
  public static readonly ConditionSpec WeaponHoly = ConditionSpec.Create("Weapon Holy", 3)
.AddHandler(DispatcherType.DealingDamage, sub_100FFA50)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 12)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-HOLY-medium")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd, 273, 8)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 8)
.AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 8)
.AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 8)
.AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 8)
.AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 8)
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 7)
                    .Build();


[TempleDllLocation(0x102f0f10)]
  public static readonly ConditionSpec WeaponUnholy = ConditionSpec.Create("Weapon Unholy", 3)
.AddHandler(DispatcherType.DealingDamage, sub_100FF960)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 20)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-UNHOLY-medium")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd, 273, 4)
.AddHandler(DispatcherType.SkillLevel, CommonConditionCallbacks.NegativeLevelSkillPenalty, 273, 4)
.AddHandler(DispatcherType.ToHitBonus2, CommonConditionCallbacks.NegativeLevelToHitBonus, 273, 4)
.AddHandler(DispatcherType.SaveThrowLevel, CommonConditionCallbacks.sub_100EF6E0, 273, 4)
.AddHandler(DispatcherType.MaxHP, CommonConditionCallbacks.NegativeLevelMaxHp, 273, 4)
.AddHandler(DispatcherType.GetLevel, CommonConditionCallbacks.NegativeLevel, 273, 4)
.AddHandler(DispatcherType.WeaponGlowType, WeaponGlowCb, 10)
                    .Build();


[TempleDllLocation(0x102f1008)]
  public static readonly ConditionSpec WeaponLawful = ConditionSpec.Create("Weapon Lawful", 3)
.AddHandler(DispatcherType.DealingDamage, sub_100FFB40)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 68)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.DealingDamage2, WeaponPlayParticleOnHit, "hit-LAW-medium")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd, 273, 2)
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
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.TempNegativeLvlOnAdd, 273, 1)
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
.AddHandler(DispatcherType.DealingDamage, sub_101024A0)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
.AddHandler(DispatcherType.ToHitBonus2, sub_101023F0)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Bane")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
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
.AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus, sub_100FFF90)
.AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_Speed, sub_100FFFD0)
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
.AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus, sub_100FFF90)
.AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_Speed, sub_100FFFD0)
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
  public static readonly ConditionSpec ShieldEnhancementBonus = ConditionSpec.Create("Shield Enhancement Bonus", 3)
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
  public static readonly ConditionSpec SkillCircumstanceBonus = ConditionSpec.Create("Skill Circumstance Bonus", 3)
.AddHandler(DispatcherType.SkillLevel, SkillBonusCallback, 21)
                    .Build();


[TempleDllLocation(0x102f199c)]
  public static readonly ConditionSpec SkillCompetenceBonus = ConditionSpec.Create("Skill Competence Bonus", 3)
.AddHandler(DispatcherType.SkillLevel, SkillBonusCallback, 34)
                    .Build();


[TempleDllLocation(0x102f19cc)]
  public static readonly ConditionSpec AttributeEnhancementBonus = ConditionSpec.Create("Attribute Enhancement Bonus", 3)
.AddHandler(DispatcherType.AbilityScoreLevel, AttributeEnhancementBonus_callback)
                    .Build();


[TempleDllLocation(0x102f19fc)]
  public static readonly ConditionSpec SavingThrowResistanceBonus = ConditionSpec.Create("Saving Throw Resistance Bonus", 3)
.AddHandler(DispatcherType.SaveThrowLevel, AddSavingThrowResistanceBonus)
                    .Build();


[TempleDllLocation(0x102f1a30)]
  public static readonly ConditionSpec ItemParticleSystem = ConditionSpec.Create("Item Particle System", 3)
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, sub_100ED3E0, 1, 0)
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Show, sub_100ED3E0, 1, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Hide, CommonConditionCallbacks.EndParticlesFromArg, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Pack, sub_100EFC80, 1)
                    .Build();


[TempleDllLocation(0x102f1ab0)]
  public static readonly ConditionSpec FrostBow = ConditionSpec.Create("Frost Bow", 3)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-Arrow Frost")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
                    .Build();


[TempleDllLocation(0x102f1c20)]
  public static readonly ConditionSpec RingofInvisibility = ConditionSpec.Create("Ring of Invisibility", 3)
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, RingOfInvisibilityStatusD20StatusInit)
.AddHandler(DispatcherType.ConditionRemove, RingOfInvisibilityRemove)
.AddHandler(DispatcherType.RadialMenuEntry, RingOfInvisibilityRadial)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)116, RingOfInvisPerform)
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
  public static readonly ConditionSpec ProofAgainstDetectionLocation = ConditionSpec.Create("Proof Against Detection Location", 3)
                    .Build();


[TempleDllLocation(0x102f1b70)]
  public static readonly ConditionSpec ElementalResistance = ConditionSpec.Create("Elemental Resistance", 3)
.AddHandler(DispatcherType.TakingDamage2, ItemElementalResistanceDR)
                    .Build();


[TempleDllLocation(0x102f1ba0)]
  public static readonly ConditionSpec StaffOfStriking = ConditionSpec.Create("Staff Of Striking", 3)
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 0)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.RadialMenuEntry, StaffOfStrikingRadial)
.AddHandler(DispatcherType.DealingDamage, sub_10101760)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
                    .Build();


[TempleDllLocation(0x102f1ca0)]
  public static readonly ConditionSpec DaggerofVenom = ConditionSpec.Create("Dagger of Venom", 4)
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, sub_10101820)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.CondNodeSetArgToZero)
.AddHandler(DispatcherType.RadialMenuEntry, DaggerOfVenomRadial)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)116, DaggerOfVenomActivate)
.AddHandler(DispatcherType.DealingDamage2, DaggerOfVenomPoisonOnDamage)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_10101A20)
                    .Build();


[TempleDllLocation(0x102f1d60)]
  public static readonly ConditionSpec ElementalResistanceperround = ConditionSpec.Create("Elemental Resistance per round", 4)
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
  public static readonly ConditionSpec Ringoffreedomofmovement = ConditionSpec.Create("Ring of freedom of movement", 3)
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
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)120, BootsOfSpeedD20Check)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)120, BootsOfSpeedPerform)
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
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance, CommonConditionCallbacks.SpellResistanceQuery)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipSpellResistanceCallback, 5048)
                    .Build();


[TempleDllLocation(0x102f2068)]
  public static readonly ConditionSpec WeaponFlameTongue = ConditionSpec.Create("Weapon Flame Tongue", 3)
.AddHandler(DispatcherType.DealingDamage, BurstWeaponCritDice, 10)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.RadialMenuEntry, ActivateDeviceSpellRadial)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)122, sub_101027C0)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 1)
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
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)122, sub_101027C0)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 1)
                    .Build();


[TempleDllLocation(0x102f21a0)]
  public static readonly ConditionSpec SwordofLifeStealing = ConditionSpec.Create("Sword of Life Stealing", 3)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.ToHitBonus2, WeaponEnhancementToHitBonus)
.AddHandler(DispatcherType.DealingDamage, WeaponDamageBonus)
.AddHandler(DispatcherType.DealingDamage, AttackPowerTypeAdd, 4)
.AddQueryHandler(D20DispatcherKey.QUE_Masterwork, QueryMasterwork)
.AddHandler(DispatcherType.DealingDamage2, sub_10102800)
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
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)122, sub_10103A00)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_10103AE0)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
                    .Build();


[TempleDllLocation(0x102f23c0)]
  public static readonly ConditionSpec ElementalGem = ConditionSpec.Create("Elemental Gem", 3)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.RadialMenuEntry, ElementalGemRadial)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)122, ElementalGemPerform)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ElementalGemNewdayRest)
.AddQueryHandler(D20DispatcherKey.QUE_Elemental_Gem_State, ElementalGemQueryState)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)120, ElementalGemActionCheck)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)120, sub_101040D0)
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
  public static readonly ConditionSpec NormalProjectileParticles = ConditionSpec.Create("Normal Projectile Particles", 3)
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-arrow normal")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
                    .Build();


[TempleDllLocation(0x102f2558)]
  public static readonly ConditionSpec ThievesTools = ConditionSpec.Create("Thieves Tools", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Has_Thieves_Tools, sub_101044F0)
                    .Build();


[TempleDllLocation(0x102f2588)]
  public static readonly ConditionSpec ThievesToolsMasterwork = ConditionSpec.Create("Thieves Tools Masterwork", 3)
.AddQueryHandler(D20DispatcherKey.QUE_Has_Thieves_Tools, sub_101044F0)
.AddSkillLevelHandler(SkillId.disable_device, ThievesToolsMasterworkSkillLevel)
.AddSkillLevelHandler(SkillId.open_lock, ThievesToolsMasterworkSkillLevel)
                    .Build();


[TempleDllLocation(0x102f25e0)]
  public static readonly ConditionSpec CompositeBow = ConditionSpec.Create("Composite Bow", 3)
.AddHandler(DispatcherType.ToHitBonus2, sub_10104700)
.AddHandler(DispatcherType.DealingDamage, sub_101047C0)
                    .Build();


[TempleDllLocation(0x102f26a0)]
  public static readonly ConditionSpec JaersSpheresofFire = ConditionSpec.Create("Jaer's Spheres of Fire", 3)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "sp-Spheres of Fire-proj")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
                    .Build();


[TempleDllLocation(0x102f26e8)]
  public static readonly ConditionSpec HolyWater = ConditionSpec.Create("Holy Water", 3)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-arrow normal")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.DealingDamage, HolyWaterOnDamage, 1)
                    .Build();


[TempleDllLocation(0x102f2740)]
  public static readonly ConditionSpec UnholyWater = ConditionSpec.Create("Unholy Water", 3)
.AddHandler(DispatcherType.ProjectileCreated, ProjectileCreatileParticles, "ef-arrow normal")
.AddHandler(DispatcherType.ProjectileDestroyed, sub_101013D0, 1)
.AddHandler(DispatcherType.DealingDamage, sub_10104660, 1)
                    .Build();


[TempleDllLocation(0x102f2624)]
  public static readonly ConditionSpec BardicInstrument = ConditionSpec.Create("Bardic Instrument", 3)
.AddQueryHandler(D20DispatcherKey.QUE_BardicInstrument, CommonConditionCallbacks.D20Query_Callback_GetSDDKey1, 0)
                    .Build();


[TempleDllLocation(0x102f2798)]
  public static readonly ConditionSpec CharismaCompetenceBonus = ConditionSpec.Create("Charisma Competence Bonus", 3)
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
.AddHandler(DispatcherType.ToHitBonus2, sub_10104990)
.AddHandler(DispatcherType.DealingDamage, sub_10104A20)
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
  public static readonly ConditionSpec UseableItemXTimesPerDay = ConditionSpec.Create("Useable Item X Times Per Day", 4)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CondNodeAnullArg3)
.AddHandler(DispatcherType.RadialMenuEntry, UseableItemXTimesPerDayRadialMenu)
.AddHandler(DispatcherType.ItemForceRemove, ItemForceRemoveCallback_SetItemPadWielderArgs)
.AddHandler(DispatcherType.D20ActionPerform, D20DispatcherKey.LVL_Stats_Finalize, UseableItemXTimesPerDayPerform)
                    .Build();


[TempleDllLocation(0x102f2990)]
  public static readonly ConditionSpec Buckler = ConditionSpec.Create("Buckler", 3)
.AddHandler(DispatcherType.ToHitBonus2, BucklerToHitPenalty)
.AddHandler(DispatcherType.GetAC, BucklerAcBonus)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 0)
.AddHandler(DispatcherType.BucklerAcPenalty, BucklerAcPenalty)
.AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_AC_Bonus, QueryAcBonus, 0)
.AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus, sub_100FFF90)
.AddQueryHandler(D20DispatcherKey.QUE_Armor_Get_Max_Speed, sub_100FFFD0)
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


public static IReadOnlyList<ConditionSpec> Conditions {get;} = new List<ConditionSpec>
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
public static void   BootsOfSpeedPerform(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg4;
  int condArg5;
  int v4;
  
  int v6;

  condArg3 = evt.GetConditionArg3();
  condArg4 = evt.GetConditionArg4();
  condArg5 = evt.GetConditionArg(4);
  if ( evt.GetDispIoD20ActionTurnBased().action.data1 == condArg3 )
  {
    if ( (condArg4 )!=0)
    {
      v4 = 0;
      GameSystems.ParticleSys.Remove(condArg5);
    }
    else
    {
      v4 = 1;
      v6 = GameSystems.ParticleSys.CreateAtObj("sp-Haste", evt.objHndCaller);
      evt.SetConditionArg(4, v6);
    }
    evt.SetConditionArg4(v4);
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x10102800)]
public static void   sub_10102800(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  Dice v3;
  int v4;
  unsigned Dice v5;
  int v6;
  Dice v7;
  int v8;
  int v9;
  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  if ( !(evt.dispIO[4].ioType & 4) && (AttackPacket *)&evt.dispIO[1].GetWeaponUsed() == v2 )
  {
    if ( evt.dispIO[4].ioType & 2 )
    {
      *(_QWORD *)&evt.dispIO[2].AddCondition(StatusEffects.TempNegativeLevel, 0, 16);
      v3 = 1.new Dice(6, 0);
      v4 = GetPackedDiceBonus/*0x10038c90*/(v3);
      v5 = 1.new Dice(6, 0);
      v6 = GetPackedDiceType/*0x10038c40*/(v5);
      v7 = 1.new Dice(6, 0);
      v8 = GetPackedDiceNumDice/*0x10038c30*/(v7);
      v9 = DiceRoller/*0x10038b60*/(v8, v6, v4);
      evt.objHndCaller.AddCondition(StatusEffects.TemporaryHitPoints, 0, 14400, v9);
    }
  }
}


[DispTypes(DispatcherType.NewDay)]
[TempleDllLocation(0x10103ae0)]
public static void   sub_10103AE0(in DispatcherCallbackArgs evt)
{  int v2;
  CHAR v3;
  int v4;
  int v5;
  int v6;
  int v7;
  int condArg1;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  evt.SetConditionArg(7, 0);
  evt.SetConditionArg(8, 0);
  v2 = 3;
  v3 = 24;
  v4 = 4;
  do
  {
    if ( (1 << v3) & condArg1 )
    {
      v5 = evt.GetConditionArg(v2);
      v6 = (v5 << 8 >> 24) - 1;
      if ( v6 < 0 )
      {
        v6 = 0;
      }
      v7 = (v5 << 16 >> 24) - 1;
      if ( v7 < 0 )
      {
        v7 = 0;
      }
      evt.SetConditionArg(v2, v6 | ((v7 | (v6 << 8)) << 8));
    }
    v3 -= 8;
    ++v2;
    --v4;
  }
  while ( v4 );
}


[DispTypes(DispatcherType.NewDay)]
[TempleDllLocation(0x10101eb0)]
[TemplePlusLocation("ability_fixes.cpp:71")]
public static void   BootsOfSpeedNewday(in DispatcherCallbackArgs evt)
{
  int condArg2;

  condArg2 = evt.GetConditionArg2();
  evt.SetConditionArg1(condArg2);
}
/* Orphan comments:
TP Replaced @ ability_fixes.cpp:71
*/


[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x10100b60)]
[TemplePlusLocation("condition.cpp:454")]
public static void   UseableItemActionCheck(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int invIdx_;
  GameObjectBody item;
  int v4;
  int v5;
  int itemType;
  int v7;
  CHAR itemFlags;
  int condArg1;
  DispIoD20ActionTurnBased dispIo;
  int umdSkillRank;
  SpellStoreData spData;

  dispIo = evt.GetDispIoD20ActionTurnBased();
  condArg3 = evt.GetConditionArg3();
  invIdx_ = condArg3;
  item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v4 = HIDWORD(item);
  v5 = item;
  itemType = item.type;
  umdSkillRank = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.use_magic_device);
  if ( dispIo.action.data1 == invIdx_ )
  {
    if ( itemType != 8 && !GameSystems.Item.IsIdentified(__PAIR__(v4, v5)) )
    {
      goto LABEL_21;
    }
    v7 = GameSystems.Item.GetItemSpellCharges(__PAIR__(v4, v5));
    if ( v7 != -1 && v7 < 1 )
    {
      dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
      return;
    }
    itemFlags = __PAIR__(v4, v5).GetItemFlags();
    condArg1 = evt.GetConditionArg1();
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v4, v5), obj_f.item_spell_idx, condArg1, &spData);
    if ( (itemType == 9 || itemFlags & ItemFlag.NEEDS_SPELL && (itemType == 12 || itemType == 4))
      && !GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spData.spellEnum)
      && (umdSkillRank )==0)
    {
      dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
      return;
    }
    if ( itemType != 9 || GameSystems.Spell.CheckAbilityScoreReqForSpell(evt.objHndCaller, spData.spellEnum, -1) || (umdSkillRank )!=0)
    {
      dispIo.returnVal = 0;
    }
    else
    {
LABEL_21:
      dispIo.returnVal = 14;
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:454
*/


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100ff960)]
public static void   sub_100FF960(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoDamage dispIo;
  DispIoDamage v4;
  GameObjectBody v5;
  int v6;
  int v7;
  int v8;
  Dice v9;
  string v10;
  GameObjectBody objHnd;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  v4 = dispIo;
  v5 = dispIo.attackPacket.GetWeaponUsed();
  v6 = HIDWORD(v5);
  v7 = v5;
  objHnd = v4.attackPacket.victim;
  v8 = v5.WeaponFlags;
  if ( (int)v2 == v7 && HIDWORD(v2) == v6 || (v8 & 0x400) !=0&& GameSystems.Item.ItemWornAt(evt.objHndCaller, 9) == __PAIR__(v6, v7) )
  {
    if ( (objHnd.GetStat(Stat.alignment) & 4 )!=0)
    {
      v9 = 2.new Dice(6, 0);
      v10 = GameSystems.MapObject.GetDisplayName(__PAIR__(v6, v7), evt.objHndCaller);
      v4.damage.AddDamageDice(v9, DamageType.Unspecified, 0x79, v10);
    }
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x10100370)]
public static void   ShieldAcBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoAttackBonus dispIo;
  string v4;
  D20CAF v5;
  int bonValue;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  dispIo = evt.GetDispIoAttackBonus();
  v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
  v5 = dispIo.attackPacket.flags;
  if ( !(((v5 & D20CAF.TOUCH_ATTACK)!=0)) )
  {
    dispIo.bonlist.AddBonus(bonValue, 29, 125, v4);
  }
}


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x10100f20)]
public static void   RingOfInvisPerform(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  if ( condArg1 == evt.GetDispIoD20ActionTurnBased().action.data1 )
  {
    if ( (condArg2 )!=0)
    {
      GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Magical_Item_Deactivate, condArg1, (ulong)condArg1 >> 32);
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
public static void   sub_10101A20(in DispatcherCallbackArgs evt)
{
  evt.SetConditionArg2(1);
}


[DispTypes(DispatcherType.ConditionRemove)]
[TempleDllLocation(0x10100ef0)]
public static void   RingOfInvisibilityRemove(in DispatcherCallbackArgs evt)
{
  int condArg1;

  condArg1 = evt.GetConditionArg1();
  GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Magical_Item_Deactivate, condArg1, (ulong)condArg1 >> 32);
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x10102970)]
public static void   SavingThrow_FamiliarBonus_Callback(in DispatcherCallbackArgs evt)
{  int condArg3;
  GameObjectBody v3;
  int condArg2;
  string v5;/*INLINED:v1=evt.subDispNode*/  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetDispIoSavingThrow();
  if ( evt.dispKey - 7 == evt.GetConditionArg1() )
  {
    condArg2 = evt.GetConditionArg2();
    v5 = GameSystems.MapObject.GetDisplayName(v3, evt.objHndCaller);
    (BonusList *)&evt.subDispNode[2].AddBonus(condArg2, 0, 278, v5);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100fff20)]
public static void   QueryAcBonus(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Query dispIo;
  int condArg3;

  dispIo = evt.GetDispIoD20Query();
  if ( (evt.objHndCaller == null)    || (condArg3 = evt.GetConditionArg3(),
        *(_QWORD *)&dispIo.data1 == GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3)) )
  {
    dispIo.return_val += evt.GetConditionArg(data);
  }
}


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x101040d0)]
public static void   sub_101040D0(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int v3;
  int v4;
  int v5;
  int v6;
  FadeAndTeleportArgs v7;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  if ( evt.GetDispIoD20ActionTurnBased().action.data1 == condArg2 )
  {
    evt.SetConditionArg1(condArg1 | 0x1000000);
    v3 = 3 * condArg2;
    v4 = dword_10290088/*0x10290088*/[3 * condArg2];
    v5 = dword_10290084/*0x10290084*/[3 * condArg2];
    v7.flags = 512;
    v7.obj = GameSystems.Party.GetPCGroupMemberN(0);
    v6 = dword_10290080/*0x10290080*/[v3];
    v7.destLoc.locx = v5;
    v7.destLoc.locy = ((ulong)v5 >> 32) | v4;
    v7.destMapId = v6;
    GameSystems.Teleport.FadeAndTeleport(&v7);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100ff670)]
[TemplePlusLocation("general_condition_fixes.cpp:38")]
public static void   WeaponKeepBonus(in DispatcherCallbackArgs evt)
{
  evt.GetDispIoD20Query().return_val = 2;
}
/* Orphan comments:
TP Replaced @ general_condition_fixes.cpp:38
*/


[DispTypes(DispatcherType.NewDay)]
[TempleDllLocation(0x10104cc0)]
public static void   CondNodeAnullArg3(in DispatcherCallbackArgs evt)
{
  evt.SetConditionArg4(0);
}


[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x10104080)]
public static void   ElementalGemActionCheck(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int v2;

  condArg2 = evt.GetConditionArg2();
  if ( evt.GetDispIoD20ActionTurnBased().action.data1 == condArg2 )
  {
    GameSystems.Map.GetCurrentMapId();
    v2 = dword_10290080/*0x10290080*/[3 * condArg2];
  }
}


[DispTypes(DispatcherType.ItemForceRemove)]
[TempleDllLocation(0x10104410)]
public static void   ItemForceRemoveCallback_SetItemPadWielderArgs(in DispatcherCallbackArgs evt)
{
  int i;
  int condArg3;
  GameObjectBody item;
  int itemHi;
  int itemLo;
  int v6;
  
  int j;
  int arg;
  int v10;

  i = 0;
  v10 = 0;
  condArg3 = evt.GetConditionArg3();
  item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  itemHi = HIDWORD(item);
  itemLo = item;
  if ( item.GetArrayLength(obj_f.item_pad_wielder_condition_array) > 0 )
  {
    while ( 1 )
    {
      v6 = __PAIR__(itemHi, itemLo).GetInt32(obj_f.item_pad_wielder_condition_array, i);
      if ( v6 == evt.subDispNode.condNode.condStruct )
      {
        break;
      }
      v10 += v6.numArgs;
      if ( ++i >= __PAIR__(itemHi, itemLo).GetArrayLength(obj_f.item_pad_wielder_condition_array) )
      {
        return;
      }
    }
    for ( j = 0; j < v6.numArgs; ++j )
    {
      arg = evt.GetConditionArg(j);
      __PAIR__(itemHi, itemLo).SetInt32(obj_f.item_pad_wielder_argument_array, j + v10, arg);
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x10104990)]
public static void   sub_10104990(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  DispIoAttackBonus dispIo;
  string v4;
  GameObjectBody ObjHnd;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  ObjHnd = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  if ( (dispIo.attackPacket.GetWeaponUsed() == null))
  {
    v4 = GameSystems.MapObject.GetDisplayName(ObjHnd, evt.objHndCaller);
    dispIo.bonlist.AddBonus(condArg1, 0, 112, v4);
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x10102140)]
public static void   BootsOfSpeedSaveThrow(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  if ( (evt.GetConditionArg4() )!=0)
  {
    dispIo = evt.GetDispIoSavingThrow();
    if ( evt.dispKey == D20DispatcherKey.SAVE_REFLEX )
    {
      dispIo.bonlist.AddBonus(data1, 8, data2);
    }
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x10104ee0)]
public static void   BucklerAcBonus(in DispatcherCallbackArgs args)
{  int condArg3;
  DispIoAttackBonus dispIo;
  int condArg1;
  string v5;
  D20CAF v6;
  GameObjectBody ObjHnd;/*INLINED:v1=args.subDispNode*/  args.subDispNode = (SubDispNode *)args.GetConditionArg2();
  condArg3 = args.GetConditionArg3();
  ObjHnd = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, condArg3);
  dispIo = args.GetDispIoAttackBonus();
  if ( args.subDispNode )
  {
    dispIo.bonlist.zeroBonusSetMeslineNum(326);
  }
  else
  {
    condArg1 = args.GetConditionArg1();
    v5 = GameSystems.MapObject.GetDisplayName(ObjHnd, args.objHndCaller);
    v6 = dispIo.attackPacket.flags;
    if ( !(((v6 & D20CAF.TOUCH_ATTACK)!=0)) )
    {
      dispIo.bonlist.AddBonus(condArg1, 29, 125, v5);
    }
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x10104880)]
public static void   CharismaCompetenceBonusSkillLevel(in DispatcherCallbackArgs args)
{
  int condArg1;
  int condArg3;
  string v3;
  GameObjectBody ObjHnd;

  condArg1 = args.GetConditionArg1();
  condArg3 = args.GetConditionArg3();
  ObjHnd = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, condArg3);
  args.subDispNode = (SubDispNode *)args.GetDispIoObjBonus();
  if ( condArg3 >= 100 )
  {
    v3 = GameSystems.MapObject.GetDisplayName(ObjHnd, args.objHndCaller);
    (BonusList *)args.subDispNode.next.AddBonus(condArg1, 34, 112, v3);
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10101960)]
public static void   DaggerOfVenomRadial(in DispatcherCallbackArgs evt)
{  int condArg1;
  int condArg3;
  GameObjectBody v4;
  int v5;
  int v6;
  int v7;
  RadialMenuEntry radMenuEntry;
  int evta;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  v4 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v5 = HIDWORD(v4);
  v6 = v4;
  if ( GameSystems.Item.IsIdentified(v4) )
  {
    if ( (evta )!=0)
    {
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_STANDARD;
      radMenuEntry.d20ActionData1 = condArg1;
      radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v5, v6), __PAIR__(v5, v6));
      radMenuEntry.helpSystemHashkey = "TAG_DAGGER"/*ELFHASH*/;
      v7 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v7);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x10104660)]
public static void   sub_10104660(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  GameObjectBody v2;
  string v3;
  Dice v4;

  dispIo = evt.GetDispIoDamage();
  v2 = dispIo.attackPacket.GetWeaponUsed();
  if ( v2 !=null)
  {
    if ( GameSystems.Critter.IsCategory(dispIo.attackPacket.victim, MonsterCategory.outsider)
      && (dispIo.attackPacket.victim.GetStat(Stat.alignment) & 4 )!=0)
    {
      v3 = (string )GetDisplayNameForDebug/*0x10021200*/(v2);
      v4 = 2.new Dice(4, 0);
      dispIo.damage.AddDamageDice(v4, DamageType.Acid, 0x79, v3);
    }
    else
    {
      DamagePacketInit/*0x100e0390*/(&dispIo.damage);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x10101840)]
public static void   DaggerOfVenomPoisonOnDamage(in DispatcherCallbackArgs args)
{
  int condArg3;
  GameObjectBody item;
  int condArg4;
  int condArg2;
  DispIoDamage dispIo;

  condArg3 = args.GetConditionArg3();
  item = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, condArg3);
  condArg4 = args.GetConditionArg4();
  condArg2 = args.GetConditionArg2();
  if ( (condArg4 )!=0)
  {
    dispIo = args.GetDispIoDamage();
    if ( item == dispIo.attackPacket.GetWeaponUsed() )
    {
      args.SetConditionArg4(0);
      args.SetConditionArg2(condArg2 - 1);
      dispIo.attackPacket.victim.AddCondition(StatusEffects.Poisoned, 20, 10, 14);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x10104190)]
public static void   FragarachDealingDmg(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoDamage dispIo;
  string v4;
  GameObjectBody ObjHnd;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)4;
  if ( (evt.objHndCaller.GetBaseStat(Stat.alignment) & 8)==0)
  {
    dispIo = evt.GetDispIoDamage();
    ObjHnd = dispIo.attackPacket.GetWeaponUsed();
    if ( v2 == ObjHnd )
    {
      if ( ((dispIo.attackPacket.flags & D20CAF.CRITICAL) == 0)        && (dispIo.attackPacket.victim.GetStat(Stat.alignment) & 8)==0        && (evt.objHndCaller.GetStat(Stat.alignment) == 2 || evt.objHndCaller.GetStat(Stat.alignment) == 6) )
      {
        evt.subDispNode = (SubDispNode *)8;
      }
      v4 = GameSystems.MapObject.GetDisplayName(ObjHnd, evt.objHndCaller);
      dispIo.damage.AddDamageBonus((int)evt.subDispNode, 12, 112, v4);
    }
  }
}


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x101027c0)]
public static void   sub_101027C0(in DispatcherCallbackArgs evt)
{
  int condArg3;

  condArg3 = evt.GetConditionArg3();
  if ( evt.GetDispIoD20ActionTurnBased().action.data1 == condArg3 )
  {
    evt.SetConditionArg2(0);
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100ff5f0)]
public static void   WeaponMasterworkToHitBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  DispIoAttackBonus dispIo;
  GameObjectBody v6;
  string v7;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  dispIo = evt.GetDispIoAttackBonus();
  v6 = dispIo.attackPacket.GetWeaponUsed();
  if ( __PAIR__(v3, v4) == v6 )
  {
    v7 = GameSystems.MapObject.GetDisplayName(v6, evt.objHndCaller);
    dispIo.bonlist.AddBonus(1, 12, 241, v7);
  }
}


[DispTypes(DispatcherType.BucklerAcPenalty)]
[TempleDllLocation(0x10104e40)]
[TemplePlusLocation("condition.cpp:467")]
public static void   BucklerAcPenalty(in DispatcherCallbackArgs evt)
{
  int condArg3;
  DispIoAttackBonus dispIo;
  DispIoAttackBonus evtObj_;
  GameObjectBody weapon;
  ulong v5;
  int v6;

  condArg3 = evt.GetConditionArg3();
  GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  evtObj_ = dispIo;
  weapon = dispIo.attackPacket.GetWeaponUsed();
  v5 = __PAIR__(weapon, HIDWORD(weapon));
  GameSystems.MapObject.GetDisplayName(weapon, evt.objHndCaller);
  if ( (evtObj_.attackPacket.flags & D20CAF.RANGED)==0)
  {
    if ( v5 )
    {
      if ( GameSystems.Item.GetWieldType(evt.objHndCaller, __PAIR__(v5, HIDWORD(v5))) == 2
        || (v6 = evtObj_.attackPacket.dispKey, v6 == 6)
        || v6 == 8 )
      {
        evt.SetConditionArg2(1);
      }
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:467
*/


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x101011f0)]
public static void   AttributeEnhancementBonus_callback(in DispatcherCallbackArgs evt)
{  int condArg1;
  int condArg3;
  GameObjectBody v4;
  string itemName;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg2();
  if ( evt.dispKey - 1 == condArg1 )
  {
    condArg3 = evt.GetConditionArg3();
    v4 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
    evt.dispKey = (Disp_Key)evt.GetDispIoBonusList();
    itemName = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
    (BonusList *)(evt.dispKey + 4).AddBonus(evt.subDispNode, 12, 112, itemName);
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x10101450)]
public static void   WeaponPlayParticleOnHit(in DispatcherCallbackArgs evt, string data)
{
  int condArg3;
  GameObjectBody v2;
  int v3;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  if ( (AttackPacket *)&evt.dispIO[1].GetWeaponUsed() == v2 )
  {
    if ( data !=null)
    {
      v3 = (string /*ELFHASH*/data);
      GameSystems.ParticleSys.CreateAtObj(v3, *(_QWORD *)&evt.dispIO[2]);
    }
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x10102ae0)]
public static void   FailedCopyScrollQuery(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg2;

  dispIo = evt.GetDispIoD20Query();
  condArg2 = evt.GetConditionArg2();
  if ( GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.spellcraft) > condArg2 )
  {
    evt.RemoveThisCondition();
LABEL_3:
    dispIo.return_val = 0;
    return;
  }
  if ( *(_QWORD *)&dispIo.data1 != evt.GetConditionArg1() )
  {
    goto LABEL_3;
  }
  dispIo.return_val = 1;
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x10104a20)]
public static void   sub_10104A20(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  DispIoDamage dispIo;
  string v4;
  GameObjectBody ObjHnd;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  ObjHnd = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  if ( (dispIo.attackPacket.GetWeaponUsed() == null))
  {
    v4 = GameSystems.MapObject.GetDisplayName(ObjHnd, evt.objHndCaller);
    dispIo.damage.AddDamageBonus(condArg1, 0, 112, v4);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x10104910)]
public static void   NecklaceOfDetectionSkillLevel(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int v2;
  GameObjectBody v3;
  int v4;
  string v5;
  int v6;

  condArg3 = evt.GetConditionArg3();
  v2 = condArg3;
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v6 = HIDWORD(v3);
  v4 = v3;
  evt.subDispNode = (SubDispNode *)evt.GetDispIoObjBonus();
  if ( v2 >= 100 )
  {
    v5 = GameSystems.MapObject.GetDisplayName(__PAIR__(v6, v4), evt.objHndCaller);
    (BonusList *)evt.subDispNode.next.AddBonus(5, 0, 112, v5);
  }
}


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x101022f0)]
public static void   BootsOfSpeedTooltip(in DispatcherCallbackArgs evt, int data)
{
  DispIoTooltip dispIo;
  int v2;
  int v3;
  string meslineValue;
int meslineKey;

  if ( (evt.GetConditionArg4() )!=0)
  {
    dispIo = evt.GetDispIoTooltip();
    meslineKey = data;
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    v3 = dispIo.numStrings;
    if ( v3 < 10 )
    {
      dispIo.numStrings = v3 + 1;
      strncpy(dispIo.strings[v3].text, meslineValue, 0x100);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100ffa50)]
public static void   sub_100FFA50(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoDamage dispIo;
  DispIoDamage v4;
  GameObjectBody v5;
  int v6;
  int v7;
  int v8;
  Dice v9;
  string v10;
  GameObjectBody objHnd;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  v4 = dispIo;
  v5 = dispIo.attackPacket.GetWeaponUsed();
  v6 = HIDWORD(v5);
  v7 = v5;
  objHnd = v4.attackPacket.victim;
  v8 = v5.WeaponFlags;
  if ( (int)v2 == v7 && HIDWORD(v2) == v6 || (v8 & 0x400) !=0&& GameSystems.Item.ItemWornAt(evt.objHndCaller, 9) == __PAIR__(v6, v7) )
  {
    if ( (objHnd.GetStat(Stat.alignment) & 8 )!=0)
    {
      v9 = 2.new Dice(6, 0);
      v10 = GameSystems.MapObject.GetDisplayName(__PAIR__(v6, v7), evt.objHndCaller);
      v4.damage.AddDamageDice(v9, DamageType.Unspecified, 0x79, v10);
    }
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100fffd0)]
public static void   sub_100FFFD0(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int v2;
  int v3;
  int v4;
  int v5;
  int v6;
  GameObjectBody parent;

  v6 = 30;
  dispIo = evt.GetDispIoD20Query();
  v2 = dispIo.data2;
  v3 = dispIo.data1;
  GameSystems.Item.GetParent(*(_QWORD *)&dispIo.data1, &parent);
  if ( (parent == null))
  {
    goto LABEL_15;
  }
  if ( parent.IsPC() || parent.IsNPC() )
  {
    v6 = (ulong)DispatcherExtensions.Dispatch40GetMoveSpeedBase(parent, 0);
  }
  if ( GameSystems.Item.ItemWornAt(parent, 5) != __PAIR__(v2, v3)
    || GameSystems.D20.D20Query(parent, D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium)
    || GameSystems.D20.D20Query(parent, D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy)
    || GameSystems.D20.D20Query(parent, D20DispatcherKey.QUE_Critter_Is_Encumbered_Overburdened)
    || (v4 = __PAIR__(v2, v3).GetInt32(obj_f.armor_flags), v5 = GetArmorType/*0x10065bc0*/(v4), v5 <= 0)
    || v5 > 2 )
  {
LABEL_15:
    dispIo.return_val = 100;
  }
  else
  {
    dispIo.return_val = v6 > 20 ? 20 : 15;
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100fff90)]
public static void   sub_100FFF90(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int v2;
  int v3;

  dispIo = evt.GetDispIoD20Query();
  v2 = dispIo.data2;
  v3 = dispIo.data1;
  if ( sub_1009CA00/*0x1009ca00*/(*(_QWORD *)&dispIo.data1, 222) )
  {
    dispIo.return_val = __PAIR__(v2, v3).GetInt32(obj_f.armor_max_dex_bonus);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x10102a70)]
public static void   AddFamiliarSkillBonus(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int condArg3;
  GameObjectBody v3;
  string v4;

  v1 = data;
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetDispIoObjBonus();
  v4 = GameSystems.MapObject.GetDisplayName(v3, evt.objHndCaller);
  (BonusList *)evt.subDispNode.next.AddBonus(v1, 0, 279, v4);
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x10101150)]
[TemplePlusLocation("condition.cpp:431")]
public static void   SkillBonusCallback(in DispatcherCallbackArgs evt, int data)
{  int condArg1;
  int condArg3;
  GameObjectBody v4;
  string v5;
  int condArg2;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  evt.subDispNode = (SubDispNode *)data;
  if ( evt.dispKey - 20 == condArg1 )
  {
    condArg3 = evt.GetConditionArg3();
    v4 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
    evt.dispKey = (Disp_Key)evt.GetDispIoObjBonus();
    v5 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
    *(BonusList **)(evt.dispKey + 8).AddBonus(condArg2, evt.subDispNode, 112, v5);
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:431
*/


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x101047c0)]
public static void   sub_101047C0(in DispatcherCallbackArgs evt)
{  int condArg3;
  GameObjectBody v3;
  DispIoDamage dispIo;
  GameObjectBody v5;
  int v6;
  SubDispNode *v7;
  string v8;/*INLINED:v1=evt.subDispNode*/  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  v5 = dispIo.attackPacket.GetWeaponUsed();
  if ( v3 == v5 )
  {
    v6 = evt.objHndCaller.GetStat(0);
    v7 = (SubDispNode *)D20StatSystem.GetModifierForAbilityScore(v6);
    if ( (int)v7 > (int)evt.subDispNode )
    {
      v7 = evt.subDispNode;
    }
    if ( (int)v7 > 0 )
    {
      v8 = GameSystems.MapObject.GetDisplayName(v5, evt.objHndCaller);
      dispIo.damage.AddDamageBonus((int)v7, 0, 312, v8);
    }
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10101660)]
public static void   StaffOfStrikingRadial(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  GameObjectBody v3;  int v5;
  int v6;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  if ( GameSystems.Item.IsIdentified(v3) && condArg1 >= 1 )
  {
    radMenuEntry = RadialMenuEntry.Create();
    if ( DispatcherExtensions.DispatchToHitBonusBase(evt.objHndCaller, 0) > 0 )
    {/*INLINED:v4=evt.subDispNode.condNode*/      radMenuEntry.maxArg = 3;
      radMenuEntry.minArg = 0;
      radMenuEntry.type = RadialMenuEntryType.Slider;
      radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 1);
      radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
      radMenuEntry.d20ActionType = -1;
      radMenuEntry.d20ActionData1 = 0;
      meslineKey = 5075;
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      radMenuEntry.text = (string )meslineValue;
      radMenuEntry.helpSystemHashkey = "TAG_RADIAL_MENU_STAFF_OF_STRIKING"/*ELFHASH*/;
      v6 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v6);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100ffb40)]
public static void   sub_100FFB40(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoDamage dispIo;
  DispIoDamage v4;
  GameObjectBody v5;
  int v6;
  int v7;
  int v8;
  Dice v9;
  string v10;
  GameObjectBody objHnd;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  v4 = dispIo;
  v5 = dispIo.attackPacket.GetWeaponUsed();
  v6 = HIDWORD(v5);
  v7 = v5;
  objHnd = v4.attackPacket.victim;
  v8 = v5.WeaponFlags;
  if ( (int)v2 == v7 && HIDWORD(v2) == v6 || (v8 & 0x400) !=0&& GameSystems.Item.ItemWornAt(evt.objHndCaller, 9) == __PAIR__(v6, v7) )
  {
    if ( (objHnd.GetStat(Stat.alignment) & 2 )!=0)
    {
      v9 = 2.new Dice(6, 0);
      v10 = GameSystems.MapObject.GetDisplayName(__PAIR__(v6, v7), evt.objHndCaller);
      v4.damage.AddDamageDice(v9, DamageType.Unspecified, 0x79, v10);
    }
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x101002e0)]
public static void   DeflectionAcBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoAttackBonus dispIo;
  string v4;
  D20CAF v5;
  int bonValue;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  dispIo = evt.GetDispIoAttackBonus();
  v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
  v5 = dispIo.attackPacket.flags;
  if ( !(((v5 & D20CAF.TOUCH_ATTACK)!=0)) )
  {
    dispIo.bonlist.AddBonus(bonValue, 11, 148, v4);
  }
}


[DispTypes(DispatcherType.D20Signal, DispatcherType.ConditionAddFromD20StatusInit)]
[TempleDllLocation(0x100ed3e0)]
public static void   sub_100ED3E0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  string v5;
  int v6;
  int v7;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  if ( v2 !=null)
  {
    v5 = (string )evt.GetConditionArg(data1);
    if ( v5 !=null)
    {
      GameSystems.ParticleSys.End(v5);
    }
    v6 = evt.GetConditionArg(data2);
    v7 = GameSystems.ParticleSys.CreateAtObj(v6, __PAIR__(v3, v4));
    evt.SetConditionArg(data1, v7);
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x10104ab0)]
public static void   amuletOfNaturalArmorACBonus(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  GameObjectBody v3;
  int v4;
  int v5;
  string v6;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v4 = HIDWORD(v3);
  v5 = v3;
  evt.subDispNode = (SubDispNode *)evt.GetDispIoAttackBonus();
  v6 = GameSystems.MapObject.GetDisplayName(__PAIR__(v4, v5), evt.objHndCaller);
  (BonusList *)&evt.subDispNode[4].next.AddBonus(condArg1, 9, 112, v6);
}


[DispTypes(DispatcherType.ProjectileDestroyed)]
[TempleDllLocation(0x101013d0)]
public static void   sub_101013D0(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  GameObjectBody v2;
  DispIoAttackBonus dispIo;
  int v4;
  int v5;
  string v6;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  v4 = dispIo.attackPacket.ammoItem;
  v5 = HIDWORD(dispIo.attackPacket.ammoItem);
  if ( __PAIR__(v2, HIDWORD(v2))
    && __PAIR__(v4, v5)
    && __PAIR__(dispIo.attackPacket.weaponUsed, HIDWORD(dispIo.attackPacket.weaponUsed)) == __PAIR__(v2, HIDWORD(v2)) )
  {
    v6 = (string )__PAIR__(v5, v4).GetInt32(obj_f.projectile_part_sys_id);
    if ( v6 !=null)
    {
      GameSystems.ParticleSys.End(v6);
    }
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10100790)]
public static void   ReloadRadial(in DispatcherCallbackArgs evt)
{
  int v1;
  GameObjectBody v2;
  
  int v4;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.RELOAD;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5009;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 203);
  if ( v2 !=null)
  {
    if ( v2.GetWeaponType() == 17 )
    {
    }
    else
    {
    }
    radMenuEntry.helpSystemHashkey = "TAG_WEAPONS_CROSSBOW_LIGHT";
    radMenuEntry.text = (string )meslineValue;
    v4 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Offense);
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v4);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x10104330)]
[TemplePlusLocation("generalfixes.cpp:241")]
public static void   FragarachAnswering(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;

  dispIo = evt.GetDispIoDamage();
  if ( (evt.objHndCaller.GetBaseStat(Stat.alignment) & 8)==0&& (evt.objHndCaller.GetBaseStat(Stat.alignment) & 2 )!=0)
  {
    if ( evt.objHndCaller.GetStat(Stat.alignment) == 2 )
    {
      if ( evt.GetConditionArg1() <= 0 )
      {
        return;
      }
      evt.SetConditionArg1(0);
    }
    *(float *)&evt.dispIO = dispIo.attackPacket.attacker.DistanceToInFeetClamped(evt.objHndCaller);
    if ( evt.objHndCaller.GetReach(D20ActionType.UNSPECIFIED_ATTACK) >= (float)*(float *)&evt.dispIO )
    {
      GameSystems.Anim.Interrupt(evt.objHndCaller, 6, 0);
      GameSystems.D20.Actions.DoAoo(evt.objHndCaller, dispIo.attackPacket.attacker);
      GameSystems.D20.Actions.sequencePerform();
    }
  }
}
/* Orphan comments:
TP Replaced @ generalfixes.cpp:241
*/


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x10101b60)]
public static void   sub_10101B60(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int condArg3;
  DispIoAttackBonus dispIo;
  string v4;
  GameObjectBody ObjHnd;

  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  ObjHnd = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  if ( condArg2 > 0 )
  {
    dispIo = evt.GetDispIoAttackBonus();
    v4 = GameSystems.MapObject.GetDisplayName(ObjHnd, evt.objHndCaller);
    dispIo.bonlist.AddBonus(condArg2, 0, 112, v4);
  }
}


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x10103f40)]
public static void   ElementalGemPerform(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  int v3;
  int v4;
  int v5;
  int v6;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v3 = evt.GetDispIoD20ActionTurnBased().action.data1;
  if ( v3 >> 16 == condArg3 )
  {
    v4 = (ushort)v3 - 1;
    if ( (v4 )!=0)
    {
      v5 = v4 - 1;
      if ( (v5 )!=0)
      {
        if ( v5 == 1 )
        {
          v6 = condArg1 & 0xFFFFFF07 | 7;
        }
        else
        {
          v6 = (int)evt.subDispNode;
        }
      }
      else
      {
        v6 = condArg1 & 0xFFFF07FF | 0x700;
      }
    }
    else
    {
      v6 = condArg1 & 0xFF07FFFF | 0x70000;
    }
    evt.SetConditionArg1(v6);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x101028e0)]
public static void   FamiliarSkillBonus2(in DispatcherCallbackArgs evt)
{  int condArg3;
  GameObjectBody v3;
  int condArg2;
  string v5;/*INLINED:v1=evt.subDispNode*/  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetDispIoObjBonus();
  if ( evt.dispKey - 20 == evt.GetConditionArg1() )
  {
    condArg2 = evt.GetConditionArg2();
    v5 = GameSystems.MapObject.GetDisplayName(v3, evt.objHndCaller);
    (BonusList *)evt.subDispNode.next.AddBonus(condArg2, 0, 278, v5);
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x10101280)]
public static void   AddSavingThrowResistanceBonus(in DispatcherCallbackArgs evt)
{  int condArg1;
  int condArg3;
  GameObjectBody v4;
  string v5;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg2();
  if ( evt.dispKey - 7 == condArg1 )
  {
    condArg3 = evt.GetConditionArg3();
    v4 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
    evt.dispKey = (Disp_Key)evt.GetDispIoSavingThrow();
    v5 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
    (BonusList *)(evt.dispKey + 24).AddBonus(evt.subDispNode, 15, 112, v5);
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x10101540)]
public static void   AddLuckPoisonSaveBonus(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  DispIoSavingThrow dispIo;
  string v4;
  GameObjectBody ObjHnd;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  ObjHnd = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoSavingThrow();
  if ( (dispIo.flags & D20SavingThrowFlag.POISON)!=0)
  {
    v4 = GameSystems.MapObject.GetDisplayName(ObjHnd, evt.objHndCaller);
    dispIo.bonlist.AddBonus(condArg1, 15, 112, v4);
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x101020a0)]
[TemplePlusLocation("ability_fixes.cpp:72")]
public static void   BootsOfSpeedBeginRound(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg4;
  int condArg5;
  int roundsNew;
  
  condArg1 = evt.GetConditionArg1();
  condArg4 = evt.GetConditionArg4();
  condArg5 = evt.GetConditionArg(4);
  if ( (condArg4 )!=0)
  {
    roundsNew = condArg1 - evt.GetDispIoD20Signal().data1;
    if ( roundsNew >= 0 )
    {
      evt.SetConditionArg1(roundsNew);
    }
    else
    {
                              CommonConditionCallbacks.conditionRemoveCallback(in evt);
      GameSystems.ParticleSys.Remove(condArg5);
    }
  }
}
/* Orphan comments:
TP Replaced @ ability_fixes.cpp:72
*/


[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x10101f90)]
public static void   BootsOfSpeedD20Check(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg4;
  TurnBasedStatus *v3;
  DispIoD20ActionTurnBased dispIo;

  condArg3 = evt.GetConditionArg3();
  condArg4 = evt.GetConditionArg4();
  v3 = evt.GetDispIoD20ActionTurnBased().tbStatus;
  dispIo = evt.GetDispIoD20ActionTurnBased();
  if ( dispIo.action.data1 == condArg3 && (condArg4 )==0)
  {
    if ( v3.tbsFlags & 0x20 )
    {
      dispIo.returnVal = 14;
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x101024a0)]
public static void   sub_101024A0(in DispatcherCallbackArgs evt)
{
  int condArg1;
  _ObjCategory v2;
  int condArg3;
  DispIoDamage dispIo;
  DispIoDamage v5;
  GameObjectBody v6;
  string v7;
  int nCategorySubtype/*0x102e83cc*/;

  condArg1 = evt.GetConditionArg1();
  v2 = *((_DWORD *)&dword_102E83C8/*0x102e83c8*/ + 2 * condArg1);
  nCategorySubtype/*0x102e83cc*/ = *(&::nCategorySubtype/*0x102e83cc*/ + 2 * condArg1);
  condArg3 = evt.GetConditionArg3();
  GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  v5 = dispIo;
  v6 = dispIo.attackPacket.GetWeaponUsed();
  v7 = GameSystems.MapObject.GetDisplayName(v6, evt.objHndCaller);
  if ( GameSystems.Critter.IsCategory(v5.attackPacket.victim, v2) )
  {
    if ( GameSystems.Critter.IsCategorySubtype(v5.attackPacket.victim, nCategorySubtype/*0x102e83cc*/) )
    {
      v5.damage.AddDamageDice(2, DamageType.Unspecified, 0x7D, v7);
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x10104bf0)]
public static void   BracersOfArcheryToHitBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  DispIoAttackBonus dispIo;
  DispIoAttackBonus v5;
  GameObjectBody v6;  BonusList *v8;
  string v9;
  int condArg1;
  int v11;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v11 = HIDWORD(v2);
  v3 = v2;
  dispIo = evt.GetDispIoAttackBonus();
  v5 = dispIo;
  v6 = dispIo.attackPacket.GetWeaponUsed();
  if ( v6 !=null)
  {/*INLINED:v7=(SubDispNode *)v6.GetWeaponType()*/    evt.subDispNode = (SubDispNode *)v6.GetWeaponType();
    if ( (int)(SubDispNode *)v6.GetWeaponType() >= 46 && (int)(SubDispNode *)v6.GetWeaponType() <= 49 )
    {
      v8 = &v5.bonlist;
      v9 = GameSystems.MapObject.GetDisplayName(__PAIR__(v11, v3), evt.objHndCaller);
      if ( GameSystems.Feat.IsProficientWithWeaponType(evt.objHndCaller, (uint)evt.subDispNode) )
      {
        v8.AddBonus(condArg1, 34, 112, v9);
      }
      else
      {
        bonCapAddWithDescr/*0x100e6340*/(v8, 37, 0, 0x70, v9);
      }
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x101045b0)]
public static void   HolyWaterOnDamage(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  GameObjectBody v2;
  string v3;
  Dice v4;

  dispIo = evt.GetDispIoDamage();
  v2 = dispIo.attackPacket.GetWeaponUsed();
  if ( v2 !=null)
  {
    if ( GameSystems.Critter.IsCategory(dispIo.attackPacket.victim, MonsterCategory.undead)
      || GameSystems.Critter.IsCategory(dispIo.attackPacket.victim, MonsterCategory.outsider)
      && (dispIo.attackPacket.victim.GetStat(Stat.alignment) & 8 )!=0)
    {
      v3 = (string )GetDisplayNameForDebug/*0x10021200*/(v2);
      v4 = 2.new Dice(4, 0);
      dispIo.damage.AddDamageDice(v4, DamageType.Acid, 0x79, v3);
    }
    else
    {
      DamagePacketInit/*0x100e0390*/(&dispIo.damage);
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x10104700)]
public static void   sub_10104700(in DispatcherCallbackArgs evt)
{  int condArg3;
  GameObjectBody v3;
  DispIoAttackBonus dispIo;
  GameObjectBody v5;
  int v6;
  string v7;/*INLINED:v1=evt.subDispNode*/  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  v5 = dispIo.attackPacket.GetWeaponUsed();
  if ( v3 == v5 )
  {
    v6 = evt.objHndCaller.GetStat(0);
    if ( D20StatSystem.GetModifierForAbilityScore(v6) < (int)evt.subDispNode )
    {
      v7 = GameSystems.MapObject.GetDisplayName(v5, evt.objHndCaller);
      dispIo.bonlist.AddBonus(-2, 0, 313, v7);
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x10101a40)]
public static void   sub_10101A40(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int condArg3;
  GameObjectBody v3;
  DispIoAttackBonus dispIo;
  GameObjectBody v5;
  string v6;

  condArg2 = evt.GetConditionArg2();
  if ( condArg2 > 0 )
  {
    condArg3 = evt.GetConditionArg3();
    v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
    dispIo = evt.GetDispIoAttackBonus();
    v5 = dispIo.attackPacket.GetWeaponUsed();
    if ( v3 == v5 )
    {
      v6 = GameSystems.MapObject.GetDisplayName(v5, evt.objHndCaller);
      dispIo.bonlist.AddBonus(-condArg2, 0, 112, v6);
    }
  }
}


[DispTypes(DispatcherType.NewDay)]
[TempleDllLocation(0x10103fd0)]
public static void   ElementalGemNewdayRest(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  int v3;

  condArg1 = evt.GetConditionArg1();
  v2 = (condArg1 << 8 >> 24) - 1;
  if ( v2 < 0 )
  {
    v2 = 0;
  }
  v3 = (condArg1 << 16 >> 24) - 1;
  if ( v3 < 0 )
  {
    v3 = 0;
  }
  evt.SetConditionArg1(v2 | ((v3 | (v2 << 8)) << 8));
}


[DispTypes(DispatcherType.ProjectileCreated)]
[TempleDllLocation(0x10101310)]
public static void   ProjectileCreatileParticles(in DispatcherCallbackArgs evt, string data)
{
  int condArg3;
  GameObjectBody v2;
  DispIoAttackBonus dispIo;
  int v4;
  int v5;
  int v6;
  int v7;
  int v8;
  int v9;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  v4 = dispIo.attackPacket.ammoItem;
  v5 = dispIo.attackPacket.weaponUsed;
  v6 = HIDWORD(dispIo.attackPacket.weaponUsed);
  v7 = HIDWORD(dispIo.attackPacket.ammoItem);
  if ( dispIo.attackPacket.ammoItem
!=null    && (v5 == (int)v2 && v6 == HIDWORD(v2) || condArg3 == 209 && GameSystems.Item.AmmoMatchesWeapon(__PAIR__(v6, v5), v2))
    && !__PAIR__(v7, v4).GetInt32(obj_f.projectile_part_sys_id) )
  {
    if ( data !=null)
    {
      v8 = (string /*ELFHASH*/data);
      v9 = GameSystems.ParticleSys.CreateAtObj(v8, __PAIR__(v7, v4));
      __PAIR__(v7, v4).SetInt32(obj_f.projectile_part_sys_id, v9);
    }
  }
}


[DispTypes(DispatcherType.ConditionAddFromD20StatusInit)]
[TempleDllLocation(0x10101820)]
public static void   sub_10101820(in DispatcherCallbackArgs evt)
{
  evt.SetConditionArg1((int)evt.subDispNode.condNode);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10100cf0)]
public static void   UseableMagicStaffRadial(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  int v5;
  string v6;
  int index;
  int v8;
  int condArg1;
  int invIdx;
  int parentIdx;
  SpellStoreData spData;
  RadialMenuEntry radMenuEntry;
  int a3;
  int classCode;
  int spellLvl;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  invIdx = condArg3;
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  v2.type;
  v8 = GameSystems.Item.GetItemSpellCharges(__PAIR__(v3, v4));
  if ( GameSystems.Item.IsIdentified(__PAIR__(v3, v4)) )
  {
    if ( v8 > 0 )
    {
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), __PAIR__(v3, v4));
      v5 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
      parentIdx = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v5);
      for ( index = 0; index < condArg1; ++index )
      {
        ObjGetArrayElement/*0x1009e770*/(__PAIR__(v3, v4), obj_f.item_spell_idx, index, &spData);
        if ( GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spData.spellEnum) )
        {
          if ( v8 >= 4 * (spData.spellEnum == WellKnownSpells.RaiseDead) + 1 )
          {
            sub_10079DB0/*0x10079db0*/(spData.spellEnum, evt.objHndCaller, (int)&a3);
            radMenuEntry = RadialMenuEntry.Create();
            radMenuEntry.d20SpellData.SetSpellData(spData.spellEnum, classCode, spellLvl, invIdx, 0);
            radMenuEntry.d20ActionType = D20ActionType.USE_ITEM;
            radMenuEntry.d20ActionData1 = 0;
            radMenuEntry.text = GameSystems.Spell.GetSpellName(spData.spellEnum);
            v6 = GameSystems.Spell.GetSpellHelpTopic(spData.spellEnum);
            radMenuEntry.helpSystemHashkey = v6/*ELFHASH*/;
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
          }
        }
      }
    }
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x10102760)]
public static void   QueryMasterwork(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg3;

  dispIo = evt.GetDispIoD20Query();
  if ( dispIo !=null)
  {
    condArg3 = evt.GetConditionArg3();
    if ( *(_QWORD *)&dispIo.data1 == GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3) )
    {
      dispIo.return_val = 1;
    }
  }
}


[DispTypes(DispatcherType.BeginRound, DispatcherType.ConditionAddFromD20StatusInit)]
[TempleDllLocation(0x10101c90)]
public static void   ElementalResistancePerRoundRefresh(in DispatcherCallbackArgs evt)
{
  int condArg2;

  condArg2 = evt.GetConditionArg2();
  evt.SetConditionArg4(condArg2);
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x10102240)]
public static void   BootsOfSpeed_MovementBonus_Callback(in DispatcherCallbackArgs evt)
{
  DispIoMoveSpeed dispIo;

  if ( (evt.GetConditionArg4() )!=0)
  {
    dispIo = evt.GetDispIoMoveSpeed();
    dispIo.bonlist.AddBonus(30, 12, 174);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100ff690)]
public static void   ItemRemoveEnhancement(in DispatcherCallbackArgs evt)
{
  DispIoD20Signal dispIo;
  
  dispIo = evt.GetDispIoD20Signal();
  if ( *(_QWORD *)&dispIo.data1 == evt.GetConditionArg(4) )
  {
                    CommonConditionCallbacks.conditionRemoveCallback(in evt);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x10102b70)]
public static void   sub_10102B70(in DispatcherCallbackArgs evt)
{
  DispIoD20Signal dispIo;
  int condArg3;
  GameObjectBody v3;
  GameObjectBody v4;
  GameObjectBody v5;
  string v6;
  int v7;
  CHAR v8;
  int v9;
  int v10;
  int v11;
  int v12;
  SubDispNode *v13;
  int v14;
  int v15;
  int v16;
  int v17;
  int v18;
  int v19;
  int condArg1;
  GameObjectBody ObjHnd;
  GameObjectBody v22;
  int arg1;
  int arg2;
  GameObjectBody v25;

  dispIo = evt.GetDispIoD20Signal();
  condArg3 = evt.GetConditionArg3();
  v25 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  condArg1 = evt.GetConditionArg1();
  v3 = GetProtoHandle/*0x1003ad70*/(12019);
  v4 = GetProtoHandle/*0x1003ad70*/(12020);
  ObjHnd = GetProtoHandle/*0x1003ad70*/(12021);
  v22 = GetProtoHandle/*0x1003ad70*/(12022);
  HIDWORD(v5) = dispIo.data2;
  arg1 = dispIo.data1;
  arg2 = *(_QWORD *)&dispIo.data1 >> 32;
  LODWORD(v5) = *(_QWORD *)&dispIo.data1;
  v6 = GameSystems.MapObject.GetDisplayName(*(_QWORD *)&dispIo.data1, v5);
  if ( !strcmp(v6, GameSystems.MapObject.GetDisplayName(v3, v3)) )
  {
    v7 = 3;
    v8 = 24;
  }
  else if ( !strcmp(v6, GameSystems.MapObject.GetDisplayName(v4, v4)) )
  {
    v7 = 4;
    v8 = 16;
  }
  else if ( !strcmp(v6, GameSystems.MapObject.GetDisplayName(ObjHnd, ObjHnd)) )
  {
    v7 = 5;
    v8 = 8;
  }
  else
  {
    if ( strcmp(v6, GameSystems.MapObject.GetDisplayName(v22, v22)) )
    {
      return;
    }
    v7 = 6;
    v8 = 0;
  }
  if ( !((1 << v8) & condArg1) )
  {
    v9 = condArg1 | (1 << v8);
    evt.SetConditionArg1(condArg1 | (1 << v8));
    v10 = GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Elemental_Gem_State, arg1, arg2);
    evt.SetConditionArg(v7, v10);
    if ( v9 > 0x1000000 )
    {
      if ( v9 > 16842752 )
      {
        v18 = v9 - 16842753;
        if ( (v18 )==0)
        {
          v13 = (SubDispNode *)344;
          goto LABEL_44;
        }
        v19 = v18 - 255;
        if ( (v19 )==0)
        {
          v13 = (SubDispNode *)337;
          goto LABEL_44;
        }
        if ( v19 == 1 )
        {
          v13 = (SubDispNode *)336;
          goto LABEL_44;
        }
      }
      else
      {
        if ( v9 == 16842752 )
        {
          v13 = (SubDispNode *)338;
          goto LABEL_44;
        }
        v16 = v9 - 16777217;
        if ( (v16 )==0)
        {
          v13 = (SubDispNode *)345;
          goto LABEL_44;
        }
        v17 = v16 - 255;
        if ( (v17 )==0)
        {
          v13 = (SubDispNode *)342;
          goto LABEL_44;
        }
        if ( v17 == 1 )
        {
          v13 = (SubDispNode *)340;
          goto LABEL_44;
        }
      }
    }
    else
    {
      if ( v9 == 0x1000000 )
      {
        v13 = (SubDispNode *)346;
        goto LABEL_44;
      }
      if ( v9 > 0x10000 )
      {
        v14 = v9 - 65537;
        if ( (v14 )==0)
        {
          v13 = (SubDispNode *)334;
          goto LABEL_44;
        }
        v15 = v14 - 255;
        if ( (v15 )==0)
        {
          v13 = (SubDispNode *)335;
          goto LABEL_44;
        }
        if ( v15 == 1 )
        {
          v13 = (SubDispNode *)333;
          goto LABEL_44;
        }
      }
      else
      {
        if ( v9 == 0x10000 )
        {
          v13 = (SubDispNode *)339;
          goto LABEL_44;
        }
        v11 = v9 - 1;
        if ( (v11 )==0)
        {
          v13 = (SubDispNode *)332;
          goto LABEL_44;
        }
        v12 = v11 - 255;
        if ( (v12 )==0)
        {
          v13 = (SubDispNode *)343;
          goto LABEL_44;
        }
        if ( v12 == 1 )
        {
          v13 = (SubDispNode *)341;
LABEL_44:
          v25.SetInt32(obj_f.item_inv_aid, (int)v13);
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
public static void   UseableItemXTimesPerDayRadialMenu(in DispatcherCallbackArgs evt)
{
  int condArg2;
  
  condArg2 = evt.GetConditionArg2();
  if ( evt.GetConditionArg4() < condArg2 )
  {
                    ItemEffects.UseableItemRadialEntry(in evt);
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10103b90)]
public static void   ElementalGemRadial(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  int v5;
  int v6;
  string v7;
  string v8;
  string v9;
  int parentIdx;
  int condArg1;
  string meslineValue;
int meslineKey;
  int condArg2;
  SpellStoreData spellData;
  RadialMenuEntry radMenuEntry;
  CHAR text;

  condArg3 = evt.GetConditionArg3();
  condArg2 = evt.GetConditionArg2();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  if ( GameSystems.Item.IsIdentified(v2) )
  {
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), __PAIR__(v3, v4));
    v5 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
    parentIdx = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v5);
    condArg1 = evt.GetConditionArg1();
    if ( !BYTE3(condArg1) && GameSystems.Map.GetCurrentMapId() != dword_10290080/*0x10290080*/[3 * condArg2] )
    {
      switch ( condArg2 )
      {
        case 0:
          meslineKey = 5096;
          break;
        case 1:
          meslineKey = 5097;
          break;
        case 2:
          meslineKey = 5098;
          break;
        case 3:
          meslineKey = 5099;
          break;
        default:
          break;
      }
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      radMenuEntry = RadialMenuEntry.Create();
      text = String.Format("{0}", meslineValue);
      RadialMenuEntrySetText/*0x100f00b0*/(ref radMenuEntry, &text);
      radMenuEntry.type = 0;
      radMenuEntry.minArg = 0;
      radMenuEntry.maxArg = 0;
      radMenuEntry.actualArg = 0;
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_FREE;
      radMenuEntry.d20ActionData1 = condArg2;
      radMenuEntry.helpSystemHashkey = "TAG_GOLDEN_SKULL"/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
    }
    if ( !(byte)(condArg1 >> 16) )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v3, v4), obj_f.item_spell_idx, 0, &spellData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellData.spellEnum, spellData.spellClassCode, spellData.spellLevel, condArg3, 0);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 16) | 1;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellData.spellEnum);
      v7 = GameSystems.Spell.GetSpellHelpTopic(spellData.spellEnum);
      radMenuEntry.helpSystemHashkey = v7/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
    }
    if ( !BYTE1(condArg1) )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v3, v4), obj_f.item_spell_idx, 1, &spellData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellData.spellEnum, spellData.spellClassCode, spellData.spellLevel, condArg3, 0);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 16) | 2;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellData.spellEnum);
      v8 = GameSystems.Spell.GetSpellHelpTopic(spellData.spellEnum);
      radMenuEntry.helpSystemHashkey = v8/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
    }
    if ( !(_BYTE)condArg1 )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v3, v4), obj_f.item_spell_idx, 2, &spellData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellData.spellEnum, spellData.spellClassCode, spellData.spellLevel, condArg3, 2);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 16) | 3;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellData.spellEnum);
      v9 = GameSystems.Spell.GetSpellHelpTopic(spellData.spellEnum);
      radMenuEntry.helpSystemHashkey = v9/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100ffe90)]
[TemplePlusLocation("condition.cpp:478")]
public static void   WeaponDamageBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody item;
  int v3;
  int v4;
  DispIoDamage dispIo;
  string v6;

  condArg3 = evt.GetConditionArg3();
  item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(item);
  v4 = item;
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  dispIo = evt.GetDispIoDamage();
  if ( v4 == LODWORD(dispIo.attackPacket.weaponUsed) && v3 == HIDWORD(dispIo.attackPacket.weaponUsed)
    || v4 == LODWORD(dispIo.attackPacket.ammoItem) && v3 == HIDWORD(dispIo.attackPacket.ammoItem) )
  {
    v6 = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), evt.objHndCaller);
    dispIo.damage.AddDamageBonus((int)evt.subDispNode, 12, 147, v6);
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:478
*/


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x101018f0)]
public static void   DaggerOfVenomActivate(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg4;

  condArg1 = evt.GetConditionArg1();
  condArg4 = evt.GetConditionArg4();
  if ( condArg1 == evt.GetDispIoD20ActionTurnBased().action.data1 )
  {
    if ( (condArg4 )!=0)
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
public static void   BootsOfSpeedToHitBonus2(in DispatcherCallbackArgs evt, int data)
{
  DispIoAttackBonus dispIo;

  if ( (evt.GetConditionArg4() )!=0)
  {
    dispIo = evt.GetDispIoAttackBonus();
    dispIo.bonlist.AddBonus(1, 0, 174);
  }
}


[DispTypes(DispatcherType.GetCriticalHitRange)]
[TempleDllLocation(0x100ffd20)]
public static void   WeaponKeenCritHitRange(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody itemHndl;
  DispIoAttackBonus dispIo;
  DispIoAttackBonus evtObj_;
  GameObjectBody weapUsed;
  int weapUsedHi;
  int weapUsedLo;
  int weapFlags;
  string v9;
  int bonValue;

  bonValue = 1;
  condArg3 = evt.GetConditionArg3();
  itemHndl = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  evtObj_ = dispIo;
  weapUsed = dispIo.attackPacket.GetWeaponUsed();
  weapUsedHi = HIDWORD(weapUsed);
  weapUsedLo = weapUsed;
  weapFlags = weapUsed.WeaponFlags;
  if ( (int)itemHndl == weapUsedLo && HIDWORD(itemHndl) == weapUsedHi
    || (weapFlags & 0x400) !=0&& GameSystems.Item.ItemWornAt(evt.objHndCaller, 9) == __PAIR__(weapUsedHi, weapUsedLo) )
  {
    bonValue = __PAIR__(weapUsedHi, weapUsedLo).GetInt32(obj_f.weapon_crit_range);
  }
  v9 = GameSystems.MapObject.GetDisplayName(__PAIR__(weapUsedHi, weapUsedLo), evt.objHndCaller);
  evtObj_.bonlist.AddBonus(bonValue, 0, 246, v9);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10101be0)]
public static void   ActivateDefendingWeaponRadial(in DispatcherCallbackArgs evt)
{
  int condArg1;  int v3;
  int v4;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  condArg1 = evt.GetConditionArg1();/*INLINED:v2=evt.subDispNode.condNode*/  radMenuEntry.maxArg = condArg1;
  radMenuEntry.minArg = 0;
  radMenuEntry.type = RadialMenuEntryType.Slider;
  radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 1);
  radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
  radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_FREE;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5078;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.helpSystemHashkey = "TAG_RADIAL_MENU_ACTIVATE_DEFENDING_WEAPON"/*ELFHASH*/;
  radMenuEntry.text = (string )meslineValue;
  v4 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v4);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x101010b0)]
public static void   RingOfInvisibilityRadial(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  GameObjectBody v3;
  int v4;
  int v5;
  int v6;
  RadialMenuEntry radMenuEntry;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v4 = HIDWORD(v3);
  v5 = v3;
  if ( GameSystems.Item.IsIdentified(v3) )
  {
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_STANDARD;
    radMenuEntry.d20ActionData1 = condArg1;
    radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v4, v5), __PAIR__(v4, v5));
    radMenuEntry.helpSystemHashkey = "TAG_MAGIC_ITEMS"/*ELFHASH*/;
    v6 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v6);
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x10101ad0)]
public static void   sub_10101AD0(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg2;
  DispIoDamage dispIo;
  GameObjectBody v4;
  string v5;
  GameObjectBody v6;

  condArg3 = evt.GetConditionArg3();
  v6 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  condArg2 = evt.GetConditionArg2();
  if ( condArg2 > 0 )
  {
    dispIo = evt.GetDispIoDamage();
    v4 = dispIo.attackPacket.GetWeaponUsed();
    if ( v6 == v4 )
    {
      v5 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
      dispIo.damage.AddDamageBonus(-condArg2, 0, 112, v5);
    }
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x10100470)]
public static void   MasterworkArmorSkillBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  DispIoObjBonus dispIo;
  string v6;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  dispIo = evt.GetDispIoObjBonus();
  v6 = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), evt.objHndCaller);
  dispIo.bonOut.AddBonus(1, 0, 242, v6);
  nullsub_1/*0x100027f0*/();
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x10102370)]
public static void   ArmorShadowSilentMovesSkillBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  DispIoObjBonus dispIo;
  string v6;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  dispIo = evt.GetDispIoObjBonus();
  v6 = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), evt.objHndCaller);
  dispIo.bonOut.AddBonus(5, 34, 112, v6);
  nullsub_1/*0x100027f0*/();
}


[DispTypes(DispatcherType.MaxHP)]
[TempleDllLocation(0x10102a00)]
public static void   sub_10102A00(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int condArg1;
  string v4;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  condArg1 = evt.GetConditionArg1();
  v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
  (BonusList *)&evt.dispIO.data.AddBonus(condArg1, 0, 278, v4);
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x10100250)]
public static void   ArmorEnhancementAcBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoAttackBonus dispIo;
  string v4;
  D20CAF v5;
  int bonValue;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  dispIo = evt.GetDispIoAttackBonus();
  v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
  v5 = dispIo.attackPacket.flags;
  if ( !(((v5 & D20CAF.TOUCH_ATTACK)!=0)) )
  {
    dispIo.bonlist.AddBonus(bonValue, 12, 147, v4);
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100ffc30)]
public static void   sub_100FFC30(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoDamage dispIo;
  DispIoDamage v4;
  GameObjectBody v5;
  int v6;
  int v7;
  int v8;
  Dice v9;
  string v10;
  GameObjectBody objHnd;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  v4 = dispIo;
  v5 = dispIo.attackPacket.GetWeaponUsed();
  v6 = HIDWORD(v5);
  v7 = v5;
  objHnd = v4.attackPacket.victim;
  v8 = v5.WeaponFlags;
  if ( (int)v2 == v7 && HIDWORD(v2) == v6 || (v8 & 0x400) !=0&& GameSystems.Item.ItemWornAt(evt.objHndCaller, 9) == __PAIR__(v6, v7) )
  {
    if ( (objHnd.GetStat(Stat.alignment) & 1 )!=0)
    {
      v9 = 2.new Dice(6, 0);
      v10 = GameSystems.MapObject.GetDisplayName(__PAIR__(v6, v7), evt.objHndCaller);
      v4.damage.AddDamageDice(v9, DamageType.Unspecified, 0x79, v10);
    }
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x101021c0)]
public static void   sub_101021C0(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;

  if ( (evt.GetConditionArg4() )!=0)
  {
    dispIo = evt.GetDispIoAttackBonus();
    dispIo.bonlist.AddBonus(1, 8, 174);
  }
}


[DispTypes(DispatcherType.SkillLevel, DispatcherType.AbilityCheckModifier)]
[TempleDllLocation(0x10100650)]
public static void   sub_10100650(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  int bonValue;
  DispIoObjBonus dispIo;
  string v7;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  dispIo = evt.GetDispIoObjBonus();
  v7 = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), evt.objHndCaller);
  if ( sub_1009CA00/*0x1009ca00*/(__PAIR__(v3, v4), 224) )
  {
    bonValue = GameSystems.D20.GetArmorSkillCheckPenalty(__PAIR__(v3, v4));
    if ( evt.objHndCaller.type != 14 && !GameSystems.Feat.IsProficientWithArmor(evt.objHndCaller, __PAIR__(v3, v4)) )
    {
      dispIo.bonOut.AddBonus(bonValue, 0, 112, v7);
      nullsub_1/*0x100027f0*/();
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x10104da0)]
[TemplePlusLocation("condition.cpp:464")]
public static void   BucklerToHitPenalty(in DispatcherCallbackArgs evt)
{
  int condArg3;
  DispIoAttackBonus dispIo;
  DispIoAttackBonus v3;
  GameObjectBody v4;
  ulong v5;
  int v6;

  condArg3 = evt.GetConditionArg3();
  GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  v3 = dispIo;
  v4 = dispIo.attackPacket.GetWeaponUsed();
  v5 = __PAIR__(v4, HIDWORD(v4));
  GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
  if ( ((v3.attackPacket.flags & D20CAF.RANGED) == 0))
  {
    if ( v5 )
    {
      if ( GameSystems.Item.GetWieldType(evt.objHndCaller, __PAIR__(v5, HIDWORD(v5))) == 2
        || (v6 = v3.attackPacket.dispKey, v6 == 6)
        || v6 == 8 )
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
public static void   AttackPowerTypeAdd(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  int v5;
  DispIoDamage dispIo;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = v2;
  v4 = data;
  v5 = HIDWORD(v2);
  dispIo = evt.GetDispIoDamage();
  if ( v3 == LODWORD(dispIo.attackPacket.weaponUsed) && v5 == HIDWORD(dispIo.attackPacket.weaponUsed)
    || v3 == LODWORD(dispIo.attackPacket.ammoItem) && v5 == HIDWORD(dispIo.attackPacket.ammoItem) )
  {
    AddAttackPowerType/*0x100e0520*/(&dispIo.damage, v4);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x101005b0)]
public static void   SkillLevelArmorPenalty(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  string v5;
  int v6;
  DispIoObjBonus dispIo;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  dispIo = evt.GetDispIoObjBonus();
  v5 = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), evt.objHndCaller);
  if ( sub_1009CA00/*0x1009ca00*/(__PAIR__(v3, v4), 224) )
  {
    v6 = GameSystems.D20.GetArmorSkillCheckPenalty(__PAIR__(v3, v4));
    dispIo.bonOut.AddBonus(v6, 0, 112, v5);
    nullsub_1/*0x100027f0*/();
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x10104020)]
public static void   ElementalGemQueryState(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int condArg1;
  DispIoD20Query dispIo;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  condArg1 = evt.GetConditionArg1();
  dispIo = evt.GetDispIoD20Query();
  if ( dispIo !=null&& (int)v2 == dispIo.data1 && HIDWORD(v2) == dispIo.data2 )
  {
    dispIo.return_val = condArg1;
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x10104530)]
public static void   ThievesToolsMasterworkSkillLevel(in DispatcherCallbackArgs evt)
{
  int condArg3;
  DispIoObjBonus dispIo;

  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIoObjBonus();
  if ( condArg3 == 215 )
  {
    dispIo.bonOut.AddBonus(2, 21, 315);
  }
}


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x10103a00)]
public static void   sub_10103A00(in DispatcherCallbackArgs evt)
{
  int condArg3;
  DispIoD20ActionTurnBased dispIo;
  int v3;
  int v4;
  int v5;
  int v6;
  int v7;
  int v8;
  int v9;
  int v10;

  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIoD20ActionTurnBased();
  v3 = dispIo.action.data1 >> 16;
  v4 = (ushort)dispIo.action.data1;
  if ( v3 == condArg3 )
  {
    if ( v4 == 12 )
    {
      evt.SetConditionArg(7, 1);
    }
    else if ( v4 == 13 )
    {
      evt.SetConditionArg(8, 1);
    }
    else
    {
      v5 = v4 / 3 + 3;
      v6 = v4 % 3;
      if ( (v6 )!=0)
      {
        v7 = v6 - 1;
        if ( (v7 )!=0)
        {
          if ( v7 == 1 )
          {
            v8 = 7;
            v9 = -256;
          }
          else
          {
            v8 = (int)evt.subDispNode;
            v9 = (int)evt.subDispNode;
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
      v10 = evt.GetConditionArg(v5);
      evt.SetConditionArg(v5, v8 | v9 & v10);
    }
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10102ed0)]
public static void   GoldenSkullRadial(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  string v4;
  string v5;
  int condArg4;
  string v7;
  string v8;
  string v9;
  int condArg5;
  string v11;
  string v12;
  string v13;
  int condArg6;
  string v15;
  string v16;
  string v17;
  int condArg7;
  string v19;
  string v20;
  string v21;
  int condArg1;
  int parentIdxa;
  int v24;
  short v25;
  short v26;
  short v27;
  short v28;
  SpellStoreData spellStoreData;
  int v30;
  RadialMenuEntry radMenuEntry;
  int v32;
  int v33;
  int v34;
  int condArg8;
  int condArg9;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v30 = HIDWORD(v2);
  v24 = 0;
  v32 = 0;
  v34 = 0;
  v33 = 0;
  condArg1 = evt.GetConditionArg1();
  condArg8 = evt.GetConditionArg(7);
  condArg9 = evt.GetConditionArg(8);
  if ( (evt.objHndCaller.GetStat(Stat.level_cleric) )!=0&& (evt.objHndCaller.GetStat(Stat.alignment) & 4
)!=0    || (evt.objHndCaller.GetStat(Stat.level_paladin) )!=0&& !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin) )
  {
    return;
  }
  if ( (condArg1 & 0x1000000 )!=0)
  {
    v24 = 1;
  }
  if ( (condArg1 & 0x10000 )!=0)
  {
    v32 = 1;
  }
  if ( (condArg1 & 0x100) !=0)
  {
    v34 = 1;
  }
  if ( (condArg1 & 1 )!=0)
  {
    v33 = 1;
  }
  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v30, (int)v2), __PAIR__(v30, (int)v2));
  v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
  parentIdxa = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v3);
  if ( (condArg8 )==0)
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 12, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xC;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v4 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v4/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
  }
  if ( (condArg9 )==0)
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 13, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xD;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v5 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v5/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
  }
  if ( (v24 )!=0)
  {
    condArg4 = evt.GetConditionArg4();
    v25 = condArg4;
    if ( !(byte)(condArg4 >> 16) )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 0, &spellStoreData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = condArg3 << 16;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
      v7 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
      radMenuEntry.helpSystemHashkey = v7/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
      LOWORD(condArg4) = v25;
    }
    if ( !BYTE1(condArg4) )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 1, &spellStoreData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 16) | 1;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
      v8 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
      radMenuEntry.helpSystemHashkey = v8/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
      LOBYTE(condArg4) = v25;
    }
    if ( !(_BYTE)condArg4 )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 2, &spellStoreData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 2);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 16) | 2;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
      v9 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
      radMenuEntry.helpSystemHashkey = v9/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
    }
    if ( (v32 )==0)
    {
LABEL_35:
      if ( (v34 )==0)
      {
        goto LABEL_42;
      }
      goto LABEL_36;
    }
LABEL_29:
    condArg5 = evt.GetConditionArg(4);
    v26 = condArg5;
    if ( !(byte)(condArg5 >> 16) )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 3, &spellStoreData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 16) | 3;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
      v11 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
      radMenuEntry.helpSystemHashkey = v11/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
      LOWORD(condArg5) = v26;
    }
    if ( !BYTE1(condArg5) )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 4, &spellStoreData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 8) | 4;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
      v12 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
      radMenuEntry.helpSystemHashkey = v12/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
      LOBYTE(condArg5) = v26;
    }
    if ( !(_BYTE)condArg5 )
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 5, &spellStoreData);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 2);
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
      radMenuEntry.d20ActionData1 = (condArg3 << 16) | 5;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
      v13 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
      radMenuEntry.helpSystemHashkey = v13/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
    }
    goto LABEL_35;
  }
  if ( (v32 )!=0)
  {
    goto LABEL_29;
  }
  if ( (v34 )==0)
  {
    if ( (v33 )==0)
    {
      return;
    }
    goto LABEL_43;
  }
LABEL_36:
  condArg6 = evt.GetConditionArg(5);
  v27 = condArg6;
  if ( !(byte)(condArg6 >> 16) )
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 6, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 6;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v15 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v15/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
    LOWORD(condArg6) = v27;
  }
  if ( !BYTE1(condArg6) )
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 7, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 7;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v16 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v16/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
    LOBYTE(condArg6) = v27;
  }
  if ( !(_BYTE)condArg6 )
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 8, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 2);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 8;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v17 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v17/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
  }
LABEL_42:
  if ( (v33 )==0)
  {
    return;
  }
LABEL_43:
  condArg7 = evt.GetConditionArg(6);
  v28 = condArg7;
  if ( !(byte)(condArg7 >> 16) )
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 9, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 9;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v19 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v19/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
    LOWORD(condArg7) = v28;
  }
  if ( !BYTE1(condArg7) )
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 10, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 0);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xA;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v20 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v20/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
    LOBYTE(condArg7) = v28;
  }
  if ( !(_BYTE)condArg7 )
  {
    ObjGetArrayElement/*0x1009e770*/(__PAIR__(v30, (int)v2), obj_f.item_spell_idx, 11, &spellStoreData);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, condArg3, 2);
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = (condArg3 << 16) | 0xB;
    radMenuEntry.text = GameSystems.Spell.GetSpellName(spellStoreData.spellEnum);
    v21 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
    radMenuEntry.helpSystemHashkey = v21/*ELFHASH*/;
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdxa);
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100ff7f0)]
public static void   BurstWeaponCritDice(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  GameObjectBody dicePacked;
  DispIoDamage v3;
  GameObjectBody v4;
  int v5;
  int v6;
  Dice v7;
  int v8;
  string v9;
  DispIoAttackBonus dispIo;

  condArg3 = evt.GetConditionArg3();
  dicePacked = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = evt.GetDispIoDamage();
  v4 = v3.attackPacket.GetWeaponUsed();
  v5 = v4.WeaponFlags;
  if ( dicePacked == v4 || (v5 & 0x400) !=0&& GameSystems.Item.ItemWornAt(evt.objHndCaller, 9) == v4 )
  {
    if ( (v3.attackPacket.flags & D20CAF.CRITICAL)!=0)
    {
      dispIo = new DispIoAttackBonus();
      dispIo.attackPacket.victim = v3.attackPacket.victim;
      dispIo.attackPacket.attacker = evt.objHndCaller;
      dispIo.attackPacket.dispKey = v3.attackPacket.dispKey;
      dispIo.attackPacket.flags = v3.attackPacket.flags;
      dispIo.attackPacket.weaponUsed = v3.attackPacket.weaponUsed;
      dispIo.attackPacket.ammoItem = v3.attackPacket.ammoItem;
      v6 = Dispatch24GetCritExtraDice/*0x1004eaf0*/(evt.objHndCaller, &dispIo);
      DispIoAttackBonusDebug/*0x1004d9f0*/(&dispIo);
      v7 = v6.new Dice(10, 0);
    }
    else
    {
      v7 = 1.new Dice(6, 0);
    }
    v8 = v7;
    v9 = GameSystems.MapObject.GetDisplayName(v4, evt.objHndCaller);
    v3.damage.AddDamageDice(v8, (D20DT)data, 0x79, v9);
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10101d80)]
public static void   KeoghtomsOintmentRadial(in DispatcherCallbackArgs evt)
{  GameObjectBody v2;
  int v3;
  int v4;
  int v5;
  int v6;
  string v7;
  int parentIdx;
  SpellStoreData spellEnumOrg;
  RadialMenuEntry radMenuEntry;/*INLINED:v1=(SubDispNode *)evt.GetConditionArg3()*/  evt.subDispNode = (SubDispNode *)evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, (int)(SubDispNode *)evt.GetConditionArg3());
  v3 = HIDWORD(v2);
  v4 = v2;
  GameSystems.Item.GetItemSpellCharges(v2);
  if ( GameSystems.Item.IsIdentified(__PAIR__(v3, v4)) )
  {
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), __PAIR__(v3, v4));
    v5 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
    parentIdx = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v5);
    v6 = 0;
    do
    {
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v3, v4), obj_f.item_spell_idx, v6, &spellEnumOrg);
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20SpellData.SetSpellData(spellEnumOrg.spellEnum, spellEnumOrg.spellClassCode, spellEnumOrg.spellLevel, (int)evt.subDispNode, 0);
      radMenuEntry.d20ActionType = D20ActionType.USE_ITEM;
      radMenuEntry.d20ActionData1 = 0;
      radMenuEntry.text = GameSystems.Spell.GetSpellName(spellEnumOrg.spellEnum);
      v7 = GameSystems.Spell.GetSpellHelpTopic(spellEnumOrg.spellEnum);
      radMenuEntry.helpSystemHashkey = v7/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, parentIdx);
      ++v6;
    }
    while ( v6 < 3 );
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x10100720)]
public static void   ArmorBonusAcBonusCapValue(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  string v5;
  int v6;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  evt.subDispNode = (SubDispNode *)evt.GetDispIoAttackBonus();
  v5 = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), evt.objHndCaller);
  v6 = Dispatch70MaxDexAcBonus/*0x1004f200*/(__PAIR__(v3, v4));
  bonCapAddWithDescr/*0x100e6340*/((BonusList *)&evt.subDispNode[4].next, 3, v6, 0x70, v5);
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x10104b30)]
public static void   BracersOfArcheryDamageBonus(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int condArg3;
  DispIoDamage dispIo;
  GameObjectBody weapused;
  int weapType;
  string v7;
  Dice v8;
  GameObjectBody itemHandle;

  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  itemHandle = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoDamage();
  weapused = dispIo.attackPacket.GetWeaponUsed();
  if ( weapused !=null)
  {
    weapType = weapused.GetWeaponType();
    if ( weapType >= 46 && weapType <= 49 )
    {
      if ( GameSystems.Feat.IsProficientWithWeaponType(evt.objHndCaller, weapType) )
      {
        v7 = GameSystems.MapObject.GetDisplayName(itemHandle, evt.objHndCaller);
        v8 = 0.new Dice(0, condArg2);
        dispIo.damage.AddDamageDice(v8, DamageType.Piercing, 0x79, v7);
      }
    }
  }
}


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x10104d30)]
public static void   UseableItemXTimesPerDayPerform(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg4;

  condArg3 = evt.GetConditionArg3();
  condArg4 = evt.GetConditionArg4();
  evt.GetConditionArg2();
  if ( evt.GetDispIoD20ActionTurnBased().action.data1 == condArg3 )
  {
    evt.SetConditionArg4(condArg4 + 1);
  }
}


[DispTypes(DispatcherType.SkillLevel, DispatcherType.AbilityCheckModifier)]
[TempleDllLocation(0x10100500)]
public static void   sub_10100500(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoObjBonus dispIo;
  string v4;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  if ( evt.objHndCaller.type != 14 && !GameSystems.Feat.IsProficientWithArmor(evt.objHndCaller, v2) )
  {
    dispIo = evt.GetDispIoObjBonus();
    v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
    dispIo.bonOut.AddBonus(1, 0, 242, v4);
    nullsub_1/*0x100027f0*/();
  }
}


[DispTypes(DispatcherType.WeaponGlowType)]
[TempleDllLocation(0x100edf40)]
public static void   WeaponGlowCb(in DispatcherCallbackArgs evt, int data)
{  DispIoD20Query dispIo;
  int condArg3;
  int argsa;/*INLINED:sdn=evt.subDispNode*/  evt.subDispNode = (SubDispNode *)data;
  dispIo = evt.GetDispIoD20Query();
  if ( (evt.objHndCaller == null)    || (condArg3 = evt.GetConditionArg3(),
        GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3) == *(_QWORD *)&dispIo.data1) )
  {
    if ( dispIo.return_val < argsa )
    {
      dispIo.return_val = argsa;
    }
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x101003f0)]
public static void   ShieldEnhancementAcBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int condArg1;
  string displayName;
  int v5;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  condArg1 = evt.GetConditionArg1();
  displayName = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
  v5 = evt.dispIO[4].ioType;
  if ( (v5 & 0x100)==0)
  {
    (BonusList *)&evt.dispIO[7].AddBonus(condArg1, 33, 147, displayName);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x101044f0)]
public static void   sub_101044F0(in DispatcherCallbackArgs evt)
{
  int condArg3;
  DispIoD20Query dispIo;

  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIoD20Query();
  if ( dispIo !=null)
  {
    if ( condArg3 == 215 )
    {
      dispIo.return_val = 1;
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100ffdf0)]
public static void   WeaponEnhancementToHitBonus(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  GameObjectBody v3;
  DispIoAttackBonus dispIo;
  int v5;
  int v6;
  string v7;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  v5 = dispIo.attackPacket.weaponUsed;
  v6 = HIDWORD(dispIo.attackPacket.weaponUsed);
  if ( v3 == __PAIR__(v6, v5) || v3 == dispIo.attackPacket.ammoItem && GameSystems.Item.AmmoMatchesWeapon(__PAIR__(v6, v5), v3) )
  {
    v7 = GameSystems.MapObject.GetDisplayName(v3, evt.objHndCaller);
    dispIo.bonlist.AddBonus(condArg1, 12, 147, v7);
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x101001d0)]
public static void   ArmorBonusAcBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  DispIoAttackBonus dispIo;
  string v4;
  D20CAF v5;
  int bonValue;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  dispIo = evt.GetDispIoAttackBonus();
  v4 = GameSystems.MapObject.GetDisplayName(v2, evt.objHndCaller);
  v5 = dispIo.attackPacket.flags;
  if ( !(((v5 & D20CAF.TOUCH_ATTACK)!=0)) )
  {
    dispIo.bonlist.AddBonus(bonValue, 28, 124, v4);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x10100fc0)]
public static void   RingOfInvisSequence(in DispatcherCallbackArgs evt)
{
  int condArg1;
  ActionSequence seq;
  int N;
  int i;
  D20Action d20a;
  int v6;
  int v7;
  int v8;
  D20Action v9;

  condArg1 = evt.GetConditionArg1();
  seq = (ActionSequence )evt.GetDispIoD20Signal().data1;
  N = seq.d20ActArrayNum;
  i = 0;
  if ( N > 0 )
  {
    d20a = (D20Action )seq;
    while ( 2 )
    {
      switch ( d20a.d20ActType )
      {
        default:
          ++i;
          ++d20a;
          if ( i < N )
          {
            continue;
          }
          break;
        case 0x40:
          v6 = i;
          v7 = seq.d20ActArray[v6].d20ATarget;
          v8 = seq.d20ActArray[v6].d20APerformer;
          v9 = &seq.d20ActArray[v6];
          if ( v7 == v8 && HIDWORD(v9.d20ATarget) == HIDWORD(v9.d20APerformer) )
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
          GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Magical_Item_Deactivate, condArg1, (ulong)condArg1 >> 32);
          evt.SetConditionArg2(0);
          break;
      }
      break;
    }
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10102650)]
public static void   ActivateDeviceSpellRadial(in DispatcherCallbackArgs evt)
{  int condArg3;
  GameObjectBody v3;
  int condArg1;
  string v5;
  int v6;
  SpellStoreData spellEnumOrg;
  RadialMenuEntry radMenuEntry;/*INLINED:v1=evt.subDispNode*/  evt.subDispNode = (SubDispNode *)evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  if ( evt.subDispNode )
  {
    condArg1 = evt.GetConditionArg1();
    ObjGetArrayElement/*0x1009e770*/(v3, obj_f.item_spell_idx, condArg1, &spellEnumOrg);
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = condArg3;
    radMenuEntry.d20SpellData.SetSpellData(spellEnumOrg.spellEnum, spellEnumOrg.spellClassCode, spellEnumOrg.spellLevel, condArg3, 0);
    radMenuEntry.text = GameSystems.MapObject.GetDisplayName(v3, v3);
    v5 = GameSystems.Spell.GetSpellHelpTopic(spellEnumOrg.spellEnum);
    radMenuEntry.helpSystemHashkey = v5/*ELFHASH*/;
    v6 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v6);
  }
}


[DispTypes(DispatcherType.ConditionAddFromD20StatusInit)]
[TempleDllLocation(0x10100ec0)]
public static void   RingOfInvisibilityStatusD20StatusInit(in DispatcherCallbackArgs evt)
{
  evt.SetConditionArg1((int)evt.subDispNode.condNode);
  evt.SetConditionArg2(0);
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x10102550)]
public static void   WeaponDisruptionOnDamage(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  GameObjectBody v4;
  
  int v6;
  string meslineValue;
int meslineKey;
  int v8;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = v2;
  LOBYTE(v2) = evt.dispIO[4].ioType;
  v8 = HIDWORD(v2);
  if ( !(v2 & 4)
    && (AttackPacket *)&evt.dispIO[1].GetWeaponUsed() == __PAIR__(v8, v3)
    && GameSystems.Critter.IsCategory(*(_QWORD *)&evt.dispIO[2], MonsterCategory.undead)
    && !GameSystems.D20.Combat.SavingThrow(*(_QWORD *)&evt.dispIO[2], evt.objHndCaller, 14, SavingThrowType.Will, 0) )
  {
    GameSystems.D20.Combat.Kill(*(_QWORD *)&evt.dispIO[2], evt.objHndCaller);
    v4 = (GameObjectBody)evt.dispIO[2];
    GameSystems.ParticleSys.CreateAtObj("sp-Destroy Undead", v4);
    meslineKey = 7000;
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    GameSystems.RollHistory.CreateFromFreeText((string )meslineValue);
    GameSystems.RollHistory.CreateFromFreeText("\n");
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100efc80)]
public static void   sub_100EFC80(in DispatcherCallbackArgs evt, int data)
{
  evt.SetConditionArg(data, 0xDEADBEEF);
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x10104290)]
public static void   FragarachToHitBonus(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  DispIoAttackBonus dispIo;
  GameObjectBody v6;
  string v7;
  BonusList *v8;

  if ( (evt.objHndCaller.GetBaseStat(Stat.alignment) & 8)==0)
  {
    condArg3 = evt.GetConditionArg3();
    v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
    v3 = HIDWORD(v2);
    v4 = v2;
    dispIo = evt.GetDispIoAttackBonus();
    v6 = dispIo.attackPacket.GetWeaponUsed();
    if ( __PAIR__(v3, v4) == v6 )
    {
      dispIo.attackPacket.flags |= D20CAF.ALWAYS_HIT;
      v7 = GameSystems.MapObject.GetDisplayName(v6, evt.objHndCaller);
      v8 = &dispIo.bonlist;
      v8.AddBonus(4, 12, 112, v7);
      v8.zeroBonusSetMeslineNum(308);
    }
  }
}


[DispTypes(DispatcherType.GetMoveSpeed)]
[TempleDllLocation(0x10102280)]
public static void   BootsOfSpeedGetMoveSpeed(in DispatcherCallbackArgs evt)
{
  int v1;
  BonusList *v2;
  int v3;
  int v4;
  int *v5;

  v1 = 0;
  if ( (evt.GetConditionArg4() )!=0)
  {
    v2 = evt.GetDispIoMoveSpeed().bonlist;
    v3 = v2.bonCount;
    v4 = 0;
    if ( v3 > 0 )
    {
      v5 = &v2.bonusEntries[0].bonType;
      while ( *v5 != 1 )
      {
        ++v4;
        v5 += 4;
        if ( v4 >= v3 )
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
public static void   ItemElementalResistanceDR(in DispatcherCallbackArgs evt)
{  int condArg3;
  GameObjectBody itemHandle;
  int condArg2;
  string itemName;/*INLINED:sdn=evt.subDispNode*/  condArg3 = evt.GetConditionArg3();
  itemHandle = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  evt.dispIO = (DispIOGeneral *)evt.GetDispIoDamage();
  itemName = GameSystems.MapObject.GetDisplayName(itemHandle, evt.objHndCaller);
  (DamagePacket )&evt.dispIO[7].AddDR(condArg2, (D20DT)evt.subDispNode, 0x79, itemName);
}


[DispTypes(DispatcherType.GetMoveSpeed)]
[TempleDllLocation(0x10100130)]
public static void   ArmorBonusMovementSthg_Callback(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  DispIoMoveSpeed dispIo;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = GameSystems.D20.D20QueryItem(v2, D20DispatcherKey.QUE_Armor_Get_Max_Speed);
  if ( v3 != 100 && evt.objHndCaller.GetStat(Stat.race) != 1 )
  {
    dispIo = evt.GetDispIoMoveSpeed();
    if ( dispIo.bonlist.bonusEntries[0].bonValue >= 30 )
    {
      if ( v3 < 30 )
      {
        dispIo.factor = dispIo.factor * 0.66F;
      }
    }
    else if ( v3 < 20 )
    {
      dispIo.factor = dispIo.factor * 0.75F;
    }
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10101ee0)]
public static void   BootsOfSpeedRadial(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int condArg1;
  int v4;
  RadialMenuEntry radMenuEntry;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Item.IsIdentified(v2) )
  {
    if ( condArg1 >= 1 )
    {
      radMenuEntry = RadialMenuEntry.Create();
      radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_FREE;
      radMenuEntry.d20ActionData1 = condArg3;
      radMenuEntry.text = GameSystems.MapObject.GetDisplayName(v2, v2);
      radMenuEntry.helpSystemHashkey = "TAG_MAGIC_ITEMS"/*ELFHASH*/;
      v4 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v4);
    }
  }
}


[DispTypes(DispatcherType.GetCriticalHitExtraDice)]
[TempleDllLocation(0x10104fa0)]
public static void   RodOfSmiting_CritHit(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg1;
  GameObjectBody v3;
  DispIoAttackBonus dispIo;
  GameObjectBody v5;
  int v6;
  CHAR v7;

  condArg3 = evt.GetConditionArg3();
  condArg1 = evt.GetConditionArg1();
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  v5 = dispIo.attackPacket.GetWeaponUsed();
  if ( condArg1 > 0 && v5 == v3 )
  {
    v6 = GetDisplayNameForDebug/*0x10021200*/(v3);
    v7 = String.Format("{0}", v6);
    dispIo.bonlist.AddBonus(2, 0, 112, &v7);
    evt.SetConditionArg1(condArg1 - 1);
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x101023f0)]
public static void   sub_101023F0(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  DispIoAttackBonus dispIo;
  DispIoAttackBonus v4;
  GameObjectBody v5;
  string v6;
  _ObjCategory v7;
  int v8;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dispIo = evt.GetDispIoAttackBonus();
  v4 = dispIo;
  v5 = dispIo.attackPacket.GetWeaponUsed();
  v6 = GameSystems.MapObject.GetDisplayName(v5, evt.objHndCaller);
  v7 = *((_DWORD *)&dword_102E83C8/*0x102e83c8*/ + 2 * condArg1);
  v8 = *(&nCategorySubtype/*0x102e83cc*/ + 2 * condArg1);
  if ( GameSystems.Critter.IsCategory(v4.attackPacket.victim, v7) )
  {
    if ( GameSystems.Critter.IsCategorySubtype(v4.attackPacket.victim, v8) )
    {
      v4.bonlist.AddBonus(2, 0, 263, v6);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100ff730)]
public static void   sub_100FF730(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  GameObjectBody v2;
  DispIoDamage dispIo;
  DispIoDamage v4;
  GameObjectBody v5;
  int v6;
  int v7;
  int v8;
  string v9;
  Dice dicePacked;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  dicePacked = 1.new Dice(6, 0);
  dispIo = evt.GetDispIoDamage();
  v4 = dispIo;
  v5 = dispIo.attackPacket.GetWeaponUsed();
  v6 = HIDWORD(v5);
  v7 = v5;
  v8 = v5.WeaponFlags;
  if ( (int)v2 == v7 && HIDWORD(v2) == v6
    || (v8 & 0x400) !=0&& LODWORD(v4.attackPacket.ammoItem) == v7 && HIDWORD(v4.attackPacket.ammoItem) == v6 )
  {
    v9 = GameSystems.MapObject.GetDisplayName(__PAIR__(v6, v7), evt.objHndCaller);
    v4.damage.AddDamageDice(dicePacked, (D20DT)data, 0x79, v9);
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x10101760)]
public static void   sub_10101760(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int condArg1;
  int condArg2;
  DispIoDamage dispIo;
  GameObjectBody v6;
  string v7;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  evt.SetConditionArg1(condArg1 - condArg2);
  evt.SetConditionArg2(0);
  dispIo = evt.GetDispIoDamage();
  v6 = dispIo.attackPacket.GetWeaponUsed();
  if ( v2 == v6 )
  {
    v7 = GameSystems.MapObject.GetDisplayName(v6, evt.objHndCaller);
    dispIo.damage.AddDamageBonus(3 * condArg2, 0, 112, v7);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x10101cc0)]
public static void   ElementalResistancePerRoundTakingDamage(in DispatcherCallbackArgs args)
{  int condArg1;
  D20DT v3;
  int condArg4;
  DispIoDamage dispIo;
  DamagePacket damPkt;
  int v7;
  int v8;
  int v9;
  GameObjectBody v10;
  string v11;
  D20DT damType;/*INLINED:v1=args.subDispNode*/  condArg1 = args.GetConditionArg1();
  v3 = condArg1;
  damType = condArg1;
  args.subDispNode = (SubDispNode *)args.GetConditionArg3();
  condArg4 = args.GetConditionArg4();
  dispIo = args.GetDispIoDamage();
  damPkt = &dispIo.damage;
  v7 = GetDamageTypeOverallDamage/*0x100e1210*/(&dispIo.damage, v3);
  if ( v7 > 0 && (condArg4 )!=0&& condArg4 >= 0 )
  {
    if ( condArg4 < v7 )
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
    v10 = GameSystems.Item.GetItemAtInvIdx(args.objHndCaller, (int)args.subDispNode);
    v11 = GameSystems.MapObject.GetDisplayName(v10, args.objHndCaller);
    damPkt.AddDR(v8, damType, 0x7C, v11);
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x10100840)]
[TemplePlusLocation("condition.cpp:455")]
public static void   UseableItemRadialEntry(in DispatcherCallbackArgs evt)
{
  int condArg3;
  GameObjectBody v2;
  int v3;
  int v4;
  int v5;
  int condArg1;
  int v7;
  int v8;
  string v9;
  int v10;
  int v11;
  int v12;
  int v13;
  int v14;
  int v15;
  int v16;
  int invIdx;
  CHAR v18;
  SpellStoreData spellStoreData;
  RadialMenuEntry radMenuEntry;

  condArg3 = evt.GetConditionArg3();
  invIdx = condArg3;
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  v3 = HIDWORD(v2);
  v4 = v2;
  v14 = v2.type;
  v15 = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.use_magic_device);
  if ( v14 == ObjectType.food || GameSystems.Item.IsIdentified(__PAIR__(v3, v4)) )
  {
    v5 = GameSystems.Item.GetItemSpellCharges(__PAIR__(v3, v4));
    if ( v5 > 0 || v5 == -1 )
    {
      v18 = __PAIR__(v3, v4).GetItemFlags();
      condArg1 = evt.GetConditionArg1();
      ObjGetArrayElement/*0x1009e770*/(__PAIR__(v3, v4), obj_f.item_spell_idx, condArg1, &spellStoreData);
      v7 = v14;
      if ( v14 == ObjectType.scroll || v18 & ItemFlag.NEEDS_SPELL && (v14 == ObjectType.generic || v14 == ObjectType.weapon) )
      {
        if ( !GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spellStoreData.spellEnum) && (v15 )==0)
        {
          return;
        }
        v7 = v14;
      }
      if ( v7 == ObjectType.scroll && !GameSystems.Spell.CheckAbilityScoreReqForSpell(evt.objHndCaller, spellStoreData.spellEnum, -1) && (v15 )==0)
      {
        return;
      }
      radMenuEntry = RadialMenuEntry.Create();
      if ( v14 != ObjectType.food || (v8 = GameSystems.Item.IsMagical(__PAIR__(v3, v4)), radMenuEntry.d20ActionType = D20ActionType.USE_POTION, (v8)==0) )
      {
        radMenuEntry.d20ActionType = D20ActionType.USE_ITEM;
      }
      radMenuEntry.d20ActionData1 = invIdx;
      radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, invIdx, 0);
      radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), evt.objHndCaller);
      if ( v14 == 9 )
      {
        v16 = 2;
      }
      else if ( v14 == 8 )
      {
        v16 = 1;
      }
      else
      {
        v16 = evt.GetConditionArg2() != 3 ? 0 : 3;
      }
      v9 = GameSystems.Spell.GetSpellHelpTopic(spellStoreData.spellEnum);
      radMenuEntry.helpSystemHashkey = v9/*ELFHASH*/;
      if ( v16 != 1 )
      {
        if ( v16 == 2 )
        {
          v11 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Scrolls);
          GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v11);
        }
        else
        {
          if ( v16 != 3 )
          {
            v12 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Items);
LABEL_30:
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v12);
            goto LABEL_31;
          }
          v10 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Wands);
          GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v10);
        }
LABEL_31:
        if ( v14 == 9
          && evt.objHndCaller.GetStat(Stat.level_wizard) >= 1
          && GameSystems.Spell.HashMatchingClassForSpell(evt.objHndCaller, spellStoreData.spellEnum)
          && SLOBYTE(spellStoreData.spellClassCode) < 0
          && IsArcaneClass/*0x10076170*/(spellStoreData.spellClassCode & 0x7F)
          && !GameSystems.Spell.SpellKnownQueryGetData(evt.objHndCaller, spellStoreData.spellEnum, 0, 0, 0) )
        {
          radMenuEntry = RadialMenuEntry.Create();
          radMenuEntry.text = GameSystems.MapObject.GetDisplayName(__PAIR__(v3, v4), __PAIR__(v3, v4));
          radMenuEntry.d20ActionType = D20ActionType.COPY_SCROLL;
          radMenuEntry.d20ActionData1 = __PAIR__(v3, v4).GetItemInventoryLocation();
          radMenuEntry.d20SpellData.SetSpellData(spellStoreData.spellEnum, spellStoreData.spellClassCode, spellStoreData.spellLevel, invIdx, 0);
          v13 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.CopyScroll);
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
public static void   WeaponHasEnhancementBonus(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  dispIo.return_val = 1;
  *(_QWORD *)&dispIo.data1 = evt.GetConditionArg1();
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x101014c0)]
public static void   BurstDamageTargetParticles(in DispatcherCallbackArgs evt, string data)
{
  int condArg3;
  GameObjectBody v2;
  int v3;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, condArg3);
  if ( (AttackPacket *)&evt.dispIO[1].GetWeaponUsed() == v2 && data !=null)
  {
    v3 = (string /*ELFHASH*/data);
    if ( evt.dispIO[4].ioType & D20CAF.CRITICAL )
    {
      GameSystems.ParticleSys.CreateAtObj(v3, *(_QWORD *)&evt.dispIO[2]);
    }
  }
}


[DispTypes(DispatcherType.GetBonusAttacks)]
[TempleDllLocation(0x10102190)]
[TemplePlusLocation("spell_condition.cpp:258")]
public static void   BootsOfSpeedBonusAttack(in DispatcherCallbackArgs evt)
{
  DispIoD20ActionTurnBased dispIo;

  if ( (evt.GetConditionArg4() )!=0)
  {
    dispIo = evt.GetDispIoD20ActionTurnBased();
    ++dispIo.returnVal;
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:258
*/

}
}
/*

GetPackedDiceNumDice @ 0x10038c30 = 1
Dispatch24GetCritExtraDice @ 0x1004eaf0 = 1
GetArmorType @ 0x10065bc0 = 1
GetDamageTypeOverallDamage @ 0x100e1210 = 1
dword_10290088 @ 0x10290088 = 1
Dispatch70MaxDexAcBonus @ 0x1004f200 = 1
IsArcaneClass @ 0x10076170 = 1
RadialMenuEntrySetText @ 0x100f00b0 = 1
AddAttackPowerType @ 0x100e0520 = 1
GetPackedDiceBonus @ 0x10038c90 = 1
DiceRoller @ 0x10038b60 = 1
dword_10290084 @ 0x10290084 = 1
sub_10079DB0 @ 0x10079db0 = 1
DispIoAttackBonusDebug @ 0x1004d9f0 = 1
GetPackedDiceType @ 0x10038c40 = 1
DamagePacketInit @ 0x100e0390 = 2
CondNodeGetArgPtr @ 0x100e1af0 = 2
bonCapAddWithDescr @ 0x100e6340 = 2
dword_102E83C8 @ 0x102e83c8 = 2
GetDisplayNameForDebug @ 0x10021200 = 3
sub_1009CA00 @ 0x1009ca00 = 3
dword_10290080 @ 0x10290080 = 3
GetProtoHandle @ 0x1003ad70 = 4
nullsub_1 @ 0x100027f0 = 5
nCategorySubtype @ 0x102e83cc = 5
ObjGetArrayElement @ 0x1009e770 = 22
*/