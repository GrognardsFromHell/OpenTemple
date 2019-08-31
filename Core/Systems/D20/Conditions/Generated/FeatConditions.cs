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

public static class FeatConditions {

private static readonly ILogger Logger = new ConsoleLogger();
[TempleDllLocation(0x102aad48)]
  public static readonly ConditionSpec NatureSense = ConditionSpec.Create("Nature Sense", 1)
.Prevents(NimbleFingers)
.AddSkillLevelHandler(SkillId.wilderness_lore, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.knowledge_nature, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102aae18)]
  public static readonly ConditionSpec CraftStaff = ConditionSpec.Create("Craft Staff", 0)
.AddHandler(DispatcherType.RadialMenuEntry, CraftStaffRadialMenu)
                    .Build();


[TempleDllLocation(0x102aade8)]
  public static readonly ConditionSpec ForgeRing = ConditionSpec.Create("Forge Ring", 0)
.AddHandler(DispatcherType.RadialMenuEntry, ForgeRingRadialMenu)
                    .Build();


  // TODO: This condition is actually overwritten by the condition found in StatusEffects,
  // but still referenced directly by the feat->condition mapping
[TempleDllLocation(0x102aae80)]
  public static readonly ConditionSpec SpellResistance = ConditionSpec.Create("Spell Resistance", 3)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.SpellResistanceDebug)
.AddHandler(DispatcherType.ConditionAdd, sub_100F9430)
.AddHandler(DispatcherType.SpellResistanceMod, CommonConditionCallbacks.SpellResistanceMod_Callback, 5048)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance, CommonConditionCallbacks.SpellResistanceQuery)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipSpellResistanceCallback, 5048)
                    .Build();


[TempleDllLocation(0x102ed418)]
  public static readonly ConditionSpec VenomImmunityDruid = ConditionSpec.Create("Venom_Immunity_Druid", 1)
.Prevents(VenomImmunityDruid)
.AddHandler(DispatcherType.ConditionAddPre, sub_100F9150, StatusEffects.Poisoned)
                    .Build();


[TempleDllLocation(0x102ec918)]
  public static readonly ConditionSpec Alertness = ConditionSpec.Create("Alertness", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.listen, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.spot, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102eca28)]
  public static readonly ConditionSpec Cleave = ConditionSpec.Create("Cleave", 2)
.SetUnique()
.RemovedBy(GreatCleave)
.AddSignalHandler(D20DispatcherKey.SIG_Dropped_Enemy, CleaveDroppedEnemy)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
                    .Build();


[TempleDllLocation(0x102eca98)]
  public static readonly ConditionSpec DeflectArrows = ConditionSpec.Create("Deflect_Arrows", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.DeflectArrows, DeflectArrowsCallback)
                    .Build();


[TempleDllLocation(0x102ecb08)]
  public static readonly ConditionSpec Dodge = ConditionSpec.Create("Dodge", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, sub_100F7C90)
.AddHandler(DispatcherType.BeginRound, sub_100F7C90)
.AddHandler(DispatcherType.GetAC, Dodge_ACBonus_Callback)
                    .Build();


[TempleDllLocation(0x102ecb78)]
  public static readonly ConditionSpec FeatExpertise = ConditionSpec.Create("Feat Expertise", 2)
.SetUnique()
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 0)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, CombatExpertise_RadialMenuEntry_Callback)
.AddHandler(DispatcherType.ToHitBonus2, CombatExpertiseToHitPenalty)
.AddHandler(DispatcherType.GetAC, CombatExpertiseAcBonus)
.AddSignalHandler(D20DispatcherKey.SIG_Attack_Made, TacticalOptionAbusePrevention)
.AddSignalHandler(D20DispatcherKey.SIG_SetExpertise, CombatExpertiseSet)
                    .Build();


[TempleDllLocation(0x102ec9b8)]
  public static readonly ConditionSpec GreatCleave = ConditionSpec.Create("Great_Cleave", 2)
.Prevents(Cleave)
.SetUnique()
.AddSignalHandler(D20DispatcherKey.SIG_Dropped_Enemy, GreatCleaveDroppedEnemy)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
                    .Build();


[TempleDllLocation(0x102ecce0)]
  public static readonly ConditionSpec GreatFortitude = ConditionSpec.Create("Great_Fortitude", 1)
.SetUnique()
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, FeatSaveBonus, 2)
                    .Build();


[TempleDllLocation(0x102ecd28)]
  public static readonly ConditionSpec ImprovedCritical = ConditionSpec.Create("Improved_Critical", 2)
.SetUnique()
.AddHandler(DispatcherType.GetCriticalHitRange, ImprovedCriticalGetCritThreatRange)
                    .Build();


[TempleDllLocation(0x102ecd70)]
  public static readonly ConditionSpec ImprovedInitiative = ConditionSpec.Create("Improved_Initiative", 2)
.SetUnique()
.AddHandler(DispatcherType.InitiativeMod, ImprovedInitiativeCallback)
                    .Build();


[TempleDllLocation(0x102ece10)]
  public static readonly ConditionSpec ImprovedTwoWeapon = ConditionSpec.Create("Improved_Two_Weapon", 2)
.SetUnique()
.Prevents(ImprovedTwoWeaponRanger)
.AddHandler(DispatcherType.GetNumAttacksBase, ImprovedTWF)
                    .Build();


[TempleDllLocation(0x102ecdb8)]
  public static readonly ConditionSpec ImprovedTwoWeaponRanger = ConditionSpec.Create("Improved_Two_Weapon_Ranger", 2)
.Prevents(ImprovedTwoWeapon)
.RemovedBy(ImprovedTwoWeaponRanger)
.AddHandler<SubDispatcherCallback>(DispatcherType.GetNumAttacksBase, ArmorLightOnly, ImprovedTWF/*0x100fd1c0*/)
                    .Build();


[TempleDllLocation(0x102eceb0)]
  public static readonly ConditionSpec IronWill = ConditionSpec.Create("Iron_Will", 1)
.SetUnique()
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, FeatSaveBonus, 2)
                    .Build();


[TempleDllLocation(0x102ecef8)]
  public static readonly ConditionSpec LightingReflexes = ConditionSpec.Create("Lighting_Reflexes", 1)
.SetUnique()
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, FeatSaveBonus, 2)
                    .Build();


[TempleDllLocation(0x102ecf40)]
  public static readonly ConditionSpec FeatMobility = ConditionSpec.Create("Feat_Mobility", 1)
.SetUnique()
.AddHandler(DispatcherType.GetAC, MobilityAcBonus)
                    .Build();


[TempleDllLocation(0x102ecf88)]
  public static readonly ConditionSpec PointBlankShot = ConditionSpec.Create("Point_Blank_Shot", 1)
.SetUnique()
.AddHandler(DispatcherType.ToHitBonus2, PointBlankShotToHitBonus)
.AddHandler(DispatcherType.DealingDamage, PointBlankShotDamage)
                    .Build();


[TempleDllLocation(0x102ecfe0)]
  public static readonly ConditionSpec PowerAttack = ConditionSpec.Create("Power Attack", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, PowerAttackRadialMenu)
.AddHandler(DispatcherType.ToHitBonus2, PowerAttackToHitPenalty)
.AddHandler(DispatcherType.DealingDamage, PowerAttackDamageBonus)
.AddSignalHandler(D20DispatcherKey.SIG_SetPowerAttack, PowerAttackSetViaSignal)
                    .Build();


[TempleDllLocation(0x102ed074)]
  public static readonly ConditionSpec QuickDraw = ConditionSpec.Create("Quick_Draw", 1)
.SetUnique()
                    .Build();


[TempleDllLocation(0x102ed0a8)]
  public static readonly ConditionSpec RapidShot = ConditionSpec.Create("Rapid_Shot", 1)
.SetUnique()
.Prevents(RapidShotRanger)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, RapidShotRadialMenu)
.AddHandler(DispatcherType.ToHitBonus2, RapidShotMallus)
.AddHandler(DispatcherType.GetBonusAttacks, RapidShotNumAttacksPerTurn)
                    .Build();


[TempleDllLocation(0x102ed140)]
  public static readonly ConditionSpec RapidShotRanger = ConditionSpec.Create("Rapid_Shot_Ranger", 1)
.SetUnique()
.RemovedBy(RapidShot)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, RapidShotRadialMenu)
.AddHandler<SubDispatcherCallback>(DispatcherType.ToHitBonus2, ArmorLightOnly, RapidShotMallus/*0x100fa8d0*/)
.AddHandler<SubDispatcherCallback>(DispatcherType.GetBonusAttacks, ArmorLightOnly, RapidShotNumAttacksPerTurn/*0x100fa920*/)
                    .Build();


[TempleDllLocation(0x102ed1d4)]
  public static readonly ConditionSpec Run = ConditionSpec.Create("Run", 1)
.SetUnique()
                    .Build();


[TempleDllLocation(0x102ed208)]
  public static readonly ConditionSpec SkillFocus = ConditionSpec.Create("Skill_Focus", 1)
.AddHandler(DispatcherType.ConditionAddPre, CommonConditionCallbacks.CondPreventSameArg, (ConditionSpec) null)
.AddHandler(DispatcherType.ConditionAddPre, sub_100FAC30, SkillFocus)
.AddHandler(DispatcherType.ConditionAdd, sub_100FAC80)
.AddHandler(DispatcherType.SkillLevel, SkillFocusSkillLevelCallback)
                    .Build();


[TempleDllLocation(0x102ee4b4)]
  public static readonly ConditionSpec SpellFocus = ConditionSpec.Create("Spell Focus", 1)
.AddHandler(DispatcherType.SpellDcMod, SpellDcMod_SpellFocus_Callback)
                    .Build();


[TempleDllLocation(0x102ed2c0)]
  public static readonly ConditionSpec featstunningfist = ConditionSpec.Create("feat_stunning_fist", 2)
.Prevents(featstunningfist)
.AddHandler(DispatcherType.ConditionAdd, StunningFistResetArg)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, StunningFistResetArg)
.AddHandler(DispatcherType.RadialMenuEntry, StunningFistRadialMenu)
.AddHandler(DispatcherType.D20ActionPerform, D20DispatcherKey.D20A_LAY_ON_HANDS_USE, sub_100F9910)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_LAY_ON_HANDS_USE, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_LAY_ON_HANDS_USE, sub_100F99B0)
                    .Build();


[TempleDllLocation(0x102ed3c0)]
  public static readonly ConditionSpec TwoWeapon = ConditionSpec.Create("Two_Weapon", 1)
.SetUnique()
.Prevents(TwoWeaponRanger)
.AddHandler(DispatcherType.ToHitBonus2, TwoWeaponFightingBonus)
                    .Build();


[TempleDllLocation(0x102ed490)]
  public static readonly ConditionSpec WeaponFinesse = ConditionSpec.Create("Weapon_Finesse", 2)
.SetUniqueWithKeyArg1()
.AddHandler(DispatcherType.ToHitBonus2, WeaponFinesseToHitBonus)
                    .Build();


[TempleDllLocation(0x102ed4d8)]
  public static readonly ConditionSpec WeaponFocus = ConditionSpec.Create("Weapon_Focus", 2)
.SetUniqueWithKeyArg1()
.AddHandler(DispatcherType.ToHitBonus2, WeaponFocusToHitBonus)
                    .Build();


[TempleDllLocation(0x102ed520)]
  public static readonly ConditionSpec WeaponSpecialization = ConditionSpec.Create("Weapon_Specialization", 2)
.SetUniqueWithKeyArg1()
.AddHandler(DispatcherType.DealingDamage, WeaponSpecializationDamageBonus)
                    .Build();


[TempleDllLocation(0x102ed568)]
  public static readonly ConditionSpec WhirlwindAttack = ConditionSpec.Create("Whirlwind_Attack", 1)
.SetUnique()
.AddHandler(DispatcherType.RadialMenuEntry, WhirlwindAttackRadial)
                    .Build();


[TempleDllLocation(0x102ed368)]
  public static readonly ConditionSpec TwoWeaponRanger = ConditionSpec.Create("Two_Weapon_Ranger", 1)
.SetUnique()
.RemovedBy(TwoWeapon)
.AddHandler(DispatcherType.ToHitBonus2, TwoWeaponFightingBonusRanger)
                    .Build();


[TempleDllLocation(0x102ed8c0)]
  public static readonly ConditionSpec UncannyDodge = ConditionSpec.Create("Uncanny Dodge", 2)
.SetUnique()
.AddHandler(DispatcherType.GetAC, UncannyDodgeAcBonus)
.AddHandler(DispatcherType.SaveThrowLevel, UncannyDodgeSaveThrowBonus)
                    .Build();


[TempleDllLocation(0x102ed918)]
  public static readonly ConditionSpec ImprovedUncannyDodge = ConditionSpec.Create("Improved Uncanny Dodge", 2)
.SetUnique()
.AddQueryHandler(D20DispatcherKey.QUE_CanBeFlanked, sub_100F9180)
                    .Build();


[TempleDllLocation(0x102ed960)]
  public static readonly ConditionSpec FlurryOfBlows = ConditionSpec.Create("Flurry Of Blows", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.ToHitBonus2, MonkFlurryToHitPenalty)
.AddHandler(DispatcherType.GetBonusAttacks, GlobalOnDamage)
.AddHandler(DispatcherType.RadialMenuEntry, FlurryOfBlowsRadial)
.AddQueryHandler(D20DispatcherKey.QUE_WieldedTwoHanded, sub_100F94F0)
                    .Build();


[TempleDllLocation(0x102ed9f8)]
  public static readonly ConditionSpec BarbarianRage = ConditionSpec.Create("Barbarian_Rage", 2)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, sub_100F9540)
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_100F9540)
.AddHandler(DispatcherType.RadialMenuEntry, BarbarianRageRadialMenu)
.AddHandler(DispatcherType.D20ActionPerform, D20DispatcherKey.D20A_READIED_INTERRUPT, BarbarianRagePerform)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READIED_INTERRUPT, sub_100F96B0, 0)
                    .Build();


[TempleDllLocation(0x102ed278)]
  public static readonly ConditionSpec SneakAttack = ConditionSpec.Create("Sneak_Attack", 1)
.SetUniqueWithKeyArg1()
.AddHandler(DispatcherType.DealingDamage, SneakAttackDamageBonus)
                    .Build();


[TempleDllLocation(0x102edaa0)]
  public static readonly ConditionSpec DivineGrace = ConditionSpec.Create("Divine Grace", 0)
.SetUnique()
.AddHandler(DispatcherType.SaveThrowLevel, DivineGraceSave)
                    .Build();


[TempleDllLocation(0x102edae8)]
  public static readonly ConditionSpec SmiteEvil = ConditionSpec.Create("Smite Evil", 2)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, SmiteEvilRefresh)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, SmiteEvilRefresh)
.AddHandler(DispatcherType.RadialMenuEntry, SmiteEvilRadialMenu)
.AddHandler(DispatcherType.D20ActionPerform, D20DispatcherKey.D20A_WHOLENESS_OF_BODY_USE, SmiteEvilD20A)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_WHOLENESS_OF_BODY_USE, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
                    .Build();


[TempleDllLocation(0x102edb80)]
  public static readonly ConditionSpec AuraofCourage = ConditionSpec.Create("Aura of Courage", 0)
.SetUnique()
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_COURAGE, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_COURAGE)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 568)
.AddHandler(DispatcherType.BeginRound, AuraOfCourageBeginRound)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.conditionRemoveCallback)
                    .Build();


[TempleDllLocation(0x102edc00)]
  public static readonly ConditionSpec LayonHands = ConditionSpec.Create("Lay on Hands", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, LayOnHandsRefresher)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, LayOnHandsRefresher)
.AddHandler(DispatcherType.RadialMenuEntry, LayOnHandsRadialMenu)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)139, LayOnHandsPerform)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)139, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_DISMISS_SPELLS, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionOnActionFrame, (D20DispatcherKey)139, LayOnHandsPerformOnActionFrame)
                    .Build();


[TempleDllLocation(0x102edcc0)]
  public static readonly ConditionSpec Evasion = ConditionSpec.Create("Evasion", 0)
.SetUnique()
.AddHandler(DispatcherType.ReflexThrow, sub_100FA3C0)
.SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
                    .Build();


[TempleDllLocation(0x102edd18)]
  public static readonly ConditionSpec ImprovedEvasion = ConditionSpec.Create("Improved_Evasion", 0)
.SetUnique()
.AddHandler(DispatcherType.ReflexThrow, sub_100FA470)
.SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
                    .Build();


[TempleDllLocation(0x102edd70)]
  public static readonly ConditionSpec FastMovement = ConditionSpec.Create("Fast_Movement", 0)
.SetUnique()
.AddHandler(DispatcherType.GetMoveSpeedBase, sub_100FA510)
                    .Build();


[TempleDllLocation(0x102eddb8)]
  public static readonly ConditionSpec DivineHealth = ConditionSpec.Create("Divine_Health", 0)
.SetUnique()
.AddHandler(DispatcherType.ConditionAddPre, DivineHealthDiseaseGuard)
                    .Build();


[TempleDllLocation(0x102ede00)]
  public static readonly ConditionSpec FavoredEnemy = ConditionSpec.Create("Favored_Enemy", 2)
.AddHandler(DispatcherType.ConditionAdd, sub_100FA660)
.SetUnique()
.AddHandler(DispatcherType.SkillLevel, FavoredEnemySkillBonus)
.AddHandler(DispatcherType.DealingDamage, FavoredEnemyToHitBonus)
                    .Build();


[TempleDllLocation(0x102ede70)]
  public static readonly ConditionSpec DetectEvil = ConditionSpec.Create("Detect Evil", 0)
.SetUnique()
.AddHandler(DispatcherType.RadialMenuEntry, PaladinDetectEvilRadial)
.AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_FLEE_COMBAT, DetectEvilActionFrame)
                    .Build();


[TempleDllLocation(0x102edec8)]
  public static readonly ConditionSpec KiStrike = ConditionSpec.Create("Ki Strike", 0)
.SetUnique()
.AddHandler(DispatcherType.DealingDamage2, KiStrikeOnDamage)
                    .Build();


[TempleDllLocation(0x102edf10)]
  public static readonly ConditionSpec DefensiveRoll = ConditionSpec.Create("Defensive Roll", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.TakingDamage2, DefensiveRollOnDamage)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
                    .Build();


[TempleDllLocation(0x102edf80)]
  public static readonly ConditionSpec Opportunist = ConditionSpec.Create("Opportunist", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Broadcast_Action, OpportunistBroadcastAction)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
                    .Build();


[TempleDllLocation(0x102edff0)]
  public static readonly ConditionSpec StillMind = ConditionSpec.Create("Still Mind", 1)
.SetUnique()
.AddHandler(DispatcherType.SaveThrowLevel, sub_100FAF60)
                    .Build();


[TempleDllLocation(0x102ee038)]
  public static readonly ConditionSpec PurityOfBody = ConditionSpec.Create("Purity Of Body", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAddPre, sub_100FAFB0, StatusEffects.IncubatingDisease)
                    .Build();


[TempleDllLocation(0x102ee080)]
  public static readonly ConditionSpec RemoveDisease = ConditionSpec.Create("Remove Disease", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, sub_100FAFE0)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_100FB010)
.AddHandler(DispatcherType.RadialMenuEntry, PaladinRemoveDiseaseRadial)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)111, RemoveDiseaseActionPerform)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)111, sub_100FB180)
.AddHandler(DispatcherType.D20ActionOnActionFrame, (D20DispatcherKey)111, sub_100FB1F0)
                    .Build();


[TempleDllLocation(0x102ee128)]
  public static readonly ConditionSpec WholenessofBody = ConditionSpec.Create("Wholeness of Body", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, sub_100FB290)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_100FB290)
.AddHandler(DispatcherType.RadialMenuEntry, MonkWholenessOfBodyRadial)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)140, RemoveDiseaseActionPerform)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)140, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)113, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionOnActionFrame, (D20DispatcherKey)140, sub_100FB450)
                    .Build();


[TempleDllLocation(0x102ee240)]
  public static readonly ConditionSpec BrewPotion = ConditionSpec.Create("Brew Potion", 0)
.AddHandler(DispatcherType.RadialMenuEntry, BrewPotionRadialMenu)
                    .Build();


[TempleDllLocation(0x102ee1e8)]
  public static readonly ConditionSpec SkillMastery = ConditionSpec.Create("Skill Mastery", 2)
.AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
.AddHandler(DispatcherType.SkillLevel, SkillMasterySkillLevel)
.AddSignalHandler(D20DispatcherKey.SIG_Rogue_Skill_Mastery_Init, sub_100FB660)
                    .Build();


[TempleDllLocation(0x102ee270)]
  public static readonly ConditionSpec ScribeScroll = ConditionSpec.Create("Scribe Scroll", 0)
.AddHandler(DispatcherType.RadialMenuEntry, ScribeScrollRadialMenu)
                    .Build();


[TempleDllLocation(0x102ee2a0)]
  public static readonly ConditionSpec CraftWand = ConditionSpec.Create("Craft Wand", 0)
.AddHandler(DispatcherType.RadialMenuEntry, CraftWandRadialMenu)
                    .Build();


[TempleDllLocation(0x102ee2d0)]
  public static readonly ConditionSpec CraftRod = ConditionSpec.Create("Craft Rod", 0)
.AddHandler(DispatcherType.RadialMenuEntry, CraftRodRadialMenu)
                    .Build();


[TempleDllLocation(0x102ee300)]
  public static readonly ConditionSpec CraftWonderousItem = ConditionSpec.Create("Craft Wonderous Item", 0)
.AddHandler(DispatcherType.RadialMenuEntry, CraftWonderousItemRadialMenu)
                    .Build();


[TempleDllLocation(0x102ee330)]
  public static readonly ConditionSpec CraftMagicArmsandArmor = ConditionSpec.Create("Craft Magic Arms and Armor", 0)
.AddHandler(DispatcherType.RadialMenuEntry, CraftMagicArmsAndArmorRadialMenu)
                    .Build();


[TempleDllLocation(0x102ee360)]
  public static readonly ConditionSpec Track = ConditionSpec.Create("Track", 0)
.AddHandler(DispatcherType.RadialMenuEntry, TrackRadialMenu)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)115, TrackActivate)
                    .Build();


[TempleDllLocation(0x102ee3a8)]
  public static readonly ConditionSpec WildShape = ConditionSpec.Create("Wild Shape", 3)
.AddHandler(DispatcherType.ConditionAdd, WildShapeInit)
.AddHandler(DispatcherType.RadialMenuEntry, WildShapeRadialMenu)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)119, WildShapeCheck)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)119, WildShapeMorph)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, WildShapeInit)
.AddHandler(DispatcherType.BeginRound, WildShapeBeginRound)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, WildshapeReplaceStats)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, WildshapeReplaceStats)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, WildshapeReplaceStats)
.AddQueryHandler(D20DispatcherKey.QUE_Polymorphed, WildShapePolymorphedQuery)
.AddHandler(DispatcherType.GetCritterNaturalAttacksNum, WildShapeGetNumAttacks)
.AddQueryHandler(D20DispatcherKey.QUE_CannotCast, WildShapeCannotCastQuery)
                    .Build();


[TempleDllLocation(0x102ed45c)]
  public static readonly ConditionSpec Toughness = ConditionSpec.Create("Toughness", 1)
.AddHandler(DispatcherType.MaxHP, sub_100FC0B0)
                    .Build();


[TempleDllLocation(0x102ee4e8)]
  public static readonly ConditionSpec AnimalCompanion = ConditionSpec.Create("Animal Companion", 5)
.AddHandler(DispatcherType.ConditionAdd, sub_100FCA90)
.AddHandler(DispatcherType.RadialMenuEntry, AnimalCompanionRadialMenu)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_100FC150)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)119, AnimalCompanionCheck)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)119, AnimalCompanionSummonDismiss)
.AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
                    .Build();


[TempleDllLocation(0x102ee6b0)]
  public static readonly ConditionSpec CallFamiliar = ConditionSpec.Create("Call Familiar", 5)
.AddHandler(DispatcherType.ConditionAdd, sub_100FCA90)
.AddHandler(DispatcherType.RadialMenuEntry, CallFamiliarRadial)
.AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey)119, FamiliarSummonCheck)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)119, FamiliarSummonDismiss)
.AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
                    .Build();


[TempleDllLocation(0x102ee748)]
  public static readonly ConditionSpec Acrobatic = ConditionSpec.Create("Acrobatic", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.pick_pocket, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.tumble, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102ee7a0)]
  public static readonly ConditionSpec Investigator = ConditionSpec.Create("Investigator", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.gather_information, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.search, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102ee7f8)]
  public static readonly ConditionSpec MagicalAffinity = ConditionSpec.Create("Magical Affinity", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.spellcraft, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.use_magic_device, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102ee850)]
  public static readonly ConditionSpec Negotiator = ConditionSpec.Create("Negotiator", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.diplomacy, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.sense_motive, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102ee8a8)]
  public static readonly ConditionSpec NimbleFingers = ConditionSpec.Create("Nimble Fingers", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.open_lock, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.disable_device, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102ee900)]
  public static readonly ConditionSpec Persuasive = ConditionSpec.Create("Persuasive", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.bluff, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.intimidate, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102ee958)]
  public static readonly ConditionSpec SelfSufficient = ConditionSpec.Create("Self Sufficient", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.heal, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.wilderness_lore, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102ee9b0)]
  public static readonly ConditionSpec Stealthy = ConditionSpec.Create("Stealthy", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.hide, FeatSkillBonus, 2)
.AddSkillLevelHandler(SkillId.move_silently, FeatSkillBonus, 2)
                    .Build();


[TempleDllLocation(0x102eea50)]
  public static readonly ConditionSpec ManyShot = ConditionSpec.Create("Many Shot", 1)
.SetUnique()
.Prevents(ManyShotRanger)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, ManyshotRadial)
.AddHandler(DispatcherType.ToHitBonus2, ManyshotPenalty)
.AddSignalHandler(D20DispatcherKey.SIG_Attack_Made, ManyshotAttackMadeHandler)
                    .Build();


[TempleDllLocation(0x102eeae8)]
  public static readonly ConditionSpec ManyShotRanger = ConditionSpec.Create("Many Shot Ranger", 1)
.SetUnique()
.RemovedBy(ManyShot)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, ManyshotRadial)
.AddHandler<SubDispatcherCallback>(DispatcherType.ToHitBonus2, ArmorLightOnly, ManyshotPenalty/*0x100fcf50*/)
.AddSignalHandler<SubDispatcherCallback>(D20DispatcherKey.SIG_Attack_Made, ArmorLightOnly, ManyshotAttackMadeHandler/*0x100fd030*/)
                    .Build();


[TempleDllLocation(0x102eeb7c)]
  public static readonly ConditionSpec GreaterSpellFocus = ConditionSpec.Create("Greater Spell Focus", 1)
.AddHandler(DispatcherType.SpellDcMod, SpellDcMod_SpellFocus_Callback)
                    .Build();


[TempleDllLocation(0x102aada0)]
  public static readonly ConditionSpec TwoWeaponDefense = ConditionSpec.Create("Two Weapon Defense", 3)
.SetUnique()
.AddHandler(DispatcherType.GetAC, TwoWeaponDefenseAcBonus)
                    .Build();


[TempleDllLocation(0x102aaf58)]
  public static readonly ConditionSpec GreaterWeaponFocus = ConditionSpec.Create("Greater_Weapon_Focus", 2)
.SetUniqueWithKeyArg1()
.AddHandler(DispatcherType.ToHitBonus2, WeaponFocusToHitBonus)
                    .Build();


[TempleDllLocation(0x102ece68)]
  public static readonly ConditionSpec ImprovedTrip = ConditionSpec.Create("Improved_Trip", 2)
.SetUnique()
.AddHandler(DispatcherType.AbilityCheckModifier, ImprovedTripAbilityCheckBonus)
                    .Build();


[TempleDllLocation(0x102ed5b0)]
  public static readonly ConditionSpec AOO = ConditionSpec.Create("AOO", 3)
.AddHandler(DispatcherType.ConditionAdd, AooReset)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, true)
.AddQueryHandler(D20DispatcherKey.QUE_AOOWillTake, AoOWillTake_Callback)
.AddHandler(DispatcherType.BeginRound, AooReset)
.AddSignalHandler(D20DispatcherKey.SIG_AOOPerformed, AoOPerformed)
                    .Build();


[TempleDllLocation(0x102ed630)]
  public static readonly ConditionSpec CastDefensively = ConditionSpec.Create("Cast_Defensively", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddQueryHandler(D20DispatcherKey.QUE_ActionTriggersAOO, CastDefensivelyActionTriggersAooQuery)
.AddHandler(DispatcherType.RadialMenuEntry, CastDefensivelyRadial)
.AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, CastDefensivelySpellInterrupted)
.AddSignalHandler(D20DispatcherKey.SIG_SetCastDefensively, SetCastDefensively)
                    .Build();


[TempleDllLocation(0x102ed738)]
  public static readonly ConditionSpec CombatCasting = ConditionSpec.Create("Combat_Casting", 1)
.SetUnique()
.AddSkillLevelHandler(SkillId.concentration, CommonConditionCallbacks.CompetenceBonus, 4, 155)
.AddSkillLevelHandler(SkillId.concentration, CommonConditionCallbacks.conditionRemoveCallback)
                    .Build();


[TempleDllLocation(0x102ed790)]
  public static readonly ConditionSpec DealSubdualDamage = ConditionSpec.Create("Deal_Subdual_Damage", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, NonlethalDamageRadial)
.AddHandler(DispatcherType.ToHitBonus2, sub_100F8E70)
.AddHandler(DispatcherType.DealingDamage, sub_100F8ED0)
.AddSignalHandler(D20DispatcherKey.SIG_DealNormalDamage, sub_100F8F40)
                    .Build();


[TempleDllLocation(0x102ecc38)]
  public static readonly ConditionSpec FightingDefensively = ConditionSpec.Create("Fighting Defensively", 2)
.SetUnique()
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 0)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, FightDefensivelyRadialMenu)
.AddHandler(DispatcherType.ToHitBonus2, FightDefensivelyToHitPenalty)
.AddHandler(DispatcherType.GetAC, FightDefensivelyAcBonus)
.AddSignalHandler(D20DispatcherKey.SIG_Attack_Made, TacticalOptionAbusePrevention)
                    .Build();


[TempleDllLocation(0x102ed828)]
  public static readonly ConditionSpec DealNormalDamage = ConditionSpec.Create("Deal_Normal_Damage", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, DealNormalDamageCallback)
.AddHandler(DispatcherType.ToHitBonus2, sub_100F9040)
.AddHandler(DispatcherType.DealingDamage, sub_100F90C0)
.AddSignalHandler(D20DispatcherKey.SIG_DealNormalDamage, sub_100F9120)
                    .Build();


[TempleDllLocation(0x102ee590)]
  public static readonly ConditionSpec AnimalCompanionAnimal = ConditionSpec.Create("Animal Companion Animal", 6)
.AddHandler(DispatcherType.ConditionAdd, AnimalCompanionOnAdd)
.AddHandler(DispatcherType.BeginRound, AnimalCompanionBeginRound)
.AddHandler(DispatcherType.ToHitBonus2, AnimalCompanionToHitBonus)
.AddHandler(DispatcherType.GetBonusAttacks, AnimalCompanionNumAttacksBonus)
.AddHandler(DispatcherType.SaveThrowLevel, AnimalCompanionSaveThrowBonus)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, AnimalCompanionStatBonus)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, AnimalCompanionStatBonus)
.AddHandler(DispatcherType.ReflexThrow, AnimalCompanionReflexBonus)
.AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 1)
.AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 1)
.SetQueryResult(D20DispatcherKey.QUE_ExperienceExempt, true)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Cannot_Loot, true)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Cannot_Wield_Items, true)
                    .Build();


[TempleDllLocation(0x102ed6c8)]
  public static readonly ConditionSpec AutoendTurn = ConditionSpec.Create("Autoend_Turn", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddQueryHandler(D20DispatcherKey.QUE_Autoend_Turn, CommonConditionCallbacks.D20Query_Callback_GetSDDKey1, 0)
.AddHandler(DispatcherType.RadialMenuEntry, AutoendTurnRadial)
                    .Build();


public static IReadOnlyList<ConditionSpec> Conditions {get;} = new List<ConditionSpec>
{
NimbleFingers,
TwoWeaponRanger,
Stealthy,
Dodge,
MagicalAffinity,
SneakAttack,
AOO,
featstunningfist,
SmiteEvil,
StillMind,
RapidShot,
Cleave,
BarbarianRage,
ImprovedTwoWeapon,
DivineGrace,
GreatFortitude,
ImprovedTwoWeaponRanger,
WeaponFinesse,
QuickDraw,
Acrobatic,
WholenessofBody,
AutoendTurn,
AuraofCourage,
RapidShotRanger,
DealNormalDamage,
SelfSufficient,
Alertness,
RemoveDisease,
Run,
VenomImmunityDruid,
AnimalCompanion,
ImprovedCritical,
CraftMagicArmsandArmor,
DealSubdualDamage,
FastMovement,
BrewPotion,
CallFamiliar,
DeflectArrows,
PointBlankShot,
ManyShot,
Opportunist,
CastDefensively,
KiStrike,
Negotiator,
DivineHealth,
PurityOfBody,
DefensiveRoll,
LightingReflexes,
WeaponSpecialization,
CraftRod,
ImprovedInitiative,
ImprovedEvasion,
GreatCleave,
Persuasive,
CraftWand,
CombatCasting,
FavoredEnemy,
SkillFocus,
TwoWeapon,
FeatExpertise,
FeatMobility,
IronWill,
UncannyDodge,
TwoWeaponDefense,
LayonHands,
Evasion,
AnimalCompanionAnimal,
GreaterSpellFocus,
NatureSense,
PowerAttack,
SkillMastery,
FightingDefensively,
Track,
WeaponFocus,
WhirlwindAttack,
FlurryOfBlows,
WildShape,
GreaterWeaponFocus,
CraftWonderousItem,
ManyShotRanger,
Investigator,
ScribeScroll,
ImprovedUncannyDodge,
DetectEvil,
SpellFocus,
CraftStaff,
Toughness,
ImprovedTrip,
ForgeRing,
};

[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100fb660)]
public static void sub_100FB660(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb5b0)]
public static void CraftWandRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100fa060)]
public static void LayOnHandsPerform(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100fbdb0)]
public static void WildShapeInit(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fcdf0)]
public static void AutoendTurnRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100fafb0)]
public static void sub_100FAFB0(in DispatcherCallbackArgs evt, ConditionSpec data) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100f9820)]
public static void StunningFistResetArg(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f8ed0)]
public static void sub_100F8ED0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fcad0)]
public static void CallFamiliarRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fc8f0)]
public static void AnimalCompanionSaveThrowBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb970)]
public static void TrackRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x100fb180)]
public static void sub_100FB180(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100fcdb0)]
public static void MobilityAcBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100fd120)]
public static void TwoWeaponDefenseAcBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x100fa0f0)]
public static void LayOnHandsPerformOnActionFrame(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100faa00)]
public static void PaladinDetectEvilRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100faf10)]
public static void FeatSaveBonus(in DispatcherCallbackArgs evt, int data) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f9390)]
public static void FlurryOfBlowsRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100f9dc0)]
public static void AuraOfCourageBeginRound(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100fc860)]
public static void AnimalCompanionToHitBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb750)]
public static void CraftMagicArmsAndArmorRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f9180)]
public static void sub_100F9180(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100faf60)]
public static void sub_100FAF60(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100fafe0)]
public static void sub_100FAFE0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f9470)]
public static void MonkFlurryToHitPenalty(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.AbilityCheckModifier)]
[TempleDllLocation(0x100fa430)]
public static void ImprovedTripAbilityCheckBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100fba00)]
public static void TrackActivate(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetCriticalHitRange)]
[TempleDllLocation(0x100f8320)]
public static void ImprovedCriticalGetCritThreatRange(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f8f40)]
public static void sub_100F8F40(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fc170)]
public static void AnimalCompanionRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f9870)]
public static void StunningFistRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fa960)]
public static void RapidShotRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f8a10)]
public static void AoOWillTake_Callback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f8a70)]
public static void AoOPerformed(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x100fb1f0)]
public static void sub_100FB1F0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f80c0)]
public static void WeaponFinesseToHitBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x100faa90)]
public static void DetectEvilActionFrame(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ReflexThrow)]
[TempleDllLocation(0x100fa470)]
public static void sub_100FA470(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb070)]
public static void PaladinRemoveDiseaseRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f84c0)]
public static void PowerAttackToHitPenalty(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100fd4f0)]
public static void AnimalCompanionSummonDismiss(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f82a0)]
public static void WeaponSpecializationDamageBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetBonusAttacks)]
[TempleDllLocation(0x100fc8b0)]
public static void AnimalCompanionNumAttacksBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f8cc0)]
public static void CastDefensivelySpellInterrupted(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100f9d00)]
public static void SmiteEvilD20A(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f90c0)]
public static void sub_100F90C0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100fac30)]
public static void sub_100FAC30(in DispatcherCallbackArgs evt, ConditionSpec data) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetNumAttacksBase)]
[TempleDllLocation(0x100fd1c0)]
public static void ImprovedTWF(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100faac0)]
public static void KiStrikeOnDamage(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100fca90)]
public static void sub_100FCA90(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100fc980)]
public static void AnimalCompanionStatBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100f92a0)]
public static void UncannyDodgeSaveThrowBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f8660)]
public static void PowerAttackSetViaSignal(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100f8040)]
public static void FightDefensivelyAcBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fbb20)]
public static void WildShapeRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100f9ba0)]
public static void DivineGraceSave(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f7e20)]
public static void CombatExpertiseToHitPenalty(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f8220)]
public static void WeaponFocusToHitBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.NewDay)]
[TempleDllLocation(0x100fc150)]
public static void sub_100FC150(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f8be0)]
public static void CastDefensivelyActionTriggersAooQuery(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100fab50)]
public static void DefensiveRollOnDamage(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100facc0)]
public static void SkillFocusSkillLevelCallback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f8000)]
public static void FightDefensivelyToHitPenalty(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f9040)]
public static void sub_100F9040(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb5d0)]
public static void CraftRodRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f8c20)]
public static void CastDefensivelyRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.MaxHP)]
[TempleDllLocation(0x100fc0b0)]
public static void sub_100FC0B0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100fd030)]
public static void ManyshotAttackMadeHandler(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f86b0)]
public static void CleaveDroppedEnemy(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100f9430)]
public static void sub_100F9430(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f7f60)]
public static void FightDefensivelyRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100f7cd0)]
public static void Dodge_ACBonus_Callback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100fad90)]
public static void PointBlankShotDamage(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb770)]
public static void CraftStaffRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100f9220)]
public static void UncannyDodgeAcBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x100fbc60)]
public static void WildShapeCheck(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f9a10)]
public static void SneakAttackDamageBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100fc7f0)]
public static void AnimalCompanionBeginRound(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100fac80)]
public static void sub_100FAC80(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.InitiativeMod)]
[TempleDllLocation(0x100f83d0)]
public static void ImprovedInitiativeCallback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100fa660)]
public static void sub_100FA660(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.BeginRound)]
[TempleDllLocation(0x100f8af0)]
public static void AooReset(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f88c0)]
public static void TwoWeaponFightingBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x100fcbf0)]
public static void FamiliarSummonCheck(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb590)]
public static void ScribeScrollRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100fb150)]
public static void RemoveDiseaseActionPerform(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100f9910)]
public static void sub_100F9910(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ReflexThrow)]
[TempleDllLocation(0x100fc9d0)]
public static void AnimalCompanionReflexBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.BeginRound)]
[TempleDllLocation(0x100f7c90)]
public static void sub_100F7C90(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100fbce0)]
public static void WildShapeMorph(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f8940)]
public static void TwoWeaponFightingBonusRanger(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100fa510)]
public static void sub_100FA510(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f7d60)]
public static void CombatExpertise_RadialMenuEntry_Callback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f8da0)]
public static void NonlethalDamageRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x100f96b0)]
public static void sub_100F96B0(in DispatcherCallbackArgs evt, int data) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100f7e70)]
public static void CombatExpertiseAcBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb730)]
public static void CraftWonderousItemRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb790)]
public static void ForgeRingRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f8b70)]
public static void WhirlwindAttackRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb570)]
public static void BrewPotionRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f7f00)]
public static void CombatExpertiseSet(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f8e70)]
public static void sub_100F8E70(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f94f0)]
public static void sub_100F94F0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ReflexThrow)]
[TempleDllLocation(0x100fa3c0)]
public static void sub_100FA3C0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100fa8d0)]
public static void RapidShotMallus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f8d60)]
public static void SetCastDefensively(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f8540)]
public static void PowerAttackDamageBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fb2d0)]
public static void MonkWholenessOfBodyRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100f9760)]
public static void BarbarianRagePerform(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100f9e00)]
public static void LayOnHandsRefresher(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f8410)]
public static void PowerAttackRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100f9150)]
public static void sub_100F9150(in DispatcherCallbackArgs evt, ConditionSpec data) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DeflectArrows)]
[TempleDllLocation(0x100fa2f0)]
public static void DeflectArrowsCallback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100f9c00)]
public static void SmiteEvilRefresh(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100fa780)]
public static void FavoredEnemyToHitBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f9c50)]
public static void SmiteEvilRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f9120)]
public static void sub_100F9120(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x100fd620)]
public static void FamiliarSummonDismiss(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100fbfc0)]
public static void WildShapeCannotCastQuery(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100fa690)]
public static void FavoredEnemySkillBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f87d0)]
public static void GreatCleaveDroppedEnemy(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x100fc2b0)]
public static void AnimalCompanionCheck(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100fc7c0)]
public static void AnimalCompanionOnAdd(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f7ed0)]
public static void TacticalOptionAbusePrevention(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100f9540)]
public static void sub_100F9540(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100fa880)]
public static void DivineHealthDiseaseGuard(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x100fb450)]
public static void sub_100FB450(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100fbe70)]
public static void WildShapeBeginRound(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fceb0)]
public static void ManyshotRadial(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetBonusAttacks)]
[TempleDllLocation(0x100ee910)]
public static void GlobalOnDamage(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100fad20)]
public static void PointBlankShotToHitBonus(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100fbf30)]
public static void WildshapeReplaceStats(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetBonusAttacks)]
[TempleDllLocation(0x100fa920)]
public static void RapidShotNumAttacksPerTurn(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100fcf50)]
public static void ManyshotPenalty(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f95b0)]
public static void BarbarianRageRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100fb290)]
public static void sub_100FB290(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.NewDay)]
[TempleDllLocation(0x100fb010)]
public static void sub_100FB010(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SpellDcMod)]
[TempleDllLocation(0x100fc050)]
public static void SpellDcMod_SpellFocus_Callback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f8f70)]
public static void DealNormalDamageCallback(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x100f99b0)]
public static void sub_100F99B0(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100f9ec0)]
public static void LayOnHandsRadialMenu(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100fbf90)]
public static void WildShapePolymorphedQuery(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100fadf0)]
public static void OpportunistBroadcastAction(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetNumAttacksBase, DispatcherType.D20Signal, DispatcherType.ToHitBonus2, DispatcherType.GetBonusAttacks)]
[TempleDllLocation(0x100fd250)]
public static void ArmorLightOnly(in DispatcherCallbackArgs evt, SubDispatcherCallback data) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100fb5f0)]
public static void SkillMasterySkillLevel(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.GetCritterNaturalAttacksNum)]
[TempleDllLocation(0x100fc010)]
public static void WildShapeGetNumAttacks(in DispatcherCallbackArgs evt) {
throw new NotImplementedException();
}



[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100faec0)]
public static void FeatSkillBonus(in DispatcherCallbackArgs evt, int data) {
throw new NotImplementedException();
}


}
}
/*


*/