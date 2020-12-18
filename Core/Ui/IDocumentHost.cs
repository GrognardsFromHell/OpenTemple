using System;
using System.Threading.Tasks;

namespace OpenTemple.Core.Ui
{
    public interface IDocumentHost
    {
        Task<T> Defer<T>(Func<T> task);

        Task Defer(Action task);
    }
}