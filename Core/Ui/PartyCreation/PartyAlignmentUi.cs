using System;
using System.Collections.Generic;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.PartyCreation
{
    public class PartyAlignmentUi : IDisposable
    {
        private Alignment? _alignment;

        [TempleDllLocation(0x10bda764)]
        private WidgetContainer _container;

        [TempleDllLocation(0x10bda73c)]
        private Dictionary<Alignment, WidgetButton> _alignmentButtons;

        [TempleDllLocation(0x10bdb920)]
        private WidgetButton _okButton;

        private WidgetImage _selectionRect;

        public event Action<Alignment> OnConfirm;

        public event Action OnCancel;

        [TempleDllLocation(0x1011f2c0)]
        public PartyAlignmentUi()
        {
            var doc = WidgetDoc.Load("ui/party_creation/party_alignment.json");

            _container = doc.TakeRootContainer();
            _container.SetVisible(false);

            // RENDER: 0x1011be20
            // MESSAGE: 0x1011ed20
            _container.SetKeyStateChangeHandler(evt =>
            {
                if (evt.key == DIK.DIK_ESCAPE && evt.down)
                {
                    Cancel();
                    return true;
                }

                return false;
            });

            // Alignment buttons:
            // MESSAGE: 0x1011e5c0
            // RENDER: 0x1011e460
            _alignmentButtons = new Dictionary<Alignment, WidgetButton>
            {
                {Alignment.TRUE_NEUTRAL, doc.GetButton("alignment_tn")},
                {Alignment.LAWFUL_NEUTRAL, doc.GetButton("alignment_ln")},
                {Alignment.CHAOTIC_NEUTRAL, doc.GetButton("alignment_cn")},
                {Alignment.NEUTRAL_GOOD, doc.GetButton("alignment_ng")},
                {Alignment.LAWFUL_GOOD, doc.GetButton("alignment_lg")},
                {Alignment.CHAOTIC_GOOD, doc.GetButton("alignment_cg")},
                {Alignment.NEUTRAL_EVIL, doc.GetButton("alignment_ne")},
                {Alignment.LAWFUL_EVIL, doc.GetButton("alignment_le")},
                {Alignment.CHAOTIC_EVIL, doc.GetButton("alignment_ce")}
            };
            foreach (var (alignment, button) in _alignmentButtons)
            {
                var alignmentName = GameSystems.Stat.GetAlignmentName(alignment).ToUpper();
                button.SetText(alignmentName);

                button.SetClickHandler(() => SelectAlignment(alignment));
            }

            // OK Button:
            // MESSAGE: 0x1011bf70
            // RENDER: 0x1011beb0
            _okButton = doc.GetButton("ok");
            _okButton.SetClickHandler(() =>
            {
                var alignment = _alignment;
                if (alignment.HasValue)
                {
                    Hide();
                    OnConfirm?.Invoke(alignment.Value);
                }
            });

            // Cancel button: 0x10bdd614
            // MESSAGE: 0x1011ed50
            // RENDER: 0x1011bfa0
            var cancelButton = doc.GetButton("cancel");
            cancelButton.SetClickHandler(Cancel);

            _selectionRect = doc.GetImageContent("selected");
        }

        [TempleDllLocation(0x1011e620)]
        private void Cancel()
        {
            OnCancel?.Invoke();
        }

        [TempleDllLocation(0x1011e5c0)]
        private void SelectAlignment(Alignment alignment)
        {
            _alignment = alignment;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (!_alignment.HasValue)
            {
                _selectionRect.Visible = false;
                _okButton.SetDisabled(true);
            }
            else
            {
                _selectionRect.Visible = true;
                _okButton.SetDisabled(false);

                // Center the rectangle on the button that is the selected alignment
                var button = _alignmentButtons[_alignment.Value];
                var x = button.X + button.Width / 2 - _selectionRect.FixedSize.Width / 2;
                var y = button.Y + button.Height / 2 - _selectionRect.FixedSize.Height / 2;
                _selectionRect.SetX(x);
                _selectionRect.SetY(y);
            }

            // Mark any buttons as active that have one of the alignment axes in common with the selected alignment
            foreach (var (alignment, button) in _alignmentButtons)
            {
                if (_alignment.HasValue
                    && GameSystems.Stat.AlignmentsUnopposed(_alignment.Value, alignment))
                {
                    button.SetActive(true);
                }
                else
                {
                    button.SetActive(false);
                }
            }
        }

        [TempleDllLocation(0x1011bdc0)]
        public void Dispose()
        {
            _container?.Dispose();
            _container = null;
        }

        public void Reset()
        {
            foreach (var button in _alignmentButtons.Values)
            {
                button.SetActive(false);
            }

            _alignment = null;
        }

        public void Show()
        {
            _container.CenterOnScreen();
            _container.SetVisible(true);
            _container.BringToFront();

//            dword_10BDC430/*0x10bdc430*/ = (string )uiPcCreationText_SelectAPartyAlignment/*0x10bdb018*/;

            UpdateSelection();
        }

        public void Hide()
        {
            _container.SetVisible(false);
        }
    }
}