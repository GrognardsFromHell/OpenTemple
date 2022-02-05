using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems.ClassFeatures;

/// <summary>
/// Supports selecting cleric features. This includes
/// selecting positive/negative energy once, if the deity is unaligned.
/// Otherwise it allows selection of domains.
/// </summary>
internal class ClericFeaturesUi : IChargenSystem
{
    private readonly List<WidgetButton> _availableDomainButtons = new();

    private readonly WidgetText _draggedDomainLabel;

    [TempleDllLocation(0x10c3c4e0)]
    private readonly List<DomainId> _selectableDomains = new();

    private readonly WidgetButton _selectedDomain1;

    private readonly WidgetButton _selectedDomain2;

    private readonly WidgetText _worshipsLabel;

    private readonly WidgetContainer _channelingTypeContainer;

    private readonly WidgetText _channelingTypeLabel;

    private readonly WidgetButton _channelingPositiveButton;

    private readonly WidgetButton _channelingNegativeButton;

    private CharEditorSelectionPacket _pkt;

    public ClericFeaturesUi()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/abilities_cleric_ui.json");
        Container = doc.GetRootContainer();
        Container.Visible = false;

        _worshipsLabel = doc.GetTextContent("worshipsLabel");

        for (var i = 0; i < 5; i++)
        {
            var index = i;
            var button = doc.GetButton($"deityDomain{i + 1}");
            button.SetMouseMsgHandler(msg => DeityDomainMouseHandler(msg, button, index));
            _availableDomainButtons.Add(button);
        }

        _selectedDomain1 = doc.GetButton("selectedDomain1");
        _selectedDomain2 = doc.GetButton("selectedDomain2");
        _draggedDomainLabel = new WidgetText("", "domainSelectionText");

        _channelingTypeContainer = doc.GetContainer("channelingTypeContainer");
        _channelingTypeLabel = doc.GetTextContent("channelingTypeLabel");
        _channelingPositiveButton = doc.GetButton("channelingPositiveButton");
        _channelingPositiveButton.SetClickHandler(() =>
        {
            _pkt.alignmentChoice = AlignmentChoice.Positive;
            OnAlignmentChoiceChanged();
        });
        _channelingNegativeButton = doc.GetButton("channelingNegativeButton");
        _channelingNegativeButton.SetClickHandler(() =>
        {
            _pkt.alignmentChoice = AlignmentChoice.Negative;
            OnAlignmentChoiceChanged();
        });
    }

    public WidgetContainer Container { get; }

    public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

    public string HelpTopic { get; }

    public void Reset(CharEditorSelectionPacket pkt)
    {
        _pkt = pkt;
        _pkt.domain1 = DomainId.None;
        _pkt.domain2 = DomainId.None;
    }

    public void Show()
    {
        Container.Visible = true;

        // Players need to choose whether they want to channel positive or negative energy for
        // certain deity/alignment combinations
        var playerObj = UiSystems.PCCreation.EditedChar;
        var deityId = _pkt.deityId.GetValueOrDefault();
        var alignment = playerObj.GetAlignment();
        var deityAlignmentChoice = GameSystems.Deity.GetAlignmentChoice(deityId, alignment);
        _channelingTypeContainer.Visible = deityAlignmentChoice == AlignmentChoice.Undecided;

        GetDeityDomains();

        UpdateDomainButtons();
        UpdateChannelingType();
    }

    public bool CheckComplete()
    {
        if (_pkt.domain1 != DomainId.None && _pkt.domain2 != DomainId.None)
        {
            return _pkt.alignmentChoice != AlignmentChoice.Undecided;
        }

        return false;
    }

    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        playerObj.SetInt32(obj_f.critter_domain_1, (int) charSpec.domain1);
        playerObj.SetInt32(obj_f.critter_domain_2, (int) charSpec.domain2);
        playerObj.SetInt32(obj_f.critter_alignment_choice, (int) charSpec.alignmentChoice);
    }

    private bool DeityDomainMouseHandler(MessageMouseArgs msg, WidgetButton widget, int index)
    {
        var domain = _selectableDomains[index];

        if (Globals.UiManager.GetMouseCaptureWidget() == widget)
        {
            if ((msg.flags & MouseEventFlag.LeftReleased) != 0)
            {
                Tig.Mouse.SetCursorDrawCallback(null);
                Globals.UiManager.UnsetMouseCaptureWidget(widget);
                widget.Visible = true;

                var widgetUnderCursor = Globals.UiManager.GetWidgetAt(msg.X, msg.Y);
                if (widgetUnderCursor == _selectedDomain1)
                {
                    RemoveSelectedDomain(domain);
                    _pkt.domain1 = domain;
                    OnDomainsChanged();
                }
                else if (widgetUnderCursor == _selectedDomain2)
                {
                    RemoveSelectedDomain(domain);
                    _pkt.domain2 = domain;
                    OnDomainsChanged();
                }
            }

            return true;
        }

        if (IsDomainSelected(domain))
        {
            // Do not allow interaction with unassigned ability scores
            return true;
        }

        // Allow quickly swapping values between the two columns, but only when we actually have rolled values
        // (not in point buy mode)
        if ((msg.flags & MouseEventFlag.RightClick) != 0)
        {
            if (_pkt.domain1 == DomainId.None)
            {
                _pkt.domain1 = domain;
                OnDomainsChanged();
                return true;
            }

            if (_pkt.domain2 == DomainId.None)
            {
                _pkt.domain2 = domain;
                OnDomainsChanged();
                return true;
            }
        }
        else if ((msg.flags & MouseEventFlag.LeftClick) != 0)
        {
            if (!Globals.UiManager.SetMouseCaptureWidget(widget))
            {
                // Something else has the mouse capture right now (how are we getting this message then...?)
                return true;
            }

            // Figure out where in the widget we got clicked so we can draw the dragged text with the proper offset
            var globalContentArea = widget.GetContentArea(true);
            var localX = msg.X - globalContentArea.X;
            var localY = msg.Y - globalContentArea.Y;
            _draggedDomainLabel.Text = widget.Text;
            widget.Visible = false;

            // This will draw the ability score being dragged under the mouse cursor
            Tig.Mouse.SetCursorDrawCallback((x, y, arg) =>
            {
                var point = new Point(x, y);
                point.Offset(-localX, -localY);
                var contentArea = new Rectangle(point, widget.GetSize());

                _draggedDomainLabel.SetBounds(contentArea);
                _draggedDomainLabel.Render();
            });
        }

        return true;
    }

    private bool IsDomainSelected(DomainId domain)
    {
        return _pkt.domain1 == domain || _pkt.domain2 == domain;
    }

    private void RemoveSelectedDomain(DomainId domain)
    {
        if (_pkt.domain1 == domain)
        {
            _pkt.domain1 = DomainId.None;
        }

        if (_pkt.domain2 == domain)
        {
            _pkt.domain2 = DomainId.None;
        }
    }

    private void OnDomainsChanged()
    {
        UpdateDomainButtons();
    }

    private void OnAlignmentChoiceChanged()
    {
        UpdateChannelingType();
    }

    private void UpdateDomainButtons()
    {
        if (_pkt.domain1 != DomainId.None)
        {
            _selectedDomain1.Text = GameSystems.Spell.GetDomainName(_pkt.domain1);
        }
        else
        {
            _selectedDomain1.Text = "";
        }

        if (_pkt.domain2 != DomainId.None)
        {
            _selectedDomain2.Text = GameSystems.Spell.GetDomainName(_pkt.domain2);
        }
        else
        {
            _selectedDomain2.Text = "";
        }

        for (var i = 0; i < _availableDomainButtons.Count; i++)
        {
            var button = _availableDomainButtons[i];
            if (i >= _selectableDomains.Count)
            {
                button.Visible = false;
                continue;
            }

            button.Visible = true;
            var domain = _selectableDomains[i];
            button.Text = GameSystems.Spell.GetDomainName(domain);
            if (IsDomainSelected(domain))
            {
                button.SetStyle("deityDomainButtonDisabled");
            }
            else
            {
                button.SetStyle("deityDomainButton");
            }
        }
    }

    private void UpdateChannelingType()
    {
        var msgId = 18100 + (int) _pkt.deityId.GetValueOrDefault();
        _channelingTypeLabel.Text = $"#{{pc_creation:{msgId}}}";

        _channelingPositiveButton.SetActive(_pkt.alignmentChoice == AlignmentChoice.Positive);
        _channelingNegativeButton.SetActive(_pkt.alignmentChoice == AlignmentChoice.Negative);
    }

    [TempleDllLocation(0x10184da0)]
    private void GetDeityDomains()
    {
        var deityId = _pkt.deityId.GetValueOrDefault();
        var deityName = GameSystems.Deity.GetName(deityId);
        _worshipsLabel.Text = "@1#{pc_creation:18000}@0 " + deityName;

        _selectableDomains.Clear();
        _selectableDomains.AddRange(GameSystems.Deity.GetDomains(deityId));
    }
}