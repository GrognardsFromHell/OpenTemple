using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Party
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

            var healthBar = new PortraitHealthBar(
                partyMember,
                new Rectangle(uiParams.party_ui_frame.X + 12, uiParams.party_ui_frame.Y + 55, 51, 7)
            );
            container.Add(healthBar);

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
                buffButton.X = buffButton.X + uiParams.buff_spacing * i;
                _buffDebuffIcons.Add(buffButton);
                buffButton.Visible = false;
                container.Add(buffButton);
            }

            for (var i = 0; i < 8; i++)
            {
                var debuffButton = new BuffDebuffButton(uiParams.buff_icons, BuffDebuffType.Debuff, i);
                debuffButton.X = debuffButton.X + uiParams.buff_spacing * i;
                debuffButton.Y = uiParams.ailment_y;
                _buffDebuffIcons.Add(debuffButton);
                debuffButton.Visible = false;
                container.Add(debuffButton);
            }

            for (var i = 0; i < 6; i++)
            {
                var conditionButton = new BuffDebuffButton(uiParams.condition, BuffDebuffType.Condition, i);
                conditionButton.X = conditionButton.X + uiParams.condition_spacing * i;
                _buffDebuffIcons.Add(conditionButton);
                conditionButton.Visible = false;
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
                    icon.Visible = true;
                    icon.IconPath = GameSystems.D20.BuffDebuff.GetIconPath(entry);
                    icon.Tooltip = GameSystems.D20.BuffDebuff.GetTooltip(entry);
                    icon.HelpTopic = GameSystems.D20.BuffDebuff.GetHelpTopic(entry);
                }
                else
                {
                    icon.Visible = false;
                }
            }

            bool disableLevelUp = false;
            if (GameSystems.Critter.IsDeadNullDestroyed(PartyMember))
            {
                DismissButton.Visible = true;
                disableLevelUp = true;
            }
            else
            {
                DismissButton.Visible = false;
            }

            if (GameSystems.Critter.CanLevelUp(PartyMember))
            {
                LevelUpButton.Visible = true;
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
                LevelUpButton.Visible = false;
            }
        }
    }
}