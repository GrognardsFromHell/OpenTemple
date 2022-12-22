using System.Collections.Generic;

namespace OpenTemple.Core.Systems.Sound;

public class SoundScheme
{
    public string SchemeName;
    public int SchemelistKey;
    public int SchemeId;
    public List<SoundSchemeElement> Lines = new();
    public string? CombatIntro;
    public string? CombatLoop;

    [TempleDllLocation(0x1003bad0)]
    public void Reset()
    {
        SchemeName = null;
        SchemelistKey = 0;
        CombatIntro = null;
        CombatLoop = null;
        Lines.Clear();
    }

    public void GetCombatMusicFiles(out string combatIntro, out string combatLoop)
    {
        if (CombatIntro != null)
        {
            combatIntro = $"sound/{CombatIntro}";
        }
        else
        {
            combatIntro = "sound/music/combatintro.mp3";
        }

        if (CombatLoop != null)
        {
            combatLoop = $"sound/{CombatLoop}";
        }
        else
        {
            combatLoop = "sound/music/combatmusic.mp3";
        }
    }
}