using System;
using System.Diagnostics;

namespace SpicyTemple.Core
{
    /// <summary>
    /// Describes a location in temple.dll where the original function can be found.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TempleDllLocationAttribute : Attribute
    {
        public uint Location { get; }

        public TempleDllLocationAttribute(uint location)
        {
            Location = location;
        }
    }
}