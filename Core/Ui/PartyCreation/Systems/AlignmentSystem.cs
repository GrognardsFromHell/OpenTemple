using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.D20.Classes.Prereq;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f7a40)]
    internal class AlignmentSystem : IChargenSystem
    {
        public string HelpTopic => "TAG_CHARGEN_ALIGNMENT";

        public ChargenStages Stage => ChargenStages.CG_Stage_Alignment;

        public WidgetContainer Container { get; }

        private readonly WidgetText _partyAlignmentLabel;

        private readonly Dictionary<Alignment, WidgetButton> _alignmentButtons;

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x10187f30)]
        public AlignmentSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/alignment_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            _partyAlignmentLabel = doc.GetTextContent("partyAlignmentLabel");

            _alignmentButtons = new Dictionary<Alignment, WidgetButton>
            {
                {Alignment.LAWFUL_GOOD, doc.GetButton("lawfulGood")},
                {Alignment.NEUTRAL_GOOD, doc.GetButton("neutralGood")},
                {Alignment.CHAOTIC_GOOD, doc.GetButton("chaoticGood")},
                {Alignment.LAWFUL_NEUTRAL, doc.GetButton("lawfulNeutral")},
                {Alignment.NEUTRAL, doc.GetButton("neutral")},
                {Alignment.CHAOTIC_NEUTRAL, doc.GetButton("chaoticNeutral")},
                {Alignment.LAWFUL_EVIL, doc.GetButton("lawfulEvil")},
                {Alignment.NEUTRAL_EVIL, doc.GetButton("neutralEvil")},
                {Alignment.CHAOTIC_EVIL, doc.GetButton("chaoticEvil")},
            };

            foreach (var (alignment, button) in _alignmentButtons)
            {
                button.OnMouseEnter += msg => ShowAlignmentHelp(alignment);
                button.OnMouseExit += msg => UpdateDescriptionBox();
                button.SetClickHandler(() => SelectAlignment(alignment));
            }
        }

        [TempleDllLocation(0x10187810)]
        public void Reset(CharEditorSelectionPacket selPkt)
        {
            _pkt = selPkt;
            selPkt.alignment = null;
        }

        [TempleDllLocation(0x10187c20)]
        [TempleDllLocation(0x10187b10)]
        public void Activate()
        {
            var partyAlignment = GameSystems.Party.PartyAlignment;
            _partyAlignmentLabel.SetText("#{pc_creation:16000} @1" + GameSystems.Stat.GetAlignmentName(partyAlignment));

            foreach (var (alignment, button ) in _alignmentButtons)
            {
                // TODO: We should show a tooltip explaining WHY a certain alignment is unavailable
                var compatibleWithParty = IsCompatibleAlignment(partyAlignment, alignment);
                var compatibleWithClass = D20ClassSystem.IsCompatibleAlignment(_pkt.classCode, alignment);
                button.SetDisabled(!compatibleWithParty || !compatibleWithClass);
            }

            UpdateSelection();
        }

        private void SelectAlignment(Alignment alignment)
        {
            _pkt.alignment = alignment;
            UpdateSelection();
            UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Alignment);
        }

        private void UpdateSelection()
        {
            foreach (var (alignment, button ) in _alignmentButtons)
            {
                button.SetActive(_pkt.alignment == alignment);
            }
        }

        [TempleDllLocation(0x1011b880)]
        private bool IsCompatibleAlignment(Alignment partyAlignment, Alignment playerAlignment)
        {
            if (Globals.Config.laxRules && Globals.Config.disableAlignmentRestrictions)
            {
                return true;
            }

            return GameSystems.Stat.AlignmentsUnopposed(partyAlignment, playerAlignment);
        }

        [TempleDllLocation(0x10187860)]
        public bool CheckComplete()
        {
            return _pkt.alignment.HasValue;
        }

        [TempleDllLocation(0x10187870)]
        public void Finalize(CharEditorSelectionPacket selPkt, ref GameObjectBody playerObj)
        {
            Trace.Assert(selPkt.alignment.HasValue);
            playerObj.SetInt32(obj_f.critter_alignment, (int) selPkt.alignment.Value);
        }

        private void ShowAlignmentHelp(Alignment alignment)
        {
            var text = GameSystems.Stat.GetAlignmentShortDesc(alignment);
            UiSystems.PCCreation.ShowHelpText(text);
        }

        [TempleDllLocation(0x101878a0)]
        private void UpdateDescriptionBox()
        {
            if (_pkt.alignment.HasValue)
            {
                ShowAlignmentHelp(_pkt.alignment.Value);
            }
            else
            {
                UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
            }
        }

        public bool CompleteForTesting(Dictionary<string, object> props)
        {
            Activate(); // Update the buttons

            foreach (var (alignment, button) in _alignmentButtons)
            {
                if (!button.IsDisabled())
                {
                    SelectAlignment(alignment);
                    return true;
                }
            }

            return false;
        }
    }
}