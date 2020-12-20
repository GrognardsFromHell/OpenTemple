using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation
{
    public class StatBlockWidget
    {
        public WidgetContainer Container { get; }

        private readonly StatBlockAbilityScore[] _abilityScoreWidgets;

        private readonly StatBlockValue _experience;
        private readonly StatBlockValue _level;
        private readonly StatBlockValue _hp;
        private readonly StatBlockValue _ac;
        private readonly StatBlockValue _fortitudeSave;
        private readonly StatBlockValue _reflexSave;
        private readonly StatBlockValue _willSave;
        private readonly StatBlockValue _initiativeBonus;
        private readonly StatBlockValue _speed;
        private readonly StatBlockValue _meleeAttackBonus;
        private readonly StatBlockValue _rangedAttackBonus;
        private readonly StatBlockValue _height;
        private readonly StatBlockValue _weight;

        public StatBlockWidget()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/stat_block_ui.json");
            Container = doc.TakeRootContainer();

            StatBlockAbilityScore AddAbilityScore(string containerId, Stat ability)
            {
                var widget = new StatBlockAbilityScore(ability);
                doc.GetContainer(containerId).Add(widget.Container);
                return widget;
            }

            _abilityScoreWidgets = new[]
            {
                AddAbilityScore("strength", Stat.strength),
                AddAbilityScore("dexterity", Stat.dexterity),
                AddAbilityScore("constitution", Stat.constitution),
                AddAbilityScore("intelligence", Stat.intelligence),
                AddAbilityScore("wisdom", Stat.wisdom),
                AddAbilityScore("charisma", Stat.charisma)
            };

            _experience = new StatBlockValue(doc, "experience");
            _level = new StatBlockValue(doc, "level");
            _hp = new StatBlockValue(doc, "hp");
            _ac = new StatBlockValue(doc, "ac");
            _fortitudeSave = new StatBlockValue(doc, "fortitudeSave");
            _reflexSave = new StatBlockValue(doc, "reflexSave");
            _willSave = new StatBlockValue(doc, "willSave");
            _initiativeBonus = new StatBlockValue(doc, "initiativeBonus");
            _speed = new StatBlockValue(doc, "speed");
            _meleeAttackBonus = new StatBlockValue(doc, "meleeAttackBonus");
            _rangedAttackBonus = new StatBlockValue(doc, "rangedAttackBonus");
            _height = new StatBlockValue(doc, "height");
            _weight = new StatBlockValue(doc, "weight");
        }

        public void Update(CharEditorSelectionPacket pkt, GameObjectBody critter, ChargenStages completedStages)
        {
            for (var i = 0; i < pkt.abilityStats.Length; i++)
            {
                _abilityScoreWidgets[i].IsActive = pkt.abilityStats[i] != -1;
                _abilityScoreWidgets[i].AbilityScore = pkt.abilityStats[i];
            }

            UpdateHeight(pkt);
            UpdateWeight(pkt);
            UpdateExperienceAndLevel(critter, completedStages);
            UpdateHpAndAc(critter, completedStages);
            UpdateSavingThrows(critter, completedStages);
            UpdateInitiativeAndSpeed(critter, completedStages);
            UpdateAttackBonus(critter, completedStages);
        }

        [TempleDllLocation(0x1011d760)]
        private void UpdateHeight(CharEditorSelectionPacket pkt)
        {
            _height.IsActive = pkt.modelScale > 0.0f;
            var feet = pkt.height / 12;
            var inches = pkt.height % 12;
            _height.Value = $"{feet}'{inches}\"";
        }

        [TempleDllLocation(0x1011d910)]
        private void UpdateWeight(CharEditorSelectionPacket pkt)
        {
            _weight.IsActive = pkt.modelScale > 0.0f;
            _weight.Value = pkt.weight.ToString();
        }

        [TempleDllLocation(0x1011d470)]
        private void UpdateExperienceAndLevel(GameObjectBody critter, ChargenStages completedStages)
        {
            if (critter != null && completedStages >= ChargenStages.CG_Stage_Class)
            {
                // Vanilla just used constants (exp: 0, lvl: 1) here, but we query the
                // actual object for consistency's sake
                _experience.IsActive = true;
                _experience.Value = critter.GetStat(Stat.experience).ToString();
                _level.IsActive = true;
                _level.Value = critter.GetStat(Stat.level).ToString();
            }
            else
            {
                _experience.IsActive = false;
                _level.IsActive = false;
            }
        }

        [TempleDllLocation(0x1011cd10)]
        private void UpdateHpAndAc(GameObjectBody critter, ChargenStages completedStages)
        {
            if (critter != null && completedStages > ChargenStages.CG_Stage_Class)
            {
                _hp.IsActive = true;
                _hp.Value = critter.GetStat(Stat.hp_max).ToString();
                _ac.IsActive = true;
                _ac.Value = critter.GetStat(Stat.ac).ToString();
            }
            else
            {
                _hp.IsActive = false;
                _ac.IsActive = false;
            }
        }

        [TempleDllLocation(0x1011d010)]
        private void UpdateSavingThrows(GameObjectBody critter, ChargenStages completedStages)
        {
            if (critter != null && completedStages >= ChargenStages.CG_Stage_Class)
            {
                _reflexSave.IsActive = true;
                _reflexSave.Value = critter.GetStat(Stat.save_reflexes).ToString();
                _fortitudeSave.IsActive = true;
                _fortitudeSave.Value = critter.GetStat(Stat.save_fortitude).ToString();
                _willSave.IsActive = true;
                _willSave.Value = critter.GetStat(Stat.save_willpower).ToString();
            }
            else
            {
                _fortitudeSave.IsActive = false;
                _reflexSave.IsActive = false;
                _willSave.IsActive = false;
            }
        }

        [TempleDllLocation(0x1011ca20)]
        private void UpdateInitiativeAndSpeed(GameObjectBody critter, ChargenStages completedStages)
        {
            if (critter != null && completedStages > ChargenStages.CG_Stage_Class)
            {
                _initiativeBonus.IsActive = true;
                _initiativeBonus.Value = critter.GetStat(Stat.initiative_bonus).ToString();
                _speed.IsActive = true;
                var movementSpeed = critter.Dispatch41GetMoveSpeed(out _);
                _speed.Value = ((int) movementSpeed).ToString();
            }
            else
            {
                _initiativeBonus.IsActive = false;
                _speed.IsActive = false;
            }
        }

        [TempleDllLocation(0x1011c6c0)]
        private void UpdateAttackBonus(GameObjectBody critter, ChargenStages completedStages)
        {
            if (critter != null && completedStages > ChargenStages.CG_Stage_Class)
            {
                _meleeAttackBonus.IsActive = true;
                var meleeBonus = critter.GetStat(Stat.melee_attack_bonus);
                _meleeAttackBonus.Value = $"{meleeBonus:+#;-#;0}";
                _rangedAttackBonus.IsActive = true;
                var rangedBonus = critter.GetStat(Stat.ranged_attack_bonus);
                _rangedAttackBonus.Value = $"{rangedBonus:+#;-#;0}";
            }
            else
            {
                _meleeAttackBonus.IsActive = false;
                _rangedAttackBonus.IsActive = false;
            }
        }
    }
}