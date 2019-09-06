using System.Drawing;

namespace SpicyTemple.Core.Ui.UtilityBar
{
    public class UtilityBarUi
    {
        [TempleDllLocation(0x10bd33f8)]
        private bool uiUtilityBarVisible;

        private UtilityBarHistoryUi _historyUi = new UtilityBarHistoryUi();

        public UtilityBarUi()
        {
        }

        public void Hide()
        {
            // TODO throw new System.NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x101156b0)]
        public void HideOpenedWindows(bool b)
        {
            // TODO  throw new System.NotImplementedException();
        }

        public bool IsVisible() => uiUtilityBarVisible;

        [TempleDllLocation(0x1010ee80)]
        [TemplePlusLocation("ui_utility_bar.cpp:12")]
        public void Show()
        {
            // TODO WidgetSetHidden/*0x101f9100*/(uiUtilityBarWndId/*0x10bd2ee8*/, 0);
            // TODO j_WidgetCopy/*0x101f87a0*/(uiUtilityBarWndId/*0x10bd2ee8*/, (LgcyWidget *)&uiUtilityBarWnd/*0x10bd3120*/);
            // TODO WidgetBringToFront/*0x101f8e40*/(uiUtilityBarWndId/*0x10bd2ee8*/);
            _historyUi.Show();
            uiUtilityBarVisible = true;
        }

    }
}