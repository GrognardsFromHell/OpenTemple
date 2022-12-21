
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

    public abstract void Render();

    public void SetBounds(Rectangle contentArea)
    {
        ContentArea = contentArea;
        Dirty = true;
    }

    public Rectangle GetBounds()
    {
        return ContentArea;
    }

    public virtual Size GetPreferredSize()
    {
        return PreferredSize;
    }

    public int X { get; set; }

    public int Y { get; set; }

    public Size FixedSize
    {
        get => new(FixedWidth, FixedHeight);
        set
        {
            _fixedWidth = value.Width;
            _fixedHeight = value.Height;
            OnUpdateFixedSize();
        }
    }

    public int FixedWidth
    {
        get => _fixedWidth;
        set
        {
            _fixedWidth = value;
            OnUpdateFixedSize();
        }
    }

    public int FixedHeight
    {
        get => _fixedHeight;
        set
        {
            _fixedHeight = value;
            OnUpdateFixedSize();
        }
    }

    protected Rectangle ContentArea;
    protected Size PreferredSize;
    protected bool Dirty = true;
    private int _fixedWidth;
    private int _fixedHeight;
    private WidgetBase? _parent;

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