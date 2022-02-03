using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Party;

internal class PortraitButton : WidgetButtonBase
{
    private readonly IStyleDefinition _hpTextSubdualStyle = Globals.UiStyles.Get("hp-text-subdual");

    private readonly GameObject _obj;

    public GameObject PartyMember => _obj;

    private int _currentPortraitId;

    private WidgetImage _normalPortrait;
    private WidgetImage _greyPortrait;
    private WidgetText _hpLabel;
    private WidgetImage _highlight;
    private WidgetImage _highlightHover;
    private WidgetImage _highlightPressed;

    public PortraitButton(GameObject obj)
    {
        _obj = obj;

        _highlight = new WidgetImage("art/interface/party_ui/Highlight.tga");
        _highlightHover = new WidgetImage("art/interface/party_ui/Highlight-hover-on.tga");
        _highlightPressed = new WidgetImage("art/interface/party_ui/Highlight-pressed.tga");

        AddStyle("hp-text");
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

        if (Globals.Config.ShowPartyHitPoints)
        {
            _hpLabel ??= new WidgetText();

            var text = new ComplexInlineElement();
            text.AppendContent($"{hpCurrent}/{hpMax}");
            var subdualDamage = _obj.GetInt32(obj_f.critter_subdual_damage);
            if (subdualDamage > 0)
            {
                text.AppendContent($"({subdualDamage})", _hpTextSubdualStyle);
            }

            _hpLabel.Content = text;
            _hpLabel.Y = Height - 12;
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
                UiSystems.InGameSelect.AddToFocusGroup(_obj);
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