using OpenTemple.Core.Logging;
using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class MonsterMeleePoisonEx
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

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