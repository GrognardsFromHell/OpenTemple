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

    public WidgetScrollView(int width, int height) : base(width, height)
    {
        // Otherwise scroll-wheel doesnt work in empty areas
        HitTesting = HitTestingMode.Area;

        var scrollBar = new WidgetScrollBar();
        scrollBar.Height = height;
        scrollBar.X = width - scrollBar.Width;
        _scrollBar = scrollBar;
        base.Add(scrollBar);

        var scrollView = new WidgetContainer(GetInnerWidth(), height);
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

    public int GetInnerWidth()
    {
        return Width - _scrollBar.Width - 2 * ContainerPadding;
    }

    public int GetInnerHeight()
    {
        return Height - 2 * ContainerPadding;
    }

    protected override void HandleMouseWheel(WheelEvent e)
    {
        _scrollBar.DispatchMouseWheel(e, true);
        e.StopPropagation();
    }

    protected internal override void UpdateLayout()
    {
        base.UpdateLayout();

        if (_layoutInvalid)
        {
            var x = (int) ComputedStyles.PaddingLeft;
            var y = (int) ComputedStyles.PaddingTop;
            var innerWidth = GetInnerWidth();
            foreach (var child in _container.Children)
            {
                child.X = x;
                child.Y = y;
                child.AutoSizeWidth = false;
                child.Width = innerWidth;
                y += child.Height + Gap;
            }

            y += (int) ComputedStyles.PaddingBottom;
        
            _scrollBar.Max = Math.Max(0, y - GetInnerHeight());
            if (_container.Children.Count > 0)
            {
                _scrollBar.Quantum = y / _container.Children.Count;
            }

            _layoutInvalid = false;
        }
    }

    private void UpdateInnerContainer()
    {
        _container.X = ContainerPadding;
        _container.Width = GetInnerWidth();

        _container.Y = ContainerPadding;
        _container.Height = GetInnerHeight();
    }

    private void InvalidateLayout()
    {
        _layoutInvalid = true;
    }
}