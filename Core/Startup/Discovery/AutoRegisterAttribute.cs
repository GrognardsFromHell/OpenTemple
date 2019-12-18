using System;
using JetBrains.Annotations;

namespace SpicyTemple.Core.Startup.Discovery
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public class AutoRegisterAttribute : Attribute
    {
    }
}