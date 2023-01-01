
using System.Drawing;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.Widgets;

public abstract class WidgetContent : Styleable
{
    public WidgetBase? Parent
    {
        get => _parent;
        set
        {
            if (value != _parent)
            {
                _parent = value;
                OnParentChanged();
            }
        }
    }

    public bool Visible { get; set; } = true;

    public abstract void Render(PointF origin);

    public void SetBounds(RectangleF contentArea)
    {
        ContentArea = contentArea;
        Dirty = true;
    }

    public RectangleF GetBounds()
    {
        return ContentArea;
    }

    public virtual SizeF GetPreferredSize()
    {
        return PreferredSize;
    }

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            Parent?.NotifyLayoutChange(LayoutChangeFlag.Content);
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            Parent?.NotifyLayoutChange(LayoutChangeFlag.Content);
        }
    }

    public SizeF FixedSize
    {
        get => new(FixedWidth, FixedHeight);
        set
        {
            _fixedWidth = value.Width;
            _fixedHeight = value.Height;
            OnUpdateFixedSize();
            Parent?.NotifyLayoutChange(LayoutChangeFlag.Content);
        }
    }

    public float FixedWidth
    {
        get => _fixedWidth;
        set
        {
            _fixedWidth = value;
            OnUpdateFixedSize();
            Parent?.NotifyLayoutChange(LayoutChangeFlag.Content);
        }
    }

    public float FixedHeight
    {
        get => _fixedHeight;
        set
        {
            _fixedHeight = value;
            OnUpdateFixedSize();
            Parent?.NotifyLayoutChange(LayoutChangeFlag.Content);
        }
    }

    protected RectangleF ContentArea;
    protected SizeF PreferredSize;
    protected bool Dirty = true;
    private float _fixedWidth;
    private float _fixedHeight;
    private WidgetBase? _parent;
    private float _x;
    private float _y;

    public override Styleable? StyleParent => Parent;

    // Widget content does not have its own pseudo-class state
    public override StylingState PseudoClassState => default;

    protected virtual void OnUpdateFixedSize()
    {
    }

    protected virtual void OnParentChanged()
    {
        // When the parent changes, the styling parent has also changed
        InvalidateStyles();
    }

};