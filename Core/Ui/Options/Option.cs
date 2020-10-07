using System;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Options
{
    public abstract class Option
    {
        public string Label { get; }

        protected Option(string label)
        {
            Label = label;
        }
    }
}