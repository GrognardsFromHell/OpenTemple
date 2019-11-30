using SpicyTemple.Core.IO.SaveGames.UiState;

namespace SpicyTemple.Core.Ui
{
    public interface ISaveGameAwareUi
    {
        void SaveGame(SavedUiState savedState);
        void LoadGame(SavedUiState savedState);
    }
}