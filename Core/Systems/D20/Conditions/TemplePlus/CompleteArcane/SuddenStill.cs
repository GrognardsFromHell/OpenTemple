using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class SuddenStill
    {
        // Sudden Still:  Complete Arcane, p. 83
        private static void ApplyStill(ref MetaMagicData metaMagicData)
        {
            metaMagicData.IsStill = true;
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Still");

        [AutoRegister, FeatCondition("Sudden Still")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Still Feat", "Sudden Still", ApplyStill)
            .Build();
    }
}