using System.Drawing;
using System.Text;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Feats
{
    public class FeatButton : WidgetButtonBase
    {
        private const PredefinedFont Font = PredefinedFont.ARIAL_10;

        private static readonly TigTextStyle NormalStyle;

        private static readonly TigTextStyle HoverStyle;

        static FeatButton()
        {
            NormalStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
            {
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                kerning = 2,
                tracking = 2
            };
            HoverStyle = NormalStyle.Copy();
            HoverStyle.textColor = new ColorRect(new PackedLinearColorA(0xFF0D6BE3));
        }

        private FeatId _featId;

        private readonly WidgetLegacyText _label;

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
            _label = new WidgetLegacyText("", Font, NormalStyle);
            AddContent(_label);
            SetClickHandler(ShowFeatHelp);
            OnMouseEnter += ShowShortFeatDescription;
            OnMouseExit += HideShortFeatDescription;
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
            if (ButtonState == LgcyButtonState.Hovered || ButtonState == LgcyButtonState.Down)
            {
                _label.TextStyle = HoverStyle;
            }
            else
            {
                _label.TextStyle = NormalStyle;
            }

            base.Render();
        }
    }
}