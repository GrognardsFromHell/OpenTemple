using System.Collections.Generic;

namespace OpenTemple.Core.Systems.Sound;

public class SoundScheme
{
    public string schemeName;
    public int schemelistKey;
    public int schemeId;
    public List<SoundSchemeElement> lines = new();
    public string? combatintro;
    public string? combatloop;

    [TempleDllLocation(0x1003bad0)]
    public void Reset()
    {
        schemeName = null;
        schemelistKey = 0;
        combatintro = null;
        combatloop = null;
        lines.Clear();
    }

    public void GetCombatMusicFiles(out string combatIntro, out string combatLoop)
    {
        if (combatintro != null)
        {
            combatIntro = $"sound/{combatintro}";
        }
        else
        {
            combatIntro = "sound/music/combatintro.mp3";
        }

        if (combatloop != null)
        {
            combatLoop = $"sound/{combatloop}";
        }
        else
        {
            combatLoop = "sound/music/combatmusic.mp3";
        }
    }
}