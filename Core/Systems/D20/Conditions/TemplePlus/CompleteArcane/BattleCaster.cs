using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class BattleCaster
    {
        // Query is to be made from any class that allows a caster to wear some armor without arcane failure

        public static void ImprovedArcaneFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1; // Return 1 to improve the class's arcane failure resistance for armor
        }

        // args are just-in-case placeholders
        [AutoRegister, FeatCondition("Battle Caster")]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Battle Caster", 2)
            .SetUnique()
            .AddQueryHandler("Improved Armored Casting", ImprovedArcaneFailure)
            .Build();
    }
}