using SpicyTemple.Core.IO.SaveGames.GameState;

namespace SpicyTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface ISaveGameHook
    {
        void OnAfterSave(string saveDirectory, SavedGameState savedState);

        void OnAfterLoad(string saveDirectory, SavedGameState savedState);
    }
}