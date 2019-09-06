using System.Collections.Generic;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.Dialog
{
    public class DialogScripts
    {
        private Dictionary<int, DialogScript> _scripts = new Dictionary<int, DialogScript>();

        public DialogScript Get(int scriptId)
        {
            if (_scripts.TryGetValue(scriptId, out var script))
            {
                return script;
            }

            script = Load(scriptId);
            if (script == null)
            {
                return null;
            }

            _scripts[scriptId] = script;
            return script;
        }

        private DialogScript Load(int scriptId)
        {
            var path = GameSystems.ScriptName.GetDialogScriptPath(scriptId);
            if (path == null)
            {
                return null;
            }

            var content = Tig.FS.ReadTextFile(path);

            var parser = new DialogScriptParser(path, content);

            var lines = new Dictionary<int, DialogLine>(500);
            while (parser.GetSingleLine(out var line, out var fileLine))
            {
                lines[line.key] = line;
            }

            return new DialogScript(scriptId, path, lines);
        }
    }

    public class DialogScript
    {
        public int Id { get; }

        public string Path { get; }

        public Dictionary<int, DialogLine> Lines { get; }

        public TimePoint LastTimeUsed { get; private set; }

        public DialogScript(int id, string path, Dictionary<int, DialogLine> lines)
        {
            Id = id;
            Path = path;
            Lines = lines;
        }

        public void MarkUsed()
        {
            LastTimeUsed = TimePoint.Now;
        }
    }

    public struct DialogLine
    {
        public int key;
        public string txt;
        public string genderField; // set to -1 for PC lines
        public int minIq; // 0 for NPC lines
        public string testField; // condition script that determines whether to display the line
        public int answerLineId; // NPC line to display next
        public string effectField; // script line to run

        public bool IsPcLine => minIq != 0;
        public bool IsNpcLine => !IsPcLine;
    }
}