using OpenTemple.Core.IO.SaveGames.UiState;

namespace OpenTemple.Core.Ui;

public interface ISaveGameAwareUi
{
    void SaveGame(SavedUiState savedState);
    void LoadGame(SavedUiState savedState);
}