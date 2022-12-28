using System;

namespace OpenTemple.Core.Ui.Widgets;

public readonly record struct Dimension(DimensionUnit Type, float Value)
{
    public static readonly Dimension Auto = new(DimensionUnit.Auto, 0);

    public float Evaluate(float availableSpace, float uiScale, float preferredSize)
    {
        return Type switch
        {
            DimensionUnit.Auto => preferredSize,
            DimensionUnit.Pixels => Value,
            DimensionUnit.ScreenPixels => uiScale * Value,
            DimensionUnit.Percent when !float.IsFinite(availableSpace) => preferredSize,
            DimensionUnit.Percent => Value * availableSpace / 100,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static Dimension Pixels(float value)
    {
        return new Dimension(DimensionUnit.Pixels, value);
    }

    public static Dimension ScreenPixels(float value)
    {
        return new Dimension(DimensionUnit.ScreenPixels, value);
    }

    public static Dimension Percent(float value)
    {
        return new Dimension(DimensionUnit.Percent, value);
    }

    public override string ToString()
    {
        return Type switch
        {
            DimensionUnit.Auto => "auto",
            DimensionUnit.Pixels => Value + "px",
            DimensionUnit.ScreenPixels => Value + "spx",
            DimensionUnit.Percent => Value + "%",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum DimensionUnit : byte
{
    /// <summary>
    /// The dimension should be determined automatically. Value is not used.
    /// </summary>
    Auto,

    /// <summary>
    /// The unit is pixels as measured by the user interface scene.
    /// </summary>
    Pixels,

    /// <summary>
    /// The unit is pixels as measured by the screen.
    /// </summary>
    ScreenPixels,

    /// <summary>
    /// A dimension expressed in percent of the same dimension content area of the parent. 
    /// </summary>
    Percent
}