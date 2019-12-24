using System;
using JetBrains.Annotations;

namespace OpenTemple.Core.Startup.Discovery
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public class AutoRegisterAttribute : Attribute
    {
    }
}