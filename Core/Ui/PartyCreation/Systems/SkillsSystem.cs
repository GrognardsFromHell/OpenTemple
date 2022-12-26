using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7af0)]
internal class SkillsSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_SKILLS";

    public ChargenStage Stage => ChargenStage.Skills;

    public WidgetContainer Container { get; }

    private CharEditorSelectionPacket _pkt;

    [TempleDllLocation(0x10c36248)]
    private bool _uiPcCreationSkillsActivated;

    [TempleDllLocation(0x10c37218)]
    private SkillId? _skillIdxMax;

    [TempleDllLocation(0x10c379b8)]
    [TempleDllLocation(0x10c37910)]
    private readonly List<SkillId> _chargenSkills = new();

    [TempleDllLocation(0x10181b70)]
    public SkillsSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/skills_ui.json");
        Container = doc.GetRootContainer();
    }

    [TempleDllLocation(0x10180630)]
    public void Reset(CharEditorSelectionPacket pkt)
    {
        _pkt = pkt;
        _pkt.skillPointsAdded.Clear();
        for (SkillId skillIdx = default; skillIdx < SkillId.count; skillIdx++)
        {
            _pkt.skillPointsAdded[skillIdx] = 0;
        }
        _uiPcCreationSkillsActivated = false;
    }

    [TempleDllLocation(0x10181380)]
    public void Activate()
    {
        _skillIdxMax = null;
        _chargenSkills.Clear();
        for (SkillId skillIdx = default; skillIdx < SkillId.count; skillIdx++)
        {
            if (GameSystems.Skill.IsEnabled(skillIdx))
            {
                _chargenSkills.Add(skillIdx);
            }

            ++skillIdx;
        }
        _chargenSkills.Sort(GameSystems.Skill.SkillNameComparer);

        // j_WidgetCopy /*0x101f87a0*/(uiPcCreationSkillsScrollbarId /*0x10c37a14*/,
        // (LgcyWidget*) &uiPcCreationSkillsScrollbar /*0x10c36780*/);
        // uiPcCreationSkillsScrollbar /*0x10c36780*/.yMax = chargenSkillsAvailableCount /*0x10c379b8*/ - 7;
        // j_ui_widget_set /*0x101f87b0*/(uiPcCreationSkillsScrollbarId /*0x10c37a14*/,
        // &uiPcCreationSkillsScrollbar /*0x10c36780*/);
        if (!_uiPcCreationSkillsActivated)
        {
            _pkt.skillPointsSpent = 0;
            var points = 4 * D20ClassSystem.GetSkillPoints(UiSystems.PCCreation.EditedChar, _pkt.classCode);
            _pkt.availableSkillPoints = points;
            if (_pkt.raceId == RaceId.human)
            {
                _pkt.availableSkillPoints += 4;
            }
            if (_pkt.availableSkillPoints < 4)
            {
                _pkt.availableSkillPoints = 4;
            }
        }

        // TODO UiPcCreationSkillTextDraw /*0x10180eb0*/();
        _uiPcCreationSkillsActivated = true;
    }

    [TempleDllLocation(0x10180690)]
    public bool CheckComplete()
    {
        return _uiPcCreationSkillsActivated && _pkt.availableSkillPoints == _pkt.skillPointsSpent;
    }

    [TempleDllLocation(0x101806b0)]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        foreach (var (skillId, amount) in charSpec.skillPointsAdded)
        {
            GameSystems.Skill.AddSkillRanks(playerObj, skillId, amount);
        }
    }

    private void ShowSkillHelp(SkillId skillId)
    {
        var helpTopic = GameSystems.Skill.GetHelpTopic(skillId);
        UiSystems.PCCreation.ShowHelpTopic(helpTopic);
    }

    [TempleDllLocation(0x101806f0)]
    private void UpdateDescriptionBox()
    {
        if (_skillIdxMax.HasValue)
        {
            ShowSkillHelp(_skillIdxMax.Value);
        }
        else
        {
            UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
        }
    }

}