using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpicyTemple.Core.Scripting
{
    public interface IDevScripting
    {
        object EvaluateExpression(string command);

        string Complete(string command);

        Task<object> RunScriptAsync(string path);

        void RunStartupScripts();
    }
}