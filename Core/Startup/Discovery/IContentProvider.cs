using System.Collections.Generic;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Startup.Discovery
{
    public interface IContentProvider
    {
        IEnumerable<ConditionSpec> Conditions { get; }

        IEnumerable<(FeatId, ConditionSpec)> FeatConditions { get; }

        IEnumerable<D20ClassSpec> Classes { get; }

        IEnumerable<RaceSpec> Races { get; }
    }
}