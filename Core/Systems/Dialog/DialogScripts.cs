using System;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.Dialog
{
    public class DialogScripts
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private Dictionary<int, DialogScript> _scripts = new Dictionary<int, DialogScript>();

        public bool TryGet(int scriptId, out DialogScript script)
        {
            script = Get(scriptId);
            return script != null;
        }

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

        [TempleDllLocation(0x10036600)]
        private DialogScript Load(int scriptId)
        {
            if (!GameSystems.ScriptName.TryGetDialogScriptPath(scriptId, out var path))
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

            // Link lines together that belong together
            var orderedKeys = lines.Keys.ToArray();
            Array.Sort(orderedKeys);
            var previousLineKey = -1;
            foreach (var lineKey in orderedKeys)
            {
                var line = lines[lineKey];
                if (line.IsPcLine)
                {
                    if (previousLineKey == -1)
                    {
                        Logger.Warn("Found a PC response line {0} without a preceeding NPC line.", line.key);
                    }
                    else
                    {
                        var prevLine = lines[previousLineKey];
                        prevLine.nextResponseKey = line.key;
                        lines[previousLineKey] = prevLine;
                    }
                }

                previousLineKey = line.key;
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
        public int nextResponseKey;

        public bool IsPcLine => minIq != 0;
        public bool IsNpcLine => !IsPcLine;
    }
}