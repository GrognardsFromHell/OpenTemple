using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.CharSheet;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Party
{
    internal class PortraitButton : WidgetButtonBase
    {
        private static readonly TigTextStyle HpTextStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_CENTER | TigTextStyleFlag.TTSF_DROP_SHADOW,
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            bgColor = new ColorRect(new PackedLinearColorA(17, 17, 17, 153)),
            kerning = 2,
            tracking = 5,
            additionalTextColors = new[]
            {
                // Used for subdual damage
                new ColorRect(new PackedLinearColorA(0xFF6666FF)),
            }
        };

        private readonly GameObjectBody _obj;

        private int _currentPortraitId;

        private WidgetImage _normalPortrait;
        private WidgetImage _greyPortrait;
        private WidgetLegacyText _hpLabel;
        private WidgetImage _highlight;
        private WidgetImage _highlightHover;
        private WidgetImage _highlightPressed;

        public PortraitButton(GameObjectBody obj)
        {
            _obj = obj;

            _highlight = new WidgetImage("art/interface/party_ui/Highlight.tga");
            _highlightHover = new WidgetImage("art/interface/party_ui/Highlight-hover-on.tga");
            _highlightPressed = new WidgetImage("art/interface/party_ui/Highlight-pressed.tga");
        }

        [TempleDllLocation(0x10BE33E8)]
        private bool dword_10BE33E8;

        [TempleDllLocation(0x10BE3414)]
        private bool dword_10BE3414;

        [TempleDllLocation(0x101331e0)]
        public override bool HandleMessage(Message msg)
        {
            if (msg.type == MessageType.WIDGET)
            {
                // TODO: Remainder of this function (lots of stuff)
            }

            return base.HandleMessage(msg);
        }

        [TempleDllLocation(0x101331e0)]
        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            // TODO TEMPORARY
            if (msg.flags.HasFlag(MouseEventFlag.LeftReleased))
            {
                GameSystems.Party.AddToSelection(_obj);
            }

            if (msg.flags.HasFlag(MouseEventFlag.LeftDown))
            {
                if (dword_10BE33E8 && !dword_10BE3414)
                {
                    dword_10BE3414 = true;
                    return true;
                }
            }

            if (msg.flags.HasFlag(MouseEventFlag.RightReleased))
            {
                if (UiSystems.CharSheet.State != CharInventoryState.LevelUp)
                {
                    if (!GameSystems.Combat.IsCombatActive() || GameSystems.D20.Initiative.CurrentActor == _obj)
                    {
                        if (UiSystems.CharSheet.HasCurrentCritter
                            && UiSystems.CharSheet.State != CharInventoryState.LevelUp)
                        {
                            if (UiSystems.CharSheet.CurrentCritter == _obj)
                            {
                                UiSystems.CharSheet.CurrentPage = 0;
                                UiSystems.CharSheet.Hide(CharInventoryState.Closed);
                            }
                            else
                            {
                                UiSystems.CharSheet.ShowInState(0, _obj);
                            }
                        }
                        else
                        {
                            UiSystems.UtilityBar.HideOpenedWindows(true);
                            UiSystems.CharSheet.State = 0;
                            UiSystems.CharSheet.Show(_obj);
                        }
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x10132850)]
        public override void Render()
        {
            UpdatePortrait();

            var hpMax = GameSystems.Stat.StatLevelGet(_obj, Stat.hp_max);
            var hpCurrent = GameSystems.Stat.StatLevelGet(_obj, Stat.hp_current);
            WidgetImage imageToUse;
            if (hpCurrent < 1)
            {
                // TODO: This should probably use a D20 dispatch to see if
                // TODO: the character has the disabled or dying status
                imageToUse = _greyPortrait;
            }
            else
            {
                if (ButtonState == LgcyButtonState.Disabled)
                {
                    imageToUse = _greyPortrait;
                }
                else
                {
                    imageToUse = _normalPortrait;
                }
            }

            ClearContent();
            AddContent(imageToUse);

            if (GameSystems.Party.ShowHitPoints)
            {
                if (_hpLabel == null)
                {
                    _hpLabel = new WidgetLegacyText("HP", PredefinedFont.ARIAL_10, HpTextStyle);
                }

                var hpText = $"@0{hpCurrent}/{hpMax}";
                var subdualDamage = _obj.GetInt32(obj_f.critter_subdual_damage);
                if (subdualDamage > 0)
                {
                    hpText += $"@1({subdualDamage})";
                }

                _hpLabel.Text = hpText;
                _hpLabel.SetY(GetHeight() - 12);
                AddContent(_hpLabel);
            }

            // TODO: Render flashing get-hit indicator

            if (GameSystems.Party.IsSelected(_obj))
            {
                AddContent(_highlight);
            }

            if (ButtonState == LgcyButtonState.Hovered || UiSystems.Party.ForceHovered == _obj)
            {
                AddContent(_highlightHover);

                if (!UiSystems.CharSheet.HasCurrentCritter && !UiSystems.Logbook.IsVisible)
                {
                    UiSystems.InGameSelect.Focus = _obj;
                }
            }
            else if (ButtonState == LgcyButtonState.Down || UiSystems.Party.ForcePressed == _obj)
            {
                AddContent(_highlightPressed);

                if (!UiSystems.CharSheet.HasCurrentCritter)
                {
                    UiSystems.InGameSelect.AddToGroupArray(_obj);
                }
            }

            base.Render();
        }

        [TempleDllLocation(0x10132850)]
        private void UpdatePortrait()
        {
            var portraitId = _obj.GetInt32(obj_f.critter_portrait);

            if (_currentPortraitId == portraitId)
            {
                return; // Nothing to update
            }

            _currentPortraitId = portraitId;

            var normalPath = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Small);
            _normalPortrait?.Dispose();
            RemoveContent(_normalPortrait);
            _normalPortrait = new WidgetImage(normalPath);
            AddContent(_normalPortrait);

            var greyPath = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.SmallGrey);
            _greyPortrait?.Dispose();
            RemoveContent(_greyPortrait);
            if (greyPath != null)
            {
                _greyPortrait = new WidgetImage(greyPath);
            }

            AddContent(_greyPortrait);
        }
    }
}