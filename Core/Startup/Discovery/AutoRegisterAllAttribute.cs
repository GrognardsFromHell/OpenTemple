using System;
using JetBrains.Annotations;

namespace OpenTemple.Core.Startup.Discovery;

/// <summary>
/// Annotate a public static method that returns an IEnumerable of a supported content type to have
/// all returned items be auto-registered. 
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse(ImplicitUseKindFlags.Access)]
public class AutoRegisterAllAttribute : Attribute
{
}