using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static class TemplePlusItemEffects
    {
        public static readonly ConditionSpec WeaponSeeking = ConditionSpec.Create("Weapon Seeking", 3)
            .AddHandler(DispatcherType.GetAttackerConcealmentMissChance, WeaponSeekingAttackerConcealmentMissChance)
            .SupportHasConditionQuery()
            .Build();

        [TemplePlusLocation("ItemCallbacks::WeaponSeekingAttackerConcealmentMissChance")]
        private static void WeaponSeekingAttackerConcealmentMissChance(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddCap(0, 0, 347);
        }

        public static readonly ConditionSpec WeaponSpeed = ConditionSpec.Create("Weapon Speed", 3)
            .AddHandler(DispatcherType.GetBonusAttacks, WeaponSpeedBonusAttacks)
            .Build();

        [TemplePlusLocation("ItemCallbacks::WeaponSpeed")]
        private static void WeaponSpeedBonusAttacks(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            dispIo.bonlist.AddBonus(1, 34, 346); // Weapon of Speed
        }

        public static readonly ConditionSpec WeaponThundering = ConditionSpec.Create("Weapon Thundering", 4)
            .AddHandler(DispatcherType.DealingDamage, WeaponThunderingDealingDamage)
            .Build();

        [TemplePlusLocation("ItemCallbacks::WeaponThundering")]
        private static void WeaponThunderingDealingDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            var attacker = dispIo.attackPacket.attacker;
            if (attacker == null)
                return;
            var victim = dispIo.attackPacket.victim;

            var invIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(attacker, invIdx);

            var weapUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weapUsed == null)
                return;

            if (weapUsed == item)
            {
                if ((dispIo.attackPacket.flags & D20CAF.CRITICAL) != 0)
                {
                    var critMultiplier = item.GetInt32(obj_f.weapon_crit_hit_chart);
                    dispIo.damage.AddDamageDice(new Dice(critMultiplier - 1, 8), DamageType.Sonic, 121);
                    GameSystems.SoundGame.PositionalSound(100000, victim); // thunderclap.mp3
                    if (!GameSystems.D20.Combat.SavingThrow(victim, attacker, 14, SavingThrowType.Fortitude))
                    {
                        victim.AddCondition(SpellEffects.SpellDeafness, 0, 0, 0);
                    }
                }
            }
        }

        public static readonly ConditionSpec WeaponVicious = ConditionSpec.Create("Weapon Vicious", 3)
            .AddHandler(DispatcherType.DealingDamage, WeaponViciousDealingDamage)
            .AddHandler(DispatcherType.DealingDamage2, WeaponViciousBlowback)
            .Build();

        [TemplePlusLocation("ItemCallbacks::WeaponVicious")]
        private static void WeaponViciousDealingDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            var attacker = dispIo.attackPacket.attacker;
            if (attacker == null)
                return;

            var invIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(attacker, invIdx);
            var dice = new Dice(2, 6);

            var weapUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weapUsed == null)
                return;

            if (weapUsed == item)
            {
                var itemName = GameSystems.MapObject.GetDisplayName(item);
                dispIo.damage.AddDamageDice(dice, DamageType.Magic, 121, itemName);
            }
        }

        [TemplePlusLocation("ItemCallbacks::WeaponViciousBlowback")]
        private static void WeaponViciousBlowback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            var attacker = dispIo.attackPacket.attacker;
            if (attacker == null)
                return;

            var invIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(attacker, invIdx);
            var dice = new Dice(2, 6);

            var weapUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weapUsed == null)
                return;

            if (weapUsed == item)
            {
                //	auto counterDam = Dice(1, 6);
                //	damage.DealDamage(attacker, 0i64, counterDam, DamageType::Magic, (int)AttackPowerType::Unspecified, 0, 100, D20A_NONE);
                //not that simply unfortunately !
            }
        }

        public static readonly ConditionSpec WeaponWounding = ConditionSpec.Create("Weapon Wounding", 3)
            .AddHandler(DispatcherType.DealingDamage2, WeaponWoundingDealingDamage)
            .Build();

        [TemplePlusLocation("ItemCallbacks::WeaponWounding")]
        private static void WeaponWoundingDealingDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            var attacker = dispIo.attackPacket.attacker;
            if (attacker == null)
                return;
            var victim = dispIo.attackPacket.victim;

            var invIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(attacker, invIdx);

            var weapUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weapUsed == null)
                return;

            if (weapUsed == item)
            {
                if (victim.D20Query(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits))
                {
                    return;
                }

                var victimName = GameSystems.MapObject.GetDisplayNameForParty(victim);
                GameSystems.RollHistory.CreateFromFreeText($"{victimName} takes 1 Con damage.\n");
                victim.AddCondition(StatusEffects.DamageAbilityLoss, 2, 1);
            }
        }

        public static readonly ConditionSpec WeaponMerciful = ConditionSpec.Create("Weapon Merciful", 4)
            .AddHandler(DispatcherType.DealingDamage, WeaponMercifulDealingDamage)
            .Build();

        [TemplePlusLocation("ItemCallbacks::WeaponMerciful")]
        private static void WeaponMercifulDealingDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            var attacker = dispIo.attackPacket.attacker;
            if (attacker == null)
            {
                return;
            }

            var invIdx = evt.GetConditionArg3();
            var item = GameSystems.Item.GetItemAtInvIdx(attacker, invIdx);
            var dice = Dice.D6;

            var weapUsed = dispIo.attackPacket.GetWeaponUsed();
            if (weapUsed == null)
            {
                return;
            }

            if (weapUsed == item)
            {
                dispIo.damage.AddDamageDice(dice, DamageType.Subdual, 121);
            }
        }

        public static readonly ConditionSpec[] Conditions =
        {
            WeaponSeeking,
            WeaponSpeed,
            WeaponThundering,
            WeaponVicious,
            WeaponWounding
        };
    }
}