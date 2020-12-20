using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class PowerCritical
    {
        private static void PowerCriticalBonus(in DispatcherCallbackArgs evt, WeaponType selectedType)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var weaponObj = dispIo.attackPacket.GetWeaponUsed();
            // Get the weapon type or set to the appropriate unarmed weapon type
            WeaponType usedType;
            if (weaponObj != null)
            {
                usedType = weaponObj.GetWeaponType();
            }
            else
            {
                var size = (SizeCategory) evt.objHndCaller.GetInt(obj_f.size);
                if (size == SizeCategory.Small)
                {
                    usedType = WeaponType.unarmed_strike_small_being;
                }
                else
                {
                    usedType = WeaponType.unarmed_strike_medium_sized_being;
                }
            }

            if (selectedType == usedType)
            {
                dispIo.bonlist.AddBonusFromFeat(4, 0, 114, (FeatId) ElfHash.Hash("Power Critical"));
            }
        }

        // TODO: Register
        public IEnumerable<ConditionSpec> CreateConditions()
        {
            for (var weapon = WeaponType.gauntlet; weapon < WeaponType.mindblade; weapon++)
            {
                if (GameSystems.Feat.TryGetFeatForWeaponType("Power Critical", weapon, out var featEnum))
                {
                    var featName = GameSystems.Feat.GetFeatName(featEnum);
                    yield return ConditionSpec.Create(featName, 3)
                        .AddHandler(DispatcherType.ConfirmCriticalBonus, PowerCriticalBonus, weapon)
                        .Build();
                }
            }
        }
    }
}
