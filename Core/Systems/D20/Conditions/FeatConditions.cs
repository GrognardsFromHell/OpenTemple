using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.TimeEvents;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static class FeatConditions
    {
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

// In the condition registry, this is overwritten by the status effect spell resistance
        [TempleDllLocation(0x102aae80)]
        public static readonly ConditionSpec SpellResistance = ConditionSpec.Create("Spell Resistance", 3)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.SpellResistanceDebug)
            .AddHandler(DispatcherType.ConditionAdd, sub_100F9430)
            .AddHandler(DispatcherType.SpellResistanceMod, CommonConditionCallbacks.SpellResistanceMod_Callback, 5048)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance,
                CommonConditionCallbacks.SpellResistanceQuery)
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
        public static readonly ConditionSpec ImprovedTwoWeaponRanger = ConditionSpec
            .Create("Improved_Two_Weapon_Ranger", 2)
            .Prevents(ImprovedTwoWeapon)
            .RemovedBy(ImprovedTwoWeaponRanger)
            .AddHandler<SubDispatcherCallback>(DispatcherType.GetNumAttacksBase, ArmorLightOnly,
                ImprovedTWF /*0x100fd1c0*/)
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
            .AddHandler<SubDispatcherCallback>(DispatcherType.ToHitBonus2, ArmorLightOnly,
                RapidShotMallus /*0x100fa8d0*/)
            .AddHandler<SubDispatcherCallback>(DispatcherType.GetBonusAttacks, ArmorLightOnly,
                RapidShotNumAttacksPerTurn /*0x100fa920*/)
            .Build();


        [TempleDllLocation(0x102ed1d4)]
        public static readonly ConditionSpec Run = ConditionSpec.Create("Run", 1)
            .SetUnique()
            .Build();


        [TempleDllLocation(0x102ed208)]
        public static readonly ConditionSpec SkillFocus = ConditionSpec.Create("Skill_Focus", 1)
            .AddHandler(DispatcherType.ConditionAddPre, CommonConditionCallbacks.CondPreventSameArg,
                (ConditionSpec) null)
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
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_LAY_ON_HANDS_USE,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
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
            .AddHandler(DispatcherType.GetBonusAttacks, FlurryOfBlowsBonusAttacks)
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
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_WHOLENESS_OF_BODY_USE,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .Build();


        [TempleDllLocation(0x102edb80)]
        public static readonly ConditionSpec AuraofCourage = ConditionSpec.Create("Aura of Courage", 0)
            .SetUnique()
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_COURAGE,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_COURAGE)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 568)
            .AddHandler(DispatcherType.BeginRound, AuraOfCourageBeginRound)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.conditionRemoveCallback)
            .Build();


        [TempleDllLocation(0x102edc00)]
        public static readonly ConditionSpec LayonHands = ConditionSpec.Create("Lay on Hands", 3)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, LayOnHandsRefresher)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, LayOnHandsRefresher)
            .AddHandler(DispatcherType.RadialMenuEntry, LayOnHandsRadialMenu)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 139, LayOnHandsPerform)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 139,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_DISMISS_SPELLS,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, (D20DispatcherKey) 139, LayOnHandsPerformOnActionFrame)
            .Build();


        [TempleDllLocation(0x102edcc0)]
        public static readonly ConditionSpec Evasion = ConditionSpec.Create("Evasion", 0)
            .SetUnique()
            .AddHandler(DispatcherType.ReflexThrow, EvasionReflexThrow)
            .SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
            .Build();


        [TempleDllLocation(0x102edd18)]
        public static readonly ConditionSpec ImprovedEvasion = ConditionSpec.Create("Improved_Evasion", 0)
            .SetUnique()
            .AddHandler(DispatcherType.ReflexThrow, ImprovedEvasionReflexThrow)
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
            .AddHandler(DispatcherType.DealingDamage, FavoredEnemyDamageBonus)
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
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .Build();


        [TempleDllLocation(0x102edf80)]
        public static readonly ConditionSpec Opportunist = ConditionSpec.Create("Opportunist", 1)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
            .AddSignalHandler(D20DispatcherKey.SIG_Broadcast_Action, OpportunistBroadcastAction)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
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
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, RemoveDiseaseResetCharges)
            .AddHandler(DispatcherType.RadialMenuEntry, PaladinRemoveDiseaseRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 111, RemoveDiseaseActionPerform)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 111, RemoveDiseaseActionCheck)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, (D20DispatcherKey) 111, sub_100FB1F0)
            .Build();


        [TempleDllLocation(0x102ee128)]
        public static readonly ConditionSpec WholenessofBody = ConditionSpec.Create("Wholeness of Body", 3)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, sub_100FB290)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_100FB290)
            .AddHandler(DispatcherType.RadialMenuEntry, MonkWholenessOfBodyRadial)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 140, RemoveDiseaseActionPerform)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 140,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 113,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, (D20DispatcherKey) 140, WholenessOfBodyActionFrame)
            .Build();


        [TempleDllLocation(0x102ee240)]
        public static readonly ConditionSpec BrewPotion = ConditionSpec.Create("Brew Potion", 0)
            .AddHandler(DispatcherType.RadialMenuEntry, BrewPotionRadialMenu)
            .Build();


        [TempleDllLocation(0x102ee1e8)]
        public static readonly ConditionSpec SkillMastery = ConditionSpec.Create("Skill Mastery", 2)
            .AddHandler(DispatcherType.ConditionAdd, DummyCallbacks.EmptyFunction)
            .AddHandler(DispatcherType.SkillLevel, SkillMasterySkillLevel)
            .AddSignalHandler(D20DispatcherKey.SIG_Rogue_Skill_Mastery_Init, RogueSkillMasteryInit)
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
        public static readonly ConditionSpec CraftMagicArmsandArmor = ConditionSpec
            .Create("Craft Magic Arms and Armor", 0)
            .AddHandler(DispatcherType.RadialMenuEntry, CraftMagicArmsAndArmorRadialMenu)
            .Build();


        [TempleDllLocation(0x102ee360)]
        public static readonly ConditionSpec Track = ConditionSpec.Create("Track", 0)
            .AddHandler(DispatcherType.RadialMenuEntry, TrackRadialMenu)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 115, TrackActivate)
            .Build();


        [TempleDllLocation(0x102ee3a8)]
        public static readonly ConditionSpec WildShape = ConditionSpec.Create("Wild Shape", 3)
            .AddHandler(DispatcherType.ConditionAdd, WildShapeInit)
            .AddHandler(DispatcherType.RadialMenuEntry, WildShapeRadialMenu)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 119, WildShapeCheck)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 119, WildShapeMorph)
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
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 119, AnimalCompanionCheck)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 119, AnimalCompanionSummonDismiss)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
            .Build();


        [TempleDllLocation(0x102ee6b0)]
        public static readonly ConditionSpec CallFamiliar = ConditionSpec.Create("Call Familiar", 5)
            .AddHandler(DispatcherType.ConditionAdd, sub_100FCA90)
            .AddHandler(DispatcherType.RadialMenuEntry, CallFamiliarRadial)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 119, FamiliarSummonCheck)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 119, FamiliarSummonDismiss)
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
            .AddHandler<SubDispatcherCallback>(DispatcherType.ToHitBonus2, ArmorLightOnly,
                ManyshotPenalty /*0x100fcf50*/)
            .AddSignalHandler<SubDispatcherCallback>(D20DispatcherKey.SIG_Attack_Made, ArmorLightOnly,
                ManyshotAttackMadeHandler /*0x100fd030*/)
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
            .AddHandler(DispatcherType.DealingDamage, NonlethalDamageSetSubdual)
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
            .AddQueryHandler(D20DispatcherKey.QUE_Autoend_Turn, CommonConditionCallbacks.D20Query_Callback_GetSDDKey1,
                0)
            .AddHandler(DispatcherType.RadialMenuEntry, AutoendTurnRadial)
            .Build();


        public static IReadOnlyList<ConditionSpec> Conditions { get; } = new List<ConditionSpec>
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
        public static void RogueSkillMasteryInit(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg2()) == 0)
            {
                var dispIo = evt.GetDispIoD20Signal();
                evt.SetConditionArg2(dispIo.data1);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb5b0)]
        public static void CraftWandRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage.craft_wand, "TAG_CRAFT_WAND", 3);
        }

        [TempleDllLocation(0x100fb6a0)]
        private static void AddCraftUiRadialEntry(in DispatcherCallbackArgs evt, D20CombatMessage combatMesId,
            string helpSystemString, int craftingTypeCode)
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                var radMenuEntry = RadialMenuEntry.CreateAction((int) combatMesId,
                    D20ActionType.ITEM_CREATION, craftingTypeCode, helpSystemString);
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Feats);
            }
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100fa060)]
        [TemplePlusLocation("condition.cpp:407")]
        public static void LayOnHandsPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var action = dispIo.action;

            bool success;
            if (GameSystems.Critter.IsUndead(action.d20ATarget))
            {
                action.d20Caf |= D20CAF.TOUCH_ATTACK;
                if ((action.d20Caf & D20CAF.RANGED) != 0)
                {
                    return;
                }

                GameSystems.D20.Combat.ToHitProcessing(action);
                success = GameSystems.Anim.PushAttemptAttack(action.d20APerformer, action.d20ATarget);
            }
            else
            {
                success = GameSystems.Anim.PushAnimate(action.d20APerformer, NormalAnimType.EnchantmentCasting);
            }

            if (success)
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }
        }

        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100fbdb0)]
        [TemplePlusLocation("condition.cpp:472")]
        public static void WildShapeInit(in DispatcherCallbackArgs evt)
        {
            var druidLvl = evt.objHndCaller.GetStat(Stat.level_druid);
            var numTimes = 1; // number of times can wild shape per day
            if (druidLvl >= 6)
            {
                switch (druidLvl)
                {
                    case 6:
                        numTimes = 2;
                        break;
                    case 7:
                    case 8:
                    case 9:
                        numTimes = 3;
                        break;
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        numTimes = 4;
                        break;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        numTimes = 5;
                        break;
                    default: // 18 and above
                        numTimes = 6;
                        break;
                }

                // elemental num times (new)
                if (druidLvl >= 16)
                {
                    numTimes += (1 << 8);
                }

                if (druidLvl >= 18)
                    numTimes += (1 << 8);
                if (druidLvl >= 20)
                    numTimes += (1 << 8);
            }

            //See if any bonus uses should be added
            var extraWildShape = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Extra Wildshape Uses");
            var extraElementalWildShape =
                GameSystems.D20.D20QueryPython(evt.objHndCaller, "Extra Wildshape Elemental Uses");
            numTimes += extraWildShape;
            numTimes += (1 << 8) * extraElementalWildShape;

            evt.SetConditionArg1(numTimes);
            if (evt.GetConditionArg3() != 0)
            {
                evt.SetConditionArg3(0);
                evt.objHndCaller.FreeAnimHandle();

                GameSystems.ParticleSys.CreateAtObj("sp-animal shape", evt.objHndCaller);
                GameSystems.D20.Status.initItemConditions(evt.objHndCaller);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fcdf0)]
        public static void AutoendTurnRadial(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = evt.CreateToggleForArg(0);
            var meslineKey = 5084;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            radMenuEntry.text = (string) meslineValue;
            var v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Options);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100fafb0)]
        public static void sub_100FAFB0(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == data)
            {
                if ((dispIo.arg1) != 0)
                {
                    dispIo.outputFlag = 0;
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100f9820)]
        public static void StunningFistResetArg(in DispatcherCallbackArgs evt)
        {
            var overallLevel = evt.objHndCaller.GetStat(Stat.level);
            var monkLevel = evt.objHndCaller.GetStat(Stat.level_monk);
            var nonMonkLevel = overallLevel - monkLevel;

            // If a monk belt is worn, treat the monk level one higher
            var hasMonkBelt =
                GameSystems.Item.IsProtoWornAt(evt.objHndCaller, EquipSlot.Lockpicks, WellKnownProtos.MonksBelt);
            if (hasMonkBelt)
            {
                monkLevel += 1;
            }

            var usesPerDay = monkLevel + nonMonkLevel / 4;
            evt.SetConditionArg1(usesPerDay);
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f8ed0)]
        [TemplePlusLocation("condition.cpp:426")]
        public static void NonlethalDamageSetSubdual(in DispatcherCallbackArgs evt)
        {
            //First check if subdual damage is turned on
            if (evt.GetConditionArg1() != 0)
            {
                var dispIo = evt.GetDispIoDamage();

                //If a weapon is used or the attacker has the improved unarmed strike feat and not a ranged attack then change to subdual
                if ((dispIo.attackPacket.GetWeaponUsed() != null ||
                     GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.IMPROVED_UNARMED_STRIKE))
                    && (dispIo.attackPacket.flags & D20CAF.RANGED) == 0)
                {
                    // Convert all physical damage to subudal.  Fire and other special damage types are excluded.
                    // The game originally converted only the first dice for all damage types.  This will work
                    // more correctly for feats that add bonus damage dice.
                    for (var i = 0; i < dispIo.damage.dice.Count; i++)
                    {
                        var dice = dispIo.damage.dice[i];
                        var diceType = dice.type;
                        if (diceType == DamageType.Bludgeoning ||
                            diceType == DamageType.Piercing ||
                            diceType == DamageType.Slashing ||
                            diceType == DamageType.BludgeoningAndPiercing ||
                            diceType == DamageType.PiercingAndSlashing ||
                            diceType == DamageType.SlashingAndBludgeoning ||
                            diceType == DamageType.SlashingAndBludgeoningAndPiercing)
                        {
                            dice.type = DamageType.Subdual;
                            dispIo.damage.dice[i] = dice;
                        }
                    }
                }
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fcad0)]
        public static void CallFamiliarRadial(in DispatcherCallbackArgs evt)
        {
            var condArg5 = evt.GetConditionArg(4);
            if (condArg5 != 0)
            {
                if (GameSystems.TimeEvent.CurrentDayOfYear <= condArg5)
                {
                    return;
                }

                evt.SetConditionArg(4, 0);
            }

            var parentEntry = RadialMenuEntry.CreateParent(6005);
            var parentIdx =
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                    RadialMenuStandardNode.Class);

            var condArg1 = evt.GetConditionArg1();
            if ((condArg1 | evt.GetConditionArg2()) != 0)
            {
                var dismissEntry = RadialMenuEntry.CreateAction(6006, D20ActionType.CLASS_ABILITY_SA, 29,
                    "TAG_CLASS_FEATURES_WIZARD_CALL_FAMILIAR");
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref dismissEntry, parentIdx);
            }
            else
            {
                for (var i = 0; i < 10; i++)
                {
                    AddCallFamiliarSubEntry(evt.objHndCaller, i, parentIdx);
                }
            }
        }

        private static readonly int[] FamiliarProtos =
        {
            12045, // Bat
            12046, // Cat
            12047, // Hawk
            12048, // Lizard
            12049, // Owl
            12050, // Rat
            12051, // Raven
            12052, // Snake
            12053, // Toad
            12054 // Weasel
        };

        [TempleDllLocation(0x100fca20)]
        private static void AddCallFamiliarSubEntry(GameObjectBody caster, int familiarIdx, int parentIdx)
        {
            var familiarProtoId = FamiliarProtos[familiarIdx];
            var familiarProto = GameSystems.Proto.GetProtoById((ushort) familiarProtoId);
            var familiarText = GameSystems.MapObject.GetDisplayName(familiarProto);

            var radMenuEntry = RadialMenuEntry.CreateAction(familiarText, D20ActionType.CLASS_ABILITY_SA,
                19 + familiarIdx, "TAG_CLASS_FEATURES_WIZARD_CALL_FAMILIAR");
            GameSystems.D20.RadialMenu.AddChildNode(caster, ref radMenuEntry, parentIdx);
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100fc8f0)]
        public static void AnimalCompanionSaveThrowBonus(in DispatcherCallbackArgs evt)
        {
            int bonusValue;

            var dispIo = evt.GetDispIoSavingThrow();
            var levelAboveMin = AnimalCompanionRefreshHp(in evt, evt.objHndCaller);
            if (evt.dispKey == D20DispatcherKey.SAVE_FORTITUDE || evt.dispKey == D20DispatcherKey.SAVE_REFLEX)
            {
                bonusValue = (levelAboveMin + 4) / 2;
            }
            else
            {
                bonusValue = levelAboveMin / 3;
            }

            dispIo.bonlist.AddBonus(bonusValue, 0, 275);
            if (evt.dispKey == D20DispatcherKey.SAVE_WILL && levelAboveMin >= 6)
            {
                dispIo.bonlist.AddBonus(4, 0, 276);
            }
        }

        [TempleDllLocation(0x100fc680)]
        private static int AnimalCompanionRefreshHp(in DispatcherCallbackArgs evt, GameObjectBody obj)
        {
            var master = evt.GetConditionObjArg(1);
            if (master != null)
            {
                var drdLvl = master.GetStat(Stat.level_druid);
                var rgrLvl = master.GetStat(Stat.level_ranger);
                if (rgrLvl > 4)
                {
                    drdLvl += rgrLvl / 2;
                }

                var ownerLevelAboveMin = drdLvl - evt.GetConditionArg1();
                var condArg6 = evt.GetConditionArg(5);
                if (ownerLevelAboveMin > condArg6)
                {
                    var v9 = 2 * (condArg6 / 3);
                    var v10 = 2 * (ownerLevelAboveMin / 3);
                    var baseHp = obj.GetInt32(obj_f.hp_pts);
                    var hitDieSides = obj.GetInt32(obj_f.npc_hitdice_idx, 1);
                    if (v9 < v10)
                    {
                        var v12 = v10 - v9;
                        for (var i = 0; i < v12; i++)
                        {
                            baseHp += Dice.Roll(1, hitDieSides);
                        }
                    }

                    obj.SetInt32(obj_f.hp_pts, baseHp);
                    evt.SetConditionArg(5, ownerLevelAboveMin);
                }

                return ownerLevelAboveMin;
            }
            else
            {
                AnimalCompanionRunoff(evt.subDispNode.condNode, null, obj);
                return 0;
            }
        }

        [TempleDllLocation(0x100fc3d0)]
        [TemplePlusLocation("ability_fixes.cpp:82")]
        private static void AnimalCompanionRunoff(ConditionAttachment conditionAttachment, GameObjectBody owner,
            GameObjectBody obj)
        {
            GameSystems.Party.RemoveFromAllGroups(obj);
            GameUiBridge.UpdatePartyUi();

            var runOffFrom = owner?.GetLocationFull() ?? obj.GetLocationFull();

            // Make sure it doesnt attack us while running off
            obj.SetNPCFlags(obj.GetNPCFlags() | NpcFlag.KOS_OVERRIDE);
            GameSystems.MapObject.SetFlags(obj, ObjectFlag.CLICK_THROUGH);
            obj.AiFlags |= AiFlag.RunningOff;
            GameSystems.ObjFade.FadeTo(obj, 0, 25, 5, 2);

            GameSystems.Anim.PushRunNearTile(obj, runOffFrom, 5);
            conditionAttachment.args[0] = 0;
            // TODO: This is not going to work since 1+2 are an obj-ref
            conditionAttachment.args[1] = 0;
            conditionAttachment.args[2] = 0;
            conditionAttachment.args[4] = 0;
            throw new NotImplementedException();
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb970)]
        public static void TrackRadialMenu(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                var radMenuEntry = RadialMenuEntry.CreateAction(D20CombatMessage.track,
                    D20ActionType.TRACK, 0, "TAG_TRACK");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Feats);
            }
        }

        private static int GetWeeklyUsesOfRemoveDisease(GameObjectBody critter)
        {
            var paladinLevels = critter.GetStat(Stat.level_paladin) / 3;
            return (paladinLevels - 3) / 3; // On level 6 you get it once, then every 3 levels thereafter
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100fb180)]
        public static void RemoveDiseaseActionCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var usesPerWeek = GetWeeklyUsesOfRemoveDisease(evt.objHndCaller);

            var condArg1 = evt.GetConditionArg1();
            var bitmask = 0xF;
            for (var i = 0; i < usesPerWeek; i++)
            {
                if ((condArg1 & bitmask) == 0)
                {
                    return; // Found a free use of the feat
                }

                bitmask <<= 4;
            }

            // No uses left
            dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100fcdb0)]
        public static void MobilityAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if ((dispIo.attackPacket.flags & D20CAF.AOO_MOVEMENT) != 0)
            {
                var v2 = GameSystems.Feat.GetFeatName(FeatId.MOBILITY);
                dispIo.bonlist.AddBonus(4, 8, 114, v2);
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100fd120)]
        public static void TwoWeaponDefenseAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v1 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 203);
            var v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 204);
            if (v1 != v2 && v1 != null && v2 != null)
            {
                var v3 = 2;
                if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_FightingDefensively))
                {
                    v3 = 1;
                }

                var v4 = GameSystems.Feat.GetFeatName(FeatId.TWO_WEAPON_DEFENSE);
                dispIo.bonlist.AddBonus(v3, 29, 114, v4);
            }
        }


        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x100fa0f0)]
        [TemplePlusLocation("condition.cpp:409")]
        public static void LayOnHandsPerformOnActionFrame(in DispatcherCallbackArgs evt)
        {
            Dice v11;
            int evta;

            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var v2 = dispIo.action;
            GameSystems.RollHistory.CreateRollHistoryString(v2.rollHistId0);
            GameSystems.RollHistory.CreateRollHistoryString(v2.rollHistId1);
            GameSystems.RollHistory.CreateRollHistoryString(v2.rollHistId2); /*INLINED:v3=evt.subDispNode*/
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var v5 = condArg1 - condArg2;
            var damageDicePacked = Dice.Constant(condArg2);
            evt.SetConditionArg1(v5);
            evt.SetConditionArg2(v5);
            if (GameSystems.Critter.IsUndead(dispIo.action.d20ATarget))
            {
                var v12 = dispIo.action;
                if ((v12.d20Caf & D20CAF.HIT) != 0)
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1B, evt.objHndCaller, v12.d20ATarget);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 5051);
                    GameSystems.D20.Combat.DoUnclassifiedDamage(dispIo.action.d20ATarget, evt.objHndCaller,
                        damageDicePacked, DamageType.PositiveEnergy, D20AttackPower.UNSPECIFIED, v2.d20ActType);
                }
                else
                {
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 29);
                }
            }
            else
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1B, evt.objHndCaller,
                    dispIo.action.d20ATarget);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 5051);
                int v6 = dispIo.action.d20ATarget.GetInt32(obj_f.hp_damage);
                var v7 = dispIo.action.d20ATarget.GetInt32(obj_f.critter_subdual_damage);
                if (v7 >= v6)
                {
                    evta = v7;
                }
                else
                {
                    v7 = v6;
                    evta = v6;
                }

                if (condArg2 <= v7)
                {
                    v11 = damageDicePacked;
                }
                else
                {
                    var v8 = condArg2 - v7;
                    var v9 = evt.GetConditionArg1();
                    evt.SetConditionArg1(v8 + v9);
                    var v10 = evt.GetConditionArg2();
                    evt.SetConditionArg2(v8 + v10);
                    v11 = Dice.Constant(evta);
                }

                GameSystems.Combat.Heal(dispIo.action.d20ATarget, evt.objHndCaller, v11, v2.d20ActType);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100faa00)]
        public static void PaladinDetectEvilRadial(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                var radMenuEntry = RadialMenuEntry.CreateAction(5054, D20ActionType.DETECT_EVIL, 0,
                    "TAG_CLASS_FEATURES_PALADIN_DETECT_EVIL");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Class);
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100faf10)]
        public static void FeatSaveBonus(in DispatcherCallbackArgs evt, int data)
        {
            var v1 = data;
            var dispIo = evt.GetDispIoSavingThrow();
            var condArg1 = evt.GetConditionArg1();
            var v4 = GameSystems.Feat.GetFeatName((FeatId) condArg1);
            dispIo.bonlist.AddBonus(v1, 0, 114, v4);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f9390)]
        public static void FlurryOfBlowsRadial(in DispatcherCallbackArgs evt)
        {
            int v2;

            var radMenuEntry = evt.CreateToggleForArg(0);
            var meslineKey = 5038;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            radMenuEntry.text = (string) meslineValue;
            radMenuEntry.helpSystemHashkey = "TAG_CLASS_FEATURES_MONK_FLURRY_OF_BLOWS";
            var v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100f9dc0)]
        public static void AuraOfCourageBeginRound(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                evt.objHndCaller.AddConditionToPartyAround(50F, StatusEffects.CouragedAura, evt.objHndCaller);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100fc860)]
        public static void AnimalCompanionToHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            int v2 = AnimalCompanionRefreshHp(in evt, evt.objHndCaller);
            dispIo.bonlist.AddBonus(3 * v2 / 4, 0, 275);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb750)]
        public static void CraftMagicArmsAndArmorRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage.craft_magic_arms_and_armor, "TAG_CRAFT_MAA", 8);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f9180)]
        public static void sub_100F9180(in DispatcherCallbackArgs evt)
        {
            var v9 = 0;
            var dispIo = evt.GetDispIoD20Query();
            var v2 = dispIo;
            var v3 = (GameObjectBody) dispIo.obj;
            var v5 = evt.objHndCaller.GetStat(Stat.level_barbarian);
            if (v5 >= 2)
            {
                v9 = v5;
            }

            var v6 = evt.objHndCaller.GetStat(Stat.level_rogue);
            if (v6 >= 4)
            {
                v9 += v6;
            }

            int v7 = v3.GetStat(Stat.level_barbarian);
            if (v7 < 2)
            {
                v7 = 0;
            }

            int v8 = v3.GetStat(Stat.level_rogue);
            if (v8 >= 4)
            {
                v7 += v8;
            }

            if (v7 < v9 + 4)
            {
                v2.return_val = 0;
            }
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100faf60)]
        public static void sub_100FAF60(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var condArg1 = evt.GetConditionArg1();
            if ((dispIo.flags & D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT) != 0)
            {
                var v3 = GameSystems.Feat.GetFeatName((FeatId) condArg1);
                dispIo.bonlist.AddBonus(2, 0, 114, v3);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100fafe0)]
        public static void sub_100FAFE0(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(0);
            evt.SetConditionArg2(0);
        }

        [TempleDllLocation(0x100f9320)]
        private static bool CanUseFlurryOfBlows(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (condArg1 != 0)
            {
                var firstWeapon = GameSystems.D20.GetAttackWeapon(evt.objHndCaller, 1, 0);
                if (!IsWeaponAllowedForFlurryOfBlows(firstWeapon, evt.objHndCaller))
                {
                    return false;
                }

                var secondWeapon = GameSystems.D20.GetAttackWeapon(evt.objHndCaller, 2, 0);
                if (!IsWeaponAllowedForFlurryOfBlows(secondWeapon, evt.objHndCaller))
                {
                    return false;
                }

                return ClassConditions.FulfillsMonkArmorAndLoadRequirement(evt.objHndCaller);
            }

            return false;
        }

        [TempleDllLocation(0x100b7040)]
        private static bool IsWeaponAllowedForFlurryOfBlows(GameObjectBody obj, GameObjectBody wielder)
        {
            var result = true;
            if (obj != null)
            {
                var weaponType = obj.GetWeaponType();
                if (!GameSystems.Weapon.IsHalflingWeapon(weaponType)
                    || weaponType != WeaponType.quarterstaff && GameSystems.Item.GetWieldType(wielder, obj) != 0)
                {
                    result = false;
                }
            }

            return result;
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f9470)]
        public static void MonkFlurryToHitPenalty(in DispatcherCallbackArgs evt)
        {
            var v1 = -2;
            if (CanUseFlurryOfBlows /*0x100f9320*/(in evt))
            {
                var dispIo = evt.GetDispIoAttackBonus();
                var v3 = dispIo;
                var v4 = dispIo.attackPacket.flags;
                if (((v4 & D20CAF.RANGED) == 0))
                {
                    if ((v4 & D20CAF.FULL_ATTACK) != 0)
                    {
                        var v5 = evt.objHndCaller.GetStat(Stat.level_monk);
                        if (v5 < 9)
                        {
                            if (v5 >= 5)
                            {
                                v1 = -1;
                            }

                            v3.bonlist.AddBonus(v1, 0, 286);
                        }
                    }
                }
            }
        }


        [DispTypes(DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x100fa430)]
        public static void ImprovedTripAbilityCheckBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if ((dispIo.flags & SkillCheckFlags.UnderDuress) != 0)
            {
                dispIo.bonOut.AddBonusFromFeat(4, 0, 114, FeatId.IMPROVED_TRIP);
            }
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100fba00)]
        public static void TrackActivate(in DispatcherCallbackArgs evt)
        {
            evt.GetDispIoD20ActionTurnBased();
            GameUiBridge.ActivateTrack(evt.objHndCaller);
        }


        [DispTypes(DispatcherType.GetCriticalHitRange)]
        [TempleDllLocation(0x100f8320)]
        public static void ImprovedCriticalGetCritThreatRange(in DispatcherCallbackArgs evt)
        {
            WeaponType weaponType;

            var threatRangeSize = 1;
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = (WeaponType) evt.GetConditionArg2();
            var dispIo = evt.GetDispIoAttackBonus();
            var weapUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weapUsed != null)
            {
                weaponType = weapUsed.GetWeaponType();
                threatRangeSize = weapUsed.GetInt32(obj_f.weapon_crit_range);
            }
            else
            {
                weaponType = WeaponType.unarmed_strike_medium_sized_being;
            }

            if (weaponType == condArg2)
            {
                var v7 = GameSystems.Feat.GetFeatName((FeatId) condArg1);
                dispIo.bonlist.AddBonus(threatRangeSize, 0, 114, v7);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f8f40)]
        public static void sub_100F8F40(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            evt.SetConditionArg1(dispIo.data1 == 0 ? 1 : 0);
        }

        [TempleDllLocation(0x102ef174)]
        private static readonly int[] AnimalCompanionLevelRestrictions =
        {
            0,
            0,
            0,
            0,
            0,
            3,
            3,
            6,
            9,
            3,
        };

        [TempleDllLocation(0x102EF14C)]
        private static readonly int[] AnimalCompanionProtoIds =
        {
            14050,
            14049,
            14051,
            14056,
            14362,
            14090,
            14052,
            14053,
            14054,
            14055,
        };

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fc170)]
        public static void AnimalCompanionRadialMenu(in DispatcherCallbackArgs evt)
        {
            var drdLvl = evt.objHndCaller.GetStat(Stat.level_druid);
            var v2 = drdLvl;
            var animalCompLvl = drdLvl;
            var v3 = evt.objHndCaller.GetStat(Stat.level_ranger);
            if (v3 >= 4)
            {
                v2 += v3 / 2;
                animalCompLvl = v2;
            }

            if (v2 >= 1 && (evt.GetConditionArg(4)) == 0)
            {
                var parentEntry = RadialMenuEntry.CreateParent(D20CombatMessage.animal_companion);
                var parentIdx = GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry,
                    RadialMenuStandardNode.Class);
                var condArg1 = evt.GetConditionArg1();
                if ((condArg1 | evt.GetConditionArg2()) != 0)
                {
                    var dismissEntry = RadialMenuEntry.CreateAction(6001, D20ActionType.CLASS_ABILITY_SA, 18,
                        "TAG_CLASS_FEATURES_DRUID_ANIMAL_COMPANION");
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref dismissEntry, parentIdx);
                }
                else if (GameSystems.Party.AiFollowerCount <= 5)
                {
                    for (var i = 0; i < AnimalCompanionLevelRestrictions.Length; i++)
                    {
                        if (AnimalCompanionLevelRestrictions[i] < animalCompLvl)
                        {
                            AnimalCompanionGetRadialMenuOptions(evt.objHndCaller, i, parentIdx);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100fc0e0)]
        private static void AnimalCompanionGetRadialMenuOptions(GameObjectBody ObjHnd, int animalCompanionIdx,
            int parentIdx)
        {
            var protoId = AnimalCompanionProtoIds[animalCompanionIdx];
            var protoObj = GameSystems.Proto.GetProtoById((ushort) protoId);
            var companionName = GameSystems.MapObject.GetDisplayName(protoObj);

            var radMenuEntry = RadialMenuEntry.CreateAction(companionName, D20ActionType.CLASS_ABILITY_SA,
                8 + animalCompanionIdx, "TAG_CLASS_FEATURES_DRUID_ANIMAL_COMPANION");
            GameSystems.D20.RadialMenu.AddChildNode(ObjHnd, ref radMenuEntry, parentIdx);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f9870)]
        public static void StunningFistRadialMenu(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.STUNNING_FIST)
                || GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.STUNNING_ATTACKS))
            {
                var radMenuEntry = RadialMenuEntry.CreateAction(5047, D20ActionType.STUNNING_FIST, 0, "TAG_STUNNING_FIST");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry, RadialMenuStandardNode.Feats);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fa960)]
        public static void RapidShotRadialMenu(in DispatcherCallbackArgs evt)
        {
            int v2;

            var radMenuEntry = evt.CreateToggleForArg(0);
            var meslineKey = 5053;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            radMenuEntry.text = (string) meslineValue;
            radMenuEntry.helpSystemHashkey = "TAG_RAPID_SHOT";
            var v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Feats);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f8a10)]
        public static void AoOWillTake_Callback(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var obj = evt.GetConditionObjArg(1);
            var dispIo = evt.GetDispIoD20Query();
            if (obj != dispIo.obj && condArg1 > 0)
            {
                dispIo.return_val = 1;
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f8a70)]
        public static void AoOPerformed(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            evt.GetConditionArg2();
            evt.GetConditionArg3();
            var dispIo = evt.GetDispIoD20Signal();
            evt.SetConditionArg1(condArg1 - 1);
            var target = (GameObjectBody) dispIo.obj;
            evt.SetConditionObjArg(1, target);
        }

        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x100fb1f0)]
        public static void sub_100FB1F0(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var action = dispIo.action;

            var weeklyUses = GetWeeklyUsesOfRemoveDisease(evt.objHndCaller);
            var condArg1 = evt.GetConditionArg1();
            var bitmask = 7;
            for (var i = 0; i < weeklyUses; i++)
            {
                if ((condArg1 & bitmask) == 0)
                {
                    evt.SetConditionArg1(condArg1 | bitmask);
                    GameSystems.D20.D20SendSignal(action.d20ATarget, D20DispatcherKey.SIG_Remove_Disease);
                    return;
                }

                bitmask <<= 4;
            }
        }

        private static bool CanBeUsedForWeaponFinesse(GameObjectBody wielder, GameObjectBody weapon)
        {
            if (weapon == null)
            {
                return true; // Works for unarmed attacks
            }

            // Light weapons are fine
            if (GameSystems.Item.GetWieldType(wielder, weapon) == 0)
            {
                return true;
            }

            var weaponType = weapon.GetWeaponType();
            if (wielder.GetStat(Stat.size) > 4 && weaponType == WeaponType.rapier)
            {
                // Specifically allow rapiers. TODO: The size category check is fishy!
                return true;
            }

            // Spiked Chain is explicitly whitelisted
            if (weaponType == WeaponType.spike_chain)
            {
                return true;
            }

            return false;
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f80c0)]
        [TemplePlusLocation("ability_fixes.cpp:157")]
        public static void WeaponFinesseToHitBonus(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoAttackBonus();
            var weapon = dispIo.attackPacket.GetWeaponUsed();

            if (!CanBeUsedForWeaponFinesse(evt.objHndCaller, weapon))
            {
                // TODO: Should likely print a zero bonus line here stating weapon finesse can't be used (and why)
                return;
            }

            var dexMod = evt.objHndCaller.GetStat(Stat.dex_mod);
            var strMod = evt.objHndCaller.GetStat(Stat.str_mod);

            // TODO: Bad check. It should instead check the bonus list for the actual strength bonus used, which might be capped for some reason
            if (dexMod > strMod)
            {
                var featName = GameSystems.Feat.GetFeatName((FeatId) condArg1);
                dispIo.bonlist.AddCap(2, 0, 114, featName);
                dispIo.bonlist.AddBonus(dexMod, 3, 104, featName);

                // TP Fix: Strip the actual negative strength bonus which might already been applied here
                if (strMod < 0)
                {
                    dispIo.bonlist.ModifyBonus(-strMod, 2, 103);
                }

                var shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.Shield);
                if (shield == null)
                {
                    shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);
                }

                if (shield != null && shield.type == ObjectType.armor)
                {
                    var skillCheckPenalty = GameSystems.D20.GetArmorSkillCheckPenalty(shield);
                    var shieldName = GameSystems.MapObject.GetDisplayName(shield, evt.objHndCaller);
                    dispIo.bonlist.AddBonus(skillCheckPenalty, 0, 125, shieldName);
                }
            }
        }

        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x100faa90)]
        public static void DetectEvilActionFrame(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            dispIo.action.d20ATarget.AddCondition(StatusEffects.DetectingEvil);
            dispIo.returnVal = 0;
        }


        [DispTypes(DispatcherType.ReflexThrow)]
        [TempleDllLocation(0x100fa470)]
        public static void ImprovedEvasionReflexThrow(in DispatcherCallbackArgs evt)
        {
            if (!evt.objHndCaller.IsWearingLightArmorOrLess())
            {
                return;
            }

            var dispIo = evt.GetDispIoReflexThrow();
            if (!dispIo.throwResult)
            {
                dispIo.throwResult = true; // TODO was -1 to probably indicate evasion was used???
                dispIo.damageMesLine = 108;
                switch (dispIo.reduction)
                {
                    case D20SavingThrowReduction.None:
                        dispIo.effectiveReduction = 0;
                        break;
                    case D20SavingThrowReduction.Half:
                        dispIo.effectiveReduction = 50;
                        break;
                    case D20SavingThrowReduction.Quarter:
                        dispIo.effectiveReduction = 75;
                        break;
                    default:
                        dispIo.effectiveReduction = 100;
                        break;
                }
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb070)]
        public static void PaladinRemoveDiseaseRadial(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                return;
            }

            var weeklyUses = GetWeeklyUsesOfRemoveDisease(evt.objHndCaller);
            var condArg1 = evt.GetConditionArg1();

            var bitmask = 0xF;
            for (var i = 0; i < weeklyUses; i++)
            {
                if ((condArg1 & bitmask) == 0)
                {
                    var radMenuEntry = RadialMenuEntry.CreateAction(5063, D20ActionType.REMOVE_DISEASE, 0,
                        "TAG_CLASS_FEATURES_PALADIN_REMOVE_DISEASE");
                    GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                        RadialMenuStandardNode.Class);
                    return;
                }

                bitmask <<= 4;
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f84c0)]
        [TemplePlusLocation("condition.cpp:451")]
        public static void PowerAttackToHitPenalty(in DispatcherCallbackArgs evt)
        {
            var powerAttackAmt = evt.GetConditionArg1();

            if (powerAttackAmt == 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoAttackBonus();

            // ignore ranged weapons
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                return;
            }

            // get wield type
            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
            var wieldType = GameSystems.Item.GetWieldType(evt.objHndCaller, weaponUsed, true);
            // the wield type if the weapon is not enlarged along with the critter
            var wieldTypeWeaponModified = GameSystems.Item.GetWieldType(evt.objHndCaller, weaponUsed);
            var modifiedByEnlarge = wieldType != wieldTypeWeaponModified;

            dispIo.bonlist.AddBonusFromFeat(-powerAttackAmt, 0, 114,
                FeatId.POWER_ATTACK); // todo convenience disabling of to hit penalty when not dual wielding
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100fd4f0)]
        public static void AnimalCompanionSummonDismiss(in DispatcherCallbackArgs evt)
        {
            var action = evt.GetDispIoD20ActionTurnBased().action;
            var summonKey = action.data1;
            if (summonKey >= 8 && summonKey <= 18)
            {
                var companion = evt.GetConditionObjArg(0);
                if (companion != null)
                {
                    if (summonKey == 18)
                    {
                        var v4 = GameSystems.D20.Combat.GetCombatMesLine(0x1774);
                        var v5 = GameSystems.MapObject.GetDisplayName(companion);
                        var v6 = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage
                            .are_you_sure_you_want_to_dismiss);
                        var conditionAttachment = evt.subDispNode.condNode;
                        var master = evt.objHndCaller;
                        GameUiBridge.Confirm($"{v6}{v5}{v4}", null, false, buttonIdx =>
                        {
                            if (buttonIdx == 0)
                            {
                                AnimalCompanionRunoff(conditionAttachment, master, companion);
                            }
                        });
                    }
                }
                else
                {
                    var conditionAttachment = evt.subDispNode.condNode;
                    var master = evt.objHndCaller;
                    var animalCompanionIdx = action.data1 - 8;
                    var message = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage
                        .name_your_animal_companion);
                    GameUiBridge.ShowTextEntry(message, (name, confirmed) =>
                    {
                        if (confirmed)
                        {
                            AnimalCompanionSummonCallback(name, conditionAttachment,
                                master, animalCompanionIdx);
                        }
                    });
                }
            }
        }

        [TempleDllLocation(0x100fd3f0)]
        [TemplePlusLocation("animalcompanion.cpp")]
        private static void AnimalCompanionSummonCallback(string nameIn, ConditionAttachment conditionAttachment,
            GameObjectBody master, int animalCompanionIdx)
        {
            if (AnimalCompanionTrimName(nameIn, out var nameTrimmed))
            {
                AnimalCompanionAdd(conditionAttachment, master, animalCompanionIdx, nameTrimmed);
            }
        }

        [TempleDllLocation(0x100fc520)]
        private static void AnimalCompanionAdd(ConditionAttachment conditionAttachment, GameObjectBody leader,
            int animalCompanionIdx, string customName)
        {
            var companionProtoId = (ushort) AnimalCompanionProtoIds[animalCompanionIdx];
            var companion = GameSystems.MapObject.CreateObject(companionProtoId, leader.GetLocationFull());
            GameSystems.Critter.GenerateHp(companion);
            if (GameSystems.Critter.FollowerAdd(companion, leader, true, true))
            {
                // TODO: Arguments... (object to be precise)!
                companion.AddCondition(AnimalCompanionAnimal, AnimalCompanionLevelRestrictions[animalCompanionIdx],
                    leader);
                GameUiBridge.UpdatePartyUi();
                GameSystems.AI.ForceSpreadOut(companion);

                // Here we'd need to set the handle of the companion in args 0+1
                // CondNodeSetArg/*0x100e1ad0*/(pSubDispListNode.condNode, 0, SHIDWORD(handleNew));
                // CondNodeSetArg/*0x100e1ad0*/(pSubDispListNode.condNode, 1, handleNew);
                throw new NotImplementedException();

                GameSystems.ParticleSys.CreateAtObj("sp-Summon Natures Ally I", companion);
                var descId = GameSystems.Description.Create(customName);
                companion.SetInt32(obj_f.description, descId);
                GameSystems.Reaction.AdjustReaction(companion, leader, 100);
                GameSystems.MapObject.SetRotation(companion, companion.RotationTo(leader));
            }
            else
            {
                GameSystems.Object.Destroy(companion);
            }
        }

        [TempleDllLocation(0x100fc4e0)]
        private static bool AnimalCompanionTrimName(string nameIn, out string nameOut)
        {
            nameIn = nameIn.Trim();
            if (nameIn.Length > 0)
            {
                nameOut = nameIn;
                return true;
            }

            nameOut = null;
            return false;
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f82a0)]
        public static void WeaponSpecializationDamageBonus(in DispatcherCallbackArgs evt)
        {
            WeaponType v6;

            var condArg1 = evt.GetConditionArg1();
            var condArg2 = (WeaponType) evt.GetConditionArg2();
            var dispIo = evt.GetDispIoDamage();
            var v4 = dispIo;
            var v5 = dispIo.attackPacket.GetWeaponUsed();
            if (v5 != null)
            {
                v6 = v5.GetWeaponType();
            }
            else
            {
                v6 = WeaponType.unarmed_strike_medium_sized_being;
            }

            if (v6 == condArg2)
            {
                var v7 = GameSystems.Feat.GetFeatName((FeatId) condArg1);
                v4.damage.AddDamageBonus(2, 0, 114, v7);
            }
        }


        [DispTypes(DispatcherType.GetBonusAttacks)]
        [TempleDllLocation(0x100fc8b0)]
        public static void AnimalCompanionNumAttacksBonus(in DispatcherCallbackArgs evt)
        {
            int v1 = AnimalCompanionRefreshHp(in evt, evt.objHndCaller);
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            if (((dispIo.action.d20Caf & D20CAF.RANGED) == 0) && v1 >= 9)
            {
                ++dispIo.returnVal;
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f8cc0)]
        [TemplePlusLocation("condition.cpp:482")]
        public static void CastDefensivelySpellInterrupted(in DispatcherCallbackArgs evt)
        {
            var isSet = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Query();

            if (dispIo.return_val == 1)
            {
                return; // already interrupted by sthg else
            }

            if (isSet == 0)
            {
                return; // not casting defensively
            }

            if (!GameSystems.Combat.IsCombatActive())
            {
                return; // forego this outside of combat
            }

            // check if no threatening melee enemies - if so, disregard casting defensively (since it's just annoying micromanagement!!!)
            var enemiesCanMelee = GameSystems.Combat.GetEnemiesCanMelee(evt.objHndCaller);
            if (enemiesCanMelee.Count == 0)
            {
                return;
            }

            var spellData = (D20SpellData) dispIo.obj;
            if (spellData == default)
            {
                return;
            }

            // odd, but that's where it was in the original code...
            if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.COMBAT_CASTING))
            {
                evt.objHndCaller.AddCondition(CombatCasting, 0);
            }

            var rollRes = GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration,
                15 + spellData.spellSlotLevel, out _, SkillCheckFlags.UnderDuress);
            if (!rollRes)
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(25, evt.objHndCaller, null);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 58);
                dispIo.return_val = 1;
            }
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100f9d00)]
        public static void SmiteEvilD20A(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var action = dispIo.action;
            if (action.d20ATarget == null
                || !action.d20ATarget.IsCritter()
                || GameSystems.D20.D20QueryWithObject(action.d20ATarget,
                    D20DispatcherKey.QUE_CanBeAffected_PerformAction,
                    action,
                    defaultResult: 1) != 0)
            {
                var condArg1 = evt.GetConditionArg1();
                evt.SetConditionArg1(condArg1 - 1);
                if (action.d20ATarget.HasEvilAlignment())
                {
                    action.d20APerformer.AddCondition(StatusEffects.SmitingEvil);
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f90c0)]
        public static void sub_100F90C0(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoDamage();
                if (dispIo.attackPacket.GetWeaponUsed() == null
                    && !GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.SIMPLE_WEAPON_PROFICIENCY_MONK))
                {
                    dispIo.damage.SetDamageType(DamageType.Bludgeoning);
                }
            }
        }

        private static readonly IImmutableDictionary<SkillId, FeatId> SkillFocusFeats = new Dictionary<SkillId, FeatId>
        {
            {SkillId.appraise, FeatId.SKILL_FOCUS_APPRAISE},
            {SkillId.bluff, FeatId.SKILL_FOCUS_BLUFF},
            {SkillId.concentration, FeatId.SKILL_FOCUS_CONCENTRATION},
            {SkillId.diplomacy, FeatId.SKILL_FOCUS_DIPLOMACY},
            {SkillId.disable_device, FeatId.SKILL_FOCUS_DISABLE_DEVICE},
            {SkillId.gather_information, FeatId.SKILL_FOCUS_GATHER_INFORMATION},
            {SkillId.heal, FeatId.SKILL_FOCUS_HEAL},
            {SkillId.hide, FeatId.SKILL_FOCUS_HIDE},
            {SkillId.intimidate, FeatId.SKILL_FOCUS_INTIMIDATE},
            {SkillId.listen, FeatId.SKILL_FOCUS_LISTEN},
            {SkillId.move_silently, FeatId.SKILL_FOCUS_MOVE_SILENTLY},
            {SkillId.open_lock, FeatId.SKILL_FOCUS_OPEN_LOCK},
            {SkillId.pick_pocket, FeatId.SKILL_FOCUS_SLIGHT_OF_HAND},
            {SkillId.search, FeatId.SKILL_FOCUS_SEARCH},
            {SkillId.sense_motive, FeatId.SKILL_FOCUS_SENSE_MOTIVE},
            {SkillId.spellcraft, FeatId.SKILL_FOCUS_SPELLCRAFT},
            {SkillId.spot, FeatId.SKILL_FOCUS_SPOT},
            {SkillId.tumble, FeatId.SKILL_FOCUS_TUMBLE},
            {SkillId.use_magic_device, FeatId.SKILL_FOCUS_USE_MAGIC_DEVICE},
            {SkillId.wilderness_lore, FeatId.SKILL_FOCUS_SURVIVAL},
            {SkillId.perform, FeatId.SKILL_FOCUS_PERFORMANCE}
        }.ToImmutableDictionary();

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100fac30)]
        public static void sub_100FAC30(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var skill = (SkillId) evt.GetConditionArg1();
            if (!SkillFocusFeats.TryGetKey(skill, out var featId))
            {
                return;
            }

            var dispIo = evt.GetDispIoCondStruct();
            // TODO: This actually seems fishy because we're using a skillId from arg1 to compare it against arg1 of the incoming condition as a feat id, what?!?!
            if (dispIo.condStruct == (ConditionSpec) data && dispIo.arg1 == (int) featId)
            {
                dispIo.outputFlag = 0;
            }
        }

        [DispTypes(DispatcherType.GetNumAttacksBase)]
        [TempleDllLocation(0x100fd1c0)]
        public static void ImprovedTWF(in DispatcherCallbackArgs evt)
        {
            var v1 = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponPrimary);
            var v2 = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // TODO: This check seems dubious!
            if (v1 != v2 && v1 != null && v2 != null)
            {
                if ((v1.WeaponFlags & WeaponFlag.RANGED_WEAPON) == 0 && v2.type != ObjectType.armor)
                {
                    ++dispIo.returnVal;
                }
            }
        }

        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100faac0)]
        public static void KiStrikeOnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var damagePacket = dispIo.damage;
            if (GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponPrimary) == null &&
                GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary) == null)
            {
                var monkLevels = evt.objHndCaller.GetStat(Stat.level_monk);
                var attackPower = D20AttackPower.MAGIC;
                var mesLine = 331;

                if (monkLevels >= 10)
                {
                    mesLine = 332;
                    attackPower |= D20AttackPower.LAW;
                }

                // This is a Co8 hack/extension, and the mes-key only exists in Co8
                if (monkLevels >= 16)
                {
                    mesLine = 334;
                    attackPower |= D20AttackPower.ADAMANTIUM;
                }

                damagePacket.AddAttackPower(attackPower);
                damagePacket.bonuses.zeroBonusSetMeslineNum(mesLine);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100fca90)]
        public static void sub_100FCA90(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(0);
            evt.SetConditionArg2(0);
            evt.SetConditionArg(4, 0);
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100fc980)]
        public static void AnimalCompanionStatBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            int v2 = AnimalCompanionRefreshHp(in evt, evt.objHndCaller);
            dispIo.bonlist.AddBonus(v2 / 3, 0, 275);
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100f92a0)]
        public static void UncannyDodgeSaveThrowBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var v2 = evt.objHndCaller.GetStat(Stat.level_barbarian);
            if (evt.objHndCaller.GetStat(Stat.level_rogue) >= 2)
            {
                v2 = v2 + evt.objHndCaller.GetStat(Stat.level_rogue) - 1;
            }

            if (v2 >= 10)
            {
                if ((dispIo.flags & D20SavingThrowFlag.CHARM) != 0)
                {
                    var v3 = GameSystems.Feat.GetFeatName(FeatId.UNCANNY_DODGE);
                    dispIo.bonlist.AddBonus(1, 8, 114, v3);
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f8660)]
        public static void PowerAttackSetViaSignal(in DispatcherCallbackArgs evt)
        {
            var v1 = DispatcherExtensions.DispatchToHitBonusBase(evt.objHndCaller);
            var v2 = evt.GetDispIoD20Signal().data1;
            if (v2 > v1)
            {
                v2 = v1;
            }

            if (v2 < 0)
            {
                v2 = 0;
            }

            evt.SetConditionArg1(v2);
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100f8040)]
        public static void FightDefensivelyAcBonus(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0 && (evt.GetConditionArg2()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                dispIo.bonlist.AddBonus(2, 8, 116);
                if (evt.objHndCaller.dispatch1ESkillLevel(SkillId.tumble, null, 0) >= 5)
                {
                    dispIo.bonlist.AddBonus(1, 8, 159);
                }
            }
        }

        private const int WildShapeActionBitmask = 1 << 24;

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fbb20)]
        [TemplePlusLocation("condition.cpp:473")]
        public static void WildShapeRadialMenu(in DispatcherCallbackArgs evt)
        {
            var wildshapeMain = RadialMenuEntry.CreateParent(5076);

            var druid = evt.objHndCaller;
            var wsId = GameSystems.D20.RadialMenu.AddToStandardNode(druid, ref wildshapeMain,
                RadialMenuStandardNode.Class);

            // if wild shape active - add "Deactivate" node
            if (evt.GetConditionArg3() != 0)
            {
                var wsDeactivate = RadialMenuEntry.CreateAction(5077, D20ActionType.CLASS_ABILITY_SA,
                    (int) WildShapeProtoIdx.Deactivate, "TAG_CLASS_FEATURES_DRUID_WILD_SHAPE");
                wsDeactivate.AddAsChild(druid, wsId);
                return;
            }

            // else add the WS options
            void AddOption(WildShapeProtoIdx optionIdx, int parentIdx)
            {
                var wsProto = DruidWildShapes.Options[optionIdx].protoId;
                var protoCode = (int) optionIdx | WildShapeActionBitmask;

                var wsOption = RadialMenuEntry.CreateAction(0, D20ActionType.CLASS_ABILITY_SA, protoCode,
                    "TAG_CLASS_FEATURES_DRUID_WILD_SHAPE");

                var protoHandle = GameSystems.Proto.GetProtoById((ushort) wsProto);
                wsOption.text = GameSystems.MapObject.GetDisplayName(protoHandle);
                GameSystems.D20.RadialMenu.AddChildNode(druid, ref wsOption, parentIdx);
            }

            var druidLvl = druid.GetStat(Stat.level_druid);
            foreach (var kvp in DruidWildShapes.Options)
            {
                if (druidLvl >= kvp.Value.minLvl && kvp.Value.monCat == MonsterCategory.animal)
                {
                    AddOption(kvp.Key, wsId);
                }
            }

            if (GameSystems.D20.D20Query(druid, D20DispatcherKey.QUE_Wearing_Ring_of_Change))
            {
                AddOption(WildShapeProtoIdx.Hill_Giant, wsId);
            }

            // elementals
            if (druidLvl >= 16)
            {
                var wsElem = RadialMenuEntry.CreateParent(5118);
                var elemId = GameSystems.D20.RadialMenu.AddChildNode(druid, ref wsElem, wsId);

                foreach (var kvp in DruidWildShapes.Options)
                {
                    if (druidLvl >= kvp.Value.minLvl && kvp.Value.monCat == MonsterCategory.elemental)
                    {
                        AddOption(kvp.Key, elemId);
                    }
                }
            }
        }

        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100f9ba0)]
        public static void DivineGraceSave(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoSavingThrow();
            if (!GameSystems.D20.D20Query(args.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                var v2 = args.objHndCaller.GetStat(Stat.charisma);
                var v3 = D20StatSystem.GetModifierForAbilityScore(v2);
                dispIo.bonlist.AddBonus(v3, 0, 197);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f7e20)]
        public static void CombatExpertiseToHitPenalty(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if ((condArg1) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                if (((dispIo.attackPacket.flags & D20CAF.RANGED) == 0))
                {
                    var v3 = GameSystems.Feat.GetFeatName(FeatId.COMBAT_EXPERTISE);
                    dispIo.bonlist.AddBonus(-condArg1, 0, 114, v3);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f8220)]
        public static void WeaponFocusToHitBonus(in DispatcherCallbackArgs evt)
        {
            WeaponType itemWeaponType;

            var condArg1 = (FeatId) evt.GetConditionArg1();
            var condArg2 = (WeaponType) evt.GetConditionArg2();
            var dispIo = evt.GetDispIoAttackBonus();
            var v4 = dispIo;
            var v5 = dispIo.attackPacket.GetWeaponUsed();
            if (v5 != null)
            {
                itemWeaponType = v5.GetWeaponType();
            }
            else
            {
                itemWeaponType = WeaponType.unarmed_strike_medium_sized_being;
            }

            if (itemWeaponType == condArg2)
            {
                var featName = GameSystems.Feat.GetFeatName(condArg1);
                v4.bonlist.AddBonus(1, 0, 114, featName);
            }
        }


        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x100fc150)]
        public static void sub_100FC150(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg(4, 0);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f8be0)]
        [TemplePlusLocation("condition.cpp:481")]
        public static void CastDefensivelyActionTriggersAooQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();

            var isSet = evt.GetConditionArg1() != 0;
            var action = (D20Action) dispIo.obj;

            if (action.d20ActType == D20ActionType.CAST_SPELL && isSet)
            {
                dispIo.return_val = 0;
                return;
            }

            if (action.d20ActType == D20ActionType.USE_ITEM)
            {
                var invIdx = action.d20SpellData.itemSpellData;
                var item = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, invIdx);
                if (item == null)
                {
                    dispIo.return_val = 0;
                    return;
                }

                if (item.type == ObjectType.generic || item.type == ObjectType.weapon)
                {
                    dispIo.return_val = 0;
                    return;
                }

                if (item.type != ObjectType.scroll)
                {
                    Logger.Warn("CastDefensivelyAooTrigger: Unexpected item type {0}", item.type);
                }

                // for the rest of the item types (should only be ObjectType.scroll?)
                if (isSet)
                {
                    dispIo.return_val = 0;
                }

                // otherwise it will be as default (1)
            }
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100fab50)]
        public static void DefensiveRollOnDamage(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoDamage();
            var damagePacket = dispIo.damage;
            if ((condArg1) != 0)
            {
                evt.SetConditionArg1(0);
                var v3 = evt.objHndCaller.GetInt32(obj_f.hp_damage);
                if (evt.objHndCaller.GetInt32(obj_f.hp_pts) - v3 <= damagePacket.finalDamage)
                {
                    var actionType = dispIo.attackPacket.d20ActnType;
                    if (actionType != D20ActionType.CAST_SPELL
                        && actionType != D20ActionType.TOUCH_ATTACK
                        && actionType != D20ActionType.TURN_UNDEAD
                        && actionType != D20ActionType.DEATH_TOUCH
                        && actionType != D20ActionType.STUNNING_FIST
                        && actionType != D20ActionType.SMITE_EVIL
                        && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Flatfooted))
                    {
                        if (GameSystems.D20.Combat.SavingThrow(evt.objHndCaller, null,
                            damagePacket.finalDamage, SavingThrowType.Reflex))
                        {
                            damagePacket.AddModFactor(0.5f, DamageType.Unspecified, 112);
                        }
                    }
                }
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100facc0)]
        public static void SkillFocusSkillLevelCallback(in DispatcherCallbackArgs evt)
        {
            var skillId = (SkillId) evt.GetConditionArg1();
            var dispIo = evt.GetDispIoObjBonus();
            var usedSkillId = evt.GetSkillIdFromDispatcherKey();
            if (usedSkillId == skillId && SkillFocusFeats.TryGetValue(skillId, out var featId))
            {
                dispIo.bonOut.AddBonusFromFeat(3, 0, 114, featId);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f8000)]
        public static void FightDefensivelyToHitPenalty(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                if (((dispIo.attackPacket.flags & D20CAF.RANGED) == 0))
                {
                    dispIo.bonlist.AddBonus(-4, 0, 116);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f9040)]
        public static void sub_100F9040(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                if ((dispIo.attackPacket.GetWeaponUsed() == null) && !GameSystems.Feat.HasFeat(evt.objHndCaller,
                                                                      FeatId.SIMPLE_WEAPON_PROFICIENCY_MONK)
                                                                  && !GameSystems.Feat.HasFeat(evt.objHndCaller,
                                                                      FeatId.IMPROVED_UNARMED_STRIKE))
                {
                    dispIo.bonlist.AddBonus(-4, 0, 157);
                }
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb5d0)]
        public static void CraftRodRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage.craft_rod, "TAG_CRAFT_ROD", 4);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f8c20)]
        public static void CastDefensivelyRadial(in DispatcherCallbackArgs evt)
        {
            int v2;

            var radMenuEntry = evt.CreateToggleForArg(0);
            var meslineKey = 5013;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            radMenuEntry.text = (string) meslineValue;
            radMenuEntry.helpSystemHashkey = "TAG_RADIAL_MENU_CAST_DEFENSIVELY";
            var v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Options);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
        }


        [DispTypes(DispatcherType.MaxHP)]
        [TempleDllLocation(0x100fc0b0)]
        public static void sub_100FC0B0(in DispatcherCallbackArgs evt)
        {
            var featId = (FeatId) evt.GetConditionArg1();
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonusFromFeat(3, 0, 114, featId);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100fd030)]
        public static void ManyshotAttackMadeHandler(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var evtObjDamage = (DispIoDamage) dispIo.obj;
            var caf = evtObjDamage.attackPacket.flags;
            if ((caf & D20CAF.MANYSHOT) != 0)
            {
                if ((caf & D20CAF.HIT) != 0)
                {
                    var descrToChange = GameSystems.MapObject.GetDisplayNameForParty(evt.objHndCaller);
                    var attackerName = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.uses_Manyshot);
                    GameSystems.RollHistory.CreateFromFreeText($"{descrToChange} {attackerName}");

                    var actionType = evtObjDamage.attackPacket.d20ActnType;
                    var victim = evtObjDamage.attackPacket.victim;
                    var flags = evtObjDamage.attackPacket.flags & ~(D20CAF.MANYSHOT | D20CAF.CRITICAL) |
                                D20CAF.NO_PRECISION_DAMAGE;
                    var d20data = evtObjDamage.attackPacket.dispKey;
                    evtObjDamage.attackPacket.flags = flags;
                    var attacker = evtObjDamage.attackPacket.attacker;
                    GameSystems.D20.Combat.DealAttackDamage(attacker, victim, d20data, flags, actionType);

                    var ammoItem = evtObjDamage.attackPacket.ammoItem;
                    ammoItem.SetQuantity(ammoItem.GetQuantity() - 1);
                }
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f86b0)]
        public static void CleaveDroppedEnemy(in DispatcherCallbackArgs evt)
        {
            var targetsCleaved = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Signal();
            var dispIoDmg = (DispIoDamage) dispIo.obj;
            var weaponUsed = dispIoDmg.attackPacket.GetWeaponUsed();
            if (GameSystems.D20.D20QueryWithObject(evt.objHndCaller, D20DispatcherKey.QUE_Weapon_Is_Mighty_Cleaving,
                    weaponUsed) != 0)
            {
                if (targetsCleaved >= 2)
                {
                    return;
                }
            }
            else if (targetsCleaved != 0)
            {
                return;
            }

            var caf = dispIoDmg.attackPacket.flags;
            if ((caf & D20CAF.RANGED) == 0 && GameSystems.D20.Actions.IsOkayToCleave(evt.objHndCaller))
            {
                var cleaveTarget = GameSystems.D20.Combat.GetCleaveTarget(evt.objHndCaller);
                if (cleaveTarget != null)
                {
                    var targetLoc = cleaveTarget.GetLocationFull();
                    var data1 = dispIoDmg.attackPacket.dispKey; // This is the attack code
                    D20ActionCallbacks.InsertD20Action(evt.objHndCaller, D20ActionType.CLEAVE, data1, cleaveTarget,
                        targetLoc, 0);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x17, evt.objHndCaller, null);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 36);
                    evt.SetConditionArg1(targetsCleaved + 1);
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100f9430)]
        public static void sub_100F9430(in DispatcherCallbackArgs evt)
        {
            var v1 = evt.objHndCaller.GetStat(Stat.level_monk);
            evt.SetConditionArg1(v1 + 10);
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f7f60)]
        public static void FightDefensivelyRadialMenu(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = evt.CreateToggleForArg(0);
            var meslineKey = 5010;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            radMenuEntry.text = meslineValue;
            radMenuEntry.helpSystemHashkey = "TAG_RADIAL_MENU_FIGHT_DEFENSIVELY";
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Options);
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100f7cd0)]
        public static void Dodge_ACBonus_Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var dodgeAppliedAgainst = evt.GetConditionObjArg(1);
            if (dodgeAppliedAgainst == null)
            {
                dodgeAppliedAgainst = dispIo.attackPacket.attacker;
                evt.SetConditionObjArg(1, dodgeAppliedAgainst);
            }

            if (dodgeAppliedAgainst == dispIo.attackPacket.attacker)
            {
                dispIo.bonlist.AddBonusFromFeat(1, 8, 114, FeatId.DODGE);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100fad90)]
        public static void PointBlankShotDamage(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoDamage();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                if (args.objHndCaller.DistanceToInFeetClamped(dispIo.attackPacket.victim) < 30F)
                {
                    var v2 = GameSystems.Feat.GetFeatName(FeatId.POINT_BLANK_SHOT);
                    dispIo.damage.AddDamageBonus(1, 0, 114, v2);
                }
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb770)]
        public static void CraftStaffRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage._stunned, "TAG_CRAFT_STAFF", 6);
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100f9220)]
        public static void UncannyDodgeAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v2 = evt.objHndCaller.GetStat(Stat.level_barbarian);
            if (evt.objHndCaller.GetStat(Stat.level_rogue) >= 2)
            {
                v2 = v2 + evt.objHndCaller.GetStat(Stat.level_rogue) - 1;
            }

            if (v2 >= 10)
            {
                var v3 = dispIo.attackPacket.flags;
                if (((v3 & D20CAF.TRAP) != 0))
                {
                    var v4 = GameSystems.Feat.GetFeatName(FeatId.UNCANNY_DODGE);
                    dispIo.bonlist.AddBonus(1, 8, 114, v4);
                }
            }
        }


        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100fbc60)]
        [TemplePlusLocation("condition.cpp:474")]
        public static void WildShapeCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var druidLvl = evt.objHndCaller.GetStat(Stat.level_druid);

            var action = dispIo.action;
            if ((action.data1 & WildShapeActionBitmask) != 0)
            {
                if (evt.GetConditionArg3() != 0) // already polymorphed
                    return;

                var optionId = (WildShapeProtoIdx) (action.data1 & ~WildShapeActionBitmask);
                var spec = DruidWildShapes.Options[optionId];
                if (druidLvl < spec.minLvl && spec.minLvl != -1)
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                }

                var numTimes = evt.GetConditionArg1();
                if (spec.monCat == MonsterCategory.elemental)
                {
                    numTimes >>= 8;
                    if (numTimes <= 0)
                        dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                }
                else
                {
                    // normal animal (or plant)
                    numTimes &= 0xFF;
                    if (numTimes <= 0)
                        dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                }
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f9a10)]
        [TemplePlusLocation("condition.cpp:494")]
        public static void SneakAttackDamageBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var atkPkt = dispIo.attackPacket;
            var tgt = atkPkt.victim;

            if (tgt == null)
            {
                return;
            }

            if (atkPkt.attacker == null)
            {
                Logger.Error("SneakAttackDamage: Error! Null attacker in attack packet");
                return;
            }

            // imprecise attacks cannot sneak attack
            if ((atkPkt.flags & D20CAF.NO_PRECISION_DAMAGE) != D20CAF.NONE)
            {
                return;
            }

            // limit to 30'
            var within30Feet = evt.objHndCaller.DistanceToObjInFeet(tgt) < 30.0f;

            // See if it is a critical and if criticals cause sneak attacks
            bool sneakAttackFromCrit = false;
            if ((atkPkt.flags & D20CAF.CRITICAL) != 0)
            {
                var result = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Sneak Attack Critical");
                if (result > 0)
                {
                    sneakAttackFromCrit = true;
                }
            }

            bool sneakAttackCondition = (atkPkt.flags & D20CAF.FLANKED) != 0
                                        || GameSystems.D20.D20Query(tgt, D20DispatcherKey.QUE_SneakAttack)
                                        || GameSystems.D20.D20QueryWithObject(atkPkt.attacker,
                                            D20DispatcherKey.QUE_OpponentSneakAttack, dispIo) != 0
                                        || !GameSystems.Critter.CanSense(tgt, atkPkt.attacker);

            // From the SRD:  The rogue must be able to see the target well enough to pick out a vital
            // spot and must be able to reach such a spot. A rogue cannot sneak attack while striking a
            // creature with concealment or striking the limbs of a creature whose vitals are beyond reach.
            bool canSenseTarget = GameSystems.Critter.CanSense(evt.objHndCaller, tgt);

            if ((sneakAttackCondition && canSenseTarget && within30Feet) || sneakAttackFromCrit)
            {
                // get sneak attack dice (NEW! now via query, for prestige class modularity)
                var sneakAttackDice = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Sneak Attack Dice");
                var sneakAttackDmgBonus = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Sneak Attack Bonus");

                if (sneakAttackDice <= 0)
                    return;

                if (GameSystems.D20.D20Query(tgt, D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits))
                {
                    dispIo.damage.bonuses.zeroBonusSetMeslineNum(325);
                    return;
                }


                var sneakDmgDice = new Dice(sneakAttackDice, 6, sneakAttackDmgBonus);
                if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.DEADLY_PRECISION))
                {
                    sneakDmgDice = new Dice(sneakAttackDice, 5, 1 * sneakAttackDice + sneakAttackDmgBonus);
                }

                dispIo.damage.AddDamageDice(sneakDmgDice, DamageType.Unspecified, 106);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 90); // Sneak Attack!
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(26, evt.objHndCaller, tgt);

                GameSystems.D20.D20SignalPython(evt.objHndCaller, "Sneak Attack Damage Applied");

                // crippling strike ability loss
                if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.CRIPPLING_STRIKE))
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(47, evt.objHndCaller, tgt);
                    tgt.AddCondition("Damage_Ability_Loss", 0,
                        2); // note: vanilla had a bug (did 1 damage instead of 2)
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 96); // Ability Loss
                }
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100fc7f0)]
        public static void AnimalCompanionBeginRound(in DispatcherCallbackArgs evt)
        {
            var master = evt.GetConditionObjArg(1);
            if (master == null || !GameSystems.Party.IsInParty(master))
            {
                AnimalCompanionRunoff(evt.subDispNode.condNode, master, evt.objHndCaller);
            }

            AnimalCompanionRefreshHp(in evt, evt.objHndCaller);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100fac80)]
        public static void sub_100FAC80(in DispatcherCallbackArgs evt)
        {
            var featId = (FeatId) evt.GetConditionArg1();

            // This is weird, replace arg1 with the skill id rather than the feat
            foreach (var kvp in SkillFocusFeats)
            {
                if (kvp.Value == featId)
                {
                    evt.SetConditionArg1((int) kvp.Key);
                    return;
                }
            }
        }

        [DispTypes(DispatcherType.InitiativeMod)]
        [TempleDllLocation(0x100f83d0)]
        public static void ImprovedInitiativeCallback(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonusFromFeat(4, 0, 114, (FeatId) condArg1);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100fa660)]
        public static void sub_100FA660(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg1();
            evt.SetConditionArg2(2);
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.BeginRound)]
        [TempleDllLocation(0x100f8af0)]
        public static void AooReset(in DispatcherCallbackArgs evt)
        {
            var numAoosRem = 1;
            if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.COMBAT_REFLEXES))
            {
                var dexScore = evt.objHndCaller.GetStat(Stat.dexterity);
                numAoosRem = D20StatSystem.GetModifierForAbilityScore(dexScore);
                if (numAoosRem < 1)
                {
                    numAoosRem = 1;
                }
            }

            evt.SetConditionArg1(numAoosRem);
            evt.SetConditionArg2(0);
            evt.SetConditionArg3(0);
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f88c0)]
        [TemplePlusLocation("condition.cpp:420")]
        public static void TwoWeaponFightingBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();

            var feat = (FeatId) evt.GetConditionArg1();
            int attackCode = dispIo.attackPacket.dispKey;
            if (GameSystems.D20.UsingSecondaryWeapon(evt.objHndCaller, attackCode))
            {
                dispIo.bonlist.AddBonusFromFeat(6, 0, 114, feat);
            }
            else
            {
                GameSystems.D20.ExtractAttackNumber(evt.objHndCaller, attackCode, out _, out var dualWielding);
                if (dualWielding)
                {
                    dispIo.bonlist.AddBonusFromFeat(2, 0, 114, feat);
                }
            }
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100fcbf0)]
        public static void FamiliarSummonCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var summonIdx = dispIo.action.data1;
            if (summonIdx >= 19 && summonIdx <= 29)
            {
                var condArg1 = evt.GetConditionArg1();
                if ((condArg1 | evt.GetConditionArg2()) != 0)
                {
                    if (dispIo.action.data1 != 29)
                    {
                        dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    }
                }
                else
                {
                    var condArg5 = evt.GetConditionArg(4);
                    if ((condArg5) != 0 && GameSystems.TimeEvent.CurrentDayOfYear < condArg5)
                    {
                        dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    }
                    else if (GameSystems.Combat.IsCombatActive())
                    {
                        var text = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.not_during_combat_AC);
                        GameSystems.TextFloater.FloatLine(evt.objHndCaller, TextFloaterCategory.Generic,
                            TextFloaterColor.Red, text);
                        dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_COMBAT_ONLY;
                    }
                }
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb590)]
        public static void ScribeScrollRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage.scribe_scroll, "TAG_SCRIBE_SCROLL", 2);
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100fb150)]
        [TemplePlusLocation("condition.cpp:408")]
        public static void RemoveDiseaseActionPerform(in DispatcherCallbackArgs evt)
        {
            var action = evt.GetDispIoD20ActionTurnBased().action;
            if (GameSystems.Anim.PushAnimate(action.d20APerformer, NormalAnimType.EnchantmentCasting))
            {
                action.animID = GameSystems.Anim.GetActionAnimId(action.d20APerformer);
                action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            }
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100f9910)]
        public static void sub_100F9910(in DispatcherCallbackArgs evt)
        {
            var action = evt.GetDispIoD20ActionTurnBased().action;
            // TODO: This seems like an inlined function and should be moved out to a utility (i.e. DispatcherExtensions)
            if ((action.d20ATarget == null)
                || !action.d20ATarget.IsCritter()
                || GameSystems.D20.D20QueryWithObject(
                    action.d20ATarget,
                    D20DispatcherKey.QUE_CanBeAffected_PerformAction,
                    action,
                    defaultResult: 1) != 0)
            {
                var condArg1 = evt.GetConditionArg1();
                evt.SetConditionArg1(condArg1 - 1);
                action.d20APerformer.AddCondition(StatusEffects.StunningFistAttacking);
            }
        }


        [DispTypes(DispatcherType.ReflexThrow)]
        [TempleDllLocation(0x100fc9d0)]
        public static void AnimalCompanionReflexBonus(in DispatcherCallbackArgs evt)
        {
            if (AnimalCompanionRefreshHp(in evt, evt.objHndCaller) >= 3)
            {
                EvasionReflexThrow(in evt);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.BeginRound)]
        [TempleDllLocation(0x100f7c90)]
        public static void sub_100F7C90(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(1);
            evt.SetConditionArg2(0);
            evt.SetConditionArg3(0);
        }


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100fbce0)]
        [TemplePlusLocation("condition.cpp:475")]
        public static void WildShapeMorph(in DispatcherCallbackArgs evt)
        {
            var druid = evt.objHndCaller;
            var druidLvl = druid.GetStat(Stat.level_druid);

            var dispIo = evt.GetDispIoD20ActionTurnBased();

            var action = dispIo.action;
            if ((action.data1 & WildShapeActionBitmask) == 0)
            {
                return;
            }

            void initObj(int protoId)
            {
                druid.FreeAnimHandle();
                if (protoId != 0)
                {
                    var lvl = druid.GetStat(Stat.level);
                    GameSystems.Combat.Heal(druid, druid, new Dice(0, 0, lvl), D20ActionType.CLASS_ABILITY_SA);
                }

                GameSystems.ParticleSys.CreateAtObj("sp-animal shape", druid);
                GameSystems.D20.Status.initItemConditions(druid);
            }

            var curWsProto = evt.GetConditionArg3();
            if (curWsProto != 0)
            {
                // deactivating
                evt.SetConditionArg3(0);
                initObj(0);
                return;
            }

            var numTimes = evt.GetConditionArg1();
            var idx = (WildShapeProtoIdx) (action.data1 & ~WildShapeActionBitmask);
            var protoSpec = DruidWildShapes.Options[idx];
            if (protoSpec.monCat == MonsterCategory.elemental)
            {
                if ((numTimes >> 8) <= 0)
                    return;
                evt.SetConditionArg1(numTimes - (1 << 8));
            }
            else
            {
                // normal animal or plant
                if ((numTimes & 0xFF) <= 0)
                    return;
                evt.SetConditionArg1(numTimes - 1);
            }

            evt.SetConditionArg2(600 * druidLvl);
            evt.SetConditionArg3(protoSpec.protoId);
            initObj(protoSpec.protoId);
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f8940)]
        [TemplePlusLocation("condition.cpp:421")]
        public static void TwoWeaponFightingBonusRanger(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (!evt.objHndCaller.IsWearingLightArmorOrLess())
            {
                dispIo.bonlist.zeroBonusSetMeslineNum(166);
                return;
            }

            TwoWeaponFightingBonus(in evt);
        }

        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100fa510)]
        public static void sub_100FA510(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            var monkLvl = evt.objHndCaller.GetStat(Stat.level_monk);
            if (monkLvl >= 3)
            {
                if (ClassConditions.FulfillsMonkArmorAndLoadRequirement(evt.objHndCaller))
                {
                    dispIo.bonlist.AddBonus(10 * (monkLvl / 3), 12, 214);
                }
                else
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(329);
                }
            }

            if (evt.objHndCaller.GetStat(Stat.level_barbarian) > 0)
            {
                if (!evt.objHndCaller.IsWearingMediumArmorOrLess()
                    || evt.objHndCaller.GetStat(Stat.load) >= 3)
                {
                    dispIo.bonlist.zeroBonusSetMeslineNum(329);
                }
                else
                {
                    dispIo.bonlist.AddBonus(10, 0, 214);
                }
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f7d60)]
        [TemplePlusLocation("condition.cpp:418")]
        public static void CombatExpertise_RadialMenuEntry_Callback(in DispatcherCallbackArgs evt)
        {
            var bab = evt.objHndCaller.DispatchToHitBonusBase();
            if (bab > 0)
            {
                int maxArg;
                if (GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.SUPERIOR_EXPERTISE))
                {
                    maxArg = bab;
                }
                else
                {
                    maxArg = Math.Min(5, bab);
                }

                var entry = evt.CreateSliderForArg(0, 0, maxArg);
                entry.text = GameSystems.D20.Combat.GetCombatMesLine(5007);
                entry.helpSystemHashkey = "TAG_COMBAT_EXPERTISE";
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry, RadialMenuStandardNode.Feats);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f8da0)]
        [TemplePlusLocation("condition.cpp:425")]
        public static void NonlethalDamageRadial(in DispatcherCallbackArgs evt)
        {
            // Check for a weapon or the improved unarmed strike feat
            if (GameSystems.D20.GetAttackWeapon(evt.objHndCaller, 1, 0) != null
                || GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.IMPROVED_UNARMED_STRIKE))
            {
                var entry = evt.CreateToggleForArg(0);
                entry.text = GameSystems.D20.Combat.GetCombatMesLine(5014);
                entry.helpSystemHashkey = "TAG_RADIAL_MENU_NONLETHAL_DAMAGE";
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry,
                    RadialMenuStandardNode.Options);
            }
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100f96b0)]
        public static void sub_100F96B0(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var action = dispIo.action;
            var v3 = evt.objHndCaller.GetStat(Stat.constitution);
            if (D20StatSystem.GetModifierForAbilityScore(v3) + 5 <= 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            var rageOn = action.data1 - 1;
            if ((rageOn) != 0)
            {
                if (rageOn == 1 && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Raged))
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    return;
                }
            }
            else
            {
                if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Raged)
                    || GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Fatigued))
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    return;
                }

                if (evt.GetConditionArg1() <= data)
                {
                    dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                }
            }
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100f7e70)]
        [TemplePlusLocation("ability_fixes.cpp:40")]
        public static void CombatExpertiseAcBonus(in DispatcherCallbackArgs evt)
        {
            var expertiseAmt = evt.GetConditionArg1();
            if (expertiseAmt == 0)
            {
                return;
            }

            var attackMade = evt.GetConditionArg2();
            if (attackMade == 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonusFromFeat(expertiseAmt, 8, 114, FeatId.COMBAT_EXPERTISE);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb730)]
        public static void CraftWonderousItemRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage.craft_wondrous_item, "TAG_CRAFT_WONDROUS", 5);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb790)]
        public static void ForgeRingRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage.forge_ring, "TAG_FORGE_RING", 7);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f8b70)]
        public static void WhirlwindAttackRadial(in DispatcherCallbackArgs evt)
        {
            var radMenuEntry = RadialMenuEntry.CreateAction(5011, D20ActionType.WHIRLWIND_ATTACK, 0,
                "TAG_WHIRLWIND_ATTACK");
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry, RadialMenuStandardNode.Offense);
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb570)]
        public static void BrewPotionRadialMenu(in DispatcherCallbackArgs evt)
        {
            AddCraftUiRadialEntry(in evt, D20CombatMessage.brew_potion, "TAG_BREW_POTION", 1);
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f7f00)]
        [TemplePlusLocation("condition.cpp:419")]
        public static void CombatExpertiseSet(in DispatcherCallbackArgs evt)
        {
            int bab = evt.objHndCaller.DispatchToHitBonusBase();
            if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.SUPERIOR_EXPERTISE))
            {
                bab = Math.Min(5, bab);
            }

            var dispIo = evt.GetDispIoD20Signal();
            int bonus = dispIo.data1;
            if (bonus > bab)
                bonus = bab;
            if (bonus < 0)
                bonus = 0;
            evt.SetConditionArg1(bonus);
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f8e70)]
        public static void sub_100F8E70(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                if (dispIo.attackPacket.GetWeaponUsed() != null)
                {
                    if (((dispIo.attackPacket.flags & D20CAF.RANGED) == 0))
                    {
                        dispIo.bonlist.AddBonus(-4, 0, 156);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f94f0)]
        public static void sub_100F94F0(in DispatcherCallbackArgs evt)
        {
            if (CanUseFlurryOfBlows(in evt))
            {
                var dispIo = evt.GetDispIoD20Query();
                var dispIoDamage = (DispIoDamage) dispIo.obj;
                var caf = dispIoDamage.attackPacket.flags;
                if ((caf & D20CAF.FULL_ATTACK) != 0)
                {
                    if ((caf & D20CAF.RANGED) == 0)
                    {
                        dispIo.return_val = 0;
                    }
                }
            }
        }

        [DispTypes(DispatcherType.ReflexThrow)]
        [TempleDllLocation(0x100fa3c0)]
        public static void EvasionReflexThrow(in DispatcherCallbackArgs evt)
        {
            if (evt.objHndCaller.IsWearingLightArmorOrLess())
            {
                var dispIo = evt.GetDispIoReflexThrow();
                if (dispIo.throwResult)
                {
                    dispIo.throwResult = true; // TODO: This was '2' to indicate evasion???
                    dispIo.effectiveReduction = 0;
                    dispIo.damageMesLine = 107;
                }
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100fa8d0)]
        public static void RapidShotMallus(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                var v2 = dispIo.attackPacket.flags;
                if ((v2 & D20CAF.RANGED) != 0)
                {
                    if ((v2 & D20CAF.FULL_ATTACK) != 0)
                    {
                        dispIo.bonlist.AddBonus(-2, 0, 216);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f8d60)]
        public static void SetCastDefensively(in DispatcherCallbackArgs evt)
        {
            var v1 = evt.GetDispIoD20Signal().data1;
            if (v1 <= 1)
            {
                if (v1 < 0)
                {
                    v1 = 0;
                }
            }
            else
            {
                v1 = 1;
            }

            evt.SetConditionArg1(v1);
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f8540)]
        [TemplePlusLocation("condition.cpp:450")]
        public static void PowerAttackDamageBonus(in DispatcherCallbackArgs evt)
        {
            var powerAttackAmt = evt.GetConditionArg1();

            if (powerAttackAmt == 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoDamage();

            // ignore ranged weapons
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                return;
            }

            // get wield type
            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
            var wieldType = GameSystems.Item.GetWieldType(evt.objHndCaller, weaponUsed, true);
            var wieldTypeWeaponModified =
                GameSystems.Item.GetWieldType(evt.objHndCaller,
                    weaponUsed); // the wield type if the weapon is not enlarged along with the critter
            var modifiedByEnlarge = wieldType != wieldTypeWeaponModified;

            // check offhand
            var offhandWeapon = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);
            var shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.Shield);
            var regardOffhand =
                (offhandWeapon != null ||
                 shield != null &&
                 !GameSystems.Item.IsBuckler(shield)); // is there an offhand item (weapon/non-buckler shield)

            // case 1
            switch (wieldType)
            {
                case 0: // light weapon
                    switch (wieldTypeWeaponModified)
                    {
                        case 0:
                            dispIo.damage.bonuses.zeroBonusSetMeslineNum(305);
                            return;
                        case 1: // benefitting from enlargement of weapon
                            if (regardOffhand)
                                dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            else
                                dispIo.damage.bonuses.AddBonusFromFeat(2 * powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                        case 2:
                            if (regardOffhand)
                                dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            else
                                dispIo.damage.bonuses.AddBonusFromFeat(2 * powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                        default:
                            dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                    }
                case 1: // single handed wield if weapon is unaffected
                    switch (wieldTypeWeaponModified)
                    {
                        case 0
                            : // only in reduce person; going to assume the "beneficial" case that the reduction was made voluntarily and hence you let the weapon stay larger
                        case 1:
                            if (regardOffhand)
                                dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            else
                                dispIo.damage.bonuses.AddBonusFromFeat(2 * powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                        case 2:
                            if (regardOffhand)
                                dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            else
                                dispIo.damage.bonuses.AddBonusFromFeat(2 * powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                        default:
                            dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                    }
                case 2: // two handed wield if weapon is unaffected
                    switch (wieldTypeWeaponModified)
                    {
                        case 0:
                        case 1
                            : // only in reduce person; going to assume the "beneficial" case that the reduction was made voluntarily and hence you let the weapon stay larger
                            if (regardOffhand) // has offhand item, so assume the weapon stayed the old size
                                dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            else
                                dispIo.damage.bonuses.AddBonusFromFeat(2 * powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                        case 2:
                            if (regardOffhand) // shouldn't really be possible... maybe if player is cheating
                            {
                                dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                                Logger.Warn("Illegally wielding weapon along withoffhand!");
                            }
                            else
                                dispIo.damage.bonuses.AddBonusFromFeat(2 * powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);

                            return;
                        default:
                            dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                            return;
                    }
                case 3:
                    dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                    return;
                case 4:
                default:
                    dispIo.damage.bonuses.AddBonusFromFeat(powerAttackAmt, 0, 114, FeatId.POWER_ATTACK);
                    return;
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fb2d0)]
        public static void MonkWholenessOfBodyRadial(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();

            var parentEntry = RadialMenuEntry.CreateParent(5065);
            var parentIdx = GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry, RadialMenuStandardNode.Class);

            if (condArg2 > 0)
            {
                var slider = evt.CreateSliderForArg(1, 1, condArg2);
                slider.d20ActionType = D20ActionType.WHOLENESS_OF_BODY_SET;
                slider.text = GameSystems.D20.Combat.GetCombatMesLine(6014); // "Set"
                slider.text2 = GameSystems.D20.Combat.GetCombatMesLine(5065);
                slider.helpSystemHashkey = "TAG_CLASS_FEATURES_MONK_WHOLENESS_OF_BODY";
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref slider, parentIdx);

                var useEntry = RadialMenuEntry.CreateAction(6013, D20ActionType.WHOLENESS_OF_BODY_USE,
                    0, "TAG_CLASS_FEATURES_MONK_WHOLENESS_OF_BODY");
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref useEntry, parentIdx);
            }
        }


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100f9760)]
        public static void BarbarianRagePerform(in DispatcherCallbackArgs evt)
        {
            var v1 = evt.GetDispIoD20ActionTurnBased().action;
            if (v1.data1 == 1)
            {
                if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Raged)
                    && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Fatigued))
                {
                    var condArg1 = evt.GetConditionArg1();
                    evt.SetConditionArg1(condArg1 - 1);
                    v1.d20APerformer.AddCondition(StatusEffects.BarbarianRaged);
                }
            }
            else if (v1.data1 == 2)
            {
                if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Raged))
                {
                    v1.d20APerformer.AddCondition(StatusEffects.BarbarianFatigued);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100f9e00)]
        public static void LayOnHandsRefresher(in DispatcherCallbackArgs evt)
        {
            var v4 = evt.objHndCaller.GetStat(Stat.level_paladin);
            var v1 = evt.objHndCaller.GetStat(Stat.charisma);
            var v2 = D20StatSystem.GetModifierForAbilityScore(v1);
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                evt.SetConditionArg1(0);
            }
            else
            {
                var v3 = v4 * v2;
                if (v3 <= 0 || v1 < 12)
                {
                    evt.SetConditionArg1(0);
                    evt.SetConditionArg2(0);
                }
                else
                {
                    evt.SetConditionArg1(v3);
                    evt.SetConditionArg2(v3);
                }
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f8410)]
        public static void PowerAttackRadialMenu(in DispatcherCallbackArgs evt)
        {
            var bab = evt.objHndCaller.DispatchToHitBonusBase();
            if (bab > 0)
            {
                var radMenuEntry = evt.CreateSliderForArg(0, 0, bab);
                var meslineKey = 5006;
                var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
                radMenuEntry.text = meslineValue;
                radMenuEntry.helpSystemHashkey = "TAG_POWER_ATTACK";
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                    RadialMenuStandardNode.Feats);
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100f9150)]
        public static void sub_100F9150(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == (ConditionSpec) data && dispIo.arg1 < 0x18)
            {
                dispIo.outputFlag = 0;
            }
        }


        [DispTypes(DispatcherType.DeflectArrows)]
        [TempleDllLocation(0x100fa2f0)]
        public static void DeflectArrowsCallback(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                if (dispIo.attackPacket.weaponUsed != null)
                {
                    if (GameSystems.Critter.CanSense(dispIo.attackPacket.attacker, dispIo.attackPacket.victim)
                        && !GameSystems.D20.D20Query(dispIo.attackPacket.victim, D20DispatcherKey.QUE_Flatfooted)
                        && ((GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 203) == null) ||
                            (GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 204) == null)))
                    {
                        var v2 = dispIo.attackPacket.flags;
                        if ((v2 & D20CAF.RANGED) != 0)
                        {
                            dispIo.attackPacket.flags = v2 & ~(D20CAF.CRITICAL | D20CAF.HIT) | D20CAF.DEFLECT_ARROWS;
                            evt.SetConditionArg1(0);
                        }
                    }
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100f9c00)]
        public static void SmiteEvilRefresh(in DispatcherCallbackArgs evt)
        {
            int v1;

            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                v1 = 0;
            }
            else
            {
                v1 = evt.objHndCaller.GetStat(Stat.level_paladin) / 5 + 1;
            }

            evt.SetConditionArg1(v1);
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100fa780)]
        public static void FavoredEnemyDamageBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var attacker = evt.objHndCaller;
            var victim = dispIo.attackPacket.victim;
            if (dispIo.attackPacket.victim == null)
            {
                return;
            }

            if (FavoredEnemies.GetFavoredEnemyBonusAgainst(attacker, victim, out var bonus, out _)
                && dispIo.attackPacket.d20ActnType != D20ActionType.CAST_SPELL)
            {
                dispIo.damage.AddDamageBonus(bonus, 0, 215);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f9c50)]
        public static void SmiteEvilRadialMenu(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                if ((condArg1) != 0)
                {
                    var radMenuEntry = RadialMenuEntry.CreateAction(5049, D20ActionType.SMITE_EVIL, 0, "TAG_CLASS_FEATURES_PALADIN_SMITE_EVIL");
                    GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry, RadialMenuStandardNode.Class);
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f9120)]
        public static void sub_100F9120(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            evt.SetConditionArg1(dispIo.data1);
        }


        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100fd620)]
        public static void FamiliarSummonDismiss(in DispatcherCallbackArgs evt)
        {
            var action = evt.GetDispIoD20ActionTurnBased().action;
            var summonKey = action.data1;
            if (summonKey >= 19 && summonKey <= 29)
            {
                var familiar = evt.GetConditionObjArg(0);
                if (familiar != null)
                {
                    if (summonKey == 29)
                    {
                        var v6 = GameSystems.D20.Combat.GetCombatMesLine(6007);
                        var v5 = GameSystems.MapObject.GetDisplayName(familiar);
                        var v4 = GameSystems.D20.Combat.GetCombatMesLine(6008);
                        var conditionAttachment = evt.subDispNode.condNode;
                        GameUiBridge.Confirm($"{v6}{v5}{v4}", null, false, buttonIdx =>
                        {
                            if (buttonIdx == 0)
                            {
                                DismissFamiliar(conditionAttachment, familiar);
                            }
                        });
                    }
                }
                else
                {
                    var conditionAttachment = evt.subDispNode.condNode;
                    var master = evt.objHndCaller;
                    var familiarIdx = action.data1 - 19;
                    var message = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage
                        .name_your_familiar);
                    GameUiBridge.ShowTextEntry(message, (name, confirmed) =>
                    {
                        if (confirmed && AnimalCompanionTrimName(name, out name))
                        {
                            SummonFamiliar(conditionAttachment, master, name, familiarIdx);
                        }
                    });
                }
            }
        }

        [TempleDllLocation(0x100fccd0)]
        private static void SummonFamiliar(ConditionAttachment conditionAttachment, GameObjectBody summoner,
            string name, int familiarIdx)
        {
            var protoId = (ushort) FamiliarProtos[familiarIdx];
            var locAndOffOut = summoner.GetLocationFull();
            var handleNew = GameSystems.MapObject.CreateObject(protoId, locAndOffOut);
            GameSystems.Item.SetItemParent(handleNew, summoner, ItemInsertFlag.Use_Max_Idx_200);

            // TODO: This is wrong, we need to store the handle to the familiar in the condition
            conditionAttachment.args[0] = 0;
            conditionAttachment.args[1] = 1;

            GameSystems.ParticleSys.CreateAtObj("sp-Summon Monster I", summoner);

            var descId = GameSystems.Description.Create(name);
            handleNew.SetInt32(obj_f.description, descId);

            handleNew.SetItemFlag(ItemFlag.NO_TRANSFER_SPECIAL, true);
            handleNew.SetItemFlag(ItemFlag.IDENTIFIED, true);
        }

        [TempleDllLocation(0x100fd440)]
        private static void DismissFamiliar(ConditionAttachment conditionAttachment, GameObjectBody familiar)
        {
            GameSystems.Object.Destroy(familiar);
            // TODO: This is borked and is supposed to reset the object arg in slot 0
            conditionAttachment.args[0] = 0;
            conditionAttachment.args[1] = 0;

            // Able to resummon one year later (which is wrong btw...)
            var ableToResummon = GameSystems.TimeEvent.CurrentDayOfYear + 366;
            conditionAttachment.args[4] = ableToResummon;
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fbfc0)]
        public static void WildShapeCannotCastQuery(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg3()) != 0)
            {
                if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.NATURAL_SPELL))
                {
                    evt.GetDispIoD20Query().return_val = 1;
                }
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100fa690)]
        public static void FavoredEnemySkillBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            var target = dispIo.obj;
            if (target == null)
            {
                return;
            }

            var skillId = evt.GetSkillIdFromDispatcherKey();
            switch (skillId)
            {
                case SkillId.bluff:
                case SkillId.listen:
                case SkillId.sense_motive:
                case SkillId.spot:
                case SkillId.wilderness_lore:
                    if (FavoredEnemies.GetFavoredEnemyBonusAgainst(evt.objHndCaller, target, out var bonus, out _))
                    {
                        dispIo.bonOut.AddBonus(bonus, 0, 215);
                    }

                    break;
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f87d0)]
        public static void GreatCleaveDroppedEnemy(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var dispIoDmg = (DispIoDamage) dispIo.obj;

            var caf = dispIoDmg.attackPacket.flags;
            var targetsCleaved = evt.GetConditionArg1();

            // TODO: Normal cleave has a check here that prevent cleaving when whirlwinding
            if ((caf & D20CAF.RANGED) == 0)
            {
                var cleaveTarget = GameSystems.D20.Combat.GetCleaveTarget(evt.objHndCaller);
                if (cleaveTarget != null)
                {
                    var targetLoc = cleaveTarget.GetLocationFull();
                    var data1 = dispIoDmg.attackPacket.dispKey; // This is the attack code
                    D20ActionCallbacks.InsertD20Action(evt.objHndCaller, D20ActionType.CLEAVE, data1, cleaveTarget,
                        targetLoc, 0);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x17, evt.objHndCaller, null);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 36);

                    if (targetsCleaved != 0)
                    {
                        // Message for subsequent cleaves
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x18, evt.objHndCaller, null);
                        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 37);
                    }
                    else
                    {
                        // Message for the first cleave
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x17, evt.objHndCaller, null);
                        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 36);
                    }

                    evt.SetConditionArg1(targetsCleaved + 1);
                }
            }
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100fc2b0)]
        public static void AnimalCompanionCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var action = dispIo.action;
            var effectiveDruidLevel = evt.objHndCaller.GetStat(Stat.level_druid);
            var rangerLevel = evt.objHndCaller.GetStat(Stat.level_ranger);
            if (rangerLevel >= 4)
            {
                effectiveDruidLevel += rangerLevel / 2;
            }

            var summonCode = action.data1;
            if (summonCode >= 8 && summonCode <= 18)
            {
                var condArg1 = evt.GetConditionArg1();
                if ((condArg1 | evt.GetConditionArg2()) != 0)
                {
                    if (action.data1 == 18)
                    {
                        return;
                    }

                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    return;
                }

                var minLevel = AnimalCompanionLevelRestrictions[summonCode - 8];
                if (effectiveDruidLevel < minLevel + 1
                    || evt.GetConditionArg(4) != 0
                    || GameSystems.Party.AiFollowerCount > 5)
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    return;
                }

                if (GameSystems.Combat.IsCombatActive())
                {
                    var v7 = GameSystems.D20.Combat.GetCombatMesLine(6002);
                    GameSystems.TextFloater.FloatLine(evt.objHndCaller, TextFloaterCategory.Generic,
                        TextFloaterColor.Red, v7);
                    dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_COMBAT_ONLY;
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100fc7c0)]
        public static void AnimalCompanionOnAdd(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg(5, 0);
            AnimalCompanionRefreshHp(in evt, evt.objHndCaller);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f7ed0)]
        [TemplePlusLocation("ability_fixes.cpp:39")]
        public static void TacticalOptionAbusePrevention(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var damPkt = (DispIoDamage) dispIo.obj;
            if ((damPkt.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                return;
            }

            evt.SetConditionArg2(1);
        }

        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100f9540)]
        public static void sub_100F9540(in DispatcherCallbackArgs evt)
        {
            var v1 = evt.objHndCaller.GetStat(Stat.level_barbarian);
            evt.SetConditionArg1(v1 / 4 + 1);

            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Raged)
                || GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Fatigued))
            {
                evt.KillPartSysInArg(1);
            }
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100fa880)]
        public static void DivineHealthDiseaseGuard(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                var dispIo = evt.GetDispIoCondStruct();
                var v2 = dispIo.condStruct;
                if (v2 == StatusEffects.NSDiseased || v2 == StatusEffects.IncubatingDisease)
                {
                    dispIo.outputFlag = 0;
                }
            }
        }


        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x100fb450)]
        public static void WholenessOfBodyActionFrame(in DispatcherCallbackArgs evt)
        {
            var action = evt.GetDispIoD20ActionTurnBased().action;
            var condArg1 = evt.GetConditionArg1();
            var remainingHealAmt = evt.GetConditionArg2();
            var v4 = condArg1 - remainingHealAmt;
            evt.SetConditionArg1(v4);
            evt.SetConditionArg2(v4);

            var damage = evt.objHndCaller.GetInt32(obj_f.hp_damage);
            var subdualDamage = evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage);
            var healingNeeded = Math.Max(damage, subdualDamage);

            Dice actualHeal;
            if (remainingHealAmt <= healingNeeded)
            {
                actualHeal = Dice.Constant(remainingHealAmt);
            }
            else
            {
                var v7 = remainingHealAmt - healingNeeded;
                evt.SetConditionArg1(evt.GetConditionArg1() + v7);
                evt.SetConditionArg2(evt.GetConditionArg2() + v7);
                actualHeal = Dice.Constant(healingNeeded);
            }

            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1C, evt.objHndCaller, null);
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 5065);
            GameSystems.Combat.Heal(evt.objHndCaller, evt.objHndCaller, actualHeal, action.d20ActType);
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100fbe70)]
        public static void WildShapeBeginRound(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg3()) != 0)
            {
                var dispIo = evt.GetDispIoD20Signal();
                var v2 = evt.GetConditionArg2() - dispIo.data1;
                if (v2 < 0)
                {
                    evt.SetConditionArg3(0);
                    evt.objHndCaller.FreeAnimHandle();

                    GameSystems.ParticleSys.CreateAtObj("sp-animal shape", evt.objHndCaller);
                    GameSystems.D20.Status.initItemConditions(evt.objHndCaller);
                }

                evt.SetConditionArg2(v2);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fceb0)]
        public static void ManyshotRadial(in DispatcherCallbackArgs evt)
        {
            int v2;

            var radMenuEntry = evt.CreateToggleForArg(0);
            var meslineKey = 5095;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            radMenuEntry.text = (string) meslineValue;
            radMenuEntry.helpSystemHashkey = "TAG_MANYSHOT";
            var v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Feats);
            GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
        }


        [DispTypes(DispatcherType.GetBonusAttacks)]
        [TempleDllLocation(0x100ee910)]
        public static void FlurryOfBlowsBonusAttacks(in DispatcherCallbackArgs evt)
        {
            Dice damageDice;
            DamageType damageType;

            var damageMesLine = 100;
            string weaponName = "";
            var dispIo = evt.GetDispIoDamage();
            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
            var attacker = evt.objHndCaller;
            var attackCode = dispIo.attackPacket.dispKey;

            // This is needed to correctly determine natural attack dice for polymorphed monks
            var obj = evt.objHndCaller;
            var polyProtoId = GameSystems.D20.D20QueryInt(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed);
            if (polyProtoId != 0)
            {
                obj = GameSystems.Proto.GetProtoById((ushort) polyProtoId);
            }

            if (weaponUsed != null)
            {
                var diceDispIo = new DispIoAttackDice();
                diceDispIo.flags = dispIo.attackPacket.flags;
                diceDispIo.wielder = evt.objHndCaller;
                diceDispIo.weapon = weaponUsed;
                damageDice = evt.objHndCaller.DispatchGetAttackDice(diceDispIo);
                damageType = diceDispIo.attackDamageType;
                weaponName = GameSystems.MapObject.GetDisplayNameForParty(weaponUsed);
            }
            else
            {
                if (attackCode < 10)
                {
                    return;
                }

                var naturalAttackIdx = attackCode - 10;
                var naturalAttackDice = GameSystems.Critter.GetCritterDamageDice(obj, naturalAttackIdx);
                damageType = GameSystems.Critter.GetCritterAttackDamageType(obj, naturalAttackIdx);
                damageMesLine = 114 + (int) GameSystems.Critter.GetCritterAttackType(obj, naturalAttackIdx);
                var diceDispIo = new DispIoAttackDice();
                diceDispIo.flags = dispIo.attackPacket.flags;
                diceDispIo.weapon = null;
                diceDispIo.wielder = attacker;
                diceDispIo.dicePacked = naturalAttackDice;
                diceDispIo.attackDamageType = damageType;
                damageDice = attacker.DispatchGetAttackDice(diceDispIo);
            }

            var damage = dispIo.damage;
            damage.AddDamageDice(damageDice, damageType, damageMesLine, weaponName);
            var strMod = attacker.GetStat(Stat.str_mod);
            var caf = dispIo.attackPacket.flags;
            if (((caf & D20CAF.RANGED) == 0) || (caf & D20CAF.THROWN) != 0)
            {
                if (attackCode == 6 || attackCode == 8)
                {
                    if (strMod > 0)
                    {
                        strMod /= 2;
                    }
                }
                else if (GameSystems.D20.D20QueryWithObject(attacker, D20DispatcherKey.QUE_WieldedTwoHanded, dispIo) !=
                         0
                         && strMod > 0
                         && (GameSystems.Item.GetWieldType(attacker, weaponUsed)) != 0)
                {
                    strMod += strMod / 2;
                }

                if (attackCode >= 10 && strMod > 0 && GameSystems.Critter.GetDamageIdx(obj, attackCode - 10) > 0)
                {
                    strMod /= 2;
                }

                damage.AddDamageBonus(strMod, 2, 103);
                return;
            }

            if ((weaponUsed == null))
            {
                return;
            }

            var weaponType = weaponUsed.GetWeaponType();
            if (weaponType == WeaponType.sling)
            {
                damage.AddDamageBonus(strMod, 2, 103);
                return;
            }

            if ((weaponType == WeaponType.shortbow || weaponType == WeaponType.longbow) && strMod < 0)
            {
                damage.AddDamageBonus(strMod, 2, 103);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100fad20)]
        public static void PointBlankShotToHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var victim = dispIo.attackPacket.victim;
            if (victim != null && (dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                if (evt.objHndCaller.DistanceToInFeetClamped(victim) < 30.0f)
                {
                    dispIo.bonlist.AddBonusFromFeat(1, 0, 114, FeatId.POINT_BLANK_SHOT);
                }
            }
        }

        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100fbf30)]
        public static void WildshapeReplaceStats(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            var protoId = evt.GetConditionArg3();
            if (protoId != 0)
            {
                var attribute = evt.GetAttributeFromDispatcherKey();
                var protoObj = GameSystems.Proto.GetProtoById((ushort) protoId);
                var baseValue = protoObj.GetInt32(obj_f.critter_abilities_idx, (int) attribute);
                var text = GameSystems.D20.Combat.GetCombatMesLine(118);
                dispIo.bonlist.ReplaceBonus(1, baseValue, 102, text);
            }
        }


        [DispTypes(DispatcherType.GetBonusAttacks)]
        [TempleDllLocation(0x100fa920)]
        public static void RapidShotNumAttacksPerTurn(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoD20ActionTurnBased();
                if ((dispIo.action.d20Caf & D20CAF.RANGED) != 0)
                {
                    ++dispIo.returnVal;
                }
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100fcf50)]
        public static void ManyshotPenalty(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) != 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
                {
                    if (dispIo.attackPacket.ammoItem != null
                        && (dispIo.attackPacket.weaponUsed.GetInt32(obj_f.weapon_ammo_type)) == 0
                        && (dispIo.attackPacket.flags & D20CAF.FULL_ATTACK) == 0
                        && dispIo.attackPacket.dispKey == 0
                        && dispIo.attackPacket.d20ActnType == D20ActionType.STANDARD_RANGED_ATTACK
                        && dispIo.attackPacket.ammoItem.GetInt32(obj_f.ammo_quantity) > 1
                        && evt.objHndCaller.DistanceToInFeetClamped(dispIo.attackPacket.victim) <= 30F)
                    {
                        dispIo.attackPacket.flags |= D20CAF.MANYSHOT;
                        dispIo.bonlist.AddBonus(-2, 0, 304);
                    }
                }
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f95b0)]
        public static void BarbarianRageRadialMenu(in DispatcherCallbackArgs evt)
        {
            if ((evt.objHndCaller.GetStat(Stat.level_barbarian)) != 0)
            {
                var conMod = evt.objHndCaller.GetStat(Stat.con_mod);
                if (5 + conMod > 0)
                {
                    int combatMesKey;
                    int data1;
                    if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Raged)
                        || GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Fatigued))
                    {
                        if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Barbarian_Raged))
                        {
                            return;
                        }

                        data1 = 2;
                        combatMesKey = 5046; // Fatigued
                    }
                    else
                    {
                        data1 = 1;
                        combatMesKey = 5100; // Barbarian Rage
                    }

                    var radMenuEntry = RadialMenuEntry.CreateAction(combatMesKey, D20ActionType.BARBARIAN_RAGE, data1,
                        "TAG_CLASS_FEATURES_BARBARIAN_RAGE");
                    GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry, RadialMenuStandardNode.Class);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100fb290)]
        public static void sub_100FB290(in DispatcherCallbackArgs evt)
        {
            var v1 = 2 * evt.objHndCaller.GetStat(Stat.level_monk);
            evt.SetConditionArg1(v1);
            evt.SetConditionArg2(v1);
        }


        [DispTypes(DispatcherType.NewDay)]
        [TempleDllLocation(0x100fb010)]
        public static void RemoveDiseaseResetCharges(in DispatcherCallbackArgs evt)
        {
            // TODO: Instead of weekly charges, they become daily charges? Ooookay... Big buff!
            evt.SetConditionArg1(0);
        }

        private static bool GetSchoolOfMagicFromFeat(FeatId feat, out SchoolOfMagic school)
        {
            if (feat >= FeatId.SPELL_FOCUS_ABJURATION && feat <= FeatId.SPELL_FOCUS_TRANSMUTATION)
            {
                school = SchoolOfMagic.Abjuration + (feat - FeatId.SPELL_FOCUS_ABJURATION);
                return true;
            }

            if (feat >= FeatId.GREATER_SPELL_FOCUS_ABJURATION && feat <= FeatId.GREATER_SPELL_FOCUS_TRANSMUTATION)
            {
                school = SchoolOfMagic.Abjuration + (feat - FeatId.GREATER_SPELL_FOCUS_ABJURATION);
                return true;
            }

            school = SchoolOfMagic.None;
            return false;
        }

        [DispTypes(DispatcherType.SpellDcMod)]
        [TempleDllLocation(0x100fc050)]
        public static void SpellDcMod_SpellFocus_Callback(in DispatcherCallbackArgs evt)
        {
            var featId = (FeatId) evt.GetConditionArg1();
            var dispIo = evt.GetDispIOBonusListAndSpellEntry();
            var school = dispIo.spellEntry.spellSchoolEnum;

            if (GetSchoolOfMagicFromFeat(featId, out var featSchool) && featSchool == school)
            {
                dispIo.bonList.AddBonusFromFeat(1, 0, 114, featId);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f8f70)]
        [TemplePlusLocation("condition.cpp:427")]
        public static void DealNormalDamageCallback(in DispatcherCallbackArgs evt)
        {
            // No weapon and doesn't have the improved unarmed strike feat
            if (GameSystems.D20.GetAttackWeapon(evt.objHndCaller, 1, D20CAF.NONE) == null
                && !GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.IMPROVED_UNARMED_STRIKE))
            {
                var entry = evt.CreateToggleForArg(0);
                entry.text = GameSystems.D20.Combat.GetCombatMesLine(5015);
                entry.helpSystemHashkey = "TAG_RADIAL_MENU_NONLETHAL_DAMAGE";
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry,
                    RadialMenuStandardNode.Options);
            }
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100f99b0)]
        public static void sub_100F99B0(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var v2 = dispIo.action;
            if (GameSystems.D20.GetAttackWeapon(v2.d20APerformer, 1, 0) != null)
            {
                dispIo.returnVal = ActionErrorCode.AEC_WRONG_WEAPON_TYPE;
            }
            else if (GameSystems.D20.GetAttackWeapon(v2.d20APerformer, 2, 0) != null)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100f9ec0)]
        public static void LayOnHandsRadialMenu(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
            {
                var parentEntry = RadialMenuEntry.CreateParent(5051);
                var parentIdx = GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref parentEntry, RadialMenuStandardNode.Class);

                if (evt.GetConditionArg1() > 0)
                {
                    var condArg1 = evt.GetConditionArg1();

                    var slider = evt.CreateSliderForArg(1, 1, condArg1);
                    slider.d20ActionType = D20ActionType.LAY_ON_HANDS_SET;
                    slider.d20ActionData1 = 0;
                    slider.text = GameSystems.D20.Combat.GetCombatMesLine(6014); // Set
                    slider.text2 = GameSystems.D20.Combat.GetCombatMesLine(5051); // Lay on Hands
                    slider.helpSystemHashkey = "TAG_CLASS_FEATURES_PALADIN_LAY_ON_HANDS";
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref slider, parentIdx);

                    var use = RadialMenuEntry.CreateAction(6013, D20ActionType.LAY_ON_HANDS_USE,
                        0, "TAG_CLASS_FEATURES_PALADIN_LAY_ON_HANDS");
                    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref use, parentIdx);
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fbf90)]
        public static void WildShapePolymorphedQuery(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            if ((condArg3) != 0)
            {
                evt.GetDispIoD20Query().return_val = condArg3;
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100fadf0)]
        [TemplePlusLocation("ability_fixes.cpp:88")]
        public static void OpportunistBroadcastAction(in DispatcherCallbackArgs evt)
        {
            var numAvail = evt.GetConditionArg1();
            if (numAvail == 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoD20Signal();
            var action = (D20Action) dispIo.obj;
            // fixes vanilla bug where you got an AOO for your making your own attack
            if (action.d20APerformer == evt.objHndCaller)
            {
                return;
            }

            var tgt = action.d20ATarget;
            // fixed missing check on target (e.g. this would fire on move actions)
            if (tgt == null)
            {
                return;
            }

            if (GameSystems.D20.D20QueryWithObject(evt.objHndCaller, D20DispatcherKey.QUE_AOOPossible, tgt) == 0)
                return;
            if (GameSystems.D20.D20QueryWithObject(evt.objHndCaller, D20DispatcherKey.QUE_AOOWillTake, tgt) == 0)
                return;
            if (GameSystems.Combat.AffiliationSame(evt.objHndCaller, tgt))
                return;
            if (!GameSystems.Combat.CanMeleeTarget(evt.objHndCaller, tgt))
                return;

            if (!action.IsMeleeHit())
                return;

            evt.SetConditionArg1(numAvail - 1);
            GameSystems.D20.Actions.DoAoo(evt.objHndCaller, tgt);
        }

        [DispTypes(DispatcherType.GetNumAttacksBase, DispatcherType.D20Signal, DispatcherType.ToHitBonus2,
            DispatcherType.GetBonusAttacks)]
        [TempleDllLocation(0x100fd250)]
        public static void ArmorLightOnly(in DispatcherCallbackArgs evt, SubDispatcherCallback data)
        {
            if (evt.objHndCaller.IsWearingLightArmorOrLess())
            {
                data(in evt);
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100fb5f0)]
        public static void SkillMasterySkillLevel(in DispatcherCallbackArgs evt)
        {
            var feat = (FeatId) evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoObjBonus();
            var skillId = evt.GetSkillIdFromDispatcherKey();
            if (((1 << (int) skillId) & condArg2) != 0)
            {
                dispIo.bonOut.AddBonusFromFeat(2, 0, 114, feat);
            }
        }

        [DispTypes(DispatcherType.GetCritterNaturalAttacksNum)]
        [TempleDllLocation(0x100fc010)]
        public static void WildShapeGetNumAttacks(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg3()) != 0)
            {
                var dispIo = evt.GetDispIoD20ActionTurnBased();
                dispIo.returnVal = (ActionErrorCode) GameSystems.D20.Actions.DispatchD20ActionCheck(dispIo.action,
                    dispIo.tbStatus,
                    DispatcherType.GetNumAttacksBase);
            }
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100faec0)]
        public static void FeatSkillBonus(in DispatcherCallbackArgs evt, int data)
        {
            var v1 = data;
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoObjBonus();
            var v4 = GameSystems.Feat.GetFeatName((FeatId) condArg1);
            dispIo.bonOut.AddBonus(v1, 0, 114, v4);
        }
    }
}