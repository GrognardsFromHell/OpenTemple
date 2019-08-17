using SpicyTemple.Core.Systems.Help;

namespace SpicyTemple.Core.Ui
{
    public class HelpUi
    {
        [TempleDllLocation(0x10be2e84)]
        private bool uiHelpIsVisible;

        [TempleDllLocation(0x10130300)]
        public bool IsVisible => uiHelpIsVisible;

        [TempleDllLocation(0x10130670)]
        public void Show(HelpRequest a, int b)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10130640)]
        public void Hide()
        {

        }

    }
}