using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class TellingBlow
    {
        // Used by temple+.  Returns 1 if sneak attack damage should be done on a critial.
        // Always returns true when the character has the telling blow feat.
        public static void SneakAttackOnCritical(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1; // Turn On For Telling Blow
        }

        // args are just-in-case placeholders
        [FeatCondition("Telling Blow")]
        [AutoRegister] public static readonly ConditionSpec tellingBlow = ConditionSpec.Create("Telling Blow", 2)
            .SetUnique()
            .AddQueryHandler("Sneak Attack Critical", SneakAttackOnCritical)
            .Build();
    }
}