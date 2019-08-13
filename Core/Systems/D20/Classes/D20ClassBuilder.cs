using System.Collections.Generic;
using System.Collections.Immutable;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;

namespace SpicyTemple.Core.Systems.D20.Classes
{
    public class D20ClassBuilder
    {
        private readonly Stat _classEnum;

        private int _hitDie;

        private string _conditionName;

        private string _category;

        private ClassDefinitionFlag _flags;

        private string _helpTopic;

        private BaseAttackProgressionType _baseAttack;

        private SavingThrowProgressionType _fortitudeSave;

        private SavingThrowProgressionType _reflexSave;

        private SavingThrowProgressionType _willSave;

        private int _skillPointsPerLevel;

        private readonly ImmutableHashSet<SkillId>.Builder _classSkills = ImmutableHashSet.CreateBuilder<SkillId>();

        private readonly List<IAlignmentRequirement> _alignmentRequirements = new List<IAlignmentRequirement>();

        private readonly List<ImplicitClassFeat> _implicitFeats = new List<ImplicitClassFeat>();

        private D20ClassBuilder(Stat classEnum)
        {
            _classEnum = classEnum;
        }

        public static D20ClassBuilder Create(Stat classEnum)
        {
            return new D20ClassBuilder(classEnum);
        }

        public D20ClassBuilder WithConditionName(string conditionName)
        {
            _conditionName = conditionName;
            return this;
        }

        public D20ClassBuilder WithCondition(ConditionSpec condition)
        {
            return WithConditionName(condition.condName);
        }

        public D20ClassBuilder WithCategory(string category)
        {
            _category = category;
            return this;
        }

        public D20ClassBuilder WithHelpTopic(string helpTopic)
        {
            _helpTopic = helpTopic;
            return this;
        }

        public D20ClassBuilder WithHitDie(int sides)
        {
            _hitDie = sides;
            return this;
        }

        public D20ClassBuilder AddFlags(params ClassDefinitionFlag[] flags)
        {
            foreach (var flag in flags)
            {
                _flags |= flag;
            }

            return this;
        }

        public D20ClassBuilder WithBaseAttackBonus(BaseAttackProgressionType progression)
        {
            _baseAttack = progression;
            return this;
        }

        public D20ClassBuilder WithFortitudeSave(SavingThrowProgressionType progression)
        {
            _fortitudeSave = progression;
            return this;
        }

        public D20ClassBuilder WithReflexSave(SavingThrowProgressionType progression)
        {
            _reflexSave = progression;
            return this;
        }

        public D20ClassBuilder WithWillSave(SavingThrowProgressionType progression)
        {
            _willSave = progression;
            return this;
        }

        public D20ClassBuilder WithSkillPointsPerLevel(int points)
        {
            _skillPointsPerLevel = points;
            return this;
        }

        public D20ClassBuilder WithAlignmentRequirement(IAlignmentRequirement requirement)
        {
            _alignmentRequirements.Add(requirement);
            return this;
        }

        public D20ClassBuilder AddClassSkills(params SkillId[] skills)
        {
            foreach (var skill in skills)
            {
                _classSkills.Add(skill);
            }

            return this;
        }

        public D20ClassBuilder AddFeats(int level, params FeatId[] feats)
        {
            foreach (var feat in feats)
            {
                _implicitFeats.Add(new ImplicitClassFeat(level, feat));
            }

            return this;
        }

        public D20ClassSpec Build()
        {
            var spec = new D20ClassSpec();
            spec.classEnum = _classEnum;
            spec.flags = _flags;
            // TODO spec.deityClass
            spec.helpTopic = _helpTopic;

            return spec;
        }

    }
}