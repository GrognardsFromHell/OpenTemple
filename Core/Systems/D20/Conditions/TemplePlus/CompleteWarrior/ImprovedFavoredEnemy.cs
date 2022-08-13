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

public class ImprovedFavoredEnemy
{
    private static readonly int bon_val = 3;

    public static void impFavoredEnemyDamageBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        var target = dispIo.attackPacket.victim;
        if (FavoredEnemies.GetFavoredEnemyBonusAgainst(evt.objHndCaller, target, out _, out _))
        {
            dispIo.damage.bonuses.AddBonusFromFeat(bon_val, 0, 114, (FeatId) ElfHash.Hash("Improved Favored Enemy"));
        }
    }

    // args are just-in-case placeholders
    [FeatCondition("Improved Favored Enemy")]
    [AutoRegister]
    public static readonly ConditionSpec impFavoredEnemy = ConditionSpec.Create("Improved Favored Enemy", 2, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.DealingDamage, impFavoredEnemyDamageBonus)
        );
}