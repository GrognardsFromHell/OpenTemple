using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenTemple.Core.Scripting
{
    public interface IDynamicScripting
    {
        object EvaluateExpression(string command);

        string Complete(string command);

        Task<object> RunScriptAsync(string path);

        void RunStartupScripts();

        void AddAssembly(Assembly assembly);
    }
}