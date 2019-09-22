using System.Collections.Generic;
using IronPython.Compiler.Ast;

namespace ScriptConversion
{
    /// <summary>
    /// Collects all defined functions from a Python node (and all descendants).
    /// </summary>
    public class DefinedFunctionWalker : PythonWalker
    {
        public ISet<string> DefinedFunctions { get; } = new HashSet<string>();

        public override bool Walk(FunctionDefinition node)
        {
            DefinedFunctions.Add(node.Name);
            // We are only interested in top-level functions
            return false;
        }
    }
}