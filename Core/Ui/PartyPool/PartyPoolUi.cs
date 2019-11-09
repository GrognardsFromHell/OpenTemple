using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Help;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.MainMenu;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.PartyPool
{
    public class PartyPoolUi : IResetAwareSystem, ISaveGameAwareGameSystem
    {
        private const string AddButtonStyle = "partyPoolAddButton";

        private const string AddButtonLabel = "#{party_pool:20}";

        private const string RemoveButtonStyle = "partyPoolRemoveButton";

        private const string RemoveButtonLabel = "#{party_pool:21}";

        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10163720)]
        public bool IsVisible => _container.IsVisible();

        private Alignment _alignment;

        // This is a fullscreen backdrop preventing click-through
        [TempleDllLocation(0x10BF1764)]
        private WidgetContainer uiPartypoolWidgetId;

        [TempleDllLocation(0x10BF24E0)]
        private bool uiPartyCreationNotFromShopmap; // TODO Rename to editParty

        [TempleDllLocation(0x10BF17A8)]
        private LocAndOffsets locToCreatePcs;

        [TempleDllLocation(0x10BF1760)]
        private int uiPartyPoolPcsIdx;

        [TempleDllLocation(0x10BF2408)]
        private WidgetButton _addRemoveButton;

        [TempleDllLocation(0x10BF2538)]
        private WidgetButton _viewButton;

        [TempleDllLocation(0x10BF2410)]
        private WidgetButton _renameButton;

        [TempleDllLocation(0x10BF239C)]
        private WidgetButton _deleteButton;

        [TempleDllLocation(0x10BDB8E0)]
        private WidgetButton _beginAdventuringButton;

        [TempleDllLocation(0x10bf1ba4)]
        private WidgetContainer _container;

        [TempleDllLocation(0x10bf21a4)]
        private WidgetButton _hidePreGenButton;

        [TempleDllLocation(0x10bf22b4)]
        private WidgetButton _hideIncompatibleButton;

        private WidgetText _partyAlignmentLabel;

        private ScrollBox _helpScrollBox;

        // Scrolls through available players
        [TempleDllLocation(0x10bf23b4)]
        private WidgetScrollBar _scrollBar;

        private PartyPoolSlot[] _slots;

        private readonly PartyPoolPortraits _portraits;

        public PartyPoolUi()
        {
            // TODO: Auto-resize to screen size
            _container = new WidgetContainer(Tig.RenderingDevice.GetCamera().ScreenSize);
            _container.SetVisible(false);
            // Eat mouse clicks to prevent "walking around" on the shopmap
            _container.SetMouseMsgHandler(msg => true);

            var doc = WidgetDoc.Load("ui/party_pool.json");
            var window = doc.TakeRootContainer();
            _container.Add(window);

            _addRemoveButton = doc.GetButton("addRemoveButton");
            // ADD button
            // Render: 0x10163800
            // Message: 0x10166020
            _addRemoveButton.SetClickHandler(AddOrRemovePlayer);

            // Created @ 0x1016610b
            // var @ [TempleDllLocation(0x10bf2398)]
            var createButton = doc.GetButton("createButton");
            // createButton.OnHandleMessage += 0x10165760;
            // createButton.OnBeforeRender += 0x101639e0;

            // Created @ 0x101649ae
            _viewButton = doc.GetButton("viewButton");
            // _viewButton.OnHandleMessage += 0x10163b60;
            // _viewButton.OnBeforeRender += 0x10163aa0;

            // Created @ 0x101667fe
            _renameButton = doc.GetButton("renameButton");
            // _renameButton.OnHandleMessage += 0x101664c0;
            // _renameButton.OnBeforeRender += 0x10163bc0;

            // Created @ 0x101665ee
            _deleteButton = doc.GetButton("deleteButton");
            // _deleteButton.OnHandleMessage += 0x10166270;
            // _deleteButton.OnBeforeRender += 0x10163c80;

            // var @ [TempleDllLocation(0x10bf29f0)]
            var exitButton = doc.GetButton("exitButton");
            // exitButton.OnHandleMessage += 0x10166040;
            // exitButton.OnBeforeRender += 0x10163910;
            exitButton.SetClickHandler(Cancel);

            _hidePreGenButton = doc.GetButton("hidePregen");
            // Hide Pregenerated chars, RENDER: 0x10164320, Message: 0x10164460

            _hideIncompatibleButton = doc.GetButton("hideIncompatible");
            // Hide Pregenerated chars, RENDER: 0x101644a0, Message: 0x101645e0

            _partyAlignmentLabel = doc.GetTextContent("partyAlignment");
            _partyAlignmentLabel.LegacyAdditionalTextColors = new[]
            {
                new ColorRect(new PackedLinearColorA(0xFF1AC4FF))
            };

            uiPartypoolWidgetId = new WidgetContainer(Tig.RenderingDevice.GetCamera().ScreenSize);

            _beginAdventuringButton = new WidgetButton();

            var scrollBoxSettings = new ScrollBoxSettings
            {
                Font = PredefinedFont.ARIAL_10,
                Indent = 15,
                TextArea = new Rectangle(14, 28, 210, 313),
                ScrollBarPos = new Point(226, 8),
                ScrollBarHeight = 333
            };
            var helpContainer = doc.GetWindow("helpContainer");
            _helpScrollBox = new ScrollBox(new Rectangle(Point.Empty, helpContainer.Rectangle.Size), scrollBoxSettings);
            _helpScrollBox.SetHelpContent("TAG_CHARGEN_PARTY_POOL", includeTitle: true);
            _helpScrollBox.OnLinkClicked += GameSystems.Help.OpenLink;
            helpContainer.Add(_helpScrollBox);

            _scrollBar = doc.GetScrollBar("scrollBar");
            _scrollBar.SetValueChangeHandler(_ => Update());

            var slotsContainer = doc.GetWindow("slots");
            _slots = new PartyPoolSlot[7];
            for (var i = 0; i < _slots.Length; i++)
            {
                var slot = new PartyPoolSlot();
                var padding = 0;
                if (i > 0)
                {
                    padding = 2;
                }

                slot.SetY(i * (slot.GetHeight() + padding));
                var slotIdx = i;
                slot.SetClickHandler(() => SelectAvailable(_scrollBar.GetValue() + slotIdx));
                // Forward scrollwheel to the scrollbar
                slot.SetMouseMsgHandler(msg =>
                {
                    if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
                    {
                        return _scrollBar.HandleMouseMessage(msg);
                    }

                    return false;
                });
                _slots[i] = slot;

                slotsContainer.Add(slot);
            }

            _portraits = new PartyPoolPortraits();
            var portraitContainer = _portraits.Container;
            // Position it in the lower left corner of the parent container
            portraitContainer.SetY(_container.GetHeight() - portraitContainer.GetHeight());
            _portraits.OnSelectedChanged += PartyMemberSelectionChanged;
            _container.Add(portraitContainer);

            Update();
        }

        [TempleDllLocation(0x10bf23a8)]
        private bool _confirmingPlayerRemoval;

        [TempleDllLocation(0x10166020)]
        private void AddOrRemovePlayer()
        {
            if (!_confirmingPlayerRemoval)
            {
                if (_portraits.Selected == null)
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
                    RemoveSelectedPlayer();
                }

                if (uiPartyCreationNotFromShopmap || GameSystems.Party.PartySize == 0)
                {
                    _beginAdventuringButton.SetVisible(false);
                }
                else
                {
                    _beginAdventuringButton.SetVisible(true);
                }
            }
        }

        [TempleDllLocation(0x10163100)]
        private void RemoveSelectedPlayer()
        {
            var selectedPortrait = _portraits.Selected;
            if (selectedPortrait == null)
            {
                return;
            }

            // Find the selected player and remove them
            var player = _availablePlayers.Find(p => p.handle == selectedPortrait);
            if (player != null)
            {
                player.flag4 = false;
                GameSystems.Party.RemoveFromAllGroups(player.handle);
            }

            ClearSelection();
        }

        [TempleDllLocation(0x10163150)]
        [TemplePlusLocation("ui_legacysystems.cpp:771")]
        private void RemoveAndDeleteSelectedPlayer()
        {
            var selectedPortrait = _portraits.Selected;
            if (selectedPortrait == null)
            {
                return;
            }

            // Find the selected player and remove them
            var player = _availablePlayers.Find(p => p.handle == selectedPortrait);
            if (player != null)
            {
                GameSystems.Party.RemoveFromAllGroups(player.handle);
                GameSystems.MapObject.RemoveMapObj(player.handle);
                player.handle = null;
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
            var player = _availablePlayers.Find(p => p.Selected);
            if (_confirmingPlayerRemoval
                || player == null
                || player.flag4
                || player.state != SlotState.CanJoin)
            {
                return false;
            }

            CreatePlayerOnDemand(player);

            player.flag4 = true;
            GameSystems.Party.AddToPCGroup(player.handle);
            ClearSelection();
            return true;
        }

        // Clear only the available party member selection
        private void PartyMemberSelectionChanged()
        {
            foreach (var availablePlayer in _availablePlayers)
            {
                availablePlayer.Selected = false;
            }
            Update();
        }

        [TempleDllLocation(0x10025f10)]
        [TemplePlusLocation("generalfixes.cpp:399")]
        private void CreatePlayerOnDemand(PartyPoolPlayer player)
        {
            if (player.handle == null)
            {
                using var reader = new BinaryReader(new MemoryStream(player.data));
                var handle = GameObjectBody.Load(reader);
                handle.UnfreezeIds();
                handle.SetLocation(locXY.Zero);
                GameSystems.MapObject.InitDynamic(handle, locXY.Zero);
                player.handle = handle;
            }
        }

        private void ClearSelection(bool update = true)
        {
            foreach (var player in _availablePlayers)
            {
                player.Selected = false;
            }

            _portraits.Selected = null;

            if (update)
            {
                Update();
            }
        }

        private void SelectAvailable(int slotIdx)
        {
            ClearSelection(false);

            if (slotIdx < _availablePlayers.Count)
            {
                _availablePlayers[slotIdx].Selected = true;
            }

            Update();
        }

        [TempleDllLocation(0x10165cd0)]
        public void Reset()
        {
            UiPartyPool_10163D40();
            UiPartypoolClose(false);

            // This is relevant to savegames, since it stores which players cannot be added to the party anymore
            partypoolPcAlreadyBeenInPartyIds.Clear();
        }

        [TempleDllLocation(0x10165d10)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10165da0)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10165e60)]
        public void Show(bool editParty)
        {
            _alignment = GameSystems.Party.PartyAlignment;

            // TODO int& uiPcCreationMainWndId = temple.GetRef<int>(0x10BDD690);

            uiPartypoolWidgetId.SetVisible(true);
            uiPartypoolWidgetId.BringToFront();
            // TODO Globals.UiManager.BringToFront(uiPcCreationMainWndId);
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
            PartyPoolLoader();
            AddPcsFromBuffer();
            Update();
            PcPortraitsRefresh();
            UiPartyPoolScrollbox_10164620();

            _container.Show();
            _container.CenterOnScreen();

            UiSystems.Party.Hide();
            UiSystems.UtilityBar.Hide();
        }

        [TempleDllLocation(0x10165a50)]
        private void Cancel()
        {
            UiSystems.CharSheet.Hide(0);
            _beginAdventuringButton.SetVisible(false);
            UiPartypoolClose(!uiPartyCreationNotFromShopmap);

            if (!uiPartyCreationNotFromShopmap)
            {
                UiSystems.PCCreation.ClearParty();
                UiSystems.PCCreation.Hide();
                UiSystems.MainMenu.Show(MainMenuPage.Difficulty);
                UiPartyPool_10163D40();
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

        [TempleDllLocation(0x10bf2ba8)]
        private ISet<ObjectId> partypoolPcAlreadyBeenInPartyIds = new HashSet<ObjectId>();

        [TempleDllLocation(0x10164fb0)]
        private void UiPartypoolClose(bool a1)
        {
            if (!a1)
            {
                foreach (var player in GameSystems.Party.PlayerCharacters)
                {
                    partypoolPcAlreadyBeenInPartyIds.Add(player.id);
                }
            }

            uiPartypoolWidgetId.SetVisible(false);
            _container.SetVisible(false);

            if (!a1)
            {
                UiSystems.Party.UpdateAndShowMaybe();
                UiSystems.UtilityBar.Show();
                UiSystems.Party.Show();
            }
        }

        [TempleDllLocation(0x10163d40)]
        public void UiPartyPool_10163D40()
        {
            foreach (var availablePlayer in _availablePlayers)
            {
                if (availablePlayer.handle != null)
                {
                    GameSystems.Object.Destroy(availablePlayer.handle);
                }
            }

            // TODO partyPoolPcIndices/*0x10bf2378*/ = 0;
        }

        [TempleDllLocation(0x10bf0f30)]
        private List<ObjectId> pcCreationObjIdBuffer = new List<ObjectId>();

        [TempleDllLocation(0x10bf253c)]
        private readonly List<PartyPoolPlayer> _availablePlayers = new List<PartyPoolPlayer>();

        [TempleDllLocation(0x101631B0)]
        private void GetPcCreationPcBuffer()
        {
            pcCreationObjIdBuffer.Clear();
            foreach (var player in GameSystems.Party.PlayerCharacters)
            {
                pcCreationObjIdBuffer.Add(player.id);
            }
        }

        [TempleDllLocation(0x10163e30)]
        public void ClearAvailable()
        {
            foreach (var availablePlayer in _availablePlayers)
            {
                if (!availablePlayer.flag4 && availablePlayer.handle != null)
                {
                    GameSystems.Object.Destroy(availablePlayer.handle);
                    availablePlayer.handle = null;
                }
            }

            _availablePlayers.Clear();
        }

        [TempleDllLocation(0x10165790)]
        private bool PartyPoolLoader()
        {
            ClearAvailable();

            var searchPattern = "players/*.ToEEPC";
            if (Globals.GameLib.IsIronmanGame)
            {
                searchPattern = "players/ironman/*.ToEEIMan";
            }

            // Load premade PCs from archives/data files
            foreach (var path in Tig.FS.Search(searchPattern))
            {
                using var reader = Tig.FS.OpenBinaryReader(path);
                try
                {
                    var availablePc = PartyPoolPlayer.Read(reader);
                    availablePc.premade = true;
                    availablePc.path = path;
                    _availablePlayers.Add(availablePc);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to read premade player from {0}: {1}", path, e);
                }
            }

            // TODO: Read PCs from the player's chars folder!

            return true;
        }

        [TempleDllLocation(0x10163210)]
        private void AddPcsFromBuffer()
        {
// TODO
        }

        [TempleDllLocation(0x10165150)]
        private void Update()
        {
            var alignmentName = GameSystems.Stat.GetAlignmentName(_alignment);
            _partyAlignmentLabel.SetText("@1#{party_pool:1}@0 " + alignmentName);

            UpdateSlots();
            _portraits.Update();

            _addRemoveButton.SetStyle(AddButtonStyle);
            _addRemoveButton.SetText(AddButtonLabel);
            _addRemoveButton.SetDisabled(true);
            _viewButton.SetDisabled(true);
            _renameButton.SetDisabled(true);
            _deleteButton.SetDisabled(true);

            var selectedPlayer = _availablePlayers.Find(p => p.Selected);
            if (selectedPlayer != null)
            {
                if (selectedPlayer.state == SlotState.CanJoin)
                {
                    _addRemoveButton.SetDisabled(false);
                }

                _viewButton.SetDisabled(false);
                // TODO: where a char is premade or not should be decided based on the storage location, not its content
                if (selectedPlayer.premade)
                {
                    _renameButton.SetDisabled(false);
                    _deleteButton.SetDisabled(false);
                }
            }
            else if (_portraits.Selected != null)
            {
                _addRemoveButton.SetStyle(RemoveButtonStyle);
                _addRemoveButton.SetText(RemoveButtonLabel);
                _addRemoveButton.SetDisabled(false);
            }

            if (uiPartyCreationNotFromShopmap || GameSystems.Party.PartySize == 0)
            {
                _beginAdventuringButton.SetVisible(false);
            }
            else
            {
                _beginAdventuringButton.SetVisible(true);
            }
        }

        private void UpdateSlots()
        {
            var hiddenSlots = Math.Max(0, _availablePlayers.Count - _slots.Length);
            _scrollBar.SetMax(hiddenSlots);

            foreach (var player in _availablePlayers)
            {
                UpdateState(player);
            }

            for (var i = 0; i < _slots.Length; i++)
            {
                var actualIdx = _scrollBar.GetValue() + i;
                _slots[i].Player = actualIdx < _availablePlayers.Count ? _availablePlayers[actualIdx] : null;
            }
        }

        [TempleDllLocation(0x10163440)]
        private void PcPortraitsRefresh()
        {
            // TODO
        }

        [TempleDllLocation(0x10164620)]
        private void UiPartyPoolScrollbox_10164620()
        {
            // TODO
        }

        [TempleDllLocation(0x10164d60)]
        private void UpdateState(PartyPoolPlayer pc)
        {
            if (!GameSystems.Stat.AlignmentsUnopposed(pc.alignment, _alignment))
            {
                pc.state = SlotState.OpposedAlignment;
                return;
            }

            // Paladins cannot be in a party with any evil characters and vice-versa
            if (pc.primaryClass == Stat.level_paladin)
            {
                foreach (var otherPartyMember in GameSystems.Party.PartyMembers)
                {
                    if (otherPartyMember.HasEvilAlignment())
                    {
                        pc.state = SlotState.PaladinOpoposedAlignment;
                        return;
                    }
                }
            }
            else if (pc.alignment.IsEvil())
            {
                foreach (var otherPartyMember in GameSystems.Party.PartyMembers)
                {
                    if (otherPartyMember.GetStat(Stat.level_paladin) > 0)
                    {
                        pc.state = SlotState.PaladinOpoposedAlignment;
                        return;
                    }
                }
            }

            if (partypoolPcAlreadyBeenInPartyIds.Contains(pc.objId))
            {
                pc.state = SlotState.WasInParty;
            }
            else
            {
                pc.state = SlotState.CanJoin;
            }
        }
    }

    internal class PartyPoolPlayer
    {
        public bool flag4;
        public byte[] data;
        public int field_C;
        public ObjectId objId;
        public string name;
        public bool premade; // Was flag 8
        public string path;
        public int portraitId;
        public Gender gender;
        public Stat primaryClass;
        public RaceId race;
        public Alignment alignment;
        public int hpMax;
        public int field_14C;
        public GameObjectBody handle;
        public SlotState state;
        public bool Selected;

        public static PartyPoolPlayer Read(BinaryReader reader)
        {
            var result = new PartyPoolPlayer();
            var flags = reader.ReadInt32();
            result.premade = (flags & 8) != 0;
            var dataSize = reader.ReadInt32();
            result.objId = reader.ReadObjectId();
            result.name = reader.ReadPrefixedString();
            // 6 32-bit integers follow
            result.portraitId = reader.ReadInt32();
            result.gender = reader.ReadInt32() switch
            {
                0 => Gender.Female,
                1 => Gender.Male,
                _ => Gender.Male
            };
            result.primaryClass = reader.ReadInt32() switch
            {
                7 => Stat.level_barbarian,
                8 => Stat.level_bard,
                9 => Stat.level_cleric,
                10 => Stat.level_druid,
                11 => Stat.level_fighter,
                12 => Stat.level_monk,
                13 => Stat.level_paladin,
                14 => Stat.level_ranger,
                15 => Stat.level_rogue,
                16 => Stat.level_sorcerer,
                17 => Stat.level_wizard,
                _ => Stat.level_fighter
            };
            result.race = reader.ReadInt32() switch
            {
                0 => RaceId.human,
                1 => RaceId.dwarf,
                2 => RaceId.elf,
                3 => RaceId.gnome,
                4 => RaceId.halfelf,
                5 => RaceId.half_orc,
                6 => RaceId.halfling,
                _ => RaceId.human
            };
            result.alignment = reader.ReadInt32() switch
            {
                0 => Alignment.TRUE_NEUTRAL,
                1 => Alignment.LAWFUL_NEUTRAL,
                2 => Alignment.CHAOTIC_NEUTRAL,
                4 => Alignment.NEUTRAL_GOOD,
                5 => Alignment.LAWFUL_GOOD,
                6 => Alignment.CHAOTIC_GOOD,
                8 => Alignment.NEUTRAL_EVIL,
                9 => Alignment.LAWFUL_EVIL,
                10 => Alignment.CHAOTIC_EVIL,
                _ => Alignment.TRUE_NEUTRAL
            };
            result.hpMax = reader.ReadInt32();
            result.data = reader.ReadBytes(dataSize);
            return result;
        }
    }
}