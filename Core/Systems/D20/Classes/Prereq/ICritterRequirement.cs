using System;
using System.Text;
using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.D20.Classes.Prereq
{
    public interface ICritterRequirement
    {
        bool FullfillsRequirements(GameObject critter);

        void DescribeRequirement(StringBuilder builder);
    }

}
