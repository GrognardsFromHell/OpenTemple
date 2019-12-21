using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.CharSheet.Feats
{
    public class CharSheetFeatsUi : IDisposable
    {
        [TempleDllLocation(0x10d19db0)]
        public WidgetContainer Container { get; }

        private WidgetScrollBar _scrollbar;

        private readonly FeatButton[] _featButtons;

        private readonly List<FeatId> _featsToDisplay = new List<FeatId>();

        [TempleDllLocation(0x101bcbf0)]
        [TempleDllLocation(0x101bc910)]
        public CharSheetFeatsUi()
        {
            Container = new WidgetContainer(new Rectangle(256, 76, 398, 266));
            Container.SetMouseMsgHandler(ForwardScrollWheel);
            Container.Name = "char_feats_ui_main_window";
            Container.Visible = false;

            _scrollbar = new WidgetScrollBar(new Rectangle(384, 0, 13, 266));
            _scrollbar.SetValueChangeHandler(_ => UpdateButtons());
            Container.Add(_scrollbar);

            _featButtons = new FeatButton[20];
            for (var i = 0; i < _featButtons.Length; i++)
            {
                var button = new FeatButton(new Rectangle(1, 1 + 13 * i, 280, 13));
                button.Name = "char_feats_ui_feat_button" + i;
                button.SetMouseMsgHandler(ForwardScrollWheel);
                _featButtons[i] = button;
                Container.Add(button);
            }
        }

        [TempleDllLocation(0x101bbf90)]
        private bool ForwardScrollWheel(MessageMouseArgs msg)
        {
            // Forward the scroll wheel to the scrollbar
            if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
            {
                _scrollbar.HandleMouseMessage(msg);
            }

            return true;
        }

        private void UpdateButtons()
        {
            var offset = _scrollbar.GetValue();
            for (var i = 0; i < _featButtons.Length; i++)
            {
                var button = _featButtons[i];
                var featIdx = i + offset;
                if (featIdx < _featsToDisplay.Count)
                {
                    button.Visible = true;
                    button.Feat = _featsToDisplay[featIdx];
                }
                else
                {
                    button.Visible = false;
                }
            }
        }

        [TempleDllLocation(0x101bbd50)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101bbdb0)]
        public void Show(GameObjectBody critter)
        {
            // Build the list of feats to display
            _featsToDisplay.Clear();
            // NOTE: This will actually include duplicates if the same feat can be taken multiple times
            // which is what I personally find more suitable to this list
            _featsToDisplay.AddRange(GameSystems.Feat.FeatListGet(critter));
            _scrollbar.SetValue(0);
            _scrollbar.SetMax(Math.Max(0, _featsToDisplay.Count - _featButtons.Length));

            UpdateButtons();
            Container.Visible = true;
            Stub.TODO();
        }

        [TempleDllLocation(0x101bbe70)]
        public void Hide()
        {
            _featsToDisplay.Clear();
            Container.Visible = false;
            Stub.TODO();
        }

        public void Reset()
        {
        }
    }
}