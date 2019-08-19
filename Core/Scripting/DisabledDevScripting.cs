using System.Threading.Tasks;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Scripting
{
    public class DisabledDevScripting : IDevScripting
    {
        public object EvaluateExpression(string command)
        {
            return "[error] Scripting is disabled, is the DevScripting assembly not available?";
        }

        public string Complete(string command)
        {
            return command;
        }

        public Task<object> RunScriptAsync(string path)
        {
            Tig.Console.Append("[error] Scripting is disabled, is the DevScripting assembly not available?");
            return Task.FromResult<object>(null);
        }

        public void RunStartupScripts()
        {
        }
    }
}