using System;

namespace SpicyTemple.Core.Systems.Script
{
    /// <summary>
    /// Marks a class as a script that implements the Spell with the given id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SpellScriptAttribute : Attribute
    {
        public int Id { get; }

        public SpellScriptAttribute(int id)
        {
            Id = id;
        }
    }
}