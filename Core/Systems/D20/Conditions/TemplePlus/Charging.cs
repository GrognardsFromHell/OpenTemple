using OpenTemple.Core.Startup.Discovery;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

public class Charging
{
    public static void ChargingQuery(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        // Always return true since the condition is set
        dispIo.return_val = 1;
    }

    [AutoRegister]
    public static readonly ConditionSpec ChargingExtension = StatusEffects.Charging.Extend(builder => builder
        .AddQueryHandler("Charging", ChargingQuery)
    );
}