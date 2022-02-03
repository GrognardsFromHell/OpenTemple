
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    public class FavoredPowerAttack
    {

        public static void favoredPowerAttackDamageBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var weapon = dispIo.attackPacket.GetWeaponUsed();
            var wieldType = GameSystems.Item.GetWieldType(evt.objHndCaller, weapon);
            // No Power attack for light weapons or ranged weapons
            if ((wieldType != 0) && !GameSystems.Item.IsRangedWeapon(weapon))
            {
                var target = dispIo.attackPacket.victim;

                if (FavoredEnemies.GetFavoredEnemyBonusAgainst(evt.objHndCaller, target, out _, out _))
                {
                    // Bonus Value Based on power attack selection
                    var PowerAttackValue = (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller, "Power Attack Value");
                    if (PowerAttackValue != 0)
                    {
                        // Add 1x more power attack for one or two handed for a total of x2 or x3 damage
                        dispIo.damage.bonuses.AddBonusFromFeat(PowerAttackValue, 0, 114, (FeatId) ElfHash.Hash("Favored Power Attack"));
                    }

                }

            }
        }

        // args are just-in-case placeholders
        [FeatCondition("Favored Power Attack")]
        [AutoRegister] public static readonly ConditionSpec favoredPowerAttack = ConditionSpec.Create("Favored Power Attack", 2)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, favoredPowerAttackDamageBonus)
            .Build();
    }
}
