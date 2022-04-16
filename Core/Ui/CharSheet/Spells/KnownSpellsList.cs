using System;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells;

public class KnownSpellsList : WidgetContainer
{
    private readonly WidgetScrollBar _scrollbar;

    public event Action<SpellStoreData, MemorizedSpellButton> OnMemorizeSpell;

    public event Action<SpellStoreData> OnEditMetamagic;
    
    public KnownSpellsList(Rectangle rectangle, GameObject caster, int classCode) : base(rectangle)
    {
        var spellsKnown = caster.GetSpellArray(obj_f.critter_spells_known_idx);
        var domainSpells = GameSystems.Spell.IsDomainSpell(classCode);

        // Try scrolling one spell per scrollbar-tick
        var buttonHeight = 10;
        var currentY = 0;
        for (var level = 0; level <= 9; level++)
        {
            var headerAdded = false;

            foreach (var spell in spellsKnown)
            {
                if (domainSpells != GameSystems.Spell.IsDomainSpell(spell.classCode)
                    || !domainSpells && spell.classCode != classCode
                    || spell.spellLevel != level)
                {
                    continue;
                }

                if (!headerAdded)
                {
                    var levelHeader = new WidgetText($"#{{char_ui_spells:3}} {level}", "char-spell-level");
                    levelHeader.X = 5;
                    levelHeader.Y = currentY;
                    currentY += levelHeader.GetPreferredSize().Height;
                    AddContent(levelHeader);
                    headerAdded = true;
                }

                var spellOpposesAlignment =
                    GameSystems.Spell.SpellOpposesAlignment(caster, spell.classCode, spell.spellEnum);
                var spellButton = new KnownSpellButton(
                    new Rectangle(13, currentY, Width - 8, 12),
                    spellOpposesAlignment,
                    spell
                );
                spellButton.Y = currentY;
                spellButton.OnMemorizeSpell += (spell, button) => OnMemorizeSpell?.Invoke(spell, button);
                Add(spellButton);

                // Add a button that allows writing a copy of this spell with applied metamagic
                if (GameSystems.Spell.GetAvailableMetamagicFeats(caster, spell).Any()
                    || GameSystems.Spell.GetAppliedMetamagicFeats(spell).Any())
                {
                    var metamagicButton = new WidgetButton();
                    metamagicButton.X = 1;
                    metamagicButton.Y = currentY + 1;
                    metamagicButton.SetSize(new Size(12, 12));
                    metamagicButton.SetStyle("metamagic-button");
                    metamagicButton.SetClickHandler(() => OnEditMetamagic?.Invoke(spell));
                    Add(metamagicButton);
                }

                currentY += spellButton.Height;
                buttonHeight = Math.Max(buttonHeight, spellButton.Height);
            }
        }

        var overscroll = currentY - Height;
        if (overscroll > 0)
        {
            var lines = (int) MathF.Ceiling(overscroll / (float) buttonHeight);

            _scrollbar = new WidgetScrollBar();
            _scrollbar.X = Width - _scrollbar.Width;
            _scrollbar.Height = Height;

            // Clip existing items that overlap the scrollbar
            foreach (var widgetBase in GetChildren())
            {
                if (widgetBase.X + widgetBase.Width >= _scrollbar.X)
                {
                    var remainingWidth = Math.Max(0, _scrollbar.X - widgetBase.X);
                    widgetBase.Width = remainingWidth;
                }
            }

            _scrollbar.SetMin(0);
            _scrollbar.Max = lines;
            _scrollbar.SetValueChangeHandler(value =>
            {
                SetScrollOffsetY(value * buttonHeight);
                _scrollbar.Y = value * buttonHeight; // Horrible fakery, moving the scrollbar along
            });
            Add(_scrollbar);
        }
    }

    public override bool HandleMouseMessage(MessageMouseArgs msg)
    {
        // Forward scroll wheel messages to the scrollbar.
        if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
        {
            _scrollbar.HandleMouseMessage(msg);
            return true;
        }

        return base.HandleMouseMessage(msg);
    }
}