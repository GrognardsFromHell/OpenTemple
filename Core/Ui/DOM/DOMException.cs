using System;

namespace OpenTemple.Core.Ui.DOM
{
    public class DOMException : Exception
    {
        public string Name { get; }

        public DOMException(string message = "", string name = "Error") : base(message)
        {
            Name = name;
        }
    }
}