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
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class MonsterMeleePoisonEx
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public static void OnDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();

            // print(GetConditionName() + " start...")
            var attackNumAllowed = evt.GetConditionArg2();
            attackNumAllowed = attackNumAllowed + 1000;
            // print(GetConditionName() + " attackNumAllowed (+1000): " + str(attackNumAllowed))
            // print(GetConditionName() + " dispIo.attack_packet.event_key: " + str(dispIo.attack_packet.event_key))
            if (((attackNumAllowed == 999) || (dispIo.attackPacket.dispKey == attackNumAllowed)))
            {
                var poisonId = evt.GetConditionArg1();
                // print(GetConditionName() + " poisonId: " + str(poisonId))
                // print(GetConditionName() + " adding condition Poisoned...")
                dispIo.attackPacket.victim.AddCondition("Poisoned", poisonId, 0);
            }
            else
            {
                Logger.Info("{0}", ("Monster Melee Poison Ex" + " skip poison due to wrong attack"));
            }
        }

        [AutoRegister]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Monster Melee Poison Ex", 2)
            .AddHandler(DispatcherType.DealingDamage2, OnDamage)
            .Build();
    }
}