using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Party;

public class PartyUiPortrait : IDisposable
{
    public GameObject PartyMember { get; }

    public WidgetContainer Widget { get; }

    private readonly List<BuffDebuffButton> _buffDebuffIcons = new();

    public PartyUiPortrait(GameObject partyMember, PartyUiParams uiParams)
    {
        PartyMember = partyMember;

        var container =
            new WidgetContainer(uiParams.party_ui_main_window.Width, uiParams.party_ui_main_window.Height);

        var image = new WidgetImage("art/interface/party_ui/Character Portrait Frame.tga");
        image.X = uiParams.party_ui_frame.X;
        image.Y = uiParams.party_ui_frame.Y;
        image.FixedWidth = uiParams.party_ui_frame.Width;
        image.FixedHeight = uiParams.party_ui_frame.Height;
        container.AddContent(image);

        var portraitButton = new PortraitButton(partyMember);
        portraitButton.Size = uiParams.party_ui_portrait_button.Size;
        portraitButton.Pos = uiParams.party_ui_portrait_button.Location;
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
            NormalImagePath = uiParams.Textures[PartyUiTexture.DismissBtnNormal],
            DisabledImagePath = uiParams.Textures[PartyUiTexture.DismissBtnDisabled],
            HoverImagePath = uiParams.Textures[PartyUiTexture.DismissBtnHover],
            PressedImagePath = uiParams.Textures[PartyUiTexture.DismissBtnPressed],
        });
        DismissButton.AddClickListener(OnDismissClick);
        container.Add(DismissButton);

        // Level Up icon
        LevelUpButton = new WidgetButton(uiParams.party_ui_level_icon);
        LevelUpButton.SetStyle(new WidgetButtonStyle
        {
            NormalImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnNormal],
            DisabledImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnDisabled],
            HoverImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnHovered],
            PressedImagePath = uiParams.Textures[PartyUiTexture.LevelUpBtnPressed],
        });
        LevelUpButton.AddClickListener(OnLevelUpClick);
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
        if (GameSystems.Combat.IsCombatActive())
            return;
        if (GameSystems.Critter.IsDeadNullDestroyed(PartyMember))
            return;

        UiSystems.CharSheet.Hide(0);
        UiSystems.CharSheet.State = CharInventoryState.LevelUp;
        GameUiBridge.OpenInventory(PartyMember);
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
                icon.TooltipText = GameSystems.D20.BuffDebuff.GetTooltip(entry);
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
                LevelUpButton.Disabled = true;
            }
            else
            {
                LevelUpButton.Disabled = false;
            }
        }
        else
        {
            LevelUpButton.Visible = false;
        }
    }
}