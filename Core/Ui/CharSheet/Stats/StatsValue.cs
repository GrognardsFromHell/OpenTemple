using System;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Stats
{
    public class StatsValue : WidgetButtonBase
    {
        private readonly WidgetImage _downImage;
        private readonly WidgetImage _hoverImage;

        private readonly WidgetLegacyText _label;

        private readonly WidgetLegacyText _tooltipLabel;

        private readonly Func<GameObjectBody, string> _valueSupplier;

        public StatsValue(
            Func<GameObjectBody, string> valueSupplier,
            Rectangle rect,
            StatsUiTexture downImage,
            StatsUiTexture hoverImage,
            StatsUiParams uiParams) : base(rect)
        {
            _downImage = new WidgetImage(uiParams.TexturePaths[downImage]);
            _hoverImage = new WidgetImage(uiParams.TexturePaths[hoverImage]);
            _valueSupplier = valueSupplier;

            _label = new WidgetLegacyText(
                "",
                uiParams.MoneyFont,
                new TigTextStyle
                {
                    kerning = 1,
                    tracking = 5,
                    textColor = new ColorRect(PackedLinearColorA.White),
                    additionalTextColors = new []
                    {
                        new ColorRect(new PackedLinearColorA(32, 255, 32, 255)),
                        new ColorRect(new PackedLinearColorA(255, 32, 32, 255)),
                        new ColorRect(new PackedLinearColorA(255, 51, 51, 255)),
                    }
                }
            );
            SetWidgetMsgHandler(msg =>
            {
                if (msg.widgetEventType == TigMsgWidgetEvent.Exited)
                {
                    UiSystems.CharSheet.Help.ClearHelpText();
                    return true;
                }

                return false;
            });

            var tooltipStyle = new TigTextStyle();
            tooltipStyle.bgColor = new ColorRect(new PackedLinearColorA(17, 17, 17, 204));
            tooltipStyle.shadowColor = new ColorRect(PackedLinearColorA.Black);
            tooltipStyle.textColor = new ColorRect(PackedLinearColorA.White);
            tooltipStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW
                                 | TigTextStyleFlag.TTSF_BACKGROUND
                                 | TigTextStyleFlag.TTSF_BORDER;
            tooltipStyle.tracking = 2;
            tooltipStyle.kerning = 2;

            _tooltipLabel = new WidgetLegacyText("", PredefinedFont.ARIAL_10, tooltipStyle);
        }

        public string Tooltip { get; set; } = UiSystems.Tooltip.GetString(6044);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _downImage.Dispose();
                _hoverImage.Dispose();
            }
        }

        public override void Render()
        {
            WidgetImage renderImage = null;
            if (ButtonState == LgcyButtonState.Hovered)
            {
                renderImage = _hoverImage;
            }
            else if (ButtonState == LgcyButtonState.Down)
            {
                renderImage = _downImage;
            }

            var contentArea = GetContentArea();

            if (renderImage != null)
            {
                renderImage.ContentArea = new RectangleF(
                    contentArea.X,
                    contentArea.Y - 1,
                    renderImage.GetPreferredSize().Width,
                    renderImage.GetPreferredSize().Height
                );
                renderImage.Render();
            }

            var critter = UiSystems.CharSheet.CurrentCritter;
            _label.Text = critter != null ? _valueSupplier(critter) : "";

            var labelSize = _label.GetPreferredSize();
            // Center horizontally and vertically within the content area
            var labelArea = new RectangleF(
                contentArea.X + (contentArea.Width - labelSize.Width) / 2,
                contentArea.Y + (contentArea.Height - labelSize.Height) / 2,
                contentArea.Width = labelSize.Width,
                contentArea.Height = labelSize.Height
            );

            _label.ContentArea = labelArea;
            _label.Render();
        }

        public override void RenderTooltip(float x, float y)
        {
            if (Tooltip != null)
            {
                _tooltipLabel.Text = Tooltip;

                var preferredSize = _tooltipLabel.GetPreferredSize();
                var contentArea = new RectangleF(x, y - preferredSize.Height, preferredSize.Width, preferredSize.Height);
                _tooltipLabel.ContentArea = contentArea;
                _tooltipLabel.Render();
            }
        }
    }
}