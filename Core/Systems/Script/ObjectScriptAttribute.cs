using System;

namespace OpenTemple.Core.Systems.Script;

/// <summary>
/// Marks a class as a script that can be attached to game objects with the given id.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ObjectScriptAttribute : Attribute
{
    public int Id { get; }

    public ObjectScriptAttribute(int id)
    {
        Id = id;
    }
}