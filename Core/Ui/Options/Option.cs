using OpenTemple.Core.Ui.Widgets;
using ReactiveUI;

namespace OpenTemple.Core.Ui.Options
{
    public abstract class Option : ReactiveObject
    {
        public string Label { get; }

        protected Option(string label)
        {
            Label = label;
        }

        public virtual void Reset()
        {
        }

        public virtual void Apply()
        {
        }

        public virtual void Cancel()
        {
        }
    }
}