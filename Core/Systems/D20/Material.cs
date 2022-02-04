using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.D20;

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

public static class Materials
{
    public static readonly Dictionary<string, Material> IdToMaterial = new()
    {
        {"mat_stone", Material.stone},
        {"mat_brick", Material.brick},
        {"mat_wood", Material.wood},
        {"mat_plant", Material.plant},
        {"mat_flesh", Material.flesh},
        {"mat_metal", Material.metal},
        {"mat_glass", Material.glass},
        {"mat_cloth", Material.cloth},
        {"mat_liquid", Material.liquid},
        {"mat_paper", Material.paper},
        {"mat_gas", Material.gas},
        {"mat_force", Material.force},
        {"mat_fire", Material.fire},
        {"mat_powder", Material.powder},
    };
    
    public static readonly Dictionary<Material, string> MaterialToId = IdToMaterial
        .ToDictionary(x => x.Value, x => x.Key);
}

public static class ObjectMaterialExtensions
{
    public static Material GetMaterial(this GameObject obj)
    {
        return (Material) obj.GetInt32(obj_f.material);
    }
}