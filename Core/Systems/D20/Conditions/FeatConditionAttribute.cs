using System;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Conditions
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