using System;

namespace OpenTemple.Core.Systems.Script;

/// <summary>
/// Marks a class as a script that contains extra methods for dialogs.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DialogScriptAttribute : Attribute
{
    public int Id { get; }

    public DialogScriptAttribute(int id)
    {
        Id = id;
    }
}