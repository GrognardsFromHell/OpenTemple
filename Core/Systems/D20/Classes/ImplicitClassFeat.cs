using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems.D20.Classes;

/// <summary>
/// A feat that is automatically granted by a class when a  certain level is reached.
/// </summary>
public readonly struct ImplicitClassFeat
{
    public readonly int Level;

    public readonly FeatId Feat;

    public ImplicitClassFeat(int level, FeatId feat)
    {
        Level = level;
        Feat = feat;
    }
}