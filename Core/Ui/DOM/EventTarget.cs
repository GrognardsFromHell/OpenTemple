using System.Diagnostics.CodeAnalysis;

namespace OpenTemple.Core.Ui.DOM
{
    public interface EventTarget
    {
        void AddEventListener(string type, [AllowNull]
            EventListener callback,
            bool capture = false,
            bool once = false,
            bool passive = false);

        void RemoveEventListener(string type, [AllowNull]
            EventListener callback,
            bool capture = false);

        bool DispatchEvent(IEvent evt);
    }
}