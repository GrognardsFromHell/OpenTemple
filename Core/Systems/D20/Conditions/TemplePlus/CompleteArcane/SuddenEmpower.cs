using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Sudden Empower:  Complete Arcane, p. 83
    public class SuddenEmpower
    {

        public static readonly FeatId Id = (FeatId) ElfHash.Hash("Sudden Empower");

        private static void ApplyEmpower(ref MetaMagicData metaMagicData)
        {
            // Don't Empower more than once
            if (metaMagicData.metaMagicEmpowerSpellCount < 1)
            {
                metaMagicData.metaMagicEmpowerSpellCount = 1;
            }
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Empower");

        [AutoRegister, FeatCondition("Sudden Empower")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Empower Feat", "Sudden Empower", ApplyEmpower)
            .Build();
    }
}