using System;
using System.Drawing;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Skills;

public class CharSheetSkillsUi : IDisposable
{
    // Only a subset of skills is actually available to the player
    private static readonly SkillId[] VisibleSkills =
    {
        SkillId.appraise,
        SkillId.bluff,
        SkillId.concentration,
        SkillId.diplomacy,
        SkillId.disable_device,
        SkillId.gather_information,
        SkillId.heal,
        SkillId.hide,
        SkillId.intimidate,
        SkillId.listen,
        SkillId.move_silently,
        SkillId.open_lock,
        SkillId.pick_pocket,
        SkillId.search,
        SkillId.sense_motive,
        SkillId.spellcraft,
        SkillId.spot,
        SkillId.tumble,
        SkillId.use_magic_device,
        SkillId.wilderness_lore,
        SkillId.perform
    };

    private WidgetScrollBar _scrollbar;

    public WidgetContainer Container { get; }

    private SkillButton[] _skillButtons = new SkillButton[20];

    private WidgetText _skillRanks;

    private WidgetText _attributeBonus;

    private WidgetText _attributeType;

    private WidgetText _miscBonus;

    private WidgetText _total;

    [TempleDllLocation(0x101be3f0)]
    public CharSheetSkillsUi()
    {
        var detailsDoc = WidgetDoc.Load("ui/char_skills.json");

        Container = detailsDoc.GetRootContainer();
        Container.SetMouseMsgHandler(msg =>
        {
            // Forward mouse wheel messages to the scrollbar
            if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
            {
                _scrollbar.HandleMouseMessage(msg);
            }

            return true;
        });
        Container.Name = "char_skills_ui_main_window";
        Container.Visible = false;

        _skillRanks = detailsDoc.GetTextContent("skill-ranks-label");
        _attributeBonus = detailsDoc.GetTextContent("skill-attribute-bonus-label");
        _attributeType = detailsDoc.GetTextContent("skill-attribute-type-label");
        _miscBonus = detailsDoc.GetTextContent("skill-misc-bonus-label");
        _total = detailsDoc.GetTextContent("skill-total-label");
        HideSkillDetails();

        for (var i = 0; i < 20; i++)
        {
            var button = new SkillButton(new Rectangle(1, 1 + 13 * i, 156, 13));
            button.OnMouseEnter += _ => ShowSkillDetails(button);
            button.OnMouseLeave += _ => HideSkillDetails();
            button.SetMouseMsgHandler(msg =>
            {
                if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
                {
                    return _scrollbar.HandleMouseMessage(msg);
                }

                return false;
            });
            Container.Add(button);
            _skillButtons[i] = button;
        }

        if (VisibleSkills.Length > _skillButtons.Length)
        {
            _scrollbar = new WidgetScrollBar(new Rectangle(160, -4, 13, 267));
            _scrollbar.SetMin(0);
            _scrollbar.Max = VisibleSkills.Length - _skillButtons.Length;
            Container.Add(_scrollbar);
            _scrollbar.SetValueChangeHandler((value) => UpdateSkillButtons());
        }

        UpdateSkillButtons();
    }

    private void ShowSkillDetails(SkillButton button)
    {
        _skillRanks.Visible = true;
        _attributeBonus.Visible = true;
        _attributeType.Visible = true;
        _miscBonus.Visible = true;
        _total.Visible = true;

        button.GetSkillBreakdown(out var skillRanks,
            out var attributeBonus,
            out var attributeType,
            out var miscBonus,
            out var totalBonus);

        _skillRanks.Text = skillRanks;
        _attributeBonus.Text = attributeBonus;
        _attributeType.Text = attributeType;
        _miscBonus.Text = miscBonus;
        _total.Text = totalBonus;

        // Display the skill's short description in the help text box
        UiSystems.CharSheet.Help.SetHelpText(GameSystems.Skill.GetShortDescription(button.Skill));
    }

    private void HideSkillDetails()
    {
        _skillRanks.Visible = false;
        _attributeBonus.Visible = false;
        _attributeType.Visible = false;
        _miscBonus.Visible = false;
        _total.Visible = false;
    }

    private void UpdateSkillButtons()
    {
        var offset = _scrollbar?.GetValue() ?? 0;
        for (var i = 0; i < _skillButtons.Length; i++)
        {
            var skillId = (SkillId) (i + offset);
            _skillButtons[i].SetSkill(skillId);
        }
    }

    [TempleDllLocation(0x101bd810)]
    public void Dispose()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x101bcc30)]
    public void Show()
    {
        Stub.TODO();
        Container.Visible = true;
    }

    [TempleDllLocation(0x101bcc60)]
    public void Hide()
    {
        Stub.TODO();
        Container.Visible = false;
    }

    public void Reset()
    {
    }
}