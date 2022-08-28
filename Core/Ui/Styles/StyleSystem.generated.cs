#nullable enable
using System;
using System.CodeDom.Compiler;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenTemple.Core.Ui.Styles;

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public interface IStyleDefinition {
    public OpenTemple.Core.GFX.PackedLinearColorA? BackgroundColor { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA? BorderColor { get; }
    public float? BorderWidth { get; }
    public float? MarginTop { get; }
    public float? MarginRight { get; }
    public float? MarginBottom { get; }
    public float? MarginLeft { get; }
    public float? PaddingTop { get; }
    public float? PaddingRight { get; }
    public float? PaddingBottom { get; }
    public float? PaddingLeft { get; }
    public bool? HangingIndent { get; }
    public float? Indent { get; }
    public float? TabStopWidth { get; }
    public TextAlign? TextAlignment { get; }
    public ParagraphAlign? ParagraphAlignment { get; }
    public WordWrap? WordWrap { get; }
    public TrimMode? TrimMode { get; }
    public TrimmingSign? TrimmingSign { get; }
    public string? FontFace { get; }
    public float? FontSize { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA? Color { get; }
    public bool? Underline { get; }
    public bool? LineThrough { get; }
    public FontStretch? FontStretch { get; }
    public FontStyle? FontStyle { get; }
    public FontWeight? FontWeight { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA? DropShadowColor { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA? OutlineColor { get; }
    public float? OutlineWidth { get; }
    public bool? Kerning { get; }

    public void MergeInto(StyleDefinition target) {
        if (BackgroundColor != null) {
            target.BackgroundColor = BackgroundColor;
        }
        if (BorderColor != null) {
            target.BorderColor = BorderColor;
        }
        if (BorderWidth != null) {
            target.BorderWidth = BorderWidth;
        }
        if (MarginTop != null) {
            target.MarginTop = MarginTop;
        }
        if (MarginRight != null) {
            target.MarginRight = MarginRight;
        }
        if (MarginBottom != null) {
            target.MarginBottom = MarginBottom;
        }
        if (MarginLeft != null) {
            target.MarginLeft = MarginLeft;
        }
        if (PaddingTop != null) {
            target.PaddingTop = PaddingTop;
        }
        if (PaddingRight != null) {
            target.PaddingRight = PaddingRight;
        }
        if (PaddingBottom != null) {
            target.PaddingBottom = PaddingBottom;
        }
        if (PaddingLeft != null) {
            target.PaddingLeft = PaddingLeft;
        }
        if (HangingIndent != null) {
            target.HangingIndent = HangingIndent;
        }
        if (Indent != null) {
            target.Indent = Indent;
        }
        if (TabStopWidth != null) {
            target.TabStopWidth = TabStopWidth;
        }
        if (TextAlignment != null) {
            target.TextAlignment = TextAlignment;
        }
        if (ParagraphAlignment != null) {
            target.ParagraphAlignment = ParagraphAlignment;
        }
        if (WordWrap != null) {
            target.WordWrap = WordWrap;
        }
        if (TrimMode != null) {
            target.TrimMode = TrimMode;
        }
        if (TrimmingSign != null) {
            target.TrimmingSign = TrimmingSign;
        }
        if (FontFace != null) {
            target.FontFace = FontFace;
        }
        if (FontSize != null) {
            target.FontSize = FontSize;
        }
        if (Color != null) {
            target.Color = Color;
        }
        if (Underline != null) {
            target.Underline = Underline;
        }
        if (LineThrough != null) {
            target.LineThrough = LineThrough;
        }
        if (FontStretch != null) {
            target.FontStretch = FontStretch;
        }
        if (FontStyle != null) {
            target.FontStyle = FontStyle;
        }
        if (FontWeight != null) {
            target.FontWeight = FontWeight;
        }
        if (DropShadowColor != null) {
            target.DropShadowColor = DropShadowColor;
        }
        if (OutlineColor != null) {
            target.OutlineColor = OutlineColor;
        }
        if (OutlineWidth != null) {
            target.OutlineWidth = OutlineWidth;
        }
        if (Kerning != null) {
            target.Kerning = Kerning;
        }
    }
}
[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public sealed class StyleDefinition : IStyleDefinition {
    private OpenTemple.Core.GFX.PackedLinearColorA? _backgroundColor;
    private OpenTemple.Core.GFX.PackedLinearColorA? _borderColor;
    private float? _borderWidth;
    private float? _marginTop;
    private float? _marginRight;
    private float? _marginBottom;
    private float? _marginLeft;
    private float? _paddingTop;
    private float? _paddingRight;
    private float? _paddingBottom;
    private float? _paddingLeft;
    private bool? _hangingIndent;
    private float? _indent;
    private float? _tabStopWidth;
    private TextAlign? _textAlignment;
    private ParagraphAlign? _paragraphAlignment;
    private WordWrap? _wordWrap;
    private TrimMode? _trimMode;
    private TrimmingSign? _trimmingSign;
    private string? _fontFace;
    private float? _fontSize;
    private OpenTemple.Core.GFX.PackedLinearColorA? _color;
    private bool? _underline;
    private bool? _lineThrough;
    private FontStretch? _fontStretch;
    private FontStyle? _fontStyle;
    private FontWeight? _fontWeight;
    private OpenTemple.Core.GFX.PackedLinearColorA? _dropShadowColor;
    private OpenTemple.Core.GFX.PackedLinearColorA? _outlineColor;
    private float? _outlineWidth;
    private bool? _kerning;

    /// <summary>
    /// This event is raised whenever a change is made to this style definition.
    /// </summary>
    public event Action OnChange;

    public OpenTemple.Core.GFX.PackedLinearColorA? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                OnChange?.Invoke();
            }
        }
    }
    public OpenTemple.Core.GFX.PackedLinearColorA? BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor != value)
            {
                _borderColor = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? BorderWidth
    {
        get => _borderWidth;
        set
        {
            if (_borderWidth != value)
            {
                _borderWidth = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? MarginTop
    {
        get => _marginTop;
        set
        {
            if (_marginTop != value)
            {
                _marginTop = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? MarginRight
    {
        get => _marginRight;
        set
        {
            if (_marginRight != value)
            {
                _marginRight = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? MarginBottom
    {
        get => _marginBottom;
        set
        {
            if (_marginBottom != value)
            {
                _marginBottom = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? MarginLeft
    {
        get => _marginLeft;
        set
        {
            if (_marginLeft != value)
            {
                _marginLeft = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? PaddingTop
    {
        get => _paddingTop;
        set
        {
            if (_paddingTop != value)
            {
                _paddingTop = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? PaddingRight
    {
        get => _paddingRight;
        set
        {
            if (_paddingRight != value)
            {
                _paddingRight = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? PaddingBottom
    {
        get => _paddingBottom;
        set
        {
            if (_paddingBottom != value)
            {
                _paddingBottom = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? PaddingLeft
    {
        get => _paddingLeft;
        set
        {
            if (_paddingLeft != value)
            {
                _paddingLeft = value;
                OnChange?.Invoke();
            }
        }
    }
    public bool? HangingIndent
    {
        get => _hangingIndent;
        set
        {
            if (_hangingIndent != value)
            {
                _hangingIndent = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? Indent
    {
        get => _indent;
        set
        {
            if (_indent != value)
            {
                _indent = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? TabStopWidth
    {
        get => _tabStopWidth;
        set
        {
            if (_tabStopWidth != value)
            {
                _tabStopWidth = value;
                OnChange?.Invoke();
            }
        }
    }
    public TextAlign? TextAlignment
    {
        get => _textAlignment;
        set
        {
            if (_textAlignment != value)
            {
                _textAlignment = value;
                OnChange?.Invoke();
            }
        }
    }
    public ParagraphAlign? ParagraphAlignment
    {
        get => _paragraphAlignment;
        set
        {
            if (_paragraphAlignment != value)
            {
                _paragraphAlignment = value;
                OnChange?.Invoke();
            }
        }
    }
    public WordWrap? WordWrap
    {
        get => _wordWrap;
        set
        {
            if (_wordWrap != value)
            {
                _wordWrap = value;
                OnChange?.Invoke();
            }
        }
    }
    public TrimMode? TrimMode
    {
        get => _trimMode;
        set
        {
            if (_trimMode != value)
            {
                _trimMode = value;
                OnChange?.Invoke();
            }
        }
    }
    public TrimmingSign? TrimmingSign
    {
        get => _trimmingSign;
        set
        {
            if (_trimmingSign != value)
            {
                _trimmingSign = value;
                OnChange?.Invoke();
            }
        }
    }
    public string? FontFace
    {
        get => _fontFace;
        set
        {
            if (_fontFace != value)
            {
                _fontFace = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                OnChange?.Invoke();
            }
        }
    }
    public OpenTemple.Core.GFX.PackedLinearColorA? Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnChange?.Invoke();
            }
        }
    }
    public bool? Underline
    {
        get => _underline;
        set
        {
            if (_underline != value)
            {
                _underline = value;
                OnChange?.Invoke();
            }
        }
    }
    public bool? LineThrough
    {
        get => _lineThrough;
        set
        {
            if (_lineThrough != value)
            {
                _lineThrough = value;
                OnChange?.Invoke();
            }
        }
    }
    public FontStretch? FontStretch
    {
        get => _fontStretch;
        set
        {
            if (_fontStretch != value)
            {
                _fontStretch = value;
                OnChange?.Invoke();
            }
        }
    }
    public FontStyle? FontStyle
    {
        get => _fontStyle;
        set
        {
            if (_fontStyle != value)
            {
                _fontStyle = value;
                OnChange?.Invoke();
            }
        }
    }
    public FontWeight? FontWeight
    {
        get => _fontWeight;
        set
        {
            if (_fontWeight != value)
            {
                _fontWeight = value;
                OnChange?.Invoke();
            }
        }
    }
    public OpenTemple.Core.GFX.PackedLinearColorA? DropShadowColor
    {
        get => _dropShadowColor;
        set
        {
            if (_dropShadowColor != value)
            {
                _dropShadowColor = value;
                OnChange?.Invoke();
            }
        }
    }
    public OpenTemple.Core.GFX.PackedLinearColorA? OutlineColor
    {
        get => _outlineColor;
        set
        {
            if (_outlineColor != value)
            {
                _outlineColor = value;
                OnChange?.Invoke();
            }
        }
    }
    public float? OutlineWidth
    {
        get => _outlineWidth;
        set
        {
            if (_outlineWidth != value)
            {
                _outlineWidth = value;
                OnChange?.Invoke();
            }
        }
    }
    public bool? Kerning
    {
        get => _kerning;
        set
        {
            if (_kerning != value)
            {
                _kerning = value;
                OnChange?.Invoke();
            }
        }
    }
}

/// <summary>
/// Contains the resolved values for all style properties.
/// </summary>
[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public class ComputedStyles {
    public OpenTemple.Core.GFX.PackedLinearColorA BackgroundColor { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA BorderColor { get; }
    public float BorderWidth { get; }
    public float MarginTop { get; }
    public float MarginRight { get; }
    public float MarginBottom { get; }
    public float MarginLeft { get; }
    public float PaddingTop { get; }
    public float PaddingRight { get; }
    public float PaddingBottom { get; }
    public float PaddingLeft { get; }
    public bool HangingIndent { get; }
    public float Indent { get; }
    public float TabStopWidth { get; }
    public TextAlign TextAlignment { get; }
    public ParagraphAlign ParagraphAlignment { get; }
    public WordWrap WordWrap { get; }
    public TrimMode TrimMode { get; }
    public TrimmingSign TrimmingSign { get; }
    public string FontFace { get; }
    public float FontSize { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA Color { get; }
    public bool Underline { get; }
    public bool LineThrough { get; }
    public FontStretch FontStretch { get; }
    public FontStyle FontStyle { get; }
    public FontWeight FontWeight { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA DropShadowColor { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA OutlineColor { get; }
    public float OutlineWidth { get; }
    public bool Kerning { get; }

    // Generate a constructor to populate every field
    public ComputedStyles(
        OpenTemple.Core.GFX.PackedLinearColorA backgroundColor,
        OpenTemple.Core.GFX.PackedLinearColorA borderColor,
        float borderWidth,
        float marginTop,
        float marginRight,
        float marginBottom,
        float marginLeft,
        float paddingTop,
        float paddingRight,
        float paddingBottom,
        float paddingLeft,
        bool hangingIndent,
        float indent,
        float tabStopWidth,
        TextAlign textAlignment,
        ParagraphAlign paragraphAlignment,
        WordWrap wordWrap,
        TrimMode trimMode,
        TrimmingSign trimmingSign,
        string fontFace,
        float fontSize,
        OpenTemple.Core.GFX.PackedLinearColorA color,
        bool underline,
        bool lineThrough,
        FontStretch fontStretch,
        FontStyle fontStyle,
        FontWeight fontWeight,
        OpenTemple.Core.GFX.PackedLinearColorA dropShadowColor,
        OpenTemple.Core.GFX.PackedLinearColorA outlineColor,
        float outlineWidth,
        bool kerning
    )
    {
        this.BackgroundColor = backgroundColor;
        this.BorderColor = borderColor;
        this.BorderWidth = borderWidth;
        this.MarginTop = marginTop;
        this.MarginRight = marginRight;
        this.MarginBottom = marginBottom;
        this.MarginLeft = marginLeft;
        this.PaddingTop = paddingTop;
        this.PaddingRight = paddingRight;
        this.PaddingBottom = paddingBottom;
        this.PaddingLeft = paddingLeft;
        this.HangingIndent = hangingIndent;
        this.Indent = indent;
        this.TabStopWidth = tabStopWidth;
        this.TextAlignment = textAlignment;
        this.ParagraphAlignment = paragraphAlignment;
        this.WordWrap = wordWrap;
        this.TrimMode = trimMode;
        this.TrimmingSign = trimmingSign;
        this.FontFace = fontFace;
        this.FontSize = fontSize;
        this.Color = color;
        this.Underline = underline;
        this.LineThrough = lineThrough;
        this.FontStretch = fontStretch;
        this.FontStyle = fontStyle;
        this.FontWeight = fontWeight;
        this.DropShadowColor = dropShadowColor;
        this.OutlineColor = outlineColor;
        this.OutlineWidth = outlineWidth;
        this.Kerning = kerning;
    }
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public class StyleResolver
{

    public OpenTemple.Core.GFX.PackedLinearColorA DefaultBackgroundColor { get; } = default;
    public OpenTemple.Core.GFX.PackedLinearColorA DefaultBorderColor { get; } = default;
    public float DefaultBorderWidth { get; } = 0;
    public float DefaultMarginTop { get; } = 0;
    public float DefaultMarginRight { get; } = 0;
    public float DefaultMarginBottom { get; } = 0;
    public float DefaultMarginLeft { get; } = 0;
    public float DefaultPaddingTop { get; } = 0;
    public float DefaultPaddingRight { get; } = 0;
    public float DefaultPaddingBottom { get; } = 0;
    public float DefaultPaddingLeft { get; } = 0;
    public bool DefaultHangingIndent { get; } = false;
    public float DefaultIndent { get; } = 0;
    public float DefaultTabStopWidth { get; } = 0;
    public TextAlign DefaultTextAlignment { get; } = TextAlign.Left;
    public ParagraphAlign DefaultParagraphAlignment { get; } = ParagraphAlign.Near;
    public WordWrap DefaultWordWrap { get; } = WordWrap.Wrap;
    public TrimMode DefaultTrimMode { get; } = TrimMode.None;
    public TrimmingSign DefaultTrimmingSign { get; } = TrimmingSign.Ellipsis;
    public string DefaultFontFace { get; }
    public float DefaultFontSize { get; }
    public OpenTemple.Core.GFX.PackedLinearColorA DefaultColor { get; }
    public bool DefaultUnderline { get; } = false;
    public bool DefaultLineThrough { get; } = false;
    public FontStretch DefaultFontStretch { get; } = FontStretch.Normal;
    public FontStyle DefaultFontStyle { get; } = FontStyle.Normal;
    public FontWeight DefaultFontWeight { get; } = FontWeight.Regular;
    public OpenTemple.Core.GFX.PackedLinearColorA DefaultDropShadowColor { get; } = default;
    public OpenTemple.Core.GFX.PackedLinearColorA DefaultOutlineColor { get; } = default;
    public float DefaultOutlineWidth { get; } = 0;
    public bool DefaultKerning { get; } = true;

    public ComputedStyles DefaultStyle { get; private set; }


    /// <summary>
    /// Initialize a style resolver that copies it's default styles from a given style definition.
    /// It must have the following properties set, otherwise the constructor will throw an exception:
    ///    /// - FontFace
    ///    /// - FontSize
    ///    /// - Color
    ///    /// </<summary>
    public StyleResolver(IStyleDefinition defaultStyle) : this(
        defaultStyle.FontFace ?? throw new ArgumentException("defaultStyle must define FontFace"),
        defaultStyle.FontSize ?? throw new ArgumentException("defaultStyle must define FontSize"),
        defaultStyle.Color ?? throw new ArgumentException("defaultStyle must define Color")
    )
    {
        // Copy over any other defined property to the default values
        if (defaultStyle.BackgroundColor != null)
        {
            DefaultBackgroundColor = defaultStyle.BackgroundColor.Value;
        }
        if (defaultStyle.BorderColor != null)
        {
            DefaultBorderColor = defaultStyle.BorderColor.Value;
        }
        if (defaultStyle.BorderWidth != null)
        {
            DefaultBorderWidth = defaultStyle.BorderWidth.Value;
        }
        if (defaultStyle.MarginTop != null)
        {
            DefaultMarginTop = defaultStyle.MarginTop.Value;
        }
        if (defaultStyle.MarginRight != null)
        {
            DefaultMarginRight = defaultStyle.MarginRight.Value;
        }
        if (defaultStyle.MarginBottom != null)
        {
            DefaultMarginBottom = defaultStyle.MarginBottom.Value;
        }
        if (defaultStyle.MarginLeft != null)
        {
            DefaultMarginLeft = defaultStyle.MarginLeft.Value;
        }
        if (defaultStyle.PaddingTop != null)
        {
            DefaultPaddingTop = defaultStyle.PaddingTop.Value;
        }
        if (defaultStyle.PaddingRight != null)
        {
            DefaultPaddingRight = defaultStyle.PaddingRight.Value;
        }
        if (defaultStyle.PaddingBottom != null)
        {
            DefaultPaddingBottom = defaultStyle.PaddingBottom.Value;
        }
        if (defaultStyle.PaddingLeft != null)
        {
            DefaultPaddingLeft = defaultStyle.PaddingLeft.Value;
        }
        if (defaultStyle.HangingIndent != null)
        {
            DefaultHangingIndent = defaultStyle.HangingIndent.Value;
        }
        if (defaultStyle.Indent != null)
        {
            DefaultIndent = defaultStyle.Indent.Value;
        }
        if (defaultStyle.TabStopWidth != null)
        {
            DefaultTabStopWidth = defaultStyle.TabStopWidth.Value;
        }
        if (defaultStyle.TextAlignment != null)
        {
            DefaultTextAlignment = defaultStyle.TextAlignment.Value;
        }
        if (defaultStyle.ParagraphAlignment != null)
        {
            DefaultParagraphAlignment = defaultStyle.ParagraphAlignment.Value;
        }
        if (defaultStyle.WordWrap != null)
        {
            DefaultWordWrap = defaultStyle.WordWrap.Value;
        }
        if (defaultStyle.TrimMode != null)
        {
            DefaultTrimMode = defaultStyle.TrimMode.Value;
        }
        if (defaultStyle.TrimmingSign != null)
        {
            DefaultTrimmingSign = defaultStyle.TrimmingSign.Value;
        }
        if (defaultStyle.Underline != null)
        {
            DefaultUnderline = defaultStyle.Underline.Value;
        }
        if (defaultStyle.LineThrough != null)
        {
            DefaultLineThrough = defaultStyle.LineThrough.Value;
        }
        if (defaultStyle.FontStretch != null)
        {
            DefaultFontStretch = defaultStyle.FontStretch.Value;
        }
        if (defaultStyle.FontStyle != null)
        {
            DefaultFontStyle = defaultStyle.FontStyle.Value;
        }
        if (defaultStyle.FontWeight != null)
        {
            DefaultFontWeight = defaultStyle.FontWeight.Value;
        }
        if (defaultStyle.DropShadowColor != null)
        {
            DefaultDropShadowColor = defaultStyle.DropShadowColor.Value;
        }
        if (defaultStyle.OutlineColor != null)
        {
            DefaultOutlineColor = defaultStyle.OutlineColor.Value;
        }
        if (defaultStyle.OutlineWidth != null)
        {
            DefaultOutlineWidth = defaultStyle.OutlineWidth.Value;
        }
        if (defaultStyle.Kerning != null)
        {
            DefaultKerning = defaultStyle.Kerning.Value;
        }

        DefaultStyle = CreateDefaultStyle();
    }

    /// <summary>
    /// Initialize a style resolver with default values for properties
    /// that do not have a default value.
    /// </<summary>
    public StyleResolver(
        string defaultFontFace,
        float defaultFontSize,
        OpenTemple.Core.GFX.PackedLinearColorA defaultColor
    ) {
        DefaultFontFace = defaultFontFace;
        DefaultFontSize = defaultFontSize;
        DefaultColor = defaultColor;

        DefaultStyle = CreateDefaultStyle();
    }



    /// <summary>
    /// Compute the effective styles given the syles applied directly to an element, and the parent elements
    /// computed styles.
    /// </summary>
    public ComputedStyles Resolve(IReadOnlyList<IStyleDefinition> styles, ComputedStyles? parentStyles = null)
    {
        // Shortcut for elements that are unstyled and have no parent
        if (styles.Count == 0 && parentStyles == null)
        {
            return DefaultStyle;
        }

        // One bit-field block per 64 properties
        ulong frozenProps1 = default;

                OpenTemple.Core.GFX.PackedLinearColorA backgroundColor = DefaultBackgroundColor;
        OpenTemple.Core.GFX.PackedLinearColorA borderColor = DefaultBorderColor;
        float borderWidth = DefaultBorderWidth;
        float marginTop = DefaultMarginTop;
        float marginRight = DefaultMarginRight;
        float marginBottom = DefaultMarginBottom;
        float marginLeft = DefaultMarginLeft;
        float paddingTop = DefaultPaddingTop;
        float paddingRight = DefaultPaddingRight;
        float paddingBottom = DefaultPaddingBottom;
        float paddingLeft = DefaultPaddingLeft;
        bool hangingIndent = DefaultHangingIndent;
        float indent = DefaultIndent;
        float tabStopWidth = DefaultTabStopWidth;
        TextAlign textAlignment = DefaultTextAlignment;
        ParagraphAlign paragraphAlignment = DefaultParagraphAlignment;
        WordWrap wordWrap = DefaultWordWrap;
        TrimMode trimMode = DefaultTrimMode;
        TrimmingSign trimmingSign = DefaultTrimmingSign;
        string fontFace = DefaultFontFace;
        float fontSize = DefaultFontSize;
        OpenTemple.Core.GFX.PackedLinearColorA color = DefaultColor;
        bool underline = DefaultUnderline;
        bool lineThrough = DefaultLineThrough;
        FontStretch fontStretch = DefaultFontStretch;
        FontStyle fontStyle = DefaultFontStyle;
        FontWeight fontWeight = DefaultFontWeight;
        OpenTemple.Core.GFX.PackedLinearColorA dropShadowColor = DefaultDropShadowColor;
        OpenTemple.Core.GFX.PackedLinearColorA outlineColor = DefaultOutlineColor;
        float outlineWidth = DefaultOutlineWidth;
        bool kerning = DefaultKerning;

        // In the first pass, we resolve all properties, including non-inheritable ones
        foreach (var style in styles)
        {
            // Copy each property that is not-null, but only if it's not applied from a higher-priority source yet

            if ((frozenProps1 & 0x1) == 0 && style.BackgroundColor != null)
            {
                backgroundColor = style.BackgroundColor.Value;
                frozenProps1 |= 0x1;
            }

            if ((frozenProps1 & 0x2) == 0 && style.BorderColor != null)
            {
                borderColor = style.BorderColor.Value;
                frozenProps1 |= 0x2;
            }

            if ((frozenProps1 & 0x4) == 0 && style.BorderWidth != null)
            {
                borderWidth = style.BorderWidth.Value;
                frozenProps1 |= 0x4;
            }

            if ((frozenProps1 & 0x8) == 0 && style.MarginTop != null)
            {
                marginTop = style.MarginTop.Value;
                frozenProps1 |= 0x8;
            }

            if ((frozenProps1 & 0x10) == 0 && style.MarginRight != null)
            {
                marginRight = style.MarginRight.Value;
                frozenProps1 |= 0x10;
            }

            if ((frozenProps1 & 0x20) == 0 && style.MarginBottom != null)
            {
                marginBottom = style.MarginBottom.Value;
                frozenProps1 |= 0x20;
            }

            if ((frozenProps1 & 0x40) == 0 && style.MarginLeft != null)
            {
                marginLeft = style.MarginLeft.Value;
                frozenProps1 |= 0x40;
            }

            if ((frozenProps1 & 0x80) == 0 && style.PaddingTop != null)
            {
                paddingTop = style.PaddingTop.Value;
                frozenProps1 |= 0x80;
            }

            if ((frozenProps1 & 0x100) == 0 && style.PaddingRight != null)
            {
                paddingRight = style.PaddingRight.Value;
                frozenProps1 |= 0x100;
            }

            if ((frozenProps1 & 0x200) == 0 && style.PaddingBottom != null)
            {
                paddingBottom = style.PaddingBottom.Value;
                frozenProps1 |= 0x200;
            }

            if ((frozenProps1 & 0x400) == 0 && style.PaddingLeft != null)
            {
                paddingLeft = style.PaddingLeft.Value;
                frozenProps1 |= 0x400;
            }

            if ((frozenProps1 & 0x800) == 0 && style.HangingIndent != null)
            {
                hangingIndent = style.HangingIndent.Value;
                frozenProps1 |= 0x800;
            }

            if ((frozenProps1 & 0x1000) == 0 && style.Indent != null)
            {
                indent = style.Indent.Value;
                frozenProps1 |= 0x1000;
            }

            if ((frozenProps1 & 0x2000) == 0 && style.TabStopWidth != null)
            {
                tabStopWidth = style.TabStopWidth.Value;
                frozenProps1 |= 0x2000;
            }

            if ((frozenProps1 & 0x4000) == 0 && style.TextAlignment != null)
            {
                textAlignment = style.TextAlignment.Value;
                frozenProps1 |= 0x4000;
            }

            if ((frozenProps1 & 0x8000) == 0 && style.ParagraphAlignment != null)
            {
                paragraphAlignment = style.ParagraphAlignment.Value;
                frozenProps1 |= 0x8000;
            }

            if ((frozenProps1 & 0x10000) == 0 && style.WordWrap != null)
            {
                wordWrap = style.WordWrap.Value;
                frozenProps1 |= 0x10000;
            }

            if ((frozenProps1 & 0x20000) == 0 && style.TrimMode != null)
            {
                trimMode = style.TrimMode.Value;
                frozenProps1 |= 0x20000;
            }

            if ((frozenProps1 & 0x40000) == 0 && style.TrimmingSign != null)
            {
                trimmingSign = style.TrimmingSign.Value;
                frozenProps1 |= 0x40000;
            }

            if ((frozenProps1 & 0x80000) == 0 && style.FontFace != null)
            {
                fontFace = style.FontFace;
                frozenProps1 |= 0x80000;
            }

            if ((frozenProps1 & 0x100000) == 0 && style.FontSize != null)
            {
                fontSize = style.FontSize.Value;
                frozenProps1 |= 0x100000;
            }

            if ((frozenProps1 & 0x200000) == 0 && style.Color != null)
            {
                color = style.Color.Value;
                frozenProps1 |= 0x200000;
            }

            if ((frozenProps1 & 0x400000) == 0 && style.Underline != null)
            {
                underline = style.Underline.Value;
                frozenProps1 |= 0x400000;
            }

            if ((frozenProps1 & 0x800000) == 0 && style.LineThrough != null)
            {
                lineThrough = style.LineThrough.Value;
                frozenProps1 |= 0x800000;
            }

            if ((frozenProps1 & 0x1000000) == 0 && style.FontStretch != null)
            {
                fontStretch = style.FontStretch.Value;
                frozenProps1 |= 0x1000000;
            }

            if ((frozenProps1 & 0x2000000) == 0 && style.FontStyle != null)
            {
                fontStyle = style.FontStyle.Value;
                frozenProps1 |= 0x2000000;
            }

            if ((frozenProps1 & 0x4000000) == 0 && style.FontWeight != null)
            {
                fontWeight = style.FontWeight.Value;
                frozenProps1 |= 0x4000000;
            }

            if ((frozenProps1 & 0x8000000) == 0 && style.DropShadowColor != null)
            {
                dropShadowColor = style.DropShadowColor.Value;
                frozenProps1 |= 0x8000000;
            }

            if ((frozenProps1 & 0x10000000) == 0 && style.OutlineColor != null)
            {
                outlineColor = style.OutlineColor.Value;
                frozenProps1 |= 0x10000000;
            }

            if ((frozenProps1 & 0x20000000) == 0 && style.OutlineWidth != null)
            {
                outlineWidth = style.OutlineWidth.Value;
                frozenProps1 |= 0x20000000;
            }

            if ((frozenProps1 & 0x40000000) == 0 && style.Kerning != null)
            {
                kerning = style.Kerning.Value;
                frozenProps1 |= 0x40000000;
            }
        }

        // Copy each inheritable property from the parent if it's not yet defined locally
        if (parentStyles != null)
        {
            if ((frozenProps1 & 0x800) == 0)
            {
                                hangingIndent = parentStyles.HangingIndent;
            }
            if ((frozenProps1 & 0x1000) == 0)
            {
                indent = parentStyles.Indent;
            }
            if ((frozenProps1 & 0x2000) == 0)
            {
                tabStopWidth = parentStyles.TabStopWidth;
            }
            if ((frozenProps1 & 0x4000) == 0)
            {
                textAlignment = parentStyles.TextAlignment;
            }
            if ((frozenProps1 & 0x8000) == 0)
            {
                paragraphAlignment = parentStyles.ParagraphAlignment;
            }
            if ((frozenProps1 & 0x10000) == 0)
            {
                wordWrap = parentStyles.WordWrap;
            }
            if ((frozenProps1 & 0x20000) == 0)
            {
                trimMode = parentStyles.TrimMode;
            }
            if ((frozenProps1 & 0x40000) == 0)
            {
                trimmingSign = parentStyles.TrimmingSign;
            }
            if ((frozenProps1 & 0x80000) == 0)
            {
                fontFace = parentStyles.FontFace;
            }
            if ((frozenProps1 & 0x100000) == 0)
            {
                fontSize = parentStyles.FontSize;
            }
            if ((frozenProps1 & 0x200000) == 0)
            {
                color = parentStyles.Color;
            }
            if ((frozenProps1 & 0x400000) == 0)
            {
                underline = parentStyles.Underline;
            }
            if ((frozenProps1 & 0x800000) == 0)
            {
                lineThrough = parentStyles.LineThrough;
            }
            if ((frozenProps1 & 0x1000000) == 0)
            {
                fontStretch = parentStyles.FontStretch;
            }
            if ((frozenProps1 & 0x2000000) == 0)
            {
                fontStyle = parentStyles.FontStyle;
            }
            if ((frozenProps1 & 0x4000000) == 0)
            {
                fontWeight = parentStyles.FontWeight;
            }
            if ((frozenProps1 & 0x8000000) == 0)
            {
                dropShadowColor = parentStyles.DropShadowColor;
            }
            if ((frozenProps1 & 0x10000000) == 0)
            {
                outlineColor = parentStyles.OutlineColor;
            }
            if ((frozenProps1 & 0x20000000) == 0)
            {
                outlineWidth = parentStyles.OutlineWidth;
            }
            if ((frozenProps1 & 0x40000000) == 0)
            {
                kerning = parentStyles.Kerning;
            }
        }

        return new ComputedStyles(
            backgroundColor,
            borderColor,
            borderWidth,
            marginTop,
            marginRight,
            marginBottom,
            marginLeft,
            paddingTop,
            paddingRight,
            paddingBottom,
            paddingLeft,
            hangingIndent,
            indent,
            tabStopWidth,
            textAlignment,
            paragraphAlignment,
            wordWrap,
            trimMode,
            trimmingSign,
            fontFace,
            fontSize,
            color,
            underline,
            lineThrough,
            fontStretch,
            fontStyle,
            fontWeight,
            dropShadowColor,
            outlineColor,
            outlineWidth,
            kerning
        );
    }

    private ComputedStyles CreateDefaultStyle()
    {
        return new ComputedStyles(
            DefaultBackgroundColor,
            DefaultBorderColor,
            DefaultBorderWidth,
            DefaultMarginTop,
            DefaultMarginRight,
            DefaultMarginBottom,
            DefaultMarginLeft,
            DefaultPaddingTop,
            DefaultPaddingRight,
            DefaultPaddingBottom,
            DefaultPaddingLeft,
            DefaultHangingIndent,
            DefaultIndent,
            DefaultTabStopWidth,
            DefaultTextAlignment,
            DefaultParagraphAlignment,
            DefaultWordWrap,
            DefaultTrimMode,
            DefaultTrimmingSign,
            DefaultFontFace,
            DefaultFontSize,
            DefaultColor,
            DefaultUnderline,
            DefaultLineThrough,
            DefaultFontStretch,
            DefaultFontStyle,
            DefaultFontWeight,
            DefaultDropShadowColor,
            DefaultOutlineColor,
            DefaultOutlineWidth,
            DefaultKerning
        );
    }
}
[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public static partial class StyleJsonDeserializer {

    ///<summary>
    ///Tries to deserialize a style definition from the given JSON object.
    ///</summary>
    public static void DeserializeProperties(in JsonElement element, StyleDefinition definition) {
        JsonElement propertyNode;
        if (element.TryGetProperty("backgroundColor", out propertyNode))
        {
            try {
                definition.BackgroundColor = ReadColor(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property BackgroundColor: " + e);
            }
        }
        if (element.TryGetProperty("borderColor", out propertyNode))
        {
            try {
                definition.BorderColor = ReadColor(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property BorderColor: " + e);
            }
        }
        if (element.TryGetProperty("borderWidth", out propertyNode))
        {
            try {
                definition.BorderWidth = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property BorderWidth: " + e);
            }
        }
        if (element.TryGetProperty("marginTop", out propertyNode))
        {
            try {
                definition.MarginTop = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property MarginTop: " + e);
            }
        }
        if (element.TryGetProperty("marginRight", out propertyNode))
        {
            try {
                definition.MarginRight = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property MarginRight: " + e);
            }
        }
        if (element.TryGetProperty("marginBottom", out propertyNode))
        {
            try {
                definition.MarginBottom = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property MarginBottom: " + e);
            }
        }
        if (element.TryGetProperty("marginLeft", out propertyNode))
        {
            try {
                definition.MarginLeft = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property MarginLeft: " + e);
            }
        }
        if (element.TryGetProperty("paddingTop", out propertyNode))
        {
            try {
                definition.PaddingTop = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property PaddingTop: " + e);
            }
        }
        if (element.TryGetProperty("paddingRight", out propertyNode))
        {
            try {
                definition.PaddingRight = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property PaddingRight: " + e);
            }
        }
        if (element.TryGetProperty("paddingBottom", out propertyNode))
        {
            try {
                definition.PaddingBottom = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property PaddingBottom: " + e);
            }
        }
        if (element.TryGetProperty("paddingLeft", out propertyNode))
        {
            try {
                definition.PaddingLeft = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property PaddingLeft: " + e);
            }
        }
        if (element.TryGetProperty("hangingIndent", out propertyNode))
        {
            try {
                definition.HangingIndent = ReadBool(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property HangingIndent: " + e);
            }
        }
        if (element.TryGetProperty("indent", out propertyNode))
        {
            try {
                definition.Indent = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property Indent: " + e);
            }
        }
        if (element.TryGetProperty("tabStopWidth", out propertyNode))
        {
            try {
                definition.TabStopWidth = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property TabStopWidth: " + e);
            }
        }
        if (element.TryGetProperty("textAlignment", out propertyNode))
        {
            try {
                definition.TextAlignment = ReadTextAlign(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property TextAlignment: " + e);
            }
        }
        if (element.TryGetProperty("paragraphAlignment", out propertyNode))
        {
            try {
                definition.ParagraphAlignment = ReadParagraphAlign(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property ParagraphAlignment: " + e);
            }
        }
        if (element.TryGetProperty("wordWrap", out propertyNode))
        {
            try {
                definition.WordWrap = ReadWordWrap(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property WordWrap: " + e);
            }
        }
        if (element.TryGetProperty("trimMode", out propertyNode))
        {
            try {
                definition.TrimMode = ReadTrimMode(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property TrimMode: " + e);
            }
        }
        if (element.TryGetProperty("trimmingSign", out propertyNode))
        {
            try {
                definition.TrimmingSign = ReadTrimmingSign(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property TrimmingSign: " + e);
            }
        }
        if (element.TryGetProperty("fontFace", out propertyNode))
        {
            try {
                definition.FontFace = ReadString(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property FontFace: " + e);
            }
        }
        if (element.TryGetProperty("fontSize", out propertyNode))
        {
            try {
                definition.FontSize = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property FontSize: " + e);
            }
        }
        if (element.TryGetProperty("color", out propertyNode))
        {
            try {
                definition.Color = ReadColor(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property Color: " + e);
            }
        }
        if (element.TryGetProperty("underline", out propertyNode))
        {
            try {
                definition.Underline = ReadBool(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property Underline: " + e);
            }
        }
        if (element.TryGetProperty("lineThrough", out propertyNode))
        {
            try {
                definition.LineThrough = ReadBool(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property LineThrough: " + e);
            }
        }
        if (element.TryGetProperty("fontStretch", out propertyNode))
        {
            try {
                definition.FontStretch = ReadFontStretch(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property FontStretch: " + e);
            }
        }
        if (element.TryGetProperty("fontStyle", out propertyNode))
        {
            try {
                definition.FontStyle = ReadFontStyle(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property FontStyle: " + e);
            }
        }
        if (element.TryGetProperty("fontWeight", out propertyNode))
        {
            try {
                definition.FontWeight = ReadFontWeight(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property FontWeight: " + e);
            }
        }
        if (element.TryGetProperty("dropShadowColor", out propertyNode))
        {
            try {
                definition.DropShadowColor = ReadColor(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property DropShadowColor: " + e);
            }
        }
        if (element.TryGetProperty("outlineColor", out propertyNode))
        {
            try {
                definition.OutlineColor = ReadColor(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property OutlineColor: " + e);
            }
        }
        if (element.TryGetProperty("outlineWidth", out propertyNode))
        {
            try {
                definition.OutlineWidth = ReadFloat(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property OutlineWidth: " + e);
            }
        }
        if (element.TryGetProperty("kerning", out propertyNode))
        {
            try {
                definition.Kerning = ReadBool(propertyNode);
            } catch (Exception e) {
                throw new StyleParsingException("Failed to read property Kerning: " + e);
            }
        }
    }

    // generate read methods for generated enumerations
        
        private static TextAlign? ReadTextAlign(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "left" => TextAlign.Left,
            "center" => TextAlign.Center,
            "right" => TextAlign.Right,
            "justified" => TextAlign.Justified,
                _ => throw new Exception("Invalid value for TextAlign: " + value)
            };
        }
        
        private static ParagraphAlign? ReadParagraphAlign(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "near" => ParagraphAlign.Near,
            "far" => ParagraphAlign.Far,
            "center" => ParagraphAlign.Center,
                _ => throw new Exception("Invalid value for ParagraphAlign: " + value)
            };
        }
        
        private static WordWrap? ReadWordWrap(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "wrap" => WordWrap.Wrap,
            "nowrap" => WordWrap.NoWrap,
            "emergencybreak" => WordWrap.EmergencyBreak,
            "wholeword" => WordWrap.WholeWord,
            "character" => WordWrap.Character,
                _ => throw new Exception("Invalid value for WordWrap: " + value)
            };
        }
        
        private static TrimMode? ReadTrimMode(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "none" => TrimMode.None,
            "character" => TrimMode.Character,
            "word" => TrimMode.Word,
                _ => throw new Exception("Invalid value for TrimMode: " + value)
            };
        }
        
        private static TrimmingSign? ReadTrimmingSign(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "none" => TrimmingSign.None,
            "ellipsis" => TrimmingSign.Ellipsis,
                _ => throw new Exception("Invalid value for TrimmingSign: " + value)
            };
        }
        
        private static FontStretch? ReadFontStretch(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "ultracondensed" => FontStretch.UltraCondensed,
            "extracondensed" => FontStretch.ExtraCondensed,
            "condensed" => FontStretch.Condensed,
            "semicondensed" => FontStretch.SemiCondensed,
            "normal" => FontStretch.Normal,
            "semiexpanded" => FontStretch.SemiExpanded,
            "expanded" => FontStretch.Expanded,
            "extraexpanded" => FontStretch.ExtraExpanded,
            "ultraexpanded" => FontStretch.UltraExpanded,
                _ => throw new Exception("Invalid value for FontStretch: " + value)
            };
        }
        
        private static FontStyle? ReadFontStyle(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "normal" => FontStyle.Normal,
            "italic" => FontStyle.Italic,
            "oblique" => FontStyle.Oblique,
                _ => throw new Exception("Invalid value for FontStyle: " + value)
            };
        }
        
        private static FontWeight? ReadFontWeight(in JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) {
                return null;
            }
    
            var value = element.GetString();
            return value switch {
            "thin" => FontWeight.Thin,
            "extralight" => FontWeight.ExtraLight,
            "light" => FontWeight.Light,
            "regular" => FontWeight.Regular,
            "medium" => FontWeight.Medium,
            "semibold" => FontWeight.SemiBold,
            "bold" => FontWeight.Bold,
            "extrabold" => FontWeight.ExtraBold,
            "black" => FontWeight.Black,
            "extrablack" => FontWeight.ExtraBlack,
                _ => throw new Exception("Invalid value for FontWeight: " + value)
            };
        }
        

}


[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum TextAlign
{
    Left = 0,
    Center,
    Right,
    Justified,
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum ParagraphAlign
{
    Near = 0,
    Far,
    Center,
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum WordWrap
{
    Wrap = 0,
    NoWrap,
    EmergencyBreak,
    WholeWord,
    Character,
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum TrimMode
{
    None = 0,
    Character,
    Word,
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum TrimmingSign
{
    None = 0,
    Ellipsis,
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum FontStretch
{
    UltraCondensed = 0,
    ExtraCondensed,
    Condensed,
    SemiCondensed,
    Normal,
    SemiExpanded,
    Expanded,
    ExtraExpanded,
    UltraExpanded,
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum FontStyle
{
    Normal = 0,
    Italic,
    Oblique,
}

[GeneratedCode("Core.StyleSystemGenerator", "2022-08-28T13:39:21.0773331Z")]
public enum FontWeight
{
    Thin = 100,
    ExtraLight = 200,
    Light = 300,
    Regular = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    ExtraBold = 800,
    Black = 900,
    ExtraBlack = 950,
}

