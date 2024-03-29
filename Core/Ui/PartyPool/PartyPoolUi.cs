using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyPool;

public class PartyPoolUi : IResetAwareSystem, ISaveGameAwareUi
{
    private const string AddButtonStyle = "partyPoolAddButton";

    private const string AddButtonLabel = "#{party_pool:20}";

    private const string RemoveButtonStyle = "partyPoolRemoveButton";

    private const string RemoveButtonLabel = "#{party_pool:21}";

    private const string CheckboxCheckedStyle = "partyPoolCheckboxChecked";

    private const string CheckboxUncheckedStyle = "partyPoolCheckboxUnchecked";

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10163720)]
    public bool IsVisible => _container.IsInTree;

    private Alignment _alignment;

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
    public WidgetButton BeginAdventuringButton { get; private set; }

    [TempleDllLocation(0x10bf1ba4)]
    [TempleDllLocation(0x10BF1764)]
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

    [TempleDllLocation(0x10bf0f30)]
    private List<ObjectId> pcCreationObjIdBuffer = new();

    [TempleDllLocation(0x10bf253c)]
    private readonly List<PartyPoolPlayer> _availablePlayers = new();

    private List<PartyPoolPlayer> _filteredPlayers;

    [TempleDllLocation(0x10bf2ba8)]
    private ISet<ObjectId> partypoolPcAlreadyBeenInPartyIds = new HashSet<ObjectId>();

    public PartyPoolUi()
    {
        // TODO: Auto-resize to screen size
        _container = new WidgetContainer
        {
            PixelSize = Globals.UiManager.CanvasSize
        };
        _container.PreventsInGameInteraction = true;

        var doc = WidgetDoc.Load("ui/party_pool.json");
        var window = doc.GetRootContainer();
        _container.Add(window);

        _addRemoveButton = doc.GetButton("addRemoveButton");
        // ADD button
        // Render: 0x10163800
        // Message: 0x10166020
        _addRemoveButton.AddClickListener(AddOrRemovePlayer);

        // Created @ 0x1016610b
        // var @ [TempleDllLocation(0x10bf2398)]
        var createButton = doc.GetButton("createButton");
        createButton.AddClickListener(StartCharCreation);

        // Created @ 0x101649ae
        _viewButton = doc.GetButton("viewButton");
        _viewButton.AddClickListener(ViewSelected);
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
        exitButton.AddClickListener(Cancel);

        _hidePreGenButton = doc.GetButton("hidePregen");
        // Hide Pregenerated chars, RENDER: 0x10164320, Message: 0x10164460
        _hidePreGenButton.AddClickListener(() =>
        {
            Globals.Config.PartyPoolHidePreGeneratedChars = !Globals.Config.PartyPoolHidePreGeneratedChars;
            Update();
        });

        _hideIncompatibleButton = doc.GetButton("hideIncompatible");
        // Hide Pregenerated chars, RENDER: 0x101644a0, Message: 0x101645e0
        _hideIncompatibleButton.AddClickListener(() =>
        {
            Globals.Config.PartyPoolHideIncompatibleChars = !Globals.Config.PartyPoolHideIncompatibleChars;
            Update();
        });

        _partyAlignmentLabel = doc.GetTextContent("partyAlignment");

        // Begin Adventuring button, original render @ 0x1011c060, msg @ 0x1011fee0
        BeginAdventuringButton = new WidgetButton();
        BeginAdventuringButton.SetStyle("partyPoolBeginAdventuring");
        BeginAdventuringButton.Text = "#{pc_creation:408}\n#{pc_creation:409}";
        BeginAdventuringButton.PixelSize = new Size(151, 64);
        BeginAdventuringButton.LocalStyles.PaddingLeft = 14;
        BeginAdventuringButton.LocalStyles.PaddingTop = 10;
        BeginAdventuringButton.LocalStyles.PaddingRight = 14;
        BeginAdventuringButton.LocalStyles.PaddingBottom = 10;
        // TODO: Reposition on screen size change
        var x = _container.ComputePreferredBorderAreaSize().Width - BeginAdventuringButton.ComputePreferredBorderAreaSize().Width;
        BeginAdventuringButton.Pos = new PointF(_container.BorderArea.Height - BeginAdventuringButton.BorderArea.Height, x);
        BeginAdventuringButton.AddClickListener(BeginAdventuring);
        _container.Add(BeginAdventuringButton);

        var scrollBoxSettings = new ScrollBoxSettings
        {
            Font = PredefinedFont.ARIAL_10,
            Indent = 15,
            TextArea = new Rectangle(14, 28, 210, 313),
            ScrollBarPos = new Point(226, 8),
            ScrollBarHeight = 333
        };
        var helpContainer = doc.GetContainer("helpContainer");
        _helpScrollBox = new ScrollBox(RectangleF.Empty, scrollBoxSettings);
        _helpScrollBox.Anchors.FillParent();
        _helpScrollBox.SetHelpContent("TAG_CHARGEN_PARTY_POOL", includeTitle: true);
        _helpScrollBox.OnLinkClicked += GameSystems.Help.OpenLink;
        helpContainer.Add(_helpScrollBox);

        _scrollBar = doc.GetScrollBar("scrollBar");
        _scrollBar.SetValueChangeHandler(_ => Update());

        var slotsContainer = doc.GetContainer("slots");
        // Forward scrollwheel to the scrollbar
        slotsContainer.OnMouseWheel += e => { _scrollBar.DispatchMouseWheel(e); };

        _slots = new PartyPoolSlot[7];
        for (var i = 0; i < _slots.Length; i++)
        {
            var slot = new PartyPoolSlot();
            var padding = 0;
            if (i > 0)
            {
                padding = 2;
            }

            slot.Y = i * (slot.ComputePreferredBorderAreaSize().Height + padding);
            slot.AddClickListener(() => SelectAvailable(slot));
            slot.OnDoubleClick += e =>
            {
                if (slot.Player != null)
                {
                    TogglePlayerInGroup(slot.Player);
                }                
                e.PreventDefault();
                e.StopPropagation();
            };
            _slots[i] = slot;

            slotsContainer.Add(slot);
        }

        _portraits = new PartyPoolPortraits();
        var portraitContainer = _portraits.Container;
        // Position it in the lower left corner of the parent container
        portraitContainer.Y = _container.ComputePreferredBorderAreaSize().Height - portraitContainer.ComputePreferredBorderAreaSize().Height;
        _portraits.OnSelectedChanged += PartyMemberSelectionChanged;
        _container.Add(portraitContainer);

        Update();
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
        var selected = _availablePlayers.Find(p => p.Selected);

        if (selected != null && !_confirmingPlayerRemoval)
        {
            var critter = selected.GetOrCreateGameObject();

            UiSystems.CharSheet.State = CharInventoryState.PartyPool;
            UiSystems.CharSheet.Show(critter);
        }
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
                BeginAdventuringButton.Visible = false;
            }
            else
            {
                BeginAdventuringButton.Visible = true;
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
            RemovePlayerFromGroup(player);
        }

        ClearSelection();
    }

    private void RemovePlayerFromGroup(PartyPoolPlayer player)
    {
        if (player.InGroup)
        {
            player.InGroup = false;
            if (player.handle != null)
            {
                GameSystems.Party.RemoveFromAllGroups(player.handle);
            }
        }
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
        if (player?.handle != null)
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
            || player.InGroup
            || player.state != SlotState.CanJoin)
        {
            return false;
        }

        AddPlayerToGroup(player);
        return true;
    }

    private void AddPlayerToGroup(PartyPoolPlayer player)
    {
        if (player.InGroup)
        {
            return;
        }
        var playerObj = player.GetOrCreateGameObject();
        player.InGroup = true;
        GameSystems.Party.AddToPCGroup(playerObj);
        ClearSelection();
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

    private void SelectAvailable(PartyPoolSlot slot)
    {
        ClearSelection(false);

        var player = slot.Player;
        if (player != null)
        {
            player.Selected = true;
        }

        Update();
    }

    private void TogglePlayerInGroup(PartyPoolPlayer player)
    {
        if (player.InGroup)
        {
            RemovePlayerFromGroup(player);
        }
        else
        {
            AddPlayerToGroup(player);
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
        PartyPoolLoader();
        AddPcsFromBuffer();
        Update();

        Globals.UiManager.AddWindow(_container);
        _container.CenterInParent();

        UiSystems.Party.Hide();
        UiSystems.UtilityBar.Hide();
    }

    [TempleDllLocation(0x10165a50)]
    private void Cancel()
    {
        UiSystems.CharSheet.Hide(0);
        BeginAdventuringButton.Visible = false;
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

        Globals.UiManager.RemoveWindow(_container);

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
    public void ClearAll()
    {
        foreach (var availablePlayer in _availablePlayers)
        {
            if (availablePlayer.handle != null)
            {
                GameSystems.Object.Destroy(availablePlayer.handle);
            }
        }

        _availablePlayers.Clear();
        _filteredPlayers.Clear();
    }

    /// <summary>
    /// Clears only the players that have not been added to the party before.
    /// </summary>
    [TempleDllLocation(0x10163e30)]
    public void ClearAvailable()
    {
        foreach (var availablePlayer in _availablePlayers)
        {
            if (!availablePlayer.InGroup && availablePlayer.handle != null)
            {
                GameSystems.Object.Destroy(availablePlayer.handle);
            }
        }

        _availablePlayers.Clear();
        _filteredPlayers.Clear();
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
        Stub.TODO();
// TODO
    }

    [TempleDllLocation(0x10165150)]
    private void Update()
    {
        var partyAlignment = new ComplexInlineElement();
        partyAlignment.AppendTranslation("party_pool:1").AddStyle("partyAlignmentAccent");
        partyAlignment.AppendContent(" ");
        partyAlignment.AppendContent(GameSystems.Stat.GetAlignmentName(_alignment));
        _partyAlignmentLabel.Content = partyAlignment;

        UpdateCheckboxes();
        UpdateSlots();
        _portraits.Update();

        _addRemoveButton.SetStyle(AddButtonStyle);
        _addRemoveButton.Text = AddButtonLabel;
        _addRemoveButton.Disabled = true;
        _viewButton.Disabled = true;
        _renameButton.Disabled = true;
        _deleteButton.Disabled = true;

        var selectedPlayer = _availablePlayers.Find(p => p.Selected);
        if (selectedPlayer != null)
        {
            if (selectedPlayer.state == SlotState.CanJoin)
            {
                _addRemoveButton.Disabled = false;
            }

            _viewButton.Disabled = false;
            // TODO: where a char is premade or not should be decided based on the storage location, not its content
            if (!selectedPlayer.premade)
            {
                _renameButton.Disabled = false;
                _deleteButton.Disabled = false;
            }
        }
        else if (_portraits.Selected != null)
        {
            _addRemoveButton.SetStyle(RemoveButtonStyle);
            _addRemoveButton.Text = RemoveButtonLabel;
            _addRemoveButton.Disabled = false;
        }

        if (uiPartyCreationNotFromShopmap || GameSystems.Party.PartySize == 0)
        {
            BeginAdventuringButton.Visible = false;
        }
        else
        {
            BeginAdventuringButton.Visible = true;
        }
    }

    private void UpdateCheckboxes()
    {
        _hidePreGenButton.SetStyle(Globals.Config.PartyPoolHidePreGeneratedChars
            ? CheckboxCheckedStyle
            : CheckboxUncheckedStyle);
        _hideIncompatibleButton.SetStyle(Globals.Config.PartyPoolHideIncompatibleChars
            ? CheckboxCheckedStyle
            : CheckboxUncheckedStyle);
    }

    private void UpdateSlots()
    {
        foreach (var player in _availablePlayers)
        {
            UpdateState(player);
        }

        _filteredPlayers = _availablePlayers.Where(p =>
        {
            if (Globals.Config.PartyPoolHideIncompatibleChars && p.state != SlotState.CanJoin)
            {
                return false;
            }

            if (Globals.Config.PartyPoolHidePreGeneratedChars && p.premade)
            {
                return false;
            }

            return true;
        }).ToList();

        var hiddenSlots = Math.Max(0, _filteredPlayers.Count - _slots.Length);
        _scrollBar.Max = hiddenSlots;

        for (var i = 0; i < _slots.Length; i++)
        {
            var actualIdx = _scrollBar.GetValue() + i;
            _slots[i].Player = actualIdx < _filteredPlayers.Count ? _filteredPlayers[actualIdx] : null;
        }
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
                    pc.state = SlotState.PaladinOpposedAlignment;
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
                    pc.state = SlotState.PaladinOpposedAlignment;
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

    [TempleDllLocation(0x10166490)]
    public void Add(GameObject player)
    {
        throw new NotImplementedException();
    }
}

internal class PartyPoolPlayer
{
    public bool InGroup;
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
    public GameObject? handle;
    public SlotState state;
    public bool Selected;

    public static PartyPoolPlayer Read(BinaryReader reader)
    {
        var result = new PartyPoolPlayer();
        var flags = reader.ReadInt32();
        result.premade = (flags & 8) == 0;
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

    [TempleDllLocation(0x10025f10)]
    [TemplePlusLocation("generalfixes.cpp:399")]
    public GameObject GetOrCreateGameObject()
    {
        if (handle == null)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            handle = GameObject.Load(reader);
            handle.UnfreezeIds();
            handle.SetLocation(locXY.Zero);
            GameSystems.MapObject.InitDynamic(handle, locXY.Zero);
        }
        return handle;
    }
}
