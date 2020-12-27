using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Party
{
    public class BuffDebuffButton : WidgetButtonBase
    {
        private readonly WidgetImage _image;

        private readonly WidgetLegacyText _tooltipLabel;

        private string _tooltip;

        public BuffDebuffType Type { get; }

        public int Index { get; }

        public string Tooltip
        {
            get => _tooltip;
            set
            {
                _tooltip = value;
                _tooltipLabel.Text = value;
            }
        }

        public string HelpTopic { get; set; }

        public string IconPath
        {
            set => _image.SetTexture(value);
        }

        public BuffDebuffButton(Rectangle rect, BuffDebuffType type, int index) : base(rect)
        {
            _image = new WidgetImage(null);
            _image.SourceRect = new Rectangle(Point.Empty, rect.Size);
            AddContent(_image);
            Type = type;
            Index = index;
            SetClickHandler(ShowHelpTopic);

            _tooltipLabel = new WidgetLegacyText(
                "",
                PredefinedFont.ARIAL_10,
                new TigTextStyle
                {
                    bgColor = new ColorRect(new PackedLinearColorA(17, 17, 17, 204)),
                    textColor = new ColorRect(new PackedLinearColorA(153, 255, 153, 255)),
                    shadowColor = new ColorRect(PackedLinearColorA.Black),
                    kerning = 2,
                    tracking = 5,
                    flags = TigTextStyleFlag.TTSF_DROP_SHADOW
                            | TigTextStyleFlag.TTSF_BACKGROUND
                            | TigTextStyleFlag.TTSF_BORDER
                }
            );
        }

        [TempleDllLocation(0x101323d0)]
        private void ShowHelpTopic()
        {
            if (HelpTopic != null)
            {
                GameSystems.Help.ShowTopic(HelpTopic);
            }
        }

        [TempleDllLocation(0x10131ea0)]
        public override void RenderTooltip(float x, float y)
        {
            if (ButtonState == LgcyButtonState.Disabled || Tooltip == null)
            {
                return;
            }

            var preferredSize = _tooltipLabel.GetPreferredSize();
            var contentArea = new RectangleF(x + 10, y - 20, preferredSize.Width, preferredSize.Height);
            _tooltipLabel.ContentArea = contentArea;
            _tooltipLabel.Render();
        }
    }
}