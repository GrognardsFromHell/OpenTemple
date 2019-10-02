using System;

namespace SpicyTemple.Core.Systems.Script
{
    /// <summary>
    /// Marks a class as a script that contains extra methods for dialogs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DialogScriptAttribute : Attribute
    {
        public int Id { get; }

        public DialogScriptAttribute(int id)
        {
            Id = id;
        }
    }
}