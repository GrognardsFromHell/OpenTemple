using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f7a6c)]
    internal class DeitySystem : IChargenSystem
    {
        public string HelpTopic => "TAG_CHARGEN_DEITY";

        public ChargenStages Stage => ChargenStages.CG_Stage_Deity;

        public WidgetContainer Container { get; }

        private readonly Dictionary<DeityId, WidgetButton> _deityButtons;

        [TempleDllLocation(0x10187690)]
        public DeitySystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/deity_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            _deityButtons = new Dictionary<DeityId, WidgetButton>();
            foreach (var deityId in GameSystems.Deity.PlayerSelectableDeities)
            {
                var buttonIndex = _deityButtons.Count;
                var button = new WidgetButton(new Rectangle(
                    48 + 171 * (buttonIndex / 10),
                    25 + 21 * (buttonIndex % 10),
                    162,
                    12
                ));
                button.SetStyle("deity-button");
                button.SetText(GameSystems.Deity.GetName(deityId).ToUpper());
                button.OnMouseEnter += msg => ShowDeityHelp(deityId);
                button.OnMouseExit += msg => UpdateDescriptionBox();
                button.SetClickHandler(() => SelectDeity(deityId));
                Container.Add(button);
                _deityButtons[deityId] = button;
            }
        }

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x10187050)]
        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            _pkt.deityId = null;
            _pkt.alignmentChoice = 0;
        }

        [TempleDllLocation(0x10187390)]
        public void Activate()
        {
            UpdateDescriptionBox();
            UpdateButtonStates();
        }

        [TempleDllLocation(0x101870b0)]
        public bool CheckComplete()
        {
            return _pkt.deityId.HasValue;
        }

        [TempleDllLocation(0x101870c0)]
        public void Finalize(CharEditorSelectionPacket pkt, ref GameObjectBody playerObj)
        {
            Trace.Assert(pkt.deityId.HasValue);
            playerObj.SetInt32(obj_f.critter_deity, (int) pkt.deityId.Value);
        }

        private void SelectDeity(DeityId deityId)
        {
            _pkt.deityId = deityId;
            _pkt.alignmentChoice = GameSystems.Deity.GetAlignmentChoice(deityId, _pkt.alignment.GetValueOrDefault());
            UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Deity);
            UpdateDescriptionBox();
            UpdateButtonStates();
        }

        [TempleDllLocation(0x10187340)]
        private void UpdateButtonStates()
        {
            foreach (var (deityId, button) in _deityButtons)
            {
                if (GameSystems.Deity.CanSelect(UiSystems.PCCreation.EditedChar, deityId))
                {
                    button.SetDisabled(false);
                    button.SetActive(_pkt.deityId == deityId);
                }
                else
                {
                    button.SetDisabled(true);
                    button.SetActive(false);
                }
            }
        }

        [TempleDllLocation(0x101870f0)]
        private void UpdateDescriptionBox()
        {
            if (_pkt.deityId.HasValue)
            {
                ShowDeityHelp(_pkt.deityId.Value);
            }
            else
            {
                UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
            }
        }

        private void ShowDeityHelp(DeityId deityId)
        {
            var topic = GameSystems.Deity.GetHelpTopic(deityId);
            UiSystems.PCCreation.ShowHelpTopic(topic);
        }

        public bool CompleteForTesting(Dictionary<string, object> props)
        {
            var candidates = new List<DeityId>();
            foreach (var (deityId, button) in _deityButtons)
            {
                if (!button.IsDisabled())
                {
                    candidates.Add(deityId);
                }
            }

            SelectDeity(GameSystems.Random.PickRandom(candidates));
            return true;
        }
    }
}