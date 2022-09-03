using System;
using System.Collections.Immutable;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells;

/// <summary>
/// Represents a slot in the memorized spell list. It can either contain an already memorized spell or
/// represent an empty slot into which a spell can be memorized.
/// </summary>
public class MemorizedSpellButton : WidgetButtonBase
{
    private readonly GameObject _caster;
    private readonly SpellsPerDay _spellsPerDay;
    private readonly int _level;
    private readonly int _slotIndex;   
    private readonly WidgetText _spellName;

    // This outline is shown, when the slot is pending or free
    private readonly WidgetRectangle _slotRectangle;

    private static readonly PackedLinearColorA UnmemorizedBorder = new(0xff646464);
    private static readonly PackedLinearColorA ValidDropTargetBorder = new(0xff007f00);

    public event Action OnChange;

    public SpellSlot Slot
    {
        get => _spellsPerDay.Levels[_level].Slots[_slotIndex];
        set
        {
            _spellsPerDay.Levels[_level].Slots[_slotIndex] = value;
            
            /* TODO if (GameSystems.Spell.IsDomainSpell(spell.classCode))
            {
                var domainName = GameSystems.Spell.GetSpellDomainName(spell.classCode);
                spellName += " (" + domainName + ")";
            }*/

            if (value.HasSpell)
            {
                var spellName = GameSystems.Spell.GetSpellName(value.SpellEnum);

                var styleId = !value.HasBeenUsed ? "char-spell-grey" : "char-spell-body";
                _spellName.StyleIds = ImmutableList.Create(styleId);
                _spellName.Text = spellName;
                _spellName.Visible = true;
                _spellName.X = 2; // Left padding

                TooltipText = GameSystems.Spell.GetSpellName(Slot.SpellEnum);
            }
            else
            {
                _spellName.Visible = false;
                TooltipText = null;
            }

            UpdateSlotRectangle();
            
            OnChange?.Invoke();
        }
    }

    public MemorizedSpellButton(
        GameObject caster, 
        Rectangle rect,
        SpellsPerDay spellsPerDay,
        int level,
        int slotIndex) : base(rect)
    {
        _caster = caster;
        _spellsPerDay = spellsPerDay;
        _level = level;
        _slotIndex = slotIndex;

        _slotRectangle = new WidgetRectangle();
        _slotRectangle.Pen = UnmemorizedBorder;
        _slotRectangle.Visible = false;
        AddContent(_slotRectangle);

        _spellName = new WidgetText("", "char-spell-body");
        _spellName.Visible = false;
        AddContent(_spellName);

        AddClickListener(OnClicked);

        OnBeforeRender += UpdateSlotRectangle;
        OnMouseEnter += ShowSpellHelp;
        OnMouseLeave += HideSpellHelp;
    }

    private void UpdateSlotRectangle()
    {
        // Highlight valid drop targets for known spells
        if (Globals.UiManager.DraggedObject is DraggedKnownSpell knownSpell && CanMemorize(knownSpell.Spell))
        {
            _slotRectangle.Visible = true;
            _slotRectangle.Pen = ValidDropTargetBorder;
        }
        else
        {
            _slotRectangle.Visible = Slot.HasBeenUsed || !Slot.HasSpell;
            _slotRectangle.Pen = UnmemorizedBorder;
        }
    }

    private void OnClicked()
    {
        if (Slot.HasSpell)
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                var spellHelpTopic = GameSystems.Spell.GetSpellHelpTopic(Slot.SpellEnum);
                GameSystems.Help.ShowTopic(spellHelpTopic);
            }
            else
            {
                Slot = default;
            }
        }
    }

    public override bool HandleMouseMessage(MessageMouseArgs msg)
    {
        // Forward scroll wheel messages to the parent (which will forward it to the scrollbar)
        if (_parent != null && (msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
        {
            return _parent.HandleMouseMessage(msg);
        }

        return base.HandleMouseMessage(msg);
    }

    [TempleDllLocation(0x101b85a0)]
    private void ShowSpellHelp(MouseEvent e)
    {
        if (Slot.HasSpell)
        {
            var helpText = GameSystems.Spell.GetSpellDescription(Slot.SpellEnum);
            UiSystems.CharSheet.Help.SetHelpText(helpText);
        }
    }

    private void HideSpellHelp(MouseEvent e)
    {
        UiSystems.CharSheet.Help.ClearHelpText();
    }

    public bool CanMemorize(SpellStoreData knownSpell)
    {
        return _spellsPerDay.IncludesClassCode(knownSpell.classCode)
            &&  _spellsPerDay.CanMemorizeInSlot(_caster, knownSpell, _level, Slot);
    }
}