using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells;

public class MemorizedSpellButton : WidgetButtonBase
{
    private readonly WidgetText _spellName;

    private readonly WidgetRectangle _slotRectangle;

    private SpellSlot _spellSlot;

    public int Level { get; }

    public int SlotIndex { get; }

    public event Action OnUnmemorizeSpell;

    public SpellSlot Slot
    {
        get => _spellSlot;
        set
        {
            _spellSlot = value;

            /* TODO if (GameSystems.Spell.IsDomainSpell(spell.classCode))
            {
                var domainName = GameSystems.Spell.GetSpellDomainName(spell.classCode);
                spellName += " (" + domainName + ")";
            }*/

            _slotRectangle.Visible = value.HasBeenUsed || !value.HasSpell;

            if (value.HasSpell)
            {
                var spellName = GameSystems.Spell.GetSpellName(value.SpellEnum);

                var styleId = !value.HasBeenUsed ? "char-spell-grey" : "char-spell-body";
                _spellName.AddStyle(styleId);
                _spellName.Text = spellName;
                _spellName.Visible = true;

                OnMouseEnter += ShowSpellHelp;
                OnMouseExit += HideSpellHelp;

                TooltipText = GameSystems.Spell.GetSpellName(_spellSlot.SpellEnum);
            }
            else
            {
                _spellName.Visible = false;
            }
        }
    }

    public MemorizedSpellButton(Rectangle rect, int level, int slotIndex) : base(rect)
    {
        Level = level;
        SlotIndex = slotIndex;

        _slotRectangle = new WidgetRectangle();
        _slotRectangle.Pen = new PackedLinearColorA(0xff646464);
        _slotRectangle.Visible = false;
        AddContent(_slotRectangle);

        _spellName = new WidgetText("", "char-spell-body");
        _spellName.Visible = false;
        AddContent(_spellName);

        SetClickHandler(OnClicked);
    }

    private void OnClicked()
    {
        if (_spellSlot.HasSpell)
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                var spellHelpTopic = GameSystems.Spell.GetSpellHelpTopic(_spellSlot.SpellEnum);
                GameSystems.Help.ShowTopic(spellHelpTopic);
            }
            else
            {
                OnUnmemorizeSpell?.Invoke();
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
    private void ShowSpellHelp(MessageWidgetArgs obj)
    {
        if (_spellSlot.HasSpell)
        {
            var helpText = GameSystems.Spell.GetSpellDescription(_spellSlot.SpellEnum);
            UiSystems.CharSheet.Help.SetHelpText(helpText);
        }
    }

    private void HideSpellHelp(MessageWidgetArgs obj)
    {
        UiSystems.CharSheet.Help.ClearHelpText();
    }
}