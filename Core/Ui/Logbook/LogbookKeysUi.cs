using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.SaveGames.UiState;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Startup;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.Logbook
{
    public class LogbookKeysUi : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private const PredefinedFont CaptionFont = PredefinedFont.ARIAL_12;

        private static readonly TigTextStyle CaptionTextStyle =
            new TigTextStyle(new ColorRect(PackedLinearColorA.Black))
            {
                flags = TigTextStyleFlag.TTSF_TRUNCATE,
                kerning = 1,
                tracking = 3
            };

        [TempleDllLocation(0x102fede0)]
        private bool _showPopupForNewKey = true;

        [TempleDllLocation(0x10c4c698)]
        private readonly Dictionary<int, KeylogEntry> _keys = new Dictionary<int, KeylogEntry>();

        private WidgetContainer _container;

        [TempleDllLocation(0x10c4c4c8)]
        private WidgetScrollBar _scrollbar;

        [TempleDllLocation(0x10c4c680)]
        private readonly LogbookKeyButton[] _rows = new LogbookKeyButton[10];

        private readonly LogbookKeyTranslations _translations = new LogbookKeyTranslations();

        private LogbookKeyDetailsUi _details;

        private LogbookKeyAcquiredPopup _keyAcquiredPopup;

        [TempleDllLocation(0x10198a90)]
        public LogbookKeysUi()
        {
            LoadKeys();

            CreateWidgets();

            _keyAcquiredPopup = new LogbookKeyAcquiredPopup(_translations);
            _keyAcquiredPopup.OnChangeNotificationSetting += enabled =>
            {
                Logger.Info("Changing notification setting for key-acquired popup to: {0}", enabled);
                _showPopupForNewKey = enabled;
            };
        }

        public WidgetContainer Container => _container;

        [TempleDllLocation(0x10198680)]
        private void CreateWidgets()
        {
            // Created @ 0x10198765
            _container = new WidgetContainer(new Rectangle(64, 68, 648, 333));
            _container.ZIndex = 100051;
            _container.Name = "logbook_ui_keys_acquired_keys_window";
            _container.Visible = false;

            // Container for the scrollable list on the left
            var listContainer = new WidgetContainer(new Rectangle(0, 20, 283, 313));
            _container.Add(listContainer);

            _scrollbar = new WidgetScrollBar(new Rectangle(269, 0, 13, 314));
            _scrollbar.SetValueChangeHandler(_ => Update());
            listContainer.Add(_scrollbar);

            for (var i = 0; i < 10; i++)
            {
                var row = new LogbookKeyButton(_translations, new Rectangle(2, 3 + 31 * i, 267, 30));
                row.SetClickHandler(() =>
                {
                    foreach (var otherRow in _rows)
                    {
                        otherRow.IsSelected = false;
                    }

                    var key = row.Key;
                    if (key != null)
                    {
                        _details.Key = key;
                        row.IsSelected = true;
                    }
                    else
                    {
                        _details.Key = null;
                    }
                });
                row.Name = "logbook_ui_keys_acquired_keys_butn" + i;
                _rows[i] = row;
                listContainer.Add(row);
            }

            _details = new LogbookKeyDetailsUi(_translations);
            _container.Add(_details.Container);

            // Add caption and outline for the list container
            var listCaption = new WidgetLegacyText("  " + _translations.ListCaption, CaptionFont, CaptionTextStyle);
            listCaption.SetX(listContainer.X - 3);
            listCaption.SetY(listContainer.Y - listCaption.GetPreferredSize().Height - 8);
            _container.AddContent(listCaption);

            var listOutline = new WidgetRectangle();
            listOutline.SetX(listContainer.X);
            listOutline.SetY(listContainer.Y);
            listOutline.FixedSize = listContainer.GetSize();
            listOutline.Pen = new PackedLinearColorA(0xFF909090);
            Container.AddContent(listOutline);

            // Add caption and outline for the details widget
            var detailsCaption =
                new WidgetLegacyText("  " + _translations.DetailCaption, CaptionFont, CaptionTextStyle);
            detailsCaption.SetX(_details.Container.X - 3);
            detailsCaption.SetY(_details.Container.Y - detailsCaption.GetPreferredSize().Height - 8);
            _container.AddContent(detailsCaption);

            var detailsOutline = new WidgetRectangle();
            detailsOutline.SetX(_details.Container.X);
            detailsOutline.SetY(_details.Container.Y);
            detailsOutline.FixedSize = _details.Container.GetSize();
            detailsOutline.Pen = new PackedLinearColorA(0xFF909090);
            Container.AddContent(detailsOutline);
        }

        [TempleDllLocation(0x10197d40)]
        private void LoadKeys()
        {
            var keyLines = Tig.FS.ReadMesFile("mes/gamekeylog.mes");

            for (var i = 0; i < 100; i++)
            {
                if (!keyLines.ContainsKey(i))
                {
                    continue;
                }

                _keys[i] = new KeylogEntry
                {
                    KeyId = i,
                    Title = keyLines[i],
                    Description = keyLines[100 + i]
                };
            }
        }

        [TempleDllLocation(0x10198290)]
        public void MarkKeyUsed(int keyId, TimePoint timeUsed)
        {
            if (_keys.TryGetValue(keyId, out var key))
            {
                Logger.Info("Marking key {0} as used.", keyId);
                key.Used = timeUsed;
            }
        }

        [TempleDllLocation(0x101982c0)]
        public bool IsKeyAcquired(int keyId)
        {
            if (!_keys.TryGetValue(keyId, out var key))
            {
                return false;
            }

            return key.IsAcquired;
        }

        [TempleDllLocation(0x10198230)]
        public void KeyAcquired(int keyId, TimePoint timePoint)
        {
            if (!_keys.ContainsKey(keyId))
            {
                Logger.Error("Unknown key id found: {0}", keyId);
                return;
            }

            if (!_keys[keyId].IsAcquired && _showPopupForNewKey)
            {
                _keyAcquiredPopup.Show();
            }

            _keys[keyId].Acquired = timePoint;
            UiSystems.UtilityBar.PulseLogbookButton();
        }

        [TempleDllLocation(0x10198380)]
        public void Update()
        {
            var acquiredKeys = _keys.Values.Where(k => k.IsAcquired).ToList();

            // set the scrollbar so it only scrolls for keys beyond 10
            _scrollbar.SetMax(Math.Max(0, acquiredKeys.Count - 10));

            // Set the shown key for each row
            var offset = _scrollbar.GetValue();
            var index = 0;
            foreach (var key in acquiredKeys.Skip(offset).Take(_rows.Length))
            {
                _rows[index++].Key = key;
            }

            // Reset the remaining rows
            for (; index < _rows.Length; index++)
            {
                _rows[index].Key = null;
            }
        }

        public void Show()
        {
            Update();

            // Show details for first key by default
            if (_details.Key == null && _rows[0].Key != null)
            {
                _details.Key = _rows[0].Key;
            }

            _container.Visible = true;
        }

        [TempleDllLocation(0x10195480)]
        public void Hide()
        {
            _container.Visible = false;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10195110)]
        public void Reset()
        {
            foreach (var keylogEntry in _keys.Values)
            {
                keylogEntry.Acquired = default;
                keylogEntry.Used = default;
            }
        }

        [TempleDllLocation(0x101952c0)]
        public SavedLogbookKeysUiState Save()
        {
            var result = new SavedLogbookKeysUiState();
            result.EnableKeyNotifications = _showPopupForNewKey;
            foreach (var (keyId, key) in _keys)
            {
                result.Keys[keyId] = new SavedKeyState(key.Acquired, key.Used);
            }

            return result;
        }

        [TempleDllLocation(0x10195360)]
        public void Load(SavedLogbookKeysUiState savedState)
        {
            _showPopupForNewKey = savedState.EnableKeyNotifications;

            foreach (var keylogEntry in _keys.Values)
            {
                keylogEntry.Acquired = default;
                keylogEntry.Used = default;

                if (savedState.Keys.TryGetValue(keylogEntry.KeyId, out var savedKeyState))
                {
                    keylogEntry.Acquired = savedKeyState.Acquired;
                    keylogEntry.Used = savedKeyState.Used;
                }
            }
        }
    }

    internal class KeylogEntry
    {
        public int KeyId { get; set; }
        public TimePoint Acquired { get; set; }
        public TimePoint Used { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsAcquired => Acquired != default;
    }
}