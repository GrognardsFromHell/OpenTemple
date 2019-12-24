using System;
using System.Diagnostics;

namespace OpenTemple.Core
{
    /// <summary>
    /// Describes a location in temple.dll where the original function can be found.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TempleDllLocationAttribute : Attribute
    {
        public uint Location { get; }

        public bool Secondary { get; }

        public TempleDllLocationAttribute(uint location, bool secondary = false)
        {
            Location = location;
            Secondary = secondary;
        }
    }
}