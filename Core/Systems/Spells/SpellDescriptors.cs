using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Spells;

public class SpellDescriptors : IEnumerable<SpellEntry>
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static readonly Regex FilenamePattern = new Regex(@"^(\d+).*");

    [TempleDllLocation(0x10AAF428)]
    private readonly Dictionary<int, SpellEntry> _spells = new Dictionary<int, SpellEntry>();

    [TempleDllLocation(0x1007B5B0)]
    public SpellDescriptors()
    {
        foreach (var path in Tig.FS.Search("rules/spells/*.txt"))
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            var match = FilenamePattern.Match(filename);
            if (!match.Success)
            {
                continue;
            }

            var spellEnum = int.Parse(match.Groups[1].Value);
            if (_spells.ContainsKey(spellEnum))
            {
                Logger.Warn("Spell ID {0} is duplicated: {1}", spellEnum, path);
                continue;
            }

            _spells[spellEnum] = SpellFileParser.Parse(spellEnum, path);
        }
    }

    public bool TryGetEntry(int spellEnum, out SpellEntry spellEntry) =>
        _spells.TryGetValue(spellEnum, out spellEntry);

    public bool ContainsKey(int spellEnum) => _spells.ContainsKey(spellEnum);

    public IEnumerator<SpellEntry> GetEnumerator() => _spells.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _spells.Values.GetEnumerator();
}