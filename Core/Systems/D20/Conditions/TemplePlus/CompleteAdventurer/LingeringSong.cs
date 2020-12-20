using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Lingering Song, Complete Adventurer: p. 111
    public class LingeringSong
    {
        public static void QueryMaxBardicMusicExtraRounds(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // 5 Extra Rounds from this feat
            dispIo.return_val += 5;
        }

        // Extra, Extra
        [FeatCondition("Lingering Song")]
        [AutoRegister]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Lingering Song", 2)
            .SetUnique()
            .AddQueryHandler("Bardic Ability Duration Bonus", QueryMaxBardicMusicExtraRounds)
            .Build();
    }
}
