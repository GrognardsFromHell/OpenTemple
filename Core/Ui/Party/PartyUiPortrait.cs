using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Party
{
    public class PartyUiPortrait : IDisposable
    {
        public GameObjectBody PartyMember { get; }

        public WidgetContainer Widget { get; }

        private readonly List<BuffDebuffButton> _buffDebuffIcons = new List<BuffDebuffButton>();

        public event Action<PartyUiPortrait, MessageWidgetArgs> OnPortraitWidgetMsg;

        public event Action<PartyUiPortrait, MessageMouseArgs> OnPortraitMouseMsg;

        public PartyUiPortrait(GameObjectBody partyMember, PartyUiParams uiParams)
        {
            PartyMember = partyMember;

            var container =
                new WidgetContainer(uiParams.party_ui_main_window.Width, uiParams.party_ui_main_window.Height);

            var image = new WidgetImage("art/interface/party_ui/Character Portrait Frame.tga");
            image.SetX(uiParams.party_ui_frame.X);
            image.SetY(uiParams.party_ui_frame.Y);
            image.SetFixedWidth(uiParams.party_ui_frame.Width);
            image.SetFixedHeight(uiParams.party_ui_frame.Height);
            container.AddContent(image);

            var portraitButton = new PortraitButton(partyMember);
            portraitButton.SetSize(uiParams.party_ui_portrait_button.Size);
            portraitButton.SetPos(uiParams.party_ui_portrait_button.Location);
            portraitButton.SetWidgetMsgHandler(args =>
            {
                OnPortraitWidgetMsg?.Invoke(this, args);
                return true;
            });
            portraitButton.SetMouseMsgHandler(args =>
            {
                OnPortraitMouseMsg?.Invoke(this, args);
                return true;
            });
            container.Add(portraitButton);

            // Dismiss button
            DismissButton = new WidgetButton(uiParams.party_ui_remove_icon);
            DismissButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = uiParams.Textures[PartyUiTexture.DismissBtnNormal],
                disabledImagePath = uiParams.Textures[PartyUiTexture.DismissBtnDisabled],
                hoverImagePath = uiParams.Textures[PartyUiTexture.DismissBtnHover],
                pressedImagePath = uiParams.Textures[PartyUiTexture.DismissBtnPressed],
            });
            DismissButton.SetClickHandler(OnDismissClick);
            container.Add(DismissButton);

            // Level Up icon
            LevelUpButton = new WidgetButton(uiParams.party_ui_level_icon);
            LevelUpButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnNormal],
                disabledImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnDisabled],
                hoverImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnHovered],
                pressedImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnPressed],
            });
            LevelUpButton.SetClickHandler(OnLevelUpClick);
            container.Add(LevelUpButton);

            for (var i = 0; i < 8; i++)
            {
                var buffButton = new BuffDebuffButton(uiParams.buff_icons, BuffDebuffType.Buff, i);
                buffButton.SetX(buffButton.GetX() + uiParams.buff_spacing * i);
                _buffDebuffIcons.Add(buffButton);
                buffButton.SetVisible(false);
                container.Add(buffButton);
            }

            for (var i = 0; i < 8; i++)
            {
                var debuffButton = new BuffDebuffButton(uiParams.buff_icons, BuffDebuffType.Debuff, i);
                debuffButton.SetX(debuffButton.GetX() + uiParams.buff_spacing * i);
                debuffButton.SetY(uiParams.ailment_y);
                _buffDebuffIcons.Add(debuffButton);
                debuffButton.SetVisible(false);
                container.Add(debuffButton);
            }

            for (var i = 0; i < 6; i++)
            {
                var conditionButton = new BuffDebuffButton(uiParams.condition, BuffDebuffType.Condition, i);
                conditionButton.SetX(conditionButton.GetX() + uiParams.condition_spacing * i);
                _buffDebuffIcons.Add(conditionButton);
                conditionButton.SetVisible(false);
                container.Add(conditionButton);
            }

            Widget = container;
            Update();
        }

        [TempleDllLocation(0x101336b0)]
        private void OnDismissClick()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10133770)]
        private void OnLevelUpClick()
        {
            Stub.TODO();
        }

        public void Dispose()
        {
            Widget.Dispose();
        }

        public WidgetButton DismissButton { get; private set; }

        public WidgetButton LevelUpButton { get; private set; }

        [TempleDllLocation(0x10134cb0)]
        public void Update()
        {
            var buffDebuff = GameSystems.D20.BuffDebuff.GetBuffDebuff(PartyMember);
            foreach (var icon in _buffDebuffIcons)
            {
                if (buffDebuff.TryGetEntry(icon.Type, icon.Index, out var entry))
                {
                    icon.SetVisible(true);
                    icon.IconPath = GameSystems.D20.BuffDebuff.GetIconPath(entry);
                    icon.Tooltip = GameSystems.D20.BuffDebuff.GetTooltip(entry);
                    icon.HelpTopic = GameSystems.D20.BuffDebuff.GetHelpTopic(entry);
                }
                else
                {
                    icon.SetVisible(false);
                }
            }

            bool disableLevelUp = false;
            if (GameSystems.Critter.IsDeadNullDestroyed(PartyMember))
            {
                DismissButton.SetVisible(true);
                disableLevelUp = true;
            }
            else
            {
                DismissButton.SetVisible(false);
            }

            if (GameSystems.Critter.CanLevelUp(PartyMember))
            {
                LevelUpButton.SetVisible(true);
                if (GameSystems.Combat.IsCombatActive() || disableLevelUp)
                {
                    LevelUpButton.SetDisabled(true);
                }
                else
                {
                    LevelUpButton.SetDisabled(false);
                }
            }
            else
            {
                LevelUpButton.SetVisible(false);
            }
        }
    }

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
        public override void RenderTooltip(int x, int y)
        {
            if (ButtonState == LgcyButtonState.Disabled || Tooltip == null)
            {
                return;
            }

            var preferredSize = _tooltipLabel.GetPreferredSize();
            var contentArea = new Rectangle(x + 10, y - 20, preferredSize.Width, preferredSize.Height);
            _tooltipLabel.SetContentArea(contentArea);
            _tooltipLabel.Render();
        }
    }
}