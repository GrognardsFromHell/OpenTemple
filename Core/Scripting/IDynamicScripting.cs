using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpicyTemple.Core.Scripting
{
    public interface IDynamicScripting
    {
        object EvaluateExpression(string command);

        string Complete(string command);

        Task<object> RunScriptAsync(string path);

        void RunStartupScripts();
    }
}