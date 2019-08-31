using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static class FavoredEnemies
    {
        public static readonly FavoredEnemyType[] Types =
        {
            new FavoredEnemyType(MonsterCategory.aberration),
            new FavoredEnemyType(MonsterCategory.animal),
            new FavoredEnemyType(MonsterCategory.beast),
            new FavoredEnemyType(MonsterCategory.construct),
            new FavoredEnemyType(MonsterCategory.dragon),
            new FavoredEnemyType(MonsterCategory.elemental),
            new FavoredEnemyType(MonsterCategory.fey),
            new FavoredEnemyType(MonsterCategory.giant),
            new FavoredEnemyType(MonsterCategory.magical_beast),
            new FavoredEnemyType(MonsterCategory.monstrous_humanoid),
            new FavoredEnemyType(MonsterCategory.ooze),
            new FavoredEnemyType(MonsterCategory.plant),
            new FavoredEnemyType(MonsterCategory.shapechanger),
            new FavoredEnemyType(MonsterCategory.undead),
            new FavoredEnemyType(MonsterCategory.vermin),
            new FavoredEnemyType(MonsterCategory.outsider, MonsterSubtype.evil),
            new FavoredEnemyType(MonsterCategory.outsider, MonsterSubtype.good),
            new FavoredEnemyType(MonsterCategory.outsider, MonsterSubtype.lawful),
            new FavoredEnemyType(MonsterCategory.outsider, MonsterSubtype.chaotic),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.goblinoid),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.reptilian),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.dwarf),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.elf),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.gnoll),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.gnome),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.halfling),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.orc),
            new FavoredEnemyType(MonsterCategory.humanoid, MonsterSubtype.human),
        };

        public static bool IsOfType(GameObjectBody critter, int favoredEnemyTypeIdx)
        {
            var favoredEnemyType = Types[favoredEnemyTypeIdx];
            if ( GameSystems.Critter.IsCategory(critter, favoredEnemyType.Category) )
            {
                if ( favoredEnemyType.Subtype == 0 || GameSystems.Critter.IsCategorySubtype(critter, favoredEnemyType.Subtype) )
                {
                    return true;
                }
            }

            return false;
        }
    }

    public readonly struct FavoredEnemyType
    {
        public readonly MonsterCategory Category;
        public readonly MonsterSubtype Subtype;

        public FavoredEnemyType(MonsterCategory category, MonsterSubtype subtype)
        {
            Category = category;
            Subtype = subtype;
        }

        public FavoredEnemyType(MonsterCategory category)
        {
            Category = category;
            Subtype = 0;
        }
    }
}