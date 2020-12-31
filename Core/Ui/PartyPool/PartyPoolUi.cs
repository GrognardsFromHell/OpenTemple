using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DynamicData;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.MainMenu;
using ReactiveUI;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Button = OpenTemple.Widgets.Button;
using Point = System.Drawing.Point;

namespace OpenTemple.Core.Ui.PartyPool
{
    public class PartyPoolUi : AvaloniaObject, IResetAwareSystem, ISaveGameAwareUi
    {
        private const string AddButtonStyle = "partyPoolAddButton";

        private const string AddButtonLabel = "#{party_pool:20}";

        private const string RemoveButtonStyle = "partyPoolRemoveButton";

        private const string RemoveButtonLabel = "#{party_pool:21}";

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10163720)]
        public bool IsVisible => _container.IsVisible;

        private Alignment _alignment;

        [TempleDllLocation(0x10BF24E0)]
        private bool uiPartyCreationNotFromShopmap; // TODO Rename to editParty

        [TempleDllLocation(0x10BF17A8)]
        private LocAndOffsets locToCreatePcs;

        [TempleDllLocation(0x10BF1760)]
        private int uiPartyPoolPcsIdx;

        [TempleDllLocation(0x10BF2408)]
        private Button _addRemoveButton;

        [TempleDllLocation(0x10BF2538)]
        private Button _viewButton;

        [TempleDllLocation(0x10BF2410)]
        private Button _renameButton;

        [TempleDllLocation(0x10BF239C)]
        private Button _deleteButton;

        [TempleDllLocation(0x10BDB8E0)]
        public Button BeginAdventuringButton { get; private set; }

        [TempleDllLocation(0x10bf1ba4)] [TempleDllLocation(0x10BF1764)]
        private PartyCreationBackdrop _container;

        private ScrollBox _helpScrollBox;

        [TempleDllLocation(0x10bf0f30)]
        private List<ObjectId> pcCreationObjIdBuffer = new();

        [TempleDllLocation(0x10bf2ba8)]
        private ISet<ObjectId> partypoolPcAlreadyBeenInPartyIds = new HashSet<ObjectId>();

        private readonly PartyPoolDialog _dialog;

        private readonly PartyPoolModel _viewModel = new();

        public PartyPoolUi()
        {
            _viewModel.Slots = new List<PartyMemberSlotModel>(Globals.Config.MaxPCs);
            for (var i = 0; i < Globals.Config.MaxPCs; i++)
            {
                _viewModel.Slots.Add(new PartyMemberSlotModel(_viewModel));
            }

            _container = new PartyCreationBackdrop();
            _container.IsVisible = false;
            _container.DataContext = _viewModel;
            Tig.MainWindow.AddOverlay(_container);

            _dialog = _container.FindControl<PartyPoolDialog>("dialog");
            _dialog.DataContext = _viewModel;

            _addRemoveButton = _dialog.FindControl<Button>("addRemoveButton");
            // ADD button
            // Render: 0x10163800
            // Message: 0x10166020
            _addRemoveButton.Click += (_, _) => AddOrRemovePlayer();

            // Created @ 0x1016610b
            // var @ [TempleDllLocation(0x10bf2398)]
            var createButton = _dialog.FindControl<Button>("createButton");
            createButton.Click += (_, _) => StartCharCreation();

            // Created @ 0x101649ae
            _viewButton = _dialog.FindControl<Button>("viewButton");
            _viewButton.Click += (_, _) => ViewSelected();
            // _viewButton.OnBeforeRender += 0x10163aa0;

            // Created @ 0x101667fe
            _renameButton = _dialog.FindControl<Button>("renameButton");
            // _renameButton.OnHandleMessage += 0x101664c0;
            // _renameButton.OnBeforeRender += 0x10163bc0;

            // Created @ 0x101665ee
            _deleteButton = _dialog.FindControl<Button>("deleteButton");
            // _deleteButton.OnHandleMessage += 0x10166270;
            // _deleteButton.OnBeforeRender += 0x10163c80;

            // var @ [TempleDllLocation(0x10bf29f0)]
            var exitButton = _dialog.FindControl<Button>("exitButton");
            // exitButton.OnHandleMessage += 0x10166040;
            // exitButton.OnBeforeRender += 0x10163910;
            exitButton.Click += (_, _) => Cancel();

            // Begin Adventuring button, original render @ 0x1011c060, msg @ 0x1011fee0
            BeginAdventuringButton = _container.FindControl<Button>("beginAdventuring");
            BeginAdventuringButton.Click += (_, _) => BeginAdventuring();

            var scrollBoxSettings = new ScrollBoxSettings
            {
                Font = PredefinedFont.ARIAL_10,
                Indent = 15,
                TextArea = new Rectangle(14, 28, 210, 313),
                ScrollBarPos = new Point(226, 8),
                ScrollBarHeight = 333
            };

// TODO           var helpContainer = _dialog.GetContainer("helpContainer");
// TODO           _helpScrollBox = new ScrollBox(new Rectangle(Point.Empty, helpContainer.Rectangle.Size), scrollBoxSettings);
// TODO           _helpScrollBox.SetHelpContent("TAG_CHARGEN_PARTY_POOL", includeTitle: true);
// TODO           _helpScrollBox.OnLinkClicked += GameSystems.Help.OpenLink;
// TODO           helpContainer.Add(_helpScrollBox);

            _viewModel.WhenAnyValue(e => e.SelectedPlayer)
                .Subscribe(_ => UpdateButtonStates());

            // Update config from checkbox settings
            _viewModel.HidePremadePlayers = Globals.Config.PartyPoolHidePreGeneratedChars;
            _viewModel.HideIncompatiblePlayers = Globals.Config.PartyPoolHideIncompatibleChars;

            _viewModel.WhenAnyValue(e => e.HidePremadePlayers)
                .Subscribe(x => Globals.Config.PartyPoolHidePreGeneratedChars = x);
            _viewModel.WhenAnyValue(e => e.HideIncompatiblePlayers)
                .Subscribe(x => Globals.Config.PartyPoolHideIncompatibleChars = x);
        }

        [TempleDllLocation(0x10165760)]
        private void StartCharCreation()
        {
            UiSystems.CharSheet.Hide(0);
            UiSystems.PCCreation.Begin();
            UiSystems.PartyPool.UiPartypoolClose(true);
        }

        [TempleDllLocation(0x1011fee0)]
        private void BeginAdventuring()
        {
            UiSystems.PCCreation.UiChargenFinalize();
            GameSystems.Party.AddPartyMoney(0, 500, 0, 0);
        }

        [TempleDllLocation(0x10163b60)]
        private void ViewSelected()
        {
            var selected = _viewModel.SelectedPlayer;

            if (selected != null && !_confirmingPlayerRemoval)
            {
                CreatePlayerOnDemand(selected);

                UiSystems.CharSheet.State = CharInventoryState.PartyPool;
                UiSystems.CharSheet.Show(selected.GameObject);
            }
        }

        [TempleDllLocation(0x10bf23a8)]
        private bool _confirmingPlayerRemoval;

        [TempleDllLocation(0x10166020)]
        private void AddOrRemovePlayer()
        {
            var selectedPlayer = _viewModel.SelectedPlayer;
            if (selectedPlayer == null)
            {
                return;
            }

            if (!_confirmingPlayerRemoval)
            {
                var selectedGameObject = selectedPlayer.GameObject;
                if (selectedGameObject == null || !GameSystems.Party.IsInParty(selectedGameObject))
                {
                    AddSelectedPlayer();
                }
                else if (uiPartyCreationNotFromShopmap)
                {
                    _confirmingPlayerRemoval = true;
                    UiSystems.Popup.ConfirmBox("#{party_pool:105}", "#{party_pool:106}", true, RemovePlayerConfirm);
                }
                else
                {
                    RemoveFromParty(selectedPlayer);
                }

                if (uiPartyCreationNotFromShopmap || GameSystems.Party.PartySize == 0)
                {
                    BeginAdventuringButton.IsVisible = false;
                }
                else
                {
                    BeginAdventuringButton.IsVisible = true;
                }
            }
        }

        [TempleDllLocation(0x10163100)]
        private void RemoveFromParty(PartyPoolRowModel player)
        {
            player.InParty = false;
            GameSystems.Party.RemoveFromAllGroups(player.GameObject);
            ClearSelection();
        }

        [TempleDllLocation(0x10163150)]
        [TemplePlusLocation("ui_legacysystems.cpp:771")]
        private void RemoveAndDeleteSelectedPlayer()
        {
            var player = _viewModel.SelectedPlayer;

            // Find the selected player and remove them
            if (player?.GameObject != null)
            {
                GameSystems.Party.RemoveFromAllGroups(player.GameObject);
                GameSystems.MapObject.RemoveMapObj(player.GameObject);
                player.GameObject = null;
            }

            ClearSelection();
        }

        [TempleDllLocation(0x101642f0)]
        private void RemovePlayerConfirm(int buttonClicked)
        {
            _confirmingPlayerRemoval = false;
            if (buttonClicked == 0)
            {
                RemoveAndDeleteSelectedPlayer();
            }
        }

        [TempleDllLocation(0x10164ec0)]
        private bool AddSelectedPlayer()
        {
            var player = _viewModel.SelectedPlayer;
            if (_confirmingPlayerRemoval
                || player == null
                || player.InParty
                || !player.CanJoin)
            {
                return false;
            }

            CreatePlayerOnDemand(player);

            player.InParty = true;
            GameSystems.Party.AddToPCGroup(player.GameObject);
            ClearSelection();
            return true;
        }

        [TempleDllLocation(0x10025f10)]
        [TemplePlusLocation("generalfixes.cpp:399")]
        private void CreatePlayerOnDemand(PartyPoolRowModel player)
        {
            if (player.GameObject == null)
            {
                using var reader = new BinaryReader(new MemoryStream(player.Player.data));
                var handle = GameObjectBody.Load(reader);
                handle.UnfreezeIds();
                handle.SetLocation(locXY.Zero);
                GameSystems.MapObject.InitDynamic(handle, locXY.Zero);
                player.GameObject = handle;
            }
        }

        private void ClearSelection(bool update = true)
        {
            _viewModel.SelectedPlayer = null;

            if (update)
            {
                Update();
            }
        }

        [TempleDllLocation(0x10165cd0)]
        public void Reset()
        {
            ClearAll();
            UiPartypoolClose(false);

            // This is relevant to savegames, since it stores which players cannot be added to the party anymore
            partypoolPcAlreadyBeenInPartyIds.Clear();
        }

        [TempleDllLocation(0x10165d10)]
        public void SaveGame(SavedUiState savedState)
        {
            savedState.PartyPoolState = new SavedPartyPoolUiState
            {
                AlreadyBeenInParty = partypoolPcAlreadyBeenInPartyIds.ToHashSet()
            };
        }

        [TempleDllLocation(0x10165da0)]
        public void LoadGame(SavedUiState savedState)
        {
            var partyPoolState = savedState.PartyPoolState;

            partypoolPcAlreadyBeenInPartyIds.Clear();
            foreach (var objectId in partyPoolState.AlreadyBeenInParty)
            {
                partypoolPcAlreadyBeenInPartyIds.Add(objectId);
            }
        }

        [TempleDllLocation(0x10165e60)]
        public void Show(bool editParty)
        {
            _alignment = GameSystems.Party.PartyAlignment;

            uiPartyCreationNotFromShopmap = editParty;
            if (editParty)
            {
                var leader = GameSystems.Party.GetLeader();
                if (leader != null)
                {
                    locToCreatePcs = leader.GetLocationFull();
                }
            }

            UiSystems.Party.Clear();

            ClearSelection();

            GetPcCreationPcBuffer();
            LoadPlayers();
            AddPcsFromBuffer();
            Update();

            _container.IsVisible = true;

            UiSystems.Party.Hide();
            UiSystems.UtilityBar.Hide();
        }

        [TempleDllLocation(0x10165a50)]
        private void Cancel()
        {
            UiSystems.CharSheet.Hide(0);
            BeginAdventuringButton.IsVisible = false;
            UiPartypoolClose(!uiPartyCreationNotFromShopmap);

            if (!uiPartyCreationNotFromShopmap)
            {
                UiSystems.PCCreation.ClearParty();
                UiSystems.PCCreation.Hide();
                UiSystems.MainMenu.Show(MainMenuPage.Difficulty);
                ClearAll();
            }
            else
            {
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    GameSystems.MapObject.Move(partyMember, locToCreatePcs);
                    GameSystems.AI.ForceSpreadOut(partyMember);
                }

                ClearAvailable();
            }
        }

        [TempleDllLocation(0x10164fb0)]
        public void UiPartypoolClose(bool a1)
        {
            if (!a1)
            {
                foreach (var player in GameSystems.Party.PlayerCharacters)
                {
                    partypoolPcAlreadyBeenInPartyIds.Add(player.id);
                }
            }

            _container.IsVisible = false;

            if (!a1)
            {
                UiSystems.Party.UpdateAndShowMaybe();
                UiSystems.UtilityBar.Show();
                UiSystems.Party.Show();
            }
        }

        /// <summary>
        /// Clear all players, even if they are in the party (this is actually somewhat dangerous...)
        /// </summary>
        [TempleDllLocation(0x10163d40)]
        private void ClearAll()
        {
            foreach (var availablePlayer in _viewModel.Players.Items)
            {
                if (availablePlayer.GameObject != null)
                {
                    GameSystems.Object.Destroy(availablePlayer.GameObject);
                }
            }

            _viewModel.Players.Clear();
        }

        /// <summary>
        /// Clears only the players that have not been added to the party before.
        /// </summary>
        [TempleDllLocation(0x10163e30)]
        public void ClearAvailable()
        {
            foreach (var availablePlayer in _viewModel.Players.Items)
            {
                if (!availablePlayer.InParty && availablePlayer.GameObject != null)
                {
                    GameSystems.Object.Destroy(availablePlayer.GameObject);
                }
            }

            _viewModel.Players.Clear();
        }

        [TempleDllLocation(0x101631B0)]
        private void GetPcCreationPcBuffer()
        {
            pcCreationObjIdBuffer.Clear();
            foreach (var player in GameSystems.Party.PlayerCharacters)
            {
                pcCreationObjIdBuffer.Add(player.id);
            }
        }

        [TempleDllLocation(0x10165790)]
        private bool LoadPlayers()
        {
            ClearAvailable();

            var searchPattern = "players/*.ToEEPC";
            if (Globals.GameLib.IsIronmanGame)
            {
                searchPattern = "players/ironman/*.ToEEIMan";
            }

            _viewModel.Players.Edit(playerList =>
            {
                // Load premade PCs from archives/data files
                foreach (var path in Tig.FS.Search(searchPattern))
                {
                    PartyPoolPlayer availablePc;
                    try
                    {
                        using var reader = Tig.FS.OpenBinaryReader(path);
                        availablePc = PartyPoolPlayer.Read(reader);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed to read premade player from {0}: {1}", path, e);
                        continue;
                    }

                    availablePc.premade = true;
                    availablePc.path = path;
                    playerList.Add(new PartyPoolRowModel(availablePc));
                }
            });

            // TODO: Read PCs from the player's chars folder!

            return true;
        }

        [TempleDllLocation(0x10163210)]
        private void AddPcsFromBuffer()
        {
            Stub.TODO();
// TODO
        }

        private void UpdateButtonStates()
        {
            _addRemoveButton.Classes.ReplaceOrAdd(RemoveButtonStyle, AddButtonStyle);
            _addRemoveButton.Content = Globals.UiAssets.ApplyTranslation(AddButtonLabel);
            _addRemoveButton.IsEnabled = false;

            _viewButton.IsEnabled = false;
            _renameButton.IsEnabled = false;
            _deleteButton.IsEnabled = false;

            var selectedPlayer = _viewModel.SelectedPlayer;
            if (selectedPlayer != null)
            {
                if (!selectedPlayer.InParty)
                {
                    if (selectedPlayer.CanJoin)
                    {
                        _addRemoveButton.IsEnabled = true;
                    }

                    _viewButton.IsEnabled = true;
                    // TODO: where a char is premade or not should be decided based on the storage location, not its content
                    if (!selectedPlayer.IsPremade)
                    {
                        _renameButton.IsEnabled = true;
                        _deleteButton.IsEnabled = true;
                    }
                }
                else
                {
                    _addRemoveButton.Classes.Replace(AddButtonStyle, RemoveButtonStyle);
                    _addRemoveButton.Content = Globals.UiAssets.ApplyTranslation(RemoveButtonLabel);
                    _addRemoveButton.IsEnabled = true;
                }
            }

            if (uiPartyCreationNotFromShopmap || GameSystems.Party.PartySize == 0)
            {
                BeginAdventuringButton.IsVisible = false;
            }
            else
            {
                BeginAdventuringButton.IsVisible = true;
            }
        }

        [TempleDllLocation(0x10165150)]
        private void Update()
        {
            _viewModel.PartyAlignmentText = GameSystems.Stat.GetAlignmentName(_alignment);

            UpdateSlots();
            UpdatePartyMemberSlots();

            UpdateButtonStates();
        }

        private void UpdatePartyMemberSlots()
        {
            var players = GameSystems.Party.PlayerCharacters.ToArray();

            for (var i = 0; i < Math.Min(players.Length, _viewModel.Slots.Count); i++)
            {
                var slot = _viewModel.Slots[i];
                if (slot.Player?.GameObject == players[i])
                {
                    continue; // No need to update
                }

                slot.Player = _viewModel.Players.Items.First(ppp => ppp.GameObject == players[i]);
            }

            for (var i = players.Length; i < _viewModel.Slots.Count; i++)
            {
                _viewModel.Slots[i].Player = null;
            }
        }

        private void UpdateSlots()
        {
            foreach (var player in _viewModel.Players.Items)
            {
                UpdateState(player);
            }
        }

        [TempleDllLocation(0x10164d60)]
        private void UpdateState(PartyPoolRowModel pc)
        {
            pc.IsOpposedAlignment = !GameSystems.Stat.AlignmentsUnopposed(pc.Player.alignment, _alignment);

            // Paladins cannot be in a party with any evil characters and vice-versa
            pc.PaladinOpposedAlignment = false;
            if (pc.Player.primaryClass == Stat.level_paladin)
            {
                foreach (var otherPartyMember in GameSystems.Party.PartyMembers)
                {
                    if (otherPartyMember.HasEvilAlignment())
                    {
                        pc.PaladinOpposedAlignment = true;
                    }
                }
            }
            else if (pc.Player.alignment.IsEvil())
            {
                foreach (var otherPartyMember in GameSystems.Party.PartyMembers)
                {
                    if (otherPartyMember.GetStat(Stat.level_paladin) > 0)
                    {
                        pc.PaladinOpposedAlignment = true;
                    }
                }
            }

            pc.WasInParty = partypoolPcAlreadyBeenInPartyIds.Contains(pc.Player.objId);
        }

        [TempleDllLocation(0x10166490)]
        public void Add(GameObjectBody player)
        {
            throw new NotImplementedException();
        }
    }
}
