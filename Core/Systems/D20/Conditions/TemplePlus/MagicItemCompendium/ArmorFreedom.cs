using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Magic Item Compendium, p. 11
    public class ArmorFreedom
    {
        // spare, spare, inv_idx
        [AutoRegister]
        public static readonly ConditionSpec armorFreedom = ConditionSpec.Create("Armor Freedom", 3)
            .SetUnique()
            .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, true)
            .Build();
    }
}