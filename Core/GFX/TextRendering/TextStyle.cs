namespace OpenTemple.Core.GFX.TextRendering;

public struct Brush
{
    public bool gradient;
    public PackedLinearColorA primaryColor;
    public PackedLinearColorA secondaryColor;

    public Brush(PackedLinearColorA fillColor)
    {
        gradient = false;
        primaryColor = fillColor;
        secondaryColor = fillColor;
    }

    public static Brush Default => new()
    {
        primaryColor = PackedLinearColorA.White,
        secondaryColor = PackedLinearColorA.White
    };
};