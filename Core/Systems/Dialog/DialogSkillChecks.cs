using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Systems.Dialog
{
    public readonly struct DialogSkillChecks
    {
        public readonly int Count;

        private readonly SkillId _skill1;
        private readonly SkillId _skill2;
        private readonly int _ranks1;
        private readonly int _ranks2;

        public DialogSkillChecks(SkillId skill, int ranks)
        {
            Count = 1;
            _skill1 = skill;
            _ranks1 = ranks;
            _skill2 = default;
            _ranks2 = default;
        }

        /// <summary>
        /// A skill check that allows either skill 1 or skill 2 to be used.
        /// </summary>
        public DialogSkillChecks(SkillId skill1, int ranks1,
            SkillId skill2, int ranks2)
        {
            Count = 1;
            _skill1 = skill1;
            _ranks1 = ranks1;
            _skill2 = skill2;
            _ranks2 = ranks2;
        }
    }
}