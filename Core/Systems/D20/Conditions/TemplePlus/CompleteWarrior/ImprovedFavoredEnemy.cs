
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
        [AutoRegister] public static readonly ConditionSpec impFavoredEnemy = ConditionSpec.Create("Improved Favored Enemy", 2)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, impFavoredEnemyDamageBonus)
            .Build();
    }
}
