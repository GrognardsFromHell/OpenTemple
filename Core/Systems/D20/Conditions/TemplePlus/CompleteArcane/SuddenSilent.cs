using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Sudden Silent:  Complete Arcane, p. 83
    public class SuddenSilent
    {
        private static void ApplySilent(ref MetaMagicData metaMagicData)
        {
            metaMagicData.IsSilent = true;
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Silent");

        [AutoRegister, FeatCondition("Sudden Silent")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Silent Feat", "Sudden Silent", ApplySilent)
            .Build();
    }
}