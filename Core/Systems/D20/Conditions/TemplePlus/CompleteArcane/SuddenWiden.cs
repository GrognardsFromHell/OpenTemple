using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class SuddenWiden
    {

        public static readonly FeatId Id = (FeatId) ElfHash.Hash("Sudden Widen");

        // Sudden Widen:  Complete Arcane, p. 83
        private static void ApplyWiden(ref MetaMagicData metaMagicData)
        {
            // Don't widen more than once
            if (metaMagicData.metaMagicWidenSpellCount < 1)
            {
                metaMagicData.metaMagicWidenSpellCount = 1;
            }
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Widen");

        [AutoRegister, FeatCondition("Sudden Widen")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Widen Feat", "Sudden Widen", ApplyWiden)
            .Build();
    }
}