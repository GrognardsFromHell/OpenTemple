using System.Text;
using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.D20.Classes.Prereq
{
    public interface ICritterRequirement
    {
        bool FullfillsRequirements(GameObjectBody critter);

        void DescribeRequirement(StringBuilder builder);
    }

}
