using System;
using System.Linq;
using System.Text;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Classes.Prereq
{
    internal class BaseAttackBonusRequirement : ICritterRequirement
    {
        private readonly int _minimumValue;

        public BaseAttackBonusRequirement(int minimumValue)
        {
            _minimumValue = minimumValue;
        }

        public bool FullfillsRequirements(GameObjectBody critter)
        {
            var bab = GameSystems.Critter.GetBaseAttackBonus(critter);
            return bab >= _minimumValue;
        }

        public void DescribeRequirement(StringBuilder builder)
        {
            builder.Append("BAB ").Append(_minimumValue).Append("+");
        }
    }

    internal class FeatRequirement : ICritterRequirement
    {
        // Critter needs any _one_ of these feats
        private readonly FeatId[] _requiredFeats;

        public FeatRequirement(FeatId[] requiredFeats)
        {
            _requiredFeats = requiredFeats.ToArray();
        }

        public bool FullfillsRequirements(GameObjectBody critter)
        {
            foreach (var featId in _requiredFeats)
            {
                if (critter.HasFeat(featId))
                {
                    return true;
                }
            }

            return false;
        }

        public void DescribeRequirement(StringBuilder builder)
        {
            if (_requiredFeats.Length > 1)
            {
                builder.Append("Has one of: ");
            }

            for (var index = 0; index < _requiredFeats.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                var featId = _requiredFeats[index];
                builder.Append(GameSystems.Feat.GetFeatName(featId));
            }
        }
    }

    internal class RaceRequirement : ICritterRequirement
    {
        // Critter needs to be any _one_ of these races
        private readonly RaceId[] _races;

        public RaceRequirement(params RaceId[] races)
        {
            _races = races.ToArray();
        }

        public bool FullfillsRequirements(GameObjectBody critter)
        {
            var race = critter.GetRace();
            return Array.IndexOf(_races, race) != -1;
        }

        public void DescribeRequirement(StringBuilder builder)
        {
            builder.Append("Race: ");

            for (var index = 0; index < _races.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(" or ");
                }

                var raceId = _races[index];
                builder.Append(GameSystems.Stat.GetRaceName(raceId));
            }
        }
    }

    internal class SpellCasterRequirement : ICritterRequirement
    {
        private readonly SpellSourceType _source;

        private readonly int _minLevel;

        public SpellCasterRequirement(SpellSourceType source, int minLevel = 1)
        {
            _source = source;
            _minLevel = minLevel;
        }

        public bool FullfillsRequirements(GameObjectBody critter)
        {
            foreach (var classId in D20ClassSystem.ClassesWithSpellLists)
            {
                var classSpec = D20ClassSystem.Classes[classId];
                if (classSpec.spellSourceType == _source)
                {
                    if (GameSystems.Spell.GetMaxSpellLevel(critter, classId) >= _minLevel)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void DescribeRequirement(StringBuilder builder)
        {
            builder.Append("Can cast " + _source + " spells of level " + _minLevel + " or higher");
        }
    }

    internal class SkillRanksRequirement : ICritterRequirement
    {
        private readonly SkillId _skill;

        private readonly int _ranks;

        public SkillRanksRequirement(SkillId skill, int ranks)
        {
            _skill = skill;
            _ranks = ranks;
        }

        public bool FullfillsRequirements(GameObjectBody critter)
        {
            return GameSystems.Skill.GetSkillRanks(critter, _skill) >= _ranks;
        }

        public void DescribeRequirement(StringBuilder builder)
        {
            builder.Append("At least  " + _ranks + " ranks in " + GameSystems.Skill.GetSkillName(_skill));
        }
    }

    internal class SneakAttackRequirement : ICritterRequirement
    {
        private readonly int _dice;

        public SneakAttackRequirement(int dice = 1)
        {
            _dice = dice;
        }

        public bool FullfillsRequirements(GameObjectBody critter)
        {
            var dice = GameSystems.D20.D20QueryInt(critter, "Sneak Attack Dice");
            return dice >= _dice;
        }

        public void DescribeRequirement(StringBuilder builder)
        {
            builder.Append("At least " + _dice + " dice of sneak attack");
        }
    }

    public static class ClassPrereqs
    {
        public static ICritterRequirement BaseAttackBonus(int minValue) => new BaseAttackBonusRequirement(minValue);

        public static ICritterRequirement Feat(params FeatId[] feats) => new FeatRequirement(feats);

        public static ICritterRequirement Race(params RaceId[] races) => new RaceRequirement(races);

        public static ICritterRequirement ArcaneSpellCaster(int minLevel = 1) =>
            new SpellCasterRequirement(SpellSourceType.Arcane);

        public static ICritterRequirement SkillRanks(SkillId skill, int minRanks) =>
            new SkillRanksRequirement(skill, minRanks);

        public static ICritterRequirement SneakAttack(int minDice) =>
            new SneakAttackRequirement(minDice);
    }
}