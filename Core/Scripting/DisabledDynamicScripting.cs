using System.Threading.Tasks;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Scripting
{
    public class DisabledDynamicScripting : IDynamicScripting
    {
        public object EvaluateExpression(string command)
        {
            return "[error] Scripting is disabled, is the DynamicScripting assembly not available?";
        }

        public string Complete(string command)
        {
            return command;
        }

        public Task<object> RunScriptAsync(string path)
        {
            Tig.Console.Append("[error] Scripting is disabled, is the DynamicScripting assembly not available?");
            return Task.FromResult<object>(null);
        }

        public void RunStartupScripts()
        {
        }
    }
}