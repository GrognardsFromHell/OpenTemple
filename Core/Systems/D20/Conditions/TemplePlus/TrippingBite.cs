using OpenTemple.Core.GFX;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    public class TrippingBite
    {
        public static void OnDamage2(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var target = dispIo.attackPacket.victim;
            if ((target != null))
            {
                if (!target.D20Query(D20DispatcherKey.QUE_Prone) && GameSystems.Combat.TripCheck(evt.objHndCaller, target))
                {
                    GameSystems.Anim.PushDying(target, new EncodedAnimId(NormalAnimType.Death));
                    target.AddCondition("Prone");
                    target.FloatMesFileLine("mes/combat.mes", 104, TextFloaterColor.Red); // Tripped!
                }
            }
        }

        [AutoRegister] public static readonly ConditionSpec tripBite = ConditionSpec.Create("Tripping Bite", 0)
            .AddHandler(DispatcherType.DealingDamage2, OnDamage2)
            .Build();
    }
}
