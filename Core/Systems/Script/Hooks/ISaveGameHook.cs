using SpicyTemple.Core.IO.SaveGames;
using SpicyTemple.Core.IO.SaveGames.GameState;

namespace SpicyTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface ISaveGameHook
    {
        void OnAfterSave(string saveDirectory, SaveGameFile saveFile);

        void OnAfterLoad(string saveDirectory, SaveGameFile saveFile);
    }
}