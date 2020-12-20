using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Sudden Enlarge:  Miniatures Handbook, p. 28
    public class SuddenEnlarge
    {

        public static readonly FeatId Id = (FeatId) ElfHash.Hash("Sudden Enlarge");

        public static void ApplyEnlarge(ref MetaMagicData metaMagicData)
        {
            // Don't enlarge more than once
            if (metaMagicData.metaMagicEnlargeSpellCount < 1)
            {
                metaMagicData.metaMagicEnlargeSpellCount = 1;
            }
        }

        // TODO tpdp.register_metamagic_feat("Sudden Enlarge");

        // Charges, Toggeled On, Spare, Spare
        [FeatCondition("Sudden Enlarge")]
        [AutoRegister] public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Enlarge Feat", "Sudden Enlarge", ApplyEnlarge)
            .Build();
    }
}