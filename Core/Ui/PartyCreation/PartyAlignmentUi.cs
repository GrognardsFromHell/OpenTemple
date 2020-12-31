using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;
using Button = OpenTemple.Widgets.Button;

namespace OpenTemple.Core.Ui.PartyCreation
{
    public class PartyAlignmentUi : IDisposable
    {
        private Alignment? _alignment;

        [TempleDllLocation(0x10bda73c)]
        private Dictionary<Alignment, Button> _alignmentButtons;

        [TempleDllLocation(0x10bdb920)]
        private Button _okButton;

        private Image _selectionRect;

        public event Action<Alignment> OnConfirm;

        public event Action OnCancel;

        [TempleDllLocation(0x10bda764)]
        private PartyAlignmentDialog _dialog;

        [TempleDllLocation(0x1011f2c0)]
        public PartyAlignmentUi()
        {
            _dialog = new();
            Tig.MainWindow.AddOverlay(_dialog);
            _dialog.IsVisible = false;

            // RENDER: 0x1011be20
            // MESSAGE: 0x1011ed20
            _dialog.KeyDown += (_, e) => {
                if (e.Key == Key.Escape)
                {
                    Cancel();
                }
            };

            // Alignment buttons:
            // MESSAGE: 0x1011e5c0
            // RENDER: 0x1011e460
            Button GetButton(string name) => _dialog.FindControl<Button>(name);

            _alignmentButtons = new Dictionary<Alignment, Button>
            {
                {Alignment.TRUE_NEUTRAL, GetButton("alignment_tn")},
                {Alignment.LAWFUL_NEUTRAL, GetButton("alignment_ln")},
                {Alignment.CHAOTIC_NEUTRAL, GetButton("alignment_cn")},
                {Alignment.NEUTRAL_GOOD, GetButton("alignment_ng")},
                {Alignment.LAWFUL_GOOD, GetButton("alignment_lg")},
                {Alignment.CHAOTIC_GOOD, GetButton("alignment_cg")},
                {Alignment.NEUTRAL_EVIL, GetButton("alignment_ne")},
                {Alignment.LAWFUL_EVIL, GetButton("alignment_le")},
                {Alignment.CHAOTIC_EVIL, GetButton("alignment_ce")}
            };
            foreach (var (alignment, button) in _alignmentButtons)
            {
                var alignmentName = GameSystems.Stat.GetAlignmentName(alignment).ToUpper();
                button.Content = alignmentName;

                button.Click += (_, _) => SelectAlignment(alignment);
            }

            // OK Button:
            // MESSAGE: 0x1011bf70
            // RENDER: 0x1011beb0
            _okButton = _dialog.FindControl<Button>("ok");
            _okButton.Click += (_, _) =>
            {
                var alignment = _alignment;
                if (alignment.HasValue)
                {
                    Hide();
                    OnConfirm?.Invoke(alignment.Value);
                }
            };

            // Cancel button: 0x10bdd614
            // MESSAGE: 0x1011ed50
            // RENDER: 0x1011bfa0
            var cancelButton = _dialog.FindControl<Button>("cancel");
            cancelButton.Click += (_, _) => Cancel();

            _selectionRect = _dialog.FindControl<Image>("selected");
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
                _selectionRect.IsVisible = false;
                _okButton.IsEnabled = false;
            }
            else
            {
                _selectionRect.IsVisible = true;
                _okButton.IsEnabled = true;
                // Center the rectangle on the button that is the selected alignment
                var button = _alignmentButtons[_alignment.Value];
                var x = Canvas.GetLeft(button) + button.Width / 2 - _selectionRect.Source.Size.Width / 2;
                var y = Canvas.GetTop(button) + button.Height / 2 - _selectionRect.Source.Size.Height / 2;
                Canvas.SetLeft(_selectionRect, x);
                Canvas.SetTop(_selectionRect, y);
            }

            // Mark any buttons as active that have one of the alignment axes in common with the selected alignment
            foreach (var (alignment, button) in _alignmentButtons)
            {
                if (_alignment.HasValue
                    && GameSystems.Stat.AlignmentsUnopposed(_alignment.Value, alignment))
                {
                    // TODO button.SetActive(true);
                }
                else
                {
                    // TODO button.SetActive(false);
                }
            }
        }

        [TempleDllLocation(0x1011bdc0)]
        public void Dispose()
        {
            Tig.MainWindow.RemoveOverlay(_dialog);
        }

        public void Reset()
        {
            foreach (var button in _alignmentButtons.Values)
            {
                // TODO button.SetActive(false);
            }

            _alignment = null;
        }

        public void Show()
        {
            _dialog.IsVisible = true;
            // TODO: DialogManager / ZIndex for _dialog

//            dword_10BDC430/*0x10bdc430*/ = (string )uiPcCreationText_SelectAPartyAlignment/*0x10bdb018*/;

            UpdateSelection();
        }

        public void Hide()
        {
            _dialog.IsVisible = false;
        }
    }
}
