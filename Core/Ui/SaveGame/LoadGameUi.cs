using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO.Images;
using SpicyTemple.Core.IO.SaveGames;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.MainMenu;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.SaveGame
{
    public class LoadGameUi : IDisposable, IViewportAwareUi
    {
        [TempleDllLocation(0x10176b00)]
        public bool IsVisible => _window.IsVisible();

        [TempleDllLocation(0x10c07ca0)]
        private WidgetContainer _window;

        [TempleDllLocation(0x10c0a468)]
        private bool _openedFromMainMenu;

        [TempleDllLocation(0x10c07d28)]
        private readonly WidgetScrollBar _scrollBar;

        private readonly WidgetButton _loadButton;

        private readonly WidgetButton _deleteButton;

        private readonly WidgetImage _largeScreenshot;

        [TempleDllLocation(0x10c0a49c)]
        private bool _confirmingDeletion;

        [TempleDllLocation(0x10c0a44c)]
        private List<SaveGameInfo> _saves = new List<SaveGameInfo>();

        private SaveGameInfo _selectedSave;

        private SaveGameSlotButton[] _slots = new SaveGameSlotButton[8];

        [TempleDllLocation(0x10177ed0)]
        public LoadGameUi()
        {
            var doc = WidgetDoc.Load("ui/load_game.json");

            _window = doc.TakeRootContainer();
            _window.SetVisible(false);

            _loadButton = doc.GetButton("load");
            _loadButton.SetClickHandler(OnLoadClick);
            _deleteButton = doc.GetButton("delete");
            _deleteButton.SetClickHandler(OnDeleteClick);
            doc.GetButton("close").SetClickHandler(OnCloseClick);

            _scrollBar = doc.GetScrollBar("scrollbar");
            _scrollBar.SetValueChangeHandler(value => UpdateSlots());

            _largeScreenshot = doc.GetImageContent("largeScreenshot");
            _largeScreenshot.Visible = false;

            // Forward mouse events on the window to the savegame list
            _window.SetKeyStateChangeHandler(OnKeyPress);

            for (var i = 0; i < _slots.Length; i++)
            {
                // Size is determined by the border image
                var rect = new Rectangle(26, 34 + 53 * i, 318, 54);

                var slot = new SaveGameSlotButton(rect);
                slot.SetClickHandler(() =>
                {
                    _selectedSave = slot.SaveGame;
                    UpdateSlots();
                });
                slot.SetMouseMsgHandler(ForwardScrollWheelMessage);
                _slots[i] = slot;
                _window.Add(slot);
            }
        }

        private bool ForwardScrollWheelMessage(MessageMouseArgs args)
        {
            if ((args.flags & MouseEventFlag.ScrollWheelChange) != 0)
            {
                return _scrollBar.HandleMouseMessage(args);
            }

            return false;
        }

        private bool OnKeyPress(MessageKeyStateChangeArgs arg)
        {
            if (arg.down)
            {
                return false;
            }

            switch (arg.key)
            {
                case DIK.DIK_ESCAPE:
                    OnCloseClick();
                    return true;
                case DIK.DIK_RETURN:
                    OnLoadClick();
                    return true;
                case DIK.DIK_DELETE:
                    OnDeleteClick();
                    return true;
                case DIK.DIK_UP:
                case DIK.DIK_NUMPAD8:
                    SelectPreviousSave();
                    return true;
                case DIK.DIK_DOWN:
                case DIK.DIK_NUMPAD2:
                    SelectNextSave();
                    return true;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x10177530)]
        private void SelectPreviousSave()
        {
            if (_selectedSave == null)
            {
                if (_saves.Count != 0)
                {
                    _selectedSave = _saves[^1];
                }
            }
            else
            {
                var currentIndex = _saves.IndexOf(_selectedSave);
                if (currentIndex != -1)
                {
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = _saves.Count - 1;
                    }

                    _selectedSave = _saves[currentIndex];
                }
            }

            ScrollIntoView(_selectedSave);
            UpdateSlots();
        }

        [TempleDllLocation(0x10177530)]
        private void SelectNextSave()
        {
            if (_selectedSave == null)
            {
                if (_saves.Count != 0)
                {
                    _selectedSave = _saves[0];
                }
            }
            else
            {
                var currentIndex = _saves.IndexOf(_selectedSave);
                if (currentIndex != -1)
                {
                    _selectedSave = _saves[(currentIndex + 1) % _saves.Count];
                }
            }

            ScrollIntoView(_selectedSave);
            UpdateSlots();
        }

        private void ScrollIntoView(SaveGameInfo save)
        {
            var index = _saves.IndexOf(save);
            if (index != -1)
            {
                var firstVisible = _scrollBar.GetValue();
                var lastVisible = firstVisible + 7;

                if (index < firstVisible)
                {
                    _scrollBar.SetValue(_scrollBar.GetValue() - (firstVisible - index));
                }
                else if (index > lastVisible)
                {
                    _scrollBar.SetValue(_scrollBar.GetValue() + (index - lastVisible));
                }
            }
        }

        [TempleDllLocation(0x101773f0)]
        private void OnLoadClick()
        {
            if (!_confirmingDeletion && _selectedSave != null)
            {
                UiSystems.MainMenu.LoadGame(_selectedSave);
            }
        }

        [TempleDllLocation(0x10177490)]
        private void OnDeleteClick()
        {
            if (!_confirmingDeletion && _selectedSave != null)
            {
                _confirmingDeletion = true;
                var saveToDelete = _selectedSave;
                UiSystems.Popup.ConfirmBox("#{loadgame:11}", "#{loadgame:10}", true, btn =>
                {
                    if (btn == 0)
                    {
                        var currentIndex = _saves.IndexOf(saveToDelete);
                        if (Globals.GameLib.DeleteSave(saveToDelete))
                        {
                            LoadSaveList(currentIndex - 1);
                        }
                    }

                    _confirmingDeletion = false;
                });
            }
        }

        [TempleDllLocation(0x101774d0)]
        private void OnCloseClick()
        {
            Hide();

            if (_openedFromMainMenu)
            {
                UiSystems.MainMenu.Show(MainMenuPage.MainMenu);
            }
        }

        [TempleDllLocation(0x101772b0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10178130)]
        public void ResizeViewport(Size size)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101772e0)]
        public void Show(bool fromMainMenu)
        {
            if (!IsVisible)
            {
                GameSystems.TimeEvent.PushDisableFidget();
            }

            _window.SetVisible(true);
            _window.CenterOnScreen();

            LoadSaveList();

            _window.SetVisible(true);
            _window.BringToFront();
            _openedFromMainMenu = fromMainMenu;
            UiSystems.UtilityBar.Hide();
        }

        private void LoadSaveList(int selectIndex = 0)
        {
            _confirmingDeletion = false;
            _saves = Globals.GameLib.GetSaveGames();
            _saves.Sort(SaveGameOrder.LastModifiedAutoFirst);
            _scrollBar.SetMax(Math.Max(0, _saves.Count - 8));

            if (selectIndex >= 0 && selectIndex < _saves.Count)
            {
                _selectedSave = _saves[selectIndex];
            }
            else
            {
                _selectedSave = null;
            }

            ScrollIntoView(_selectedSave);
            UpdateSlots();
        }

        [TempleDllLocation(0x101773a0)]
        public void Hide()
        {
            if (IsVisible)
            {
                GameSystems.TimeEvent.PopDisableFidget();
            }

            _saves.Clear();
            _selectedSave = null;
            _window.SetVisible(false);
            if (!_openedFromMainMenu)
            {
                UiSystems.UtilityBar.Show();
            }
        }

        private void UpdateSlots()
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = _slots[i];
                var actualIndex = i + _scrollBar.GetValue();

                if (actualIndex < _saves.Count)
                {
                    var save = _saves[actualIndex];
                    slot.SetSaveInfo(save);
                    slot.Selected = _selectedSave == save;
                }
                else
                {
                    slot.ClearSaveInfo();
                    slot.Selected = false;
                }
            }

            // Show save game details
            ShowSaveDetails(_selectedSave);

            // Disable/Enable load and delete buttons.
            if (_selectedSave == null
                || _selectedSave.Type == SaveGameType.AutoSave
                || _selectedSave.Type == SaveGameType.QuickSave)
            {
                _deleteButton.SetDisabled(true);
            }
            else
            {
                _deleteButton.SetDisabled(false);
            }

            _loadButton.SetDisabled(_selectedSave == null);
        }

        private void ShowSaveDetails(SaveGameInfo save)
        {
            if (save?.LargeScreenshotPath != null)
            {
                var data = File.ReadAllBytes(save.LargeScreenshotPath);
                var decodedImage = ImageIO.DecodeImage(data);
                using var texture = Tig.RenderingDevice.CreateDynamicTexture(BufferFormat.X8R8G8B8,
                    decodedImage.info.width,
                    decodedImage.info.height);
                texture.Resource.UpdateRaw(decodedImage.data, decodedImage.info.width * 4);

                _largeScreenshot.SetTexture(texture.Resource);
                _largeScreenshot.Visible = true;
            }
            else
            {
                _largeScreenshot.Visible = false;
            }
        }
    }
}