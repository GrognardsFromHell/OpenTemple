namespace OpenTemple.Core.Ui.Styles;

public static class StyleSystemExtensions
{
    public static void SetMargins(this StyleDefinition styleDefinition, float left, float top, float right, float bottom)
    {
        styleDefinition.MarginLeft = left;
        styleDefinition.MarginTop = top;
        styleDefinition.MarginRight = right;
        styleDefinition.MarginBottom = bottom;
    }

    public static void SetMargins(this StyleDefinition styleDefinition, float horizontal, float vertical)
    {
        styleDefinition.MarginLeft = horizontal;
        styleDefinition.MarginTop = vertical;
        styleDefinition.MarginRight = horizontal;
        styleDefinition.MarginBottom = vertical;
    }

    public static void SetMargins(this StyleDefinition styleDefinition, float margin)
    {
        styleDefinition.MarginLeft = margin;
        styleDefinition.MarginTop = margin;
        styleDefinition.MarginRight = margin;
        styleDefinition.MarginBottom = margin;
    }
    
    public static void SetPadding(this StyleDefinition styleDefinition, float left, float top, float right, float bottom)
    {
        styleDefinition.PaddingLeft = left;
        styleDefinition.PaddingTop = top;
        styleDefinition.PaddingRight = right;
        styleDefinition.PaddingBottom = bottom;
    }

    public static void SetPadding(this StyleDefinition styleDefinition, float horizontal, float vertical)
    {
        styleDefinition.PaddingLeft = horizontal;
        styleDefinition.PaddingTop = vertical;
        styleDefinition.PaddingRight = horizontal;
        styleDefinition.PaddingBottom = vertical;
    }

    public static void SetPadding(this StyleDefinition styleDefinition, float padding)
    {
        styleDefinition.PaddingLeft = padding;
        styleDefinition.PaddingTop = padding;
        styleDefinition.PaddingRight = padding;
        styleDefinition.PaddingBottom = padding;
    }
}