using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems.D20.Conditions;

public static class FavoredEnemies
{
    public static readonly FavoredEnemyType[] Types =
    {
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_ABERRATION, MonsterCategory.aberration),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_ANIMAL, MonsterCategory.animal),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_BEAST, MonsterCategory.beast),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_CONSTRUCT, MonsterCategory.construct),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_DRAGON, MonsterCategory.dragon),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_ELEMENTAL, MonsterCategory.elemental),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_FEY, MonsterCategory.fey),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_GIANT, MonsterCategory.giant),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_MAGICAL_BEAST, MonsterCategory.magical_beast),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_MONSTROUS_HUMANOID, MonsterCategory.monstrous_humanoid),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_OOZE, MonsterCategory.ooze),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_PLANT, MonsterCategory.plant),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_SHAPECHANGER, MonsterCategory.shapechanger),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_UNDEAD, MonsterCategory.undead),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_VERMIN, MonsterCategory.vermin),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_OUTSIDER_EVIL, MonsterCategory.outsider, MonsterSubtype.evil),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_OUTSIDER_GOOD, MonsterCategory.outsider, MonsterSubtype.good),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_OUTSIDER_LAWFUL, MonsterCategory.outsider, MonsterSubtype.lawful),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_OUTSIDER_CHAOTIC, MonsterCategory.outsider, MonsterSubtype.chaotic),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_GOBLINOID, MonsterCategory.humanoid, MonsterSubtype.goblinoid),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_REPTILIAN, MonsterCategory.humanoid, MonsterSubtype.reptilian),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_DWARF, MonsterCategory.humanoid, MonsterSubtype.dwarf),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_ELF, MonsterCategory.humanoid, MonsterSubtype.elf),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_GNOLL, MonsterCategory.humanoid, MonsterSubtype.gnoll),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_GNOME, MonsterCategory.humanoid, MonsterSubtype.gnome),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_HALFLING, MonsterCategory.humanoid, MonsterSubtype.halfling),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_ORC, MonsterCategory.humanoid, MonsterSubtype.orc),
        new FavoredEnemyType(FeatId.FAVORED_ENEMY_HUMANOID_HUMAN, MonsterCategory.humanoid, MonsterSubtype.human),
    };

    public static bool IsOfType(GameObject critter, int favoredEnemyTypeIdx)
    {
        return Types[favoredEnemyTypeIdx].IsOfType(critter);
    }

    public static bool GetFavoredEnemyBonusAgainst(GameObject critter, GameObject target,
        out int bonus, out FeatId featId)
    {
        // As per rules, if a creature matches more than one type, the highest one is used
        var highestFeatCount = 0;
        var highestFeat = FeatId.NONE;
        foreach (var favoredEnemyType in Types)
        {
            var featCount = GameSystems.Feat.HasFeatCountByClass(critter, favoredEnemyType.Feat);
            if (featCount != 0 && featCount > highestFeatCount && favoredEnemyType.IsOfType(target))
            {
                highestFeat = favoredEnemyType.Feat;
                highestFeatCount = featCount;
            }
        }

        if (highestFeatCount > 0 && highestFeat != FeatId.NONE)
        {
            bonus = 2 * highestFeatCount;
            featId = highestFeat;
            return true;
        }
        else
        {
            bonus = 0;
            featId = FeatId.NONE;
            return false;
        }
    }
}

public readonly struct FavoredEnemyType
{
    public readonly FeatId Feat;
    public readonly MonsterCategory Category;
    public readonly MonsterSubtype Subtype;

    public FavoredEnemyType(FeatId feat, MonsterCategory category, MonsterSubtype subtype)
    {
        Feat = feat;
        Category = category;
        Subtype = subtype;
    }

    public FavoredEnemyType(FeatId feat, MonsterCategory category)
    {
        Feat = feat;
        Category = category;
        Subtype = 0;
    }

    public bool IsOfType(GameObject critter)
    {
        if ( GameSystems.Critter.IsCategory(critter, Category) )
        {
            if ( Subtype == 0 || GameSystems.Critter.IsCategorySubtype(critter, Subtype) )
            {
                return true;
            }
        }

        return false;
    }
}