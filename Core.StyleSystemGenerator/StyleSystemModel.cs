
using System.Collections.Generic;

namespace Core.StyleSystemGenerator;

public class PropertyType
{
    public string Name { get; }
    public bool IsValueType { get; }
    public bool IsEnum { get; protected set; }

    public PropertyType(string name, bool isValueType)
    {
        Name = name;
        IsValueType = isValueType;
        PropertyTypes.Types.Add(this);
    }
}

public class EnumPropertyType : PropertyType
{
    public List<(string, int?)> Literals { get; } = new();

    public EnumPropertyType(string name) : base(name, true)
    {
        IsEnum = true;
    }

    public EnumPropertyType Add(string name, int? value = null)
    {
        Literals.Add((name, value));
        return this;
    }
}

public static class PropertyTypes
{
    public static List<PropertyType> Types { get; } = new();

    public static readonly PropertyType Boolean = new("bool", true);
    public static readonly PropertyType Float = new("float", true);
    public static readonly PropertyType String = new("string", false);
    public static readonly PropertyType Color = new("OpenTemple.Core.GFX.PackedLinearColorA", true);

    public static readonly EnumPropertyType TextAlign = new EnumPropertyType("TextAlign")
        .Add("Left", 0)
        .Add("Center")
        .Add("Right")
        .Add("Justified");
    public static readonly EnumPropertyType ParagraphAlign = new EnumPropertyType("ParagraphAlign")
        .Add("Near", 0)
        .Add("Far")
        .Add("Center");
    public static readonly EnumPropertyType WordWrap = new EnumPropertyType("WordWrap")
        .Add("Wrap", 0)
        .Add("NoWrap")
        .Add("EmergencyBreak")
        .Add("WholeWord")
        .Add("Character");
    public static readonly EnumPropertyType TrimMode = new EnumPropertyType("TrimMode")
        .Add("None", 0)
        .Add("Character")
        .Add("Word");
    public static readonly EnumPropertyType TrimmingSign = new EnumPropertyType("TrimmingSign")
        .Add("None", 0)
        .Add("Ellipsis");
    public static readonly EnumPropertyType FontStretch = new EnumPropertyType("FontStretch")
        .Add("UltraCondensed", 0)
        .Add("ExtraCondensed")
        .Add("Condensed")
        .Add("SemiCondensed")
        .Add("Normal")
        .Add("SemiExpanded")
        .Add("Expanded")
        .Add("ExtraExpanded")
        .Add("UltraExpanded");
    public static readonly EnumPropertyType FontStyle = new EnumPropertyType("FontStyle")
        .Add("Normal", 0)
        .Add("Italic")
        .Add("Oblique");
    // See https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight#common_weight_name_mapping
    // Maps directly to the DirectWrite weight enum
    public static readonly EnumPropertyType FontWeight = new EnumPropertyType("FontWeight")
        .Add("Thin", 100)
        .Add("ExtraLight", 200)
        .Add("Light", 300)
        .Add("Regular", 400)
        .Add("Medium", 500)
        .Add("SemiBold", 600)
        .Add("Bold", 700)
        .Add("ExtraBold", 800)
        .Add("Black", 900)
        .Add("ExtraBlack", 950);
}

/// <summary>
/// See StyleSystem.json in Core project.
/// </summary>
public class StyleSystemModel
{
    public List<StylePropertyGroup> PropertyGroups { get; set; } = new()
    {
        new StylePropertyGroup("BackgroundAndBorder")
        {
            Properties =
            {
                new StyleProperty("BackgroundColor", PropertyTypes.Color, "default", inherited: false),
                new StyleProperty("BorderColor", PropertyTypes.Color, "default", inherited: false),
                new StyleProperty("BorderWidth", PropertyTypes.Float, "0", inherited: false)
            }
        },
        new StylePropertyGroup("BoxModel")
        {
            Properties =
            {
                new StyleProperty("MarginTop", PropertyTypes.Float, "0", inherited: false),
                new StyleProperty("MarginRight", PropertyTypes.Float, "0", inherited: false),
                new StyleProperty("MarginBottom", PropertyTypes.Float, "0", inherited: false),
                new StyleProperty("MarginLeft", PropertyTypes.Float, "0", inherited: false),
                new StyleProperty("PaddingTop", PropertyTypes.Float, "0", inherited: false),
                new StyleProperty("PaddingRight", PropertyTypes.Float, "0", inherited: false),
                new StyleProperty("PaddingBottom", PropertyTypes.Float, "0", inherited: false),
                new StyleProperty("PaddingLeft", PropertyTypes.Float, "0", inherited: false),
            }
        },
        new StylePropertyGroup("Paragraph")
        {
            Properties =
            {
                new StyleProperty("HangingIndent", PropertyTypes.Boolean, "false"),
                new StyleProperty("Indent", PropertyTypes.Float, "0"),
                new StyleProperty("TabStopWidth", PropertyTypes.Float, "0"),
                new StyleProperty("TextAlignment", PropertyTypes.TextAlign, "TextAlign.Left"),
                new StyleProperty("ParagraphAlignment", PropertyTypes.ParagraphAlign, "ParagraphAlign.Near"),
                new StyleProperty("WordWrap", PropertyTypes.WordWrap, "WordWrap.Wrap"),
                new StyleProperty("TrimMode", PropertyTypes.TrimMode, "TrimMode.None"),
                new StyleProperty("TrimmingSign", PropertyTypes.TrimmingSign, "TrimmingSign.Ellipsis")
            }
        },
        new StylePropertyGroup("Text")
        {
            Properties =
            {
                new StyleProperty("FontFace", PropertyTypes.String),
                new StyleProperty("FontSize", PropertyTypes.Float),
                new StyleProperty("Color", PropertyTypes.Color),
                new StyleProperty("Underline", PropertyTypes.Boolean, "false"),
                new StyleProperty("LineThrough", PropertyTypes.Boolean, "false"),
                new StyleProperty("FontStretch", PropertyTypes.FontStretch, "FontStretch.Normal"),
                new StyleProperty("FontStyle", PropertyTypes.FontStyle, "FontStyle.Normal"),
                new StyleProperty("FontWeight", PropertyTypes.FontWeight, "FontWeight.Regular"),
                new StyleProperty("DropShadowColor", PropertyTypes.Color, "default"),
                new StyleProperty("OutlineColor", PropertyTypes.Color, "default"),
                new StyleProperty("OutlineWidth", PropertyTypes.Float, "0"),
                new StyleProperty("Kerning", PropertyTypes.Boolean, "true"),
            }
        }
    };
}

public class StylePropertyGroup
{
    public string Name { get; }
    public string VariableOrField { get; }
    public List<StyleProperty> Properties { get; set; } = new();

    public StylePropertyGroup(string name)
    {
        Name = name;
        VariableOrField = char.ToLowerInvariant(Name[0]) + Name.Substring(1);
    }
}

public class StyleProperty
{
    public string Name { get; set; }
    public string JsonName { get; set; }
    public PropertyType Type { get; set; }
    public string VariableOrField { get; }
    public string? DefaultValue { get; }
    public bool IsValueType => Type.IsValueType;

    /// <summary>
    /// Is this property inherited from parents if no specific value is set, or not?
    /// </summary>
    public bool Inherited { get; }

    public StyleProperty(string name, PropertyType type, string? defaultValue = null, bool inherited = true)
    {
        Name = name;
        Type = type;
        VariableOrField = char.ToLowerInvariant(Name[0]) + Name.Substring(1);
        JsonName = VariableOrField;
        DefaultValue = defaultValue;
        Inherited = inherited;
    }
}