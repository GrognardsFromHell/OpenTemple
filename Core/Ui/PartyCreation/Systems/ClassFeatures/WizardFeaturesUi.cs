using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems.ClassFeatures
{
    internal class WizardFeaturesUi : IChargenSystem
    {
        private CharEditorSelectionPacket _pkt;

        public WizardFeaturesUi()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/abilities_wizard_ui.json");
            Container = doc.GetRootContainer();
            Container.Visible = false;
        }

        public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

        public string HelpTopic { get; }

        public WidgetContainer Container { get; }

        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            _pkt.wizSchool = 0;
            _pkt.forbiddenSchool1 = 0;
            _pkt.forbiddenSchool2 = 0;
        }

        public bool CheckComplete()
        {
            if (_pkt.wizSchool == SchoolOfMagic.None)
            {
                return true;
            }

            if (_pkt.forbiddenSchool1 != SchoolOfMagic.None)
            {
                // Divination only needs to pick a single forbidden school
                return _pkt.wizSchool == SchoolOfMagic.Divination
                       || _pkt.forbiddenSchool2 != SchoolOfMagic.None;
            }

            return false;
        }

        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            GameSystems.Spell.SetSchoolSpecialization(
                playerObj,
                charSpec.wizSchool,
                charSpec.forbiddenSchool1,
                charSpec.forbiddenSchool2
            );
        }
    }
}