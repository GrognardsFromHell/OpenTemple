
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

    public class Craven
    {
        public static void CADamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Add 1 damage per Hit Die to the sneak attack
            dispIo.return_val += evt.objHndCaller.GetStat(Stat.level);
        }

        public static void Fearful(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            // D20STD_F_SPELL_DESCRIPTOR_FEAR
            if ((dispIo.flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR) != 0)
            {
                dispIo.bonlist.AddBonus(-2, 0, "Craven: You are easily frightened");
            }
        }

        [FeatCondition("Craven")]
        [AutoRegister]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Craven Feat", 2)
            .SetUnique()
            .AddQueryHandler("Sneak Attack Bonus", CADamage)
            // Generalized even though fear effects should only be will saves.
            .AddHandler(DispatcherType.SaveThrowLevel, Fearful)
            .Build();
    }
}
