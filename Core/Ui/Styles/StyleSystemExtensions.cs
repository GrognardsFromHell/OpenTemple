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
}