using SpicyTemple.Core.IO.SaveGames.UiState;

namespace SpicyTemple.Core.Ui.Logbook
{
    public class LogbookEgoUi
    {

        [TempleDllLocation(0x10c4d100)]
        public bool IsVisible { get; set; }

        public void Show()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10199230)]
        public void Hide()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10199040)]
        public void Reset()
        {
          Stub.TODO();
        }

        [TempleDllLocation(0x10199120)]
        public SavedLogbookEgoUiState Save()
        {
            Stub.TODO();
            return new SavedLogbookEgoUiState();
        }

        [TempleDllLocation(0x10199170)]
        public void Load(SavedLogbookEgoUiState savedState)
        {
            Stub.TODO();
        }
    }
}