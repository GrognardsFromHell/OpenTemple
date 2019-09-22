using System;

namespace SpicyTemple.Core.Systems.Script
{
    /// <summary>
    /// Marks a class as a script that can be attached to game objects with the given id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ObjectScriptAttribute : Attribute
    {
        public int Id { get; }

        public ObjectScriptAttribute(int id)
        {
            Id = id;
        }
    }
}