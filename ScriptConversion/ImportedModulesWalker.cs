using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Compiler.Ast;

namespace ScriptConversion;

public class ImportedModulesWalker : PythonWalker
{
    public ISet<string> ImportedModules { get; } = new HashSet<string>();

    public override void PostWalk(FromImportStatement node)
    {
        Trace.Assert(!node.IsFromFuture);
        ImportedModules.Add(string.Join('.', node.Root.Names));
    }

    public override void PostWalk(ImportStatement node)
    {
        foreach (var dottedName in node.Names)
        {
            ImportedModules.Add(string.Join('.', dottedName.Names));
        }
    }

    public override bool Walk(FunctionDefinition node)
    {
        // Do not collect imports within functions
        return false;
    }
}