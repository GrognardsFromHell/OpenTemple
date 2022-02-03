
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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

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