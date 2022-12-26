using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.PartyCreation.Systems.ClassFeatures;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

/// <summary>
/// Allows additional class-features to be selected for the class in which the character
/// is leveling up (or during character creation, the first class).
/// </summary>
[TempleDllLocation(0x102f7a98)]
internal class ClassFeaturesSystem : IChargenSystem
{
    private readonly Dictionary<Stat, IChargenSystem> _featuresByClass = new()
    {
        {Stat.level_cleric, new ClericFeaturesUi()},
        {Stat.level_wizard, new WizardFeaturesUi()},
        {Stat.level_ranger, new RangerFeaturesUi()}
    };

    private IChargenSystem? _activeFeaturesUi;

    private CharEditorSelectionPacket _pkt;

    [TempleDllLocation(0x10c3d0f0)]
    private bool _uiChargenAbilitiesActivated;

    [TempleDllLocation(0x10186f90)]
    public ClassFeaturesSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/abilities_ui.json");
        Container = doc.GetRootContainer();

        foreach (var featuresUi in _featuresByClass.Values)
        {
            var subContainer = featuresUi.Container;
            subContainer.Visible = false;
            Container.Add(subContainer);
        }
    }

    public string HelpTopic => "TAG_CHARGEN_ABILITIES";

    public ChargenStage Stage => ChargenStage.ClassFeatures;

    public WidgetContainer Container { get; }

    [TempleDllLocation(0x10184b90)]
    public void Reset(CharEditorSelectionPacket pkt)
    {
        _pkt = pkt;
        foreach (var featureUi in _featuresByClass.Values)
        {
            featureUi.Reset(pkt);
        }

        _uiChargenAbilitiesActivated = false;
    }

    [TempleDllLocation(0x10185e10)]
    public void Activate()
    {
        _uiChargenAbilitiesActivated = true;
    }

    [TempleDllLocation(0x10185670)]
    public void Show()
    {
        Container.Visible = true;

        _featuresByClass.TryGetValue(_pkt.classCode, out _activeFeaturesUi);
        _activeFeaturesUi?.Show();
    }

    public void Hide()
    {
        if (_activeFeaturesUi != null)
        {
            _activeFeaturesUi.Container.Visible = false;
            _activeFeaturesUi = null;
        }

        Container.Visible = false;
    }

    [TempleDllLocation(0x10184bd0)]
    public bool CheckComplete()
    {
        if (!_featuresByClass.TryGetValue(_pkt.classCode, out var featureUi))
        {
            // Always complete if the class has no special features to select
            return true;
        }

        return featureUi.CheckComplete();
    }

    [TempleDllLocation(0x10184c80)]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        _activeFeaturesUi?.Finalize(charSpec, ref playerObj);
    }

    [TempleDllLocation(0x10184c40)]
    public void UpdateDescriptionBox()
    {
        if (_pkt.classCode == Stat.level_wizard)
        {
            UiSystems.PCCreation.ShowHelpTopic("TAG_HMU_CHAR_EDITOR_WIZARD_SPEC");
        }
        else if (_pkt.classCode == Stat.level_cleric)
        {
            UiSystems.PCCreation.ShowHelpTopic("");
        }
    }
}