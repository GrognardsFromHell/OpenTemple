using System;
using JetBrains.Annotations;

namespace OpenTemple.Core.Utils;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class VisibleForScriptingAttribute : Attribute
{
}