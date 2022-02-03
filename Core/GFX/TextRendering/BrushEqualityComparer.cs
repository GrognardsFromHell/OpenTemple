using System;
using System.Collections.Generic;

namespace OpenTemple.Core.GFX.TextRendering;

internal class BrushEqualityComparer : IEqualityComparer<Brush>
{
    public bool Equals(Brush a, Brush b)
    {
        if (a.gradient != b.gradient
            || a.primaryColor != b.primaryColor) {
            return false;
        }
        return !a.gradient || a.secondaryColor == b.secondaryColor;
    }

    public int GetHashCode(Brush brush)
    {
        var hash = new HashCode();
        hash.Add(brush.gradient);
        hash.Add(brush.primaryColor);
        if (brush.gradient) {
            hash.Add(brush.secondaryColor);
        }
        return hash.ToHashCode();
    }
}