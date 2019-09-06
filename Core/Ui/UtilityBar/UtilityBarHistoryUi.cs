using System.Drawing;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.UtilityBar
{
    public class UtilityBarHistoryUi
    {
        private static readonly WidgetButtonStyle ToggleRollButtonStyle = new WidgetButtonStyle
        {
            normalImagePath = "art/interface/history/Tab_Roll_Unselected.tga",
            hoverImagePath = "art/interface/history/Tab_Roll_Hover.tga",
            pressedImagePath = "art/interface/history/Tab_Roll_Click.tga"
        };

        private static readonly WidgetButtonStyle ToggleDialogButtonStyle = new WidgetButtonStyle
        {
            normalImagePath = "art/interface/history/Dialog_Unselected.tga",
            hoverImagePath = "art/interface/history/Dialog_Hover.tga",
            pressedImagePath = "art/interface/history/Dialog_Click.tga"
        };

        [TempleDllLocation(0x10bdddb0)] [TempleDllLocation(0x10bdde54)]
        private WidgetContainer _container;

        [TempleDllLocation(0x10bdde58)]
        private ScrollBox _rollHistory;

        [TempleDllLocation(0x10bdde5c)]
        private bool _maximized;

        [TempleDllLocation(0x10bdde30)]
        private WidgetButton uiHistoryMinimizeBtn;

        [TempleDllLocation(0x10bdde14)]
        private WidgetButton uiHistoryMaximizeBtn;

        [TempleDllLocation(0x10bdde50)]
        private WidgetButton uiHistoryMinimizeDialogBtn;

        [TempleDllLocation(0x10bdde48)]
        private WidgetButton uiHistoryMaximizeDialogBtn;

        [TempleDllLocation(0x10bdde34)]
        public bool IsVisible => _container.IsVisible();

        [TempleDllLocation(0x101226a0)]
        public UtilityBarHistoryUi()
        {
            // TODO: Right align
            var screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
            _container = new WidgetContainer(new Rectangle(screenSize.Width - 171, screenSize.Height - 292 - 82, 171, 292));
            _container.ZIndex = 99900;
            Globals.UiManager.SortWindows();

            uiHistoryMinimizeBtn = new WidgetButton(new Rectangle(26, 0, 24, 20));
            uiHistoryMinimizeBtn.SetStyle(ToggleRollButtonStyle);
            uiHistoryMinimizeBtn.TooltipText = UiSystems.Tooltip.GetString(6029);
            uiHistoryMinimizeBtn.SetClickHandler(() =>
            {
                // Previously @ 0x10121C20
                if (UiSystems.HelpManager.IsSelectingHelpTarget)
                {
                    UiSystems.HelpManager.ShowPredefinedTopic(18);
                }
                else
                {
                    Minimize();
                }
            });
            _container.Add(uiHistoryMinimizeBtn);

            uiHistoryMaximizeBtn = new WidgetButton(new Rectangle(26, 272, 24, 20));
            uiHistoryMaximizeBtn.SetStyle(ToggleRollButtonStyle);
            uiHistoryMaximizeBtn.TooltipText = UiSystems.Tooltip.GetString(6029);
            uiHistoryMaximizeBtn.SetClickHandler(() =>
            {
                // Previously @ 0x10121bd0
                if (UiSystems.HelpManager.IsSelectingHelpTarget)
                {
                    UiSystems.HelpManager.ShowPredefinedTopic(18);
                }
                else
                {
                    Maximize();
                }
            });
            _container.Add(uiHistoryMaximizeBtn);

            uiHistoryMaximizeDialogBtn = new WidgetButton(new Rectangle(0, 272, 24, 20));
            uiHistoryMaximizeDialogBtn.SetStyle(ToggleDialogButtonStyle);
            uiHistoryMaximizeDialogBtn.SetClickHandler(OnDialogButtonClicked);
            uiHistoryMaximizeDialogBtn.TooltipText = UiSystems.Tooltip.GetString(6028);
            _container.Add(uiHistoryMaximizeDialogBtn);

            uiHistoryMinimizeDialogBtn = new WidgetButton(new Rectangle(0, 0, 24, 20));
            uiHistoryMinimizeDialogBtn.SetStyle(ToggleDialogButtonStyle);
            uiHistoryMinimizeDialogBtn.SetClickHandler(OnDialogButtonClicked);
            uiHistoryMinimizeDialogBtn.TooltipText = UiSystems.Tooltip.GetString(6028);
            _container.Add(uiHistoryMinimizeDialogBtn);

            ScrollBoxSettings settings = new ScrollBoxSettings();
            settings.TextArea = new Rectangle(10, 18, 144, 250);
            settings.ScrollBarPos = new Point(155, 8);
            settings.Font = PredefinedFont.ARIAL_10;
            _rollHistory = new ScrollBox(new Rectangle(-11, 18, 171, 273), settings);
            _rollHistory.BackgroundPath = "art/interface/SCROLLBOX/rollhistory_background.img";
            _container.Add(_rollHistory);

            UpdateWidgetVisibility();
        }

        [TempleDllLocation(0x10121c70)]
        private void OnDialogButtonClicked()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(19);
            }
            else
            {
                UiSystems.Dialog.ToggleHistory();
            }
        }

        private void UpdateWidgetVisibility()
        {
            _rollHistory.SetVisible(_maximized);
            uiHistoryMinimizeBtn.SetVisible(_maximized);
            uiHistoryMaximizeBtn.SetVisible(!_maximized);
            uiHistoryMinimizeDialogBtn.SetVisible(_maximized);
            uiHistoryMaximizeDialogBtn.SetVisible(!_maximized);
        }

        [TempleDllLocation(0x10121c08)]
        [TempleDllLocation(0x10121a00)]
        private void Maximize()
        {
            _maximized = true;
            UpdateWidgetVisibility();
        }

        [TempleDllLocation(0x10121c58)]
        [TempleDllLocation(0x10121a60)]
        private void Minimize()
        {
            _maximized = false;
            UpdateWidgetVisibility();
        }

        [TempleDllLocation(0x101221c0)]
        public void Show()
        {
            _container.SetVisible(true);
            if (_maximized)
            {
                Maximize();
            }
            else
            {
                Minimize();
            }
        }
    }
}