using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Sudden Quicken:  Complete Arcane, p. 83
    public class SuddenQuicken
    {
        private static void ApplyQuicken(ref MetaMagicData metaMagicData)
        {
            metaMagicData.IsQuicken = true;
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Quicken");

        [AutoRegister, FeatCondition("Sudden Quicken")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Quicken Feat", "Sudden Quicken", ApplyQuicken)
            .Build();
    }
}