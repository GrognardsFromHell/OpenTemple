using System;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Spells
{
    public class MemorizedSpellsList : WidgetContainer
    {

        private readonly WidgetScrollBar _scrollbar;

        private readonly GameObjectBody _caster;

        private readonly SpellsPerDay _spellsPerDay;

        public event Action<int, int> OnUnmemorizeSpell;

        public MemorizedSpellsList(Rectangle rectangle, GameObjectBody caster, SpellsPerDay spellsPerDay) :
            base(rectangle)
        {
            _caster = caster;
            _spellsPerDay = spellsPerDay;

            var buttonHeight = 10;
            var currentY = 0;

            foreach (var level in spellsPerDay.Levels)
            {
                if (level.Slots.Length == 0)
                {
                    continue;
                }

                var levelHeader = new WidgetText($"#{{char_ui_spells:3}} {level.Level}", "char-spell-level");
                levelHeader.SetY(currentY);
                currentY += levelHeader.GetPreferredSize().Height;
                AddContent(levelHeader);

                for (var index = 0; index < level.Slots.Length; index++)
                {
                    var spellButton = new MemorizedSpellButton(
                        new Rectangle(8, currentY, GetWidth() - 8, 12),
                        level.Level,
                        index
                    );
                    spellButton.SetY(currentY);
                    spellButton.OnUnmemorizeSpell += () =>
                        OnUnmemorizeSpell?.Invoke(spellButton.Level, spellButton.SlotIndex);
                    currentY += spellButton.GetHeight();
                    Add(spellButton);

                    buttonHeight = Math.Max(buttonHeight, spellButton.GetHeight());
                }
            }

            UpdateSpells();

            var overscroll = currentY - GetHeight();
            if (overscroll > 0)
            {
                var lines = (int) MathF.Ceiling(overscroll / (float) buttonHeight);

                _scrollbar = new WidgetScrollBar();
                _scrollbar.SetX(GetWidth() - _scrollbar.GetWidth());
                _scrollbar.SetHeight(GetHeight());

                // Clip existing items that overlap the scrollbar
                foreach (var childWidget in GetChildren())
                {
                    if (childWidget.GetX() + childWidget.GetWidth() >= _scrollbar.GetX())
                    {
                        var remainingWidth = Math.Max(0, _scrollbar.GetX() - childWidget.GetX());
                        childWidget.SetWidth(remainingWidth);
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

        public void UpdateSpells()
        {
            foreach (var childWidget in GetChildren())
            {
                if (childWidget is MemorizedSpellButton spellButton)
                {
                    if (spellButton.Level >= _spellsPerDay.Levels.Length)
                    {
                        spellButton.SetVisible(false);
                        continue;
                    }

                    ref var level = ref _spellsPerDay.Levels[spellButton.Level];
                    if (spellButton.SlotIndex >= level.Slots.Length)
                    {
                        spellButton.SetVisible(false);
                        continue;
                    }

                    spellButton.Slot = level.Slots[spellButton.SlotIndex];
                }
            }
        }
    }
}