using System.Text;
using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.D20.Classes.Prereq;

internal interface IAlignmentRequirement
{
    bool IsCompatible(Alignment alignment);
}

internal class RestrictedAlignmentPrereq : ICritterRequirement, IAlignmentRequirement
{
    private readonly Alignment _restrictedBitmask;

    public RestrictedAlignmentPrereq(Alignment restrictedBitmask)
    {
        _restrictedBitmask = restrictedBitmask;
    }

    public bool FullfillsRequirements(GameObject critter)
    {
        return IsCompatible(critter.GetAlignment());
    }

    public void DescribeRequirement(StringBuilder builder)
    {
        builder.Append("Not alignment " + _restrictedBitmask);
    }

    public bool IsCompatible(Alignment alignment)
    {
        return (alignment & _restrictedBitmask) == 0;
    }
}

public static class AlignmentPrereqs
{
    public static readonly ICritterRequirement NonLawful = new RestrictedAlignmentPrereq(Alignment.LAWFUL);
}