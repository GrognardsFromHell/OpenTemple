using SpicyTemple.Core.IO.SaveGames.UiState;

namespace SpicyTemple.Core.Ui.Logbook
{
    public class LogbookQuestsUi
    {

        [TempleDllLocation(0x10c0c498)]
        public bool IsVisible { get; set; }

        [TempleDllLocation(0x10c0c48c)]
        private int dword_10C0C48C = 0;

        [TempleDllLocation(0x101784e0)]
        public void Show()
        {
            Update();
            Stub.TODO();
        }

        [TempleDllLocation(0x101785f0)]
        public void Hide()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1017b850)]
        public void Update()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10178150)]
        public void Reset()
        {
          dword_10C0C48C = 0;
        }

        [TempleDllLocation(0x10178460)]
        public SavedLogbookQuestsUiState Save()
        {
            Stub.TODO();
            return new SavedLogbookQuestsUiState();
        }

        [TempleDllLocation(0x101784a0)]
        public void Load(SavedLogbookQuestsUiState savedState)
        {
            Stub.TODO();
        }
    }
}