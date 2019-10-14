using System;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Spells
{
    public class KnownSpellsList : WidgetContainer
    {
        private readonly WidgetScrollBar _scrollbar;

        public event Action<SpellStoreData, MemorizedSpellButton> OnMemorizeSpell;

        public KnownSpellsList(Rectangle rectangle, GameObjectBody critter, int classCode) : base(rectangle)
        {
            var spellsKnown = critter.GetSpellArray(obj_f.critter_spells_known_idx);
            var domainSpells = GameSystems.Spell.IsDomainSpell(classCode);

            // Try scrolling one spell per scrollbar-tick
            var buttonHeight = 10;
            var currentY = 0;
            for (var level = 0; level <= 9; level++)
            {
                var headerAdded = false;

                foreach (var spell in spellsKnown)
                {
                    if (!domainSpells && GameSystems.Spell.IsDomainSpell(spell.classCode)
                        || domainSpells && spell.classCode != classCode
                        || spell.spellLevel != level)
                    {
                        continue;
                    }

                    if (!headerAdded)
                    {
                        var levelHeader = new WidgetText($"#{{char_ui_spells:3}} {level}", "char-spell-level");
                        levelHeader.SetY(currentY);
                        currentY += levelHeader.GetPreferredSize().Height;
                        AddContent(levelHeader);
                        headerAdded = true;
                    }

                    var spellOpposesAlignment =
                        GameSystems.Spell.SpellOpposesAlignment(critter, spell.classCode, spell.spellEnum);
                    var spellButton = new KnownSpellButton(
                        new Rectangle(8, currentY, GetWidth() - 8, 12),
                        spellOpposesAlignment,
                        spell
                    );
                    spellButton.SetY(currentY);
                    spellButton.OnMemorizeSpell += (spell, button) => OnMemorizeSpell?.Invoke(spell, button);
                    currentY += spellButton.GetHeight();
                    Add(spellButton);

                    buttonHeight = Math.Max(buttonHeight, spellButton.GetHeight());
                }
            }

            var overscroll = currentY - GetHeight();
            if (overscroll > 0)
            {
                var lines = (int) MathF.Ceiling(overscroll / (float) buttonHeight);

                _scrollbar = new WidgetScrollBar();
                _scrollbar.SetX(GetWidth() - _scrollbar.GetWidth());
                _scrollbar.SetHeight(GetHeight());

                // Clip existing items that overlap the scrollbar
                foreach (var widgetBase in GetChildren())
                {
                    if (widgetBase.GetX() + widgetBase.GetWidth() >= _scrollbar.GetX())
                    {
                        var remainingWidth = Math.Max(0, _scrollbar.GetX() - widgetBase.GetX());
                        widgetBase.SetWidth(remainingWidth);
                    }
                }

                _scrollbar.SetMin(0);
                _scrollbar.SetMax(lines);
                _scrollbar.SetValueChangeHandler(value =>
                {
                    SetScrollOffsetY(value * buttonHeight);
                    _scrollbar.SetY(value * buttonHeight); // Horrible fakery, moving the scrollbar along
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
}