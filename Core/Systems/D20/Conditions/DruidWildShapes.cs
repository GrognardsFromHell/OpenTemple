using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenTemple.Core.Systems.D20.Conditions;

public enum WildShapeProtoIdx
{
    Deactivate = 999 + (1 << 24),
    Wolf = 0,
    Dire_Lizard,
    Brown_Bear,
    Polar_Bear,
    Legendary_Rat,
    Dire_Bear,
    Giant_Snake,
    Hill_Giant,
    Elem_Large_Air,
    Elem_Large_Earth,
    Elem_Large_Fire,
    Elem_Large_Water,
    Elem_Huge_Air,
    Elem_Huge_Earth,
    Elem_Huge_Fire,
    Elem_Huge_Water
}

public readonly struct WildShapeSpec
{
    public readonly int protoId;
    public readonly int minLvl;
    public readonly MonsterCategory monCat;

    public WildShapeSpec(int ProtoId, int MinLvl) : this(ProtoId, MinLvl, MonsterCategory.animal)
    {
    }

    public WildShapeSpec(int ProtoId, int MinLvl, MonsterCategory MonCat)
    {
        protoId = ProtoId;
        minLvl = MinLvl;
        monCat = MonCat;
    }
}

public static class DruidWildShapes
{
    public static IImmutableDictionary<WildShapeProtoIdx, WildShapeSpec> Options =
        new Dictionary<WildShapeProtoIdx, WildShapeSpec>
        {
            {WildShapeProtoIdx.Wolf, new WildShapeSpec(14050, 2)}, // 2HD
            {WildShapeProtoIdx.Dire_Lizard, new WildShapeSpec(14450, 5)}, // 5HD
            {WildShapeProtoIdx.Brown_Bear, new WildShapeSpec(14053, 7)}, // large, 6HD
            {WildShapeProtoIdx.Polar_Bear, new WildShapeSpec(14054, 8)}, // large, 8HD
            {WildShapeProtoIdx.Legendary_Rat, new WildShapeSpec(14451, 6)},
            {WildShapeProtoIdx.Dire_Bear, new WildShapeSpec(14506, 12)},
            {WildShapeProtoIdx.Giant_Snake, new WildShapeSpec(14449, 10)},
            {WildShapeProtoIdx.Hill_Giant, new WildShapeSpec(14217, -1)}, // this is added via a special option
            {WildShapeProtoIdx.Elem_Large_Air, new WildShapeSpec(14292, 16, MonsterCategory.elemental)},
            {WildShapeProtoIdx.Elem_Large_Earth, new WildShapeSpec(14296, 16, MonsterCategory.elemental)},
            {WildShapeProtoIdx.Elem_Large_Fire, new WildShapeSpec(14298, 16, MonsterCategory.elemental)},
            {WildShapeProtoIdx.Elem_Large_Water, new WildShapeSpec(14302, 16, MonsterCategory.elemental)},
            {WildShapeProtoIdx.Elem_Huge_Air, new WildShapeSpec(14508, 20, MonsterCategory.elemental)},
            {WildShapeProtoIdx.Elem_Huge_Earth, new WildShapeSpec(14509, 20, MonsterCategory.elemental)},
            {WildShapeProtoIdx.Elem_Huge_Fire, new WildShapeSpec(14510, 20, MonsterCategory.elemental)},
            {WildShapeProtoIdx.Elem_Huge_Water, new WildShapeSpec(14511, 20, MonsterCategory.elemental)}
        }.ToImmutableDictionary();
}