using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells;

public class MemorizedSpellsList : WidgetContainer
{
    private const int SlotHeight = 14;
    
    private readonly WidgetScrollBar? _scrollbar;

    private readonly GameObject _caster;

    private readonly SpellsPerDay _spellsPerDay;

    public event Action? OnChange;

    /// <summary>
    /// Key is the spell-level, Value is the list of slots for that level.
    /// </summary>
    private readonly Dictionary<int, List<MemorizedSpellButton>> _slotsByLevel = new();

    public MemorizedSpellsList(Rectangle rectangle, GameObject caster, SpellsPerDay spellsPerDay)
    {
        Pos = rectangle.Location;
        PixelSize = rectangle.Size;
        
        _caster = caster;
        _spellsPerDay = spellsPerDay;

        var buttonHeight = 10;
        var currentY = 0f;

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

            var slots = new List<MemorizedSpellButton>(level.Slots.Length);
            _slotsByLevel[level.Level] = slots;

            for (var index = 0; index < level.Slots.Length; index++)
            {
                var slotButton = new MemorizedSpellButton(
                    _caster,
                    new RectangleF(8, currentY, rectangle.Width - 8, SlotHeight),
                    _spellsPerDay,
                    level.Level,
                    index
                );
                slotButton.Y = currentY;
                slotButton.OnChange += InvokeOnChange;
                currentY += SlotHeight;
                slots.Add(slotButton);
                Add(slotButton);

                buttonHeight = Math.Max(buttonHeight, SlotHeight);
                
                slotButton.Slot = level.Slots[index];
            }
        }

        var overscroll = currentY - rectangle.Height;
        if (overscroll > 0)
        {
            var lines = (int) MathF.Ceiling(overscroll / buttonHeight);

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
                _scrollbar?.DispatchMouseWheel(e);
            };
        }
    }

    public IEnumerable<MemorizedSpellButton> SlotsByLevel(int level)
    {
        return _slotsByLevel.TryGetValue(level, out var slots) 
            ? slots : Enumerable.Empty<MemorizedSpellButton>();
    }

    private TimePoint _lastScrollTick;
    private static readonly TimeSpan ScrollInterval = TimeSpan.FromMilliseconds(100);
    private const int ScrollBandHeight = 15; // How high is the area that will auto-scroll the container

    public override void OnUpdateTime(TimePoint now)
    {
        if (_scrollbar == null || UiManager == null)
        {
            return;
        }
        
        var pos = UiManager.MousePos;
        if (Globals.UiManager.IsDragging && _lastScrollTick + ScrollInterval < now)
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

    private void InvokeOnChange()
    {
        OnChange?.Invoke();
    }
}