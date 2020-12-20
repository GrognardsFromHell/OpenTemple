using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
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
