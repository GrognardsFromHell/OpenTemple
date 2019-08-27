using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Combat
{
    public class InitiativePortraitButton : WidgetButtonBase
    {
        private readonly InitiativeMetrics _metrics;

        private readonly GameObjectBody _combatant;

        private readonly bool _smallMode;

        private readonly WidgetImage _frame;

        private readonly WidgetImage _portrait;

        private readonly WidgetImage _highlight;

        public InitiativePortraitButton(GameObjectBody combatant, bool smallMode)
        {
            _combatant = combatant;
            _smallMode = smallMode;
            _metrics = smallMode ? InitiativeMetrics.Small : InitiativeMetrics.Normal;

            SetPos(_metrics.Button.Location);
            SetSize(_metrics.Button.Size);

            _portrait = new WidgetImage(GetPortraitPath());
            AddContent(_portrait); // This is for automated cleanup

            string frameTexture;
            if (_smallMode)
            {
                frameTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/PortraitFrame_Mini.tga";
            }
            else
            {
                frameTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/PortraitFrame.tga";
            }

            _frame = new WidgetImage(frameTexture);
            AddContent(_frame); // This is for automated cleanup

            string highlightTexture;
            if (_smallMode)
            {
                highlightTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/Highlight_Mini.tga";
            }
            else
            {
                highlightTexture = "art/interface/COMBAT_UI/COMBAT_INITIATIVE_UI/Highlight_Red.tga";
            }

            _highlight = new WidgetImage(highlightTexture);
            AddContent(_highlight); // This is for automated cleanup
        }

        [TempleDllLocation(0x10141810)]
        public override void Render()
        {
            if (!IsVisible())
            {
                return;
            }

            RenderFrame();
            RenderPortrait();

            if (GameSystems.D20.Actions.IsCurrentlyActing(_combatant))
            {
                RenderHighlight();
            }

            // TODO: This should be on mouseover, not on render
            if (!GameSystems.Critter.IsConcealed(_combatant))
            {
                if (ButtonState == LgcyButtonState.Hovered)
                {
                    UiSystems.InGameSelect.Focus = _combatant;
                }
                else if (ButtonState == LgcyButtonState.Down)
                {
                    UiSystems.InGameSelect.AddToFocusGroup(_combatant);
                }
            }
        }

        [TempleDllLocation(0x10141780)]
        private void RenderFrame()
        {
            var contentArea = GetContentArea();
            // This was previously drawn in the context of the parent container
            contentArea.Offset(- GetX(), - GetY());
            contentArea.Offset(1, 1);
            contentArea.Size = _metrics.FrameSize;

            _frame.SetContentArea(contentArea);
            _frame.Render();
        }

        private void RenderPortrait()
        {
            var contentArea = GetContentArea();

            var portraitRect = contentArea;
            portraitRect.Offset(_metrics.PortraitOffset);

            _portrait.SetContentArea(portraitRect);
            _portrait.Render();
        }

        private void RenderHighlight()
        {
            var contentArea = GetContentArea();
            var highlightRect = _metrics.HighlightFrame;
            highlightRect.Offset(contentArea.Location);

            _highlight.SetContentArea(highlightRect);
            _highlight.Render();
        }

        private string GetPortraitPath()
        {
            var isDead = GameSystems.Critter.IsDeadOrUnconscious(_combatant);
            var portraitId = _combatant.GetInt32(obj_f.critter_portrait);

            PortraitVariant variant;
            if (_smallMode)
            {
                variant = isDead ? PortraitVariant.MediumGrey : PortraitVariant.Medium;
            }

            else
            {
                variant = isDead ? PortraitVariant.SmallGrey : PortraitVariant.Small;
            }

            return GameSystems.UiArtManager.GetPortraitPath(portraitId, variant);
        }
    }
}