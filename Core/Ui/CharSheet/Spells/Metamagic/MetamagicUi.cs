using System;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells.Metamagic;

/// <summary>
/// User interface to create a new spell from an existing spell with metamagic feats applied.
/// </summary>
public class MetamagicUi : IDisposable
{
    private GameObject _caster;

    private SpellStoreData _spell;

    private readonly WidgetContainer _container;

    private readonly WidgetText _spellLabel;
    private readonly WidgetText _newSpellLevelLabel;
    private readonly ScrollBox _helpText;

    private readonly WidgetScrollView _availableContainer;
    private readonly WidgetScrollView _appliedContainer;

    [TempleDllLocation(0x101b9ed0)]
    public MetamagicUi()
    {
        var doc = WidgetDoc.Load("ui/char_spells_metamagic.json");
        _container = doc.GetRootContainer();
        _container.Visible = false;

        _newSpellLevelLabel = doc.GetTextContent("new_spell_level");
        _spellLabel = doc.GetTextContent("spell_name");
        _helpText = doc.GetScrollBox("help_box");

        doc.GetButton("accept").SetClickHandler(Accept);
        doc.GetButton("cancel").SetClickHandler(Hide);

        _availableContainer = doc.GetScrollView("available_container");
        _appliedContainer = doc.GetScrollView("applied_container");
    }

    private void Accept()
    {
        Hide();
    }

    public void Dispose()
    {
        _container.Dispose();
    }

    public void Show(GameObject caster, SpellStoreData spell)
    {
        _caster = caster;
        _spell = spell;

        _availableContainer.Clear();
        _appliedContainer.Clear();

        var y = 0;
        foreach (var featId in GameSystems.Spell.GetAvailableMetamagicFeats(caster, spell))
        {
            var btn = new MetamagicFeatButton(false, featId,
                new Rectangle(0, y, _availableContainer.GetInnerWidth(), 14));
            btn.AddStyle("metamagic-feat-label");
            _availableContainer.Add(btn);
            y += btn.Height + 1;
            btn.OnMouseEnter += _ =>
            {
                ShowFeatHelp(featId);
                btn.AddStyle("metamagic-feat-label:hover");
            };
            btn.OnMouseExit += _ =>
            {
                HideFeatHelp();
                btn.RemoveStyle("metamagic-feat-label:hover");
            };

            var draggable = DragDrop.MakeDraggable(btn);
            draggable.WithDraggedContent(
                new WidgetText(GameSystems.Feat.GetFeatName(featId), "metamagic-feat-label")
            );
            draggable.WhenDroppedOn(_appliedContainer, () => { Console.WriteLine("YAY"); });
        }

        var appliedFeats = GameSystems.Spell.GetAppliedMetamagicFeats(spell).ToList();
        foreach (var featId in appliedFeats)
        {
        }

        UpdateLabels();

        _container.CenterOnScreen();
        _container.BringToFront();
        _container.Show();
    }

    public void Hide()
    {
        _caster = null;
        _spell = default;
        _container.Hide();
    }

    private void UpdateLabels()
    {
        var levelText = new ComplexInlineElement();
        levelText.AppendContent(Globals.UiAssets.ApplyTranslation("#{char_ui_spells:10} "),
            "metamagic-dialog-box-label");
        levelText.AppendContent(_spell.spellLevel.ToString());
        _newSpellLevelLabel.Content = levelText;

        var nameText = new ComplexInlineElement();
        nameText.AppendContent(Globals.UiAssets.ApplyTranslation("#{char_ui_spells:9} "), "metamagic-dialog-box-label");
        nameText.AppendContent(GameSystems.Spell.GetSpellName(_spell.spellEnum));
        _spellLabel.Content = nameText;
    }

    private void ShowFeatHelp(FeatId featId)
    {
        if (GameSystems.Feat.TryGetFeatHelpTopic(featId, out var topicId))
        {
            _helpText.SetHelpContent(topicId);
        }
    }

    private void HideFeatHelp()
    {
        _helpText.ClearContent();
    }
}