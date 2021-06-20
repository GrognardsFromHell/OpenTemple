using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems.ClassFeatures
{
    internal class RangerFeaturesUi : IChargenSystem
    {
        private CharEditorSelectionPacket _pkt;

        public RangerFeaturesUi()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/abilities_ranger_ui.json");
            Container = doc.GetRootContainer();
            Container.Visible = false;
        }

        public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

        public WidgetContainer Container { get; }

        public string HelpTopic { get; }

        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            _pkt.feat3 = null;
        }

        public bool CheckComplete()
        {
            return _pkt.feat3.HasValue;
        }

        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            Trace.Assert(charSpec.feat3.HasValue);
            GameSystems.Feat.AddFeat(playerObj, charSpec.feat3.Value);
            GameSystems.D20.Status.D20StatusRefresh(playerObj);
        }
    }
}