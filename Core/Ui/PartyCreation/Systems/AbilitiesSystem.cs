using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.PartyCreation.Systems.ClassFeatures;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    /// <summary>
    /// Allows additional class-features to be selected for the class in which the character
    /// is leveling up (or during character creation, the first class).
    /// </summary>
    [TempleDllLocation(0x102f7a98)]
    internal class AbilitiesSystem : IChargenSystem
    {
        private readonly Dictionary<Stat, IChargenSystem> _featuresByClass = new Dictionary<Stat, IChargenSystem>
        {
            {Stat.level_cleric, new ClericFeaturesUi()},
            {Stat.level_wizard, new WizardFeaturesUi()},
            {Stat.level_ranger, new RangerFeaturesUi()}
        };

        private IChargenSystem _activeFeaturesUi;

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x10c3d0f0)]
        private bool uiChargenAbilitiesActivated;

        [TempleDllLocation(0x10186f90)]
        public AbilitiesSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/abilities_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            foreach (var featuresUi in _featuresByClass.Values)
            {
                Container.Add(featuresUi.Container);
            }
        }

        public string HelpTopic => "TAG_CHARGEN_ABILITIES";

        public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

        public WidgetContainer Container { get; }

        [TempleDllLocation(0x10184b90)]
        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            foreach (var featureUi in _featuresByClass.Values)
            {
                featureUi.Reset(pkt);
            }

            uiChargenAbilitiesActivated = false;
        }

        [TempleDllLocation(0x10185e10)]
        public void Activate()
        {
            uiChargenAbilitiesActivated = true;
        }

        [TempleDllLocation(0x10185670)]
        public void Show()
        {
            Container.Show();

            _featuresByClass.TryGetValue(_pkt.classCode, out _activeFeaturesUi);
            _activeFeaturesUi?.Show();
        }

        public void Hide()
        {
            if (_activeFeaturesUi != null)
            {
                _activeFeaturesUi.Container.Visible = false;
                _activeFeaturesUi = null;
            }

            Container.Hide();
        }

        [TempleDllLocation(0x10184bd0)]
        public bool CheckComplete()
        {
            // Note that this may also be called if the actual container is not visible,
            // which is why we need to always retrieve the correct class-handler from the map
            if (!_featuresByClass.TryGetValue(_pkt.classCode, out _activeFeaturesUi))
            {
                // Always complete if the class has no special features to select
                return true;
            }

            return _activeFeaturesUi.CheckComplete();
        }

        [TempleDllLocation(0x10184c80)]
        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            _activeFeaturesUi?.Finalize(charSpec, ref playerObj);
        }

        [TempleDllLocation(0x10184c40)]
        public void UpdateDescriptionBox()
        {
            if (_pkt.classCode == Stat.level_wizard)
            {
                UiSystems.PCCreation.ShowHelpTopic("TAG_HMU_CHAR_EDITOR_WIZARD_SPEC");
            }
            else if (_pkt.classCode == Stat.level_cleric)
            {
                UiSystems.PCCreation.ShowHelpTopic("");
            }
        }

        public bool CompleteForTesting(Dictionary<string, object> props)
        {
            // Note that this may also be called if the actual container is not visible
            if (!_featuresByClass.TryGetValue(_pkt.classCode, out _activeFeaturesUi))
            {
                // Always complete if the class has no special features to select
                return true;
            }

            return _activeFeaturesUi.CompleteForTesting(props);
        }
    }
}