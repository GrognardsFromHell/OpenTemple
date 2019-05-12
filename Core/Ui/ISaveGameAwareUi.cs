namespace SpicyTemple.Core.Ui
{
    public interface ISaveGameAwareUi
    {
        bool SaveGame();
        bool LoadGame();
    }
}