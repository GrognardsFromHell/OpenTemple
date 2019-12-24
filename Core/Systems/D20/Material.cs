using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.D20
{
    public enum Material
    {
        stone = 0,
        brick = 1,
        wood = 2,
        plant = 3,
        flesh = 4,
        metal = 5,
        glass = 6,
        cloth = 7,
        liquid = 8,
        paper = 9,
        gas = 10,
        force = 11,
        fire = 12,
        powder = 13,
    }

    public static class ObjectMaterialExtensions
    {
        public static Material GetMaterial(this GameObjectBody obj)
        {
            return (Material) obj.GetInt32(obj_f.material);
        }
    }

}