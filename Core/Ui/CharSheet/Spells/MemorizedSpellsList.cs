using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells;

public class MemorizedSpellsList : WidgetContainer
{
    private readonly WidgetScrollBar _scrollbar;

    private readonly GameObject _caster;

    private readonly SpellsPerDay _spellsPerDay;

    public event Action<int, int> OnUnmemorizeSpell;

    public MemorizedSpellsList(Rectangle rectangle, GameObject caster, SpellsPerDay spellsPerDay) :
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
            levelHeader.Y = currentY;
            currentY += levelHeader.GetPreferredSize().Height;
            AddContent(levelHeader);

            for (var index = 0; index < level.Slots.Length; index++)
            {
                var spellButton = new MemorizedSpellButton(
                    new Rectangle(8, currentY, Width - 8, 12),
                    level.Level,
                    index
                );
                spellButton.Y = currentY;
                spellButton.OnUnmemorizeSpell += () =>
                    OnUnmemorizeSpell?.Invoke(spellButton.Level, spellButton.SlotIndex);
                currentY += spellButton.Height;
                Add(spellButton);

                buttonHeight = Math.Max(buttonHeight, spellButton.Height);
            }
        }

        UpdateSpells();

        var overscroll = currentY - Height;
        if (overscroll > 0)
        {
            var lines = (int) MathF.Ceiling(overscroll / (float) buttonHeight);

            _scrollbar = new WidgetScrollBar();
            _scrollbar.X = Width - _scrollbar.Width;
            _scrollbar.Height = Height;

            // Clip existing items that overlap the scrollbar
            foreach (var childWidget in GetChildren())
            {
                if (childWidget.X + childWidget.Width >= _scrollbar.X)
                {
                    var remainingWidth = Math.Max(0, _scrollbar.X - childWidget.X);
                    childWidget.Width = remainingWidth;
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
        // Forward scroll wheel messages to the scrollbar
        if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
        {
            _scrollbar?.HandleMouseMessage(msg);
            return true;
        }
        return base.HandleMouseMessage(msg);
    }

    private TimePoint _lastScrollTick;
    private static readonly TimeSpan ScrollInterval = TimeSpan.FromMilliseconds(100);
    private const int ScrollBandHeight = 15; // How high is the area that will auto-scroll the container

    public override void OnUpdateTime(TimePoint timeMs)
    {
        var pos = Tig.Mouse.GetPos();
        if (Globals.UiManager.IsDragging && _lastScrollTick + ScrollInterval < timeMs)
        {
            var contentArea = GetContentArea();
            // Scroll if the cursor is within the scroll-sensitive band
            if (pos.Y >= contentArea.Y && pos.Y < contentArea.Y + ScrollBandHeight)
            {
                _lastScrollTick = TimePoint.Now;
                _scrollbar.SetValue(_scrollbar.GetValue() - 1);
            }
            else if (pos.Y >= contentArea.Bottom - ScrollBandHeight && pos.Y < contentArea.Bottom)
            {
                _lastScrollTick = TimePoint.Now;
                _scrollbar.SetValue(_scrollbar.GetValue() + 1);
            }
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
                    spellButton.Visible = false;
                    continue;
                }

                ref var level = ref _spellsPerDay.Levels[spellButton.Level];
                if (spellButton.SlotIndex >= level.Slots.Length)
                {
                    spellButton.Visible = false;
                    continue;
                }

                spellButton.Slot = level.Slots[spellButton.SlotIndex];
            }
        }
    }
}