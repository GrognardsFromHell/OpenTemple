using System;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyPool;

/// <summary>
/// Widget that displays an available player character in the pool.
/// </summary>
internal class PartyPoolSlot : WidgetButtonBase
{
    private PartyPoolPlayer? _player;

    private readonly WidgetRectangle _border;

    private readonly WidgetImage _portrait;

    private readonly WidgetText _text;

    public PartyPoolPlayer? Player
    {
        get => _player;
        set
        {
            _player = value;
            UpdateWidgets();
        }
    }

    public PartyPoolSlot() : base(new Rectangle(0, 0, 452, 51))
    {
        // Original render function was @ 0x101652e0
        _border = new WidgetRectangle();
        AddContent(_border);

        _portrait = new WidgetImage();
        _portrait.FixedSize = new Size(51, 45);
        _portrait.X = 4;
        _portrait.Y = 3;
        AddContent(_portrait);

        _text = new WidgetText();
        _text.X = 57;
        _text.AddStyle("partyPoolSlot");
        AddContent(_text);
    }

    private void UpdateWidgets()
    {
        if (_player == null)
        {
            Visible = false;
            return;
        }

        // Update the portrait
        PackedLinearColorA? backgroundColor = null;
        var borderColor = new PackedLinearColorA(0xFF5d5d5d);

        Visible = true;
        var statusTranslationId = "party_pool:30"; // Not in party
        if (_player.state != SlotState.CanJoin && _player.Selected)
        {
            borderColor = new PackedLinearColorA(0xFFA90000);
            backgroundColor = new PackedLinearColorA(0xFF960000);
            statusTranslationId = "party_pool:32"; // Not compatible
        }
        else if (_player.state == SlotState.WasInParty)
        {
            // Was previously in party, which means somewhere in the game world, the PC exists
            borderColor = new PackedLinearColorA(0xFFA90000);
            backgroundColor = new PackedLinearColorA(0xFF520000);
            statusTranslationId = "party_pool:31"; // Has joined party
        }
        else if (_player.state != SlotState.CanJoin)
        {
            borderColor = new PackedLinearColorA(0xFFA90000);
            backgroundColor = new PackedLinearColorA(0xFF520000);
            statusTranslationId = "party_pool:32"; // Not compatible
        }
        else if (_player.InGroup)
        {
            // Currently in party
            backgroundColor = new PackedLinearColorA(0xFF205120);
            borderColor = new PackedLinearColorA(0xFF08C403);
            statusTranslationId = "party_pool:31"; // Has joined party
        }
        else if (_player.Selected)
        {
            backgroundColor = new PackedLinearColorA(0xFF133159);
            borderColor = new PackedLinearColorA(0xFF1AC3FF);
        }

        _border.Pen = borderColor;
        if (backgroundColor.HasValue)
        {
            _border.Brush = new Brush(backgroundColor.Value);
        }
        else
        {
            _border.Brush = null;
        }

        _portrait.SetTexture(GameSystems.UiArtManager.GetPortraitPath(_player.portraitId, PortraitVariant.Small));

        UpdateSlotText(statusTranslationId);
    }

    private void UpdateSlotText(string statusTranslationId)
    {
        Debug.Assert(_player != null);

        var content = new ComplexInlineElement();

        // First Line
        content.AppendContent($"{_player.name} - ");
        content.AppendTranslation(statusTranslationId);
        content.AppendBreak();

        // Second line
        var genderText = GameSystems.Stat.GetGenderName(_player.gender);
        var raceText = GameSystems.Stat.GetRaceName(_player.race);
        content.AppendContent(Globals.UiAssets.ApplyTranslation($"{genderText} {raceText}\n"));

        bool partyAlignmentIncompatible = false;
        bool paladinOpposedAlignment = false;
        if (_player.state == SlotState.OpposedAlignment)
        {
            partyAlignmentIncompatible = true;
        }
        else if (_player.state == SlotState.PaladinOpposedAlignment)
        {
            paladinOpposedAlignment = true;
        }

        // Third line
        var classText = GameSystems.Stat.GetStatName(_player.primaryClass);
        content.AppendContent(classText);
        if (paladinOpposedAlignment)
        {
            content.AppendTranslation("party_pool:41").AddStyle("partyPoolSlotIncompatible");
        }

        content.AppendBreak();

        // Fourth line
        var alignmentText = GameSystems.Stat.GetAlignmentName(_player.alignment);
        content.AppendContent(alignmentText);
        if (partyAlignmentIncompatible)
        {
            content.AppendTranslation("party_pool:40").AddStyle("partyPoolSlotIncompatible");
        }

        content.AppendBreak();

        _text.Content = content;
    }
}

public enum SlotState
{
    CanJoin = 0,
    OpposedAlignment = 2,
    PaladinOpposedAlignment = 3,
    WasInParty = 4
}