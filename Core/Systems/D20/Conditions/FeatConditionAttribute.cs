using System;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FeatConditionAttribute : Attribute
    {
        public FeatId FeatId { get; }

        public FeatConditionAttribute(string featName)
        {
            FeatId = (FeatId) ElfHash.Hash(featName);
        }

        public FeatConditionAttribute(FeatId featId)
        {
            FeatId = featId;
        }
    }
}