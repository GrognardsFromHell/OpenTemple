using System.Collections.Immutable;
using System.Linq;

#nullable enable
namespace OpenTemple.Core.Ui.Styles;

public abstract class Styleable : IStyleable
{
    private IImmutableList<string> _styleIds = ImmutableList<string>.Empty;

    // If this field is null, styles need to be reevaluated
    private ComputedStyles? _computedStyles;

    private StyleDefinition? _localStyles;

    /// <summary>
    /// Defines from which element styles are inherited.
    /// </summary>
    public abstract IStyleable? StyleParent { get; }

    public bool HasLocalStyles => _localStyles != null || _styleIds.Count > 0;

    public ComputedStyles ComputedStyles => _computedStyles ??= UpdateComputedStyles();

    public StyleDefinition LocalStyles
    {
        get
        {
            if (_localStyles == null)
            {
                _localStyles = new StyleDefinition();
                _localStyles.OnChange += InvalidateStyles;
            }

            return _localStyles;
        }
    }

    public IImmutableList<string> StyleIds
    {
        get => _styleIds;
        set
        {
            if (_styleIds.Count != value.Count || !_styleIds.SequenceEqual(value))
            {
                _styleIds = value;
                InvalidateStyles();
            }
        }
    }

    /// <summary>
    /// Adds a style to this styleable element.
    /// </summary>
    public void AddStyle(string id)
    {
        if (!_styleIds.Contains(id))
        {
            _styleIds = _styleIds.Add(id);
            InvalidateStyles();
        }
    }

    /// <summary>
    /// Remove a style from this styleable element.
    /// </summary>
    public void RemoveStyle(string id)
    {
        if (_styleIds.Contains(id))
        {
            _styleIds = _styleIds.Remove(id);
            InvalidateStyles();
        }
    }

    /// <summary>
    /// Convenience method to either enable or disable a given style on this styleable element.
    /// </summary>
    public void ToggleStyle(string id, bool enable)
    {
        if (enable)
        {
            AddStyle(id);
        }
        else
        {
            RemoveStyle(id);
        }
    }

    public void InvalidateStyles()
    {
        _computedStyles = null;
        OnStylesInvalidated();
    }

    private IImmutableList<IStyleDefinition> ComputeEffectiveStyles()
    {
        if (!HasLocalStyles)
        {
            return ImmutableList<IStyleDefinition>.Empty;
        }

        var builder = ImmutableList.CreateBuilder<IStyleDefinition>();
        if (_localStyles != null)
        {
            builder.Add(_localStyles);
        }

        // Add in reverse. Priority is lowest to highest.
        for (var i = _styleIds.Count - 1; i >= 0; i--)
        {
            var styleId = _styleIds[i];
            builder.Add(Globals.UiStyles.Get(styleId));
        }

        return builder.ToImmutable();
    }

    private ComputedStyles UpdateComputedStyles()
    {
        // Reuse the parent's styles directly if this element has no local styles,
        // and the paren't doesn't either.
        if (!HasLocalStyles && StyleParent is {HasLocalStyles: false})
        {
            return StyleParent.ComputedStyles;
        }

        return Globals.UiStyles.StyleResolver.Resolve(
            ComputeEffectiveStyles(),
            StyleParent?.ComputedStyles
        );
    }

    protected virtual void OnStylesInvalidated()
    {
    }

}