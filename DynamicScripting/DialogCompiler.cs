using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.DynamicScripting;

// Compiles the scripts contained in a .dlg file.
public class DialogCompiler
{
    public void CompileFile(string path)
    {
        var dlgContent = Tig.FS.ReadTextFile(path);

        var parser = new DialogScriptParser(path, dlgContent);
        while (parser.GetSingleLine(out var dialogLine, out var lineNumber))
        {
        }
    }
}