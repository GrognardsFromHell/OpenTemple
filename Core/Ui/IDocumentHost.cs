using System;
using System.Threading.Tasks;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui
{
    public interface IDocumentHost
    {
        Task<T> Defer<T>(Func<T> task);

        Task Defer(Action task);

        IClipboard Clipboard { get; }

        void NotifyVisualTreeChange(Node node = null);
    }
}