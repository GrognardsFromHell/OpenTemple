using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.SaveGame
{
    public class SaveGameUi : IDisposable, IViewportAwareUi
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10176b00)]
        public bool IsVisible => _window.Visible;

        [TempleDllLocation(0x10c07ca0)]
        private readonly WidgetContainer _window;

        [TempleDllLocation(0x10c0a468)]
        private bool _openedFromMainMenu;

        [TempleDllLocation(0x10c07d28)]
        private readonly WidgetScrollBar _scrollBar;

        private readonly WidgetText _loadTitle;
        private readonly WidgetButton _loadButton;

        private readonly WidgetText _saveTitle;
        private readonly WidgetButton _saveButton;

        private readonly WidgetButton _deleteButton;

        private readonly WidgetImage _largeScreenshot;

        [TempleDllLocation(0x10c0a49c)] [TempleDllLocation(0x10c073b4)]
        private bool _pendingConfirmation;

        /// <summary>
        /// Dummy save game info to represent making a new save when we're in save game mode.
        /// </summary>
        private static readonly SaveGameInfo NewSaveDummy = new SaveGameInfo()
        {
            Type = SaveGameType.NewSave
        };

        [TempleDllLocation(0x10c0a44c)]
        private List<SaveGameInfo> _saves = new List<SaveGameInfo>();

        private SaveGameInfo _selectedSave;

        private readonly SaveGameSlotButton[] _slots = new SaveGameSlotButton[8];

        private Mode _mode;

        [TempleDllLocation(0x10177ed0)]
        public SaveGameUi()
        {
            var doc = WidgetDoc.Load("ui/save_game_ui.json");

            _window = doc.GetRootContainer();
            _window.Visible = false;
            _window.SetCharHandler(OnCharEntered);

            _loadButton = doc.GetButton("load");
            _loadButton.SetClickHandler(OnLoadClick);
            _loadTitle = doc.GetTextContent("loadTitle");

            _saveButton = doc.GetButton("save");
            _saveButton.SetClickHandler(OnSaveClick);
            _saveTitle = doc.GetTextContent("saveTitle");

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

        private bool OnCharEntered(MessageCharArgs arg)
        {
            if (_selectedSave == null || _mode != Mode.Saving)
            {
                return false;
            }

            // Find the button that is currently showing the selected save
            foreach (var slot in _slots)
            {
                if (slot.Selected)
                {
                    slot.AppendNewNameChar(arg.Character);
                }
            }

            return true;
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
                // If a save game is selected (usually the case), and we're in save game mode
                // that means there's a text entry field that needs these...
                if (_mode != Mode.Saving)
                {
                    return false;
                }

                var selectedSlot = _slots.FirstOrDefault(s => s.Selected);

                return selectedSlot?.HandleKey(arg) ?? false;
            }

            switch (arg.key)
            {
                case DIK.DIK_ESCAPE:
                    OnCloseClick();
                    return true;
                case DIK.DIK_RETURN:
                    if (_mode == Mode.Loading)
                    {
                        OnLoadClick();
                    }
                    else
                    {
                        OnSaveClick();
                    }

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
            if (!_pendingConfirmation && _selectedSave != null)
            {
                if (!UiSystems.MainMenu.LoadGame(_selectedSave))
                {
                    // TODO: Actually show the message box vanilla shows in this case
                    UiSystems.Popup.ConfirmBox("#{loadgame:20}", "#{loadgame:21}", false, null);
                    return;
                }

                Hide();

                // Center view on party leader
                var leader = GameSystems.Party.GetLeader();
                if (leader != null)
                {
                    var loc = leader.GetLocation();
                    GameSystems.Location.CenterOn(loc.locx, loc.locy);
                }

                UiSystems.Party.UpdateAndShowMaybe();
            }
        }

        private void CreateSaveGame(string baseName, string displayName)
        {
            try
            {
                Globals.GameLib.SaveGame(baseName, displayName);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to create save game {0}", e);
                UiSystems.Popup.ConfirmBox("#{loadgame:21}", "#{savegame:0}", false, _ => { });
                return;
            }

            UiSystems.SaveGame.Hide();
            UiSystems.MainMenu.Hide();
        }

        [TempleDllLocation(0x10175c80)]
        private void OnSaveClick()
        {
            // Find the selected slot and it's currently entered name
            var selectedSlot = _slots.FirstOrDefault(slot => slot.Selected);

            if (_pendingConfirmation || selectedSlot == null)
            {
                return;
            }

            var displayName = selectedSlot.NewName;

            // Check if the user wants to overwrite an existing save game
            if (selectedSlot.SaveGame != NewSaveDummy)
            {
                // Reuse the existing base name to overwrite it
                var baseName = selectedSlot.SaveGame.BasePath;

                // Ask the player if they really want to overwrite
                _pendingConfirmation = true;
                UiSystems.Popup.ConfirmBox("#{savegame:16}", "#{savegame:15}", false, buttonClicked =>
                {
                    _pendingConfirmation = false;
                    if (buttonClicked == 0)
                    {
                        CreateSaveGame(baseName, displayName);
                    }
                });
            }
            else
            {
                // Create a new save slot, start by finding a free slot id
                var saveGames = Globals.GameLib.GetSaveGames();

                // Find the highest slot number for a normal save
                var highestUsedSlot = saveGames.Where(save => save.Type == SaveGameType.Normal)
                    .Select(save => save.Slot)
                    .DefaultIfEmpty(-1)
                    .Max();

                var basePath = Path.Join(Globals.GameFolders.SaveFolder, $"slot{highestUsedSlot + 1:0000}");
                CreateSaveGame(basePath, displayName);
            }
        }

        [TempleDllLocation(0x10177490)]
        private void OnDeleteClick()
        {
            if (!_pendingConfirmation && _selectedSave != null)
            {
                _pendingConfirmation = true;
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

                    _pendingConfirmation = false;
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
        public void ShowLoad(bool fromMainMenu)
        {
            Show(Mode.Loading, fromMainMenu);
        }

        [TempleDllLocation(0x10175980)]
        public void ShowSave(bool fromMainMenu)
        {
            Show(Mode.Saving, fromMainMenu);
        }

        private void Show(Mode mode, bool fromMainMenu)
        {
            if (!IsVisible)
            {
                GameSystems.TimeEvent.PauseGameTime();
            }

            _mode = mode;

            LoadSaveList();
            UpdateUi();

            _window.Visible = true;
            _window.CenterOnScreen();
            _window.BringToFront();
            _openedFromMainMenu = fromMainMenu;
            UiSystems.UtilityBar.Hide();
        }

        private void LoadSaveList(int selectIndex = 0)
        {
            _pendingConfirmation = false;
            _saves = Globals.GameLib.GetSaveGames();
            _saves.Sort(SaveGameOrder.LastModifiedAutoFirst);

            // Insert the dummy save for making a new save in first position
            if (_mode == Mode.Saving)
            {
                _saves.Insert(0, NewSaveDummy);
            }

            _scrollBar.Max = Math.Max(0, _saves.Count - 8);

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
        [TempleDllLocation(0x10175ae0)]
        public void Hide()
        {
            if (!IsVisible)
            {
                return;
            }

            GameSystems.TimeEvent.ResumeGameTime();

            _saves.Clear();
            _selectedSave = null;
            _window.Visible = false;
            if (_mode == Mode.Loading)
            {
                if (!_openedFromMainMenu)
                {
                    UiSystems.UtilityBar.Show();
                }
            }
            else
            {
                // Savegame menu is only reachable from within the game
                UiSystems.UtilityBar.Show();
                UiSystems.MainMenu.Show(MainMenuPage.MainMenu);
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
                    slot.SetSaveInfo(save, _mode == Mode.Saving);
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
            if (save == NewSaveDummy)
            {
                _largeScreenshot.Visible = false;
                return;
            }

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

        private void UpdateUi()
        {
            _loadButton.Visible = _mode == Mode.Loading;
            _loadTitle.Visible = _mode == Mode.Loading;
            _saveButton.Visible = _mode == Mode.Saving;
            _saveTitle.Visible = _mode == Mode.Saving;
        }

        enum Mode
        {
            Saving,
            Loading
        }
    }
}