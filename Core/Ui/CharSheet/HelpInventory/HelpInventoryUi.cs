using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.CharSheet.HelpInventory
{
    public class CharSheetHelpUi
    {
        private WidgetScrollView _scrollView;

        private WidgetContainer _textContainer;

        private WidgetLegacyText _textLabel;

        public CharSheetHelpUi()
        {
            var widgetDoc = WidgetDoc.Load("ui/char_help.json");
            _scrollView = (WidgetScrollView) widgetDoc.TakeRootWidget();

            _textContainer = new WidgetContainer(0, 0,
                _scrollView.GetInnerWidth(), _scrollView.GetInnerHeight());
            var style = new TigTextStyle
            {
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
                textColor = new ColorRect(PackedLinearColorA.White),
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                kerning = 2,
                tracking = 2
            };
            _textLabel = new WidgetLegacyText("", PredefinedFont.ARIAL_10, style);
            _textContainer.AddContent(_textLabel);
            _scrollView.Add(_textContainer);
        }

        [TempleDllLocation(0x10BF0BC0)]
        public bool Shown { get; set; }

        public WidgetBase Container => _scrollView;

        public void Hide()
        {
            Stub.TODO();
            Shown = false;
        }

        [TempleDllLocation(0x101627a0)]
        public void Show()
        {
            Stub.TODO();
            Shown = true;
        }

        [TempleDllLocation(0x10162c00)]
        public void SetHelpText(string text)
        {
            _textLabel.Text = text;
        }

        [TempleDllLocation(0x101628D0)]
        public string GetObjectHelp(GameObjectBody obj, GameObjectBody observer)
        {
            return UiSystems.Tooltip.GetObjectDescription(obj, observer);
        }

        public void ShowItemDescription(GameObjectBody item, GameObjectBody observer)
        {
            var text = UiSystems.CharSheet.Help.GetObjectHelp(item, observer);
            SetHelpText(text);
        }

        public void ClearHelpText() => SetHelpText("");

        [TempleDllLocation(0x10162730)]
        public void Reset()
        {
            ClearHelpText();
        }
    }
}