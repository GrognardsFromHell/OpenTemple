using System;

namespace OpenTemple.Core;

/// <summary>
/// Describes a location in TemplePlus where the rewritten function can be found.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class TemplePlusLocationAttribute : Attribute
{
    public string Location { get; }

    public TemplePlusLocationAttribute(string location)
    {
        Location = location;
    }
}