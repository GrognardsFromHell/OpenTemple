
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

    public class PowerfulCharge
    {

        public static void PowerfulChargeBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // Reset the already used this round flag
            evt.SetConditionArg1(0);
            return;
        }

        public static void PowerfulChargeDamageBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            // already used this round, do nothing
            if (evt.GetConditionArg1() != 0)
            {
                return;
            }

            // Must be charging
            var charging = evt.objHndCaller.D20Query("Charging");
            if (!charging)
            {
                return;
            }

            var size = (SizeCategory) evt.objHndCaller.GetStat(Stat.size);
            // Size must be medium or larger
            if ((size > SizeCategory.Colossal) || (size < SizeCategory.Medium))
            {
                return;
            }

            // One extra size category for greater powerful charge
            if (evt.objHndCaller.HasFeat((FeatId) ElfHash.Hash("Greater Powerful Charge")))
            {
                size = size + 1;
            }

            // Damage Dice determined based on size
            Dice dice;
            if (size == SizeCategory.Medium)
            {
                dice = new Dice(1, 8);
            }
            else if (size == SizeCategory.Large)
            {
                dice = new Dice(2, 6);
            }
            else if (size == SizeCategory.Huge)
            {
                dice = new Dice(3, 6);
            }
            else if (size == SizeCategory.Huge)
            {
                dice = new Dice(4, 6);
            }
            else if (size == SizeCategory.Gargantuan)
            {
                dice = new Dice(5, 6);
            }
            else if (size == SizeCategory.Colossal)
            {
                dice = new Dice(6, 6);
            }
            else
            {
                // else:  #Colossal with powerful charge
                dice = new Dice(8, 6);
            }

            dispIo.damage.AddDamageDice(dice, DamageType.Unspecified, 127);
            // set the already used this round flag
            evt.SetConditionArg1(1);
        }

        // userd this round, spare
        [FeatCondition("Powerful Charge")]
        [AutoRegister] public static readonly ConditionSpec powerfulCharge = ConditionSpec.Create("Powerful Charge", 2)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, PowerfulChargeDamageBonus)
            .AddHandler(DispatcherType.BeginRound, PowerfulChargeBeginRound)
            .Build();
    }
}
