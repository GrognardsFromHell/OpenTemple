using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;

namespace Scripts.Spells;

public static class SizeUtils
{
    public static void ResetSizeCategory(GameObject target)
    {

        // set new size - not a good idea, gives you 2x the penalty
        var sizeCat = (SizeCategory) target.GetInt32(obj_f.size);
        // objhandle.obj_set_int(obj_f_size, sizeCat)

        // set new reach
        target.SetInt32(obj_f.critter_reach, GetReachForSizeCategory(sizeCat));

    }

    private static int GetReachForSizeCategory(SizeCategory sizeCat)
    {
        if (sizeCat <= SizeCategory.Medium)
        {
            return 0;
        }
        else
        {
            return 10;
        }
    }

    public static void IncSizeCategory(GameObject obj)
    {
        // set new size - not a good idea, gives you 2x the penalty
        var sizeCat = (SizeCategory) obj.GetInt32(obj_f.size);
        sizeCat++;
        // objhandle.obj_set_int(obj_f_size, sizeCat)

        // set new reach
        obj.SetInt32(obj_f.critter_reach, GetReachForSizeCategory(sizeCat));

    }
}