using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    // Sudden Extend:  Complete Arcane, p. 83
    public static class SuddenExtend
    {

        private static void ApplyExtend(ref MetaMagicData metaMagicData)
        {
            // Don't Extend more than once
            if (metaMagicData.metaMagicExtendSpellCount < 1)
            {
                metaMagicData.metaMagicExtendSpellCount = 1;
            }
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Extend");

        [AutoRegister, FeatCondition("Sudden Extend")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Extend Feat", "Sudden Extend", ApplyExtend)
            .Build();

    }
}
