using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells;

public class KnownSpellsList : WidgetContainer
{
    private const int SlotHeight = 12;
    
    private readonly WidgetScrollBar? _scrollbar;

    public event Action<SpellStoreData, MemorizedSpellButton> OnMemorizeSpell;

    public KnownSpellsList(Rectangle rectangle, GameObject critter, int classCode)
    {
        Pos = rectangle.Location;
        PixelSize = rectangle.Size;
        
        var spellsKnown = critter.GetSpellArray(obj_f.critter_spells_known_idx);
        var domainSpells = GameSystems.Spell.IsDomainSpell(classCode);

        // Try scrolling one spell per scrollbar-tick
        var buttonHeight = 10;
        var currentY = 0f;
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
                    levelHeader.Y = currentY;
                    currentY += levelHeader.GetPreferredSize().Height;
                    AddContent(levelHeader);
                    headerAdded = true;
                }

                var spellOpposesAlignment =
                    GameSystems.Spell.SpellOpposesAlignment(critter, spell.classCode, spell.spellEnum);
                var spellButton = new KnownSpellButton(
                    new RectangleF(8, currentY, rectangle.Width - 8, SlotHeight),
                    spellOpposesAlignment,
                    spell
                );
                spellButton.Y = currentY;
                spellButton.OnMemorizeSpell += (spell, button) => OnMemorizeSpell?.Invoke(spell, button);
                currentY += SlotHeight;
                Add(spellButton);

                buttonHeight = Math.Max(buttonHeight, SlotHeight);
            }
        }

        var overscroll = currentY - rectangle.Height;
        if (overscroll > 0)
        {
            var lines = (int) MathF.Ceiling(overscroll / (float) buttonHeight);

            _scrollbar = new WidgetScrollBar();
            var scrollbarWidth = _scrollbar.ComputePreferredBorderAreaSize().Width;
            _scrollbar.X = rectangle.Width - scrollbarWidth;
            _scrollbar.Height = Dimension.Percent(100);

            // Clip existing items that overlap the scrollbar
            foreach (var childWidget in Children)
            {
                // TODO: This should just implement proper layout...
                if (childWidget.X + childWidget.ComputePreferredBorderAreaSize().Width >= _scrollbar.X)
                {
                    var remainingWidth = Math.Max(0, _scrollbar.X - childWidget.X);
                    childWidget.Width = Dimension.Pixels(remainingWidth);
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
            
            OnMouseWheel += e =>
            {
                _scrollbar.DispatchMouseWheel(e);
            };
        }
    }
}