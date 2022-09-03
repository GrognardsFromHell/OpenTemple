using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.Hotkeys;

public class HotkeySystem : IResetAwareSystem, ISaveGameAwareGameSystem
{
    public void Reset()
    {
        throw new System.NotImplementedException();
    }

    public void SaveGame(SavedGameState savedGameState)
    {
        throw new System.NotImplementedException();
    }

    public void LoadGame(SavedGameState savedGameState)
    {
        throw new System.NotImplementedException();
    }

    public bool IsHeld(Hotkey hotkey)
    {
        return false; // TODO
    }
}
