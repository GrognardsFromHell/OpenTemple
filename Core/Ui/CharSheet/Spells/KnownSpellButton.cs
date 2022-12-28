using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells;

public class KnownSpellButton : WidgetButtonBase
{
    private readonly bool _spellOpposesAlignment;

    private readonly SpellStoreData _spell;

    private readonly WidgetText _nameLabel;

    /// <summary>
    /// Invoked when player indicates they want to memorize this spell into a specific slot.
    /// If the spell should be memorized in the best suitable slot, the button argument will be null.
    /// </summary>
    public event Action<SpellStoreData, MemorizedSpellButton> OnMemorizeSpell;

    public KnownSpellButton(RectangleF rect,
        bool spellOpposesAlignment,
        SpellStoreData spell) : base(rect)
    {
        _spellOpposesAlignment = spellOpposesAlignment;
        _spell = spell;

        var spellName = GameSystems.Spell.GetSpellName(spell.spellEnum);

        if (GameSystems.Spell.IsDomainSpell(spell.classCode))
        {
            var domainName = GameSystems.Spell.GetSpellDomainName(spell.classCode);
            spellName += " (" + domainName + ")";
        }

        var style = spellOpposesAlignment ? "char-spell-grey" : "char-spell-body";
        _nameLabel = new WidgetText(spellName, style);
        AddContent(_nameLabel);

        OnMouseEnter += ShowSpellHelp;
        OnMouseLeave += HideSpellHelp;

        if (spellOpposesAlignment)
        {
            TooltipText = "#{char_ui_spells:12}";
        }
        else
        {
            TooltipText = GameSystems.Spell.GetSpellName(_spell.spellEnum);
            OnOtherClick += e =>
            {
                if (e.Button == MouseButton.Right)
                {
                    OnMemorizeSpell?.Invoke(_spell, null);
                }
            };
        }
    }

    [TempleDllLocation(0x101b78f0)]
    private void DrawSpellNameUnderMouse(int x, int y, object arg)
    {
        ComputedStyles style;

        var caster = UiSystems.CharSheet.CurrentCritter;
        if (GameSystems.Spell.GetSchoolSpecialization(caster, out var specializedSchool, out _, out _)
            && GameSystems.Spell.GetSpellSchoolEnum(_spell.spellEnum) == specializedSchool)
        {
            style = Globals.UiStyles.GetComputed("dragged-spell-name",
                "dragged-spell-name-specialized");
        }
        else
        {
            style = Globals.UiStyles.GetComputed("dragged-spell-name");
        }

        string displayName;
        if (!GameSystems.Spell.IsDomainSpell(_spell.classCode))
        {
            displayName = GameSystems.Spell.GetSpellName(_spell.spellEnum);
        }
        else
        {
            var spellName = GameSystems.Spell.GetSpellName(_spell.spellEnum);
            var domainName = GameSystems.Spell.GetSpellDomainName(_spell.classCode);
            displayName = $"{spellName} ({domainName})";
        }

        var extents = new RectangleF();
        extents.X = x;
        extents.Y = y - BorderArea.Height;
        extents.Width = BorderArea.Width;
        extents.Height = 0;

        Tig.RenderingDevice.TextEngine.RenderText(
            extents,
            style,
            displayName
        );
    }

    protected override void HandleMouseDown(MouseEvent e)
    {
        // No interaction when the spell opposes the caster's alignment
        if (_spellOpposesAlignment)
        {
            return;
        }
        
        if (UiSystems.HelpManager.IsSelectingHelpTarget)
        {
            var spellHelpTopic = GameSystems.Spell.GetSpellHelpTopic(_spell.spellEnum);
            GameSystems.Help.ShowTopic(spellHelpTopic);
        }
        else
        {
            StartDragging();
        }
    }

    protected override void HandleMouseUp(MouseEvent e)
    {
        // No interaction when the spell opposes the caster's alignment
        if (_spellOpposesAlignment)
        {
            return;
        }
        
        var otherWidget = Globals.UiManager.PickWidget(e.X, e.Y);
        if (otherWidget is MemorizedSpellButton memorizedSpellButton)
        {
            OnMemorizeSpell?.Invoke(_spell, memorizedSpellButton);
        }

        StopDragging();
    }

    private void StartDragging()
    {
        _nameLabel.Visible = false;
        Tig.Mouse.SetCursorDrawCallback(DrawSpellNameUnderMouse);
        UiManager.DraggedObject = new DraggedKnownSpell(_spell);
    }

    private void StopDragging()
    {
        _nameLabel.Visible = true;
        Tig.Mouse.SetCursorDrawCallback(null);
        UiManager.DraggedObject = null;
    }

    [TempleDllLocation(0x101b85a0)]
    private void ShowSpellHelp(MouseEvent e)
    {
        var helpText = GameSystems.Spell.GetSpellDescription(_spell.spellEnum);
        UiSystems.CharSheet.Help.SetHelpText(helpText);
    }

    private void HideSpellHelp(MouseEvent e)
    {
        UiSystems.CharSheet.Help.ClearHelpText();
    }
}