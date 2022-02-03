using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7964)]
internal class RaceSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_RACE";

    public ChargenStages Stage => ChargenStages.CG_Stage_Race;

    public WidgetContainer Container { get; }

    private readonly SelectionList<RaceId?> _raceList = new SelectionList<RaceId?>();

    private readonly List<WidgetButton> _subraceButtons = new List<WidgetButton>();

    private readonly List<RaceId> _subraces = new List<RaceId>();

    private CharEditorSelectionPacket _pkt;

    [TempleDllLocation(0x1018ab30)]
    public RaceSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/race_ui.json");
        Container = doc.GetRootContainer();
        Container.Visible = false;

        for (var i = 0; i < 7; i++)
        {
            var buttonIndex = i; // Avoid modified closures
            var button = doc.GetButton("subrace" + i);
            button.Visible = false;
            button.SetClickHandler(() =>
            {
                if (buttonIndex < _subraces.Count)
                {
                    _pkt.raceId = _subraces[buttonIndex];
                    UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Race);
                    UpdateActiveRace();
                }
            });
            button.OnMouseEnter += msg =>
            {
                if (buttonIndex < _subraces.Count)
                {
                    ShowRaceHelp(_subraces[buttonIndex]);
                }
            };
            button.OnMouseExit += msg => { UpdateDescriptionBox(); };
            _subraceButtons.Add(button);
        }

        var enabledBaseRaces = D20RaceSystem.EnumerateEnabledBaseRaces().ToArray();

        Container.Add(_raceList.Container);

        _raceList.OnSelectedItemChanged += () =>
        {
            _pkt.raceId = _raceList.SelectedItem;
            UpdateDescriptionBox();
            UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Race);
            UpdateActiveRace();
        };
        _raceList.OnItemHovered += raceId =>
        {
            if (raceId.HasValue)
            {
                ShowRaceHelp(raceId.Value);
            }
            else
            {
                UpdateDescriptionBox();
            }
        };

        // Race Buttons
        foreach (var raceId in enabledBaseRaces)
        {
            var name = GameSystems.Stat.GetRaceName(raceId).ToUpper();
            _raceList.AddItem(name, raceId);
        }
    }

    private void UpdateActiveRace()
    {
        if (!_pkt.raceId.HasValue)
        {
            return;
        }

        var raceBase = D20RaceSystem.GetRaceSpec(_pkt.raceId.Value).BaseRace;
        _raceList.SelectedItem = (RaceId) raceBase;
        UpdateSubraceButtons(raceBase);
    }

    private void HideSubraceButtons()
    {
        foreach (var subraceButton in _subraceButtons)
        {
            subraceButton.Visible = false;
        }
    }

    private void UpdateSubraceButtons(RaceBase raceBase)
    {
        _subraces.Clear();
        _subraces.AddRange(D20RaceSystem.EnumerateSubRaces(raceBase));

        // Don't show the subrace selection if there's only one option anyway.
        if (_subraces.Count == 1)
        {
            HideSubraceButtons();
            return;
        }

        for (var i = 0; i < _subraceButtons.Count; i++)
        {
            var button = _subraceButtons[i];
            if (i < _subraces.Count)
            {
                var subRaceId = _subraces[i];
                button.Visible = true;
                button.SetActive(_pkt.raceId.HasValue && _pkt.raceId.Value == subRaceId);
                if (D20RaceSystem.IsBaseRace(subRaceId))
                {
                    button.Text = "COMMON";
                }
                else
                {
                    button.Text = GameSystems.Stat.GetRaceName(subRaceId).ToUpper();
                }
            }
            else
            {
                button.Visible = false;
            }
        }
    }

    [TempleDllLocation(0x1018a590)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:63")]
    public void Reset(CharEditorSelectionPacket pkt)
    {
        _pkt = pkt;
        pkt.raceId = null;
        _raceList.Reset();
        UpdateActiveRace();
        HideSubraceButtons();
    }

    [TempleDllLocation(0x1018a820)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x1018aaf0)]
    public void Resize(Size a1)
    {
    }

    [TempleDllLocation(0x1018a5a0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:64")]
    public bool CheckComplete()
    {
        return _pkt.raceId.HasValue;
    }

    private static void ShowRaceHelp(RaceId id)
    {
        var text = GameSystems.Stat.GetRaceShortDesc(id);
        UiSystems.PCCreation.ShowHelpText(text);
    }

    [TempleDllLocation(0x1018a7f0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:65")]
    private void UpdateDescriptionBox()
    {
        if (_pkt.raceId.HasValue)
        {
            ShowRaceHelp(_pkt.raceId.Value);
        }
        else
        {
            UiSystems.PCCreation.ShowHelpTopic("TAG_CHARGEN_RACE");
        }
    }

    public bool CompleteForTesting(Dictionary<string, object> props)
    {
        _pkt.raceId = RaceId.human;
        UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Race);
        UpdateActiveRace();
        return true;
    }
}