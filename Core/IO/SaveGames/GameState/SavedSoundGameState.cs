using System.IO;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedSoundGameState
{

    public (int, int) CurrentSchemeIds { get; set; }

    /// <summary>
    /// This is triggered by the /OVER flag for sound schemes and uses the two stashed sound scheme ids below.
    /// </summary>
    public bool IsOverlayScheme { get; set; }

    // Supressed by the overlay scheme
    public (int, int) SchemesSuppressedByOverlay { get; set; }

    public bool IsCombatMusicPlaying { get; set; }

    public (int, int) SchemesSuppressedByCombatMusic { get; set; }

    [TempleDllLocation(0x1003cb70)]
    public static SavedSoundGameState Read(BinaryReader reader)
    {
        var result = new SavedSoundGameState();

        // Currently active scheme ids
        result.CurrentSchemeIds = (reader.ReadInt32(), reader.ReadInt32());

        result.IsOverlayScheme = reader.ReadInt32() != 0;
        result.SchemesSuppressedByOverlay = (reader.ReadInt32(), reader.ReadInt32());

        result.IsCombatMusicPlaying = reader.ReadInt32() != 0;
        result.SchemesSuppressedByCombatMusic = (reader.ReadInt32(), reader.ReadInt32());

        return result;
    }

    [TempleDllLocation(0x1003bbd0)]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32(CurrentSchemeIds.Item1);
        writer.WriteInt32(CurrentSchemeIds.Item2);

        writer.WriteInt32(IsOverlayScheme ? 1 : 0);
        writer.WriteInt32(SchemesSuppressedByOverlay.Item1);
        writer.WriteInt32(SchemesSuppressedByOverlay.Item2);

        writer.WriteInt32(IsCombatMusicPlaying ? 1 : 0);
        writer.WriteInt32(SchemesSuppressedByCombatMusic.Item1);
        writer.WriteInt32(SchemesSuppressedByCombatMusic.Item2);
    }
}