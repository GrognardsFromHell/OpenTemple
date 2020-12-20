using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    public class GreaterTwoWeaponDefense
    {
        public static void TwoWeaponDefenseAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var acBonus = 3;
            var weaponPrimary = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponPrimary);
            var weaponSecondary = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponSecondary);
            // Weapons must not be the same (How would this even be possible ?!)
            if (weaponPrimary == weaponSecondary)
            {
                return;
            }

            // Both hands must have a weapon
            if (weaponPrimary == null || weaponSecondary == null)
            {
                return;
            }

            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_FightingDefensively)) // this also covers Total Defense
            {
                acBonus = acBonus * 2;
            }

            dispIo.bonlist.AddBonus(acBonus, 29, "Greater Two-Weapon Defense"); // Shield Bonus
        }

        // args are just-in-case placeholders
        [FeatCondition("Greater Two-Weapon Defense")]
        [AutoRegister] public static readonly ConditionSpec greaterTwoWeaponDefense = ConditionSpec
            .Create("Greater Two-Weapon Defense", 2)
            .SetUnique()
            .AddHandler(DispatcherType.GetAC, TwoWeaponDefenseAcBonus)
            .Build();
    }
}
