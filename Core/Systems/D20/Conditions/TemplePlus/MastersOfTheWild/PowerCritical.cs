using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
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
