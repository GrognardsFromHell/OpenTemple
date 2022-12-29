using System;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetScrollView : WidgetContainer
{
    private readonly WidgetContainer _container;
    private readonly WidgetScrollBar _scrollBar;
    private int _gap;
    private bool _layoutInvalid = true;
    public int ContainerPadding { get; set; } = 5;

    public int Gap
    {
        get => _gap;
        set
        {
            _gap = value;
            InvalidateLayout();
        }
    }

    public WidgetScrollView()
    {
        // Otherwise scroll-wheel doesnt work in empty areas
        HitTesting = HitTestingMode.Area;

        var scrollBar = new WidgetScrollBar();
        _scrollBar = scrollBar;
        base.Add(scrollBar);

        var scrollView = new WidgetContainer();
        _container = scrollView;
        base.Add(scrollView);

        scrollBar.SetValueChangeHandler(newValue => { _container.SetScrollOffsetY(newValue); });

        UpdateInnerContainer();
    }

    public override void Add(WidgetBase childWidget)
    {
        _container.Add(childWidget);
        InvalidateLayout();
    }

    public override void Clear(bool disposeChildren = false)
    {
        _container.Clear(disposeChildren);
        InvalidateLayout();
    }

    public float GetInnerWidth()
    {
        return PaddingArea.Width - _scrollBar.BorderArea.Width - 2 * ContainerPadding;
    }

    public float GetInnerHeight()
    {
        return PaddingArea.Height - 2 * ContainerPadding;
    }

    protected override void HandleMouseWheel(WheelEvent e)
    {
        _scrollBar.DispatchMouseWheel(e, true);
        e.StopPropagation();
    }

    protected override void LayoutChildren()        
    {
        base.LayoutChildren();
        
        if (_layoutInvalid)
        {
            var x = ComputedStyles.PaddingLeft;
            var y = ComputedStyles.PaddingTop;
            var innerWidth = GetInnerWidth();
            foreach (var child in _container.Children)
            {
                child.X = x;
                child.Y = y;
                child.Width = Dimension.Pixels(innerWidth);
                var prefChildSize = child.ComputePreferredBorderAreaSize(innerWidth);
                y += prefChildSize.Height + Gap;
            }
            
            y += (int) ComputedStyles.PaddingBottom;

            var scrollbarWidth = _scrollBar.ComputePreferredBorderAreaSize().Width;
            _scrollBar.X = LayoutBox.Width - scrollbarWidth;
            _scrollBar.Height = Dimension.Pixels(LayoutBox.Height);
        
            _scrollBar.Max = Math.Max(0, (int) (y - GetInnerHeight()));
            if (_container.Children.Count > 0)
            {
                _scrollBar.Quantum = (int) y / _container.Children.Count;
            }

            _layoutInvalid = false;
        }
    }

    private void UpdateInnerContainer()
    {
        _container.X = ContainerPadding;
        _container.Width = Dimension.Pixels(GetInnerWidth());

        _container.Y = ContainerPadding;
        _container.Height = Dimension.Pixels(GetInnerHeight());
    }

    private void InvalidateLayout()
    {
        _layoutInvalid = true;
    }
}