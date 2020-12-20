using System.Threading.Tasks;

namespace OpenTemple.Core.Scripting
{
    public interface IDynamicScripting
    {
        object EvaluateExpression(string command);

        string Complete(string command);

        Task<object> RunScriptAsync(string path);

        void RunStartupScripts();
    }
}