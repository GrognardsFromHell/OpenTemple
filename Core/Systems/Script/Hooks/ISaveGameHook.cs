using OpenTemple.Core.IO.SaveGames;

namespace OpenTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface ISaveGameHook
    {
        void OnAfterSave(string saveDirectory, SaveGameFile saveFile);

        void OnAfterLoad(string saveDirectory, SaveGameFile saveFile);
    }
}