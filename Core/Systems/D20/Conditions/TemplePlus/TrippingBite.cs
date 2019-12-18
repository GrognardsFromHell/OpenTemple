
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
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
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
