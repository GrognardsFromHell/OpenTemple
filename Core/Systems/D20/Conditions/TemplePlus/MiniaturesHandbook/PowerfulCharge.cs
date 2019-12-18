
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
