using System.Collections.Generic;
using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.Feats;

public struct FeatPrereq
{
    /// see feat_requirement_codes
    public FeatRequirement featPrereqCode;

    public int featPrereqCodeArg;
}

internal class NewFeatSpec
{
    public FeatPropertyFlag flags;
    public string name;
    public string description;
    public string prereqDescr;
    public List<FeatPrereq> prereqs = new List<FeatPrereq>();

    /// for multiselect feats such as Weapon Focus
    public FeatId parentId = (FeatId) 0;

    /// for multiselect feats such as Weapon Focus
    public List<FeatId> children = new List<FeatId>();

    /// for weapon feats which are weapon specific (such as Weapon Focus - Shortsword)
    public WeaponType weapType = WeaponType.none;
}