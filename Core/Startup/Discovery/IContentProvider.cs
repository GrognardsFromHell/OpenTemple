using System.Collections.Generic;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;

namespace SpicyTemple.Core.Startup.Discovery
{
    public interface IContentProvider
    {
        IEnumerable<ConditionSpec> Conditions { get; }

        IEnumerable<(FeatId, ConditionSpec)> FeatConditions { get; }

        IEnumerable<D20ClassSpec> Classes { get; }
    }
}