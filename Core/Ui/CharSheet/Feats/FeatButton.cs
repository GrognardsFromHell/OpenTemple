using System.Drawing;
using System.Text;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Feats
{
    public class FeatButton : WidgetButtonBase
    {
        private FeatId _featId;

        private readonly WidgetText _label;

        public FeatId Feat
        {
            get => _featId;
            set
            {
                if (_featId == value)
                {
                    return;
                }

                _label.Text = GameSystems.Feat.GetFeatName(value);
                _featId = value;
            }
        }

        public FeatButton(Rectangle rect) : base(rect)
        {
            _label = new WidgetText();
            AddContent(_label);
            SetClickHandler(ShowFeatHelp);
            OnMouseEnter += ShowShortFeatDescription;
            OnMouseExit += HideShortFeatDescription;

            AddStyle("char-ui-feat-button");
        }

        [TempleDllLocation(0x101bc840)]
        private void ShowFeatHelp()
        {
            if (GameSystems.Feat.TryGetFeatHelpTopic(_featId, out var helpTopic))
            {
                GameSystems.Help.ShowTopic(helpTopic);
            }
        }

        private void ShowShortFeatDescription(MessageWidgetArgs obj)
        {
            var helpText = new StringBuilder();
            if (GameSystems.Feat.TryGetFeatDescription(_featId, out var description))
            {
                helpText.Append(description);
                helpText.Append("\n\n");
            }

            helpText.Append(GameSystems.Feat.GetFeatPrerequisites(_featId));
            UiSystems.CharSheet.Help.SetHelpText(helpText.ToString());
        }

        private void HideShortFeatDescription(MessageWidgetArgs obj)
        {
            UiSystems.CharSheet.Help.ClearHelpText();
        }

        public override void Render()
        {
            var hover = ButtonState == LgcyButtonState.Hovered || ButtonState == LgcyButtonState.Down;
            ToggleStyle("char-ui-feat-button-hover", hover);

            base.Render();
        }
    }
}