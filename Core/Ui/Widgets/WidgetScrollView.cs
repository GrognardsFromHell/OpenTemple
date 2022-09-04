using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetScrollView : WidgetContainer
{
    private readonly WidgetContainer _container;
    private readonly WidgetScrollBar _scrollBar;
    private int _padding = 5;
    
    public WidgetScrollView(int width, int height) : base(width, height)
    {
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
        UpdateInnerHeight();
    }

    public override void Clear(bool disposeChildren = false)
    {
        _container.Clear(disposeChildren);
        UpdateInnerHeight();
    }

    public int GetInnerWidth()
    {
        return Width - _scrollBar.Width - 2 * _padding;
    }

    public int GetInnerHeight()
    {
        return Height - 2 * _padding;
    }

    public void SetPadding(int padding)
    {
        _padding = padding;

        UpdateInnerContainer();
    }

    public int GetPadding()
    {
        return _padding;
    }

    protected override void HandleMouseWheel(WheelEvent e)
    {
        var curPos = _scrollBar.GetValue();
        var newPos = curPos - (int) (e.DeltaY / 10);
        _scrollBar.SetValue(newPos);
        e.StopPropagation();
    }

    private void UpdateInnerHeight()
    {
        int innerHeight = 0;
        foreach (var child in _container.GetChildren())
        {
            var childY = child.Y;
            var childH = child.Height;
            var bottom = childY + childH;
            if (bottom > innerHeight)
            {
                innerHeight = bottom;
            }
        }

        _scrollBar.Max = innerHeight;
    }

    private void UpdateInnerContainer()
    {
        _container.X = _padding;
        _container.Width = GetInnerWidth();

        _container.Y = _padding;
        _container.Height = GetInnerHeight();
    }
};