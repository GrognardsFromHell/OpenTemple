using System;

namespace OpenTemple.Core.Ui.DOM
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DomKeyNameAttribute : Attribute
    {
        public readonly string Name;

        public DomKeyNameAttribute(string name)
        {
            Name = name;
        }
    }
}