using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static class MonsterConditions
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x102eb620)]
        public static readonly ConditionSpec MonsterBansheeCharismaDrain = ConditionSpec
            .Create("Monster Banshee Charisma Drain", 0)
            .Prevents(MonsterBansheeCharismaDrain)
            .AddHandler(DispatcherType.DealingDamage2, BansheeCharismaDrainOnDamage)
            .Build();


        [TempleDllLocation(0x102eb668)]
        public static readonly ConditionSpec MonsterDamageType = ConditionSpec.Create("Monster Damage Type", 1)
            .Prevents(MonsterDamageType)
            .AddHandler(DispatcherType.DealingDamage, SetMonsterDamageType)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102eb6c0)]
        public static readonly ConditionSpec MonsterBonusDamage = ConditionSpec.Create("Monster Bonus Damage", 2)
            .Prevents(MonsterBonusDamage)
            .AddHandler(DispatcherType.DealingDamage, MonsterDamageBonus)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 1)
            .Build();


        [TempleDllLocation(0x102eb730)]
        public static readonly ConditionSpec MonsterStirge = ConditionSpec.Create("Monster Stirge", 6)
            .Prevents(MonsterStirge)
            .AddHandler(DispatcherType.DealingDamage2, StirgeDealingDamage)
            .AddHandler(DispatcherType.BeginRound, StirgeAttach_callback)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 2)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 2)
            .AddHandler(DispatcherType.TurnBasedStatusInit, BatsNoActionsWhenAttached)
            .AddHandler(DispatcherType.GetAC, MonsterStigeAcBonusCap, 2)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, BatsNoAooWhenAttached)
            .Build();


        [TempleDllLocation(0x102eb7f0)]
        public static readonly ConditionSpec MonsterFireBats = ConditionSpec.Create("Monster Fire Bats", 6)
            .Prevents(MonsterFireBats)
            .AddHandler(DispatcherType.DealingDamage2, sub_100F6A70)
            .AddHandler(DispatcherType.BeginRound, FireBats_callback)
            .AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 2)
            .AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 2)
            .AddHandler(DispatcherType.TurnBasedStatusInit, BatsNoActionsWhenAttached)
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, BatsNoAooWhenAttached)
            .AddHandler(DispatcherType.Tooltip, FirebatsTooltip, 5045)
            .Build();


        [TempleDllLocation(0x102eb8b0)]
        public static readonly ConditionSpec MonsterMeleeDisease = ConditionSpec.Create("Monster Melee Disease", 1)
            .Prevents(MonsterMeleeDisease)
            .AddHandler(DispatcherType.DealingDamage2, ConditionAddOnDamage, StatusEffects.IncubatingDisease)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyObjConditionsCallback, 0, 0)
            .Build();


        [TempleDllLocation(0x102eb908)]
        public static readonly ConditionSpec MonsterMeleePoison = ConditionSpec.Create("Monster Melee Poison", 1)
            .Prevents(MonsterMeleePoison)
            .AddHandler(DispatcherType.DealingDamage2, ConditionAddOnDamage, StatusEffects.Poisoned)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyObjConditionsCallback, 0, 0)
            .Build();


        [TempleDllLocation(0x102eb960)]
        public static readonly ConditionSpec MonsterCarrionCrawler = ConditionSpec.Create("Monster Carrion Crawler", 0)
            .Prevents(MonsterCarrionCrawler)
            .AddHandler(DispatcherType.DealingDamage2, CarrionCrawlerParalysisOnDamage)
            .Build();


        [TempleDllLocation(0x102eb9a8)]
        public static readonly ConditionSpec MonsterMeleeParalysis = ConditionSpec.Create("Monster Melee Paralysis", 2)
            .Prevents(MonsterMeleeParalysis)
            .AddHandler(DispatcherType.DealingDamage2, MonsterMeleeParalysisOnDamage)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 1)
            .Build();


        [TempleDllLocation(0x102eba18)]
        public static readonly ConditionSpec MonsterMeleeParalysisNoElf = ConditionSpec
            .Create("Monster Melee Paralysis No Elf", 2)
            .Prevents(MonsterMeleeParalysisNoElf)
            .AddHandler(DispatcherType.DealingDamage2, MonsterMeleeParalysisNoElfPreAdd)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 1)
            .Build();


        [TempleDllLocation(0x102eba88)]
        public static readonly ConditionSpec MonsterEnergyImmunity = ConditionSpec.Create("Monster Energy Immunity", 1)
            .Prevents(MonsterEnergyImmunity)
            .AddHandler(DispatcherType.TakingDamage2, MonsterEnergyImmunityOnDamage)
            .AddQueryHandler(D20DispatcherKey.QUE_AI_Fireball_OK, AllowAiToPlaceInFireAoe)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ebaf8)]
        public static readonly ConditionSpec MonsterEnergyResistance = ConditionSpec
            .Create("Monster Energy Resistance", 2)
            .Prevents(MonsterEnergyImmunity)
            .AddHandler(DispatcherType.TakingDamage2, MonsterEnergyResistanceOnDamage)
            .AddQueryHandler(D20DispatcherKey.QUE_AI_Fireball_OK, AllowAiToPlaceInFireAoe)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 1)
            .Build();


        [TempleDllLocation(0x102ebb78)]
        public static readonly ConditionSpec MonsterRegeneration5 = ConditionSpec.Create("Monster Regeneration 5", 2)
            .Prevents(MonsterRegeneration5)
            .AddHandler(DispatcherType.TakingDamage2, MonsterRegenerationOnDamage)
            .AddHandler(DispatcherType.BeginRound, RegenerationBeginRound, 5)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 1)
            .Build();


        [TempleDllLocation(0x102ebbf8)]
        public static readonly ConditionSpec MonsterRegeneration2 = ConditionSpec.Create("Monster Regeneration 2", 2)
            .Prevents(MonsterRegeneration5)
            .AddHandler(DispatcherType.TakingDamage2, MonsterRegenerationOnDamage)
            .AddHandler(DispatcherType.BeginRound, RegenerationBeginRound, 2)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 1)
            .Build();


        [TempleDllLocation(0x102ebc78)]
        public static readonly ConditionSpec MonsterRegeneration1 = ConditionSpec.Create("Monster Regeneration 1", 2)
            .Prevents(MonsterRegeneration5)
            .AddHandler(DispatcherType.TakingDamage2, MonsterRegenerationOnDamage)
            .AddHandler(DispatcherType.BeginRound, RegenerationBeginRound, 1)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 1)
            .Build();


        [TempleDllLocation(0x102ebcf8)]
        public static readonly ConditionSpec MonsterSalamander = ConditionSpec.Create("Monster Salamander", 0)
            .Prevents(MonsterSalamander)
            .AddHandler(DispatcherType.TakingDamage2, SalamanderTakingDamageReactionDamage)
            .SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
            .Build();


        [TempleDllLocation(0x102ebd50)]
        public static readonly ConditionSpec MonsterOozeSplit = ConditionSpec.Create("Monster Ooze Split", 0)
            .Prevents(MonsterSalamander)
            .AddHandler(DispatcherType.TakingDamage2, MonsterOozeSplittingOnDamage)
            .SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
            .Build();


        [TempleDllLocation(0x102ebda8)]
        public static readonly ConditionSpec MonsterSplitting = ConditionSpec.Create("Monster Splitting", 0)
            .Prevents(MonsterSplitting)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, MonsterSplittingHpChange)
            .Build();


        [TempleDllLocation(0x102ebdf0)]
        public static readonly ConditionSpec MonsterJuggernaut = ConditionSpec.Create("Monster Juggernaut", 0)
            .Prevents(MonsterJuggernaut)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
            .Prevents(StatusEffects.Poisoned)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
            .Prevents(StatusEffects.Paralyzed)
            .Prevents(StatusEffects.Stunned)
            .Prevents(StatusEffects.IncubatingDisease)
            .Prevents(StatusEffects.NSDiseased)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Death_Touch, true)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
            .AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
            .Prevents(StatusEffects.TempAbilityLoss)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Energy_Drain, true)
            .AddHandler(DispatcherType.TakingDamage2, ImmunityToAcidElectricityFireDamageCallback)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_No_Con_Score, true)
            .Build();


        [TempleDllLocation(0x102ebf50)]
        public static readonly ConditionSpec MonsterSpellResistance = ConditionSpec
            .Create("Monster Spell Resistance", 1)
            .Prevents(MonsterSpellResistance)
            .AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.SpellResistanceDebug)
            .AddHandler(DispatcherType.SpellResistanceMod, CommonConditionCallbacks.SpellResistanceMod_Callback, 5048)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance,
                CommonConditionCallbacks.SpellResistanceQuery)
            .AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipSpellResistanceCallback, 5048)
            .SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ebff8)]
        public static readonly ConditionSpec MonsterSmiting = ConditionSpec.Create("Monster Smiting", 1)
            .Prevents(MonsterSmiting)
            .AddHandler(DispatcherType.DealingDamage, MonsterSmitingDealingDamage)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST,
                CommonConditionCallbacks.CondNodeSetArgToZero)
            .Build();


        [TempleDllLocation(0x102ec050)]
        public static readonly ConditionSpec MonsterZombie = ConditionSpec.Create("Monster Zombie", 0)
            .AddHandler(DispatcherType.TurnBasedStatusInit,
                CommonConditionCallbacks.turnBasedStatusInitSingleActionOnly)
            .Build();


        [TempleDllLocation(0x102ec080)]
        public static readonly ConditionSpec MonsterLamia = ConditionSpec.Create("Monster Lamia", 0)
            .AddHandler(DispatcherType.DealingDamage2, LamiaDealingDamage)
            .Build();


        [TempleDllLocation(0x102ec0b0)]
        public static readonly ConditionSpec MonsterDRCold = ConditionSpec.Create("Monster DR Cold", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, D20AttackPower.MITHRIL)
            .AddHandler(DispatcherType.DealingDamage, AddAttackPower, D20AttackPower.MITHRIL)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec108)]
        public static readonly ConditionSpec MonsterDRColdHoly = ConditionSpec.Create("Monster DR Cold-Holy", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback,
                D20AttackPower.MITHRIL | D20AttackPower.MAGIC)
            .AddHandler(DispatcherType.DealingDamage, AddAttackPower, D20AttackPower.MITHRIL | D20AttackPower.MAGIC)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec160)]
        public static readonly ConditionSpec MonsterDRMagic = ConditionSpec.Create("Monster DR Magic", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, D20AttackPower.SILVER)
            .AddHandler(DispatcherType.DealingDamage, AddAttackPower, D20AttackPower.SILVER)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec1b8)]
        public static readonly ConditionSpec MonsterDRAll = ConditionSpec.Create("Monster DR All", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, D20AttackPower.NORMAL)
            .AddHandler(DispatcherType.DealingDamage, AddAttackPower, D20AttackPower.NORMAL)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec210)]
        public static readonly ConditionSpec MonsterDRSilver = ConditionSpec.Create("Monster DR Silver", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, D20AttackPower.UNSPECIFIED)
            .AddHandler(DispatcherType.DealingDamage, AddAttackPower, D20AttackPower.UNSPECIFIED)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec268)]
        public static readonly ConditionSpec MonsterDRHoly = ConditionSpec.Create("Monster DR Holy", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, D20AttackPower.UNHOLY)
            .AddHandler(DispatcherType.DealingDamage, AddAttackPower, D20AttackPower.UNHOLY)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec34c)]
        public static readonly ConditionSpec MonsterSuperiorTwoWeaponFighting = ConditionSpec
            .Create("Monster Superior Two Weapon Fighting", 0)
            .AddHandler(DispatcherType.ToHitBonus2, SuperiorTwoWeaponFighting)
            .Build();


        [TempleDllLocation(0x102ec37c)]
        public static readonly ConditionSpec MonsterStable = ConditionSpec.Create("Monster Stable", 0)
            .AddHandler(DispatcherType.AbilityCheckModifier, CommonConditionCallbacks.AbilityModCheckStabilityBonus)
            .Build();


        [TempleDllLocation(0x102ec46c)]
        public static readonly ConditionSpec MonsterUntripable = ConditionSpec.Create("Monster Untripable", 0)
            .SetQueryResult(D20DispatcherKey.QUE_Untripable, true)
            .Build();


        [TempleDllLocation(0x102ec3b0)]
        public static readonly ConditionSpec MonsterPlant = ConditionSpec.Create("Monster Plant", 0)
            .Prevents(MonsterPlant)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 1, 0)
            .Prevents(StatusEffects.Poisoned)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
            .Prevents(StatusEffects.Paralyzed)
            .Prevents(StatusEffects.Stunned)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
            .Build();


        [TempleDllLocation(0x102ec49c)]
        public static readonly ConditionSpec MonsterHootingFungi = ConditionSpec.Create("Monster Hooting Fungi", 0)
            .AddHandler(DispatcherType.TakingDamage2, HootingFunghiTakingDamage)
            .Build();


        [TempleDllLocation(0x102ec4d0)]
        public static readonly ConditionSpec MonsterSpider = ConditionSpec.Create("Monster Spider", 0)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 2, 0)
            .Build();


        [TempleDllLocation(0x102ec514)]
        public static readonly ConditionSpec MonsterIncorporeal = ConditionSpec.Create("Monster Incorporeal", 0)
            .AddHandler(DispatcherType.TakingDamage2, MonsterIncorporealDamageCallback)
            .Build();


        [TempleDllLocation(0x102ec548)]
        public static readonly ConditionSpec MonsterMinotaurCharge = ConditionSpec.Create("Monster Minotaur Charge", 0)
            .AddHandler(DispatcherType.DealingDamage, MinotaurChargeCallback)
            .AddQueryHandler(D20DispatcherKey.QUE_Play_Critical_Hit_Anim, MinotaurChargePlayCriticalHitAnim)
            .Build();


        [TempleDllLocation(0x102ec590)]
        public static readonly ConditionSpec MonsterFastHealing = ConditionSpec.Create("Monster Fast Healing", 1)
            .AddHandler(DispatcherType.BeginRound, FastHealingBeginRound)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec5d8)]
        public static readonly ConditionSpec MonsterPoisonImmunity = ConditionSpec.Create("Monster Poison Immunity", 0)
            .Prevents(StatusEffects.Poisoned)
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
            .Build();


        [TempleDllLocation(0x102ec2c0)]
        public static readonly ConditionSpec MonsterDRBludgeoning = ConditionSpec.Create("Monster DR Bludgeoning", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, D20AttackPower.ADAMANTIUM)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec308)]
        public static readonly ConditionSpec MonsterDRSlashing = ConditionSpec.Create("Monster DR Slashing", 1)
            .AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, D20AttackPower.PIERCING)
            .AddQueryHandler(D20DispatcherKey.SIG_Verify_Obj_Conditions, VerifyArgIsGreaterThanZero, 0)
            .Build();


        [TempleDllLocation(0x102ec620)]
        public static readonly ConditionSpec MonsterSubdualImmunity = ConditionSpec
            .Create("Monster Subdual Immunity", 0)
            .Prevents(MonsterSubdualImmunity)
            .AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
            .Build();


        [TempleDllLocation(0x102ec668)]
        public static readonly ConditionSpec MonsterSpecialFadeOut = ConditionSpec.Create("Monster Special Fade Out", 0)
            .Prevents(MonsterSpecialFadeOut)
            .AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, SpecialFadeOutDamageTaken)
            .Build();


        [TempleDllLocation(0x102ec6c0)]
        public static readonly ConditionSpec MonsterConfusionImmunity = ConditionSpec
            .Create("Monster Confusion Immunity", 0)
            .Prevents(MonsterConfusionImmunity)
            .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL,
                CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
            .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 3, 0)
            .Build();


        public static IReadOnlyList<ConditionSpec> Conditions { get; } = new List<ConditionSpec>
        {
            MonsterDRSlashing,
            MonsterStable,
            MonsterLamia,
            MonsterSmiting,
            MonsterRegeneration5,
            MonsterMeleeDisease,
            MonsterRegeneration1,
            MonsterBansheeCharismaDrain,
            MonsterMeleeParalysisNoElf,
            MonsterRegeneration2,
            MonsterSalamander,
            MonsterOozeSplit,
            MonsterPlant,
            MonsterUntripable,
            MonsterFastHealing,
            MonsterDRHoly,
            MonsterSpecialFadeOut,
            MonsterDRMagic,
            MonsterDRAll,
            MonsterDRCold,
            MonsterDRSilver,
            MonsterJuggernaut,
            MonsterMeleeParalysis,
            MonsterStirge,
            MonsterIncorporeal,
            MonsterEnergyImmunity,
            MonsterDamageType,
            MonsterPoisonImmunity,
            MonsterCarrionCrawler,
            MonsterSubdualImmunity,
            MonsterEnergyResistance,
            MonsterHootingFungi,
            MonsterSpider,
            MonsterSplitting,
            MonsterSuperiorTwoWeaponFighting,
            MonsterSpellResistance,
            MonsterFireBats,
            MonsterZombie,
            MonsterDRBludgeoning,
            MonsterMinotaurCharge,
            MonsterConfusionImmunity,
            MonsterDRColdHoly,
            MonsterMeleePoison,
            MonsterBonusDamage,
        };

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f6930)]
        public static void SetMonsterDamageType(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var damageType = (DamageType) evt.GetConditionArg1();
            dispIo.damage.SetDamageType(damageType);
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100f7870)]
        public static void SuperiorTwoWeaponFighting(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var attackCode = dispIo.attackPacket.dispKey;
            if (attackCode == 6 || attackCode == 8)
            {
                dispIo.bonlist.AddBonus(10, 0, 114);
            }

            if (attackCode == 5 || attackCode == 7 || attackCode == 9)
            {
                dispIo.bonlist.AddBonus(6, 0, 114);
            }
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f7030)]
        public static void ConditionAddOnDamage(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoDamage();
            if (((dispIo.attackPacket.flags & D20CAF.RANGED) == 0) && dispIo.attackPacket.dispKey >= 10)
            {
                var victim = dispIo.attackPacket.victim;
                if (data == StatusEffects.IncubatingDisease)
                {
                    victim.AddCondition(data, 0, condArg1, 0);
                }
                else
                {
                    victim.AddCondition(data, condArg1, 0);
                }
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f72e0)]
        [TemplePlusLocation("condition.cpp:505")]
        public static void MonsterRegenerationOnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var arg0 = evt.GetConditionArg1();
            var arg1 = evt.GetConditionArg2();

            if (arg0 == 0 && arg1 == 0)
            {
                arg0 = (int) DamageType.Fire;
                arg1 = (int) DamageType.Acid;
                evt.SetConditionArg1(arg0);
                evt.SetConditionArg2(arg1);
            }

            if (arg0 < 0)
            {
                var attackPowerType = (D20AttackPower) (arg0 & 0x7fffFFFF);
                if (attackPowerType == dispIo.damage.attackPowerType)
                {
                    return;
                }

                arg0 = arg1;
            }

            var replacedDice = false;
            for (var i = 0; i < dispIo.damage.dice.Count; i++)
            {
                if (dispIo.damage.dice[i].type != (DamageType) arg0
                    && dispIo.damage.dice[i].type != (DamageType) arg1)
                {
                    replacedDice = true;
                    var dice = dispIo.damage.dice[i];
                    dice.type = DamageType.Subdual;
                    dispIo.damage.dice[i] = dice;
                }
            }

            if (replacedDice)
            {
                dispIo.damage.bonuses.zeroBonusSetMeslineNum(322);
            }
        }

        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f71d0)]
        public static void MonsterMeleeParalysisNoElfPreAdd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            if (dispIo.attackPacket.victim.GetStat(Stat.spell_list_level) != 2)
            {
                MonsterMeleeParalysisOnDamage(in evt);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100efdf0)]
        public static void VerifyArgIsGreaterThanZero(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetConditionArg(data) <= 0)
            {
                var value = evt.GetConditionArg(data);
                Logger.Info("Invalid value for {0} on {1}.  {2} is not greater than 0",
                    evt.subDispNode.condNode.condStruct.condName, evt.objHndCaller, value);
            }
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f6a70)]
        public static void sub_100F6A70(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg1()) == 0)
            {
                StirgeDealingDamage(in evt);
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100f7aa0)]
        public static void FastHealingBeginRound(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg1();
            var amount = 10 * evt.GetDispIoD20Signal().data1;
            if (!GameSystems.Critter.IsDeadNullDestroyed(evt.objHndCaller)
                && evt.objHndCaller.GetInt32(obj_f.hp_damage) > 0
                && evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage) > 0)
            {
                var dice = Dice.Constant(amount);
                GameSystems.Combat.Heal(evt.objHndCaller, evt.objHndCaller, dice, 0);
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x39, evt.objHndCaller, null);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f7550)]
        [TemplePlusLocation("ability_fixes.cpp:76")]
        public static void MonsterSplittingHpChange(in DispatcherCallbackArgs evt)
        {
            var protoId = evt.objHndCaller.ProtoId;
            var obj = evt.objHndCaller;
            var loc = obj.GetLocationFull();
            var hpCur = evt.objHndCaller.GetStat(Stat.hp_current);
            var baseHp = obj.GetInt32(obj_f.hp_pts);
            var hpDam = obj.GetInt32(obj_f.hp_damage);
            var hpMod = (hpCur + hpDam) - baseHp;

            if (hpCur <= 10)
            {
                return;
            }

            obj.SetInt32(obj_f.hp_pts, baseHp / 2);
            obj.SetInt32(obj_f.hp_damage, (hpDam + hpMod) / 2);

            var protoHandle = GameSystems.Proto.GetProtoById((ushort) protoId);
            var newMonster = GameSystems.MapObject.CreateObject(protoHandle, loc.location);
            newMonster.SetInt32(obj_f.hp_pts, baseHp / 2);
            newMonster.SetInt32(obj_f.hp_damage, (hpDam + hpMod) / 2);
            newMonster.SetCritterFlags(newMonster.GetCritterFlags() | CritterFlag.EXPERIENCE_AWARDED);

            GameSystems.ParticleSys.CreateAtObj("hit-Acid-medium", evt.objHndCaller);
            evt.RemoveThisCondition();
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f7840)]
        public static void AddAttackPower(in DispatcherCallbackArgs evt, D20AttackPower attackPower)
        {
            var dispIo = evt.GetDispIoDamage();
            // TODO: Why is this always adding silver
            dispIo.damage.AddAttackPower(attackPower | D20AttackPower.SILVER);
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f7110)]
        public static void MonsterMeleeParalysisOnDamage(in DispatcherCallbackArgs evt)
        {
            var dc = evt.GetConditionArg1();
            var roundsDice = evt.GetConditionDiceArg(1);
            var dispIo = evt.GetDispIoDamage();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) == 0
                && dispIo.attackPacket.victim.GetStat((Stat) 288) != 2
                && !GameSystems.D20.Combat.SavingThrow(dispIo.attackPacket.victim, dispIo.attackPacket.attacker, dc,
                    SavingThrowType.Fortitude))
            {
                var rounds = roundsDice.Roll();
                dispIo.attackPacket.victim.AddCondition(StatusEffects.Paralyzed, rounds, 0);
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100f7b30)]
        public static void SpecialFadeOutDamageTaken(in DispatcherCallbackArgs evt)
        {
            if (evt.objHndCaller.GetStat(Stat.hp_current) <= 0)
            {
                GameSystems.D20.Combat.Kill(evt.objHndCaller, null);
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f7a20)]
        public static void MonsterIncorporealDamageCallback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddEtherealImmunity();
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f7800)]
        public static void MonsterDRDamageCallback(in DispatcherCallbackArgs evt, D20AttackPower attackPower)
        {
            var amount = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddPhysicalDR(amount, attackPower, 126);
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100f6ab0)]
        public static void StirgeAttach_callback(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var target = evt.GetConditionObjArg(2);
            var roll = Dice.D4.Roll();
            if (condArg1 != 0)
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(30, evt.objHndCaller, target);
                GameSystems.D20.Combat.FloatCombatLine(target, 155);
                target.AddCondition(StatusEffects.TempAbilityLoss, (int) Stat.constitution, roll);

                var suckedAbilitySum = condArg2 + roll;
                evt.SetConditionArg2(suckedAbilitySum);

                if (target.GetStat(Stat.hp_current) >= 0 && evt.objHndCaller.GetStat(Stat.hp_current) >= 0)
                {
                    if (suckedAbilitySum >= 4)
                    {
                        // Stirge has had it's fill, lets run away from the party / opponent!!
                        evt.SetConditionArg1(0);
                        FleeFromTargetOrParty(evt.objHndCaller, target);
                    }
                }
                else
                {
                    evt.SetConditionArg1(0);
                }
            }
        }

        private static void FleeFromTargetOrParty(GameObjectBody npc, GameObjectBody target)
        {
            if (GameSystems.Party.IsInParty(target))
            {
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    GameSystems.AI.FleeFrom(npc, partyMember);
                }
            }
            else
            {
                GameSystems.AI.FleeFrom(npc, target);
            }
        }

        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f69a0)]
        public static void StirgeDealingDamage(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoDamage();
            if (dispIo.attackPacket.dispKey >= 10 && (condArg1) == 0)
            {
                evt.SetConditionObjArg(2, dispIo.attackPacket.victim);
                var target = evt.GetConditionObjArg(2);
                evt.SetConditionArg1(1);
                var targetName = GameSystems.MapObject.GetDisplayName(target, target);
                var attackerName = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, D20CombatMessage.attached, attackerName,
                    targetName);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f72b0)]
        public static void AllowAiToPlaceInFireAoe(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var immunityType = (DamageType) evt.GetConditionArg1();
            if (immunityType == DamageType.Fire)
            {
                dispIo.return_val = 1;
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100f6c80)]
        public static void FireBats_callback(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var currentRounds = evt.GetConditionArg2();
            var target = evt.GetConditionObjArg(2);
            if (condArg1 != 0)
            {
                var roundCounter = currentRounds + 1;
                evt.SetConditionArg2(roundCounter);

                if (target.GetStat(Stat.hp_current) >= 0 && evt.objHndCaller.GetStat(Stat.hp_current) >= 0)
                {
                    if (roundCounter > 3)
                    {
                        evt.SetConditionArg1(0);
                        FleeFromTargetOrParty(evt.objHndCaller, target);
                    }
                    else
                    {
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1E, evt.objHndCaller,
                            target);
                        GameSystems.D20.Combat.FloatCombatLine(target, 155);
                        GameSystems.D20.Combat.DoDamage(target, evt.objHndCaller, Dice.D6,
                            DamageType.Fire, D20AttackPower.NORMAL, 100, 128, D20ActionType.NONE);
                        GameSystems.D20.Combat.DoDamage(target, evt.objHndCaller, Dice.D2,
                            DamageType.Piercing, D20AttackPower.NORMAL, 100, 130, D20ActionType.NONE);
                    }
                }
                else
                {
                    evt.SetConditionArg1(0);
                }
            }
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100f6ff0)]
        public static void MonsterStigeAcBonusCap(in DispatcherCallbackArgs evt, int data)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoAttackBonus();
            if (condArg1 != 0)
            {
                dispIo.bonlist.AddCap(3, 0, 281);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f6960)]
        public static void MonsterDamageBonus(in DispatcherCallbackArgs evt)
        {
            var damageType = (DamageType) evt.GetConditionArg1();
            var damageDice = evt.GetConditionDiceArg(1);

            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddDamageDice(damageDice, damageType, 127);
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f7670)]
        public static void ImmunityToAcidElectricityFireDamageCallback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var damage = dispIo.damage;
            damage.AddModFactor(0F, DamageType.Acid, 132);
            damage.AddModFactor(0F, DamageType.Electricity, 132);
            damage.AddModFactor(0F, DamageType.Fire, 132);
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f7490)]
        [TemplePlusLocation("ability_fixes.cpp:77")]
        public static void MonsterOozeSplittingOnDamage(in DispatcherCallbackArgs evt)
        {
            var curHp = evt.objHndCaller.GetStat(Stat.hp_current);
            if (curHp <= 10)
                return;

            var dispIo = evt.GetDispIoDamage();
            var isSplitting = false;
            var protoId = evt.objHndCaller.ProtoId;
            if (protoId == WellKnownProtos.OchreJelly &&
                dispIo.damage.GetOverallDamageByType(DamageType.Electricity) > 0)
            {
                isSplitting = true;
                dispIo.damage.AddModFactor(0.0f, DamageType.Electricity, 132);
            }

            if (dispIo.damage.GetOverallDamageByType(DamageType.Slashing) > 0)
            {
                isSplitting = true;
                dispIo.damage.AddModFactor(0.0f, DamageType.Slashing, 132);
            }

            if (dispIo.damage.GetOverallDamageByType(DamageType.Piercing) > 0)
            {
                isSplitting = true;
                dispIo.damage.AddModFactor(0.0f, DamageType.Piercing, 132);
            }

            if (dispIo.damage.GetOverallDamageByType(DamageType.PiercingAndSlashing) > 0)
            {
                isSplitting = true;
                dispIo.damage.AddModFactor(0.0f, DamageType.PiercingAndSlashing, 132);
            }

            if (isSplitting)
            {
                evt.objHndCaller.AddCondition("Monster Splitting");
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f7a70)]
        public static void MinotaurChargePlayCriticalHitAnim(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var caf = (D20CAF) dispIo.data1;
            if ((caf & D20CAF.CHARGE) != default)
            {
                dispIo.return_val = 1;
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100efe50)]
        public static void VerifyObjConditionsCallback(in DispatcherCallbackArgs evt, int argIndex, int minValue)
        {
            var value = evt.GetConditionArg(argIndex);

            if (value <= minValue)
            {
                Logger.Info("Invalid value for {0} on {1}.  {2} is not at least {3}",
                    evt.subDispNode.condNode.condStruct.condName, evt.objHndCaller, value, minValue);
            }
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f77b0)]
        public static void LamiaDealingDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            if (GameSystems.D20.GetAttackWeapon(dispIo.attackPacket.attacker, dispIo.attackPacket.dispKey, 0) == null)
            {
                // TODO: This should be 1d4, not 1
                dispIo.attackPacket.victim.AddCondition(StatusEffects.TempAbilityLoss, (int) Stat.wisdom, 1);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f76c0)]
        public static void MonsterSmitingDealingDamage(in DispatcherCallbackArgs evt)
        {
            var alreadyUsedToday = evt.GetConditionArg1();
            var attacker = evt.objHndCaller;
            var level = attacker.GetStat(Stat.level);
            if (alreadyUsedToday == 0)
            {
                evt.SetConditionArg1(1);

                var dispIo = evt.GetDispIoDamage();
                var victim = dispIo.attackPacket.victim;
                if (attacker.HasGoodAlignment() && victim.HasEvilAlignment())
                {
                    dispIo.damage.AddDamageBonus(level, 0, 300);
                }
                else if (attacker.HasEvilAlignment() && victim.HasGoodAlignment())
                {
                    dispIo.damage.AddDamageBonus(level, 0, 301);
                }
            }
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f7260)]
        public static void MonsterEnergyResistanceOnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var damageType = (DamageType) evt.GetConditionArg1();
            var amount = evt.GetConditionArg2();
            dispIo.damage.AddDR(amount, damageType, 124);
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f70a0)]
        public static void CarrionCrawlerParalysisOnDamage(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoDamage();
            var attackCode = dispIo.attackPacket.dispKey;
            if (attackCode >= 10 && attackCode <= 17 &&
                !GameSystems.D20.Combat.SavingThrow(dispIo.attackPacket.victim, dispIo.attackPacket.attacker, 13, 0))
            {
                var rounds = Dice.Roll(2, 6);
                dispIo.attackPacket.victim.AddCondition(StatusEffects.Paralyzed, rounds, 0);
            }
        }

        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100f6e80)]
        public static void FirebatsTooltip(in DispatcherCallbackArgs evt, int data)
        {
            if (evt.GetConditionArg1() != 0)
            {
                var target = evt.GetConditionObjArg(2);
                var dispIo = evt.GetDispIoTooltip();
                var text = GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.attached);
                var targetName = GameSystems.MapObject.GetDisplayName(target, target);

                dispIo.Append(text + targetName);
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100f7350)]
        public static void RegenerationBeginRound(in DispatcherCallbackArgs evt, int amountPerRound)
        {
            if (!GameSystems.Critter.IsDeadNullDestroyed(evt.objHndCaller))
            {
                var totalAmount = amountPerRound * evt.GetDispIoD20Signal().data1;
                if (evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage) > 0)
                {
                    if (GameSystems.Combat.IsCombatActive())
                    {
                        GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, totalAmount);
                    }
                    else
                    {
                        GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, 1);
                    }

                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(56, evt.objHndCaller, null);
                    GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, 0);
                }
            }
        }


        [DispTypes(DispatcherType.DealingDamage2)]
        [TempleDllLocation(0x100f67d0)]
        public static void BansheeCharismaDrainOnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            if (dispIo.attackPacket.dispKey < AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1)
            {
                return;
            }

            var attacker = dispIo.attackPacket.attacker;
            var tgt = dispIo.attackPacket.victim;
            // looks like by the rules there shouldn't even be a save? (Mon. Man. Ghost entry)

            var curHp = attacker.GetStat(Stat.hp_current);
            var maxHp = attacker.GetStat(Stat.hp_max);
            int tempHpGain = 5;
            var chaDrainDice = 1;
            if ((dispIo.attackPacket.flags & D20CAF.CRITICAL) != D20CAF.NONE)
            {
                tempHpGain = 10;
                chaDrainDice = 2;
            }

            var chaDrainAmt = new Dice(chaDrainDice, 4).Roll();

            var hpDam = maxHp - curHp;

            // add Temporary HPs
            if (tempHpGain >= hpDam)
            {
                attacker.AddCondition("Temporary_Hit_Points", 0, 14400, tempHpGain - hpDam);
                tempHpGain -= tempHpGain - hpDam;
            }

            // heal normal damage if applicable
            if (tempHpGain > 0)
            {
                GameSystems.Combat.Heal(attacker, tgt, Dice.Constant(tempHpGain), D20ActionType.NONE);
            }

            tgt.AddCondition("Temp_Ability_Loss", Stat.charisma, chaDrainAmt);
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f7220)]
        public static void MonsterEnergyImmunityOnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var damageType = (DamageType) evt.GetConditionArg1();
            dispIo.damage.AddModFactor(0F, damageType, 132);
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100f7a40)]
        public static void MinotaurChargeCallback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var flags = dispIo.attackPacket.flags;
            if ((flags & D20CAF.CHARGE) != 0)
            {
                var dice = new Dice(4, 6, 1);
                dispIo.damage.AddDamageDice(dice, DamageType.Piercing, 117);
            }
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f7910)]
        public static void HootingFunghiTakingDamage(in DispatcherCallbackArgs evt)
        {
            if (evt.GetDispIoDamage().damage.finalDamage > 0)
            {
                GameSystems.ParticleSys.CreateAtObj("mon-Hooting Cloud", evt.objHndCaller);

                using var listResult = ObjList.ListVicinity(evt.objHndCaller, ObjectListFilter.OLC_CRITTERS);
                foreach (var target in listResult)
                {
                    if (evt.objHndCaller.DistanceToObjInFeet(target) <= 10.0f)
                    {
                        var duration = Dice.D4.Roll();
                        target.AddCondition(StatusEffects.Blindness, duration, 0);
                    }
                }
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100f6f80)]
        public static void BatsNoAooWhenAttached(in DispatcherCallbackArgs evt)
        {
            var attached = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoD20Query();
            if (attached != 0)
            {
                dispIo.return_val = 0;
            }
        }

        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100f6fb0)]
        public static void BatsNoActionsWhenAttached(in DispatcherCallbackArgs evt)
        {
            if (evt.GetConditionArg1() != 0)
            {
                CommonConditionCallbacks.turnBasedStatusInitNoActions(in evt);
            }
        }

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100f73e0)]
        public static void SalamanderTakingDamageReactionDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != 0)
            {
                return;
            }

            var actionType = dispIo.attackPacket.d20ActnType;
            if (actionType == D20ActionType.STANDARD_ATTACK || actionType == D20ActionType.FULL_ATTACK)
            {
                var attackCode = dispIo.attackPacket.dispKey;
                var weapon = GameSystems.D20.GetAttackWeapon(dispIo.attackPacket.attacker, attackCode, 0);
                if (weapon == null || weapon.type == ObjectType.weapon && weapon.GetMaterial() == Material.metal)
                {
                    var dice = Dice.D6;
                    GameSystems.D20.Combat.DoDamage(dispIo.attackPacket.attacker, evt.objHndCaller, dice,
                        DamageType.Fire, D20AttackPower.NORMAL, 100, 0x80, D20ActionType.NONE);
                }
            }
        }
    }
}